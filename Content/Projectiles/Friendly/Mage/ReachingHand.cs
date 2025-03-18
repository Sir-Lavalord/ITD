using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Audio;
using Mono.Cecil;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class ReachingHand : ModProjectile
    {
		private const int duration = 50;
		public bool clasp;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.height = 78;
            Projectile.width = 70;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.Opacity -= Projectile.ai[0]*0.001f;
			
			Projectile.ai[0]++;
            if (Projectile.ai[0] > duration)
                Projectile.Kill();
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			if (!clasp) {
				clasp = true;
				Projectile.velocity *= 0.25f;
				SoundEngine.PlaySound(SoundID.NPCHit54, Projectile.Center);
			}
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D projectileTexture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, projectileTexture.Height * 0.5f);
			SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			lightColor *= Projectile.Opacity;
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 trailPos = Projectile.oldPos[k] - Main.screenPosition + (Projectile.Size * 0.5f) + new Vector2(0f, Projectile.gfxOffY);
				Color color = lightColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.spriteBatch.Draw(projectileTexture, trailPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 2, effects, 0f);
			}
			Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
			Main.spriteBatch.Draw(projectileTexture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0f);
			return false;
        }
    }
}