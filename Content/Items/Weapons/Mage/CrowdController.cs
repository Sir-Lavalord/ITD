using ITD.Content.Projectiles.Friendly.Mage;
using ITD.Systems;
using ITD.Utilities;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Mage;

public class CrowdController : ModItem
{
    public override void SetStaticDefaults()
    {
        FrontGunLayer.RegisterData(Item.type);

        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 30;
        Item.DamageType = DamageClass.Magic;
        Item.width = 86;
        Item.height = 30;
        Item.reuseDelay = 20;
        Item.useTime = 35;
        Item.useAnimation = 35;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 2;
        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ItemRarityID.LightRed;
        Item.UseSound = SoundID.Item33;
        Item.shoot = ModContent.ProjectileType<CrowdControllerProj>();
        Item.shootSpeed = 20f;
        Item.autoReuse = true;
        Item.mana = 10;
        Item.noUseGraphic = true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Vector2 muzzleOffset = Vector2.Normalize(velocity) * 14f;
        for (int i = 0; i < 20; i++)
        {
            Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(30));
            newVelocity *= Main.rand.NextFloat(1.3f);
            int dust = Dust.NewDust(position + muzzleOffset, 20, 20, DustID.RedTorch, 0f, 0f, 0, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = newVelocity;
        }
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
        ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
        modPlayer.recoilFront = 0.9f;
        modPlayer.recoilBack = 0.9f;
        return false;
    }
    public override Vector2? HoldoutOffset()
    {
        return new Vector2(20f, -16f);
    }
    public static void Hold(Player player)
    {
        ITDPlayer modPlayer = player.GetITDPlayer();
        Vector2 mouse = modPlayer.MousePosition;

        if (mouse.X < player.Center.X)
            player.direction = -1;
        else
            player.direction = 1;

        float rotation = (Vector2.Normalize(mouse - player.MountedCenter) * player.direction).ToRotation();
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilBack * player.direction - MathHelper.PiOver2 * player.direction);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        Hold(player);
    }
    public override void HoldStyle(Player player, Rectangle heldItemFrame)
    {
        Hold(player);
    }
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");

        spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
            new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
    }
}
