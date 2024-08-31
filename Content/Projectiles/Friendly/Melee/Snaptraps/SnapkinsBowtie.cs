using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra
{
    public class SnapkinsBowtie : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 12; Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 127;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            DrawOriginOffsetY = -4;
            DrawOffsetX = -4;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float progressOneToZero = (Projectile.timeLeft * 2)/255f;
            float hue = (1 - progressOneToZero) * 360f;
            Color color = Helpers.ColorFromHSV(hue, 1f, 1f);
            //Main.NewText(color.R.ToString() + " " + color.G.ToString() + " " + color.B.ToString());
            return color * progressOneToZero;
        }
        public override void AI()
        {
            float accel = 1.02f;
            Projectile.velocity *= accel;
            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        }
    }
}
