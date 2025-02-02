using Terraria;
using System;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

using ITD.Content.Dusts;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Ranger
{
    public class SkyshooterFallingStar : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; 
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 22; Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
			Projectile.timeLeft = 500;
            Projectile.ai[0] = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.rand.NextBool(2))
            {
                Projectile.scale = 1f + Main.rand.NextFloat(0.5f);
            }
            else
            {
                Projectile.scale = 1f - Main.rand.NextFloat(0.15f);
            }
        }

        public override void AI()
        {
            Projectile.rotation += 0.05f;
            Projectile.velocity *= 1.005f;

            Projectile.velocity.Y = Projectile.velocity.Y + 0.4f; 
            if (Projectile.velocity.Y > 16f) 
            {
                Projectile.velocity.Y = 16f;
            }

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}