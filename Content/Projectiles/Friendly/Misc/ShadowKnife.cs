using ITD.Utilities;
using System;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class ShadowKnife : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 800;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.minion = true; // can't use proj.minionPos without this
            DrawOffsetX = 6;
        }
        private void SpawnDust()
        {
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrystalPulse2, 0, 0f, 40, default, 1.5f);
				d.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SpawnDust();
        }
		public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead)
                Projectile.Kill();
			
            int count = 0;
			
			foreach (var target in Main.ActiveProjectiles)
            {
				if (target.type == Type && target.owner == Projectile.owner)
				{
					count++; // i wonder why player.ownedprojectilecounts doesn't work here
				}
			}

            Projectile.Center = player.Center + new Vector2(0f, player.gfxOffY);

            float spinSpeed = 2f;
			float rotation = Main.GlobalTimeWrappedHourly * spinSpeed + (Projectile.minionPos / (float)count) * MathHelper.TwoPi;
			float range = 64f;

			NPC closest = Projectile.FindClosestNPC(256f);
            if (closest != null)
            {
				range = Math.Max((player.Center - closest.Center).Length(), 64f);
            }
			Vector2 restingVelocity = (-Vector2.UnitY * range).RotatedBy(rotation);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, restingVelocity, 0.25f);
            Projectile.rotation = rotation;
        }
    }
}
