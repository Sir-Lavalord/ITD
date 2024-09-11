using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Sniptrap : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<SniptrapProjectile>(), 12f, 22, 10);
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 25);
        }
        public override bool CanUseItem(Player player) => player.GetSnaptrapPlayer().CanUseSnaptrap;
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.GetSnaptrapPlayer().ShootSnaptrap();
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