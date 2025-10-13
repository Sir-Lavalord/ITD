using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Rarities;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee;

public class DespoticSuperMeleeSword : ModItem
{
    public int directionCycle = 0;
    public int specialCycle = 0;
    public override void SetDefaults()
    {
        Item.width = Item.height = 82;
        Item.damage = 1000;
        Item.DamageType = DamageClass.Melee;
        Item.useStyle = -1;
        Item.knockBack = 7f;
        Item.value = Item.sellPrice(platinum: 1);
        Item.rare = ModContent.RarityType<DespoticRarity>();
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useTime = Item.useAnimation = 30;
        Item.UseSound = SoundID.Item71;
        Item.shoot = ModContent.ProjectileType<DespoticSuperMeleeProj>();
        Item.shootSpeed = 1;
    }
    public override bool MeleePrefix() => true;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float direction = 1;
        directionCycle = ++directionCycle % 2;
        if (directionCycle == 0)
            direction = -1;
        float special = 0;
        specialCycle = ++specialCycle % 3;
        if (specialCycle == 0)
            special = 1;

        velocity.Normalize();

        float adjustedItemScale = player.GetAdjustedItemScale(player.inventory[player.selectedItem]);
        Projectile.NewProjectile(source, position, velocity.RotatedBy(-2 * direction) * adjustedItemScale * 32f, type, damage, knockback, player.whoAmI, special, direction);
        return false;
    }
}
