using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ITD.Particles.CosJel;
using ITD.Particles;
using System.Runtime.CompilerServices;
using ITD.Utilities.EntityAnim;
using System.Diagnostics;
using ITD.Content.Projectiles.Hostile;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.DataStructures;
namespace ITD.Content.NPCs.Bosses
{
    public class CosmicJellyfishHand : ModNPC
    {
        // sorry mirrorman but my ahh isn't gonna keep reading NPC.ai[] everywhere whilst having no idea wtf is happening, have some ref properties and enums :pray:
        private enum ActionState
        {
            Waiting,
            Charging,
            Clapping,
            Slinging,
            DownToSize,
            MeteorStrike,
            TemperTantrum,
            ForceKill//it's not just downtosize trust me
        }
        private ActionState AIState { get { return (ActionState)NPC.ai[0]; } set { NPC.ai[0] = (float)value; } }
        public bool IsLeftHand => (int)NPC.ai[3] == 1;
        public int CosJelIndex => (int)NPC.ai[2];
        private ActionState UpcomingAttack { get { return (ActionState)NPC.ai[1]; } set { NPC.ai[1] = (float)value; } }
        public ParticleEmitter emitter;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 130;
            NPC.lifeMax = 6000;
            NPC.HitSound = SoundID.NPCDeath6;
            NPC.DeathSound = new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelHandDeath");
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.trapImmune = true;
            emitter = ParticleSystem.NewEmitter<SpaceMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = NPC;
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            //NPC.damage = (int)(NPC.damage * 0.5f);
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override bool CanHitPlayer(Player target, ref int CooldownSlot)
        {
            CooldownSlot = ImmunityCooldownID.Bosses;
            return AIState != ActionState.Waiting;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(bStopfalling);
            writer.Write(Timer);
            writer.Write(handCharge);
            writer.Write(handSling);
            writer.Write(handFollowThrough);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            bStopfalling = reader.ReadBoolean();
            Timer = reader.ReadInt32();
            handCharge = reader.ReadSingle();
            handSling = reader.ReadSingle();
            handFollowThrough = reader.ReadSingle();
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC body = MiscHelpers.NPCExists(CosJelIndex, ModContent.NPCType<CosmicJellyfish>());
            if (body == null)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
                return;
            }
            NPC.target = body.target;
            bool bSecondStage = body.localAI[2] != 0;
            NPC.life = NPC.lifeMax;

            Player player = Main.player[body.target];
            if (UpcomingAttack == ActionState.Clapping)
            {
                chargePos = player.Center;
            }
            else chargePos = player.Center;
        }
        public int iDisFromBoss = 160;
        public int Timer;
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        public bool bStopfalling;
        private Vector2 handTarget = Vector2.Zero;
        private Vector2 chargePos = Vector2.Zero;
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        
        public override void AI()
        {
            NPC body = MiscHelpers.NPCExists(CosJelIndex, ModContent.NPCType<CosmicJellyfish>());
            if (body == null)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
                return;
            }
            NPC.target = body.target;
            bool bSecondStage = body.localAI[2] != 0;
            NPC.life = NPC.lifeMax;

            Player player = Main.player[body.target];
            if (Main.zenithWorld)
            {
                NPC.dontTakeDamage = false;//little surprise in gfb
            }
            else
            {
                NPC.dontTakeDamage = true;
            }
            NPC.direction = NPC.spriteDirection = -(int)NPC.ai[3];
            if (NPC.scale < 1f)
            {
                NPC.scale += 0.05f;
            }
            Vector2 extendOut = new Vector2((float)Math.Cos(body.rotation), (float)Math.Sin(body.rotation));
            Vector2 toTarget = (handTarget - body.Center).SafeNormalize(Vector2.Zero);
            Vector2 chargedPosition = body.Center - extendOut * (160 * (int)NPC.ai[3]) + new Vector2(0f, body.velocity.Y) - toTarget * 150f;
            Vector2 normalCenter = body.Center - extendOut * (160 * (int)NPC.ai[3]) + new Vector2(0f, body.velocity.Y);
            float targetRotation = body.rotation;
            switch (AIState)
            {
                case ActionState.Waiting:
                    bStopfalling = false;
                    Timer = 0;
                    handSling = 0f;
                    handCharge = 0f;
                    handFollowThrough = 0f;
                    NPC.Center = Vector2.Lerp(NPC.Center, normalCenter, 0.3f);
                    break;

                case ActionState.Charging:
                    if (handCharge < 1f)
                    {
                        handCharge += 0.04f;
                        targetRotation += handCharge;
                    }
                    else
                    {
                        
                        //advance aistate by one(to Slinging)
                        //Stupid story: the text is also shared with p3 transition,
                        //so i spent several hours looking for the bug in transition, not this line
                        //Main.NewText("Slop slop slop slop.", Color.Violet);
                        AIState = UpcomingAttack;
                        chargePos = player.Center + (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 120f;
                    }
                    NPC.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                    break;
                case ActionState.Clapping:
                    handCharge = 0f;
                    if (handSling < 1f)
                    {
                        handSling += 0.03f;
                        targetRotation -= handSling;
                    }
                    for (int i = 0; i < Main.maxNPCs; i++) 
                    {
                        if (Main.npc[i].active && Main.npc[i].type == NPC.type && i != NPC.whoAmI)
                        {
                            if (Math.Abs(NPC.position.X - Main.npc[i].position.X) + Math.Abs(NPC.position.Y - Main.npc[i].position.Y) < NPC.width)
                            {
                                NPC.velocity *= 0f;
                                Main.NewText("Clapd.", Color.Violet);
                            }
                            else
                            {
                                NPC.Center = Vector2.Lerp(normalCenter, chargePos, (float)Math.Sin(handSling * Math.PI));
                            }
                        }
                        
                    }
                    break;
                case ActionState.Slinging:
                    handCharge = 0f;
                    if (handSling < 1f)
                    {
                        handSling += 0.03f;
                        targetRotation -= handSling;
                    }
                    else
                    {
                        if (handFollowThrough < 1f)
                        {
                            handFollowThrough += 0.1f;
                        }
                        else
                        {
                            if (bSecondStage)
                            {
                                NetSync();
                                if (body.ai[0] < 6)
                                {
                                    body.ai[0]++;
                                    NetSync();
                                    OtherHandControl(0, 1, 2);
                                }
                            }
                            else
                            {
                                NPC.life = 0;
                                NPC.checkDead();
                                NPC.active = false;
                            }
                            AIState = ActionState.Waiting;
                        }
                    }
                    NPC.Center = Vector2.Lerp(normalCenter, chargePos, (float)Math.Sin(handSling * Math.PI));
                    break;
                case ActionState.DownToSize:
                    handSling = 0f;
                    handCharge = 0f;
                    handFollowThrough = 0f;
                    NPC.Center = Vector2.Lerp(NPC.Center, normalCenter, 0.3f);
                    if (Timer++ >= 30)
                    {
                        if (bSecondStage)
                        {
                            body.ai[0]++;
                            NetSync();
                            OtherHandControl(0, 6, 6);
                        }
                        else
                        {
                            OtherHandControl(0, 1, 2);
                            AIState = ActionState.Waiting;
                            NPC.life = 0;
                            NPC.checkDead();
                            NPC.active = false;
                        }
                    }
                    break;
                case ActionState.MeteorStrike:
                        break;
                case ActionState.ForceKill:                    
                        NPC.life = 0;
                        NPC.checkDead();
                        NPC.active = false;
                    break;


            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (projectile.active && projectile.type == ProjectileID.CopperShortswordStab && Math.Abs(NPC.position.X - projectile.position.X) + Math.Abs(NPC.position.Y - projectile.position.Y) < NPC.width)
                {
                    if (Timer == 0 && AIState == ActionState.Slinging &&
                        handFollowThrough < 1f &&
                        player.Distance(NPC.Center) < 60f)
                    {
                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                            new ParticleOrchestraSettings { PositionInWorld = projectile.Center }, NPC.whoAmI);
                        SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), NPC.Center);
                        player.GetITDPlayer().BetterScreenshake(20, 5, 5, true);
                        AIState = ActionState.DownToSize;
                        NetSync();

                        if (body.life > body.lifeMax / 10)
                        {
                            body.life -= body.lifeMax / 10;
                        }
                        CombatText.NewText(NPC.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                        NPC.velocity = -NPC.velocity * 2;
                        // if the achievements mod is on, unlock the parry achievement
                        ITD.Instance.achievements?.Call("Event", "ParryCosJelHand");
                    }
                }

            }
            NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.05f);
            if (emitter != null)
                emitter.keptAlive = true;
            if (Main.rand.NextBool(3))
            {
                Vector2 velo = NPC.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                Vector2 veloDelta = NPC.position - NPC.oldPosition;
                Vector2 sideOffset = new(-30f * NPC.direction, -20f);
                emitter?.Emit(NPC.Center + new Vector2(0f, NPC.height / 2) + sideOffset, ((velo * 3f) + veloDelta).RotatedByRandom(0.6f));
            }
        }
        public void OtherHandControl(int currentAttack, int attackID, int upcomingID)
        {
            for (int i = 0; i < Main.maxNPCs; i++) //control other hand
            {
                if (Main.npc[i].active && Main.npc[i].type == NPC.type && i != NPC.whoAmI && Main.npc[i].ai[2] == NPC.ai[2] && Main.npc[i].ai[0] == currentAttack)
                {
                    Main.npc[i].velocity = Vector2.Zero;
                    Main.npc[i].ai[0] = attackID;
                    Main.npc[i].ai[1] = upcomingID;
                    Main.npc[i].localAI[0] = NPC.localAI[0];
                    Main.npc[i].localAI[1] = NPC.localAI[1];
                    Main.npc[i].netUpdate = true;
                    break;
                }
            }
        }
        private void NetSync()//Netsync returns!
        {
            NPC body = MiscHelpers.NPCExists(CosJelIndex, ModContent.NPCType<CosmicJellyfish>());
            if (body == null)
            {
                NPC.life = 0;
                NPC.checkDead();
                NPC.active = false;
                return;
            }
            body.netUpdate = true;
            NPC.netUpdate = true;
        }
        public override bool CheckActive()
        {
            return false;
        }
        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;
            if (AIState != ActionState.Charging && AIState != ActionState.Slinging)
            {
                int frameSpeed = 5;
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > frameSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > (finalFrame) * frameHeight)
                    {
                        NPC.frame.Y = (startFrame) * frameHeight;
                    }
                }
            }
            else if (AIState == ActionState.Charging)
            {
                NPC.frame.Y = (5) * frameHeight;
            }
            else if (AIState == ActionState.Slinging || (AIState == ActionState.MeteorStrike))
            {
                NPC.frame.Y = (6) * frameHeight;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.npcFrameCount[Type] * 0.5f);
            void DrawAtNPC(Texture2D texture)
            {
                spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            if (AIState == ActionState.Slinging || AIState == ActionState.Charging)
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 center = NPC.Size * 0.5f;
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(outline, drawPos, NPC.frame, color, NPC.oldRot[k], origin, NPC.scale, NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline));
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(tex));
            return false;
        }
    }
}