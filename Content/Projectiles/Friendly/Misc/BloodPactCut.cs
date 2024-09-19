using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class BloodPactCut : ModProjectile
    {
        private static int duration = 30;
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = duration;
            Projectile.penetrate = -1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            float progressZeroToOne = 1f-(Projectile.timeLeft / (float)duration);

            float scaleX = 1f;
            float scaleY = (float)Math.Sin(progressZeroToOne * Math.PI)*4f;

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Texture2D tex = TextureAssets.Extra[98].Value;

            Main.EntitySpriteDraw(tex, drawPosition, null, lightColor, MathHelper.PiOver4, tex.Size()/2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
            return false;
        }
    }
}
