//
// Copyright (c) 2008-2015 the Urho3D project.
// Copyright (c) 2015 Xamarin Inc
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Linq;
using Entitas;
using FixMath.NET;
using Game.UrhoSharp.Desktop.Coupling;
using Lockstep.Game;
using Lockstep.Game.Commands;              
using Lockstep.Network.Client;
using Urho;
using Urho.Gui;
using Urho.Navigation;
using Urho.Resources;

namespace Game.UrhoSharp.Desktop.Scenes
{
	public class StaticScene : Game
	{
		Camera camera;
		Scene scene;
        CrowdManager crowdManager;
        private Random rand;

        private Text worldInfoText;
        private Simulation simulation;
        private LiteNetLibNetwork network;
        private NetworkCommandQueue networkCommandQueue;

        public StaticScene(ApplicationOptions options = null) : base(options) { }

		protected override void Start ()
		{
			base.Start ();
			CreateScene ();
            CreateUI();
            CreateWorld();

            SetupViewport ();
		}

        private void CreateWorld()
        {
            network = new LiteNetLibNetwork();
            networkCommandQueue = new NetworkCommandQueue(network);
            networkCommandQueue.InitReceived +=
                (sender, init) =>
                {
                    rand = new Random(init.Seed);
                    for (int i = 0; i < 100; i++)
                    {
                        var mushroom = scene.CreateChild("Mushroom");
                        mushroom.Position = new Vector3(rand.Next(90) - 45, 0, rand.Next(90) - 45);
                        mushroom.Rotation = new Quaternion(0, rand.Next(360), 0);
                        mushroom.SetScale(0.5f + rand.Next(20000) / 10000.0f);
                        var mushroomObject = mushroom.CreateComponent<StaticModel>();
                        mushroomObject.Model = ResourceCache.GetModel("Models/Mushroom.mdl");
                        mushroomObject.SetMaterial(ResourceCache.GetMaterial("Materials/Mushroom.xml"));
                    }

                    simulation.Start(init.SimulationSpeed, init.ActorID, init.AllActors);
                };

            simulation = new Simulation(Contexts.sharedInstance, networkCommandQueue, new ViewService(ResourceCache, scene.GetChild("Jacks")));

            network.Start();
            network.Connect("127.0.0.1", 9050);
        }

        void CreateUI()
        {
            var cache = ResourceCache;

            // Create a Cursor UI element because we want to be able to hide and show it at will. When hidden, the mouse cursor will
            // control the camera, and when visible, it will point the raycast target
            XmlFile style = cache.GetXmlFile("UI/DefaultStyle.xml");
            Cursor cursor = new Cursor();
            cursor.SetStyleAuto(style);
            UI.Cursor = cursor;
        }

		void CreateScene ()
		{
			var cache = ResourceCache;
			scene = new Scene ();

			// Create the Octree component to the scene. This is required before adding any drawable components, or else nothing will
			// show up. The default octree volume will be from (-1000, -1000, -1000) to (1000, 1000, 1000) in world coordinates; it
			// is also legal to place objects outside the volume but their visibility can then not be checked in a hierarchically
			// optimizing manner
			scene.CreateComponent<Octree> ();

			// Create a child scene node (at world origin) and a StaticModel component into it. Set the StaticModel to show a simple
			// plane mesh with a "stone" material. Note that naming the scene nodes is optional. Scale the scene node larger
			// (100 x 100 world units)
			var planeNode = scene.CreateChild("Plane");
			planeNode.Scale = new Vector3 (100, 1, 100);
			var planeObject = planeNode.CreateComponent<StaticModel> ();
			planeObject.Model = cache.GetModel ("Models/Plane.mdl");
			planeObject.SetMaterial (cache.GetMaterial ("Materials/StoneTiled.xml"));

			// Create a directional light to the world so that we can see something. The light scene node's orientation controls the
			// light direction; we will use the SetDirection() function which calculates the orientation from a forward direction vector.
			// The light will use default settings (white light, no shadows)
			var lightNode = scene.CreateChild("DirectionalLight");
			lightNode.SetDirection (new Vector3(0.6f, -1.0f, 0.8f)); // The direction vector does not need to be normalized
			var light = lightNode.CreateComponent<Light>();
			light.LightType = LightType.Directional;


            // Create a DynamicNavigationMesh component to the scene root
            var navMesh = scene.CreateComponent<DynamicNavigationMesh>();
            // Set the agent height large enough to exclude the layers under boxes
            navMesh.AgentHeight = 10.0f;
            navMesh.CellHeight = 0.05f;
            navMesh.DrawObstacles = true;
            navMesh.DrawOffMeshConnections = true;
            // Create a Navigable component to the scene root. This tags all of the geometry in the scene as being part of the
            // navigation mesh. By default this is recursive, but the recursion could be turned off from Navigable
            scene.CreateComponent<Navigable>();
            // Add padding to the navigation mesh in Y-direction so that we can add objects on top of the tallest boxes
            // in the scene and still update the mesh correctly
            navMesh.Padding = new Vector3(0.0f, 10.0f, 0.0f);
            // Now build the navigation geometry. This will take some time. Note that the navigation mesh will prefer to use
            // physics geometry from the scene nodes, as it often is simpler, but if it can not find any (like in this example)
            // it will use renderable geometry instead
            navMesh.Build();

            // Create a CrowdManager component to the scene root
            crowdManager = scene.CreateComponent<CrowdManager>();
            var parameters = crowdManager.GetObstacleAvoidanceParams(0);
            // Set the params to "High (66)" setting
            parameters.VelBias = 0.5f;
            parameters.AdaptiveDivs = 7;
            parameters.AdaptiveRings = 3;
            parameters.AdaptiveDepth = 3;
            crowdManager.SetObstacleAvoidanceParams(0, parameters);

            CameraNode = scene.CreateChild ("camera");
			camera = CameraNode.CreateComponent<Camera>();
			CameraNode.Position = new Vector3 (0, 5, 0);

            scene.CreateChild("Jacks");


            worldInfoText = new Text
            {
                Value = "Waiting for all players to join...",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            worldInfoText.SetFont(ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), 15);
            UI.Root.AddChild(worldInfoText);
        }

		
		void SetupViewport ()
		{
			var renderer = Renderer;
			renderer.SetViewport (0, new Viewport (Context, scene, camera));
        }

        protected override void OnUpdate(float timeStep)
		{
			base.OnUpdate(timeStep);

            network.Update();

            SimpleMoveCamera3D(timeStep);

            const int qualShift = 1;

            if (Input.GetKeyPress(Key.P))
            {
                simulation.DumpGameLog(new FileStream(@"C:\Log\" + Math.Abs(Contexts.sharedInstance.gameState.hashCode.value) + ".bin", FileMode.Create, FileAccess.Write));
            }

            // Set destination or spawn a new jack with left mouse button
            if (Input.GetMouseButtonPress(MouseButton.Left))
            {
                SetPathPoint(Input.GetQualifierDown(qualShift));
            }

            simulation.Update(timeStep * 1000);

            if (simulation.Running)
            {
                worldInfoText.Value = "ActorId: " + simulation.LocalActorId + Environment.NewLine +
                                      "Tick: " + Contexts.sharedInstance.gameState.tick.value + Environment.NewLine + 
                                      "HashCode: " + Contexts.sharedInstance.gameState.hashCode.value;
            }
        }

        void SetPathPoint(bool spawning)
        {
            if (Raycast(250.0f, out var hitPos, out _))
            {
                var navMesh = scene.GetComponent<DynamicNavigationMesh>();
                var pathPos = navMesh.FindNearestPoint(hitPos, new Vector3(1.0f, 1.0f, 1.0f));
                var compatiblePos = new BEPUutilities.Vector2((Fix64)pathPos.X, (Fix64)pathPos.Z);
                if (spawning)
                    // Spawn a jack at the target position
                    simulation.Execute( new SpawnCommand { Position = compatiblePos });
                else
                    // Set crowd agents target position
                    simulation.Execute(new NavigateCommand
                    {
                        Destination = compatiblePos,
                        Selection = Contexts.sharedInstance.game.GetEntities(GameMatcher.LocalId).Where(entity => entity.actorId.value == simulation.LocalActorId).Select(entity => entity.id.value).ToArray()
                    });
            }
        }

        bool Raycast(float maxDistance, out Vector3 hitPos, out Drawable hitDrawable)
        {
            hitDrawable = null;
            hitPos = Vector3.Zero;

            UI ui = UI;
            IntVector2 pos = ui.CursorPosition;
            // Check the cursor is visible and there is no UI element in front of the cursor
            if (!ui.Cursor.Visible || ui.GetElementAt(pos) != null)
                return false;

            var graphics = Graphics;
            Ray cameraRay = camera.GetScreenRay((float)pos.X / graphics.Width, (float)pos.Y / graphics.Height);
            // Pick only geometry objects, not eg. zones or lights, only get the first (closest) hit

            var result = scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, maxDistance);
            if (result != null)
            {
                hitPos = result.Value.Position;
                hitDrawable = result.Value.Drawable;
                return true;
            }

            return false;
        }
    }
}