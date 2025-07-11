namespace ITD.Content.Projectiles.Hostile
{
    public class Sporeflake : ModProjectile
    {		
        public override void SetDefaults()
        {
            Projectile.width = 40; Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
        }
		public override Color? GetAlpha(Color lightColor)
        {
			Color color = new Color(255, 255, 255, 100);
            return color * Projectile.Opacity;
        }
        public override void AI()
        {
			Projectile.velocity *= 0.95f;
            Projectile.rotation += Projectile.velocity.X * 0.1f;
			Projectile.Opacity -= 0.02f;
        }
    }
}