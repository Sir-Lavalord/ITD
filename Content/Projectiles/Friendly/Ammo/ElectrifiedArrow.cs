using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Ammo
{
    public class ElectrifiedArrow : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(1);
        }
		public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

		public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[2]++;
            if (Projectile.ai[2] > 1)
            {
                Projectile.Kill();
            }

            else
            {
				for (int i = 0; i < 5; i++)
				{
					int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 2f;
				}
				SoundEngine.PlaySound(SoundID.Item94, Projectile.position);

                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    Projectile.velocity.X = -oldVelocity.X;
                }

                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
            }

            return false;
        }
		
        public override void AI()
        {
            int dust = Dust.NewDust(Projectile.position, 10, 1, DustID.Electric, 0f, 0f, 0, default(Color), 1f);
			Main.dust[dust].noGravity = true;
        }
		
		public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.Item94, Projectile.position);
        }
    }
}
