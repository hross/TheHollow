using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThePit;

namespace TheHollow.Launcher
{
    /// <summary>
    /// JSON seriliazable version of a Pit sprite description.
    /// 
    /// This can be used to load and save our own sprite libraries without needing
    /// ThePit classes and avoiding serilization of non-serialization friendly XNA classes.
    /// </summary>
    public class ModSpriteDescription
    {
        public string Name { get; set; }

        public string TextureFile { get; set; }

        public ModRectangle TextureRect { get; set; }

        public bool FlipX { get; set; }

        public bool FlipY { get; set; }

        public Point DrawOffset { get; set; }

        public Color? Color { get; set; }

        public SpriteDescription ToSpriteDescription()
        {
            return new SpriteDescription
            {
                Color = this.Color,
                DrawOffset = this.DrawOffset,
                FlipX = this.FlipX,
                FlipY = this.FlipY,
                Name = this.Name,
                TextureFile = this.TextureFile,
                TextureRect = this.TextureRect.ToXnaRectangle()
            };
        }
    }

    public class ModRectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Rectangle ToXnaRectangle()
        {
            return new Rectangle(this.X, this.Y, this.Width, this.Height);
        }
    }
}
