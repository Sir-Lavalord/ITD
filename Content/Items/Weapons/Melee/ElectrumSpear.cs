using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Weapons.Melee
{
	public class ElectrumSpear : ModItem
	{
		public override void SetStaticDefaults()
		{
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
			ItemID.Sets.Spears[Item.type] = true; 
		}

        public override void SetDefaults()
        {
            Item.damage = 19;
			Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 40;
            Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
            Item.autoReuse = false;
            Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shootSpeed = 3.5f;
			Item.shoot = ModContent.ProjectileType<ElectrumSpearProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			for (int i = 0; i < 3; i++)
            {
                Vector2 rotatedVelocity = velocity.RotatedBy(MathHelper.ToRadians(-15+15*i));
				Projectile proj = Projectile.NewProjectileDirect(source, position, rotatedVelocity, type, damage, knockback, player.whoAmI);
				proj.timeLeft += i*5;
				if (i == 2)
					proj.ai[0] = 1;
            }
            return false;
        }
		
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ItemID.ThunderSpear, 1)
                .AddIngredient(ModContent.ItemType<ElectrumBar>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
		}
	}
}