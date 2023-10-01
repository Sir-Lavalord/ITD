using ITD.Content.Items.Weapons.Electrum;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Electrum
{
	public class ElectrumSpear : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
			ItemID.Sets.Spears[Item.type] = true; 
		}

		public override void SetDefaults()
		{
			
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(silver: 10); 

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 12;
			Item.useTime = 18;
			Item.UseSound = SoundID.Item71;
			Item.autoReuse = true;
		
			Item.damage = 25;
			Item.knockBack = 6.5f;
			Item.noUseGraphic = true;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;

			Item.shootSpeed = 3.7f;
			Item.shoot = ModContent.ProjectileType<ElectrumSpearProjectile>();
		}

		public override bool CanUseItem(Player player)
		{

			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override bool? UseItem(Player player)
		{

			if (!Main.dedServ && Item.UseSound.HasValue)
			{
				SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
			}

			return null;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Content.Items.OreAndBars.Electrum.ElectrumBar>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}