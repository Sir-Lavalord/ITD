using ITD.Content.Projectiles.Friendly.Melee;

namespace ITD.Content.Items.Weapons.Melee
{
    public class BigBerth : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.knockBack = 2f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 20;
            Item.crit = 4;
            Item.scale = 1f;
            Item.noUseGraphic = true;
            Item.shootSpeed = 12f;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 10);
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.channel = true;
            Item.noMelee = true;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<BigBerthProj>();
        }
    }
}