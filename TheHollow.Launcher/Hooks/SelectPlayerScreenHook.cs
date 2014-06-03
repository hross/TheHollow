using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ThePit;
using ThePit.GameUI;
using ThePit.UI;

namespace TheHollow.Launcher
{
    /// <summary>
    /// TODO: clean this up with custom settings.
    /// </summary>
    public class SelectPlayerScreenHook : SelectPlayerScreen
    {
        public SelectPlayerScreenHook()
            : base()
        {
        }

        /// <summary>
        /// This is where we inject our custom characters into the new game screen.
        /// </summary>
        public override void PopulatePages()
        {
            this._playerClasses.Clear();
            this._playerStages.Clear();
            this._pages.ClearPages();

            var selections = Data.GetPlayerSelectionsAvailable().ToList();
            selections.AddRange(ContentManagerHook.AdditionalCharacters());

            foreach (ICharacterClass playerClass in selections)
            {
                PlayerStage playerStage = new PlayerStage();
                playerStage.CharacterClass = playerClass;
                playerStage.IsAvailable = this.IsAvailable(playerClass);
                this._playerClasses.Add(playerClass);
                this._playerStages.Add(playerStage);
                this._pages.AddPage(playerClass.DisplayName, (Page)playerStage, (object)playerClass);
            }
            this.SelectedPlayer = Enumerable.FirstOrDefault<ICharacterClass>((IEnumerable<ICharacterClass>)this._playerClasses, (Func<ICharacterClass, bool>)(x => this.IsAvailable(x)));
            this.ForceResize();
        }
    }
}
