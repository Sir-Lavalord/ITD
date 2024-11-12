using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Hostile;
using ITD.Content.NPCs.Bosses;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using ITD.Content.Dusts;
using ITD.Content.Items.Armor.Vanity.Masks;
using Terraria.Graphics.Effects;
using ITD.Particles.CosJel;
using ITD.Particles;
using Terraria.UI.Chat;
using Terraria.Chat;
using Terraria.Localization;
using ITD.Content.Items.Accessories.Movement.Boots;
using ITD.Content.NPCs.Bosses;

namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        public float rotation = 0f;
        public float AIRand = 0f;
        public bool bOkuu;
        int goodtransition;//Add to current frame for clean tentacles
        public int attackCount;
        public override void SetStaticDefaults()
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
            writer.Write(bOkuu);
            writer.Write(goodtransition);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(AIRand);

        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            bOkuu = reader.ReadBoolean();

            goodtransition = reader.ReadInt32();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            AIRand = reader.ReadSingle();
        }
        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 252;
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
            notExpertRule.OnSuccess(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<GravityBoots>(), 10));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<StarlitOre>(), 1, 15, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.Star, 1, 10, 20));


        }
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);

            NPC.damage = (int)(NPC.damage * 0.7f);
        }
        public override void OnKill()
        {
            DownedBossSystem.downedCosJel = true;
        }
        public override void AI()
        {
            ITDGlobalNPC.cosjelBoss = NPC.whoAmI;
            if (NPC.localAI[3] == 0) //just spawned
            {
                if (!NPC.HasValidTarget)
                    NPC.TargetClosest(false);

                if (NPC.ai[1] == 0)
                {
                    NPC.Center = Main.player[NPC.target].Center + new Vector2(500 * Math.Sign(NPC.Center.X - Main.player[NPC.target].Center.X), -250);
                }

                if (++NPC.ai[1] > 6 * 9) //nice
                {
                    NPC.localAI[3] = 1;
                    NPC.ai[1] = 0;
                    NPC.netUpdate = true;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        const int max = 8;
                        const float baseRotation = MathHelper.TwoPi / max;
                        for (int i = 0; i < max; i++)
                        {
                            float rotation = baseRotation * (i + Main.rand.NextFloat(-0.5f, 0.5f));
                        }

                        NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, 1);
                        NPCHelpers.NewNPCEasy(NPC.GetSource_FromAI(), NPC.Center, ModContent.NPCType<CosmicJellyfishHand>(), NPC.whoAmI, 0, 0, NPC.whoAmI, -1);
                    }
                }
                return;
            }
            Player player = Main.player[NPC.target];
            Vector2 targetPos;

            NPC.dontTakeDamage = false;
            NPC.alpha = 0;

            switch ((int)NPC.ai[0])
            {
                case -1:
                    NPC.localAI[2] = 1;

                    //NPC.dontTakeDamage = true;
                    NPC.ai[1]++;

                    NPC.velocity *= 0.95f;

                    /*if (NPC.ai[1] < 120)
                    {
                        targetPos = player.Center;
                        targetPos.Y -= 375;
                        if (NPC.Distance(targetPos) > 50)
                            Movement(targetPos, 0.6f, 24f, true);
                    }
                    else*/
                    if (NPC.ai[1] == 120) //begin healing
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath10, NPC.Center);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            //Projectile.NewProjectile(npc.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Projectiles.GlowRing>(), 0, 0f, Main.myPlayer, NPC.whoAmI, -3);


                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                const int max = 8;
                                float baseRotation = MathHelper.TwoPi / max * Main.rand.NextFloat();
                                for (int i = 0; i < max; i++)
                                {
                                    float rotation = baseRotation + MathHelper.TwoPi / max * (i + Main.rand.NextFloat(-0.5f, 0.5f));
                                }
                            }
                        }
                    }
                    else if (NPC.ai[1] > 120) //healing
                    {
                        NPC.velocity *= 0.9f;

                        int heal = (int)(NPC.lifeMax / 3 / 120 * Main.rand.NextFloat(1f, 1.5f));
                        NPC.life += heal;
                        int maxLife = NPC.lifeMax;
                        if (NPC.life > maxLife)
                            NPC.life = maxLife;
                        CombatText.NewText(NPC.Hitbox, CombatText.HealLife, heal);

                        for (int i = 0; i < 5; i++)
                        {
                            int d = Dust.NewDust(NPC.Center, 0, 0, DustID.InfernoFork, 0f, 0f, 0, default, 1.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity *= 8f;
                        }

                        if (NPC.ai[1] > 240)
                        {
                            NPC.ai[0]++;
                            NPC.ai[1] = 0;
                            NPC.ai[2] = 0;
                            NPC.netUpdate = true;
                        }
                    }
                    break;

                case 0: //float over player
                    if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 2500f
                        || !player.ZoneUnderworldHeight) //despawn code
                    {
                        NPC.TargetClosest(false);
                        if (NPC.timeLeft > 30)
                            NPC.timeLeft = 30;

                        NPC.noTileCollide = true;
                        NPC.noGravity = true;
                        NPC.velocity.Y += 1f;

                        return;
                    }
                    else
                    {
                        targetPos = player.Center;
                        targetPos.Y -= 325;
                        if (NPC.Distance(targetPos) > 50)
                            Movement(targetPos, 0.4f, 16f, true);
                    }

                    if (NPC.localAI[2] == 0 && NPC.life < NPC.lifeMax / 3)
                    {
                        NPC.ai[0] = -1;
                        NPC.ai[1] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;

                        for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
                        {
                            if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>() && Main.npc[i].ai[2] == NPC.whoAmI)
                            {
                                Main.npc[i].ai[0] = -1;
                                Main.npc[i].ai[1] = 0;
                                Main.npc[i].localAI[0] = 0;
                                Main.npc[i].localAI[1] = 0;
                                Main.npc[i].netUpdate = true;
                            }
                        }
                    }
                    break;

                case 1: //fireballs
                    if (!player.active || player.dead || Vector2.Distance(NPC.Center, player.Center) > 2500f
                        || !player.ZoneUnderworldHeight) //despawn code
                    {
                        NPC.TargetClosest(false);
                        if (NPC.timeLeft > 30)
                            NPC.timeLeft = 30;

                        NPC.noTileCollide = true;
                        NPC.noGravity = true;
                        NPC.velocity.Y += 1f;

                        return;
                    }
                    else
                    {
                        targetPos = player.Center;
                        for (int i = 0; i < 22; i++) //collision check above player's head
                        {
                            targetPos.Y -= 16;
                            Tile tile = Framing.GetTileSafely(targetPos); //if solid, stay below it
                            if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                            {
                                targetPos.Y += 50 + 16;
                                break;
                            }
                        }

                        if (NPC.Distance(targetPos) > 50)
                        {
                            Movement(targetPos, 0.2f, 12f, true);
                            NPC.position += (targetPos - NPC.Center) / 30;
                        }

                        if (--NPC.ai[2] < 0)
                        {
                            NPC.ai[2] = 75;
                            SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
                            if (NPC.ai[1] > 10 && Main.netMode != NetmodeID.MultiplayerClient) //shoot spread of fireballs, but not the first time
                            {
                                for (int i = -1; i <= 1; i++)
                                {
                                    
                                }
                            }
                        }

                        if (++NPC.ai[1] > 480)
                        {
                            NPC.ai[0]++;
                            NPC.ai[1] = 0;
                            NPC.netUpdate = true;
                        }
                    }

                    if (NPC.localAI[2] == 0 && NPC.life < NPC.lifeMax / 3)
                    {
                        NPC.ai[0] = -1;
                        NPC.ai[1] = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;

                        for (int i = 0; i < Main.maxNPCs; i++) //find hands, update
                        {
                            if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<CosmicJellyfishHand>() && Main.npc[i].ai[2] == NPC.whoAmI)
                            {
                                Main.npc[i].ai[0] = -1;
                                Main.npc[i].ai[1] = 0;
                                Main.npc[i].localAI[0] = 0;
                                Main.npc[i].localAI[1] = 0;
                                Main.npc[i].netUpdate = true;
                            }
                        }
                    }
                    break;

                default:
                    NPC.ai[0] = 0;
                    goto case 0;
            }
        }

        private void Movement(Vector2 targetPos, float speedModifier, float cap = 12f, bool fastY = false)
        {
            if (NPC.Center.X < targetPos.X)
            {
                NPC.velocity.X += speedModifier;
                if (NPC.velocity.X < 0)
                    NPC.velocity.X += speedModifier * 2;
            }
            else
            {
                NPC.velocity.X -= speedModifier;
                if (NPC.velocity.X > 0)
                    NPC.velocity.X -= speedModifier * 2;
            }
            if (NPC.Center.Y < targetPos.Y)
            {
                NPC.velocity.Y += fastY ? speedModifier * 2 : speedModifier;
                if (NPC.velocity.Y < 0)
                    NPC.velocity.Y += speedModifier * 2;
            }
            else
            {
                NPC.velocity.Y -= fastY ? speedModifier * 2 : speedModifier;
                if (NPC.velocity.Y > 0)
                    NPC.velocity.Y -= speedModifier * 2;
            }
            if (Math.Abs(NPC.velocity.X) > cap)
                NPC.velocity.X = cap * Math.Sign(NPC.velocity.X);
            if (Math.Abs(NPC.velocity.Y) > cap)
                NPC.velocity.Y = cap * Math.Sign(NPC.velocity.Y);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = 0;
            switch ((int)NPC.ai[0])
            {
                case -1:
                    if (NPC.ai[1] > 120)
                        NPC.frame.Y = frameHeight;
                    break;

                case 1:
                    if (NPC.ai[2] < 20)
                        NPC.frame.Y = frameHeight;
                    break;

                default:
                    break;
            }
        }
    }
}