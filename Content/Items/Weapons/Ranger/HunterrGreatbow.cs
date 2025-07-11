using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Content.Rarities;
using ITD.Systems;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class HunterrGreatbow : ModItem
    {
        public override void SetStaticDefaults()
        {
            HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => Color.White
            });
            Item.ResearchUnlockCount = 1;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 86;
            Item.damage = 200;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 40;
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
        public override bool NeedsAmmo(Player player)
        {
            return false;
        }

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
