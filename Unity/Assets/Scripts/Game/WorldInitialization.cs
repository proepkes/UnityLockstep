using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using Zenject;

public static class WorldInitialization
{
    static void DomainUnloadShutdown()
    {
        World.DisposeAllWorlds();

        WordStorage.Instance.Dispose();
        WordStorage.Instance = null;
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);
    }

    static ScriptBehaviourManager GetOrCreateManagerAndLogException(World world, Type type)
    {
        try
        {  
            return world.GetOrCreateManager(type);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public static void Initialize(DiContainer container, string worldName, bool editorWorld)
    {
        void RegisterHook(Type hook)
        {
            InjectionHookSupport.RegisterHook((InjectionHook)Activator.CreateInstance(hook));
        }

#pragma warning disable 0618                                                 
        // Register hybrid injection hooks
        var injectionAssembly = typeof(GameObjectArray).Assembly;
        RegisterHook(injectionAssembly.GetType("Unity.Entities.GameObjectArrayInjectionHook"));
        RegisterHook(injectionAssembly.GetType("Unity.Entities.TransformAccessArrayInjectionHook"));
        RegisterHook(injectionAssembly.GetType("Unity.Entities.ComponentArrayInjectionHook"));
#pragma warning restore 0618

        PlayerLoopManager.RegisterDomainUnload(DomainUnloadShutdown, 10000);

        var world = new World(worldName);
        World.Active = world;
        var systems = GetAllSystems(container, world, WorldSystemFilterFlags.Default);
        if (systems == null)
        {
            world.Dispose();
            if (World.Active == world)
            {
                World.Active = null;
            }
            return;
        }

        // create presentation system and simulation system
        InitializationSystemGroup initializationSystemGroup = world.GetOrCreateManager<InitializationSystemGroup>();
        SimulationSystemGroup simulationSystemGroup = world.GetOrCreateManager<SimulationSystemGroup>();
        PresentationSystemGroup presentationSystemGroup = world.GetOrCreateManager<PresentationSystemGroup>();
        // Add systems to their groups, based on the [UpdateInGroup] attribute.
        foreach (var type in systems)
        {
            // Skip the built-in root-level systems
            if (type == typeof(InitializationSystemGroup) ||
                type == typeof(SimulationSystemGroup) ||
                type == typeof(PresentationSystemGroup))
            {
                continue;
            }
            if (editorWorld)
            {
                if (!Attribute.IsDefined(type, typeof(ExecuteAlways)))
                    continue;
            }

            var groups = type.GetCustomAttributes(typeof(UpdateInGroupAttribute), true);
            if (groups.Length == 0)
            {
                simulationSystemGroup.AddSystemToUpdateList(GetOrCreateManagerAndLogException(world, type) as ComponentSystemBase);
            }

            foreach (var g in groups)
            {
                if (!(g is UpdateInGroupAttribute group))
                    continue;

                if (!(typeof(ComponentSystemGroup)).IsAssignableFrom(group.GroupType))
                {
                    Debug.LogError($"Invalid [UpdateInGroup] attribute for {type}: {group.GroupType} must be derived from ComponentSystemGroup.");
                    continue;
                }

                var groupMgr = GetOrCreateManagerAndLogException(world, group.GroupType);
                switch (groupMgr)
                {
                    case null:
                        Debug.LogWarning(
                            $"Skipping creation of {type} due to errors creating the group {@group.GroupType}. Fix these errors before continuing.");
                        continue;

                    case ComponentSystemGroup groupSys:
                        groupSys.AddSystemToUpdateList(GetOrCreateManagerAndLogException(world, type) as ComponentSystemBase);
                        break;
                }
            }
        }

        // Update player loop
        initializationSystemGroup.SortSystemUpdateList();
        simulationSystemGroup.SortSystemUpdateList();
        presentationSystemGroup.SortSystemUpdateList();
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
    }

    public static void DefaultLazyEditModeInitialize()
    {
#if UNITY_EDITOR
        if (World.Active == null)
        {
            // * OnDisable (Serialize monobehaviours in temporary backup)
            // * unload domain
            // * load new domain
            // * OnEnable (Deserialize monobehaviours in temporary backup)
            // * mark entered playmode / load scene
            // * OnDisable / OnDestroy
            // * OnEnable (Loading object from scene...)
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // We are just gonna ignore this enter playmode reload.
                // Can't see a situation where it would be useful to create something inbetween.
                // But we really need to solve this at the root. The execution order is kind if crazy.
                if (UnityEditor.EditorApplication.isPlaying)
                    Debug.LogError("Loading GameObjectEntity in Playmode but there is no active World");
            }
            else
            {
#if !UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
                var container = new DiContainer(new[] { StaticContext.Container });
                ZenjectManagersInstaller.Install(container);

                Initialize(container, "Editor World", true);

                container.Resolve<InitializableManager>().Initialize();
#endif
            }
        }
#endif
    }

    public static List<Type> GetAllSystems(DiContainer container, World world, WorldSystemFilterFlags filterFlags)
    {
        IEnumerable<Type> allTypes;
        var systemTypes = new List<Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!TypeManager.IsAssemblyReferencingEntities(assembly))
                continue;

            try
            {
                allTypes = assembly.GetTypes();

            }
            catch (ReflectionTypeLoadException e)
            {
                allTypes = e.Types.Where(t => t != null);
                Debug.LogWarning(
                    $"DefaultWorldInitialization failed loading assembly: {(assembly.IsDynamic ? assembly.ToString() : assembly.Location)}");
            }             

            bool FilterSystemType(Type type)
            {
                if (!type.IsSubclassOf(typeof(ComponentSystemBase)) || type.IsAbstract || type.ContainsGenericParameters)
                    return false;

                if (type.GetCustomAttribute<DisableAutoCreationAttribute>(true) != null)
                    return false;

                var systemFlags = WorldSystemFilterFlags.Default;
                var attrib = type.GetCustomAttribute<WorldSystemFilterAttribute>(true);
                if (attrib != null)
                    systemFlags = attrib.FilterFlags;

                return (filterFlags & systemFlags) != 0;
            }

            systemTypes.AddRange(allTypes.Where(FilterSystemType));
                                                                 
            SimulationSystemGroup simulationSystemGroup = world.GetOrCreateManager<SimulationSystemGroup>();
            foreach (var manualType in allTypes.Where(type =>
            {
                if (!type.IsSubclassOf(typeof(ComponentSystemBase)) || type.IsAbstract || type.ContainsGenericParameters)
                    return false;

                return type.GetCustomAttribute<DisableAutoCreationAttribute>(true) != null && type.GetCustomAttribute<DependencySystemAttribute>(true) != null;
            }))
            {
                container.Bind(manualType)
                    .AsSingle()
                    .OnInstantiated<ScriptBehaviourManager>((i, manager) =>
                    {
                        world.AddManager(manager);
                        simulationSystemGroup.AddSystemToUpdateList(manager as ComponentSystemBase);   
                    })
                    .NonLazy();
            }
        }

        return systemTypes;
    }
}