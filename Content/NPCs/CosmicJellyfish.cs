using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Physics;
using ITD.Content.Projectiles;

namespace ITD.Content.NPCs
{
    [AutoloadBossHead]
    public class CosmicJellyfish : ModNPC
    {
        public bool SecondStage
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }

        private int actionID = 0;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
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
            Main.npcFrameCount[NPC.type] = 5;
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

        Random rnd = new Random();
        public override bool PreAI()
        {
            Dust.NewDust(NPC.Center + new Vector2(rnd.Next(NPC.width) - NPC.width / 2, 0), 1, 1, DustID.ShimmerTorch, 0f, 0f, 0, default(Color), 1f);
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

            float rotation = rotationFactor * maxRotation;
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
                Random random = new Random();
                int projectileAmount = random.Next(3, 6);
                float XVeloDifference = 2f;
                float startXVelo = -((float)(projectileAmount - 1) / 2) * (float)XVeloDifference;
                for (int i = 0; i < projectileAmount; i++)
                {
                    Vector2 projectileVelo = new Vector2(startXVelo + XVeloDifference * i, -8f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity + projectileVelo, ModContent.ProjectileType<CosmicSludgeBomb>(), 0, 0, -1);
                }
            }
            if (sludgeTimer == 180)
            {
                SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                Random random = new Random();
                int projectileAmount = random.Next(5, 11);
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
                case 0:
                    if (speed > 1.1f)
                    {
                        NPC.velocity = aboveNormalized * (speed + 1f) / 20;
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                    }
                    break;
                case 1:
                    break;
            }
        }

        private void DoSecondStage(Player player)
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            return true;
        }
    }
}
