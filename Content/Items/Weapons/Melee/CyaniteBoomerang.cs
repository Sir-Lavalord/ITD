using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Weapons.Melee
{
	public class CyaniteBoomerang : ModItem
	{
        public override void SetDefaults()
        {
            Item.damage = 42;
			Item.DamageType = DamageClass.Melee;
            Item.width = 16;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shootSpeed = 20f;
			Item.shoot = ModContent.ProjectileType<CyaniteBoomerangProjectile>();
		}
		
		/*public override  bool CanUseItem (Player player)
        {
			for (int k = 0; k < 1000; k++)
			{
				if (Main.projectile[k].active && Main.projectile[k].owner == Main.myPlayer && Main.projectile[k].type == Item.shoot)
				{
					return false;
				}
			}
			return true;
        }*/
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<RefinedCyanite>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
		}
	}
}