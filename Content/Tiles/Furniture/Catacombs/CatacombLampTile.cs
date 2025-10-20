namespace ITD.Content.Tiles.Furniture.Catacombs;

public class CatacombLampTile : ITDLamp
{
    public override Asset<Texture2D> FlameTexture => ModContent.Request<Texture2D>(Texture + "_Flame");
    private static readonly Vector3 blueLight = new(0.47f, 0.84f, 1f);
    private static readonly Vector3 greenLight = new(0.94f, 0.76f, 0.59f);
    private static readonly Vector3 pinkLight = new(0.96f, 0.63f, 0.36f);
    public override void SetStaticLampDefaults()
    {
        DustType = DustID.Shadowflame;
        MapColor = new Color(3, 39, 105);
        LightColor = [blueLight, greenLight, pinkLight];
        EmitDust = [DustID.BlueTorch, DustID.GreenTorch, DustID.PinkTorch];
        AlternateDirection = true;
    }
}
