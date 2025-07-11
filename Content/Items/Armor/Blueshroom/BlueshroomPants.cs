namespace ITD.Content.Items.Armor.Blueshroom
{
    [AutoloadEquip(EquipType.Legs)]
    public class BlueshroomPants : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 3;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetAttackSpeed(DamageClass.Default) += 0.03f;
        }
    }
}
