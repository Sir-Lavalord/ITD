using Microsoft.Xna.Framework;
using Steamworks;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class CornucopiaProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.height = Projectile.width = 18;
            Projectile.timeLeft = 600;
            Projectile.scale = 1f;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item118, Projectile.Center);
			for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Honey,0, 0, 0, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
            }
        }
		public override void AI()
        {
			float gravity = 0.1f;
			float maxFallSpeed = 7f;
			if (Projectile.shimmerWet)
			{
				gravity = 0.065f;
				maxFallSpeed = 4f;
			}
			else
			{
				if (Projectile.honeyWet)
				{
					gravity = 0.05f;
					maxFallSpeed = 3f;
				}
				else
				{
					if (Projectile.wet)
					{
						gravity = 0.08f;
						maxFallSpeed = 5f;
					}
				}
			}
			Projectile.velocity.Y = Projectile.velocity.Y + gravity;
			if (Projectile.velocity.Y > maxFallSpeed)
			{
				Projectile.velocity.Y = maxFallSpeed;
			}
			Projectile.velocity.X = Projectile.velocity.X * 0.95f;
        }
		public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
    }
}
