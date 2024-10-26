using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Armor.Rhodium
{
    [AutoloadEquip(EquipType.Legs)]
    public class RhodiumLeggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 2;
        }
		
		public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 4f;
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<RhodiumBar>(), 25)
                .AddTile(TileID.Anvils)
                .Register();
		}
    }
}
