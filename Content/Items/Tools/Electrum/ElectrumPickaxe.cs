using ITD.Utilities;

namespace ITD.Content.Items.Tools.Electrum;

public class ElectrumPickaxe : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 16;
        Item.DamageType = DamageClass.Melee;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 12;
        Item.useAnimation = 18;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 3;
        Item.value = Item.buyPrice(silver: 34);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.pick = 59;
        Item.attackSpeedOnlyAffectsWeaponAnimation = true;
    }
    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Electric);
        dust.noGravity = true;
    }
}