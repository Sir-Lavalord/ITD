using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Systems;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee;

public class AsterKnuckle : ModItem
{
    internal static Asset<Texture2D> InvSprite;
    public override string Texture => "ITD/Content/Items/Weapons/Melee/AsterKnuckle_Arm";
    public override void SetStaticDefaults()
    {
        ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(8, 4));
        Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

    }
    public override void SetDefaults()
    {
        Item.width = 46;
        Item.height = 46;
        Item.damage = 100;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.reuseDelay = 30;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 5;
        Item.rare = ItemRarityID.Blue;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AsterBlasterBlast>();
        Item.shootSpeed = 18f;
        Item.holdStyle = -1;
        Item.master = true;
    }

    public override void HoldItem(Player player)
    {
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraExplode"), player.Center);
        SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraPunch"), player.Center);
        ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
        modPlayer.recoilFront = 0.15f;
        modPlayer.BetterScreenshake(10, 5, 5, true);
        Projectile Blast = Projectile.NewProjectileDirect(player.GetSource_FromThis(), Item.Center, Vector2.Zero,
ModContent.ProjectileType<AsterBlasterBlast>(), Item.damage, Item.knockBack, player.whoAmI);
        Blast.ai[1] = 100f;
        Blast.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f);
        Blast.netUpdate = true;
        Vector2 Shootpos = -Vector2.Normalize(Main.MouseWorld - player.MountedCenter) * Main.rand.NextFloat(12, 16);
        for (int i = 0; i < 5; i++)
        {
            int dust3 = Dust.NewDust(player.Center, Item.width, Item.headSlot, DustID.TintableDustLighted, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 100, Color.MediumPurple, Main.rand.NextFloat(1, 1.5f));
            Main.dust[dust3].noGravity = true;
        }
        for (int i = 0; i < 5; i++)
        {
            int dust1 = Dust.NewDust(player.Center, 20, 20, DustID.TintableDustLighted, Shootpos.X, Shootpos.Y, 80, Color.MediumPurple, Main.rand.NextFloat(1.5f, 2.5f));
            Main.dust[dust1].noGravity = true;

            int dust = Dust.NewDust(player.Center, 20, 20, DustID.TintableDustLighted, Shootpos.X, Shootpos.Y, 80, Color.Turquoise, Main.rand.NextFloat(1.5f, 2.5f));
            Main.dust[dust].noGravity = true;

            int dust2 = Dust.NewDust(player.Center, 30, 30, DustID.TintableDustLighted, Shootpos.X, Shootpos.Y, 80, Color.LightGoldenrodYellow, Main.rand.NextFloat(1.5f, 2.5f));
            Main.dust[dust2].noGravity = true;
        }

        return false;
    }


    public static void SetItemInHand(Player player, Rectangle heldItemFrame)
    {
        if (player.whoAmI == Main.myPlayer)
        {

            float animProgress = 1 - player.itemTime / (float)player.itemTimeMax;

            ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

            if (mouse.X < player.Center.X)
                player.direction = -1;
            else
                player.direction = 1;

            //Default
            Vector2 itemPosition = player.MountedCenter + new Vector2(-2f * player.direction, -1f * player.gravDir);
            float itemRotation = (mouse - itemPosition).ToRotation();
            itemRotation = itemRotation * player.gravDir - modPlayer.recoilFront * player.direction;

            //Adjust for animation

            if (animProgress < 0.7f)
                itemPosition -= itemRotation.ToRotationVector2() * (1 - (float)Math.Pow(1 - (0.7f - animProgress) / 0.7f, 4)) * 4f;

            if (animProgress < 0.4f)
                itemRotation += -0.45f * (float)Math.Pow((0.4f - animProgress) / 0.4f, 2) * player.direction * player.gravDir;

            Vector2 itemSize = new(30, 20);
            Vector2 itemOrigin = new(-8, 0);
            PlayerHelpers.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin, true);
        }
    }
    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        InvSprite ??= Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Melee/AsterKnuckle");

        Texture2D properSprite = InvSprite.Value;
        position.Y -= 10;
        position.X -= 4;

        spriteBatch.Draw(properSprite, position, null, drawColor, 0f, origin, scale, 0, 0);
        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        InvSprite ??= Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Melee/AsterKnuckle");

        Texture2D properSprite = InvSprite.Value;
        spriteBatch.Draw(properSprite, Item.position - Main.screenPosition + new Vector2(0, 22), null, lightColor, rotation, properSprite.Size() / 2f, scale, 0, 0);
        return false;
    }
    public override void HoldStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
    public override void UseStyle(Player player, Rectangle heldItemFrame) => SetItemInHand(player, heldItemFrame);
}
public partial class AsterKnuckleArmPlayer : ModPlayer
{
    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        if (Player is null)
            return;

        if (Player.HeldItem.ModItem is AsterKnuckle)
        {
            PlayerDrawLayers.ArmOverItem.Hide();
        }
    }
}