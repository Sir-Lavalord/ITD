using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Items.Armor.Cyanite
{
    [AutoloadEquip(EquipType.Legs)]
    public class CyaniteGreaves : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 18;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.10f;
            //+5% Damage reduction somehow
        }
    }
}
