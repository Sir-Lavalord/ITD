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

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class SoulTalismanBlast : BigBlankExplosion
    {
        public override int Lifetime => 40;
        public override Vector2 ScaleRatio => new Vector2(1.5f, 1f);

        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.LightBlue * 1.6f, Color.PaleTurquoise, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
        }
        public override string Texture => ITD.BlankTexture;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            base.AI();
        }
        public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
    }
}