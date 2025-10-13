using ITD.Content.Projectiles.Friendly.Mage;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Mage;

public class FungalRod : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Magic;
        Item.damage = 16;
        Item.mana = 6;
        Item.width = 56;
        Item.height = 56;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 1f;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = new SoundStyle("ITD/Content/Sounds/FungalRodShoot");
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<FungalRodProj>();
        Item.shootSpeed = 2.5f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float spreadAngle = MathHelper.ToRadians(15);
        int numProjectiles = 5;

        for (int i = 0; i < numProjectiles; i++)
        {
            float lerpFactor = (i - (numProjectiles - 1) / 2f) / ((numProjectiles - 1) / 2f);
            float angle = lerpFactor * spreadAngle / 2;
            Vector2 newVelocity = velocity.RotatedBy(angle);
            float curveIntensity = Math.Abs(lerpFactor);
            int direction = lerpFactor > 0 ? 1 : -1;

            Projectile proj = Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI, curveIntensity * direction, direction);
            proj.timeLeft += (int)curveIntensity * 2;
        }
        return false;
    }
}