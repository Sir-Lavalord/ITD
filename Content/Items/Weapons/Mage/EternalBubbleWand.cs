using System;

namespace ITD.Content.Items.Weapons.Mage;

public class EternalBubbleWand : ModItem
{
    public int attackCycle = 0;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 16;
        Item.DamageType = DamageClass.Magic;
        Item.width = 50;
        Item.height = 50;
        Item.useTime = 35;
        Item.useAnimation = 35;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5;
        Item.value = Item.sellPrice(copper: 60);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item15;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.mana = 4;
        Item.shootSpeed = 20;
    }
    int Timer;
    public override bool? UseItem(Player player)
    {
        //manual turning augh.
        if (player.whoAmI == Main.myPlayer)
        {
            if (Main.MouseWorld.X > player.Center.X)
            {
                player.direction = 1;
            }
            else player.direction = -1;
        }
        return base.UseItem(player);
    }
    public override void HoldItem(Player player)
    {
        if (player.itemAnimation == 0)
        {
            Timer = 0;
            return;
        }

        if (player.itemAnimation == player.itemAnimationMax)
        {
            Timer = player.itemAnimationMax - 2;
        }
        if (player.itemAnimation > 0)
        {
            Timer--;
        }

        if (Timer == player.itemAnimationMax / 2)
        {
        }
        if (Timer > 2 * player.itemAnimationMax / 3)
        {
            player.itemAnimation = player.itemAnimationMax - 2;
        }
        else
        {
            float prog = (float)Timer / (3 * (player.itemAnimationMax - 1) / 2);
            player.itemAnimation = (int)((player.itemAnimationMax - 1) * Math.Pow(MomentumProgress(prog), 1));
        }
        if (player.itemAnimation >= player.itemAnimationMax)
        {
            player.itemAnimation += 1;
        }
    }
    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        MiscHelpers.GetPointOnSwungItemPath(player, 60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out Vector2 spinPos, out Vector2 spinningpoint);
        Vector2 value = spinningpoint.RotatedBy((double)(MathHelper.PiOver2 * player.direction * player.gravDir), default);
        Dust.NewDustPerfect(spinPos, DustID.BreatheBubble, new Vector2?(value * 4f), 100, default, 1.5f).noGravity = true;
    }
    public static float MomentumProgress(float x)
    {
        return (x * x * 4) - (x * x * x);
    }

}