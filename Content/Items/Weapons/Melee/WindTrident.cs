using ITD.Content.Projectiles.Friendly.Melee;

namespace ITD.Content.Items.Weapons.Melee
{
    public class WindTrident : ModItem
    {
        public override void SetStaticDefaults()
        {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.Spears[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Cyan;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.damage = 400;
            Item.shootSpeed = 3.5f;
            Item.shoot = ModContent.ProjectileType<WindTridentProjectile>();
            Item.channel = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
    }
}
