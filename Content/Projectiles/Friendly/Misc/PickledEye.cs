using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class PickledEye : ModProjectile
    {		
        public override void SetDefaults()
        {
            Projectile.width = 28; Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
				if (oldVelocity.X != Projectile.velocity.X) {
			Projectile.velocity.X = (0f - oldVelocity.X);
			}
			if (oldVelocity.Y != Projectile.velocity.Y) {
				Projectile.velocity.Y = (0f - oldVelocity.Y);
			}
			
			Projectile.velocity *= 0.9f;
			
			for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, 0, 0, 0, default, 2f);
				dust.velocity *= 1.5f;
				dust.noGravity = true;
            }
			SoundEngine.PlaySound(SoundID.Item154, Projectile.Center);
						
			return Projectile.velocity.Length() < 3f;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, 0, 0, 0, default, 2f);
				dust.velocity *= 1.5f;
				dust.noGravity = true;
            }
			SoundEngine.PlaySound(SoundID.Item154, Projectile.Center);
        }

        public override void AI()
        {
			if (Projectile.localAI[0] == 0f)
			{
				Projectile.localAI[0] = 1f;
				for (int i = 0; i < 3; i++)
				{
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, 0, 0, 0, default, 2f);
					dust.velocity = Projectile.velocity * 0.5f;
					dust.noGravity = true;
				}
			}
			
			Projectile.velocity += new Vector2(0f, 0.3f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
		
		public override bool PreDraw(ref Color lightColor) {
			Vector2 position = Projectile.Center - Main.screenPosition;
			
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			Vector2 origin = sourceRectangle.Size() / 2f;
			
			Main.EntitySpriteDraw(texture, position, sourceRectangle, lightColor, Projectile.rotation, origin, new Vector2(1f, 1f + (Projectile.velocity.Length() * 0.05f)), SpriteEffects.None, 0f);
			
			return false;
		}
    }
}