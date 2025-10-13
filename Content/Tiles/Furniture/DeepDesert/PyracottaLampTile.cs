using ITD.Content.Dusts;

namespace ITD.Content.Tiles.Furniture.DeepDesert;

public class PyracottaLampTile : ITDLamp
{
    public override bool ExtraBottomPixel => true;
    public override Asset<Texture2D> FlameTexture => ModContent.Request<Texture2D>(Texture + "_Flame");
    public override void SetStaticLampDefaults()
    {
        DustType = ModContent.DustType<PyracottaDust>();
        MapColor = new(171, 77, 57);
        LightColor = [Color.Orange.ToVector3()];
        AlternateDirection = true;
        // we can leave emitdust as dustid.torch
    }
}
