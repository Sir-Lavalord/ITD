using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Utilities;
using System;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class BloodPactCut : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;

            Projectile.penetrate = -1;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
        }
    }
}
           