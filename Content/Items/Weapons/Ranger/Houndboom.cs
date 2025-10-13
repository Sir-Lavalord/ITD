using ITD.Systems;
using ITD.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Ranger;

public class Houndboom : ModItem
{
    public override void SetStaticDefaults()
    {
        FrontGunLayer.RegisterData(Item.type);

        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 14;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 68;
        Item.height = 24;
        Item.useTime = 20;
        Item.useAnimation = 40;
        Item.reuseDelay = 40;
        Item.useStyle = -1;
        Item.noMelee = true;
        Item.knockBack = 6;
        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Blue;
        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.shootSpeed = 6f;
        Item.useAmmo = AmmoID.Bullet;
        Item.autoReuse = false;
        Item.noUseGraphic = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        SoundEngine.PlaySound(SoundID.Item36, position);
        for (int i = 0; i < 4; i++)
        {
            Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));
            newVelocity *= 1f - Main.rand.NextFloat(0.3f);
            Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
        }
        Vector2 muzzleOffset = Vector2.Normalize(velocity) * 50f;
        position += muzzleOffset;
        for (int i = 0; i < 16; i++)
        {
            Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(25));
            newVelocity *= Main.rand.NextFloat(2f);
            int dust = Dust.NewDust(position, 0, 0, DustID.Smoke, 0f, 0f, 0, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = newVelocity;
        }
        ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
        modPlayer.recoilFront = 0.2f;
        modPlayer.recoilBack = 0.2f;
        return false;
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

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(12f, -6f);
    }
}
