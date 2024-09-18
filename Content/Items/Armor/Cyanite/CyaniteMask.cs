using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using Terraria.Localization;

namespace ITD.Content.Items.Armor.Cyanite
{
    [AutoloadEquip(EquipType.Head)]
    public class CyaniteMask : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
        public override void SetStaticDefaults()
        {
            SetBonusText = this.GetLocalization("SetBonus");
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 19;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<CyaniteGreaves>() && legs.type == ModContent.ItemType<CyanitePlating>();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.ammoCost75 = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
        }
    }
}
