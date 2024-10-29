using ITD.Utilities;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Armor.Rhodium
{
    [AutoloadEquip(EquipType.Head)]
    public class RhodiumVisor : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
        public override void SetStaticDefaults()
        {
            SetBonusText = this.GetLocalization("SetBonus");
        }
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 18;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 2;
        }
		
		public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 4f;
        }
		
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<RhodiumChestplate>() && legs.type == ModContent.ItemType<RhodiumLeggings>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.GetITDPlayer().setRhodium = true;
            player.setBonus = SetBonusText.Value;
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<RhodiumBar>(), 20)
                .AddTile(TileID.Anvils)
                .Register();
		}
    }
}
