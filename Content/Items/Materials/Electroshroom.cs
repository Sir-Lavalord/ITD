using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Items.Materials;

public class Electroshroom : ModItem
{
    private const int animFrames = 15;
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(3, animFrames));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        Item.ResearchUnlockCount = 100;
    }
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 22;
        Item.value = Item.buyPrice(silver: 1);
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Green;
        Item.maxStack = Item.CommonMaxStack;
    }
    public override void PostUpdate()
    {
        Lighting.AddLight(Item.Center, Color.Cyan.ToVector3() * 0.3f * Main.essScale);
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D texture = TextureAssets.Item[Type].Value;
        Rectangle? frame = null;
        if (Main.itemAnimations[Type] != null)
            frame = Main.itemAnimations[Type].GetFrame(texture);
        spriteBatch.Draw(texture, Item.Center - Main.screenPosition, frame, Color.White, rotation, new Vector2(texture.Width / 2f, texture.Height / animFrames / 2f), scale, SpriteEffects.None, 0f);
        return false;
    }
}