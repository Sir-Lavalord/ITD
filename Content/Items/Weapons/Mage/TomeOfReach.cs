using ITD.Content.Projectiles.Friendly.Mage;

namespace ITD.Content.Items.Weapons.Mage;

public class TomeOfReach : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Magic;
        Item.damage = 11;
        Item.mana = 13;
        Item.width = 56;
        Item.height = 56;
        Item.useTime = 36;
        Item.useAnimation = 36;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 4f;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item103;
        Item.shoot = ModContent.ProjectileType<ReachingHand>();
        Item.shootSpeed = 8f;
    }
}
