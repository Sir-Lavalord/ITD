using Terraria.GameContent;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Hostile.Gravekeeper
{
    public class GravekeeperCloneSkulls : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.Opacity = 0;
        }
		
		public override void AI()
        {
			if (Projectile.ai[1] > 0f)
			{
				Projectile.ai[2]++;
				Projectile.Opacity -= 0.025f;
				Projectile.velocity *= 0.9f;
			}
			else
			{
				Projectile.velocity.Y += 1f;
				if (Projectile.Opacity < 1)
				{
					Projectile.Opacity += 0.025f;
				}
				if (Projectile.position.Y >= Projectile.ai[0])
				{
					Projectile.ai[1] = 1f;
					Projectile.velocity.Y = -8f;
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						int type = ModContent.ProjectileType<NecroSkull>();
						int damage = 20;
						int skullCount = 2;
						if (Main.expertMode)
							skullCount += 1;
						for (int l = 0; l < skullCount; l++)
						{
							float offset = Main.rand.NextFloat(-300f, 300f);
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(offset, 0f), new Vector2(offset*0.01f, -2f), type, damage, 0, -1);
						}		
					}
					SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
					int dustType = DustID.GiantCursedSkullBolt;
					for (int l = 0; l < 8; l++)
					{
						int spawnDust = Dust.NewDust(Projectile.Center, 0, 0, dustType, 0, 0, 0, default, 2f);
						Main.dust[spawnDust].noGravity = true;
						Main.dust[spawnDust].velocity *= 3f;
					}
				}
			}
			if (Projectile.ai[2] > 40f)
				Projectile.Kill();
        }
		
		public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2;
			
			for (float i = 0f; i < 1f; i += 0.25f)
            {
				float radians = (i + Main.GlobalTimeWrappedHourly) * MathHelper.TwoPi;
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 6f + Projectile.ai[2]).RotatedBy(radians), null, new Color(200, 255, 255, 0) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
