using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Items.Materials;
using ITD.Utilities;
using ITD.Systems;
using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Weapons.Melee
{
    public class RhodiumBroadsword : ModItem
    {
		public override void SetStaticDefaults()
        {			
			HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => Color.White
            });
        }
		
        public override void SetDefaults()
        {
            Item.damage = 24;
            Item.DamageType = DamageClass.Melee;
			Item.crit = 2;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 55;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4.75f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
			Item.shootSpeed = 9.5f;
        }
		
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			int loops = Main.rand.Next(2, 4);
			Vector2 toTarget = target.Center - player.Center;
			toTarget.Normalize();
			for (int j = 0; j < loops; j++)
			{
				Vector2 velocity = toTarget.RotatedByRandom(MathHelper.ToRadians(30)) * Main.rand.NextFloat(4f, 6f);
				Projectile.NewProjectile(player.GetSource_FromThis(), target.Center.X, target.Center.Y, velocity.X, velocity.Y, 504, (int)player.GetDamage(DamageClass.Melee).ApplyTo(24), 0.5f, player.whoAmI, 0f, 0f, 0f);
			}
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
                .AddIngredient(ModContent.ItemType<RhodiumBar>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
        } 
    }
}