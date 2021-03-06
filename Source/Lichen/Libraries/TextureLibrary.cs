﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lichen.Util;

namespace Lichen.Libraries
{
    public class TextureLibrary : Library<string, Texture2D>
    {
        string fallback = "default:default";
        ContentManager contentManager;

        public TextureLibrary()
        {
            contentManager = GlobalServices.NewContentManager();
        }

        protected override Texture2D Load(string path)
        {
            Texture2D texture;
            Pathfinder pathfinder = Pathfinder.Find(path, "textures", Pathfinder.FileType.image);
            if (pathfinder.Path == null)
            {
                // Load fallback asset.
                Error.LogError("Failed to find texture <" + path + ">. Loading fallback asset <" + fallback + ">.");
                pathfinder = Pathfinder.Find(fallback, "textures", Pathfinder.FileType.image);
                if (pathfinder.Path == null)
                {
                    Error.LogErrorAndShutdown("Failed to find fallback asset.");
                }
            }
            if (pathfinder.Ext.Equals("xnb"))
            {
                contentManager.RootDirectory = pathfinder.ContentPath;
                texture = contentManager.Load<Texture2D>(pathfinder.ContentFile);
                //texture = contentManager.Load<Texture2D>("ball");
            }
            else
            {
                FileStream fileStream = new FileStream(pathfinder.Path, FileMode.Open);
                texture = Texture2D.FromStream(GlobalServices.Game.GraphicsDevice, fileStream);

                // Premultiply the alpha.
                Color[] buffer = new Color[texture.Width * texture.Height];
                texture.GetData(buffer);
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = Color.FromNonPremultiplied(buffer[i].R, buffer[i].G, buffer[i].B, buffer[i].A);
                texture.SetData(buffer);

                fileStream.Dispose();
            }
            /*
            string fullPath = GlobalServices.ContentDirectory + "/" + path;
            if (File.Exists(fullPath + ".xnb"))
            {
                texture = contentManager.Load<Texture2D>(fullPath + ".xnb");
            }
            else
            {
                if (File.Exists(fullPath + ".jpg"))
                    fullPath = fullPath + ".jpg";
                else
                    fullPath = fullPath + ".png";
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                texture = Texture2D.FromStream(GlobalServices.Game.GraphicsDevice, fileStream);
                fileStream.Dispose();
            }
            */
            return texture;
        }

        protected override void Unload(Texture2D texture)
        {
            texture.Dispose();
        }

        public override void Unload()
        {
            contentManager.Unload();
            base.Unload();
        }
    }
}
