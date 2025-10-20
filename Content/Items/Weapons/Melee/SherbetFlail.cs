using ITD.Content.Projectiles.Friendly.Melee;

namespace ITD.Content.Items.Weapons.Melee;

// Example Advanced Flail is a complete adaption of Ball O' Hurt. The Projectile has the complete code needed to customize all aspects of the flail. See ExampleFlail for a simpler example that is less customizable. 
public class SherbetFlail : ModItem
{
    public override void SetStaticDefaults()
    {
        // This line will make the damage shown in the tooltip twice the actual Item.damage. This multiplier is used to adjust for the dynamic damage capabilities of the projectile.
        // When thrown directly at enemies, the flail projectile will deal double Item.damage, matching the tooltip, but deals normal damage in other modes.
        ItemID.Sets.ToolTipDamageMultiplier[Type] = 2f;
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
        Item.useAnimation = 45; // The item's use time in ticks (60 ticks == 1 second.)
        Item.useTime = 45; // The item's use time in ticks (60 ticks == 1 second.)
        Item.knockBack = 8f; // The knockback of your flail, this is dynamically adjusted in the projectile code.
        Item.width = 32; // Hitbox width of the item.
        Item.height = 32; // Hitbox height of the item.
        Item.damage = 22; // The damage of your flail, this is dynamically adjusted in the projectile code.
        Item.noUseGraphic = true; // This makes sure the item does not get shown when the player swings his hand
        Item.shoot = ModContent.ProjectileType<SherbetFlailProjectile>(); // The flail projectile
        Item.shootSpeed = 20f; // The speed of the projectile measured in pixels per frame.
        Item.UseSound = SoundID.Item1; // The sound that this item makes when used
        Item.rare = ItemRarityID.Red; // The color of the name of your item
        Item.value = Item.sellPrice(gold: 1, silver: 50); // Sells for 1 gold 50 silver
        Item.DamageType = DamageClass.MeleeNoSpeed; // Deals melee damage
        Item.channel = true;
        Item.noMelee = true; // This makes sure the item does not deal damage from the swinging animation
    }
}