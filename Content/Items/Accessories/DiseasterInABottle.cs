using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System;

namespace ITD.Content.Items
{
    public class DisasterInABottle : ModItem
    {
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
            player.GetModPlayer<DisasterDoubleJump>().hasDisasterJump = true;
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

    public class DisasterDoubleJump : ModPlayer
    {
        public bool hasDisasterJump;
        public bool canDoubleJump;
        public bool waitDoubleJump;
        public int jumpCount;

        public FireDoubleJump fireJump;
        public TwisterDoubleJump twisterJump;
        public StormDoubleJump stormJump;

        public override void Initialize()
        {
            fireJump = Player.GetModPlayer<FireDoubleJump>();
            twisterJump = Player.GetModPlayer<TwisterDoubleJump>();
            stormJump = Player.GetModPlayer<StormDoubleJump>();
        }

        public override void ResetEffects()
        {
            hasDisasterJump = false;
        }

        public override void PreUpdate()
        {
            if (hasDisasterJump)
            {
                HandleDisasterJump();
            }
        }

        public void HandleDisasterJump()
        {
            if ((Player.velocity.Y == 0f || Player.sliding || (Player.autoJump && Player.justJumped)))
            {
                canDoubleJump = true;
                waitDoubleJump = true;
                jumpCount = 0;
            }

            if (canDoubleJump && Player.controlJump && !waitDoubleJump)
            {
                PerformDisasterJump();
            }

            waitDoubleJump = Player.controlJump;

            fireJump.hasFireJump = hasDisasterJump;
            twisterJump.hasTwisterJump = hasDisasterJump;
            stormJump.hasStormJump = hasDisasterJump;
        }

        public void PerformDisasterJump()
        {
            if (Player.jump > 0) return;

            jumpCount++;

            switch (jumpCount)
            {
                case 1:
                    fireJump.PerformDoubleJump();
                    break;
                case 2:
                    twisterJump.StartTwister();
                    break;
                case 3:
                    stormJump.PerformDoubleJump();
                    jumpCount = 0;
                    break;
            }

            canDoubleJump = false;
        }
    }
}