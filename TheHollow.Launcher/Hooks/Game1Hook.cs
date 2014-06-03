using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using ThePit;
using ThePit.UI;

namespace TheHollow.Launcher
{
    /// <summary>
    /// This helps us launch the game and inject some of our custom stuff.
    /// </summary>
    public class Game1Hook : Game1
    {
        public Game1Hook() : base()
        {
        }

        public override void Initialize()
        {
            // inject our own content handler
            this.Content = new ContentManagerHook(this.Content, this.GraphicsDevice);

            // override the static version of the content manager in case we need it
            Game1.SpriteManager.GeneralContent = this.Content;

            base.Initialize();
        }

        public override void LoadContent()
        {
            // first run the default Pit content load process
            base.LoadContent();

            // now go back and inject our own ingame screen handler
            // (this allows us to hook things like player input and in game menus)
            var screen = (Screen) new InGameScreenHook();
            screen.Name = Names.Screens.InGame;
            this.uiManager._screens[Names.Screens.InGame] = screen;
            
            // inject select player and main menu screens
            // (so we can add characters)
            screen = (Screen)new SelectPlayerScreenHook();
            screen.Name = Names.Screens.SelectPlayer;
            this.uiManager._screens[Names.Screens.SelectPlayer] = screen;

            screen = (Screen)new MainMenuScreenHook();
            screen.Name = Names.Screens.MainMenu;
            this.uiManager._screens[Names.Screens.MainMenu] = screen;
        }

        /// <summary>
        /// This is how we hook keyboard/other input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="allowFull"></param>
        /// <returns></returns>
        public override bool UpdatePlayerActionInput(InputState input, bool allowFull)
        {
            // this is a simple, hard coded example to handle input from a player
            // we can probably add some kind of modifier key here for doing something else cool
            if (input.IsKeyReleased(Keys.OemPlus))
            {
                bool canAutoTarget = false;
                bool canTargetSpace = false;
                bool isAutoTarget;
                bool isTargetBlocked;

                // find something to kill
                ITarget targetHere = this._world.Cursor.GetTargetHere(canAutoTarget, canTargetSpace, out isAutoTarget, out isTargetBlocked);
                if (null != targetHere)
                {
                    // output some data to the debug log
                    DebugLog.Write("Terminating target: {0}", targetHere.DisplayName);

                    // kill it
                    Damage.ApplyDamage(targetHere, targetHere.Health);
                }
            }

            return base.UpdatePlayerActionInput(input, allowFull);
        }
        
        

    }
}
