using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class HunterrGreatbow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 86;
            Item.damage = 2000;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 20f;
            Item.value = 999;
            Item.rare = ModContent.RarityType<UnTerrRarity>();
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.DD2_DefenseTowerSpawn;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HunterrGreatbowProj>();
            Item.useAmmo = AmmoID.Arrow;
            Item.reuseDelay = 60;
            Item.shootSpeed = 15f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] > 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 shootVelocity = velocity;
            Vector2 shootDirection = shootVelocity.SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.NewProjectile(source, position, shootDirection, ModContent.ProjectileType<HunterrGreatbowProj>(), damage, knockback, player.whoAmI);
            return false;
        }
    }
}
