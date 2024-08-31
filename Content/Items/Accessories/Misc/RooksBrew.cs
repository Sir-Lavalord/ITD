using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using rail;
using ITD.Content.Items.Accessories.Movement.Dashes;
using ITD.Content.Items.Accessories.Movement.Jumps;
using ITD.Content.Items.Accessories.Defensive.Buffs;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Items.Accessories.Misc
{
    internal class RooksBrew : ModItem
    {
        public override string Texture => Placeholder.PHBottle;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 5);

            Item.accessory = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(TileID.TinkerersWorkbench)
                .AddIngredient<DashingFeather>()
                .AddIngredient<JumpingBean>()
                .AddIngredient<CupOJoe>()
                .Register();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.jumpSpeedBoost += 1;
            player.moveSpeed += 0.08f;

            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;

            player.GetModPlayer<FeatherDashPlayer>().DashAccessoryEquipped = true;
        }
    }
}
