using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Projectiles.Hostile;
using ITD.Players;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using System.IO;
using ITD.Content.Dusts;
using ITD.Content.Items.Armor.Vanity.Masks;
using Terraria.Graphics.Effects;
using ITD.Content.Items.Accessories.Movement.Boots;
using ITD.PrimitiveDrawing;
using Terraria.Graphics.Shaders;
using ITD.Content.Items.Placeable.Furniture.Relics;
using ITD.Content.Items.Placeable.Furniture.Trophies;
using ITD.Content.Projectiles.Hostile.CosJel;


namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ITDNPC
    {
        public override Func<bool> DownedMe => () => DownedBossSystem.downedCosJel;
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
            NPCID.Sets.TrailCacheLength[Type] = 6;
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
        public ref float AITimer3 => ref NPC.localAI[2];

        //AttackID: stores current attack
        public ref float AttackID => ref NPC.ai[3];

        //AttackCount: the times an attack has been done, loops current attack until reach the set amount
        public ref float AttackCount => ref NPC.ai[0];

        //AttackTotal: stores the amount of unique attacks done, use to activate big attacks
        //every x small attack does a big attack
        public ref float AttackTotal => ref NPC.localAI[0];

        //Timer for the dash function
        public ref float DashTimer => ref NPC.localAI[1];

        //check if cosjel is dashing
        bool IsDashing;

        //dash velocity
        Vector2 dashVel;

        //store where to dash
        private Vector2 dashPos = Vector2.Zero;

        //store cosjel rotation
        public float rotation = 0f;
        
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
            Suffocate,//unused, but, trap the player inside cosjel
            Explode,//stand still
            Slamdown//slam attack, is currently dropping down
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
        //kill flag
        public override void OnKill()
        {
            DownedBossSystem.downedCosJel = true;
        }
        public override void AI()
        {
/*            Main.NewText((AI_State,AITimer1,AITimer2));*/

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }
            ITDGlobalNPC.cosjelBoss = NPC.whoAmI;
            Player player = Main.player[NPC.target];
            //start npc movement if has target
            Movement(player);
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
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
            switch ((int)AttackID)
            {
                case -2://final attack should be here
                    NPC.checkDead();
                    break;

                case -1://transition for phase 2
                    BlackholeDusting(1);
                    NPC.dontTakeDamage = true;
                    if (AITimer1++ >= 300)
                    {
                        AI_State = MovementState.FollowingRegular;
                        AttackID = Main.rand.Next(1, 4);//randomized, but not reveal new attack now
                        AITimer1 = 0;
                        NPC.dontTakeDamage = false;
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
                        AttackID = 2;//set next attack to 4(hand)
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackCount = 0;
                        distanceAbove = 250;
                    }
                    break;
                case 1: //Dash attack, spawns sideway shard
                    if (AITimer1++ == 80)
                    {
                        dashPos = player.Center;
                        if (AI_State != MovementState.Suffocate)//won't do suffocate here
                            AI_State = MovementState.Dashing;
                    }
                    if (AttackCount > 1)//loop once
                    {
                        AI_State = MovementState.FollowingRegular;
                        distanceAbove = 250;
                        ResetStats();
                        AttackID++;
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
                        AttackID = Main.rand.Next(1, 5);
                        ResetStats();
                        distanceAbove = 250;

                    }
                    break;
                case 3:
                    //clap attack, clapping makes hand spawn a save ring to stand inside
                    //when reaches max size, explodes into shard outside of ring border

                    //CLAP HAND MIGHT BE CHANGED INTO PROJECTILES INSTEAD,
                    //fist bump around cosjel instead, so the dodge is easier
                    AITimer1++;
                    if (AITimer1 < 100)
                    {
                        AI_State = MovementState.FollowingRegular;
                        NetSync();
                    }
                    if (AITimer1 == 1)
                    {
                        distanceAbove = 250;
                        NetSync();
                    }
                    if (AITimer1 == 30)
                    {
                        distanceAbove = 350;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (!HandExist(1))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                            if (!HandExist(-1))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                        }
                        NetSync();
                    }
                    if (AITimer1 == 100)
                    {
                        AI_State = MovementState.Explode;
                        HandControl(1, 1, 2, false);
                        HandControl(-1, 1, 2, false);
                        NetSync();
                    }

                    if (AttackCount > 1)
                    {

                        AI_State = MovementState.FollowingRegular;
                        AttackID = Main.rand.Next(1, 5);
                        ResetStats(true);
                        distanceAbove = 250;

                    }
            
                    break;
                case 4://set non-spell plagiarism
                    //Shoots slow moving hands that spam shard trail
                    distanceAbove = 350;
                    AITimer1++;
                    if (AITimer1 == 150)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (!HandExist(currentHand))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, currentHand);
                            HandControl(currentHand, 8, 8, false);
                        }
                        NetSync();
                    }
                    else if (AITimer1 == 300)
                    {
                        currentHand *= -1;
                        AITimer1 = 0;
                        AttackCount++;
                    }
                    if (AttackCount > 2 && !HandExist(1) && !HandExist(-1))//if hand still exists, keep attacking
                    {
                        HandControl(1, 7, 3, true);
                        HandControl(-1, 7, 3, true);
                        AI_State = MovementState.FollowingRegular;
                        AttackID = Main.rand.Next(1, 5);
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackCount = 0;
                        DashTimer = 0;
                        distanceAbove = 250;
                    }

                     break;
                    //BELOW IS NOT REWORKED YET
//------------------------------------------------------------------------------------------------------------------------
                case 5://
                    if (AITimer1++ == 60)
                    {
                        AITimer2 = 0;
                        SoundEngine.PlaySound(SoundID.Zombie101, NPC.Center);
                        if (AI_State != MovementState.Suffocate)
                            AI_State = MovementState.Dashing;
                    }
                    break;
                case 6:

                    if (AITimer1++ == 100)
                    {
                        AITimer1 = 0;

                        if (NPC.Center.X < player.Center.X)
                        {
                            HandControl(-1, 1, 4, false);
                        }
                        else
                        {
                            HandControl(1, 1, 4, false);
                        }
                    }

                    if (AITimer2++ >= 600)
                    {
                        AITimer1 = 0;
                        AITimer2 = 0;
                        AttackID++;

                        NetSync();
                        //ForceKill returns
                        HandControl(1, 7, 3, true);
                        HandControl(-1, 7, 3, true);

                    }
                    if (AITimer1 == 20)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            if (!HandExist(1))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                            if (!HandExist(-1))
                                NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                        }
                    }
                    break;

                case 7:
                    BlackholeDusting(3);
                    AI_State = MovementState.Explode;
                    AITimer1++;
                    if (AttackCount >= 30)
                    {
                        if (AITimer2++ == 60)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                                 ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 100, NPC.whoAmI);
                            }
                        }       
                        if (AITimer2 >= 800)
                        {
                            AI_State = MovementState.FollowingRegular;
                            AttackID = 0;
                            AITimer1 = 0;
                            AITimer2 = 0;
                            NetSync();
                        }
                        if (AITimer1 >= 100)
                        {
                            AITimer1 = 0;
                            SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                            //P2 stat garbage here
                            AIRand = Main.rand.Next(20, 24);
                            NetSync();
                            int projectileAmount = (int)AIRand;
                            float radius = 6.5f;
                            float sector = (float)(MathHelper.TwoPi);
                            float sectorOfSector = sector / projectileAmount;
                            float towardsAngle = toPlayer.ToRotation();
                            float startAngle = towardsAngle - sectorOfSector * (projectileAmount - 1) / 2;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                for (int i = 0; i < projectileAmount; i++)
                                {
                                    float angle = startAngle + sectorOfSector * i;
                                    Vector2 projectileVelo = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (AITimer1 >= 8)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                AIRand = Main.rand.Next(600, 800);
                                NetSync();
                                int saferange = (int)AIRand;
                                float offset = AttackCount > 0 && player.velocity != Vector2.Zero
                                    ? Main.rand.NextFloat((float)Math.PI * 2) : player.velocity.ToRotation();
                                float rotation = offset + (float)Math.PI * 2 / Main.rand.Next(10);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + saferange * Vector2.UnitX.RotatedBy(rotation), Vector2.Zero,
                                    ModContent.ProjectileType<TouhouBullet>(), 30, 0f, -1, NPC.whoAmI, 2);
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = Main.projectile[proj].Center }, Main.projectile[proj].owner);
                                AITimer1 = 0;
                            }
                        }
                    }
                    break;
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        float distanceAbove = 250f;//distance above the player
        private void ShardSlam()//slam attack, call once hit the ground
        {
            float XVeloDifference = 2;
            float startXVelo = -((float)(6) / 2) * (float)XVeloDifference;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -5f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 30, 0, -1, NPC.whoAmI);
                }
            }
        }
        //movement code
        private void Movement(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(0f, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            float speed = abovePlayer.Length() / 1.2f;//raising above player slowly
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            switch (AI_State)
            {
                case MovementState.FollowingRegular:

                    if (speed > 1.1f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity = aboveNormalized * (speed + 1f) / 20;
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
                    if (speed > 1.1f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity = aboveNormalized * (speed) / 24;
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
                    if (AITimer1 < 10)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 0.9f;
                            NetSync();
                        }
                        dashPos = player.Center;
                    }
                    else
                    {
                        Dash(dashPos, 10, 30, 80, 100, 1);
                    }
                    NPC.rotation = NPC.rotation.AngleTowards(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.2f);
                    NetSync();
                    break;
                case MovementState.Suffocate:
                    var suffer = player.GetModPlayer<ITDPlayer>();
                    NPC.velocity *= 0.6f;
                    player.velocity *= 0;
                    player.Center = NPC.Center;
                    player.AddBuff(BuffID.Suffocation, 5);
                    player.AddBuff(BuffID.Obstructed, 5);
                    if (!suffer.CosJellSuffocated)
                    {
                        AITimer1 = 0;
                        AI_State = MovementState.FollowingRegular;

                    }
                    break;

                case MovementState.Explode:
                    NPC.velocity *= 0.9f;
                    break;
                case MovementState.Slamdown:
                    //This scan for floor, then do collision, inconsistent sometimes
                    RaycastData data = Helpers.QuickRaycast(NPC.Center, NPC.velocity, (point) => { return (player.Bottom.Y >= point.ToWorldCoordinates().Y + 20); }, 2000);
                    if (AITimer2 <= 0)
                    {
                        if (NPC.Center.Distance(data.End) >= 20)
                        {
                            if (AITimer1++ >= 5)
                                NPC.velocity.Y += 0.5f;
                            else
                            {
                                NPC.velocity.X *= 0.95f;
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
/*            Main.NewText(("Dash Timer" + DashTimer));*/
            Player player = Main.player[NPC.target];
            if (DashTimer == time1)
            {
                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/NPCSounds/Bosses/CosjelDash"), NPC.Center);
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
                        Projectile proj1 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity).RotatedBy(Math.PI / 2) * 4, ModContent.ProjectileType<CosmicVoidShard>(), (NPC.defDamage), 0, Main.myPlayer);
                        Projectile proj2 = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Normalize(NPC.velocity).RotatedBy(-Math.PI / 2) * 4, ModContent.ProjectileType<CosmicVoidShard>(), (NPC.defDamage), 0, Main.myPlayer);
                        proj1.tileCollide = false;
                        proj2.tileCollide = false;

                    }
                }
            }
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
                        AITimer2 = 0;
                        AttackCount++;
                        break;
                    case 2:
                        AITimer2 = 0;
                        AI_State = MovementState.Inbetween;
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
                if (AttackID >= 8)//use phase 2 attacks
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
                HandControl(1, 7, 3, true);
                HandControl(-1, 7, 3, true);
                AttackID = -1;
                AI_State = MovementState.Explode;
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
            AttackTotal++;
            //also kill hand if called
            if (doKillHand)
            {
                HandControl(1, 7, 3, true);
                HandControl(-1, 7, 3, true);
            }
        }
        //check if cosjel is dead, if hasn't done final attack, don't kill cosjel
        public override bool CheckDead()
        {
            if (!bFinalAttack)//Subterranean Sun
            {
                AttackID = -2;
                NPC.life = NPC.lifeMax;
                NPC.dontTakeDamage = true;
                ResetStats(true);
                NetSync();
                bFinalAttack = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(), ModContent.ProjectileType<CosmicJellyfishBlackholeAura>(), 0, 0, -1, NPC.whoAmI);
                }
                AI_State = MovementState.Explode;
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
        private void HandControl(int whichHand, int attackID, int upcomingID, bool IsForceKill)
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
        }
        //find hand
        //whichHand: which hand to find
        //1 is left hand
        //-1 is right hand
        private static bool HandExist(int whichHand)
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
        }

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
            if (bSecondStage)
            {
                goodtransition = 5;
            }
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
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 stretch = new Vector2(1f, 1f);

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

            if (AttackID == 5 || AI_State == MovementState.Slamdown)
            {
                Texture2D tex = TextureAssets.Npc[NPC.type].Value;
                int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
                Rectangle frameRect = new Rectangle(0, NPC.frame.Y, tex.Width, vertSize);
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 center = NPC.Size / 2f;
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(tex, drawPos, frameRect, color, NPC.oldRot[k], origin, 1f, SpriteEffects.None, 0f);
                }
            }

            if(AttackID == -2)
            {
                default(BlackholeVertex).Draw(NPC.Center - Main.screenPosition, 1024);

            }
            Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, stretch, SpriteEffects.None);
            return false;
        }
    }
    //shader rip
    public struct BlackholeVertex
    {

        private static SimpleSquare square = new SimpleSquare();

        public void Draw(Vector2 position, float size)
        {
            GameShaders.Misc["Blackhole"].UseImage0(TextureAssets.Extra[193]);
            GameShaders.Misc["Blackhole"].UseColor(new Color(192, 59, 166));
            GameShaders.Misc["Blackhole"].UseSecondaryColor(Color.Beige);
            GameShaders.Misc["Blackhole"].Apply();
            square.Draw(position, size: new Vector2(size, size));
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }

    public struct CosmicTelegraphVertex 
    {

        private static SimpleSquare square = new SimpleSquare();

        public void Draw(Vector2 position, Vector2 size, float rotation)
        {
            GameShaders.Misc["Telegraph"].UseColor(new Color(192, 59, 166));
            GameShaders.Misc["Telegraph"].UseSecondaryColor(Color.Beige);
            GameShaders.Misc["Telegraph"].UseImage0(TextureAssets.Extra[193]);
            GameShaders.Misc["Telegraph"].UseShaderSpecificData(new Vector4(300,0,position.X,position.Y));

            GameShaders.Misc["Telegraph"].Apply();
            square.Draw(position + rotation.ToRotationVector2() * (size.X * 0.5f), Color.White, size * new Vector2(1, 1f), rotation, position + rotation.ToRotationVector2() * size.X / 2f);
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }



    }
}