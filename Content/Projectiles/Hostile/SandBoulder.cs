using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile
{
    public class SandBoulder : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 28; Projectile.height = 28;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 0, 0, 0, 0, default, 1.5f);
            }
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        }

        public override void AI()
        {
			Projectile.velocity += new Vector2(0f, 0.3f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}