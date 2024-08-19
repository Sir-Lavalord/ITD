using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class GhostlyBlade : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 60; Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 20;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
        }
		
		public override Color? GetAlpha(Color lightColor)
        {
			Color color = new Color(200, 200, 200, 100);
            return color;
        }

        public override void AI()
        {
			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 180, 0, 0, 100, default, 1.5f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity = Projectile.velocity/5f;
			
			if (Projectile.timeLeft > 15)
				Projectile.velocity.Y -= 2;
			else
				Projectile.velocity.Y += 2;
			Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();
			if (Projectile.spriteDirection == 1)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2*2;
			else
				Projectile.rotation = Projectile.velocity.ToRotation();
        }
		
		public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, 180, 0f, 0f, 0, default, 1.5f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 4f;
            }
            SoundEngine.PlaySound(SoundID.NPCHit54, Projectile.position);
        }
    }
}
