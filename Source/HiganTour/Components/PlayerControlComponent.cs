﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lichen;
using Lichen.Entities;
using Lichen.Util;
using Microsoft.Xna.Framework;

namespace HiganTour.Components
{
    class PlayerControlComponent : Lichen.Entities.Component, Lichen.Entities.IUpdateComponent
    {
        Scenes.Level level;
        Vector2 d;
        int dodgeTimer;
        //public bool Hiding { get; set; }

        public PlayerControlComponent(Scenes.Level level)
        {
            this.level = level;
        }

        public void Reset()
        {
            //furthestDistance = 0;
            d.X = 0;
            d.Y = 0;
        }

        public void Update()
        {
            if (dodgeTimer > 0) dodgeTimer--;
            Vector2 vector = new Vector2(0, 0);

            if (GlobalServices.InputManager.Held(Lichen.Input.GameCommand.Up))
            {
                vector.Y = -1;
            }
            if (GlobalServices.InputManager.Held(Lichen.Input.GameCommand.Down))
            {
                vector.Y = 1;
            }
            if (GlobalServices.InputManager.Held(Lichen.Input.GameCommand.Left))
            {
                vector.X = -1;
            }
            if (GlobalServices.InputManager.Held(Lichen.Input.GameCommand.Right))
            {
                vector.X = 1;
            }

            if (GlobalServices.InputManager.Held(Lichen.Input.GameCommand.Action2))
            {
                ((Lichen.Entities.SpriteComponent)Owner.RenderComponent).CurrentAnimation = "hiding";
                vector.X = 0;
                vector.Y = 0;
                level.Hiding++;
            }
            else
            {
                ((Lichen.Entities.SpriteComponent)Owner.RenderComponent).CurrentAnimation = "default";
                level.Hiding = 0;
                if (vector.Y > 0)
                {
                    level.Karma -= 0.5f;
                    level.KarmaChanged = true;
                }
            }

            if (vector.X != 0f || vector.Y != 0f)
            {
                vector.Normalize();
                level.PlayerSfxInstance.Volume = 0.4f;
                level.PlayerSfxInstance.IsLooped = true;
                level.PlayerSfxInstance.Resume();

                if (dodgeTimer <= 0 && GlobalServices.InputManager.Held(Lichen.Input.GameCommand.Action1))
                {
                    //dodgeTimer = 60 * 1;
                    dodgeTimer = 30;
                    vector.X = vector.X * 8f;
                    vector.Y = vector.Y * 8f;
                    d.X = vector.X;
                    d.Y = vector.Y;
                }
            }
            else
            {
                //level.PlayerSfxInstance.Pause();
                level.PlayerSfxInstance.Volume = 0.1f;
                level.PlayerSfxInstance.IsLooped = true;
                level.PlayerSfxInstance.Resume();
            }

            d.X = (d.X * 5f + vector.X) / 6f;
            d.Y = (d.Y * 5f + vector.Y) / 6f;
            Owner.X += d.X * 6f;
            Owner.Y += d.Y * 6f;

            if (Owner.X < 20f)
            {
                Owner.X = 20f;
                if (d.X < 0f) d.X = 0f;
            }
            if (Owner.X > 1280f - 20f)
            {
                Owner.X = 1280f - 20f;
                if (d.X > 0f) d.X = 0f;
            }

            foreach (Entity actor in Owner.ActorList)
            {
                if (Object.ReferenceEquals(actor, Owner)) continue;
                if (Lichen.Util.MathHelper.Distance(Owner, actor) < 50d && !level.DebugMode)
                {
                    ((Game1)Lichen.GlobalServices.Game).ChangeScene(2);
                    return;
                }
            }

            level.UpdateDistance(d.Y * 6f);
        }
    }
}
