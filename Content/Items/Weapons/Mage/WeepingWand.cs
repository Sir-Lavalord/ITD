using ITD.Content.Projectiles.Friendly.Mage;


namespace ITD.Content.Items.Weapons.Mage
{
    public class WeepingWand : ModItem
    {
        public override void SetStaticDefaults()
        {
Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Magic;
            Item.width = 38;
            Item.height = 38;
            Item.scale = 1.1f;
            Item.maxStack = 1;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item20;
            Item.noMelee = true;
            Item.channel = true;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.buyPrice(silver: 25);
            Item.rare = 5;
            Item.shoot = ModContent.ProjectileType<WeepWandWisp>();
            Item.shootSpeed = 5f;
            Item.mana = 11;
        }
    }
}