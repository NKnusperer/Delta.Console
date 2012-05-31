using Delta.Engine;
using Delta.Scenes.UserInterfaces;
using Delta.Rendering.Basics.Materials;
using Delta.Scenes;
using Delta.Utilities.Datatypes;

namespace TestGame
{
    /// <summary>
    /// Program class, just provides a Main entry point.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The entry point for the application, will just start the Game class!
        /// </summary>
        static void Main()
        {
            // If we don't have Settings yet (it is a bug), force using physics!
            if (Settings.Modules.PhysicsModule == "")
            {
                Settings.Modules.PhysicsModule = "Jitter";
            }

            // Starting a game is as easy as this :)
            Application.Start(new Game());
        }
    }
}
