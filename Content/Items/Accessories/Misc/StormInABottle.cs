using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System;

namespace ITD.Content.Items.Accessories.Misc
{
    public class StormInABottle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Gray;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<StormDoubleJump>().hasStormJump = true;
        }

        /*
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient()
                .Register();
        }
        */

    }

    public class StormDoubleJump : ModPlayer
    {
        public const int TornadoDuration = 180;
        public const int LightningInterval = 60;
        public const float TornadoHeight = 200f;
        public const float MaxLightningDistance = 300f;

        public bool hasStormJump;
        public bool canDoubleJump;
        public bool waitDoubleJump;
        public int tornadoTimer;
        public int lightningTimer;
        public Vector2 tornadoBase;

        public override void ResetEffects()
        {
            hasStormJump = false;
        }

        public override void PreUpdate()
        {
            HandleDoubleJump();
            UpdateTimers();
        }

        public void HandleDoubleJump()
        {
            if ((Player.velocity.Y == 0f || Player.sliding || (Player.autoJump && Player.justJumped)) && hasStormJump)
            {
                canDoubleJump = true;
                waitDoubleJump = true;
            }
            else if (canDoubleJump && Player.controlJump && !waitDoubleJump)
            {
                PerformDoubleJump();
            }

            waitDoubleJump = Player.controlJump;
        }

        public void PerformDoubleJump()
        {
            if (Player.jump > 0) return;

            Player.velocity.Y = -Player.jumpSpeed * 2f * Player.gravDir;
            Player.jump = (int)(Player.jumpHeight * 2f);
            canDoubleJump = false;

            StormSFX();
            InitializeTornado();
        }

        public void InitializeTornado()
        {
            tornadoTimer = TornadoDuration;
            lightningTimer = LightningInterval;
            tornadoBase = Player.Bottom + new Vector2(0, 40);

            AdjustTornadoBaseToGround();
        }

        public void AdjustTornadoBaseToGround()
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 tilePos = tornadoBase + new Vector2(0, i * 16);
                Point tilePt = tilePos.ToTileCoordinates();
                if (Main.tile[tilePt.X, tilePt.Y].HasTile && Main.tileSolid[Main.tile[tilePt.X, tilePt.Y].TileType])
                {
                    tornadoBase = new Vector2(tornadoBase.X, tilePt.Y * 16 - 8);
                    break;
                }
            }
        }

        public void UpdateTimers()
        {
            if (tornadoTimer > 0)
            {
                tornadoTimer--;
                Tornado();

                if (lightningTimer > 0)
                {
                    lightningTimer--;
                    if (lightningTimer == 0)
                    {
                        Lightning();
                        lightningTimer = LightningInterval;
                    }
                }
            }
            else
            {
                lightningTimer = 0;
            }
        }

        public void Tornado()
        {
            float progress = (float)tornadoTimer / TornadoDuration;
            for (int i = 0; i < 20; i++)
            {
                TornadoDust(progress);
            }
        }

        public void TornadoDust(float progress)
        {
            float heightProgress = Main.rand.NextFloat();
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;

            float maxRadius = 10f * progress;
            float minRadius = 100f * progress;
            float radius = MathHelper.Lerp(maxRadius, minRadius, heightProgress);

            float height = -TornadoHeight * heightProgress * progress;

            Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, height);
            Vector2 position = tornadoBase + offset;

            Vector2 velocity = new Vector2(-offset.X * 0.05f, Main.rand.NextFloat(-1f, 1f));

            if (Main.rand.NextFloat() < 0.01f)
            {
                ElectricSpark(position, velocity, progress);
            }
            else
            {
                SmokeDust(position, velocity, progress);
            }
        }

        public void ElectricSpark(Vector2 position, Vector2 velocity, float progress)
        {
            Dust spark = Dust.NewDustPerfect(
                position,
                DustID.Electric,
                velocity * 2f,
                0,
                Color.White,
                Main.rand.NextFloat(0.5f, 1f) * progress
            );

            spark.noGravity = true;
            spark.fadeIn = 0.5f;
        }

        public void SmokeDust(Vector2 position, Vector2 velocity, float progress)
        {
            Color dustColor = Color.Lerp(Color.DarkGray, Color.Gray, Main.rand.NextFloat() * 0.3f);

            Dust dust = Dust.NewDustPerfect(
                position,
                DustID.Smoke,
                velocity,
                0,
                dustColor * progress,
                Main.rand.NextFloat(1.5f, 2.5f) * progress
            );

            dust.noGravity = true;
            dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f) * progress;
        }

        public void Lightning()
        {
            float progress = (float)tornadoTimer / TornadoDuration;
            Vector2 lightningStart = CalculateLightningStart(progress);

            NPC targetNPC = FindNearestEnemy();
            if (targetNPC != null)
            {
                LightningEffects(lightningStart, targetNPC.Center);
                DamageEnemy(targetNPC);
            }
        }

        public Vector2 CalculateLightningStart(float progress)
        {
            float currentRadius = MathHelper.Lerp(100f, 10f, 1 - progress);
            float currentHeight = TornadoHeight * progress;

            float heightProgress = Main.rand.NextFloat();
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;

            float radius = MathHelper.Lerp(currentRadius, 10f, heightProgress);
            float height = -currentHeight * heightProgress;

            Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, height);
            return tornadoBase + offset;
        }

        public void LightningEffects(Vector2 start, Vector2 end)
        {
            Vector2 direction = Vector2.Normalize(end - start);

            for (int j = 0; j < 30; j++)
            {
                Vector2 dustPos = start + direction * j * 10f;
                dustPos += Main.rand.NextVector2Circular(10f, 10f);

                Dust dust = Dust.NewDustPerfect(
                    dustPos,
                    DustID.Electric,
                    Vector2.Zero,
                    0,
                    Color.White,
                    Main.rand.NextFloat(1.5f, 2f)
                );
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
        }

        // TO-DO add custom Electric DamageClass
        public void DamageEnemy(NPC targetNPC)
        {
            int damage = 300;
            damage = (int)(damage * Player.GetDamage(DamageClass.Magic).Multiplicative);

            targetNPC.StrikeNPC(new NPC.HitInfo
            {
                Damage = damage,
                Knockback = 2f,
                HitDirection = targetNPC.Center.X < tornadoBase.X ? -1 : 1,
                Crit = false,
                DamageType = DamageClass.Magic
            });
        }

        public NPC FindNearestEnemy()
        {
            NPC nearestNPC = null;
            float nearestDistance = MaxLightningDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(npc.Center, tornadoBase);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestNPC = npc;
                    }
                }
            }

            return nearestNPC;
        }

        public void StormSFX()
        {
            string soundPath = Main.rand.Next(3) switch
            {
                0 => "ITD/Content/Sounds/StormInABottle1",
                _ => "ITD/Content/Sounds/StormInABottle2",
            };

            SoundEngine.PlaySound(new SoundStyle(soundPath), Player.position);
        }
    }
}