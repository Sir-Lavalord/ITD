using ITD.Content.Tiles.BlueshroomGroves;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Materials
{
    public class Blueshroom : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.buyPrice(silver: 1);
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.maxStack = Item.CommonMaxStack;
        }
        public override bool? UseItem(Player player)
        {
            int i = (int)Main.MouseWorld.X;
            int j = (int)Main.MouseWorld.Y;
            WorldGen.PlaceObject(i, j, ModContent.TileType<BlueshroomSapling>());
            WorldGen.GrowTree(i, j);
            return true;
        }
    }
}