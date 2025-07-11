using Terraria.DataStructures;
using ITD.Content.Projectiles.Friendly.Ranger;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class ResonanceCrossbow : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 65;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 86;
            Item.height = 30;
            Item.reuseDelay = 15;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.DD2_BallistaTowerShot;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 30f;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = true;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20f, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            type = ModContent.ProjectileType<ResonanceBar>();
            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
    }
}
