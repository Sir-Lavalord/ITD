using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly
{
    public class LightningStaffStrike : ModProjectile
    {
        public override string Texture => "ITD/Content/Projectiles/BlankTexture";
        public override void SetDefaults()
        {
            Projectile.height = 64; Projectile.width = 64;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if (Main.rand.NextBool())
            {
                for (int i = 0; i < 8; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Electric, (float)Math.Cos(MathHelper.PiOver4 * i) * 6f, (float)Math.Sin(MathHelper.PiOver4 * i) * 6f);
                    d.noGravity = true;
                }
            }
        }
    }
}
