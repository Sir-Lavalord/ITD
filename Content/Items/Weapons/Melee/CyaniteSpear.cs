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
	public class CyaniteSpear : ModItem
	{
		public override void SetStaticDefaults()
		{
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
			ItemID.Sets.Spears[Item.type] = true; 
		}

        public override void SetDefaults()
        {
            Item.damage = 42;
			Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item71;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = false;
            Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shootSpeed = 3.5f;
			Item.shoot = ModContent.ProjectileType<CyaniteSpearProjectile>();
		}
		
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override bool? UseItem(Player player) {
			if (!Main.dedServ && Item.UseSound.HasValue) {
				SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
			}

			return null;
		}
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<RefinedCyanite>(), 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
		}
	}
}