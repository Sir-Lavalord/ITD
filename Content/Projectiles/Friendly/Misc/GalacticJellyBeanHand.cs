using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class GalacticJellyBeanHand : ModProjectile
    {
        private int targetNPC;
        private int slapCooldown;
        private int returnDelay = 20;
        private ref float handWindUp => ref Projectile.ai[0];
        private ref float handSling => ref Projectile.ai[1];
        private ref float handReturnIntensity => ref Projectile.ai[2];
        private enum HandState
        {
            OnPlayer,
            WindUp,
            Slap,
            Returning,
        }
        private HandState handState;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.height = 32; Projectile.width = 32;
            Projectile.damage = 75;
            Projectile.ignoreWater = true;
            handState = HandState.OnPlayer;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            float maxDetectRadius = 600f;
            NPC target = null;
            if (targetNPC > -1)
            {
                target = Main.npc[targetNPC];
            }
            //Main.NewText(handState);
            switch (handState)
            {
                case HandState.OnPlayer:
                    handWindUp = 0f;
                    handSling = 0f;
                    handReturnIntensity = 0f;
                    Player owner = Main.player[Projectile.owner];
                    Projectile.spriteDirection = Projectile.direction = owner.direction;
                    Vector2 offset = new Vector2(owner.direction == -1 ? 18 : -14, -32f + owner.gfxOffY);
                    Projectile.velocity = Vector2.Zero;
                    Projectile.Center = owner.Center + offset;// + owner.velocity;
                    if (++Projectile.frameCounter >= 5)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame = ++Projectile.frame % (Main.projFrames[Type] - 1);
                    }
                    if (--slapCooldown <= 0)
                    {
                        if (targetNPC == -1)
                        {
                            targetNPC = Projectile.FindClosestNPC(maxDetectRadius);
                        }
                        if (targetNPC != -1)
                        {
                            target = Main.npc[targetNPC];
                            slapCooldown = 60;
                            handState = HandState.WindUp;
                        }
                    }
                    break;
                case HandState.WindUp:
                    Projectile.frame = Main.projFrames[Type] - 1;
                    if (target != null)
                    {
                        if (!target.active)
                        {
                            handState = HandState.Returning;
                            target = null;
                            targetNPC = -1;
                        }
                        else
                        {
                            float dist = 128f/16f;
                            Vector2 fromTarget = (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero) * dist;
                            float mult = (float)Math.Sin(handWindUp * Math.PI);
                            Projectile.velocity = fromTarget * mult;
                            handWindUp += 0.05f;
                            if (handWindUp >= 1f)
                            {
                                handState = HandState.Slap;
                                handWindUp = 0f;
                            }
                        }
                    }
                    else
                    {
                        handState = HandState.Returning;
                    }
                    break;
                case HandState.Slap:
                    if (target != null)
                    {
                        if (!target.active)
                        {
                            handState = HandState.Returning;
                            target = null;
                            targetNPC = -1;
                        }
                        else
                        {
                            Vector2 toTarget = (target.Center - Projectile.Center);
                            float dist = (toTarget.Length() + 32f)/16f;
                            Vector2 norm2 = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * dist;
                            if (norm2 == Vector2.Zero)
                            {
                                Main.NewText("a");
                                handState = HandState.Returning;
                            }
                            else
                            {
                                Projectile.velocity = norm2 * (float)Math.Sin(handSling * Math.PI);
                                handSling += 0.05f;
                                if (handSling >= 1f)
                                {
                                    handState = HandState.Returning;
                                    handSling = 0f;
                                }
                            }
                        }
                    }
                    else
                    {
                        handState = HandState.Returning;
                    }
                    break;
                case HandState.Returning:
                    targetNPC = -1;
                    Player owner2 = Main.player[Projectile.owner];
                    Vector2 offset2 = new Vector2(owner2.direction == -1 ? 18 : -14, -32f + owner2.gfxOffY);
                    Vector2 toPlayer = (owner2.Center + offset2) - Projectile.Center;
                    //Main.NewText("Distance to player: " + toPlayer.Length());
                    Vector2 norm = toPlayer.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = norm * handReturnIntensity;
                    handReturnIntensity += 0.1f;
                    if (toPlayer.Length() < 8f)
                    {
                        returnDelay = 20;
                        handReturnIntensity = 0f;
                        handState = HandState.OnPlayer;
                    }
                    break;
                default:
                    break;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ScalingArmorPenetration += 1f;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return target.whoAmI == targetNPC && handState == HandState.Slap;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            handState = HandState.Returning;
        }
    }
}
