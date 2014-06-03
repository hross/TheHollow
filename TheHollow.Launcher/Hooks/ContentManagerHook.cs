using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ThePit;

namespace TheHollow.Launcher
{
    /// <summary>
    /// This is really the meat and potatoes for most of the modding. It allows us to load custom resources
    /// instead of pullilng the ThePit content but still use the Pit's built in structure to parse/utilize those resources.
    /// </summary>
    public class ContentManagerHook : ContentManager
    {
        #region Fields and Properites

        // the base stuff we need to operate a content manager hook
        private ContentManager _content;
        private GraphicsDevice _graphicsDevice;

        private static string _customContentPath = null;

        /// <summary>
        /// Where are we going to load our custom content from.
        /// </summary>
        private static string CustomContentPath
        {
            get
            {
                if (string.IsNullOrEmpty(_customContentPath))
                    _customContentPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigurationManager.AppSettings["ModContentPath"]);

                return _customContentPath;
            }
        }

        /// <summary>
        /// A very simple (non-threadsafe, but don't think its needed) cache of custom textures so they aren't
        /// loaded from disk every time.
        /// </summary>
        private Dictionary<string, Texture2D> _modTextures = new Dictionary<string, Texture2D>();

        #endregion

        public ContentManagerHook(ContentManager content, GraphicsDevice graphicsDevice) : base(content.ServiceProvider)
        {
            _content = content;
            _graphicsDevice = graphicsDevice;
            this.RootDirectory = content.RootDirectory;
        }

        /// <summary>
        /// This is the main method we hook to insert custom content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public override T Load<T>(string assetName)
        {
            // just one more check because of the all the places thi might get reset and this is not overridable
            if (this.RootDirectory != _content.RootDirectory)
                _content.RootDirectory = this.RootDirectory;

            // add character descriptions to existing character data
            if (assetName == Names.Content.DataCharacters)
            {
                return LoadCharacterDescriptions<T>(assetName, "Description.txt");
            }

            // add select character animations to existing character animation data
            if (assetName == Names.Content.DataAnimations)
            {
                return LoadCharacterDescriptions<T>(assetName, "Animations.txt");
            }

            // hook additional game sprite data (for avators and selection screens, etc)
            if (assetName == "game_sprites")
            {
                return AppendCharacterSpriteLibrary<T>(assetName, "GameSprites.SpriteLibrary.txt");
            }
            
            // check to see if this is a custom asset
            if (!assetName.StartsWith(ConfigurationManager.AppSettings["ModContentPrefix"]))
            {
                // if not just do the normal thing
                return _content.Load<T>(assetName);
            }
            else
            {
                // get rid of the prefix so we can load as a file
                assetName = assetName.Replace(ConfigurationManager.AppSettings["ModContentPrefix"], "");

                if (assetName.StartsWith("/") || assetName.StartsWith("\\"))
                {
                    assetName = assetName.Substring(1, assetName.Length - 1);
                }
            }

            // --- we found a custom asset so start jumping through some hoops


            // handle custom sprite libraries
            if (assetName.EndsWith("SpriteLibrary"))
            {
                return (T)(object)LoadSpriteLibrary(assetName, Path.Combine(CustomContentPath, assetName + ".txt"));
            }

            // handle custom textures
            if (assetName.EndsWith(".png"))
            {
                return (T)(object)LoadTexture(assetName, Path.Combine(CustomContentPath, assetName));
            }
            
            // fallback, just in case
            return _content.Load<T>(assetName);
        }

        #region Load Character Data

        private T LoadCharacterDescriptions<T>(string assetName, string descriptionFile)
        {
            // load up original asset
            var library = _content.Load<DescriptionLibrary>(assetName);

            // build from our character directories
            DirectoryInfo di = new DirectoryInfo(Path.Combine(CustomContentPath, "Characters"));
            if (di.Exists)
            {
                foreach (var subDir in di.GetDirectories())
                {
                    var description = subDir.EnumerateFiles(descriptionFile);
                    if (null != description && description.Count() > 0)
                    {
                        // read the new character description
                        var newCharacterLines = File.ReadAllLines(description.First().FullName);

                        // load up our modded description library and encode it
                        var newDescriptionLibrary = new ModDescriptionLibrary();

                        if (Assembly.GetAssembly(typeof(ThePit.Game1)).GetName().Version > new Version(1,2,5))
                        {
                            // we need to rebuild character lines with an additional property
                            List<string> versionLines = new List<string>();
                            foreach (string line in newCharacterLines)
                            {
                                if (line.Contains("Role: Player"))
                                {
                                    // add game version for later versions so our characters work
                                    versionLines.Add("	GameVersion: ThePit");
                                }
                                versionLines.Add(line);
                            }

                            newDescriptionLibrary.EncodeLines(versionLines);
                        }
                        else
                        {
                            newDescriptionLibrary.EncodeLines(newCharacterLines);
                        }

                        foreach (var line in newDescriptionLibrary.Lines)
                        {
                            library.Lines.Add(line);
                        }
                    }
                }
            }

            // return the new asset
            return (T)(object)library;
        }

        private T AppendCharacterSpriteLibrary<T>(string assetName, string spriteLibrary)
        {
            var library = _content.Load<SpriteLibrary>(assetName);

            // build from our character directories
            DirectoryInfo di = new DirectoryInfo(Path.Combine(CustomContentPath, "Characters"));
            if (di.Exists)
            {
                foreach (var subDir in di.GetDirectories())
                {
                    var description = subDir.EnumerateFiles(spriteLibrary);

                    if (null != description && description.Count() > 0)
                    {
                        AppendSpriteLibrary(library, assetName, description.First().FullName);
                    }
                }
            }

            // return the new asset
            return (T)(object)library;
        }

        public static IEnumerable<string> AdditionalCharacterNames()
        {
            List<string> characters = new List<string>();

            // build from our character directories
            DirectoryInfo di = new DirectoryInfo(Path.Combine(CustomContentPath, "Characters"));
            if (di.Exists)
            {
                foreach (var subDir in di.GetDirectories())
                {
                    characters.Add(subDir.Name.ToLower());
                }
            }

            return characters;
        }

        public static IEnumerable<CharacterClass> AdditionalCharacters()
        {
            List<CharacterClass> characters = new List<CharacterClass>();

            foreach (var name in AdditionalCharacterNames())
                characters.Add(Data.GetCharacterClass(name));

            return characters;
        }

        #endregion

        #region Custom Content Loading Methods

        /// <summary>
        /// Load a custom sprite library from disk instead of an XNB content store.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private SpriteLibrary LoadSpriteLibrary(string assetName, string fileName)
        {
            // grab our list of sprite descriptions
            SpriteLibrary sprites = new SpriteLibrary();
            Dictionary<string, ModSpriteDescription> allSprites = JsonConvert.DeserializeObject<Dictionary<string, ModSpriteDescription>>(
                File.ReadAllText(fileName));

            // dump the created sprites into the library
            foreach (var key in allSprites.Keys)
            {
                var sprite = Sprite.Create(this, allSprites[key].ToSpriteDescription());
                // the next line is important - if we don't load the texture here
                // ThePit will load it for us on an un-injected contet manager
                sprite.Texture = this.Load<Texture2D>(sprite.TextureAsset);
                sprites.Add(sprite);
            }

            return sprites;
        }

        /// <summary>
        /// Append a custom (json based) sprite library to an existing one.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private SpriteLibrary AppendSpriteLibrary(SpriteLibrary sprites, string assetName, string fileName)
        {
            // load addendums
            Dictionary<string, ModSpriteDescription> allSprites = JsonConvert.DeserializeObject<Dictionary<string, ModSpriteDescription>>(
                File.ReadAllText(fileName));

            // dump the created sprites into the library
            foreach (var key in allSprites.Keys)
            {
                if (!sprites.Contains(key))
                {
                    var sprite = Sprite.Create(this, allSprites[key].ToSpriteDescription());
                    // the next line is important - if we don't load the texture here
                    // ThePit will load it for us on an un-injected contet manager
                    sprite.Texture = this.Load<Texture2D>(sprite.TextureAsset);
                    sprites.Add(sprite);
                }
            }

            return sprites;
        }

        /// <summary>
        /// Load a texture from a file instead of an XNB file.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private Texture2D LoadTexture(string assetName, string fileName)
        {
            // if we have a cached version, use it
            if (_modTextures.ContainsKey(assetName))
                return _modTextures[assetName];

            // load and cache
            using (FileStream stream = File.OpenRead(fileName))
            {
                var texture = Texture2D.FromStream(_graphicsDevice, stream);
                _modTextures.Add(assetName, texture);
                return texture;
            }
        }

        #endregion

        #region Dump methods

        /// <summary>
        /// Dump an existing texture to a file so we can see how it works (saved as png).
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fileName"></param>
        public void DumpTexture(string assetName, string fileName)
        {
            // load a texture
            var texture = this.Load<Texture2D>(assetName);
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                //texture.SaveAsJpeg(fs, texture.Width, texture.Height); we can alos save as jpeg
                texture.SaveAsPng(fs, texture.Width, texture.Height);
            }
        }

        /// <summary>
        /// Dump an existing description library to a file so we can see how it works.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fileName"></param>
        public void DumpDescriptionLibrary(string assetName, string fileName)
        {
            var descriptionLibrary = this.Load<DescriptionLibrary>(assetName);

            File.WriteAllLines(fileName, descriptionLibrary.DecodeLines());
        }

        /// <summary>
        /// Dump an existing sprite library to a text file so we can see how it works.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="fileName"></param>
        public void DumpSpriteLibrary(string assetName, string fileName)
        {
            // convert the sprite library into descriptions so we can serialize the sprite info
            var sl = _content.Load<SpriteLibrary>(assetName);
            var descriptions = new Dictionary<string, ModSpriteDescription>();

            foreach (var key in sl.Keys)
            {
                var sprite = (Sprite)sl[key];
                // note this is our modded class
                // if we load a file like this we will need to load into the modded class and
                // convert it back to the library type on the fly
                var desc = new ModSpriteDescription
                {
                    Color = sprite.Color,
                    DrawOffset = sprite.DrawOffset,
                    FlipX = sprite.FlipX,
                    FlipY = sprite.FlipY,
                    Name = sprite.Name,
                    TextureFile = sprite.TextureAsset,
                    TextureRect = new ModRectangle {
                        X = sprite.TextureRect.X, 
                        Y = sprite.TextureRect.Y, 
                        Height = sprite.TextureRect.Height, 
                        Width = sprite.TextureRect.Width 
                    }
                };
                descriptions.Add(key, desc);
            }

            File.WriteAllText(fileName, JsonConvert.SerializeObject(descriptions,
                new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        #endregion

        #region IDisposabe

        public override void Unload()
        {
            _content.Unload();
            base.Unload();

            foreach (var texture in _modTextures.Values)
            {
                texture.Dispose();
            }
            _modTextures.Clear();
        }

        #endregion
    }
}
