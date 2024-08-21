using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ITD.Content.NPCs;
using ITD.Utils;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class RhodiumSwordBeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 16; Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 60;
			Projectile.extraUpdates = 11;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.GetGlobalNPC<ITDGlobalNPC>().zapped = true;
			MiscHelpers.Zap(target.Center, Main.player[Projectile.owner], (int)(Projectile.damage * 0.75f), Projectile.CritChance, 2);
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
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1.5f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.Item94, Projectile.position);
        }
    }
}
