using ITD.Utilities;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;
using System;

namespace ITD.Content.NPCs.BlueshroomGroves.Critters
{
    /// <summary>
    /// this is just so i don't have to copy code over
    /// </summary>
    public abstract class GenericSnowpoff : ITDNPC
    {
        private enum ActionState
        {
            Still,
            Wandering,
            JumpPreDig,
            Digging,
            Underground,
            JumpDigOut,
            Ball,
        }
        public ref float AITimer => ref NPC.ai[0];
        public ref float AIState => ref NPC.ai[1];
        public ref float AIRand => ref NPC.ai[2];
        public ref float AIDir => ref NPC.ai[3];
        private ActionState AI_State { get { return (ActionState)AIState; } set { AIState = (float)value; HandleAIChange(value); } }
        public abstract int NormalHeight { get; }
        public abstract int HeightAsBall { get; }
        private bool JustSpawned = true;
        private bool DugOut = true;
        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[Type] = 5;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
            NPCID.Sets.TownCritter[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = HeightAsBall;
            NPC.height = NormalHeight;
            NPC.damage = 0;
            NPC.HitSound = new SoundStyle("ITD/Content/Sounds/NPCSounds/SnowpoffHit")
            {
                PitchVariance = 0.75f
            };
            NPC.DeathSound = new SoundStyle("ITD/Content/Sounds/NPCSounds/SnowpoffDeath")
            {
                PitchVariance = 0.75f
            };
            AI_State = ActionState.Wandering;
            AIDir = 1f;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                // do death effects (snow dust, gores)
                if (Main.dedServ)
                    return;
            }
            else
            {
                if (AI_State == ActionState.Ball)
                {
                    NPC.collideY = false;
                    NPC.velocity.Y -= 1.5f;
                    AITimer = 0f;
                }
                if (!hit.DamageType.CountsAsClass(DamageClass.Magic))
                    switch (AI_State)  // try turn into ball
                    {
                        case ActionState.Wandering:
                        case ActionState.JumpPreDig:
                        case ActionState.JumpDigOut:
                        case ActionState.Still:
                            AI_State = ActionState.Ball;
                            break;
                        default:
                            break;
                    }
            }
        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (modifiers.DamageType == DamageClass.Magic)
                    return;
                modifiers.Defense += 9999;
                modifiers.HideCombatText();
            }
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.life += damageDone;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                if (!hit.DamageType.CountsAsClass(DamageClass.Magic))
                    NPC.life += damageDone;
        }
        public override void FindFrame(int frameHeight) // using this for all relevant visuals
        {
            NPC.spriteDirection = NPC.direction;
            int ballFrame = Main.npcFrameCount[Type] - 1;
            int loopFrame = ballFrame - 1;
            if (NPC.IsOnStandableGround() && AI_State == ActionState.Wandering)
            {
                CommonFrameLoop(frameHeight, 0, loopFrame);
            }
            else if (AI_State == ActionState.Digging) // fast diggy movement
            {
                CommonFrameLoop(frameHeight, 0, loopFrame, 3f);
                NPC.rotation = MathHelper.PiOver2 * NPC.direction;
            }
            else if (AI_State == ActionState.Ball)
            {
                NPC.frame.Y = frameHeight * ballFrame;
            }
            else
            {
                NPC.frame.Y = 0;
            }

            if (AI_State == ActionState.JumpPreDig)
            {
                NPC.rotation = NPC.velocity.ToRotation() * NPC.direction; // multiply by direction to get correct rotation value
            }
        }
        public override void DrawBehind(int index)
        {
            void Behind()
            {
                NPC.hide = true;
                Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            }
            if (AI_State == ActionState.Digging || AI_State == ActionState.Underground)
            {
                Behind();
            }
            else if (AI_State == ActionState.JumpDigOut)
            {
                if (DugOut)
                    NPC.hide = false;
                else
                    Behind();
            }
            else
                NPC.hide = false;
        }
        public override void AI()
        {
            if (!NPC.active)
                return;
            AITimer += 1f;
            if (AITimer > 2)
                JustSpawned = false;
            //Main.NewText(AI_State);
            //Main.NewText(AIRand);
            switch (AI_State)
            {
                case ActionState.Still:
                    BeStill();
                    break;
                case ActionState.Wandering:
                    PassiveWander();
                    break;
                case ActionState.JumpPreDig:
                    JumpPreDig();
                    break;
                case ActionState.Digging:
                    Dig();
                    break;
                case ActionState.Underground:
                    Underground();
                    break;
                case ActionState.JumpDigOut:
                    JumpDigOut();
                    break;
                case ActionState.Ball:
                    BeABall();
                    break;
            }
        }
        public void BeStill()
        {
            if (NPC.IsOnStandableGround())
            {
                NPC.velocity.X *= 0f;
            }
            if (AITimer > AIRand)
            {
                AIRand = Main.rand.Next(80, 260);
                // try to find an appropriate location to jump and dig down, otherwise wander:
                if (JustSpawned) // this variable's whole existence is for this. idk why it instantly jumps if i don't do something like this
                //if (JustSpawned || Main.rand.NextBool())
                {
                    AI_State = ActionState.Wandering;
                    return;
                }
                Vector2 jumpDir = new(AIDir, -2f);

                int rayDist = 12;
                RaycastData first = Helpers.QuickRaycast(NPC.Center, jumpDir, maxDistTiles: rayDist);//, visualize: true);
                if (first.Hit)
                {
                    AI_State = ActionState.Wandering;
                }
                else
                {
                    Vector2 jumpDir2 = new(AIDir, 2f);
                    RaycastData second = Helpers.QuickRaycast(first.End, jumpDir2, maxDistTiles: rayDist - 1);//, visualize: true);
                    if (!second.Hit)
                    {
                        AI_State = ActionState.JumpPreDig;
                        NPC.velocity.Y -= 8f; // make it jump
                        NPC.velocity.X += AIDir * 2f;
                        return;
                    }
                    AI_State = ActionState.Wandering;
                }
            }
        }
        public void PassiveWander()
        {
            if (AITimer > AIRand)
            {
                AIRand = Main.rand.Next(160, 360);
                AI_State = ActionState.Still;
                return;
            }
            NPC.direction = (int)AIDir;
            float wantedX = AIDir * 1.5f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, wantedX, 0.1f);
            StepUp();
            Vector2 tileQuery = NPC.Center + new Vector2((NPC.width / 2f * AIDir) + (0.01f * AIDir), -NPC.height / 2);
            //Dust.NewDustPerfect(tileQuery, DustID.WhiteTorch);
            if (TileHelpers.SolidTile(tileQuery.ToTileCoordinates())) // if we're trying to walk into a wall, reverse the direction
            {
                AIDir *= -1f;
            }
        }
        public void JumpPreDig()
        {
            Rectangle rect = HitboxTiles;
            Point pointUnder = new(rect.X + rect.Width / 2, HitboxTiles.Y + HitboxTiles.Height + 1);
            Tile tUnder = Framing.GetTileSafely(pointUnder);
            bool solidTile = TileHelpers.SolidTile(pointUnder);
            bool solidTopTile = TileHelpers.SolidTopTile(pointUnder);
            bool snowpoffCanDig = ITDSets.SnowpoffDiggable[tUnder.TileType];
            bool goingDown = NPC.velocity.Y >= 0;
            if (!goingDown)
                return;
            if (solidTile && snowpoffCanDig) // this can be dug
            {
                AI_State = ActionState.Digging;
                return;
            }
            else if ((solidTile || solidTopTile) && !snowpoffCanDig)
            {
                AI_State = ActionState.Wandering;
            }
        }
        public void Dig()
        {
            bool collide = TileHelpers.SolidTile(NPC.Center.ToTileCoordinates());
            Collision.HitTiles(NPC.Center, NPC.velocity, 1, 1);
            if (collide)
                NPC.velocity *= 0.7f;
            if (AITimer > 10)
            {
                AI_State = ActionState.Underground;
                AIRand = Main.rand.Next(240, 300);
            }
        }
        public void Underground()
        {
            NPC.velocity *= 0f;
            DugOut = false;
            if (AITimer > AIRand)
            {
                AI_State = ActionState.JumpDigOut;
            }
        }
        public void JumpDigOut()
        {
            bool collide = TileHelpers.SolidTile((NPC.position + new Vector2(NPC.width / 2, NPC.height)).ToTileCoordinates());
            if (!collide)
                DugOut = true;
            else
            {
                NPC.velocity.Y -= 0.5f;
            }
            Collision.HitTiles(NPC.Center, NPC.velocity, 1, 1);
            if (DugOut)
                AI_State = ActionState.Wandering;
        }
        public void BeABall()
        {
            //if (AITimer < 10)
            //    return;
            if (NPC.collideX)
            {
                NPC.velocity.X *= -1f;
            }
            if (NPC.collideY)
            {
                NPC.velocity.X *= 0.98f; // friction
                NPC.velocity.Y *= -1f;
            }
            NPC.rotation += NPC.velocity.X / 32f;
            if (AITimer > 300)
            {
                AI_State = ActionState.Wandering;
                return;
            }
        }
        private void HandleAIChange(ActionState newState)
        {
            //Main.NewText(newState);
            AITimer = 0f;
            if (newState == ActionState.Ball)
            {
                NPC.height = HeightAsBall;
                NPC.position.Y -= HeightAsBall / 2;
                NPC.velocity.Y -= 2f;
            }
            else
            {
                NPC.rotation = 0f;
                NPC.height = NormalHeight;
            }
            if (newState == ActionState.Digging || newState == ActionState.Underground || newState == ActionState.JumpDigOut)
            {
                NPC.noTileCollide = true;
                if (newState == ActionState.Underground)
                {
                    NPC.noGravity = true;
                }
            }
            else
            {
                NPC.noGravity = false;
                NPC.noTileCollide = false;
            }
            switch (newState)
            {
                case ActionState.Wandering:
                    AIDir = Main.rand.NextBool() ? 1 : -1;
                    break;
            }
            NPC.netUpdate = true;
        }
    }
}
