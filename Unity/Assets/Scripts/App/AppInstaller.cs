using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Zenject;

public class Bootstrap : ICustomBootstrap
{
    public List<Type> Initialize(List<Type> types)
    {
        // When this is called, World.Active is the world being initialized
        // The list of types returned will be the systems that are updated

        if (World.Active.Name == "Default World")
        {
            return new List<Type>();
        }

        return types;
    }
}

[CreateAssetMenu(fileName = "AppInstaller", menuName = "Installers/AppInstaller")]
public class AppInstaller : ScriptableObjectInstaller<AppInstaller>
{
    public GameObject networkPrefab;

    public override void InstallBindings()
    {
        Container.Bind<INetwork>().FromComponentInNewPrefab(networkPrefab).AsSingle().NonLazy();

        WorldInitialization.Initialize(Container, "Client World", false);
    }     
}