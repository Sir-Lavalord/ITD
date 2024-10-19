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

namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicLightningBlast : BigBlankExplosion
    {
        public override int Lifetime => 60;
        public override Vector2 ScaleRatio => new Vector2(1.5f, 1f);

        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.MediumPurple * 1.6f, Color.DarkBlue, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
        }
        public override string Texture => ITD.BlankTexture;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraExplode"), Projectile.Center);
            }
        }
        public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
    }
}