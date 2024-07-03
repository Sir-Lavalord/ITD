using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System;

namespace ITD.Content.Items.Accessories.Misc
{
    public class FirestormInABottle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FireDoubleJump>().hasFireJump = true;
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

    public class FireDoubleJump : ModPlayer
    {
        public const int FireCloudDuration = 180;
        public const int FireTrailDuration = 60;
        public const int RainDuration = 120;
        public const int FireRainLifetime = 180;

        public bool hasFireJump;
        public bool canDoubleJump;
        public bool waitDoubleJump;
        public int fireCloudTimer;
        public int fireTrailTimer;
        public int rainTimer;
        public int damageTimer;
        public Vector2 jumpPosition;

        public override void ResetEffects()
        {
            hasFireJump = false;
        }

        public override void PreUpdate()
        {
            HandleDoubleJump();
            UpdateTimers();
        }

        public override void PostUpdate()
        {
            UpdateFireRain();
        }

        public void HandleDoubleJump()
        {
            if ((Player.velocity.Y == 0f || Player.sliding || (Player.autoJump && Player.justJumped)) && hasFireJump)
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

            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.jump = (int)Player.jumpHeight;
            canDoubleJump = false;

            FirestormSFX();

            fireCloudTimer = FireCloudDuration;
            fireTrailTimer = FireTrailDuration;
            rainTimer = RainDuration;

            damageTimer = 30;

            jumpPosition = Player.Bottom;
        }

        public void UpdateTimers()
        {
            if (fireCloudTimer > 0)
            {
                fireCloudTimer--;
                FireCloud();
            }

            if (fireTrailTimer > 0)
            {
                fireTrailTimer--;
                FireTrail();
            }

            if (rainTimer > 0)
            {
                rainTimer--;
                FireRain();
            }

            if (damageTimer > 0)
            {
                damageTimer--;
            }
        }

        public void FireCloud()
        {
            float progress = (float)fireCloudTimer / FireCloudDuration;
            for (int i = 0; i < 10; i++)
            {
                FireCloudDust(progress);
            }
            if (damageTimer <= 0)
            {
                DamageEnemy(jumpPosition, true);
                damageTimer = 30;
            }
            else
            {
                DamageEnemy(jumpPosition, false);
            }
        }

        public void FireCloudDust(float progress)
        {
            Vector2 speed = Main.rand.NextVector2Circular(5f * progress, 5f * progress);
            speed.Y -= Main.rand.NextFloat(2f * progress, 8f * progress);

            int dustType = Main.rand.Next(3) switch
            {
                0 => DustID.Torch,
                1 => DustID.Smoke,
                _ => DustID.Cloud
            };

            Dust dust = Dust.NewDustPerfect(
                jumpPosition + new Vector2(Main.rand.Next(-50, 51), Main.rand.Next(-20, 21)),
                dustType,
                speed,
                100,
                Color.OrangeRed * progress,
                Main.rand.NextFloat(1f, 2.5f) * progress
            );

            dust.noGravity = true;
            dust.fadeIn = 1f * progress;

            if (dustType == DustID.Cloud)
            {
                dust.color = Color.Lerp(Color.OrangeRed, Color.White, Main.rand.NextFloat(0.5f));
            }
        }

        public void FireTrail()
        {
            for (int i = 0; i < 3; i++)
            {
                FireTrailDust();
            }
            DamageEnemy(Player.Bottom, false);
        }

        public void FireTrailDust()
        {
            Vector2 speed = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));

            Dust dust = Dust.NewDustPerfect(
                Player.Bottom + new Vector2(Main.rand.Next(-10, 11), 0),
                DustID.Torch,
                speed,
                100,
                Color.OrangeRed,
                Main.rand.NextFloat(0.5f, 1f)
            );

            dust.noGravity = true;
            dust.fadeIn = 0.5f;
            dust.color = Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.5f));
        }

        public void FireRain()
        {
            float progress = (float)rainTimer / RainDuration;
            for (int i = 0; i < 3; i++)
            {
                FireRainDust(progress);
            }
        }

        public void FireRainDust(float progress)
        {
            Vector2 rainPosition = jumpPosition + new Vector2(Main.rand.Next(-50, 51), Main.rand.Next(-20, 0));
            Vector2 rainVelocity = new Vector2(Main.windSpeedCurrent * 0.3f, Main.rand.Next(2, 4));

            Dust dust = Dust.NewDustPerfect(
                rainPosition,
                DustID.Torch,
                rainVelocity,
                0,
                Color.OrangeRed * progress,
                Main.rand.NextFloat(0.5f, 1.5f) * progress
            );

            dust.noGravity = false;
            dust.fadeIn = 0.5f;
            dust.color = Color.Lerp(Color.OrangeRed, Color.Yellow, Main.rand.NextFloat(0.5f));

            dust.customData = new FireRainData { InitialPosition = rainPosition, CreationTime = Main.GameUpdateCount };
        }

        public void UpdateFireRain()
        {
            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (dust.active && dust.type == DustID.Torch && dust.customData is FireRainData data)
                {
                    if (Main.GameUpdateCount - data.CreationTime > FireRainLifetime)
                    {
                        dust.active = false;
                        continue;
                    }

                    Vector2 nextPosition = dust.position + dust.velocity;
                    if (IsTileCollision(nextPosition))
                    {
                        dust.scale *= 0.9f;
                        dust.velocity = Vector2.Zero;
                        if (dust.scale < 0.1f)
                        {
                            dust.active = false;
                        }
                    }

                    if (dust.position.Y > data.InitialPosition.Y + 300)
                    {
                        dust.active = false;
                    }
                }
            }
        }

        public bool IsTileCollision(Vector2 position)
        {
            int tileX = (int)(position.X / 16f);
            int tileY = (int)(position.Y / 16f);

            Tile tile = Main.tile[tileX, tileY];
            return tile != null && Main.tileSolid[tile.TileType] && tile.HasTile;
        }

        public void DamageEnemy(Vector2 position, bool dealDamage)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Bottom.Y > position.Y &&
                    Math.Abs(npc.Center.X - position.X) < 100f &&
                    Math.Abs(npc.Center.Y - position.Y) < 100f)
                {
                    npc.AddBuff(BuffID.OnFire, 60);

                    if (dealDamage)
                    {
                        int damage = Main.rand.Next(21, 30);
                        int knockback = 0;
                        int hitDirection = npc.direction;
                        bool crit = false;

                        var hitInfo = new NPC.HitInfo()
                        {
                            Damage = damage,
                            Knockback = knockback,
                            HitDirection = hitDirection,
                            Crit = crit,
                            DamageType = DamageClass.Default
                        };

                        npc.StrikeNPC(hitInfo);
                    }
                }
            }
        }

        public void FirestormSFX()
        {
            string soundPath = Main.rand.Next(3) switch
            {
                0 => "ITD/Content/Sounds/FirestormInABottle1",
                1 => "ITD/Content/Sounds/FirestormInABottle2",
                _ => "ITD/Content/Sounds/FirestormInABottle3"
            };

            SoundEngine.PlaySound(new SoundStyle(soundPath), Player.position);
        }

        public class FireRainData
        {
            public Vector2 InitialPosition;
            public uint CreationTime;
        }
    }
}