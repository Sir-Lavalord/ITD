using ITD.Particles.CosJel;
using ITD.Particles;
using ITD.Players;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class WRipperDash : ModProjectile
    {
		public override string Texture => ITD.BlankTexture;
		public ParticleEmitter emitter;
		
		public const float speed = 12f;
		
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
			
			emitter = ParticleSystem.NewEmitter<SpaceMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = Projectile;
        }

		public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255);
        }

        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
			ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();

			if (modPlayer.dashTime == 0)
				Projectile.Kill();

			if (emitter != null)
                emitter.keptAlive = true;
			
			Projectile.Center = player.Center;
			
			for (int j = 0; j < 3; j++) {
				emitter?.Emit(Projectile.Center + Main.rand.NextVector2Square(-16f, 16f), player.velocity*0.25f, 0f, 20);
			}
        }
    }
}