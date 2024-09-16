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
    public class ShadowKnife : ModProjectile
    {
        public ref float PierceProgress => ref Projectile.ai[1];
        bool piercing = false;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 800;
            DrawOffsetX = 6;
        }
        private void SpawnDust()
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Shadowflame);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SpawnDust();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            int count = player.ownedProjectileCounts[Type];
            float radians = MathHelper.TwoPi / count;
            Projectile.Center = player.Center + new Vector2(0f, player.gfxOffY);
            NPC closest = Projectile.FindClosestNPC(256f);
            Vector2 toClosest = Vector2.Zero;
            if (closest != null)
            {
                toClosest = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                bool isInAngleRange = Math.Abs(Projectile.AngleTo(closest.Center) - Projectile.velocity.ToRotation()) < MathHelper.PiOver4;
                if (isInAngleRange)
                {
                    piercing = true;
                }
            }

            Vector2 restingVelocity = (-Vector2.UnitY * (piercing? 64f + (float)Math.Sin(PierceProgress * (float)Math.PI) * 128f: 64f)).RotatedBy((radians * Projectile.ai[0]) + Main.GlobalTimeWrappedHourly * 4f);
            if (piercing)
            {
                PierceProgress += 0.05f;
                if (PierceProgress > 1f)
                {
                    piercing = false;
                    PierceProgress = 0f;
                }
            }
            Projectile.velocity = restingVelocity;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}
