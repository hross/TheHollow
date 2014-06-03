using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ThePit;
using ThePit.GameUI;

namespace TheHollow.Launcher
{
    /// <summary>
    /// This hook is used to add a menu item to the in game screen. We could extend this a lot further to
    /// create custom menu options.
    /// </summary>
    public class InGameScreenHook : InGameScreen
    {
        public InGameScreenHook() : base()
        {
            base._options.AddOption("Dump Content", () => DumpContent());
            base._optionsWithSave.AddOption("Dump Content", () => DumpContent());
        }

        /// <summary>
        /// A custom menu items that dumps some content to disk.
        /// </summary>
        private static void DumpContent()
        {
            var cm = Game1.SpriteManager.GeneralContent as ContentManagerHook;

            if (null == cm)
                return;

            string contentBase = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["AssetDumpPath"]);

            // pilot character data
            cm.DumpSpriteLibrary("characters/sol_pilot/lightArmour", Path.Combine(contentBase, "sol_pilot.SpriteLibrary.txt"));
            cm.DumpSpriteLibrary("characters/sol_pilot/mediumArmour", Path.Combine(contentBase, "sol_pilot.medium.SpriteLibrary.txt"));
            cm.DumpSpriteLibrary("characters/sol_pilot/heavyArmour", Path.Combine(contentBase, "sol_pilot.heavy.SpriteLibrary.txt"));
            cm.DumpTexture("characters/sol_pilot/LightArmour_0", Path.Combine(contentBase, "sol_pilot.lighArmour_0.png"));
            cm.DumpTexture("characters/sol_pilot/MedArmour_0", Path.Combine(contentBase, "sol_pilot.MedArmour_0.png"));
            cm.DumpTexture("characters/sol_pilot/HeavyArmour_0", Path.Combine(contentBase, "sol_pilot.heavyArmour_0.png"));

            cm.DumpSpriteLibrary("game_sprites", Path.Combine(contentBase, "game_sprites.txt")); // all game sprites

            cm.DumpDescriptionLibrary("data\\d00", Path.Combine(contentBase, "d00.descriptions.txt")); // character listing
            cm.DumpDescriptionLibrary("data\\d13", Path.Combine(contentBase, "d13.animations.txt")); // select player animation listing

            cm.DumpTexture("ui\\select_character_0", Path.Combine(contentBase, "select_character_0.png")); // select player screen
           


            DebugLog.Write("Dump completed.");
        }
    }
}
