using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using ITD.Content.Items.Accessories.Expert;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class GalacticJellyBeanHand : ModProjectile
    {
        private NPC HomingTarget
        {
            get => Projectile.ai[2] == -1 ? null : Main.npc[(int)Projectile.ai[2]];
            set
            {
                Projectile.ai[2] = value == null ? -1 : value.whoAmI;
            }
        }
        public float rotation = 0f;
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        private enum HandState
        {
            Default,
            Charging,
            Slinging,
        }

        private HandState handState = HandState.Default;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.height = 32; Projectile.width = 32;
            Projectile.damage = 50;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        private Vector2 handTarget = Vector2.Zero;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player))
            {
                return;
            }
            if (HomingTarget != null)
            {
                if (Projectile.ai[0]++ >= 240)
                {
                    Projectile.ai[0] = 0;
                    if (handState == HandState.Default)
                        handState = HandState.Charging;
                }
            }
            if (HomingTarget == null)
            {
                HomingTarget = Projectile.FindClosestNPCDirect(300);
            }

            if (HomingTarget != null && !Projectile.IsValidTarget(HomingTarget))
            {
                HomingTarget = null;
            }
                Target();

        }
        Vector2 toTarget;
        Vector2 chargedPosition;
        public void Target()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 offset = new Vector2(player.direction == -1 ? 18 : -14, -32f + player.gfxOffY);
            Vector2 normalCenter = player.Center + offset + new Vector2(0f, player.velocity.Y);
            if (HomingTarget != null)
            {
                toTarget = (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                chargedPosition = player.Center + offset + new Vector2(0f, player.velocity.Y) - toTarget * 150f;
            }
            switch (handState)
            {
                case HandState.Default:
                    if (++Projectile.frameCounter >= 5)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame = ++Projectile.frame % (Main.projFrames[Type] - 1);
                    }
                    Projectile.spriteDirection = Projectile.direction = player.direction;
                    handSling = 0f;
                    handCharge = 0f;
                    handFollowThrough = 0f;
                    Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                    break;
                case HandState.Charging:
                    Projectile.frame = Main.projFrames[Type] - 1;

                    if (handCharge < 0.6f)
                    {
                        handCharge += 0.04f;
                    }
                    else
                    {
                        handState = HandState.Slinging;
                        handTarget = HomingTarget.Center + toTarget * 120f;

                    }
                    Projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                    break;
                case HandState.Slinging:
                    handCharge = 0f;
                    if (handSling < 0.8f)
                    {
                        handSling += 0.03f;
                    }
                    else
                    {
                        if (handFollowThrough < 0.8f)
                        {
                            handFollowThrough += 0.1f;
                        }
                        else
                        {
                            handState = HandState.Default;
                        }
                    }
                    Projectile.Center = Vector2.Lerp(normalCenter, handTarget, (float)Math.Sin(handSling * Math.PI));
                    break;
            }
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.GetModPlayer<CosmicHandMinionPlayer>().Active = false;

                return false;
            }

            if (owner.GetModPlayer<CosmicHandMinionPlayer>().Active)
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ScalingArmorPenetration += 1f;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return handState == HandState.Slinging;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
