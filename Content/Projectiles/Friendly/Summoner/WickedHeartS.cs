using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class WickedHeartS : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.timeLeft = 42;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
        }
		
		public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 2.5f;
            Projectile.alpha = 0;
        }

		public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha += 30;
                if (Projectile.alpha > 250)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.Size *= 0.98f;
                Projectile.alpha -= 15;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.MaceWhipNPCDebuff, 300);
        }
    }
}
