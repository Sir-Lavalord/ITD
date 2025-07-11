using ITD.Content.Projectiles.Friendly.Melee;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee
{
    public class ChainedGun : ModItem
    {
        //examplemod slop
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.reuseDelay = 10;
            Item.knockBack = 2f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 20;
            Item.crit = 4;
            Item.scale = 1f;
            Item.noUseGraphic = true;
            Item.shootSpeed = 12f;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.channel = true;
            Item.noMelee = true;
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = 1;
            Item.consumeAmmoOnFirstShotOnly = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<ChainedGunProj>(), damage, knockback, player.whoAmI,0,0, type);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}