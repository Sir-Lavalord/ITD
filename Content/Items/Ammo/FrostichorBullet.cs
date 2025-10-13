namespace ITD.Content.Items.Ammo;

// This example is similar to the Wooden Arrow item
public class FrostichorBullet : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
    }

    public override void SetDefaults()
    {
        Item.width = 8;
        Item.height = 8;

        Item.damage = 26;
        Item.DamageType = DamageClass.Ranged;

        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.knockBack = 4.5f;
        Item.value = Item.sellPrice(copper: 22);
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Ranger.Ammo.FrostichorBullet>(); // The projectile that weapons fire when using this item as ammunition.
        Item.shootSpeed = 5.5f;
        Item.ammo = AmmoID.Bullet;
    }
}