﻿using Lichen;
using Lichen.Entities;
using Lichen.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace LifeDeath.Scenes
{
    public class Level : Scene
    {
        Entity camera;
        Entity player;
        public Entity Player
        {
            get { return player; }
            set { player = value; }
        }
        public bool Hiding { get; set; }
        Entity enemy, bullet, warning;
        int warningTimer;
        List<Entity> enemies, bullets;
        int currentEnemy, currentBullet;
        Entity enemyContainer;
        Entity lycoris;
        //Entity zone1, zone2, zone3;

        List<Entity> actorList;

        Font font;
        Song bgm;
        public SoundEffect PlayerSfx { get; set; }
        public SoundEffectInstance PlayerSfxInstance { get; set; }

        Random random;

        int furthestDistance;
        float distanceTraveled;

        // Create a reference set of entities and load necessary assets.
        public override void Preload(Entity root)
        {
            this.root = root;
            actorList = new List<Entity>();

            player = new Entity()
                .AddRenderComponent(new SpriteComponent(GlobalServices.GlobalSprites.Register("lifedeath:spirit")))
                .AddChainComponent("control", new Components.PlayerControlComponent(this));
            //.AddChainComponent("motion", )
            /*
            new Entity(0, 0)
                .AddRenderComponent(new TextComponent(GlobalServices.GlobalFonts.Register("lifedeath:sans"), "Player 1"))
                .AttachTo(player);
                */
            enemy = new Entity()
                .AddRenderComponent(new SpriteComponent(GlobalServices.GlobalSprites.Register("lifedeath:fae_sm")))
                .AddChainComponent("control", new Components.AI.SeekerAIComponent(this));

            warning = new Entity()
                .AddRenderComponent(new SpriteComponent(GlobalServices.GlobalSprites.Register("lifedeath:fae_warn")));
                //.AddChainComponent("control", new Components.SignComponent(this));

            bullet = new Entity()
                .AddRenderComponent(new SpriteComponent(GlobalServices.GlobalSprites.Register("lifedeath:bullet")))
                .AddChainComponent("control", new Components.AI.BulletComponent(this, -200, 0, 0, 0));

            Sprite lycorisSprite = GlobalServices.GlobalSprites.Register("lifedeath:redlily");
            lycoris = new Entity()
                .AddRenderComponent(new SpriteComponent(lycorisSprite));

            font = GlobalServices.GlobalFonts.Register("lifedeath:sans");
            bgm = GlobalServices.GlobalSongs.Register("lifedeath:Stage");
            PlayerSfx = GlobalServices.GlobalSoundEffects.Register("lifedeath:leaves");
            PlayerSfxInstance = PlayerSfx.CreateInstance();
        }

        // Create the scene's entities by cloning reference entities.
        public override void Load()
        {
            container = new Entity()
                .AttachTo(root);

            camera = new Entity()
                .SetRenderByDepth(true)
                .AddUpdateComponent(new Components.CameraComponent(player))
                .AttachTo(container);

            /*
            zone1 = new Entity().AttachTo(container);
            zone2 = new Entity().AttachTo(container);
            zone3 = new Entity().AttachTo(container);
            */

            random = new Random();
            double phi = (Math.Sqrt(5d)-1d)/2d;
            double theta = random.NextDouble();
            for (int i = 0; i < 200; i++)
            {
                lycoris.Clone()
                    //.SetPosition(random.Next(0, 700), random.Next(0, 700))
                    //.AddChainComponent("motion", new Components.WindyComponent(random.Next(0, 1280), random.Next(0, 720)))
                    .AddChainComponent("motion", new Components.WindyComponent(this, camera, (float)(theta * 1280d + random.NextDouble() * 200d - 100d), (float)i * 920f / 200f))
                    .AttachTo(camera)
                    .AddActorList(actorList);
                theta += phi;
                if (theta > 1d) theta -= 1d;
            }

            enemyContainer = new Entity()
                .AttachTo(camera);
            //enemy.Clone().SetPosition(200, 200).AttachTo(camera)
            //enemy.Clone().SetPosition(200, 200).AttachTo(enemyContainer)
            //    .AddActor(actorList);

            //Entity player1 = player.Clone().SetPosition(300, 200).AttachTo(container);
            player.SetPosition(880, 200).AttachTo(camera)
                .AddActor(actorList);

            /*
            TextComponent tc = (TextComponent)player1.Children.First.Value.RenderComponent;
            tc.Value = "PLAYER";
            */

            bullets = new List<Entity>();
            for (int i = 0; i < 100; i++)
            {
                bullets.Add(bullet.Clone().AttachTo(camera).AddActor(actorList));
            }

            enemies = new List<Entity>();
            for (int i = 0; i < 20; i++)
            {
                enemies.Add(enemy.Clone().AttachTo(enemyContainer).AddActor(actorList));
            }

            warning.AttachTo(container).SetPosition(0, -10);
        }

        public void UpdateDistance(float delta)
        {
            if (delta < 0) distanceTraveled -= delta;

            int newDistance = (int)(distanceTraveled / 500f);
            if (newDistance > furthestDistance)
            {
                furthestDistance = newDistance;
                MakeEnemy((float)random.NextDouble() * 980f + 50f, player.Y - 600f);
                /*
                enemies[currentEnemy].SetPosition(player.X, player.Y - 400f).AttachTo(enemyContainer)
                    .AddActor(actorList);
                currentEnemy++;
                if (currentEnemy >= 20) currentEnemy = 0;
                */
            }

            warningTimer++;
            if (warningTimer > 30) warning.Visible = false;
        }

        public void MakeEnemy(float x, float y)
        {
            Entity enemy = enemies[currentEnemy];
            ((Components.AI.SeekerAIComponent)enemy.UpdateChains["control"].First()).SetAIMode(furthestDistance);
            enemy.SetPosition(x, y);
            enemy.Visible = true;
            enemy.Active = true;

            currentEnemy++;
            if (currentEnemy >= 20) currentEnemy = 0;

            warning.X = x;
            warningTimer = 0;
            warning.Visible = true;
        }

        public void MakeBullet(float x, float y, float vx, float vy)
        {
            /*
            bullet.Clone()
                .AddChainComponent("motion", new Components.AI.BulletComponent(this, x, y, vx, vy));
            */
            Entity bullet = bullets[currentBullet];
            ((Components.AI.BulletComponent)bullet.UpdateChains["control"].First()).Set(x, y, vx, vy);
            bullet.Visible = true;
            bullet.Active = true;

            currentBullet++;
            if (currentBullet >= bullets.Count) currentBullet = 0;
        }

        // Delete the scene. (Reference entities and assets remain.)
        public override void Unload()
        {
            // TODO: Should this also undo the preloading?
            container = null;
            // TODO: Is that enough to destroy the scene entities? Or do I need to parse through them all?
            //     Is garbage collection hindered by parent and child referencing each other?
        }

        public void Reset()
        {
            //enemyContainer.Children.Clear();
            actorList.Clear();

            //enemy.Clone().SetPosition(200, 200).AttachTo(enemyContainer)
            //    .AddActor(actorList);

            ((Components.PlayerControlComponent)(player.UpdateChains["control"].First())).Reset();

            player.SetPosition(880, 200)
                .AddActor(actorList);

            foreach (Entity enemy in enemies)
            {
                enemy.AddActor(actorList);
            }

            foreach (Entity bullet in bullets)
            {
                bullet.AddActor(actorList);
            }

            furthestDistance = 0;
            distanceTraveled = 0;

            MediaPlayer.Volume = 0.5f;
            MediaPlayer.Play(bgm);
            MediaPlayer.IsRepeating = true;

            foreach (Entity e in enemies)
            {
                e.X = -200;
                e.Y = 0;
                e.Visible = false;
                e.Active = false;
            }

            foreach (Entity b in bullets)
            {
                ((Components.AI.BulletComponent)b.UpdateChains["control"].First()).Reset();
                b.Visible = false;
                b.Active = false;
            }

            warning.Visible = false;
            warningTimer = 0;
        }
    }
}
