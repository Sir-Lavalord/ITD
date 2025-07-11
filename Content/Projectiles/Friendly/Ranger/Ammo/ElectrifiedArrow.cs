using Terraria.Audio;

using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
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

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.myPlayer == Projectile.owner)
			{
				NPC newTarget = null;
				float reach = 600;
				
				foreach (var npc in Main.ActiveNPCs)
				{
					if (!npc.friendly && npc.CanBeChasedBy() && npc != target)
					{
						float distance = Vector2.Distance(npc.Center, target.Center);
						if (distance < reach)
						{
							reach = distance;
							newTarget = npc;
						}
					}
				}
				if (newTarget != null)
				{
					Projectile newZap = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), newTarget.Center, new Vector2(), ModContent.ProjectileType<Zap>(), (int)(Projectile.damage * 0.75f), 0, Projectile.owner, newTarget.whoAmI, target.Center.X, target.Center.Y)];
					newZap.localAI[1] = 1;
					newZap.localNPCImmunity[target.whoAmI] = -1;
				}
			}
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
