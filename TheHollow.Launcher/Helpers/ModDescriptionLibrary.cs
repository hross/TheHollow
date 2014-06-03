using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheHollow.Launcher
{
    /// <summary>
    /// A modified version of the description library. Allows us to decode *and* encode line
    /// based descriptions for future use by ThePit.Data during content load.
    /// 
    /// This can be used to append your own file-based descriptions to Pit runtime content.
    /// </summary>
    public class ModDescriptionLibrary
    {
        public List<ushort[]> Lines = new List<ushort[]>();

        public IEnumerable<string> DecodeLines()
        {
            foreach (ushort[] numArray in this.Lines)
            {
                char[] decoded = new char[numArray.Length];
                for (int index = 0; index < numArray.Length; ++index)
                    decoded[index] = (char)((uint)numArray[index] ^ 48879U);
                yield return new string(decoded);
            }
        }

        public void EncodeLines(IEnumerable<string> lines)
        {
            this.Lines = new List<ushort[]>();

            foreach (string line in lines)
            {
                ushort[] numArray = new ushort[line.Length];

                for (int index = 0; index < line.Length; ++index)
                {
                    numArray[index] = (ushort)((char)line[index] ^ 48879U);
                }

                this.Lines.Add(numArray);
            }
        }
    }
}
