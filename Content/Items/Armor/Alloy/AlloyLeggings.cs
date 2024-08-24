using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Items.Armor.Alloy
{
    [AutoloadEquip(EquipType.Legs)]
    public class AlloyLeggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 2;
        }
    }
}
