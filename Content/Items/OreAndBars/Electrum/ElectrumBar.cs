using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;


namespace ITD.Content.Items.OreAndBars.Electrum
{
    internal class ElectrumBar : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 59;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.value = Item.buyPrice(silver: 1, copper: 75);

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;

            Item.createTile = ModContent.TileType<Content.Items.OreAndBars.Electrum.ElectrumBars>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.GoldOre, 5)
                .AddRecipeGroup("IronOre", 10)
                .AddTile(TileID.Furnaces)
                .Register();
        }
    }
}