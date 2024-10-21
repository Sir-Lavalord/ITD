using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Items.Weapons.Mage;
using ITD.Content.Items.Weapons.Melee;
using ITD.Content.Items.Weapons.Ranger;
using ITD.Content.Items.Weapons.Summoner;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Hostile;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using SteelSeries.GameSense;
using ITD.Content.Dusts;
using ITD.Content.Items.Armor.Vanity.Masks;
using Terraria.Graphics.Effects;

namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        private readonly Asset<Texture2D> spriteBack = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/CosmicJellyfish_Back");
        private int hand = -1;
        private int hand2 = -1;

/*        public static int[] oneFromOptionsDrops =
        {
            ModContent.ItemType<WormholeRipper>(),
            ModContent.ItemType<StarlightStaff>(),
            ModContent.ItemType<Quasar>(),
            ModContent.ItemType<Fishbacker>(),
        };*/
        public float rotation = 0f;
        private Vector2 CorePos;
        //Hand rigamagig will be controlled seperately from the boss
        public bool bSecondStage;
        public bool bOkuu;
        int goodtransition;//Add to current frame for clean tentacles

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            Main.npcFrameCount[NPC.type] = 11;
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 4;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(bSecondStage);
            writer.Write(bOkuu);
            writer.Write(goodtransition);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);

        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bSecondStage = reader.ReadBoolean();
            bOkuu = reader.ReadBoolean();
/*            hand = reader.ReadInt32();
            hand2 = reader.ReadInt32();*/

            goodtransition = reader.ReadInt32();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
        }

        private enum MovementState
        {
            FollowingRegular,
            Wandering,
            Ram,
            Suffocate,
            Explode
        }
        private MovementState AI_State = MovementState.FollowingRegular;

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 200;
            NPC.damage = 8;
            NPC.defense = 5;
            NPC.lifeMax = 3000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
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
                Music = ITD.Instance.GetMusic("InterstellarInvertebrate") ?? MusicID.Boss1;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<CosmicJellyfishBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishTrophy>(), 10));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CosmicJellyfishRelic>()));
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<CosmicJam>(), 4));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 15, 30));
/*            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, oneFromOptionsDrops));
*/       }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);

            NPC.damage = (int)(NPC.damage * 0.7f);
        }
        public override void OnKill()
        {
            DownedBossSystem.downedCosJel = true;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            if (AI_State == MovementState.Suffocate)
            {
                return false;
            }
            else return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;
            if (bSecondStage)
            {
                goodtransition = 5;
            }
            if (!bOkuu)
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
        public override bool PreAI()
        {
            if (!bOkuu)
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
                BlackholeDusting();
            }
            return true;
        }
        public override void AI()
        {
            CorePos = new Vector2(NPC.Center.X, NPC.Center.Y - 100);
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];
            if (player.dead || !player.active || Main.IsItDay())
            {
                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }
            if (!bOkuu)
            {
                CheckSecondStage();
            }
            if (!SkyManager.Instance["ITD:CosjelOkuuSky"].IsActive() && bOkuu)
            {
                SkyManager.Instance.Activate("ITD:CosjelOkuuSky");
            }
            Attacks(player);
            Movement(player);
            HandControl(player);
        }
        private void CheckSecondStage()
        {
            if (bSecondStage)
            {
                if (NPC.ai[3] >= 8)
                {
                    NPC.ai[3] = 1;
                }
            }
            else
            {
                if (NPC.ai[3] >= 6)
                {
                    NPC.ai[3] = 1;
                }
            }

            if (NPC.life * 100 / NPC.lifeMax < 50)
            {
                if (!bSecondStage)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        //Explosion goes here
                        int projID = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BlankExplosion>(), NPC.damage, 0, Main.myPlayer);
                        Main.projectile[projID].scale = 3f;//300x300
                        Main.projectile[projID].hostile = true;
                        Main.projectile[projID].friendly = false;
                        Main.projectile[projID].knockBack = 3f;
                    }
                    if (Main.netMode != NetmodeID.Server)//Drop some gore when changing phase
                    {

                        bSecondStage = true;
                        NPC.localAI[0] = 0;

                        NPC.localAI[1] = 0;

                        NPC.localAI[2] = 0;
                        NPC.ai[3]++;
                        handState = HandState.ForcedKill;
                        handState2 = HandState.ForcedKill;

                    }
                    NPC.netUpdate = true;

                }
            }
            return;

        }
        //TODO: FIX CODE FOR MULTIPLAYER
        private void Attacks(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            switch (NPC.ai[3])
            {

                case 0:
                    if (NPC.localAI[1]++ >= 120)//take time to get to the player
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 1:
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] == 50)
                    {
                        int projectileAmount = Main.rand.Next(5, 8);

                        if (bSecondStage)
                        {
                            projectileAmount = Main.rand.Next(7, 10);
                        }
                        else
                        {
                            projectileAmount = Main.rand.Next(3, 6);
                        }
                        float XVeloDifference = 2f;
                        float startXVelo = -((float)(projectileAmount - 1) / 2) * (float)XVeloDifference;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < projectileAmount; i++)
                            {
                                Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -8f);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity + projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 30, 0, -1, NPC.whoAmI);
                            }
                        }
                    }
                    else if (NPC.localAI[1] == 150 || NPC.localAI[1] == 100 && bSecondStage)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                    }
                    break;
                case 2:
                    //Tempo
                    if (NPC.localAI[1]++ >= 200 || expertMode && NPC.localAI[1]++ >= 180 || masterMode && NPC.localAI[1]++ >= 150)
                    {
                        NPC.localAI[1] = 0;
                        SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                        //P2 stat garbage here
                        int projectileAmount = Main.rand.Next(20, 24);
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
                                if (bSecondStage)
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo * 0.01f, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1,0,2);
                                else
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1);
                            }
                        }
                    }
                    if (NPC.localAI[2]++ >= 400 + Main.rand.Next(-100, 150))
                    {
                        NPC.ai[3]++;

                        NPC.localAI[1] = 0;

                        NPC.localAI[2] = 0;
                    }
                    break;
                case 3:
                    if (NPC.localAI[1]++ == 60)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)//Fix later, this will do for now
                        {
                            if (!bSecondStage)
                            {
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X, NPC.Center.Y - 100), ModContent.NPCType<CosmicJellyfishMini>());
                            }
                            else
                            {
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X, NPC.Center.Y - 100), ModContent.NPCType<CosmicJellyfishMini>());
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X - 200, NPC.Center.Y + 100), ModContent.NPCType<CosmicJellyfishMini>());
                                NPC.NewNPCDirect(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X + 200, NPC.Center.Y + 100), ModContent.NPCType<CosmicJellyfishMini>());
                            }
                        }
                    }
                    else if (NPC.localAI[1] >= 300)
                    {
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.netUpdate = true;
                    }
                    break;
                case 4:
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (!bSecondStage)
                        {
                            if (NPC.localAI[1]++ >= 100)
                            {
                                NPC.ai[3]++;
                                NPC.localAI[1] = 0;
                                if (handState2 == HandState.Waiting && hand2 != -1)
                                    handState2 = HandState.Charging;
                                if (handState == HandState.Waiting && hand != -1)
                                    handState = HandState.Charging;
                            }
                            if (hand == -1 && hand2 == -1)
                            {
                                if (Main.rand.NextBool(2))
                                {
                                    if (hand == -1)
                                    {
                                        hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(),20, 0.1f);
                                    }
                                }
                                else
                                {
                                    if (hand2 == -1)
                                    {
                                        hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 20, 0.1f);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (NPC.localAI[1]++ == 100)
                            {
                                if (handState == HandState.Waiting && hand != -1)
                                    handState = HandState.Charging;
                            }

                                if (NPC.localAI[2] > 6)
                                {
                                    NPC.localAI[1] = 0;
                                    NPC.localAI[2] = 0;
                                    NPC.ai[3]++;
                                    if (handState == HandState.Waiting && hand != -1)
                                        handState = HandState.ForcedKill;
                                    if (handState2 == HandState.Waiting && hand2 != -1)
                                        handState2 = HandState.ForcedKill;
                                }
                            
                            if (hand == -1) 
                            {
                                hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 20, 0.1f);
                            }
                            if (hand2 == -1)
                            {
                                hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 20, 0.1f);
                            }
                        }
                    }
                    break;
                case 5:
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] == 60)
                    {
                        if (AI_State != MovementState.Suffocate)
                            AI_State = MovementState.Ram;
                    }
                    if (NPC.localAI[0] >= 2 && !bSecondStage || NPC.localAI[0] >= 3 && bSecondStage)
                    {
                        //can't believe i have to do this, since the checking doesn't even fucking work
                        NPC.ai[3]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[0] = 0;
                        NPC.localAI[2] = 0;
                        NPC.ai[1] = 0;

                        NPC.ai[2] = 0;


                    }
                    break;
                case 6:
                    if (NPC.localAI[1]++ >= 100)
                    {
                        NPC.localAI[1] = 0;

                        if (NPC.Center.X < player.Center.X)
                        {
                            if (handState2 == HandState.Waiting && hand2 != -1 && handState == HandState.Waiting)
                                handState2 = HandState.Charging;
                        }
                        else
                        {
                            if (handState == HandState.Waiting && hand != -1 && handState2 == HandState.Waiting)
                                handState = HandState.Charging;
                        }
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {

                                if (hand == -1)
                                {
                                    hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 30, 0.1f,-1,1,0,NPC.whoAmI);
                                }
                                if (hand2 == -1)
                                {
                                    hand2 = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 30, 0.1f, -1, 1, 0, NPC.whoAmI);
                                }
                            
                        
                    }
                    if (NPC.localAI[2]++ >= 600 + Main.rand.Next(100, 200))
                    {
                        NPC.ai[3]++;
                        NPC.localAI[0] = 0;

                        NPC.localAI[1] = 0;

                        NPC.localAI[2] = 0;
                        NPC.netUpdate = true;
                        if (hand != -1)
                            handState = HandState.ForcedKill;
                        if (hand2 != -1)
                            handState2 = HandState.ForcedKill;
                    }
                    break;
                case 7:
                    BlackholeDusting();
                    AI_State = MovementState.Explode;
                    NPC.localAI[1]++;
                    if (NPC.localAI[0] >= 30)
                    {
                        if (NPC.localAI[2]++ == 150)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    Vector2 vel = NPC.DirectionTo(player.Center) * 1f; ;
                                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel,
                                     ModContent.ProjectileType<CosmicRayWarn>(), NPC.damage, 0f, -1, 300, NPC.whoAmI);
                                }
                            }
                        }
                        if (NPC.localAI[2] >= 800)
                        {
                            AI_State = MovementState.FollowingRegular;
                            NPC.ai[3] = 0;
                            NPC.localAI[1] = 0;
                            NPC.localAI[2] = 0;
                            NPC.localAI[0] = 0;
                            NPC.netUpdate = true;
                        }
                        if (NPC.localAI[1] >= 100)
                        {
                            NPC.localAI[1] = 0;
                            SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                            //P2 stat garbage here
                            int projectileAmount = Main.rand.Next(20, 24);
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
                        if (NPC.localAI[1] >= 8)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                NPC.localAI[0]++;
                                int saferange = Main.rand.Next(600, 800);
                                float offset = NPC.localAI[0] > 0 && player.velocity != Vector2.Zero
                                    ? Main.rand.NextFloat((float)Math.PI * 2) : player.velocity.ToRotation();
                                float rotation = offset + (float)Math.PI * 2 / Main.rand.Next(10);
                                int proj = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + saferange * Vector2.UnitX.RotatedBy(rotation), Vector2.Zero,
                                    ModContent.ProjectileType<TouhouBullet>(), 30, 0f, -1, NPC.whoAmI, 2);
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = Main.projectile[proj].Center }, Main.projectile[proj].owner);
                                NPC.localAI[1] = 0;
                            }
                        }
                    }
                    break;
                case -1:
                    player.GetITDPlayer().Screenshake = 20;
                    if (++NPC.localAI[2] <= 900)
                    {
                        if (Main.expertMode || Main.masterMode)
                        {
                            if (++NPC.localAI[0] > 16)
                            {
                                NPC.localAI[0] = 0;
                                NPC.localAI[1] += (float)Math.PI / 2 / 360 * 75;
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromThis(), CorePos + 600 *
                                        Vector2.UnitX.RotatedBy(NPC.localAI[1] + Math.PI / 2 * i), Vector2.Zero, ModContent.ProjectileType<TouhouBullet>(),
                                        25, 0f, -1, NPC.whoAmI);
                                    }
                                }

                            }
                            float Range = 8000;
                            float Power = 0.125f + 1.5f * 0.125f;
                            for (int i = 0; i < Main.maxPlayers; i++)
                            {
                                float Distance = Vector2.Distance(Main.player[i].Center, CorePos);
                                if (Distance < 500 && Main.player[i].grappling[0] == -1)
                                {
                                    if (Collision.CanHit(CorePos, 1, 1, Main.player[i].Center, 1, 1))
                                    {
                                        float distanceRatio = Distance / Range;
                                        float multiplier = distanceRatio;

                                        if (Main.player[i].Center.X < CorePos.X)
                                            Main.player[i].velocity.X += Power * multiplier;
                                        else
                                            Main.player[i].velocity.X -= Power * multiplier;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile Blast = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), CorePos, Vector2.Zero,
                    ModContent.ProjectileType<CosmicLightningBlast>(), (int)(NPC.damage), 2f, player.whoAmI);
                            Blast.damage = 0;
                            Blast.ai[1] = 1000f;
                            Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
                            Blast.netUpdate = true;
                        }
                        NPC.scale *= 1.01f;
                        if (NPC.scale <= 1.5f)
                        {
                            NPC.life = 0;
                            NPC.HitEffect(0, 0);
                            NPC.checkDead();
                        }
                    }
                    break;

            }
        }
        //Kind of hardcoded but it's fine
        //Will customize more if needed
        private void BlackholeDusting()
        {
            int dustRings = 3;
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
                        int dust = Dust.NewDust(CorePos, 1, 1, ModContent.DustType<CosJelDust>());
                        Main.dust[dust].position = CorePos + dustOffset;
                        Main.dust[dust].fadeIn = 1f;
                        Main.dust[dust].velocity = Vector2.Normalize(CorePos - Main.dust[dust].position) * dustVelocity;
                        Main.dust[dust].scale = 3f - h;
                    }
                }
            }

        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {

            var suffer = target.GetModPlayer<ITDPlayer>();
            if (NPC.ai[3] == 5)//Suffocate, actual retarded attack but i'm not the boss here
            {
                AI_State = MovementState.Suffocate;
                if (!suffer.CosJellSuffocated)
                {
                    suffer.CosJellSuffocated = true;
                    suffer.CosJellEscapeCurrent = masterMode ? 12 : expertMode ? 8 : 6;
                }
            }
            else
            {
                suffer.CosJellEscapeCurrent = 0;
                suffer.CosJellSuffocated = false;
            }
        }
        private void Movement(Player player)//___________________________________________________________________________________________________________________________________________________
        {
            float distanceAbove = 250f;//True melee
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(0f, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            Vector2 dashvel;
            float speed = abovePlayer.Length() / 1.3f;//True melee
            switch (AI_State)
            {
                case MovementState.FollowingRegular:
                    if (speed > 1.1f)
                    {
                        NPC.velocity = aboveNormalized * (speed + 1f) / 20;
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    break;
                case MovementState.Wandering:
                    break;
                case MovementState.Ram:
                    NPC.localAI[2]++;
                    if (NPC.localAI[2] < 10)
                    {
                        NPC.velocity *= 0.9f;
                        NPC.netUpdate = true;
                    }
                    //very hard coded
                    if (NPC.localAI[2] == 10)//set where to dash
                    {
                        dashvel = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                        NPC.velocity = dashvel;
                        NPC.netUpdate = true;
                    }

                    if (NPC.localAI[2] > 10 && NPC.localAI[2] < 30)//xcel
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.netUpdate = true;

                            NPC.velocity *= 1.18f;
                            NPC.netUpdate = true;
                        }
                    }
                    if (NPC.localAI[2] > 50) //Decelerate 
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            NPC.velocity *= 0.96f;
                            NPC.netUpdate = true;

                        }
                    }
                    if (NPC.localAI[2] >= 80)
                    {
                        NPC.localAI[0]++;
                        NPC.localAI[1] = 0;
                        NPC.localAI[2] = 0;
                        NPC.netUpdate = true;
                        AI_State = MovementState.FollowingRegular;
                    }
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
                        NPC.localAI[1] = 0;
                        NPC.ai[3]++;
                        AI_State = MovementState.FollowingRegular;

                    }
                    break;

                case MovementState.Explode:
                    NPC.velocity *= 0.9f;
                    break;

            }
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            if (AI_State == MovementState.Ram)
            {
                Vector2 velo = Vector2.Normalize(new Vector2(player.Center.X, player.Center.Y) - new Vector2(NPC.Center.X, NPC.Center.Y));
                NPC.rotation = velo.ToRotation();
                NPC.rotation = NPC.velocity.X / 50;

            }
            else
            {
                rotation = rotationFactor * maxRotation;
                NPC.rotation = rotation;
            }
        }
        //Can't kill hand directly for some reasons
        //When this is called, kill both hands
        //hand - right
        //hand2 - left
        public override bool CheckDead()
        {
            if (!bOkuu)//Subterranean Sun
            {
                NPC.ai[3] =-1;
                NPC.localAI[0] = 0;
                NPC.localAI[1] = 0;
                NPC.localAI[2] = 0;
                NPC.life = NPC.lifeMax;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                bOkuu = true;
                handState = HandState.ForcedKill;
                handState2 = HandState.ForcedKill;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(), ModContent.ProjectileType<CosmicJellyfishBlackholeAura>(), 0, 0, -1, NPC.whoAmI);
                }
                AI_State = MovementState.Explode;
                Main.NewText("BLACKHOLE BLACKHOLE BLACKHOLE BLACKHOLE BLACKHOLE BLACKHOLE BLACHKOLE BLACKHOLE.", Color.Violet);
                return false;

            }
            return true;
        }

        //Forgive me terry
        //Netcode and hands do not work
        #region HandRigamagic
        public float handCharge = 0f;
        public float handSling = 0f;
        public float handFollowThrough = 0f;
        public float handCharge2 = 0f;
        public float handSling2 = 0f;
        public float handFollowThrough2 = 0f;
        private Vector2 handTarget2 = Vector2.Zero;

        private Vector2 handTarget = Vector2.Zero;
        private Vector2 handStatic = Vector2.Zero;
        private bool targetPicked = false;

        private enum HandState
        {
            Waiting,
            Charging,
            Slinging,
            DownToSize,
            ForcedKill,
            MeteorStrike,
            TemperTantrum
        }

        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        int iDelayTime;

        private HandState handState = HandState.Waiting;
        private HandState handState2 = HandState.Waiting;
        Vector2 vLockedIn;
        int Timer;
        int Timer2;
        bool bSmackdown;
        int iMeteorCount;
        bool bSmackdown2;
        int iMeteorCount2;
        private void HandControl(Player player)
        {
            if (!player.active || player.dead)
            {
                if (hand != -1)
                {
                    handState = HandState.ForcedKill;
                }
                if (hand2 != -1)
                {
                    handState2 = HandState.ForcedKill;
                }
            }
            iDelayTime = masterMode ? 0 : expertMode ? 4 : 6;

            if (hand != -1)
            {
                Projectile Projectile = Main.projectile[hand];
                if (Projectile.active && Projectile.type == ModContent.ProjectileType<CosmicJellyfish_Hand>())
                {
                    if (Projectile.scale < 1f)
                    {
                        Projectile.scale += 0.05f;
                    }
                    Projectile.timeLeft = 2;
                    Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                    Vector2 toPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 chargedPosition = NPC.Center - extendOut * 150 + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                    Vector2 normalCenter = NPC.Center - extendOut * 150 + new Vector2(0f, NPC.velocity.Y);
                    float targetRotation = NPC.rotation;
                    if (handState != HandState.Charging && handState != HandState.Slinging
                        && (!bSmackdown))
                    {
                        if (Projectile.frameCounter++ >= 6)
                        {
                            Projectile.frameCounter = 0;
                            Projectile.frame++;
                            if (Projectile.frame >= 4)
                            {

                                Projectile.frame = 0;
                            }
                        }
                    }
                    else if (handState == HandState.Charging)
                    {
                        Projectile.frame = 5;
                    }
                    else if (handState == HandState.Slinging || (handState == HandState.MeteorStrike && bSmackdown))
                    {
                        Projectile.frame = 6;
                    }
                    switch (handState)
                    {
                        case HandState.Waiting:
                            Projectile.ai[1] = 0;
                            iMeteorCount = 0;
                            bSmackdown = false;
                            Timer = 0;
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                            break;
                        case HandState.Charging:
                            if (handCharge < 1f)
                            {
                                handCharge += 0.04f;
                                targetRotation += handCharge;
                            }
                            else
                            {
                                    if (Timer++ == 0)
                                    {
                                        vLockedIn = player.Center;
                                    }
                                else if (Timer >= iDelayTime && bSecondStage || Timer >= 0 && !bSecondStage)
                                {
                                    Timer = 0;
                                    if (Projectile.ai[0] != 1)
                                    {
                                        handState = HandState.Slinging;
                                    }
                                    else

                                    {
                                        Projectile.localAI[2] = 0;
                                        Timer = 0;
                                        handSling = 0f;
                                        handCharge = 0f;
                                        handFollowThrough = 0f;
                                        Projectile.velocity.Y += 1.5f;
                                        bSmackdown = true;
                                        Projectile.tileCollide = true;
                                        handState = HandState.MeteorStrike;
                                    }

                                    Vector2 toTarget = (vLockedIn - Projectile.Center).SafeNormalize(Vector2.Zero);
                                        handTarget = vLockedIn + toTarget * 120f;
                                        handStatic = Projectile.Center;
                                    }
                                

                            }
                            Projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge * Math.PI));
                            break;
                        case HandState.Slinging:
                            targetPicked = false;
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
                                    if (NPC.localAI[2]++ <= 6 && bSecondStage)
                                    {
                                        if (handState2 == HandState.Waiting && hand2 != -1)
                                            handState2 = HandState.Charging;
                                    }
                                    if (NPC.localAI[2] > 6 || !bSecondStage)
                                    {
                                        Projectile.Kill();
                                    }
                                    handState = HandState.Waiting;
                                }
                            }
                            Projectile.Center = Vector2.Lerp(normalCenter, handTarget, (float)Math.Sin(handSling * Math.PI));
                            break;
                        case HandState.DownToSize:
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                            if (Timer++ >= 30)
                            {
                                if (NPC.ai[3] != 2)
                                {
                                    if (bSecondStage)
                                    {
                                        if (handState2 == HandState.Waiting && hand2 != -1)
                                            handState2 = HandState.Charging;
                                        handState = HandState.Waiting;
                                    }
                                    else
                                    {
                                        handState = HandState.Waiting;
                                        Projectile.Kill();
                                    }
                                }
                                else
                                {
                                    handState = HandState.Waiting;
                                    Projectile.Kill();
                                }

                            }
                            break;
                        case HandState.ForcedKill:
                            handState = HandState.Waiting;
                            Projectile.Kill();
                            Timer = 0;
                            handSling = 0f;
                            handCharge = 0f;
                            handFollowThrough = 0f;
                            Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);
                            break;
                        case HandState.MeteorStrike:

                            if (Projectile.ai[1] == 1)
                            {
                                player.GetITDPlayer().Screenshake = 10;
                                if (iMeteorCount <= 25)
                                {
                                    if (Timer++ >= 1)
                                    {

                                        Timer = 0;
                                        iMeteorCount++;

                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                        {
                                            for (int f = 0; f < 1; f++)
                                            {
                                                SoundEngine.PlaySound(SoundID.Item88, player.Center);
                                                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), new Vector2(vLockedIn.X + (Main.rand.Next(-40, 40)), player.Center.Y - 500)
                                                    , new Vector2(0, 6).RotatedByRandom(0.01) * Main.rand.NextFloat(0.75f, 1.1f), Main.rand.Next(424, 427), Projectile.damage, Projectile.knockBack, player.whoAmI);
                                                Main.projectile[proj].hostile = true;
                                                Main.projectile[proj].friendly = false;
                                                Main.projectile[proj].scale = Main.rand.NextFloat(2, 3f);

                                            }
                                        }
                                    }
                                    }
                                    else
                                {
                                    Projectile.ai[1] = 0;
                                    bSmackdown = true;
                                    iMeteorCount = 0;
                                    Timer = 0;
                                    handState = HandState.Waiting;

                                }
                            }

                            else
                            {

                                vLockedIn.X = player.Center.X + (player.velocity.X * 35f);
                                Timer = 0;
                                handSling = 0f;
                                handCharge = 0f;
                                handFollowThrough = 0f;
                                Projectile.velocity.Y += 1.5f;
                                iMeteorCount = 0;
                                bSmackdown = true;

                            }


                            break;
                    }
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile other = Main.projectile[i];

                        if (i != Projectile.whoAmI && other.active && other.type == ProjectileID.CopperShortswordStab && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                        {
                            if (Timer == 0 && handState == HandState.Slinging && handFollowThrough < 1f &&
                                player.Distance(Projectile.Center) < 60f
                                )
                            {
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = other.Center }, Projectile.owner);
                                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), Projectile.Center);
                                player.GetITDPlayer().Screenshake = 20;
                                handState = HandState.DownToSize;

                                if (NPC.life > NPC.lifeMax / 10)
                                {
                                    NPC.life -= NPC.lifeMax / 10;
                                }
                                CombatText.NewText(Projectile.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                                Projectile.velocity = -Projectile.velocity * 2;
                                // if the achievements mod is on, unlock the parry achievement
                                ITD.Instance.achievements?.Call("Event", "ParryCosJelHand");
                            }
                        }
                    }
                }
                else
                {
                    hand = -1;
                }
            }
            if (hand2 != -1)
            {
                Projectile Projectile2 = Main.projectile[hand2];
                if (Projectile2.active && Projectile2.type == ModContent.ProjectileType<CosmicJellyfish_Hand>())
                {
                    if (Projectile2.scale < 1f)
                    {
                        Projectile2.scale += 0.05f;
                    }
                    Projectile2.timeLeft = 2;
                    Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                    Vector2 toPlayer = (player.Center - Projectile2.Center).SafeNormalize(Vector2.Zero);
                    Vector2 chargedPosition = NPC.Center - extendOut * -150 + new Vector2(0f, NPC.velocity.Y) - toPlayer * 150f;
                    Vector2 normalCenter = NPC.Center - extendOut * -150 + new Vector2(0f, NPC.velocity.Y);
                    float targetRotation = NPC.rotation;
                    if (handState2 != HandState.Charging && handState2 != HandState.Slinging
                        && (!bSmackdown2))
                    {
                        if (Projectile2.frameCounter++ >= 6)
                        {
                            Projectile2.frameCounter = 0;
                            Projectile2.frame++;
                            if (Projectile2.frame >= 4)
                            {

                                Projectile2.frame = 0;
                            }
                        }
                    }
                    else if (handState2 == HandState.Charging)
                    {
                        Projectile2.frame = 5;
                    }
                    else if (handState2 == HandState.Slinging || (handState2 == HandState.MeteorStrike && bSmackdown2))
                    {
                        Projectile2.frame = 6;
                    }
                    switch (handState2)
                    {
                        case HandState.Waiting:
                            Projectile2.ai[1] = 0;
                            iMeteorCount2 = 0;
                            bSmackdown2 = false;
                            Timer2 = 0;
                            handSling2 = 0f;
                            handCharge2 = 0f;
                            handFollowThrough2 = 0f;
                            Projectile2.Center = Vector2.Lerp(Projectile2.Center, normalCenter, 0.3f);
                            break;
                        case HandState.Charging:
                            if (handCharge2 < 1f)
                            {
                                handCharge2 += 0.04f;
                                targetRotation += handCharge2;
                            }
                            else
                            {
                                if (Timer2++ == 0)
                                {
                                    vLockedIn = player.Center;
                                }
                                else if (Timer2 >= iDelayTime && bSecondStage || Timer2 >= 0 && !bSecondStage)
                                {
                                    Timer2 = 0;
                                    if (Projectile2.ai[0] != 1)
                                    {
                                        handState2 = HandState.Slinging;
                                    }
                                    else
                                    {
                                        Projectile2.tileCollide = true;
                                        Projectile2.localAI[2] = 0;
                                        Timer2 = 0;
                                        handSling2 = 0f;
                                        handCharge2 = 0f;
                                        handFollowThrough2 = 0f;
                                        Projectile2.velocity.Y += 1.5f;
                                        bSmackdown2 = true;
                                        handState2 = HandState.MeteorStrike;
                                    }
                                    Vector2 toTarget = (vLockedIn - Projectile2.Center).SafeNormalize(Vector2.Zero);
                                    handTarget2 = vLockedIn + toTarget * 120f;
                                    handStatic = Projectile2.Center;
                                }
                            }
                            Projectile2.Center = Vector2.Lerp(normalCenter, chargedPosition, (float)Math.Sin(handCharge2 * Math.PI));
                            break;
                        case HandState.Slinging:
                            targetPicked = false;
                            handCharge2 = 0f;
                            if (handSling2 < 1f)
                            {
                                handSling2 += 0.03f;
                                targetRotation -= handSling2;
                            }
                            else
                            {
                                if (handFollowThrough2 < 1f)
                                {
                                    handFollowThrough2 += 0.1f;
                                }
                                else
                                {
                                    if (NPC.localAI[2]++ <= 6 && bSecondStage)
                                    {
                                        if (handState == HandState.Waiting && hand != -1)
                                            handState = HandState.Charging;
                                    }
                                    if (NPC.localAI[2] > 6 || !bSecondStage)
                                    {
                                        Projectile2.Kill();
                                    }
                                    handState2 = HandState.Waiting;
                                }

                            }
                            Projectile2.Center = Vector2.Lerp(normalCenter, handTarget2, (float)Math.Sin(handSling2 * Math.PI));
                            break;
                        case HandState.DownToSize:
                            handSling2 = 0f;
                            handCharge2 = 0f;
                            handFollowThrough2 = 0f;
                            Projectile2.Center = Vector2.Lerp(Projectile2.Center, normalCenter, 0.3f);
                            if (Timer2++ >= 30)
                            {
                                if (NPC.ai[3] != 2)
                                {
                                    if (bSecondStage)
                                    {
                                       handState = HandState.Charging;
                                        handState2 = HandState.Waiting;
                                    }
                                    else
                                    {
                                        handState2 = HandState.Waiting;
                                        Projectile2.Kill();
                                    }
                                }
                                else
                                {
                                    handState2 = HandState.Waiting;
                                    Projectile2.Kill();
                                }

                            }
                            break;
                        case HandState.ForcedKill:
                            handState2 = HandState.Waiting;
                            Projectile2.Kill();
                            Projectile2.localAI[0] = 0;
                            handSling2 = 0f;
                            handCharge2 = 0f;
                            handFollowThrough2 = 0f;
                            Projectile2.Center = Vector2.Lerp(Projectile2.Center, normalCenter, 0.3f);
                            break;
                        case HandState.MeteorStrike:
                            if (Projectile2.ai[1] == 1)
                            {
                                player.GetITDPlayer().Screenshake = 10;
                                if (iMeteorCount2 <= 25)
                                {
                                    if (Timer2++ >= 1)
                                    {
                                        Timer2 = 0;
                                        iMeteorCount2++;
                                        if (Main.netMode != NetmodeID.MultiplayerClient)
                                        {

                                            for (int f = 0; f < 2; f++)
                                            {
                                                SoundEngine.PlaySound(SoundID.Item88, player.Center);
                                                int proj = Projectile.NewProjectile(Projectile2.GetSource_FromThis(), new Vector2(vLockedIn.X + Main.rand.Next(-40, 40), player.Center.Y - 500)
                                                    , new Vector2(0, 6).RotatedByRandom(0.01) * Main.rand.NextFloat(0.65f, 1.1f), Main.rand.Next(424, 427), Projectile2.damage, Projectile2.knockBack, player.whoAmI);
                                                Main.projectile[proj].hostile = true;
                                                Main.projectile[proj].friendly = false;
                                                Main.projectile[proj].scale = Main.rand.NextFloat(2,3f);


                                            }
                                        }
                                        
                                    }


                                }
                                else
                                {
                                    Projectile2.ai[1] = 0;
                                    bSmackdown2 = false;
                                    iMeteorCount2 = 0;
                                    Timer2 = 0;
                                    handState2 = HandState.Waiting;
                                }
                            }
                            else
                            {

                                vLockedIn.X = player.Center.X + (player.velocity.X * 35f);
                                iMeteorCount2 = 0;
                                Timer2 = 0;
                                handSling2 = 0f;
                                handCharge2 = 0f;
                                handFollowThrough2 = 0f;
                                Projectile2.velocity.Y += 1.5f;
                                bSmackdown2 = true;
                            }


                            break;
                    }
                
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile other = Main.projectile[i];

                        if (i != Projectile2.whoAmI && other.active && other.type == ProjectileID.CopperShortswordStab && Math.Abs(Projectile2.position.X - other.position.X) + Math.Abs(Projectile2.position.Y - other.position.Y) < Projectile2.width)
                        {
                            if (Projectile2.localAI[0] == 0 && handState == HandState.Slinging && handFollowThrough < 1f &&
                                player.Distance(Projectile2.Center) < 60f
                                )
                            {
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur,
                                    new ParticleOrchestraSettings { PositionInWorld = other.Center }, Projectile2.owner);
                                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), Projectile2.Center);
                                player.GetITDPlayer().Screenshake = 20;

                                if (NPC.life > NPC.lifeMax / 10)
                                {
                                    NPC.life -= NPC.lifeMax / 10;
                                }
                                handState2 = HandState.DownToSize;
                                CombatText.NewText(Projectile2.Hitbox, Color.Violet, "DOWN TO SIZE", true);
                                Projectile2.velocity = -Projectile2.velocity * 2;
                                // if the achievements mod is on, unlock the parry achievement
                                ITD.Instance.achievements?.Call("Event", "ParryCosJelHand");
                            }
                        }
                    }

                }
                else
                {
                    hand2 = -1;
                }
            }

        }

        #endregion


        public override void DrawBehind(int index)
        {
            if (bOkuu)
            {
                Main.instance.DrawCacheNPCsOverPlayers.Add(index);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AI_State == MovementState.Ram)
            {
                Texture2D tex = TextureAssets.Npc[NPC.type].Value;
                int vertSize = tex.Height / Main.npcFrameCount[NPC.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * NPC.frame.Y, tex.Width, vertSize);
                if (handState2 == HandState.Slinging || bSmackdown)
                {
                    for (int k = 0; k < NPC.oldPos.Length; k++)
                    {
                        Vector2 center = NPC.Size / 2f;
                        Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                        Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                        spriteBatch.Draw(tex, drawPos, frameRect, color, NPC.oldRot[k], origin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
                    }
                }
            }
            if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
                int vertSize = tex.Height / Main.projFrames[projectile.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.projFrames[projectile.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * projectile.frame, tex.Width, vertSize);
                if (handState == HandState.Slinging || bSmackdown)
                {
                    for (int k = 0; k < projectile.oldPos.Length; k++)
                    {
                        Vector2 center = projectile.Size / 2f;
                        Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + center;
                        Color color = projectile.GetAlpha(drawColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                        spriteBatch.Draw(tex, drawPos, frameRect, color, projectile.oldRot[k], origin, projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.Draw(tex, projectile.Center - screenPos, frameRect, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            if (hand2 != -1)
            {
                Projectile projectile = Main.projectile[hand2];
                Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
                int vertSize = tex.Height / Main.projFrames[projectile.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.projFrames[projectile.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * projectile.frame, tex.Width, vertSize);
                if (handState2 == HandState.Slinging || bSmackdown)
                {
                    for (int k = 0; k < projectile.oldPos.Length; k++)
                    {
                        Vector2 center = projectile.Size / 2f;
                        Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + center;
                        Color color = projectile.GetAlpha(drawColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                        spriteBatch.Draw(tex, drawPos, frameRect, color, projectile.oldRot[k], origin, projectile.scale, SpriteEffects.FlipHorizontally, 0f);
                    }
                }
                spriteBatch.Draw(tex, projectile.Center - screenPos, frameRect, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            spriteBatch.Draw(spriteBack.Value, NPC.Center - screenPos - new Vector2(0f, 46f), NPC.frame, Color.White, NPC.rotation, new Vector2(spriteBack.Width() / 2f, spriteBack.Height() / (Main.npcFrameCount[NPC.type] -1) / 2f), 1f, SpriteEffects.None, default);
            return true;
        }
    }
}
