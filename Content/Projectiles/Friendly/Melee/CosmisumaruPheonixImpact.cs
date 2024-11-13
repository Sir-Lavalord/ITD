using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;
using ITD.Content.Tiles.Misc;
using ITD.Content.Dusts;
using Terraria.ID;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class CosmisumaruPheonixImpact : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 158;
            Projectile.height = 144;
            Projectile.scale = 1.2f;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 300f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.light = 0.1f;
            Projectile.damage = 45;
            Projectile.CritChance = 4;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;

            Projectile.noEnchantmentVisuals = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (5);
        }

        public override void AI()
        {
            Projectile.rotation += 0.4f * (float)Projectile.direction;
            Projectile.alpha += 10;
            Projectile.scale *= 1.01f;

            float dustRotation = Projectile.rotation + Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7f;
            Vector2 dustPosition = Projectile.Center + dustRotation.ToRotationVector2() * 84f * Projectile.scale;
            Vector2 dustVelocity = (dustRotation + Projectile.ai[0] * MathHelper.PiOver2).ToRotationVector2();
            if (Main.rand.NextFloat() * 2f < Projectile.Opacity)
            {
                Color dustColor = Color.Lerp(Color.SkyBlue, Color.White, Main.rand.NextFloat() * 0.3f);
                Dust coloredDust = Dust.NewDustPerfect(Projectile.Center + dustRotation.ToRotationVector2() * (Main.rand.NextFloat() * 80f * Projectile.scale + 20f * Projectile.scale), DustID.FireworksRGB, dustVelocity * 1f, 100, dustColor, 0.4f);
                coloredDust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.15f;
                coloredDust.noGravity = true;
            }

            if (Main.rand.NextFloat() * 1.5f < Projectile.Opacity)
            {
                Dust.NewDustPerfect(dustPosition, DustID.TintableDustLighted, dustVelocity, 100, Color.SkyBlue * Projectile.Opacity, 1.2f * Projectile.Opacity);
            }

            if (Projectile.scale > 1.5f)
            {
                Projectile.active = false;
            }
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