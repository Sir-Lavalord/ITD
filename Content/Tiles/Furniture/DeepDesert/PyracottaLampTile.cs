using ITD.Content.Dusts;

namespace ITD.Content.Tiles.Furniture.DeepDesert
{
    public class PyracottaLampTile : ITDLamp
    {
        public override Asset<Texture2D> FlameTexture => ModContent.Request<Texture2D>(Texture + "_Flame");
        public override void SetStaticLampDefaults()
        {
            DustType = ModContent.DustType<PyracottaDust>();
            MapColor = new Color(11, 67, 67);
            LightColor = [Color.Orange.ToVector3()];
            AlternateDirection = true;
            // we can leave emitdust as dustid.torch
        }
    }
}
