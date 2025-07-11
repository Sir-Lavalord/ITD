using Terraria.Audio;

namespace ITD.Content.Items.Accessories.Movement.Jumps
{
    public class DisasterInABottle : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DisasterPlayer>().hasDisasterJump = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FirestormInABottle>()
                .AddIngredient<TwisterInABottle>()
                .AddIngredient<StormInABottle>()
                .Register();
        }
    }

    public class DisasterPlayer : ModPlayer
    {
        public bool hasDisasterJump;
        private bool canJump;
        private int jumpCount;
        private FirestormPlayer firestormPlayer;
        private TwisterPlayer twisterPlayer;
        private StormPlayer stormPlayer;

        public override void Initialize()
        {
            firestormPlayer = Player.GetModPlayer<FirestormPlayer>();
            twisterPlayer = Player.GetModPlayer<TwisterPlayer>();
            stormPlayer = Player.GetModPlayer<StormPlayer>();
        }

        public override void ResetEffects()
        {
            hasDisasterJump = false;
        }

        public override void PreUpdate()
        {
            if (!hasDisasterJump)
            {
                canJump = false;
                jumpCount = 0;
            }

            if (hasDisasterJump)
            {
                HandleDisasterJump();
            }
        }

        private void HandleDisasterJump()
        {
            if (Player.velocity.Y == 0f || Player.sliding || Player.autoJump && Player.justJumped)
            {
                canJump = true;
                jumpCount = 0;
            }

            if (canJump && Player.controlJump && Player.releaseJump)
            {
                PerformDisasterJump();
            }

            UpdateSubPlayers();
        }

        private void UpdateSubPlayers()
        {
            firestormPlayer.hasFireJump = hasDisasterJump;
            twisterPlayer.hasTwisterJump = hasDisasterJump;
            stormPlayer.hasStormJump = hasDisasterJump;
        }

        private void PerformDisasterJump()
        {
            if (Player.jump > 0 || jumpCount >= 2) return;

            jumpCount++;
            switch (jumpCount)
            {
                case 1:
                    PerformFirestormTwisterJump();
                    break;
                case 2:
                    PerformStormTornadoJump();
                    canJump = false;
                    break;
            }
        }

        private void PerformFirestormTwisterJump()
        {
            Player.velocity.Y = -Player.jumpSpeed * Player.gravDir;
            Player.jump = Player.jumpHeight;

            // Firestorm
            SoundEngine.PlaySound(new SoundStyle($"ITD/Content/Sounds/FirestormInABottle{Main.rand.Next(1, 3)}"), Player.position);
            firestormPlayer.fireCloudTimer = FirestormPlayer.FireCloudDuration;
            firestormPlayer.fireTrailTimer = FirestormPlayer.FireTrailDuration;
            firestormPlayer.rainTimer = FirestormPlayer.RainDuration;
            firestormPlayer.damageTimer = FirestormPlayer.DamageInterval;
            firestormPlayer.jumpPosition = Player.Bottom;

            // Twister
            SoundEngine.PlaySound(SoundID.DoubleJump, Player.position);
            twisterPlayer.isTwistering = true;
            twisterPlayer.twisterTimer = TwisterPlayer.TwisterDuration;
            twisterPlayer.damageTimer = TwisterPlayer.DamageInterval;
            twisterPlayer.jumpPosition = Player.Bottom;
            twisterPlayer.spinTimer = 0;
            twisterPlayer.spinDirection = 1;
            twisterPlayer.currentSpeedMultiplier = 1f;
        }

        private void PerformStormTornadoJump()
        {
            Player.velocity.Y = -Player.jumpSpeed * 1.5f * Player.gravDir;
            Player.jump = (int)(Player.jumpHeight * 1.5f);

            // Storm
            SoundEngine.PlaySound(new SoundStyle($"ITD/Content/Sounds/StormInABottle{Main.rand.Next(1, 3)}"), Player.position);
            stormPlayer.tornadoTimer = StormPlayer.TornadoDuration;
            stormPlayer.lightningTimer = StormPlayer.LightningInterval;
            stormPlayer.tornadoBase = Player.Bottom + new Vector2(0, 40);
            stormPlayer.AdjustTornadoBaseToGround();
        }
    }
}