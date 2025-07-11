namespace ITD.Content.Items.Materials
{
    public class EmberlionSclerite : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 18;
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.Green;
            Item.maxStack = Item.CommonMaxStack;
        }
    }
}
