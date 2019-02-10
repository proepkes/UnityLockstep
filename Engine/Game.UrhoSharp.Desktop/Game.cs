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

using System.Diagnostics;
using Urho;
using Urho.Resources;

namespace Game.UrhoSharp.Desktop
{
	public class Game : Application
	{
		UrhoConsole console;
		DebugHud debugHud;
		ResourceCache cache;      

		protected float Yaw { get; set; }
		protected float Pitch { get; set; }
		protected Node CameraNode { get; set; }
		protected MonoDebugHud MonoDebugHud { get; set; }

		[Preserve]
		protected Game(ApplicationOptions options = null) : base(options) {}

		static Game()
		{
			UnhandledException += Application_UnhandledException1;
		}

		static void Application_UnhandledException1(object sender, UnhandledExceptionEventArgs e)
		{
			if (Debugger.IsAttached && !e.Exception.Message.Contains("BlueHighway.ttf"))
				Debugger.Break();
			e.Handled = true;
		}
        
		protected override void Start ()
		{
			Log.LogMessage += e => Debug.WriteLine($"[{e.Level}] {e.Message}");
			base.Start();

			Input.Enabled = true;
			MonoDebugHud = new MonoDebugHud(this);
			MonoDebugHud.Show();

            cache = ResourceCache;
            CreateConsoleAndDebugHud ();
			Input.KeyDown += HandleKeyDown;
		}
        
		/// <summary>
		/// Move camera for 2D samples
		/// </summary>
		protected void SimpleMoveCamera2D (float timeStep)
		{
			// Do not move if the UI has a focused element (the console)
			if (UI.FocusElement != null)
				return;

			// Movement speed as world units per second
			const float moveSpeed = 4.0f;

			// Read WASD keys and move the camera scene node to the corresponding direction if they are pressed
			if (Input.GetKeyDown(Key.W)) CameraNode.Translate( Vector3.UnitY * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.S)) CameraNode.Translate(-Vector3.UnitY * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.A)) CameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
			if (Input.GetKeyDown(Key.D)) CameraNode.Translate( Vector3.UnitX * moveSpeed * timeStep);

			if (Input.GetKeyDown(Key.PageUp))
			{
				var camera = CameraNode.GetComponent<Camera>();
				camera.Zoom *= 1.01f;
			}

			if (Input.GetKeyDown(Key.PageDown))
			{
				var camera = CameraNode.GetComponent<Camera>();
				camera.Zoom *= 0.99f;
			}
		}

		/// <summary>
		/// Move camera for 3D samples
		/// </summary>
		protected void SimpleMoveCamera3D (float timeStep, float moveSpeed = 10.0f)
		{
			const float mouseSensitivity = .1f;

			if (UI.FocusElement != null)
				return;

            var mouseMove = Input.MouseMove;
			Yaw += mouseSensitivity * mouseMove.X;
			Pitch += mouseSensitivity * mouseMove.Y;
			Pitch = MathHelper.Clamp(Pitch, -90, 90);

			CameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);

			if (Input.GetKeyDown (Key.W)) CameraNode.Translate ( Vector3.UnitZ * moveSpeed * timeStep);
			if (Input.GetKeyDown (Key.S)) CameraNode.Translate (-Vector3.UnitZ * moveSpeed * timeStep);
			if (Input.GetKeyDown (Key.A)) CameraNode.Translate (-Vector3.UnitX * moveSpeed * timeStep);
			if (Input.GetKeyDown (Key.D)) CameraNode.Translate ( Vector3.UnitX * moveSpeed * timeStep);
        }


		void CreateConsoleAndDebugHud()
		{
			var xml = cache.GetXmlFile("UI/DefaultStyle.xml");
			console = Engine.CreateConsole();
			console.DefaultStyle = xml;
			console.Background.Opacity = 0.8f;

			debugHud = Engine.CreateDebugHud();
			debugHud.DefaultStyle = xml;
		}

		async void HandleKeyDown(KeyDownEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Esc:
					await Exit();
					return;
				case Key.F1:
					console.Toggle();
					return;
				case Key.F2:
					debugHud.ToggleAll();
					return;
			}

			var renderer = Renderer;
			switch (e.Key)
			{
				case Key.N1:
					var quality = renderer.TextureQuality;
					++quality;
					if (quality > 2)
						quality = 0;
					renderer.TextureQuality = quality;
					break;

				case Key.N2:
					var mquality = renderer.MaterialQuality;
					++mquality;
					if (mquality > 2)
						mquality = 0;
					renderer.MaterialQuality = mquality;
					break;

				case Key.N3:
					renderer.SpecularLighting = !renderer.SpecularLighting;
					break;

				case Key.N4:
					renderer.DrawShadows = !renderer.DrawShadows;
					break;

				case Key.N5:
					var shadowMapSize = renderer.ShadowMapSize;
					shadowMapSize *= 2;
					if (shadowMapSize > 2048)
						shadowMapSize = 512;
					renderer.ShadowMapSize = shadowMapSize;
					break;

				// shadow depth and filtering quality
				case Key.N6:
					var q = (int)renderer.ShadowQuality++;
					if (q > 3)
						q = 0;
					renderer.ShadowQuality = (ShadowQuality)q;
					break;

				// occlusion culling
				case Key.N7:
					var o = !(renderer.MaxOccluderTriangles > 0);
					renderer.MaxOccluderTriangles = o ? 5000 : 0;
					break;

				// instancing
				case Key.N8:
					renderer.DynamicInstancing = !renderer.DynamicInstancing;
					break;
			}
		}
	}
}
