using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheHollow.Generator
{
    /// <summary>
    /// Simple program for running our IL modifications on ThePit.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // load ThePit.exe
            var exePath = ConfigurationManager.AppSettings["ThePitExePath"];
            var newExepath = ConfigurationManager.AppSettings["NewExePath"];

            // modify it so we can load our mods
            ModGenerator generator = new ModGenerator(exePath);
            generator.BatchMakePublicMethodsAndVirtualize(Lines("classes.txt"));
            generator.BatchMakePublic(Lines("public_only_classes.txt"));

            // make some method calls virtual so we can override them
            generator.MakeCallVirtual("SelectPlayerScreen", "OnOpen", "PopulatePages");
            generator.MakeCallVirtual("MainMenuScreen", "*compilergenerated*", "OpenLoadDialog");
            generator.MakeCallVirtual("Game1", "UpdatePlayerInput", "UpdatePlayerActionInput");

            // save the executable modifications
            generator.Save(newExepath);
        }

        /// <summary>
        /// Helper for loading lines from a text file. # means comment.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static IEnumerable<string> Lines(string fileName)
        {
            return File.ReadAllLines(fileName).Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"));
        }
    }
}
