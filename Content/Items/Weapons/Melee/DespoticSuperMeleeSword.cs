using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Melee
{
    public class DespoticSuperMeleeSword : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = Item.height = 82;
            Item.damage = 1000;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<DespoticRarity>();
            Item.channel = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = Item.useAnimation = 30;
            Item.shoot = ModContent.ProjectileType<DespoticSuperMeleeProj>();
        }
        public override bool MeleePrefix() => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale = player.GetAdjustedItemScale(player.inventory[player.selectedItem]);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1.75f * adjustedItemScale);
            return false;
        }
    }
}
