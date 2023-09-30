using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Rhodium
{
    public class RhodiumBow : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Rhodium Bow");
            // Tooltip.SetDefault("A bow made of precious rhodium.");
        }

        public override void SetDefaults()
        {
            Item.damage = 7; // Low damage
            Item.DamageType = DamageClass.Ranged;
            Item.width = 20;
            Item.height = 40;
            Item.useTime = 10; // Faster fire rate
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 8f; // Slightly higher shoot speed
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool? UseItem(Player player)
        {
            return base.UseItem(player);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Content.Items.OreAndBars.Rhodium.RhodiumBar>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
