using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Items.Weapons.Melee
{
	public class EmberSlasher : ModItem
	{
        // The Display Name and Tooltip of this item can be edited in the Localization/en-US_Mods.ITDQTest.hjson file.

		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.DamageType = DamageClass.Melee;
			Item.width = 48;
			Item.height = 50;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<EmberSlash>();
			Item.shootSpeed = 0.2f;
		}
        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}