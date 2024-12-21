using ITD.Content.Backgrounds;
using ITD.Networking;
using ITD.Particles;
using ITD.Utilities.EntityAnim;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Menus
{
    public struct MenuSnow(Vector2 position, int lifetime, float scale)
    {
        public Vector2 Position = position;
        public Vector2 Velocity;
        public int SpawnTime = lifetime;
        public int TimeLeft = lifetime;
        // used for "random" frame choosing
        public int SpawnIndex;
        public float Scale = scale;
        public void Update()
        {
            Position += Velocity;
            TimeLeft--;
            AI();
        }
        public void AI()
        {
            Vector2 direction = Vector2.UnitY.RotatedBy(-0.4f);
            // actual movement speed
            float speed = 2f;

            // wave properties
            float sineAmplitude = 32f;
            float sineFrequency = 0.05f;

            // so we can actually make the NPC move in a sine wave
            Vector2 perpendicular = direction.RotatedBy(Math.PI / 2d);
            float sineOffset = (float)Math.Sin(Main.timeForVisualEffects * sineFrequency + SpawnIndex) * sineAmplitude;
            Velocity = direction * speed + perpendicular * sineOffset / 16f;
        }
    }
    public class ITDAlpha : ModMenu
    {
        public override int Music => MusicLoader.GetMusicSlot("ITD/Menus/Music/AlphaMenu");
        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("ITD/Menus/Textures/AlphaMenu");
        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<BlueshroomGrovesSurfaceBackgroundStyle>();
        private readonly List<MenuSnow> snows = [];
        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            if (Main.rand.NextBool(10))
            {
                Vector2 spawnPos = Vector2.UnitX * Main.rand.NextFloat(Main.screenWidth);
                snows.Add(new MenuSnow(spawnPos, 400, Main.rand.NextFloat(1f, 1.5f)) { SpawnIndex = snows.Count });
                //snows.Add(new MenuSnow(Main.MouseScreen, 200, 2f));
            }
            for (int i = 0; i < snows.Count; i++)
            {
                MenuSnow snow = snows[i];
                // cuz of how structs work we have to modify the struct first, then reassign that value
                snow.Update();
                snows[i] = snow;
            }
            snows.RemoveAll(s => s.TimeLeft <= 0);
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Menus/Textures/AlphaSnow").Value;
            for (int i = 0; i < snows.Count; i++)
            {
                int sid = snows[i].SpawnIndex;
                int chosenFrame = sid % 3 == 0 ? 3 : sid % 2 == 0 ? 2 : 1;
                float progress = 1f - snows[i].TimeLeft / (float)snows[i].SpawnTime;
                Rectangle frame = tex.Frame(1, 3, 0, chosenFrame);
                float smooth = snows[i].Scale * (float)Math.Sin(Math.PI * progress);
                spriteBatch.Draw(tex, snows[i].Position, frame, Color.White, snows[i].TimeLeft / 6f, new Vector2(tex.Width * 0.5f, tex.Height / 3 * 0.5f), smooth, SpriteEffects.None, 0f);
            }
            //drawColor = Color.White;
            //Main.dayTime = false;
            //Main.time = Main.nightLength / 2f;
            logoScale -= 0.2f;
            logoDrawCenter.Y += 30f;
            // draw the logo
            Asset<Texture2D> glowmask = ModContent.Request<Texture2D>("ITD/Menus/Textures/AlphaMenu_Glow");
            spriteBatch.Draw(Logo.Value, logoDrawCenter, null, drawColor, logoRotation, Logo.Size() * 0.5f, logoScale, SpriteEffects.None, 0f);
            Color pulse = Color.White * (float)((Math.Sin(Main.timeForVisualEffects / 16f) + 1) / 2f);
            spriteBatch.Draw(glowmask.Value, logoDrawCenter, null, pulse, logoRotation, Logo.Size() * 0.5f, logoScale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
