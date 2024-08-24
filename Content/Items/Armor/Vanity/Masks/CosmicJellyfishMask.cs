using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Utils;
using Terraria.Localization;
using ITD.Content.Items.Armor.Alloy;

namespace ITD.Content.Items.Armor.Vanity.Masks
{
    [AutoloadEquip(EquipType.Head)]
    internal class CosmicJellyfishMask : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;

            Item.value = Item.buyPrice(silver: 75);
            Item.rare = ItemRarityID.Blue;

            Item.vanity = true;
        }

    }
}
