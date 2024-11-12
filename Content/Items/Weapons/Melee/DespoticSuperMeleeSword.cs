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
		public int attackCycle = 0;
        public override void SetDefaults()
        {
            Item.width = Item.height = 82;
            Item.damage = 1000;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<DespoticRarity>();
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useTime = Item.useAnimation = 30;
			Item.UseSound = SoundID.Item71;
            Item.shoot = ModContent.ProjectileType<DespoticSuperMeleeProj>();
			Item.shootSpeed = 32;
        }
        public override bool MeleePrefix() => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			float direction = 1;
			attackCycle = ++attackCycle % 2;
			if (attackCycle == 0)
				direction = -1;
			
            float adjustedItemScale = player.GetAdjustedItemScale(player.inventory[player.selectedItem]);
            Projectile.NewProjectile(source, position, velocity.RotatedBy(-2 * direction) * adjustedItemScale, type, damage, knockback, player.whoAmI, 1.75f, direction);
            return false;
        }
    }
}
