using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ITD.Content.NPCs;
using ITD.Utilities;

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
			target.GetGlobalNPC<ITDGlobalNPC>().zapped = true;
			MiscHelpers.Zap(target.Center, Main.player[Projectile.owner], (int)(Projectile.damage * 0.75f), Projectile.CritChance, 1);
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
