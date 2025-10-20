using ITD.Utilities;
using ITD.Utilities.Placeholders;
using Terraria.DataStructures;

namespace ITD.Content.Items.Accessories.Gimmicks.IncomprehensiblePolyhedron;

public class IncomprehensiblePolyhedron : ModItem
{
    public const string AssetPath = "ITD/Content/Items/Accessories/Gimmicks/IncomprehensiblePolyhedron/";
    public override string Texture => Placeholder.PHBigGun;
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.DefaultToAccessory();
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<A3DPlayer>().Active = true;
    }
}
public class A3DPlayer : ModPlayer
{
    public bool Active = false;
    public override void ResetEffects()
    {
        Active = false;
    }
    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (!Active)
            return;
        /*
        Matrix world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
        Matrix view = Main.GameViewMatrix.TransformationMatrix;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, -1);
        */
        //model.Draw(world, view, projection);
    }
    public override void HideDrawLayers(PlayerDrawSet drawInfo)
    {
        if (!Active)
            return;
        foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
        {
            layer.Hide();
        }
    }
}
