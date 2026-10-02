using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp
{
    public class WispFireburst : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
                Projectile.alpha -= 15;

            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.5f);
            Main.dust[dust].noGravity = true;

            Projectile.rotation += 0.1f;

            if (Projectile.localAI[0]++ == Projectile.ai[0])
            {
                Projectile.velocity = new Vector2(0, -18f);
                Projectile.netUpdate = true;
            }

            if (Projectile.localAI[0] < Projectile.ai[0])
            {
                if (Projectile.Center.Y >= Projectile.ai[1] && Projectile.velocity.Y > 0)
                {
                    Projectile.velocity *= 0.9f;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }
}