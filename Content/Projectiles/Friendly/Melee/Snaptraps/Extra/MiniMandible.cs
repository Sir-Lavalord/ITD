using ITD.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra
{
    public class MiniMandible : ModProjectile
    {
        private int DelayTimer = 0;
        public override void SetStaticDefaults()
        {
            // Total count animation frames
            Main.projFrames[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.damage = 99;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (3);
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
            set
            {
                Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }

        public override void AI()
        {
            float maxDetectRadius = 400f;

            if (DelayTimer < 5)
            {
                DelayTimer += 1;
                return;
            }

            if (Projectile.timeLeft > 20)
            {
                HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

                if (HomingTarget == null)
                {
                    Projectile.Kill();
                    return;
                }
                else
                {
                    //Projectile.alpha = 0;
                }

                Vector2 directionToTarget = HomingTarget.Center - Projectile.Center;
                directionToTarget.Normalize();

                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(6)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);

                Projectile.rotation = directionToTarget.ToRotation();
            }
            else
            {
                Projectile.velocity *= 0f;
                SoundEngine.PlaySound(SoundID.NPCHit31, Projectile.Center);
                Projectile.Kill();
            }

            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
        }
    }
}
       