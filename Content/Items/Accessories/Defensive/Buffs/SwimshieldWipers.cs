namespace ITD.Content.Items.Accessories.Defensive.Buffs
{
    public class SwimshieldWipers : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(26, 20);
        }
        // TODO: should other blindness mods buffs like calamity's be included?
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Blackout] = true;
        }
    }
}
