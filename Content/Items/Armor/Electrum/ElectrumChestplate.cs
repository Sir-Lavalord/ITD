﻿using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Armor.Electrum
{
    [AutoloadEquip(EquipType.Body)]
    public class ElectrumChestplate : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 2;
        }
		
		public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.08f;
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<ElectrumBar>(), 30)
                .AddTile(TileID.Anvils)
                .Register();
		}
    }
}
