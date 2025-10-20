using ITD.Content.Rarities;

namespace ITD.Content.Items.DevTools;

public class OblivionPickaxe : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.UsesBetterMeleeItemLocation[Type] = true;
    }
    public override void SetDefaults()
    {
        Item.damage = 1020;
        Item.DamageType = DamageClass.Melee;
        Item.width = 200;
        Item.height = 168;
        Item.useTime = 2;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5;
        Item.value = Item.buyPrice(platinum: 90);
        Item.rare = ModContent.RarityType<DespoticRarity>();
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.tileBoost = 20;

        Item.pick = 9999;
        /*            Item.attackSpeedOnlyAffectsWeaponAnimation = true; // Melee speed affects how fast the tool swings for damage purposes, but not how fast it can dig
        */
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {

    }
}