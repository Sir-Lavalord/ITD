using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Items.Materials;
using ITD.Utilities;

namespace ITD.Content.Items.Weapons.Melee
{
    public class RhodiumBroadsword : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 18;
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
			Item.shoot = ModContent.ProjectileType<RhodiumSwordBeam>();
			Item.shootSpeed = 9.5f;
        }
		
		public override void MeleeEffects (Player player, Rectangle hitbox)
        {
			Vector2 position;
			Vector2 spinningpoint;
			MiscHelpers.GetPointOnSwungItemPath(player, 50f, 50f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out position, out spinningpoint);
			Vector2 value = spinningpoint.RotatedBy((double)(1.57079637f * (float)player.direction * player.gravDir), default(Vector2));
			Dust.NewDustPerfect(position, DustID.Electric, new Vector2?(value * 4f), 100, default(Color), 1f).noGravity = true;
        }
		
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(724, 1)
                .AddIngredient(ModContent.ItemType<RhodiumBar>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
        } 
    }
}