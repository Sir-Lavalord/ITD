using ITD.Content.Items.Placeable.Furniture.DeepDesert;

namespace ITD.Content.Tiles.Furniture.DeepDesert
{
    public class PyracottaCandleTile : ITDCandle
    {
        public override Asset<Texture2D> FlameTexture => ModContent.Request<Texture2D>(Texture + "_Flame");
        public override int ItemType => ModContent.ItemType<PyracottaCandle>();
        public override Vector3 GetLightColor(int i, int j)
        {
            return Color.Orange.ToVector3();
        }
    }
}
