using System;
using Game.UrhoSharp.Desktop.Scenes;
using Urho;
using Urho.Desktop;

namespace Game.UrhoSharp.Desktop
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DesktopUrhoInitializer.AssetsDirectory = @"../../Assets";
            new StaticScene(new ApplicationOptions("Data")).Run();
        }
    }
}
