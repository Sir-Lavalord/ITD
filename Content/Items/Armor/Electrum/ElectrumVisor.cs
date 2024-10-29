using ITD.Utilities;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Armor.Electrum
{
    [AutoloadEquip(EquipType.Head)]
    public class ElectrumVisor : ModItem
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
            player.moveSpeed += 0.08f;
        }
		
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ElectrumChestplate>() && legs.type == ModContent.ItemType<ElectrumLeggings>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.GetITDPlayer().setElectrum = true;
            player.setBonus = SetBonusText.Value;
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<ElectrumBar>(), 20)
                .AddTile(TileID.Anvils)
                .Register();
		}
    }
}
