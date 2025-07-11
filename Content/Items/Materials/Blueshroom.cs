using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Materials
{
    public class Blueshroom : ModItem
    {
        private readonly Asset<Texture2D> glowmask = ModContent.Request<Texture2D>("ITD/Content/Items/Materials/Blueshroom_Glow");
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.buyPrice(silver: 1);
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<BlueshroomSapling>();
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            spriteBatch.Draw(glowmask.Value, Item.Center - Main.screenPosition, null, Color.White * BlueshroomTree.opac, rotation, glowmask.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }
}
