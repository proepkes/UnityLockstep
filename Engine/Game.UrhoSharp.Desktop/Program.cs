using System;
using Urho;
using Urho.Desktop;
using Urho.Samples;

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
