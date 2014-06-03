#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using ThePit;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Configuration;
#endregion

namespace TheHollow.Launcher
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // enable debug mode so we can get some additional hotkeys, etc
            // (i.e. hold down ALT + Number)
            DebugFlags.EnableDebugKeys = true;

            // change the content directory to whatever custom content location we want
            string basePath = ConfigurationManager.AppSettings["ContentPath"];

            // here is where we spin up a copy of the game to play
            using (var game = new Game1Hook())
            {
                game.Content.RootDirectory = Path.Combine(basePath, "Content");

                // note that sometimes the game uses the working path to load things so we will set this just to be sure
                Directory.SetCurrentDirectory(basePath);

                // yet another content manager we found that might need overrides
                Game1.SpriteManager.GeneralContent.RootDirectory = Path.Combine(basePath, "Content");

                game.Run();
            }
        }            
    }
#endif
}
