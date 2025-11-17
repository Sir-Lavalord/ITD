using ITD.Content.Dusts;
using ITD.Content.Items.Accessories.Movement.Boots;
using ITD.Content.Items.Armor.Vanity.Masks;
using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Items.Placeable.Furniture.Relics;
using ITD.Content.Items.Placeable.Furniture.Trophies;
using ITD.Content.Projectiles.Hostile;
using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Particles;
using ITD.Particles.CosJel;
using ITD.PrimitiveDrawing;
using ITD.Systems;
using ITD.Systems.DataStructures;
using ITD.Systems.Extensions;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Skies;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ITDNPC
    {
        public override Func<bool> DownedMe => () => DownedBossSystem.DownedCosJel;
        public override float BossWeight => 3.9f;
        public override IItemDropRule[] CollectibleRules =>
        [
            ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishTrophy>(), 10),
            ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CosmicJellyfishRelic>()),
            ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<CosmicJam>(), 4)
        ];
        public override void SetStaticDefaultsSafe()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            Main.npcFrameCount[NPC.type] = 10;
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 3;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(bFinalAttack);
            writer.Write(goodtransition);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(AIRand);

        }
        //AI timer, timer for attacks
        public ref float AITimer1 => ref NPC.ai[1];
        public ref float AITimer2 => ref NPC.ai[2];

        //AttackID: stores current attack
        public ref float AttackID => ref NPC.ai[3];

        //AttackCount: the times an attack has been done, loops current attack until reach the set amount
        public ref float AttackCount => ref NPC.ai[0];


        //Timer for the dash function
        public ref float DashTimer => ref NPC.localAI[1];

        //check if cosjel is dashing

        //
        Vector2 targetPos = Vector2.Zero;
        //dash velocity
        Vector2 dashVel;

        //store where to dash
        public Vector2 dashPos;

        //store cosjel rotation
        public float rotation = 0f;
        // outline opacity;
        public float glowOpacity = 1f;


        //store a random number, use to randomize the amount of projectiles in void ring attack
        public float AIRand = 0f;

        //check for final attack before killing cosjel
        public bool bFinalAttack;

        //check for cosjel phase 2
        public bool bSecondStage => NPC.localAI[2] != 0;//check for cosjel phase 2

        int goodtransition;//add a number to the current frame count to sync the animation, used to be for the tentacle

        //difficulty check
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;

        //for attack where cosjel swaps hands after attack, this starts at right hand
        int currentHand = 1;

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bFinalAttack = reader.ReadBoolean();
            goodtransition = reader.ReadInt32();
            AttackCount = reader.ReadSingle();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            AIRand = reader.ReadSingle();
        }
        private enum MovementState
        {
            FollowingRegular,//follow the player, almost always on top of player
            FollowingSlow,//follow the player but slower, for deathray sweep attack, player outruns cosjel
            Inbetween,//transition
            Dashing,//is currently dashing
            Explode,//stand still
            Slamdown,//slam attack, is currently dropping down
            RandomMove,// Sort of lock in but move randomly away from the player
            Teleport//so cheap god damn
        }
        private MovementState AI_State = MovementState.FollowingRegular;// starts in normal follow mode

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 252;
            NPC.damage = 15;
            NPC.defense = 5;
            NPC.lifeMax = 3500;
            NPC.HitSound = new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelOuch")
            {
                PitchVariance = 0.75f
            };
            NPC.DeathSound = SoundID.DD2_DefeatScene;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
            if (!Main.dedServ)
            {
                Music = ITD.Instance.GetMusic("CosmicJellyfish") ?? MusicID.Boss1;
            }

            //dash shader
            Shader.UseImage0("Images/Extra_" + 201);
            Shader.UseImage1("Images/Extra_" + 193);
            Shader.UseImage2("Images/Extra_" + 252);
            Shader.UseSaturation(-2.8f);
            Shader.UseOpacity(2f);
            //balls
            emitter = ParticleSystem.NewEmitter<SpaceMist>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
            emitter.tag = NPC;
        }
        //loot
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<CosmicJellyfishBag>()));
            base.ModifyNPCLoot(npcLoot); // keep this

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<GravityBoots>(), 10));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 15, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.Star, 1, 10, 20));


        }
        //scale npc stat
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);

            NPC.damage = (int)(NPC.damage * 0.7f);
        }
        //testing this: boss doesn't hit in these movement mode to avoid unfair hit
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return AI_State != MovementState.FollowingRegular && AI_State != MovementState.FollowingSlow;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (emitter != null)
                emitter.keptAlive = true;
        }
        //kill flag
        public override void OnKill()
        {
            DownedBossSystem.DownedCosJel = true;
        }
        private Vector2[] dashOldPositions = new Vector2[40];
        private float[] dashOldRotations = new float[40];
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            if (emitter != null)
                emitter.keptAlive = true;
            ITDGlobalNPC.cosjelBoss = NPC.whoAmI;
            Player player = Main.player[NPC.target];
            //start npc movement if has target
            Movement(player);
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 eyePos = NPC.Center + new Vector2(0, -60);
            NPC.damage = NPC.defDamage;
            if (player.dead || !player.active || Main.IsItDay())//despawn at dawn
            {
                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }
            if (!bFinalAttack)//if not final attack, always check if cosjel is in phase 2 
            {
                CheckSecondStage();
            }
            if (!SkyManager.Instance["ITD:CosjelOkuuSky"].IsActive() && bFinalAttack)//change sky color to black
            {
                SkyManager.Instance.Activate("ITD:CosjelOkuuSky");
            }
            for (int i = dashOldPositions.Length - 1; i > 0; i--)
            {
                dashOldPositions[i] = dashOldPositions[i - 1];
                dashOldRotations[i] = dashOldRotations[i - 1];
                dashOldRotations[i] = NPC.rotation + MathHelper.PiOver2;

            }
            dashOldPositions[0] = NPC.position;
            dashOldRotations[0] = NPC.rotation;
            switch ((int)AttackID)
            {
                case -2://final attack should be here, add if needed
                    NPC.checkDead();
                    break;

                case -1://transition for phase 2
                    int hitTime = 4;

                    if (AI_State == MovementState.Teleport)
                    {
                        NPC.dontTakeDamage = true;
                    }
                    else
                    {
                        if (goodtransition < 0)
                        {
                            if (AITimer2 < hitTime)
                            {
                                if (AITimer1++ >= 60)
                                {
                                    AITimer2++;
                                    AITimer1 = 0;
                                    int count = 6 + (int)AITimer2 * 2;
                                    float dist = 100 + 350 * AITimer2;
                                    float baseRot = 0 + AITimer2 * MathHelper.ToRadians(30);
                                    SoundEngine.PlaySound(SoundID.Item28, eyePos);
                                    for (int i = 0; i < count; i++)
                                    {
                                        float rot = baseRot + MathF.Tau * ((float)i / count);
                                        Vector2 vel = -rot.ToRotationVector2() * 14;
                                        Projectile sword = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos + new Vector2(dist, 0).RotatedBy(rot), Vector2.Zero, ModContent.ProjectileType<CosmicSwordStar>(), (NPC.damage), 1f, Main.myPlayer, NPC.whoAmI, i >= (count - 1) ? 1 : 0);
                                        sword.rotation = vel.ToRotation();
                                    }
                                    for (int i = 0; i < 18; i++)
                                    {
                                        int dust = Dust.NewDust(eyePos, 0, 0, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, Main.rand.NextFloat(1.25f, 2.5f));
                                        Main.dust[dust].noGravity = true;
                                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 12;
                                    }
                                }
                            }
                            else
                            {
                                if (AITimer1++ >= 180)
                                {
                                    NPC.HealEffect((int)((NPC.lifeMax / 2) - NPC.life));
                                    NPC.life = (NPC.lifeMax / 2);
                                    intimidateMe = 1;
                                    goodtransition = 5;
                                    AITimer2 = 0;
                                    AITimer1 = 0;
                                }
                            }
                        }
                        else
                        {
                            if (AITimer2 < hitTime)
                            {
                                if (AITimer1++ >= 45)
                                {
                                    AITimer2++;
                                    AITimer1 = 0;
                                    int count = 10;
                                    float dist = 200 - 30 * AITimer2;
                                    float baseRot = 0 + AITimer2 * MathHelper.ToRadians(15);
                                    SoundEngine.PlaySound(SoundID.Item28, eyePos);
                                    for (int i = 0; i < count; i++)
                                    {
                                        float rot = baseRot + MathF.Tau * ((float)i / count);
                                        Vector2 vel = rot.ToRotationVector2() * 14;
                                        Projectile sword = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos + new Vector2(dist, 0).RotatedBy(rot),
                                            Vector2.Zero, ModContent.ProjectileType<CosmicSwordStar>(), (int)(NPC.damage * 0.75f), 1f, Main.myPlayer, NPC.whoAmI, 0, 1);
                                        sword.rotation = vel.ToRotation();
                                    }
                                    for (int i = 0; i < 18; i++)
                                    {
                                        int dust = Dust.NewDust(eyePos, 0, 0, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, Main.rand.NextFloat(1.25f, 2.5f));
                                        Main.dust[dust].noGravity = true;
                                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 12;
                                    }
                                }
                            }
                            else
                            {
                                if (AITimer1++ >= 0)
                                {
                                    AI_State = MovementState.FollowingRegular;
                                    AttackID = Main.rand.Next(1, 6);//randomized, but not reveal new attack now
                                    ResetStats();
                                    NPC.dontTakeDamage = false;
                                }
                            }
                        }
                    }
                    break;

                case 0://Slam down on start
                    distanceAbove = 700;//goes high above player
                    if (AITimer1++ == 120)
                    {
                        AI_State = MovementState.Slamdown;
                    }
                    if (AttackCount > 0)//doesn't loop
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID = 1;
                        ResetStats();
                        distanceAbove = 250;
                    }
                    break;
                case 1: //Dash attack, spawns sideway wave
                    distanceAbove = 350;
                    if (AITimer1++ >= 120 && AttackCount <= 0)
                    {
                        AITimer1 = 0;
                        AI_State = MovementState.Dashing;
                    }
                    if (AttackCount > 3)//loop twice
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID++;
                        ResetStats(true);
                        distanceAbove = 250;
                    }
                    break;
                case 2: //leap up and slam down attack
                    distanceAbove = 500;
                    if (AI_State != MovementState.Slamdown)
                    {
                        AITimer1++;
                        if (AITimer1 > 40 && AITimer1 < 60)
                        {
                            AI_State = MovementState.FollowingSlow;//slow so it doesn't outpace the player
                            DashTimer = 0;

                        }
                        else if (AITimer1 >= 60)
                        {
                            AITimer1 = 0;
                            AI_State = MovementState.Slamdown;
                        }
                    }
                    if (AttackCount > 3)
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID++;
                        ResetStats(true);
                        distanceAbove = 250;
                    }
                    break;
                case 3:
                    distanceAbove = 350;
                    AITimer1++;

                    if ((AITimer1 >= 120 && AttackCount <= 0) || AITimer1 >= 250)
                    {
                        if (AttackCount < 3)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                float dur = (AttackCount % 2 == 0) ? 60 : 120;
                                //main hand
                                Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                                 ModContent.ProjectileType<CosmicFistBump>(), NPC.damage, 0f, player.whoAmI, (bSecondStage) ? 1 : 0, 0, dur);
                                Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                                 ModContent.ProjectileType<CosmicFistBump>(), NPC.damage, 0f, player.whoAmI, 0, 1, dur);
                            }
                        }
                        AITimer1 = 0;
                        AttackCount++;
                    }

                    if (AttackCount > 3)
                    {
                        AttackID++;
                        ResetStats(true);
                        AttackCount = 0;
                    }
                    break;
                case 4://tensei non-spell dumbed down
                    float dir = (AttackCount % 2 == 0) ? 1 : -1;
                    float finalAttack = 7;
                    bool notFinal = AttackCount < finalAttack;
                    float rotateAngle = notFinal ? 110 : 180;
                    float angleIncrement = notFinal ? 5 : 6;
                    if (AITimer1++ == 10)
                    {
                        distanceAbove = notFinal ? 350 : 500;

                        AITimer2 = -rotateAngle * dir;
                    }
                    if (AITimer1 >= 90)
                    {
                        AI_State = MovementState.Explode;

                        if (dir >= 1 ? (AITimer2 <= rotateAngle * dir) : (AITimer2 >= rotateAngle * dir))
                        {
                            AITimer2 += angleIncrement * dir;
                            float angleRad = MathHelper.ToRadians(AITimer2);
                            float speed = notFinal ? Main.rand.NextFloat(4f, 10f) : 3f;
                            Vector2 velocity = new Vector2((float)Math.Sin(angleRad), -(float)Math.Cos(angleRad)) * speed;
                            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -60), velocity, ModContent.ProjectileType<CosmicGlowStar>(),
                                NPC.damage,
                                0f,
                                -1, player.whoAmI, notFinal ? 30 : 60);
                            proj.localAI[0] = notFinal ? 0 : 1.5f;

                        }
                    }
                    if ((AITimer1 == 160 && AttackCount < finalAttack) || AITimer1 == 200)
                    {
                        AI_State = MovementState.FollowingRegular;

                        if (((AttackCount % 2 == 0) && ((AttackCount < finalAttack - 1 && !bSecondStage))) || ((AttackCount % 2 == 0 || AttackCount == finalAttack - 1) && bSecondStage))
                        {
                            float dist = 400;
                            if (expertMode || masterMode)
                            {
                                dist = 400 - AttackCount * 20;
                            }
                            if (AttackCount == finalAttack - 1 && bSecondStage)
                                dist = 600;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                                 ModContent.ProjectileType<CosmicFistBarrier>(), NPC.damage, 0f, -1, player.whoAmI, 0, dist);
                                Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                                 ModContent.ProjectileType<CosmicFistBarrier>(), NPC.damage, 0f, -1, player.whoAmI, 1, dist);
                            }
                        }
                        AITimer1 = 0;
                        if (AttackCount++ >= finalAttack)
                        {
                            ResetStats();
                            AttackID++;
                        }
                    }
                    break;
                case 5://spits meteor, to player, dash to meteor
                    AITimer1++;

                    if (AITimer2++ == 90)
                    {
                        AI_State = MovementState.Explode;
                        float baseRotation = Main.rand.NextFloat(MathF.Tau);
                        int amount = (expertMode || masterMode) ? 8 : 5;
                        float dist = 1000;
                        for (int i = 0; i < amount; i++)
                        {
                            float rot = baseRotation + MathF.Tau * ((float)i / amount);
                            int damage = (int)(NPC.damage * 0.5f);
                            Vector2 vector = (Vector2.Normalize(Vector2.UnitY.RotatedBy(rot)) * dist);
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicStarlitMeteorite>(), 60, 0f, Main.myPlayer, NPC.whoAmI, vector.X, vector.Y);
                            }
                        }
                    }
                    if (AITimer1 >= 240)
                    {
                        MeteorTarget ??= FindMeteorite(20000);

                        if (MeteorTarget == null)
                            return;
                        if (!MeteorTarget.active || MeteorTarget.timeLeft <= 0)
                        {
                            MeteorTarget = null;
                            return;
                        }

                        AITimer1 = 0;

                        AI_State = MovementState.Dashing;
                    }

                    if (AI_State == MovementState.Dashing)
                    {
                        MeteorTarget ??= FindMeteorite(20000);

                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile other = Main.projectile[i];

                            if (other.type == ModContent.ProjectileType<CosmicStarlitMeteorite>() && other.active && other.timeLeft > 0
                                && Math.Abs(NPC.Center.X - other.position.X)
                                + Math.Abs(NPC.Center.Y - other.position.Y) < NPC.width)
                            {
                                NPC.velocity *= 0.5f;
                                player.GetModPlayer<ITDPlayer>().BetterScreenshake(10, 10, 10, true);
                                other.Kill();
                                other.active = false;
                                MeteorTarget = null;

                            }
                        }

                    }
                    if (AttackCount > 2)//loop
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID ++;
                        ResetStats(true);
                        distanceAbove = 250;
                    }
                    break;

                case 6://me when i lie about no arena, quasi-deathray tentacle electric border
                    AITimer1++;
                    distanceAbove = 300;
                    if (AITimer1 == 120)
                    {
                        AI_State = MovementState.Explode;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicTentacleArena>(), 60, 0f, Main.myPlayer, NPC.whoAmI, 0, 0);

                        }

                    }
                    if (AITimer1 >= 600)//loop
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID = 1;
                        ResetStats(true);
                        distanceAbove = 250;
                    }
                    break;

                case 7://open whitehole, kills mini jellyfish, total jellyfish death
                    AITimer1++;
                    distanceAbove = 300;
                    int maxAttack = 3;
                    if (AttackCount <= maxAttack)
                    {
                        if (AITimer1 >= 90 && AttackCount <= 0 || AITimer1 >= 450)
                        {
                            Projectile Blast = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos, Vector2.Zero,
                    ModContent.ProjectileType<CosmicJellyfishBlast>(), 0, 0);
                            Blast.ai[1] = 250f;
                            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true;
                            AttackCount++;
                            AITimer1 = 0;
                            AI_State = MovementState.Explode;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(player.Center.X, player.Center.Y + Main.screenHeight/2), Vector2.Zero, ModContent.ProjectileType<CosmicSwarmBlackhole>(), 60, 0f, Main.myPlayer, NPC.whoAmI, 0, 0);

                            }

                        }
                    }
                    if (AITimer1 >= 60 && AITimer1 < 120 && AttackCount <= 0 || AITimer1 >= 200 && AITimer1 <= 400)
                    {
                        AI_State = MovementState.FollowingRegular;
                    }
                    if (AttackCount > maxAttack && AITimer1 >= 500)//loop
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID++;
                        ResetStats(true);
                        distanceAbove = 250;
                    }
                    break;

                case 8:
                    AITimer1++;
                    float angleDeviation = 40;
                    float amountFire = 30;
                    float maxDeviation = 90;
                    float maxAmount = 90;

                    if (expertMode || masterMode)
                    {
                        maxDeviation = 100;
                        maxAmount = 90;
                    }
                    else if (!expertMode || !masterMode)
                    {
                        maxDeviation = 70;
                        maxAmount = 50;
                    }
                    amountFire = Math.Clamp(30 + 5 * AttackCount, 0, maxAmount);
                    angleDeviation = Math.Clamp(40 + 10 * AttackCount, 0, maxDeviation);
                    Main.NewText((expertMode, angleDeviation));
                    if (AITimer1 == 60)
                    {
                        AI_State = MovementState.Explode;
                    }
                    if (AITimer1 == 100)
                    {
                        targetPos = Vector2.Normalize(player.Center - eyePos);
                    }
                    if (AITimer1 >= 120)
                    {

                        Vector2 velocity = targetPos.RotateRandom(MathHelper.ToRadians(angleDeviation));
                        Vector2 magVec = (eyePos +
                            new Vector2(100, 0).RotatedBy(velocity.ToRotation())) - eyePos;
                        magVec.Along(eyePos, 10, v =>
                        {
                            for (int i = 0; i <= 1; i++)
                            {
                                Dust dust = Dust.NewDustPerfect(new Vector2(v.X, v.Y), DustID.PurpleCrystalShard, Vector2.Zero, 0, Scale: 2);
                                dust.scale = 1.75f;
                                dust.noGravity = true;
                            }
                        });
                        for (int i = 0; i <= 3; i++)
                        {
                            Dust dust = Dust.NewDustDirect(eyePos, 1, 1, DustID.PurpleCrystalShard, 0, 0);
                            dust.scale = 1.5f;
                            dust.noGravity = true;
                            dust.velocity = -velocity * 20;
                        }
                        Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), eyePos +
                            new Vector2(100, 0).RotatedBy(velocity.ToRotation()), Vector2.Zero,
                            ModContent.ProjectileType<CosmicSwordStar2>(), NPC.damage, 0, -1, 0, 0, amountFire - (AITimer2));
                        proj.rotation = velocity.ToRotation();
                        AITimer2++;
                        if (AITimer2 >= amountFire)
                        {
                            distanceAway =300 * (AttackCount % 2 == 0? 1: -1);
                            distanceAbove = 0;
                            AI_State = MovementState.FollowingRegular;
                            AITimer2 = 0;
                            AITimer1 = 0;
                            AttackCount++;
                        }
                    }
                    if (AttackCount > 8)//loop
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID++;
                        ResetStats(true);
                        distanceAbove = 250;
                        distanceAway = 0;
                    }

                    break;
                case 9: //deathray 4 now
                    BlackholeDusting(3);
                    float ringAmount = 4;
                    AITimer1++;
                    distanceAbove = 200;
            
                    if (AttackCount >= ringAmount)
                    {
                        if (AITimer2++ == 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                                 ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 240, NPC.whoAmI);
                            }
                        }
                        if (AITimer2 >= 1000)
                        {
                            AI_State = MovementState.FollowingRegular;
                            AttackID = 1;
                            ResetStats();
                            NetSync();
                        }
                    }
                    else
                    {
                        if (AITimer1++ >= 60)
                        {
                            AI_State = MovementState.Explode;
                            AttackCount++;
                            AITimer1 = 0;
                            int count = 3 + (int)AttackCount * 2;
                            float dist = 200 + 400 * AttackCount;
                            float baseRot = 0 + AttackCount * MathHelper.ToRadians(15);
                            SoundEngine.PlaySound(SoundID.Item28, eyePos);
                            for (int i = 0; i < count; i++)
                            {
                                float rot = baseRot + MathF.Tau * ((float)i / count);
                                Vector2 vel = -rot.ToRotationVector2() * 14;
                                Projectile meteor = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos + new Vector2(dist, 0).RotatedBy(rot), Vector2.Zero, ModContent.ProjectileType<CosmicRayMeteorite>(), (NPC.damage), 1f, Main.myPlayer, NPC.whoAmI);
                            }
                            for (int i = 0; i < 18; i++)
                            {
                                int dust = Dust.NewDust(eyePos, 0, 0, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, Main.rand.NextFloat(1.25f, 2.5f));
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 12;
                            }
                        }
                    }
                    break;

            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        float distanceAbove = 250f;//distance above the player
        float distanceAway = 0;//distance away from the player, why did i not do this until now

        private void ShardSlam()//slam attack, call once hit the ground
        {
            float XVeloDifference = 2;
            float startXVelo = -((float)(10) / 2) * (float)XVeloDifference;
            for (int j = 0; j < 30; j++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center - new Vector2((NPC.width), 0), NPC.width * 2, 10, DustID.PurpleTorch);
                dust.noGravity = true;
                dust.scale = 2.25f * Main.rand.NextFloat(0.75f, 1.25f);
                dust.velocity.X = 10 * (j % 2 == 0 ? 1 : -1) * Main.rand.NextFloat(0.25f, 1.25f);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -5f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), (int)(NPC.damage * 0.75f), 0, -1, NPC.whoAmI);
                }
                if (bSecondStage)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Projectile shockwave = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(30 * j, -10), new Vector2(1 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), NPC.damage, 0, -1);
                        shockwave.spriteDirection = j;
                    }
                }
            }
        }
        //movement code                    '
        Vector2 whereToGo = Vector2.Zero;

        private void Movement(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(distanceAway, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            float speed = abovePlayer.Length() / 1.2f;//raising above player slowly
            float speed2 = 20;
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            switch (AI_State)
            {
                case MovementState.FollowingRegular:

                    if (speed > 1.1f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity = aboveNormalized * (speed + 1f) / speed2;
                            NetSync();
                        }
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    rotation = rotationFactor * maxRotation;
                    NPC.rotation = rotation;

                    break;
                case MovementState.FollowingSlow:
                    if (speed > 1.05f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            speed2 = 28f;
                            NPC.velocity = aboveNormalized * (speed) / speed2;
                            NetSync();
                        }
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    NPC.rotation = 0;

                    break;
                case MovementState.Dashing:
                    AITimer1++;
                    switch (AttackID)
                    {

                        case 1:

                            if (AITimer1 <= 10)
                            {
                                dashPos = player.Center;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.velocity *= 0.95f;
                                    NetSync();
                                }
                            }
                            else
                            {
                                Dash(dashPos, 10, 30, 80, 100, 1);
                            }
                            break;
                        case 5:
                            if (AITimer1 <= 10)
                            {
                                MeteorTarget ??= FindMeteorite(20000);
                                if (MeteorTarget == null)
                                {
                                    AttackCount++;
                                    AI_State = MovementState.FollowingRegular;
                                    return;
                                }
                                dashPos = MeteorTarget.Center;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    NPC.velocity *= 0.95f;
                                    NetSync();
                                }
                            }
                            else
                            {
                                if (expertMode || masterMode)
                                    Dash(dashPos, 1, 25, 35, 45, 2);
                                else Dash(dashPos, 1, 25, 40, 60, 2);

                            }
                            break;
                    }
                    if (DashTimer > 0)
                    {
                        if (emitter != null)
                            emitter.keptAlive = true;
                        for (int j = 0; j < 6; j++)
                        {
                            emitter?.Emit(Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity * 0.25f, 0f, 20);
                        }
                    }
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.2f);
                    NetSync();
                    break;
                case MovementState.Explode:
                    NPC.velocity *= 0.95f;
                    rotation = rotationFactor * maxRotation;
                    NPC.rotation = rotation;
                    break;
                case MovementState.Slamdown:
                    //This scan for floor, then do collision, inconsistent sometimes
                    RaycastData data = Helpers.QuickRaycast(NPC.Center, NPC.velocity, (point) => { return (player.Bottom.Y >= point.ToWorldCoordinates().Y + 20); }, 120);
                    if (AITimer2 <= 0)
                    {
                        if (NPC.Center.Distance(data.End) >= 20)
                        {
                            if (AITimer1++ >= 5)
                            {
                                NPC.velocity.X *= 0.99f;
                                NPC.velocity.Y += 0.5f;
                            }
                            else
                            {
                                NPC.velocity.X *= 0.96f;
                            }
                        }
                        else
                        {
                            NPC.velocity *= 0;
                            AITimer2++;
                        }
                    }
                    else
                    {
                        if (AITimer2++ == 2)
                        {
                            //could have been a dash() except this needs a big velo boost from the start
                            dashVel = Vector2.Normalize(new Vector2(NPC.Center.X, NPC.Center.Y - 750) - new Vector2(NPC.Center.X, NPC.Center.Y)) * 18;
                            NPC.velocity = dashVel;
                            AttackCount++;
                            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelSlam"), NPC.Center);
                            ShardSlam();
                            player.GetITDPlayer().BetterScreenshake(30, 10, 20, true);//Very shaky, might need some tweaking to the decay
                        }
                        else if (AITimer2 >= 20 && AITimer2 <= 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                NPC.velocity *= 0.95f;
                                NPC.netUpdate = true;

                            }
                        }
                        else if (AITimer2 > 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                AITimer2 = 0;
                                AITimer1 = 0;
                                AI_State = MovementState.Inbetween;
                                NetSync();
                            }
                        }
                    }
                    NPC.rotation = 0;
                    break;
                case MovementState.Teleport:// flash whole body, then teleport on top of player, no contact damage

                    if (NPC.HasValidTarget)
                    {
                        Teleport(Main.player[NPC.target].Center + new Vector2(0, -400), 60, 90, (int)AttackID);
                    }
                    NPC.velocity *= 0.95f;
                    rotation = rotationFactor * maxRotation;
                    NPC.rotation = rotation;
                    break;
            }
            teleEffect -= 0.05f;
            intimidateMe -= 0.02f;
        }
        public void Teleport(Vector2 telePos, int time1, int time2, int attackID)
        {
            AITimer1++;
            if (AITimer1 == time1 - 30 || AITimer1 == time2 - 20)
            {
                teleEffect = 1;
                if (!reappearTele)
                {
/*                    Vector2 magVec = telePos - NPC.Center;
                    magVec.Along(NPC.Center, 6, v =>
                    {
                        for (int i = 0; i <= 1; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(new Vector2(v.X, v.Y), DustID.PurpleCrystalShard, Vector2.Zero, 0, Scale: 2);
                            dust.noGravity = true;
                        }
                    });*/
                }
            }
            if (AITimer1 == time1)
            {
                NPC.Center = telePos;
            }
            else if (AITimer1 >= time2)
            {
                AITimer1 = 0;
                switch (attackID)
                {
                    case -1:
                        AI_State = MovementState.Explode;
                        break;
                    case 6:
                        AI_State = MovementState.Explode;
                        AITimer2 = 1;
                        distanceAbove = 300;
                        break;
                }
            }
        }
        //pos: set dash to where
        //time1: when to start dashing, start gaining velocity
        //time2: stop gaining velocity
        //time3: start slowing down
        //reset: time to reset attack
        //attackID: the same as attackID above, use this to add special effects to a specific attack 
        public void Dash(Vector2 pos, int time1, int time2, int time3, int reset, int attackID)
        {
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, ModContent.DustType<StarlitDust>(), 0f, 0f, 0, default, 1f);
                dust.noGravity = true;
                dust.fadeIn = 0.2f;
                Dust dust2 = Dust.NewDustDirect(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, ModContent.DustType<StarlitDust>(), 0f, 0f, 0, Color.Purple, 1f);
                dust2.noGravity = true;
                dust2.fadeIn = 0.1f;
            }
            Player player = Main.player[NPC.target];
            if (DashTimer == time1)
            {
                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelDash"), NPC.Center);
                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/WRipperRip"), NPC.Center);
                dashVel = Vector2.Normalize(new Vector2(pos.X, pos.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                NPC.velocity = dashVel;
                NPC.netUpdate = true;
            }
            DashTimer++;

            if (attackID == 1)
            {
                if (DashTimer % 10 == 0 && DashTimer > time1 && DashTimer < reset)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 vel1 = !bSecondStage ? Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2) : Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2 + MathHelper.Pi/6) * 1.25f;
                        Vector2 vel2 = !bSecondStage ? Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2) : Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2 - MathHelper.Pi/6) * 1.25f;
                        Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, vel1, ModContent.ProjectileType<CosmicWave>(), (NPC.defDamage), 0, Main.myPlayer);
                        Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, vel2, ModContent.ProjectileType<CosmicWave>(), (NPC.defDamage), 0, Main.myPlayer);
                        proj1.tileCollide = false;
                        proj2.tileCollide = false;
                        if (expertMode|| masterMode)
                        {
                            Projectile proj3 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<CosmicWave>(), (NPC.defDamage), 0, Main.myPlayer);
                            proj3.tileCollide = false;

                        }
                    }
                }

            }
            if (attackID == 2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    //it has to spawn something wtf
                    Projectile WHATISTHIS = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center,
                        Vector2.Zero, ProjectileID.None,
                        (NPC.defDamage), 0);
                }
            }
            NPC.netUpdate = true;

            if (DashTimer > time1 && DashTimer < time2)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity *= 1.18f;
                    NPC.netUpdate = true;
                }
            }
            if (DashTimer > time3)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity *= 0.96f;
                    NPC.netUpdate = true;

                }
            }
            if (DashTimer >= reset)
            {
                DashTimer = 0;
                switch (AttackID)
                {
                    case 0:
                        AITimer2 = 0;
                        AttackCount++;
                        break;
                    case 1:
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackCount++;
                        break;
                    case 2:
                        AITimer2 = 0;
                        AI_State = MovementState.Inbetween;
                        break;
                    case 5:
                        AITimer1 = 0;
                        AITimer2 = 0;
                        break;
                }

                NetSync();
            }

        }
        //check if cosjel is in second stage
        private void CheckSecondStage()
        {
            if (bSecondStage)
            {
                if (AttackID > 9)//use phase 2 attacks
                {
                    AttackID = 1;
                }
            }
            else
            {
                if (AttackID >= 6)//use phase 1 attacks
                {
                    AttackID = 1;
                }
            }
            if (NPC.life * 100 / NPC.lifeMax < 50 && !bSecondStage)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                }
                if (Main.netMode != NetmodeID.Server)
                {
                }
                AITimer1 = 0;
                AITimer2 = 0;
                NPC.localAI[2] = 1;
/*                HandControl(1, 7, 3, true);
                HandControl(-1, 7, 3, true);*/
                AttackID = -1;
                AI_State = MovementState.Teleport;
                NetSync();

            }
            return;
        }
        //Just do reset stat
        public void ResetStats(bool doKillHand = false)
        {
            AITimer1 = 0;
            AITimer2 = 0;
            AttackCount = 0;
            DashTimer = 0;
            NetSync();
        }
        //check if cosjel is dead, if hasn't done final attack, don't kill cosjel
        public override bool CheckDead()
        {
            //should a last stand even be in a boss like this
            if (!bSecondStage)
            {
                NPC.life = 1;
                return false;
            }
            return true;
        }
        //[3]: 1 is left
        //[2]: 0: wait, 1: charge, 2: sling
        //This controls cosjel hands

        //whichHand: chose the cosjel hand to control.
        //1 is left hand
        //-1 is right hand

        //AttackID: set current attack of the chosen hand to this
        //UpcomingID set the attack to go to after attackID

        //IsForceKill: ignore everything, kill hand, convoluted but it works
/*        private void HandControl(int whichHand, int attackID, int upcomingID, bool IsForceKill)
        {
            for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
            {
                if ((Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>()
                    && Main.npc[i].ai[2] == NPC.whoAmI
                    && Main.npc[i].ai[3] == whichHand
                    && Main.npc[i].ai[0] == 0) || IsForceKill)//waiting ai state
                {
                    Main.npc[i].ai[0] = attackID;
                    Main.npc[i].ai[1] = upcomingID;
                    Main.npc[i].localAI[0] = 0;
                    Main.npc[i].localAI[1] = 0;
                    Main.npc[i].netUpdate = true;
                }
            }
        }*/
        //find hand
        //whichHand: which hand to find
        //1 is left hand
        //-1 is right hand
        private Projectile MeteorTarget;
        public Projectile FindMeteorite(float maxDetectDistance)
        {
            Projectile closestProjectile = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (projectile.active && projectile.type == ModContent.ProjectileType<CosmicStarlitMeteorite>())
                {
                    float sqrDistanceToTarget = Vector2.DistanceSquared(projectile.Center, NPC.Center);

                    if (sqrDistanceToTarget < sqrMaxDetectDistance)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestProjectile = projectile;
                    }
                }
            }

            return closestProjectile;
        }
/*        private static bool HandExist(int whichHand)
        {
            for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>()
                    && Main.npc[i].ai[3] == whichHand)
                {
                    return true;
                }
            }
            return false;
        }*/

        //sync hand
        private void NetSync(bool forceAll = false)
        {
            if (Main.netMode == NetmodeID.Server || forceAll)
            {
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
            }
            NPC.netUpdate = true;
        }
        //animate cosjel
        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;
            if (!bFinalAttack)
            {
                int frameSpeed = 5;
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > frameSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > (finalFrame + goodtransition) * frameHeight)
                    {
                        NPC.frame.Y = (startFrame + goodtransition) * frameHeight;
                    }
                }
            }
            else
            {
                NPC.frame.Y = 10 * frameHeight;
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }

        //do dust effect
        public override bool PreAI()
        {
            if (!bFinalAttack)
            {
                if (!bSecondStage)
                {
                    Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, default, 1f);
                }
                else
                {
                    if (!reappearTele && teleEffect <= 0)
                    Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, Color.DarkViolet, 1f);
                }
            }
            else
            {
                BlackholeDusting(4);
            }
            return true;
        }
        //blackhole dust, for phase 2+
        //ring: amount of dust rings to spawn
        private void BlackholeDusting(int ring)
        {
            int dustRings = ring;
            for (int h = 0; h < dustRings; h++)
            {
                float distanceDivisor = h + 1f;
                float dustDistance = 750 / distanceDivisor;
                int numDust = (int)(0.1f * MathHelper.TwoPi * dustDistance);
                float angleIncrement = MathHelper.TwoPi / numDust;
                Vector2 dustOffset = new Vector2(dustDistance, 0f);
                dustOffset = dustOffset.RotatedByRandom(MathHelper.TwoPi);

                int var = (int)(dustDistance);
                float dustVelocity = 24f / distanceDivisor;
                for (int i = 0; i < numDust; i++)
                {
                    if (Main.rand.NextBool(var))
                    {
                        dustOffset = dustOffset.RotatedBy(angleIncrement);
                        int dust = Dust.NewDust(NPC.Center, 1, 1, ModContent.DustType<CosJelDust>());
                        Main.dust[dust].position = NPC.Center + dustOffset;
                        Main.dust[dust].fadeIn = 1f;
                        Main.dust[dust].velocity = Vector2.Normalize(NPC.Center - Main.dust[dust].position) * dustVelocity;
                        Main.dust[dust].scale = 3f - h;
                    }
                }
            }
        }
        public override void DrawBehind(int index)
        {
            if (bFinalAttack && AttackID != -2)
            {
                Main.instance.DrawCacheNPCsOverPlayers.Add(index);
            }

        }
        //john vertexstrip o
        public static MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);

        public VertexStrip TrailStrip = new VertexStrip();

        public ParticleEmitter emitter;

        private Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(new Color(36, 12, 34), new Color(84, 73, 255), Utils.GetLerpValue(0f, 0.8f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
            result.A /= 2;
            return result * NPC.Opacity;
        }
        private float StripWidth(float progressOnStrip)
        {
            return 80f;
        }
        float teleEffect = 0;
        float intimidateMe = 0;//actual fargo epicMe this time

        bool reappearTele = false;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 stretch = new(1f, 1f);
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
            Vector2 origin = new(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
            Rectangle frameRect = new(0, NPC.frame.Y, tex.Width, vertSize);
            Vector2 center = NPC.Size / 2f;
            Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            if (DashTimer > 0 && AI_State != MovementState.FollowingRegular)
            {
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustPerfect(
                        NPC.Center + Main.rand.NextVector2Circular(50, 50),
                        DustID.PurpleTorch,
                        NPC.velocity * 0.5f,
                        0, Color.White, 2f
                    );
                    d.noGravity = true;
                }
                stretch = new Vector2(1f, 1f + NPC.velocity.Length() * 0.025f);
            }
            if (AI_State == MovementState.Dashing)
            {
                Shader.Apply();
                TrailStrip.PrepareStrip(dashOldPositions, dashOldRotations, StripColors, StripWidth, NPC.Size * 0.5f - Main.screenPosition, dashOldPositions.Length, true);
                TrailStrip.DrawTrail();
                Main.spriteBatch.End(out SpriteBatchData spriteBatchData); // unapply shaders
                Main.spriteBatch.Begin(spriteBatchData);

            }
            if (AI_State == MovementState.Slamdown)
            {
                if (NPC.velocity.Y != 0)
                {
                    stretch = new Vector2(1f, 1f + NPC.velocity.Length() * 0.02f);
                }
                else
                {
                    stretch = new Vector2(1f + NPC.oldVelocity.Length() * 0.025f, 1f - NPC.oldVelocity.Length() * 0.01f);
                }
            }

            if (AI_State == MovementState.Dashing && DashTimer > 0)
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length) * 0.4f;
                    spriteBatch.Draw(tex, drawPos, frameRect, color, NPC.oldRot[k], origin, 1f, SpriteEffects.None, 0f);
                }
            }
            Vector2 miragePos = NPC.position - Main.screenPosition + center;

            //old treasure bag draw code, augh
            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;

            time %= 4f;
            time /= 2f;

            if (time >= 1f)
            {
                time = 2f - time;
            }

            time = time * 0.5f + 0.75f;
            if (AI_State != MovementState.FollowingRegular && AI_State != MovementState.FollowingSlow && ((teleEffect <= 0) && !reappearTele))
            {
                glowOpacity = MathHelper.Clamp(glowOpacity + 0.1f, 0f, 1f);
                for (float i = 0f; i < 1f; i += 0.25f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    spriteBatch.Draw(tex, miragePos + new Vector2(0f, 8f).RotatedBy(radians) * time, frameRect, new Color(90, 70, 255, 50) * glowOpacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
                }

                for (float i = 0f; i < 1f; i += 0.34f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    spriteBatch.Draw(tex, miragePos + new Vector2(0f, 12f).RotatedBy(radians) * time, frameRect, new Color(90, 70, 255, 50) * glowOpacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
                }
            }
            else
            {
                glowOpacity = MathHelper.Clamp(glowOpacity - 0.1f, 0f, 1f);
            }
            if (teleEffect > 0)//fargo eridanus epic
            {
                float scale = 1f * NPC.scale * (float)Math.Cos(Math.PI / 2 * teleEffect) + 1;
                if (teleEffect <= 0.05f)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, 0, 0, 0, default, Main.rand.NextFloat(1.25f,2.5f));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 20;
                    }
                    reappearTele = !reappearTele;
                }
                float opacity = NPC.Opacity * (float)Math.Sqrt(teleEffect);
                if (reappearTele)
                {
                    Main.EntitySpriteDraw(tex, miragePos, NPC.frame, new Color(255, 255, 255, 255) * opacity, NPC.rotation, NPC.frame.Size() / 2f, new Vector2(MathHelper.Max(scale - 1, 0.1f), 3 - scale), SpriteEffects.None, 0);

                    Main.EntitySpriteDraw(glowTexture, miragePos, null, new Color(255, 255, 255, 255) * opacity, NPC.rotation, glowTexture.Size() / 2f, new Vector2(MathHelper.Max(scale - 1, 0.1f), 3 - scale), SpriteEffects.None, 0);
                }
                else
                {

                    Main.EntitySpriteDraw(tex, miragePos, NPC.frame, new Color(255, 255, 255, 255) * opacity, NPC.rotation, NPC.frame.Size() / 2f, new Vector2(MathHelper.Max(2 - scale, 0.1f), scale), SpriteEffects.None, 0);

                    Main.EntitySpriteDraw(glowTexture, miragePos, null, new Color(255, 255, 255, 255) * opacity, NPC.rotation, glowTexture.Size() / 2f, new Vector2(MathHelper.Max(2 - scale, 0.1f), scale), SpriteEffects.None, 0);
                }

            }
            if (intimidateMe > 0)//fargo eridanus epic
            {
                float scale = 2f * NPC.scale * (float)Math.Cos(Math.PI / 2 * intimidateMe);
                float opacity = NPC.Opacity * (float)Math.Sqrt(intimidateMe);
                Main.EntitySpriteDraw(tex, miragePos, NPC.frame, Color.White * opacity, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale * scale, SpriteEffects.None, 0);
            }
            if (AttackID == -2)
                BlackholeVertex.Draw(NPC.Center - Main.screenPosition, 1024);
            if (teleEffect <= 0 && !reappearTele)
            {
                Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, stretch, SpriteEffects.None);
            }
            return false;
        }
    }
    //shader rip

    public readonly struct BlackholeVertex
    {
        public static void Draw(Vector2 position, float size)
        {
            GameShaders.Misc["Blackhole"]
                .UseImage0(TextureAssets.Extra[ExtrasID.RainbowRodTrailErosion])
                .UseColor(new Color(192, 59, 166))
                .UseSecondaryColor(Color.Beige)
                .Apply();
            SimpleSquare.Draw(position, size: new Vector2(size, size));
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }
    public readonly struct CosmicTelegraphVertex
    {
        public static void Draw(Vector2 position, Vector2 size, float rotation)
        {
            GameShaders.Misc["Telegraph"]
                .UseColor(Color.Purple)
                .UseSecondaryColor(Color.Purple)
                .Apply();
            SimpleSquare.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }
    }
}