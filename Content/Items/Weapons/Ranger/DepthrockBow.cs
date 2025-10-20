using ITD.Content.Items.Placeable.LayersRework;
using ITD.Content.Projectiles.Friendly.Ranger;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Ranger;

public class DepthrockBow : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 30;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 40;
        Item.height = 58;
        Item.useTime = 40;
        Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2;
        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item5;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.shootSpeed = 5f;
        Item.useAmmo = AmmoID.Arrow;
        Item.autoReuse = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        //swiped this little number off tapenki
        Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<DepthrockStoneProj>(), damage, knockback, player.whoAmI, type);
        for (int i = 0; i < 10; i++)
        {
            int dust = Dust.NewDust(position, 1, 1, DustID.Stone, velocity.X / 2, velocity.Y / 2, 0, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 2f;
        }
        return false;
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<Depthrock>(), 10);//PLACEHOLDER
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}
