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
    public class Mandinata : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 21;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shootSpeed = 4f;
            Item.shoot = ModContent.ProjectileType<MandinataProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<MandinataProjectile>(), damage, knockback, player.whoAmI);

            Projectile.NewProjectileDirect(source, position, velocity * 0.99f, ModContent.ProjectileType<MandinataBreath>(), damage / 3, knockback * 0.25f, player.whoAmI);
            Projectile.NewProjectileDirect(source, position, velocity * 0.66f, ModContent.ProjectileType<MandinataBreath>(), damage / 3, knockback * 0.25f, player.whoAmI);
            Projectile.NewProjectileDirect(source, position, velocity * 0.33f, ModContent.ProjectileType<MandinataBreath>(), damage / 3, knockback * 0.25f, player.whoAmI);

            return false;
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
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<EmberlionMandible>(), 1)
                .AddIngredient(ModContent.ItemType<EmberlionSclerite>(), 6)
                .AddIngredient(ItemID.IronBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
