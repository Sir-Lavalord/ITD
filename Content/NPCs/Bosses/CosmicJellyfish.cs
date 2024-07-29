using ITD.Content.Items.Materials;
using ITD.Content.Items.Other;
using ITD.Content.Items.PetSummons;
using ITD.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Hostile;
using ITD.Content.Items.Weapons.Melee;
using Terraria.GameContent;

namespace ITD.Content.NPCs.Bosses

{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        private readonly Asset<Texture2D> spriteBack = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/CosmicJellyfish_Back");
        private int hand = -1;
        public static int[] oneFromOptionsDrops =
        {
            ModContent.ItemType<WormholeRipper>(),
        };
        //private static List<CosmicJellyfish_Hand> hands = new List<CosmicJellyfish_Hand>();
        public float rotation = 0f;
        public float handCharge = 0f;
        public float handSling = 0f;
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        private enum States
        {
            FollowingRegular,
            Wandering,
        }
        private enum HandStates
        {
            Waiting,
            Charging,
            Slinging,
        }
        private States actionID = 0;
        private HandStates handState = HandStates.Waiting;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            Main.npcFrameCount[NPC.type] = 5;
        }

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 180;
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
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<CosmicJellyfishBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishTrophy>(), 10));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CosmicJellyfishRelic>()));
            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<CosmicJam>(), 4));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            //notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CosmicJellyfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<VoidShard>(), 1, 7, 15));
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, oneFromOptionsDrops));
        }

        public override void OnSpawn(IEntitySource source)
        {
            hand = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CosmicJellyfish_Hand>(), 1, 0.1f);
        }

        public override void OnKill()
        {
            DownedBossSystem.downedCosJel = true;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int startFrame = 0;
            int finalFrame = 4;

            if (SecondStage)
            {
                startFrame = 0;
                finalFrame = Main.npcFrameCount[NPC.type] - 1;

                if (NPC.frame.Y < startFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }

            int frameSpeed = 5;
            NPC.frameCounter += 1f;
            if (NPC.frameCounter > frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > finalFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override bool PreAI()
        {
            Dust.NewDust(NPC.Center + new Vector2(Main.rand.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, default, 1f);
            return true;
        }
        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                //flee upwards
                NPC.velocity.Y -= 0.04f;
                NPC.EncourageDespawn(10);
                return;
            }



            CheckSecondStage();

            if (SecondStage)
            {
                DoSecondStage(player);
            }
            else
            {
                DoFirstStage(player);
            }

            float maxRotation = MathHelper.Pi / 6; // Maximum rotation angle
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f); // Adjust the divisor for rotation sensitivity

            rotation = rotationFactor * maxRotation;
            NPC.rotation = rotation;
        }
        private void CheckSecondStage()
        {
            if (SecondStage)
            {
                return;
            }

            if (NPC.life * 100 / NPC.lifeMax < 50)
            {
                SecondStage = true;
                NPC.netUpdate = true;
                Main.NewText("2nd phase test", Color.White);
            }
        }

        int sludgeTimer = 0;
        private void DoFirstStage(Player player)
        {
            float distanceAbove = 275f;
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            Vector2 abovePlayer = toPlayer + new Vector2(0f, -distanceAbove);
            Vector2 aboveNormalized = Vector2.Normalize(abovePlayer);
            float speed = abovePlayer.Length();
            if (++sludgeTimer == 360)
            {
                sludgeTimer = 0;
                int projectileAmount = Main.rand.Next(3, 6);
                float XVeloDifference = 2f;
                float startXVelo = -((float)(projectileAmount - 1) / 2) * (float)XVeloDifference;
                for (int i = 0; i < projectileAmount; i++)
                {
                    Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -8f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity + projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 0, 0, -1);
                }
                if (handState == HandStates.Waiting)
                    handState = HandStates.Charging;
            }
            if (sludgeTimer == 180)
            {
                SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                int projectileAmount = Main.rand.Next(5, 11);
                float radius = 6.5f;
                float sector = MathHelper.ToRadians(80f);
                float sectorOfSector = sector / projectileAmount;
                float towardsAngle = toPlayer.ToRotation();
                float startAngle = towardsAngle - sectorOfSector * (projectileAmount - 1) / 2;
                for (int i = 0; i < projectileAmount; i++)
                {
                    float angle = startAngle + sectorOfSector * i;
                    Vector2 projectileVelo = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projectileVelo, ModContent.ProjectileType<CosmicVoidShard>(), 20, 5, -1);
                }
            }
            switch (actionID)
            {
                case States.FollowingRegular:
                    if (speed > 1.1f)
                    {
                        NPC.velocity = aboveNormalized * (speed + 1f) / 20;
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    break;
                case States.Wandering:
                    break;
            }
            HandleHand(player);
        }
        private void HandleHand(Player player)
        {
            if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                if (projectile.active && projectile.type == ModContent.ProjectileType<CosmicJellyfish_Hand>())
                {
                    if (projectile.scale < 1f)
                    {
                        projectile.scale += 0.05f;
                    }
                    projectile.timeLeft = 2;
                    Vector2 extendOut = new Vector2((float)Math.Cos(NPC.rotation), (float)Math.Sin(NPC.rotation));
                    Vector2 toPlayer = (player.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 normalCenter = NPC.Center - extendOut * 110 + new Vector2(0f, NPC.velocity.Y);
                    Vector2 chargedPosition = NPC.Center - extendOut * 110 + new Vector2(0f, NPC.velocity.Y) - toPlayer*150f;
                    projectile.rotation = NPC.rotation;
                    switch (handState)
                    {
                        case HandStates.Waiting:
                            handSling = 0f;
                            handCharge = 0f;
                            projectile.Center = Vector2.Lerp(projectile.Center, normalCenter, 0.3f);
                            break;
                        case HandStates.Charging:
                            if (handCharge < 1f)
                                handCharge += 0.02f;
                            else
                                handState = HandStates.Slinging;
                            projectile.Center = Vector2.Lerp(normalCenter, chargedPosition, handCharge);
                            break;
                        case HandStates.Slinging:
                            handCharge = 0f;
                            if (handSling < 1f)
                                handSling += 0.05f;
                            else
                                handState = HandStates.Waiting;
                            projectile.Center = Vector2.Lerp(normalCenter, player.Center, handSling);
                            break;
                    }
                }
                else
                {
                    hand = -1;
                }
            }
        }
        private void DoSecondStage(Player player)
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            spriteBatch.Draw(spriteBack.Value, NPC.Center - screenPos - new Vector2(0f, 32f), NPC.frame, Color.White, rotation, new Vector2(spriteBack.Width() / 2f, spriteBack.Height() / Main.npcFrameCount[NPC.type] / 2f), 1f, SpriteEffects.None, default);
            if (hand != -1)
            {
                Projectile projectile = Main.projectile[hand];
                Texture2D tex = TextureAssets.Projectile[projectile.type].Value;
                int vertSize = tex.Height / Main.projFrames[projectile.type];
                Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f / Main.projFrames[projectile.type]);
                Rectangle frameRect = new Rectangle(0, vertSize * projectile.frame, tex.Width, vertSize);
                if (handState == HandStates.Slinging)
                {
                    for (int k = 0; k < projectile.oldPos.Length; k++)
                    {
                        Vector2 center = projectile.Size/2f;
                        Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + center;
                        Color color = projectile.GetAlpha(drawColor) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                        spriteBatch.Draw(tex, drawPos, frameRect, color, projectile.oldRot[k], origin, projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.Draw(tex, projectile.Center - screenPos, frameRect, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}
