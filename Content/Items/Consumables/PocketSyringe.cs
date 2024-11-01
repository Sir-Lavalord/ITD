using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using ITD.Content.Tiles;
using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.Tiles.Misc;
using ITD.Content.Items.Placeable;
using Terraria.DataStructures;

namespace ITD.Content.Items.Consumables
{
    public class PocketSyringe : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.value = Item.sellPrice(0, 0, 10);
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.Friendly.Mage.PocketSyringe>();
            Item.shootSpeed = 5f;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Magic;
            Item.consumable = true;
            Item.maxStack = Item.CommonMaxStack;

            Item.autoReuse = true;
            Item.shootSpeed *= 3f;

            Item.damage = 12;
            Item.knockBack = 1;
            Item.crit = 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            const int NumProjectiles = 3;

            for (int i = 0; i < NumProjectiles; i++)
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));

                newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient(ItemID.Glass, 8)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}