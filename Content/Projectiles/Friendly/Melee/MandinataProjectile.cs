namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class MandinataProjectile : ModProjectile
    {
        protected virtual float HoldoutRangeMin => 24f;
        protected virtual float HoldoutRangeMax => 80f;
		private static int Lifespan = 25;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = Lifespan;
        }

        private Vector2 Holdout(Vector2 direction, int time)
		{
			Player player = Main.player[Projectile.owner];

			float stoppingPoint = Lifespan * 0.33f;
			float progress;
			if (time < stoppingPoint)
			{
				progress = time / stoppingPoint;
			}
			else
			{
				progress = (Lifespan - time) / stoppingPoint;
			}
			
			return player.MountedCenter + Vector2.SmoothStep(direction * HoldoutRangeMin, direction * HoldoutRangeMax, progress);
		}

		public override bool PreAI()
		{
			Player player = Main.player[Projectile.owner];
			player.heldProj = Projectile.whoAmI;

			if ((Projectile.timeLeft == 15 || Projectile.timeLeft == 20 || Projectile.timeLeft == 25) && Main.myPlayer == Projectile.owner)
			{
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), player.Center, Projectile.velocity * Projectile.timeLeft * 0.25f, ModContent.ProjectileType<MandinataBreath>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack * 0.5f, Projectile.owner);
			}
			
			Vector2 direction = Vector2.Normalize(Projectile.velocity);

			Projectile.Center = Holdout(direction, Projectile.timeLeft);

			return false;
		}
    }
}