using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Utilities;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Sniptrap : ITDSnaptrapItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<SniptrapProjectile>(), 12f, 22, 10);
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 25);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 4);
            recipe.AddIngredient(ItemID.Chain, 8);
            recipe.AddIngredient(ItemID.Hook, 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}