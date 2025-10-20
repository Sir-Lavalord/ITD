using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Systems;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Summoner;

//inthejellyofthefish
public class Fishbacker : ModItem
{

    public override void SetStaticDefaults()
    {
        HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
        {
            Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
            Color = () => new Color(255, 255, 255, 50) * 0.7f
        });
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.SummonMeleeSpeed;
        Item.damage = 18;
        Item.knockBack = 2;
        Item.rare = ItemRarityID.LightRed;
        Item.shoot = ModContent.ProjectileType<FishbackerProj>();
        Item.shootSpeed = 4;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 35;
        Item.useAnimation = 35;
        Item.UseSound = SoundID.Item152;
        Item.noMelee = true;
        Item.noUseGraphic = true;
    }
    public override bool MeleePrefix()
    {
        return true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<FishbackerProj>(), damage, knockback, player.whoAmI, 0);
        return false;
    }
    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.MeteoriteBar, 3)
            .AddIngredient(ModContent.ItemType<StarlitBar>(), 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Texture2D texture = (Texture2D)Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Summoner/Fishbacker_Glow");

        spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
            new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    }
}