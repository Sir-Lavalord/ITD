namespace ITD.Content.Items.Weapons.Melee;

public class AlloySword : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 20;
        Item.DamageType = DamageClass.Melee;
        Item.width = 60;
        Item.height = 60;
        Item.useTime = 33;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5f;
        Item.value = 10000;
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = false;
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        MiscHelpers.GetPointOnSwungItemPath(player, 70f, 70f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out Vector2 position, out Vector2 spinningpoint);
        Vector2 value = spinningpoint.RotatedBy((double)(1.57079637f * player.direction * player.gravDir), default);
        Dust.NewDustPerfect(position, DustID.WitherLightning, new Vector2?(value * 4f), 100, default, 1f).noGravity = true;
    }
}