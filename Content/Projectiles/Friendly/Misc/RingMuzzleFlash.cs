using ITD.Content.Projectiles.Friendly.Misc;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Buffs.Debuffs;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Microsoft.CodeAnalysis;
using Terraria.Graphics.Shaders;
using ITD.Particles;

namespace ITD.Content.Projectiles.Hostile
{
    public class RingMuzzleFlash : ITDParticle
    {
        
        private float OriginalScale;
        private float FinalScale;
        private float opacity;
        private Color BaseColor;
        public override void SetDefaults()
        {
            canvas = ParticleDrawCanvas.WorldUnderProjectiles;
        }
        public RingMuzzleFlash(Vector2 pos, Vector2 vel, Color col, Vector2 morph, float rotation, float originalScale, float finalScale, int lifeTime)
        {

            position = pos;
            velocity = vel;
            BaseColor = col;
            texMorph = morph;
            OriginalScale = originalScale;
            FinalScale = finalScale;
            scale = originalScale;
            spawnTimeLeft = lifeTime;
            rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void Update()
        {
            scale = MathHelper.Lerp(OriginalScale, FinalScale, ProgressZeroToOne);

            opacity = (float)Math.Sin(MathHelper.PiOver2 + timeLeft * MathHelper.PiOver2);

            BaseColor = BaseColor * opacity;
            Lighting.AddLight(position, BaseColor.R / 255f, BaseColor.G / 255f, BaseColor.B / 255f);
            velocity *= 0.95f;
        }

        public override void PostDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin").Value;
            spriteBatch.Draw(tex, position - Main.screenPosition, null, BaseColor * opacity, rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0);
        }

    }
}