using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.Gravekeeper
{
    public class GravekeeperCloneTeleport : ModProjectile
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
			if (Projectile.ai[0] > 0f)
			{
				Projectile.ai[1]++;
				Projectile.Opacity -= 0.025f;
			}
			else if (Projectile.Opacity < 1)
			{
				Projectile.Opacity += 0.025f;
			}
			if (Projectile.ai[1] > 40f)
				Projectile.Kill();
        }
		
		public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2;
			
			for (float i = 0f; i < 1f; i += 0.25f)
            {
				float radians = (i + Main.GlobalTimeWrappedHourly) * MathHelper.TwoPi;
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 6f + Projectile.ai[1]).RotatedBy(radians), null, new Color(200, 255, 255, 0) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
