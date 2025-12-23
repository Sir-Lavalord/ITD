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
using System.Linq;
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
using Terraria.ID;
using static System.Net.Mime.MediaTypeNames;
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
            NPCID.Sets.TrailCacheLength[Type] = 24;
            NPCID.Sets.TrailingMode[Type] = 3;
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

        public int maxAttack = 5;
        public List<int> availableAttacks;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(DashTimer);
            writer.Write(bFinalAttack);
            writer.Write(goodtransition);
            writer.Write(AttackCount);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(AIRand);
            writer.Write(maxAttack);
            


        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            DashTimer = reader.ReadSingle();
            bFinalAttack = reader.ReadBoolean();
            goodtransition = reader.ReadInt32();
            AttackCount = reader.ReadSingle();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            AIRand = reader.ReadSingle();
            maxAttack = reader.ReadInt32();

        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (AI_State == MovementState.SuperDashing)
            {
                target.AddBuff(BuffID.BrokenArmor, 300);
            }
        }
        public void CheckAttackList()
        {
            if (bSecondStage)
            {
                if (Main.expertMode || Main.masterMode)
                {
                    maxAttack = 10;
                }
                else maxAttack = 9;
            }
            else
            {
                maxAttack = 5;
            }
            if (availableAttacks != null)
            {
                if (availableAttacks.Count <= 0)
                {
                    availableAttacks = Enumerable.Range(1, maxAttack).ToList();
                }
            }
        }
        public int GetNextAttack()
        {
            int randomIndex = Main.rand.Next(availableAttacks.Count);
            int currentAttack = 0;
            if (currentAttack <= 0)
            currentAttack = availableAttacks[randomIndex];
            availableAttacks.RemoveAt(randomIndex);

            return currentAttack;
        }
        private enum MovementState
        {
            FollowingRegular,//follow the player, almost always on top of player
            FollowingSlow,//follow the player but slower, for deathray sweep attack, player outruns cosjel
            Inbetween,//transition
            Dashing,//is currently dashing
            Explode,//stand still
            Slamdown,//slam attack, is currently dropping down
            Aligning,// aligning for dash
            Teleport,//so cheap god damn
            SuperDashing//is bigdashing
        }
        private MovementState AI_State = MovementState.FollowingRegular;// starts in normal follow mode

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 252;
            NPC.damage = 30;
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
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 30, 60));
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
            availableAttacks = Enumerable.Range(1, maxAttack).ToList();
            if (emitter != null)
                emitter.keptAlive = true;
        }
        public int ProjectileDamage(int damage)
        {
            if (expertMode)
                return (int)(damage / 2.5f);      
            if (masterMode)
                return (int)(damage / 3.5f);
            return (int)(damage / 1);
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
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 eyePos = NPC.Center + new Vector2(0, -60);
            NPC.damage = NPC.defDamage;
            if (PlayerCheck(player))
                Movement(player);



            CheckAttackList();
            CheckSecondStage();
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
                case -2://final attack should be here, add if needed, me when i die
                    break;

                case -1://transition for phase 2
                    P2Transition(player);
                    break;
                case 0://Slam down on start
                    if (!PlayerCheck(player))
                        break;
                    StartUp();
                    break;
                case 1: //Dash attack, spawns sideway wave
                    if (!PlayerCheck(player))
                        break;
                    WaveDash();
                    break;
                case 2: //leap up and slam down attack
                    if (!PlayerCheck(player))
                        break;
                    LeapSlam();
                    break;
                case 3:
                    if (!PlayerCheck(player))
                        break;
                    FistBump(player);
                    break;
                case 4://tensei non-spell dumbed down
                    if (!PlayerCheck(player))
                        break;
                    TenseiSpell(player);
                    break;
                case 5://spits meteor, to player, dash to meteor
                    if (!PlayerCheck(player))
                        break;
                    MeteorDash(player);
                    break;
                case 6://me when i lie about no arena, quasi-deathray tentacle electric border
                    if (!PlayerCheck(player))
                        break;
                    TentacleBorder();
                    break;
                case 7://open whitehole, kills mini jellyfish, total jellyfish death
                    if (!PlayerCheck(player))
                        break;
                    WhiteholePortal(player);
                    break;
                case 8:
                    if (!PlayerCheck(player))
                        break;
                    SwordBurstFire(player);
                    break;
                case 9: //deathray 4 now
                    if (!PlayerCheck(player))
                        break;
                    MeteorDeathRay(player);   
                    break;
                case 10: //blazing star marisa
                    if (!PlayerCheck(player))
                        break;
                    BlazingStar(player);
                    break;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        float distanceAbove = 250f;//distance above the player
        float distanceAway = 0;//distance away from the player, why did i not do this until now
                               
        private bool PlayerCheck(Player player)
        {
            if ((!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 4000f) && AttackID != 0 || Main.dayTime)
            {
                NPC.TargetClosest();
                player = Main.player[NPC.target];
                if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 4000f || Main.dayTime)
                {
                    float maxRotation = MathHelper.Pi / 6;
                    float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
                    if (NPC.timeLeft > 60)
                        NPC.timeLeft = 60;
                    NPC.velocity.Y -= 0.2f; 
                    NPC.velocity.X *= 0.95f;
                    rotation = rotationFactor * maxRotation;
                    NPC.rotation = rotation;
                    NPC.Opacity -= 0.01f;
                    NPC.EncourageDespawn(30);
                }
                return false;
            }
            if (NPC.timeLeft < 600)
                NPC.timeLeft = 600;
            return true;
        }
        #region Attacks
        public void SludgeSlam()//slam attack, call once hit the ground
        {
            for (int j = 0; j < 30; j++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center - new Vector2((NPC.width), 0), NPC.width * 2, 10, DustID.PurpleTorch);
                dust.noGravity = true;
                dust.scale = 2.25f * Main.rand.NextFloat(0.75f, 1.25f);
                dust.velocity.X = 10 * (j % 2 == 0 ? 1 : -1) * Main.rand.NextFloat(0.25f, 1.25f);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int amount = 4;
                amount = masterMode ? 6 : expertMode ? 5 : 4;
                for (int i = -amount; i <= amount; i++)
                {
                    Vector2 spawnPos = new Vector2(NPC.Center.X, NPC.Center.Y);
                    Vector2 velocity = new Vector2(i * 2f, -5);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos,
                        velocity, ModContent.ProjectileType<CosmicSludgeBomb>(), ProjectileDamage(NPC.damage), 0f, Main.myPlayer, 0);
                }
                if (bSecondStage)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Projectile shockwave = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(30 * j, -10), new Vector2(1 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), ProjectileDamage(NPC.damage), 0, -1);
                        shockwave.spriteDirection = j;
                    }
                }
            }
        }

        public void StartUp()
        {
            distanceAbove = 700;//goes high above player
            if (AITimer1++ == 120)
            {
                AI_State = MovementState.Slamdown;
            }
            if (AttackCount > 0)//doesn't loop
            {
                AI_State = MovementState.FollowingRegular;
                AttackID = GetNextAttack();
                ResetStats();
                distanceAbove = 250;
            }
        }
        public void WaveDash()
        {
            distanceAbove = 350;
            if (AITimer1++ >= 120 && AttackCount <= 0)
            {
                AITimer1 = 0;
                AI_State = MovementState.Dashing;
            }
            if (AttackCount > 3)//loop twice
            {
                AI_State = MovementState.FollowingRegular;
                AttackID = GetNextAttack();
                ResetStats(true);
                distanceAbove = 250;
            }
        }
        public void LeapSlam()
        {
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
                AttackID = GetNextAttack();
                ResetStats(true);
                distanceAbove = 250;
            }
        }
        public void FistBump(Player player)
        {
            distanceAbove = 350;
            AITimer1++;
            float restTime = masterMode ? 250 : expertMode ? 280 : 300;
            if ((AITimer1 >= 120 && AttackCount <= 0) || AITimer1 >= restTime)
            {
                if (AttackCount < 3)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float dur = (AttackCount % 2 == 0) ? 60 : 120;
                        Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                         ModContent.ProjectileType<CosmicFistBump>(), ProjectileDamage(NPC.damage), 0f, player.whoAmI, (bSecondStage) ? 1 : 0, 0, dur);
                        Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                         ModContent.ProjectileType<CosmicFistBump>(), ProjectileDamage(NPC.damage), 0f, player.whoAmI, 0, 1, dur);
                    }
                }
                AITimer1 = 0;
                AttackCount++;
            }

            if (AttackCount > 3 && AITimer1 >= 150)
            {
                AttackID = GetNextAttack();
                ResetStats(true);
                AttackCount = 0;
            }
        }
        public void TenseiSpell(Player player)
        {
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

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -60), velocity, ModContent.ProjectileType<CosmicGlowStar>(),
                            ProjectileDamage(NPC.damage),
                            0f,
                            -1, player.whoAmI, notFinal ? 30 : 60);
                        proj.localAI[0] = notFinal ? 0 : 1.5f;
                    }
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
                        dist = masterMode ? 450 : expertMode ? 500 : 600;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i <= 1; i++)
                        {
                            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero,
                             ModContent.ProjectileType<CosmicFistBarrier>(), ProjectileDamage(NPC.damage), 0f, -1, player.whoAmI, i, dist);
                        }
                    }
                }
                AITimer1 = 0;
                if (AttackCount++ >= finalAttack)
                {
                    ResetStats();
                    AttackID = GetNextAttack();
                }
            }
        }
        public void MeteorDash(Player player)
        {
            AITimer1++;
            distanceAbove = 250;
            if (AITimer2++ == 90)
            {
                AI_State = MovementState.Explode;
                float baseRotation = Main.rand.NextFloat(MathF.Tau);
                int amount = (expertMode || masterMode) ? 8 : 5;
                float dist = masterMode ? 1000 : expertMode ? 1250 : 1500;
                for (int i = 0; i < amount; i++)
                {
                    float rot = baseRotation + MathF.Tau * ((float)i / amount);
                    int damage = (int)(NPC.damage * 0.5f);
                    Vector2 vector = (Vector2.Normalize(Vector2.UnitY.RotatedBy(rot)) * dist);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicStarlitMeteorite>(), ProjectileDamage((int)(NPC.damage * 3)), 0f, Main.myPlayer, NPC.whoAmI, vector.X, vector.Y);
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
            if (AttackCount > 0)//loop
            {
                AI_State = MovementState.FollowingRegular;
                AttackID = GetNextAttack();
                ResetStats(true);
                distanceAbove = 250;
            }
        }
        public void TentacleBorder()
        {
            distanceAbove = 250;
            float tentacleDistAway = 1500;
            float sweepTime = 180;
            tentacleDistAway = masterMode ? 1200 : expertMode ? 1500 : 1800;
            sweepTime = masterMode ? 150 : expertMode ? 180 : 120;
            if (AITimer2 <= 0)
            {
                AI_State = MovementState.Teleport;
            }
            else
            {
                AITimer1++;
                if (AITimer1 == 90)
                {
                    AI_State = MovementState.Explode;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicTentacleArena>(), ProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, tentacleDistAway, sweepTime);
                    }

                }
                if (AITimer1 == 150)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicTentacleArena2>(), ProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, tentacleDistAway / 6, sweepTime / 5);
                    }
                }
                if (AITimer1 >= 400 + sweepTime)//loop
                {
                    AI_State = MovementState.FollowingRegular;
                    AttackID = GetNextAttack();
                    ResetStats(true);
                    distanceAbove = 250;
                }
            }
        }
        public void WhiteholePortal(Player player)
        {
            Vector2 eyePos = NPC.Center + new Vector2(0, -60);

            AITimer1++;
            distanceAbove = 300;
            int maxAttack = 6;
            float restTime = masterMode ? 240 : expertMode ? 320 : 450;
            if (AttackCount < maxAttack)
            {
                if (AITimer1 >= 90 && AttackCount <= 0 || AITimer1 >= restTime)
                {
                    AttackCount++;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile Blast = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos, Vector2.Zero,
                            ModContent.ProjectileType<CosmicJellyfishBlast>(), 0, 0);
                        Blast.ai[1] = 250f;
                        Blast.localAI[1] = Main.rand.NextFloat(0.15f, 0.25f);
                        Blast.netUpdate = true;
                    }

                    AITimer1 = 0;
                    AI_State = MovementState.Explode;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(player.Center.X, player.Center.Y + Main.screenHeight / 2), Vector2.Zero, ModContent.ProjectileType<CosmicSwarmBlackhole>(), ProjectileDamage(NPC.damage), 0f, Main.myPlayer, NPC.whoAmI, 0, 0);
                    }
                }
            }
            if (AITimer1 >= 60 && AITimer1 < 120 && AttackCount <= 0 || AITimer1 >= restTime - 100 && AITimer1 <= restTime - 50)
            {
                AI_State = MovementState.FollowingRegular;
            }
            if (AttackCount >= maxAttack && AITimer1 >= restTime + 50)//loop
            {
                AI_State = MovementState.FollowingRegular;
                AttackID = GetNextAttack();
                ResetStats(true);
                distanceAbove = 250;
            }
        }

        public void SwordBurstFire(Player player)
        {
            Vector2 eyePos = NPC.Center + new Vector2(0, -60);

            AITimer1++;
            float angleDeviation = 30;
            float amountFire = 20;
            float maxDeviation = 70;
            float maxAmount = 80;

            if (expertMode || masterMode)
            {
                maxDeviation = 100;
                maxAmount = 80;
            }
            else if (!expertMode || !masterMode)
            {
                maxDeviation = 70;
                maxAmount = 50;
            }
            amountFire = Math.Clamp(20 + 5 * AttackCount, 0, maxAmount);
            angleDeviation = Math.Clamp(30 + 10 * AttackCount, 0, maxDeviation);
            if (AITimer1 == 60)
            {
                AI_State = MovementState.Explode;
            }
            if (AITimer1 == 90)
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
                SoundEngine.PlaySound(SoundID.Item28, eyePos);
                SoundEngine.PlaySound(SoundID.Item20, eyePos);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), eyePos +
                        new Vector2(100, 0).RotatedBy(velocity.ToRotation()), Vector2.Zero,
                        ModContent.ProjectileType<CosmicSwordStar2>(), ProjectileDamage(NPC.damage), 0, -1, 0, 0, amountFire - (AITimer2));
                    proj.rotation = velocity.ToRotation();
                }

                AITimer2++;
                if (AITimer2 >= amountFire)
                {
                    distanceAway = 300 * (AttackCount % 2 == 0 ? 1 : -1);
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
                AttackID = GetNextAttack();
                ResetStats(true);
                distanceAbove = 250;
                distanceAway = 0;
            }
        }

        public void MeteorDeathRay(Player player)
        {
            Vector2 eyePos = NPC.Center + new Vector2(0, -60);
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
                         ModContent.ProjectileType<CosmicRayWarn>(), ProjectileDamage(NPC.damage * 2), 0f, -1, 240, NPC.whoAmI);
                    }
                }
                if (AITimer2 >= 700)
                {
                    AI_State = MovementState.FollowingRegular;
                    AttackID = GetNextAttack();
                    ResetStats();
                    NetSync();
                }
            }
            else
            {
                distanceAbove = 200;

                AI_State = MovementState.Explode;
                if (AITimer1++ >= 60)
                {
                    AttackCount++;

                    AITimer1 = 0;
                    int count = 3 + (int)AttackCount * 2;
                    float dist = 350 * AttackCount;
                    float baseRot = MathHelper.ToRadians(180 / (count));
                    SoundEngine.PlaySound(SoundID.Item28, eyePos);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            float rot = baseRot + MathF.Tau * ((float)i / count);
                            Vector2 vel = -rot.ToRotationVector2() * 14;
                            Projectile meteor = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos + new Vector2(dist, 0).RotatedBy(rot)
                                , Vector2.Zero, ModContent.ProjectileType<CosmicRayMeteorite>(), ProjectileDamage(NPC.damage), 1f, Main.myPlayer, NPC.whoAmI, 60 * AttackCount);
                        }
                    }

                    for (int i = 0; i < 18; i++)
                    {
                        int dust = Dust.NewDust(eyePos, 0, 0, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, Main.rand.NextFloat(1.25f, 2.5f));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 12;
                    }
                }
            }
        }

        public const int maxDash = 6;
        public void BlazingStar(Player player)
        {
            if (AITimer2 <= 0)
            {
                if (AttackCount >= maxDash - 2)
                {
                    distanceAbove = Main.screenHeight * (AttackCount % 2 == 0 ? 1 : -1);
                    distanceAway = 0;

                }
                else
                {
                    distanceAbove = 0;
                    distanceAway = Main.screenWidth / 2 * (AttackCount % 2 == 0 ? 1 : -1);
                }
                AI_State = MovementState.Teleport;
            }
            if (AttackCount >= maxDash)
            {
                AI_State = MovementState.FollowingRegular;
                AttackID = GetNextAttack();
                ResetStats();
                distanceAbove = 250;
                distanceAway = 0;
            }
            //to be implemented later
        }
        public void P2Transition(Player player)
        {
            Vector2 eyePos = NPC.Center + new Vector2(0, -60);
            int hitTime = 1;
            float restTime = masterMode ? 45 : expertMode ? 60 : 60;
            hitTime = masterMode ? 5 : expertMode ? 5 : 3;
/*            hitTime = 1;
*/            if (AI_State == MovementState.Teleport)
            {
                NPC.dontTakeDamage = true;
            }
            else
            {
                if (goodtransition <= 0)
                {
                    if (AITimer2 < hitTime)
                    {
                        if (AITimer1++ >= restTime)
                        {
                            AITimer2++;
                            AITimer1 = 0;
                            int count = 6 + (int)(AITimer2 * 2);
                            float dist = 100 + 350 * AITimer2;
                            float baseRot = MathHelper.ToRadians(180 / (count));
                            SoundEngine.PlaySound(SoundID.Item28, eyePos);

                            if (Main.netMode != NetmodeID.MultiplayerClient) // server-authoritative spawn
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    float rot = baseRot + MathF.Tau * ((float)i / count);
                                    Vector2 vel = -rot.ToRotationVector2() * 14;
                                    Projectile sword = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos + new Vector2(dist, 0).RotatedBy(rot), Vector2.Zero, ModContent.ProjectileType<CosmicSwordStar>(), ProjectileDamage(NPC.damage), 1f, Main.myPlayer, NPC.whoAmI, i >= (count - 1) ? 1 : 0);
                                    sword.rotation = vel.ToRotation();
                                }
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
                        if (AITimer1++ >= restTime * 3)
                        {
                            if (!masterMode && !expertMode)
                            {
                                NPC.HealEffect((int)((NPC.lifeMax / 2) - NPC.life));
                                NPC.life = (NPC.lifeMax / 2);
                            }
                            else
                            {
                                NPC.HealEffect((int)((NPC.lifeMax) - NPC.life));
                                NPC.life = (NPC.lifeMax);
                            }
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
                        if (AITimer1++ >= restTime)
                        {
                            AITimer2++;
                            AITimer1 = 0;
                            int count = 12;
                            float dist = 240 - 30 * AITimer2;
                            float baseRot = 0 + AITimer2 * MathHelper.ToRadians(15);
                            SoundEngine.PlaySound(SoundID.Item28, eyePos);

                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    float rot = baseRot + MathF.Tau * ((float)i / count);
                                    Vector2 vel = rot.ToRotationVector2() * 14;
                                    Projectile sword = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), eyePos + new Vector2(dist, 0).RotatedBy(rot),
                                        Vector2.Zero, ModContent.ProjectileType<CosmicSwordStar>(), ProjectileDamage((int)(NPC.damage * 0.75f)), 1f, Main.myPlayer, NPC.whoAmI, 0, 1);
                                    sword.rotation = vel.ToRotation();
                                }
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
                        if (AITimer1++ >= restTime * 2)
                        {
                            AI_State = MovementState.FollowingRegular;
                            AttackID = GetNextAttack();
                            ResetStats();
                            NPC.dontTakeDamage = false;
                        }
                    }
                }
            }
        }
        #endregion

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

                            if (AITimer1 <= 5)
                            {
                                dashPos = player.Center + player.velocity * 10;
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
                        NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.2f);

                        if (emitter != null)
                            emitter.keptAlive = true;
                        for (int j = 0; j < 6; j++)
                        {
                            emitter?.Emit(Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity * 0.25f, 0f, 20);
                        }
                    }
                    NetSync();
                    break;
                case MovementState.Explode:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
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
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    NPC.velocity.X *= 0.99f;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    NPC.velocity.Y += 0.5f;
                            }
                            else
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                    NPC.velocity.X *= 0.96f;
                            }
                        }
                        else
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                                NPC.velocity *= 0;
                            AITimer2++;
                        }
                    }
                    else
                    {
                        if (AITimer2++ == 2)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                //could have been a dash() except this needs a big velo boost from the start
                                dashVel = Vector2.Normalize(new Vector2(NPC.Center.X, NPC.Center.Y - 750) - new Vector2(NPC.Center.X, NPC.Center.Y)) * 18;
                                NPC.velocity = dashVel;
                                AttackCount++;
                                NPC.netUpdate = true;
                            }

                            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelSlam"), NPC.Center);
                            SludgeSlam();
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
                case MovementState.Aligning:
                    if (AITimer1++ <= Math.Max(60 - AttackCount * 10, 10))
                    {
                        if (AttackCount >= maxDash - 2)
                        {
                            NPC.Center = player.Center + new Vector2(distanceAway, distanceAbove);
                        }
                        else
                        {
                            NPC.Center = player.Center + new Vector2(distanceAway, distanceAbove - NPC.localAI[0]);
                        }
                        NPC.rotation = NPC.rotation.AngleLerp(NPC.Center.AngleTo(player.Center + new Vector2(0, 0)) + MathHelper.PiOver2, 0.1f);
                    }
                    else
                    {
                        AITimer1 = 0;
                        AI_State = MovementState.SuperDashing;
                    }
                    break;
                case MovementState.Teleport:// flash whole body, then teleport on top of player, no contact damage

                    if (NPC.HasValidTarget)
                    {
                        switch (AttackID)
                        {
                            case -1:
                                Teleport(Main.player[NPC.target].Center + new Vector2(0, -400), 60, 90, (int)AttackID);
                                break;
                            case 6:
                                Teleport(Main.player[NPC.target].Center + new Vector2(0, 250), 60, 90, (int)AttackID);
                                break;
                            case 10:
                                if (NPC.localAI[0] == 0)
                                {
                                    NPC.localAI[0] = Main.rand.NextFloat(-400, 400);
                                }
                                if (AttackCount >= maxDash - 2)
                                {
                                    Teleport(Main.player[NPC.target].Center + new Vector2(0, Main.screenHeight * (AttackCount % 2 == 0 ? 1 : -1)), 40, 80, (int)AttackID);
                                }
                                else
                                {
                                    Teleport(Main.player[NPC.target].Center + new Vector2(Main.screenWidth / 2 * (AttackCount % 2 == 0 ? 1 : -1), -NPC.localAI[0]), 40, 80, (int)AttackID);
                                }
                                NPC.velocity *= 0f;
                                break;
                        }

                    }
                    NPC.velocity *= 0.95f;
                    rotation = rotationFactor * maxRotation;
                    NPC.rotation = rotation;
                    break;
                case MovementState.SuperDashing:
                    AITimer1++;

                    if (AITimer1 <= 10)
                    {
                        dashPos = player.Center + new Vector2(0, 0);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 0.95f;
                            NetSync();
                        }
                    }
                    else
                    {
                        Dash(dashPos, 1, 28, 50, 60, 10);
                    }
                    if (DashTimer > 0)
                    {
                        hideSelf = true;
                        if (emitter != null)
                            emitter.keptAlive = true;
                        for (int j = 0; j < 6; j++)
                        {
                            emitter?.Emit(Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity * 0.25f, 0f, 20);
                        }
                        NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.2f);
                    }
                    else
                    {
                        hideSelf = false;
                    }
                    NetSync();
                    break;
            }
            teleEffect = MathF.Round(Math.Clamp(teleEffect - 0.05f, -1, 1), 2);
            intimidateMe -= 0.02f;
        }
        public void Teleport(Vector2 telePos, int time1, int time2, int attackID)
        {
            AITimer1++;

            if (AITimer1 == time1 - 30)
            {
                teleEffect = 1f;
                reappearTele = true;
            }
            else if (AITimer1 == time2 - 30)
            {
                teleEffect = 1f;
                reappearTele = false;
            }

            if (AITimer1 == time1)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.Center = telePos;
                    NetSync();
                }
            }
            else if (AITimer1 >= time2)
            {
                teleEffect = 0f;
                reappearTele = false;
                // reset reappearTele after the cycle ends so next teleport will start clean

                if (Main.netMode != NetmodeID.MultiplayerClient)
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
                            distanceAbove = 250;
                            break;
                        case 10:
                            AITimer2 = 1;
                            AI_State = MovementState.Aligning;
                            break;
                    }
                    NetSync();
                }
            }
        }

        bool shouldBlank = false;
        float blankCount = 0;
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
            switch (attackID)
            {
                case 1:
                    if (DashTimer % 10 == 0 && DashTimer > time1 && DashTimer < reset)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 vel1 = !bSecondStage ? Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2) : Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2 + MathHelper.Pi / 6) * 1.25f;
                            Vector2 vel2 = !bSecondStage ? Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2) : Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2 - MathHelper.Pi / 6) * 1.25f;
                            Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, vel1, ModContent.ProjectileType<CosmicWave>(), ProjectileDamage((int)(NPC.damage * 0.75f)), 0, Main.myPlayer);
                            Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, vel2, ModContent.ProjectileType<CosmicWave>(), ProjectileDamage((int)(NPC.damage * 0.75f)), 0, Main.myPlayer);
                            proj1.tileCollide = false;
                            proj2.tileCollide = false;
                            if (expertMode || masterMode)
                            {
                                Projectile proj3 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, -Vector2.Normalize(NPC.velocity), ModContent.ProjectileType<CosmicWave>(), ProjectileDamage(NPC.damage), 0, Main.myPlayer);
                                proj3.tileCollide = false;

                            }
                        }
                    }
                    break;
                case 2:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        //it has to spawn something wtf
                        Projectile WHATISTHIS = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center,
                            Vector2.Zero, ProjectileID.None,
                            (NPC.defDamage), 0);
                    }
                    break;
                case 10:
                    if (DashTimer == 1)
                        AIRand = 40 + Main.rand.Next(-2, 1);
                    if (DashTimer == time1 + 1 || DashTimer == AIRand - 2)
                    {
                        int ring = 128 - (int)(DashTimer * 2);
                        for (int index1 = 0; index1 < ring; ++index1)
                        {
                            Vector2 vector2 = (-Vector2.UnitY.RotatedBy(index1 * 3.14159274101257 * 2 / ring) * new Vector2(8f, 16f)).RotatedBy(NPC.velocity.ToRotation());
                            int index2 = Dust.NewDust(NPC.Center, 0, 0, ModContent.DustType<CosJelDust>(), 0.0f, 0.0f, 0, new Color(), 1f);
                            Main.dust[index2].scale = 3f;
                            Main.dust[index2].noGravity = true;
                            Main.dust[index2].position = NPC.Center;
                            Main.dust[index2].velocity = Vector2.Zero;
                            Main.dust[index2].velocity += vector2 * 1.5f * (reset - DashTimer)/reset + NPC.velocity * 0.5f;
                        }
                    }
                    if (DashTimer == AIRand)
                    {
                        shouldBlank = true;
                    }
                    if (DashTimer % 1 == 0 && DashTimer > time1 + 20 && DashTimer < reset)
                    {
                        if (shouldBlank && blankCount++ <= 2)
                        {
                        }
                        else
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 vel1 = Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2);
                                Vector2 vel2 = Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2);
                                Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, vel1, ModContent.ProjectileType<CosmicSwordStar2>(), ProjectileDamage((int)(NPC.damage * 0.75f)), 0, Main.myPlayer, 0, 0, 15);
                                Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, vel2, ModContent.ProjectileType<CosmicSwordStar2>(), ProjectileDamage((int)(NPC.damage * 0.75f)), 0, Main.myPlayer, 0, 0, 15);
                                proj1.tileCollide = false;
                                proj2.tileCollide = false;
                                proj1.rotation = vel1.ToRotation();
                                proj2.rotation = vel2.ToRotation();
                            }
                        }
                    }

            
                    break;
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
                    case 10:
                        AITimer1 = 0;
                        AITimer2 = 0;
                        NPC.localAI[0] = 0;
                        shouldBlank = false;
                        blankCount = 0;
                        AttackCount++;
                        AI_State = MovementState.Teleport;
                        distanceAbove = 300;
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
                if (AttackID > maxAttack)//use phase 2 attacks
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
                availableAttacks.AddRange(Enumerable.Range(6, maxAttack-1));
                AITimer1 = 0;
                AITimer2 = 0;
                NPC.localAI[2] = 1;
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
        float prevTeleEffect = 0f;
        float intimidateMe = 0;//actual fargo epicMe this time

        bool reappearTele = false;
        bool hideSelf = false;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 stretch = new(1f, 1f);
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
            Vector2 origin = new(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
            Rectangle frameRect = new(0, NPC.frame.Y, tex.Width, vertSize);
            Vector2 center = NPC.Size / 2f;
            Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D sDashTex = ModContent.Request<Texture2D>(Texture + "_DashGlow").Value;
            Texture2D sDashTex2 = ModContent.Request<Texture2D>(Texture + "_DashThing").Value;
            Texture2D sDashTrail = TextureAssets.Extra[ExtrasID.SharpTears].Value;

            if (DashTimer > 0)
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

            if (AI_State == MovementState.Dashing)
            {
                Shader.Apply();
                TrailStrip.PrepareStrip(dashOldPositions, dashOldRotations, StripColors, StripWidth, NPC.Size * 0.5f - Main.screenPosition, dashOldPositions.Length, true);
                TrailStrip.DrawTrail();
                Main.spriteBatch.End(out SpriteBatchData spriteBatchData); // unapply shaders
                Main.spriteBatch.Begin(spriteBatchData);
                if (DashTimer > 0)
                {
                    for (int k = 0; k < NPC.oldPos.Length; k++)
                    {
                        Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                        Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length) * 0.4f;
                        spriteBatch.Draw(tex, drawPos, frameRect, color, NPC.oldRot[k], origin, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
            Color glowColor = Color.White * 0.5f;

            if (AI_State == MovementState.SuperDashing && DashTimer > 10)
            {
                for (float i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i += 0.15f)
                {
                    Color color27 = glowColor * 0.35f;
                    Color color28 = glowColor;
                    color27 *= (float)(NPCID.Sets.TrailCacheLength[NPC.type] - i) / NPCID.Sets.TrailCacheLength[NPC.type] * 0.75f;
                    color28 *= (float)(NPCID.Sets.TrailCacheLength[NPC.type] - i) / NPCID.Sets.TrailCacheLength[NPC.type] *2f;

                    float scale = NPC.scale;
                    scale *= (float)(NPCID.Sets.TrailCacheLength[NPC.type] - i) / NPCID.Sets.TrailCacheLength[NPC.type];
                    int max0 = (int)i - 1;//Math.Max((int)i - 1, 0);
                    if (max0 < 0)
                        continue;
                    Vector2 value4 = NPC.oldPos[max0];
                    float num165 = NPC.rotation; //NPC.oldRot[max0];
                    Vector2 center2 = Vector2.Lerp(NPC.oldPos[(int)i], NPC.oldPos[max0], 1 - i % 1);
                    center2 += NPC.Size / 2;
                    Vector2 origin2 = new(sDashTex.Width / 2f, sDashTex.Height / 2f);
                    Vector2 origin3 = new(sDashTrail.Width / 2f, sDashTex.Height / 2f);
                    spriteBatch.Draw(sDashTrail, center2 - screenPos, sDashTrail.Frame(1, 1, 0, 0), new Color(255, 242, 191,50), num165, origin3, scale * new Vector2(2,5), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(sDashTex, center2 - screenPos, sDashTex.Frame(1, 1, 0, 0), color27, num165, origin2, scale * 6f, SpriteEffects.None, 0);

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

                    spriteBatch.Draw(tex, miragePos + new Vector2(0f, 8f).RotatedBy(radians) * time, frameRect, new Color(90, 70, 255, 50) * glowOpacity * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
                }

                for (float i = 0f; i < 1f; i += 0.34f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;

                    spriteBatch.Draw(tex, miragePos + new Vector2(0f, 12f).RotatedBy(radians) * time, frameRect, new Color(90, 70, 255, 50) * glowOpacity * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
                }
            }
            else
            {
                glowOpacity = MathHelper.Clamp(glowOpacity - 0.1f, 0f, 1f);
            }
            if (teleEffect > 0f)
            {
                float scale = 1f * NPC.scale * (float)Math.Cos(Math.PI / 2 * teleEffect) + 1;
                if (teleEffect <= 0.05f)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PurpleTorch, 0, 0, 0, default, Main.rand.NextFloat(1.25f, 2.5f));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 20;
                    }
                }

                float opacity = NPC.Opacity * (float)Math.Sqrt(teleEffect);
                if (!reappearTele)
                {
                    Main.EntitySpriteDraw(tex, miragePos, NPC.frame, new Color(255, 255, 255, 255) * opacity, NPC.rotation, NPC.frame.Size() / 2f, new Vector2(MathHelper.Max(scale - 1, 0.1f), 3 - scale), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(glowTexture, miragePos, NPC.frame, new Color(255, 255, 255, 255) * opacity, NPC.rotation, NPC.frame.Size() / 2f, new Vector2(MathHelper.Max(scale - 1, 0.1f), 3 - scale), SpriteEffects.None, 0);
                }
                else
                {
                    Main.EntitySpriteDraw(tex, miragePos, NPC.frame, new Color(255, 255, 255, 255) * opacity, NPC.rotation, NPC.frame.Size() / 2f, new Vector2(MathHelper.Max(2 - scale, 0.1f), scale), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(glowTexture, miragePos, NPC.frame, new Color(255, 255, 255, 255) * opacity, NPC.rotation, NPC.frame.Size() / 2f, new Vector2(MathHelper.Max(2 - scale, 0.1f), scale), SpriteEffects.None, 0);
                }
            }

            if (intimidateMe > 0)//fargo eridanus epic
            {
                float scale = 2f * NPC.scale * (float)Math.Cos(Math.PI / 2 * intimidateMe);
                float opacity = NPC.Opacity * (float)Math.Sqrt(intimidateMe);
                Main.EntitySpriteDraw(tex, miragePos, NPC.frame, Color.White * opacity, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale * scale, SpriteEffects.None, 0);
            }
            if ((teleEffect <= 0 && !reappearTele))  
            {
                Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - Main.screenPosition, NPC.frame, Color.White * NPC.Opacity, NPC.rotation, NPC.frame.Size() / 2f, stretch, SpriteEffects.None);
            }
            if (hideSelf)
            {
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0, 0.2f);
            }
            else
            {
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 1, 0.2f);
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
}