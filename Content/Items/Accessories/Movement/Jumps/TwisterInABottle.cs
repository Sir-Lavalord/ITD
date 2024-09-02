using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System;
using Terraria.DataStructures;

namespace ITD.Content.Items.Accessories.Movement.Jumps
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
            player.GetModPlayer<TwisterPlayer>().hasTwisterJump = true;
        }
    }

    public class TwisterPlayer : ModPlayer
    {

        public const int DamageInterval = 30; // 2 times per second (60 frames / 2)
        public const int TotalDamage = 70;
        public const int DamageDuration = 120; // 2 seconds (60 frames * 2)
        private int damageCounter = 0;
        public const int TwisterDuration = 60;
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
        public float currentSpeedMultiplier = 1f;

        public override void ResetEffects() => hasTwisterJump = false;

        public override void PreUpdate()
        {
            HandleTwisterJump();
            UpdateTimers();
            if (isTwistering)
            {
                Player.direction = spinDirection;
                CreateTrail();
            }
        }

        public override void PostUpdate() => UpdateTwisterDust();

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (isTwistering) drawInfo.drawPlayer.direction = spinDirection;
        }

        private void HandleTwisterJump()
        {
            if ((Player.velocity.Y == 0f || Player.sliding || Player.autoJump && Player.justJumped) && hasTwisterJump)
            {
                canDoubleJump = true;
                hasReleasedJumpButton = false;
            }

            if (!Player.controlJump && Player.velocity.Y != 0f && !hasReleasedJumpButton)
            {
                hasReleasedJumpButton = true;
            }

            if (canDoubleJump && hasTwisterJump && Player.controlJump && hasReleasedJumpButton && Player.velocity.Y != 0f)
            {
                StartTwister();
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

        private void StartTwister()
        {
            isTwistering = true;
            twisterTimer = TwisterDuration;
            damageTimer = DamageInterval;
            jumpPosition = Player.Bottom;
            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.jump = Player.jumpHeight;
            spinTimer = 0;
            spinDirection = 1;
            currentSpeedMultiplier = 1f;
            canDoubleJump = false;
            hasReleasedJumpButton = false;
            damageCounter = 0;
            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/TwisterInABottle1"), Player.position);
        }

        private void ContinueTwister()
        {
            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.fallStart = (int)(Player.position.Y / 16f);
            currentSpeedMultiplier = Math.Min(currentSpeedMultiplier + AccelerationRate, SpeedBoostMultiplier);
            ApplyHorizontalMovement();
            SpinPlayer();
        }

        private void EndTwister()
        {
            isTwistering = false;
            twisterTimer = 0;
        }

        private void ApplyHorizontalMovement()
        {
            if (Player.controlLeft)
                Player.velocity.X = -Player.maxRunSpeed * currentSpeedMultiplier;
            else if (Player.controlRight)
                Player.velocity.X = Player.maxRunSpeed * currentSpeedMultiplier;
        }

        private void SpinPlayer()
        {
            spinTimer++;
            if (spinTimer % 3 == 0)
            {
                Player.bodyFrame.Y = (Player.bodyFrame.Y + Player.bodyFrame.Height) % (Player.bodyFrame.Height * 20);
                Player.direction = spinDirection;
                spinDirection *= -1;
            }
        }

        private void UpdateTimers()
        {
            if (twisterTimer > 0)
            {
                twisterTimer--;
                Twister();
            }
            if (damageTimer > 0) damageTimer--;
        }

        private void CreateTrail()
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 position = Player.Center + new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-20f, 20f));
                Vector2 velocity = Player.velocity * -0.2f + Main.rand.NextVector2Circular(1f, 1f);
                Color color = Color.Lerp(Color.White, Color.Cyan, Main.rand.NextFloat(0.7f));

                Dust dust = Dust.NewDustPerfect(
                    position,
                    DustID.Cloud,
                    velocity,
                    0,
                    color,
                    Main.rand.NextFloat(0.5f, 1f)
                );

                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        private void Twister()
        {
            float progress = (float)twisterTimer / TwisterDuration;
            for (int i = 0; i < 10; i++) CreateTwisterDust(progress);
        }

        private void CreateTwisterDust(float progress)
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

            DamageEnemy(dust.position);
        }

        private void UpdateTwisterDust()
        {
            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (dust.active && (dust.type == DustID.Cloud || dust.type == DustID.Smoke) && dust.customData is TwisterDustData data)
                {
                    UpdateDustPosition(dust, data);
                    FadeDust(dust);

                    if (Main.GameUpdateCount >= data.NextDamageTime && damageCounter < DamageDuration)
                    {
                        DamageEnemy(dust.position);
                        data.NextDamageTime = Main.GameUpdateCount + DamageInterval;
                        damageCounter += DamageInterval;
                    }
                }
            }
        }

        private void UpdateDustPosition(Dust dust, TwisterDustData data)
        {
            data.Angle += data.RotationSpeed;
            Vector2 newOffset = new Vector2(
                (float)Math.Cos(data.Angle) * data.Radius,
                dust.position.Y - jumpPosition.Y + dust.velocity.Y
            );
            dust.position = jumpPosition + newOffset + Main.rand.NextVector2Circular(1f, 1f);
        }

        private void FadeDust(Dust dust)
        {
            dust.scale *= 0.98f;
            if (dust.scale <= 0.1f || dust.position.Y > jumpPosition.Y + 170f)
            {
                dust.active = false;
            }
        }

        private void DamageEnemy(Vector2 position)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(npc.Center, position) < 10f)
                {
                    ApplyDamage(npc);
                    break;
                }
            }
        }

        private void ApplyDamage(NPC npc)
        {
            int direction = npc.Center.X < Player.Center.X ? -1 : 1;
            int damage = TotalDamage / (DamageDuration / DamageInterval);
            Player.ApplyDamageToNPC(npc, damage, 2f, direction, crit: false);
        }

        private class TwisterDustData
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