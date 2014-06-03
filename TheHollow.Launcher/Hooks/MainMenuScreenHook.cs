using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThePit;
using ThePit.GameUI;

namespace TheHollow.Launcher
{
    public class MainMenuScreenHook : MainMenuScreen
    {
        /// <summary>
        /// This is where we inject our custom characters into the load screen.
        /// </summary>
        public override void OpenLoadDialog()
        {
            OptionsDialog optionsDialog = new OptionsDialog();

            var selections = Data.GetPlayerSelectionsAvailable().ToList();
            selections.AddRange(ContentManagerHook.AdditionalCharacters());

            foreach (ICharacterClass playerClass in selections)
            {
                if (Game1.Instance.SaveGameExists(playerClass))
                {
                    string fileName = Game1.GetSaveGameFileName(playerClass);
                    optionsDialog.AddOption(playerClass.DisplayName, (System.Action)(() => this.Context.GameMessages.Post(Names.GameMessages.LoadGame, (object)fileName)));
                }
                else
                {
                    var btn = optionsDialog.AddOption("Empty", (System.Action)null);
                    btn.Enabled = false;
                }
            }
            optionsDialog.AutoOpen();
        }
    }
}
