using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.NPCs.Events.LavaRain
{
    public class Flamgoustine : ITDNPC
    {
        public enum ActionState
        {
            Idle,
            ChargingSpin,
            Spinning,
            StoppingSpin
        }
        public ActionState AIState { get { return (ActionState)NPC.ai[0]; } set { NPC.ai[0] = (float)value; } }
        public ref float AITimer => ref NPC.ai[1];
        public ref float AIRand => ref NPC.ai[2];
        public ref float AIDir => ref NPC.ai[3];
        public ref float TrailFadeIn => ref NPC.localAI[0];
        private static readonly VertexStrip vertexStrip = new();
        public override void SetStaticDefaultsSafe()
        {
            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = NPCTrailingID.PosEveryFrame;
            Main.npcFrameCount[Type] = 8;
            ITDSets.LavaRainEnemy[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.lifeMax = 70;
            NPC.damage = 20;
            NPC.defense = 12;
            NPC.width = NPC.height = 28;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit32;
            NPC.DeathSound = SoundID.NPCDeath11;
            NPC.lavaImmune = true;
        }
        public override void AI()
        {
            NPC.spriteDirection = NPC.direction;
            if (AIState != ActionState.StoppingSpin)
                AITimer++;
            if (AITimer > AIRand)
            {
                AIState++;
                AITimer = 0;
            }
            // thank you taco
            AIState = AIState switch
            {
                ActionState.Idle => Idle(),
                ActionState.ChargingSpin => ChargingSpin(),
                ActionState.Spinning => Spinning(),
                ActionState.StoppingSpin => StoppingSpin(),
                _ => AIState
            };
        }
        private ActionState Idle()
        {
            if (NPC.rotation != 0f)
                NPC.rotation *= 0.9f;
            if (NPC.collideY)
                NPC.velocity.X *= 0.75f;
            if (InvalidTarget)
            {
                NPC.TargetClosest(true);
            }
            NPC.direction = Math.Sign(Main.player[NPC.target].position.X - NPC.position.X);
            AIDir = NPC.direction;
            return ActionState.Idle;
        }
        private ActionState ChargingSpin()
        {
            NPC.velocity.X = 0f;
            if (!Main.dedServ)
            {
                if (NPC.frame.Y / NPC.frame.Height >= 6)
                {
                    NPC.rotation = MathHelper.WrapAngle(NPC.rotation + AIDir / 4f);
                    if (Main.rand.NextBool(6))
                    {
                        Vector2 spawnPosition = NPC.position + new Vector2(-AIDir * 12f, 12f);
                        Gore.NewGore(NPC.GetSource_FromThis(), spawnPosition, -Vector2.UnitX * AIDir, Main.rand.Next(61, 64), 0.5f);
                    }
                }
            }
            return ActionState.ChargingSpin;
        }
        private ActionState Spinning()
        {
            if (TrailFadeIn < 1f)
                TrailFadeIn += 0.1f;
            NPC.rotation = MathHelper.WrapAngle(NPC.rotation + AIDir / 2.5f);
            NPC.velocity.X = AIDir * 6f;
            int dustTimes = 2;
            for (int i = 0; i < dustTimes; i++)
            {
                Dust d = Dust.NewDustPerfect(NPC.Center + (Vector2.UnitX.RotatedBy(MathF.Tau / dustTimes * i).RotatedBy(NPC.rotation) * 14f), DustID.Torch, NPC.velocity, Scale: 1.2f);
                d.noGravity = true;
            }
            StepUp();
            if (NPC.collideX)
            {
                // if despite stepup, the shrimp has collided, that means we actually got in contact with a wall, so force the next state and knock it back a bit
                Collision.HitTiles(NPC.position, NPC.velocity, NPC.width, NPC.height - 1);
                SoundEngine.PlaySound(NPC.HitSound, NPC.Center);
                SoundEngine.PlaySound(SoundID.Dig, NPC.Center);
                NPC.velocity.Y = -6f;
                NPC.velocity.X -= AIDir * 8f;
                AITimer = 0;
                return ActionState.StoppingSpin;
            }
            return ActionState.Spinning;
        }
        private ActionState StoppingSpin()
        {
            if (TrailFadeIn > 0f)
                TrailFadeIn -= 0.1f;
            NPC.rotation *= 0.9f;
            if (NPC.collideY)
            {
                if (Main.rand.NextFloat() < Math.Abs(NPC.velocity.X) && Main.rand.NextBool(4))
                    Collision.HitTiles(NPC.Center, NPC.velocity, 1, NPC.height);
                NPC.velocity.X *= 0.96f;
                if (NPC.velocity.X == 0f)
                {
                    AIRand = Main.rand.Next(120, 160);
                    NPC.netUpdate = true;
                    return ActionState.Idle;
                }
            }
            return ActionState.StoppingSpin;
        }
        public override bool? CanFallThroughPlatforms()
        {
            return !InvalidTarget && Main.player[NPC.target].position.Y > NPC.position.Y;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(BuffID.OnFire, AIState == ActionState.Spinning ? 30 : 10);
        }
        public override void FindFrame(int frameHeight)
        {
            switch (AIState)
            {
                case ActionState.Idle:
                    CommonFrameLoop(frameHeight, 0, 3);
                    break;
                case ActionState.ChargingSpin:
                    if (NPC.frame.Y < frameHeight * 6)
                        CommonFrameLoop(frameHeight, 4, 6, 7);
                    else
                        CommonFrameLoop(frameHeight, 6);
                    break;
                case ActionState.Spinning:
                    CommonFrameLoop(frameHeight, 6);
                    break;
                case ActionState.StoppingSpin:
                    CommonFrameLoop(frameHeight, 4, 5);
                    break;
            }
        }
        private Color StripColors(float progressOnStrip)
        {
            Color c = Color.Lerp(Color.Orange, Color.Red * 0.5f, progressOnStrip);
            c.A = 0;
            return c * TrailFadeIn;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (TrailFadeIn > 0f)
            {
                GameShaders.Misc["LightDisc"].Apply(null);
                Vector2 offset = NPC.Size * 0.5f + new Vector2(0f, 2f);
                vertexStrip.PrepareStrip(NPC.oldPos, NPC.oldRot, StripColors, p => 8f, offset - screenPos, NPC.oldPos.Length, true);
                vertexStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }
            return true;
        }
    }
}
