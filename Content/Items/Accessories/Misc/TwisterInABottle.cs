using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace ITD.Content.Items.Accessories.Misc
{
    public class TwisterInABottle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.White;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<TwisterDoubleJump>().hasTwisterJump = true;
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

    public class TwisterDoubleJump : ModPlayer
    {
        public const int MaxTwisterDuration = 60;
        public const float SpeedBoostMultiplier = 1.5f;
        public const float AccelerationRate = 0.05f;

        public bool hasTwisterJump;
        public bool canDoubleJump;
        public bool hasReleasedJumpButton;
        public bool isTwistering;
        public int twisterTimer;
        public int damageTimer;
        public int spinTimer;
        public int spinDirection = 1;
        public Vector2 jumpPosition;
        public Dictionary<int, uint> npcDamageTimes = new Dictionary<int, uint>();
        public float currentSpeedMultiplier = 1f;

        public override void ResetEffects()
        {
            hasTwisterJump = false;
        }

        public override void PreUpdate()
        {
            HandleTwisterJump();
            UpdateTimers();

            if (isTwistering)
            {
                Player.direction = spinDirection;
            }
        }

        public override void PostUpdate()
        {
            UpdateTwisterDust();
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (isTwistering)
            {
                drawInfo.drawPlayer.direction = spinDirection;
            }
        }
        public void HandleTwisterJump()
        {
            ResetDoubleJumpAbility();
            CheckJumpButtonRelease();
            ActivateTwister();
        }

        public void ResetDoubleJumpAbility()
        {
            if (Player.velocity.Y == 0f || Player.sliding || (Player.autoJump && Player.justJumped))
            {
                canDoubleJump = true;
                hasReleasedJumpButton = false;
            }
        }

        public void CheckJumpButtonRelease()
        {
            if (!Player.controlJump && Player.velocity.Y != 0f && !hasReleasedJumpButton)
            {
                hasReleasedJumpButton = true;
            }
        }

        public void ActivateTwister()
        {
            if (canDoubleJump && hasTwisterJump && Player.controlJump && hasReleasedJumpButton && Player.velocity.Y != 0f)
            {
                StartTwister();
                canDoubleJump = false;
                hasReleasedJumpButton = false;
            }
            else if (isTwistering && Player.controlJump && twisterTimer > 0)
            {
                ContinueTwister();
            }
            else if (isTwistering)
            {
                EndTwister();
            }
        }

        public void StartTwister()
        {
            isTwistering = true;
            twisterTimer = MaxTwisterDuration;
            damageTimer = 15;
            jumpPosition = Player.Bottom;
            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.jump = (int)Player.jumpHeight;
            spinTimer = 0;
            spinDirection = 1;
            currentSpeedMultiplier = 1f;
            TwisterSFX();
        }

        public void ContinueTwister()
        {
            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.fallStart = (int)(Player.position.Y / 16f);

            currentSpeedMultiplier = Math.Min(currentSpeedMultiplier + AccelerationRate, SpeedBoostMultiplier);

            ApplyHorizontalMovement();
            CreateDustTrail();
            SpinPlayer();
        }

        public void EndTwister()
        {
            isTwistering = false;
            twisterTimer = 0;
        }

        public void ApplyHorizontalMovement()
        {
            if (Player.controlLeft)
            {
                Player.velocity.X = -Player.maxRunSpeed * currentSpeedMultiplier;
            }
            else if (Player.controlRight)
            {
                Player.velocity.X = Player.maxRunSpeed * currentSpeedMultiplier;
            }
        }

        public void SpinPlayer()
        {
            spinTimer++;
            if (spinTimer % 3 == 0)
            {
                Player.bodyFrame.Y = (Player.bodyFrame.Y + Player.bodyFrame.Height) % (Player.bodyFrame.Height * 20);
                Player.direction = spinDirection;
                spinDirection *= -1;
            }

            Player.direction = spinDirection;
        }

        public void CreateDustTrail()
        {
            for (int i = 0; i < 3; i++)
            {
                CreateDustParticle();
            }
        }

        public void CreateDustParticle()
        {
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.Next(10, 41);
            Vector2 dustPosition = Player.Center + offset;

            Vector2 dustVelocity = offset.RotatedBy(MathHelper.PiOver2) * 0.05f;
            dustVelocity.Y -= Main.rand.NextFloat(0.5f, 1f);

            int dustType = Main.rand.NextBool(3) ? DustID.Cloud : DustID.Smoke;
            float scale = Main.rand.NextFloat(0.8f, 1.2f);

            Color dustColor = Color.Lerp(Color.White, Color.LightBlue, Main.rand.NextFloat());

            Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 0, dustColor, scale);
            dust.noGravity = true;
            dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f);
            dust.customData = new TwisterDustData
            {
                InitialPosition = dustPosition,
                Angle = angle,
                Radius = offset.Length(),
                RotationSpeed = Main.rand.NextFloat(0.08f, 0.12f),
                CreationTime = Main.GameUpdateCount,
                NextDamageTime = Main.GameUpdateCount + 60
            };
        }

        public void UpdateTwisterDust()
        {
            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (dust.active && (dust.type == DustID.Cloud || dust.type == DustID.Smoke) && dust.customData is TwisterDustData data)
                {
                    UpdateDustPosition(dust, data);
                    FadeDust(dust);
                    DamageEnemy(dust);
                }
            }
        }

        public void UpdateDustPosition(Dust dust, TwisterDustData data)
        {
            float age = (Main.GameUpdateCount - data.CreationTime) / 60f;

            data.Angle += data.RotationSpeed;

            Vector2 newOffset = new Vector2(
                (float)Math.Cos(data.Angle) * data.Radius,
                dust.position.Y - jumpPosition.Y + dust.velocity.Y
            );

            dust.position = jumpPosition + newOffset;
            dust.position += Main.rand.NextVector2Circular(1f, 1f);
        }

        public void FadeDust(Dust dust)
        {
            dust.scale *= 0.98f;
            if (dust.scale <= 0.1f || dust.position.Y > jumpPosition.Y + 170f)
            {
                dust.active = false;
            }
        }

        public void UpdateTimers()
        {
            if (twisterTimer > 0)
            {
                twisterTimer--;
                Twister();
            }

            if (damageTimer > 0)
            {
                damageTimer--;
            }
        }

        public void Twister()
        {
            float progress = (float)twisterTimer / MaxTwisterDuration;
            for (int i = 0; i < 10; i++)
            {
                TwisterDust(progress);
            }
        }

        public void TwisterDust(float progress)
        {
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float radius = Main.rand.NextFloat(20f, 60f);
            float height = Main.rand.NextFloat(-120f, 40f) * progress;

            Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, height);
            Vector2 position = jumpPosition + offset;

            Vector2 velocity = new Vector2(-offset.X * Main.rand.NextFloat(0.03f, 0.07f), Main.rand.NextFloat(0.5f, 2.5f));

            int dustType = Main.rand.NextBool(3) ? DustID.Cloud : DustID.Smoke;

            Color dustColor = Color.Lerp(Color.White, Color.LightBlue, Main.rand.NextFloat(0.7f));

            Dust dust = Dust.NewDustPerfect(
                position,
                dustType,
                velocity,
                0,
                dustColor * progress,
                Main.rand.NextFloat(0.8f, 1.8f) * progress
            );

            dust.noGravity = true;
            dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f) * progress;

            dust.customData = new TwisterDustData
            {
                InitialPosition = position,
                Angle = angle,
                Radius = radius,
                RotationSpeed = Main.rand.NextFloat(0.08f, 0.12f),
                CreationTime = Main.GameUpdateCount,
                NextDamageTime = Main.GameUpdateCount
            };

            DamageEnemy(dust);
        }

        public void DamageEnemy(Dust dust)
        {
            if (dust.customData is TwisterDustData data && Main.GameUpdateCount >= data.NextDamageTime)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly &&
                        Vector2.Distance(npc.Center, dust.position) < 10f)
                    {
                        if (!npcDamageTimes.ContainsKey(npc.whoAmI) || Main.GameUpdateCount >= npcDamageTimes[npc.whoAmI])
                        {
                            ApplyDamage(npc);
                            npcDamageTimes[npc.whoAmI] = Main.GameUpdateCount + 60;
                        }
                    }
                }
                data.NextDamageTime = Main.GameUpdateCount + 60;
            }
        }

        public void ApplyDamage(NPC npc)
        {
            int damage = 70;
            int knockback = 2;
            int hitDirection = npc.Center.X < Player.Center.X ? -1 : 1;
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

        public void TwisterSFX()
        {
            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/TwisterInABottle1"), Player.position);
        }

        public class TwisterDustData
        {
            public Vector2 InitialPosition;
            public float Angle;
            public float Radius;
            public float RotationSpeed;
            public uint CreationTime;
            public uint NextDamageTime;
        }
    }
}