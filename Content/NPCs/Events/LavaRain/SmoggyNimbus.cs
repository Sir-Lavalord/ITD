using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Events.LavaRain
{
    public class SmoggyNimbus : ITDNPC
    {
        public enum ActionState
        {
            Following,
            Spinning
        }
        public ref float AITimer => ref NPC.ai[0];
        public ref float AILockOnPeriod => ref NPC.ai[1];
        public ref float AIRand => ref NPC.ai[2];
        public ActionState AIState { get { return (ActionState)NPC.ai[3]; } set { NPC.ai[3] = (float)value; } }
        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[Type] = 1;
            ITDSets.LavaRainEnemy[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.damage = 30;
            NPC.width = 50;
            NPC.height = 40;
            NPC.defense = 20;
            NPC.lifeMax = 100;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCDeath33;
            NPC.noGravity = true;
            NPC.lavaImmune = true;
        }
        public override void AI()
        {
            Dust d = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 1, DustID.Torch, SpeedY: 5f);
            d.noGravity = true;
            if (InvalidTarget)
                NPC.TargetClosest(false);
            NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0 ? -1 : 1;
            // angry nimbus AI seems to be a simple velocity change so it arrives right over the player. if overshot, it recalculates the speed again.
            // if it's x is within range of the player, it starts shooting the rain projectiles.
            // we can do something similar except add a little spice to it, maybe add some extra attacks
            Player target = Main.player[NPC.target];
            Vector2 targetPosition = target.Center - Vector2.UnitY * 216f;
            Vector2 toTargetPosition = targetPosition - NPC.Center;
            Vector2 toTargetPositionNormalized = toTargetPosition.SafeNormalize(Vector2.Zero);
            if (AIState == ActionState.Following)
            {
                // changing the x velo will be a simple smoothstep. for changing y, let's make it a fixed speed so melee users can actually do stuff against the enemy.
                NPC.velocity.X = MathHelper.Clamp(MathHelper.SmoothStep(NPC.velocity.X, toTargetPosition.X, 0.04f), -6f, 6f);
                NPC.velocity.Y = toTargetPositionNormalized.Y * 4f;
                // attacks
                float range = 50f;
                float xRemapped = NPC.Center.X - target.Center.X;
                if (Math.Abs(xRemapped) < range && NPC.position.Y < target.position.Y)
                {
                    AILockOnPeriod = 60;
                }
            }
            else
            {
                int attackLength = 60;
                int attackFreq = 10;
                AITimer++;
                NPC.rotation = Helpers.Remap(AITimer, 0, attackLength, 0, MathHelper.TwoPi);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (AITimer % attackFreq == 0)
                    {
                        Vector2 velo = (NPC.rotation + MathHelper.PiOver2).ToRotationVector2();
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velo * 8.5f, ModContent.ProjectileType<SmoggyNimbusRaindrop>(), NPC.damage, 0f, ai0: Main.rand.Next(3));
                    }
                }
                if (AITimer >= attackLength)
                {
                    AIState = ActionState.Following;
                    AIRand = Main.rand.Next(60, 80);
                    NPC.netUpdate = true;
                }
            }
            if (AILockOnPeriod > 0)
            {
                AILockOnPeriod--;
                if (++AITimer > AIRand)
                {
                    AITimer = 0;
                    AIRand = Main.rand.Next(16, 28);
                    NPC.netUpdate = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 projPos = Vector2.Lerp(NPC.BottomLeft, NPC.BottomRight, Main.rand.NextFloat());
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), projPos, (Vector2.UnitY * 8.5f) + new Vector2(target.velocity.X, 0f), ModContent.ProjectileType<SmoggyNimbusRaindrop>(), NPC.damage, 0f, ai0: Main.rand.Next(3));
                    }
                }
            }
            else if (AIState == ActionState.Following)
            {
                if (--AITimer < -AIRand)
                {
                    AITimer = 0;
                    NPC.netUpdate = true;
                    AIState = ActionState.Spinning;
                    NPC.velocity *= 0f;
                }
            }
        }
        public override bool? CanFallThroughPlatforms() => true;
    }
}
