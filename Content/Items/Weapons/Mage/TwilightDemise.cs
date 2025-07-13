using ITD.Content.Projectiles.Friendly.Mage;
using ITD.Systems;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Mage
{
    public class TwilightDemise : ModItem
    {
        public override void SetStaticDefaults()
        {
            HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => Color.White * 0.7f
            });
            Item.ResearchUnlockCount = 1;

            Item.staff[Type] = true;
        }
        //kylie elistaff
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.damage = 100;
            Item.mana = 6;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TwilightDemiseProj>();
            Item.shootSpeed = 12f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 120f;
            position += muzzleOffset;
            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Mage/TwilightDemise_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White * 0.7f, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
    }
}
