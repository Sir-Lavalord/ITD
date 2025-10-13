using ITD.Content.Items.Placeable;
using Terraria.DataStructures;


namespace ITD.Content.Items.Weapons.Mage;

public class PocketSyringe : ModItem
{
    public override void SetStaticDefaults()
    {
        Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.value = Item.sellPrice(0, 0, 10);
        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noUseGraphic = true;
        Item.useAnimation = 17;
        Item.useTime = 17;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Mage.PocketSyringeProjectile>();
        Item.shootSpeed = 15f;
        Item.noMelee = true;
        Item.DamageType = DamageClass.Magic;
        Item.consumable = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.UseSound = SoundID.Item1;

        Item.damage = 10;
        Item.knockBack = 1;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        const int NumProjectiles = 3;
        if (player.lifeRegen < 0)
            damage -= player.lifeRegen / 2;

        for (int i = 0; i < NumProjectiles; i++)
        {
            Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));

            newVelocity *= 1f - Main.rand.NextFloat(0.3f);

            Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
        }
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe(10)
            .AddIngredient(ModContent.ItemType<TemperedGlassItem>(), 1)
            .AddTile(TileID.Furnaces)//there is the glass klin but who's gonna use that
            .Register();
    }
}