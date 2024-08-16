using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using ITD.Content.Tiles;
using ITD.Content.Items.Placeable;

namespace ITD.Content.Items.Materials
{
    public class RefinedCyanite : ModItem
    {
        public override void SetStaticDefaults()
        {			
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 59;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 40;
            Item.maxStack = 99;
			Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 2);
        }
    }
}