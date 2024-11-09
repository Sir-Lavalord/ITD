using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ITD.Content.Projectiles;
using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Quasar : ModItem
    {

        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 60;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = false;
        }
		
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.GetGlobalProjectile<ITDGlobalProjectile>().aura = 1;
			
			for (int i = 0; i < 16; i++) {
				int dust = Dust.NewDust(position-new Vector2(6, 6), 12, 12, ModContent.DustType<StarlitDust>(), 0f, 0f, 0, default, 2f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity = velocity * Main.rand.NextFloat(1.5f);
			}
            return false;
        }
		
		public override Vector2? HoldoutOffset() {
			return new Vector2(-4f, 0f);
		}
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.Star, 5)
                .AddIngredient(ItemID.MeteoriteBar, 3)
                .AddIngredient(ModContent.ItemType<StarlitBar>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
