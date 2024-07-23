using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Snaptraps.Extra
{
    public class EvilSpitProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 8; Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override string Texture => "ITD/Content/Projectiles/BlankTexture";
        public override void AI()
        {
            Dust.NewDust(Projectile.position, 8, 8, Projectile.ai[0] == 0f ? DustID.ScourgeOfTheCorruptor : DustID.Crimslime);
            Projectile.velocity.Y += 0.15f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);

            // If the projectile hits the left or right side of the tile, reverse the X velocity
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }

            // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Dust.NewDust(Projectile.Center, 1, 1, Projectile.ai[0] == 0f ? DustID.ScourgeOfTheCorruptor : DustID.Crimslime, Projectile.velocity.X, Projectile.velocity.Y);
            Projectile.Kill();
            return false;
        }
    }
}
