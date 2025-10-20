using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Ranger.Ammo;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Ranger;

public class ElectrumBow : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 14;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 16;
        Item.height = 30;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2;
        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item5;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.shootSpeed = 8f;
        Item.useAmmo = AmmoID.Arrow;
        Item.autoReuse = false;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (type == 1)
            type = ModContent.ProjectileType<ElectrifiedArrow>();
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<ElectrumBar>(), 7);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}
