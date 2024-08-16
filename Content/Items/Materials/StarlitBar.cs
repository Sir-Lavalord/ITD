using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using ITD.Content.Tiles;
using ITD.Content.Items.Placeable;

namespace ITD.Content.Items.Materials
{
    public class StarlitBar : ModItem
    {
        public override void SetStaticDefaults()
        {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
			
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 59;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 24;
            Item.maxStack = 99;
			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 10);
			
			Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;

            Item.createTile = ModContent.TileType<StarlitBars>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<StarlitOre>(), 4)
                .AddTile(TileID.Furnaces)
                .Register();
        }
    }
}