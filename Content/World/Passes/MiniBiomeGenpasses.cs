using ITD.Content.Tiles.Misc;
using Terraria.DataStructures;

namespace ITD.Content.World.Passes;

[Autoload(false)]
public sealed class BlackMoldPass : ITDGenpass
{
    public override string Name => "Black Mold";
    public override double Weight => 1.0;
    public override GenpassOrder Order => new(GenpassOrderType.After, "Gem Caves");
    public override void Generate(Point16 selectedOrigin)
    {
        int every = 750;
        int amt = Main.maxTilesX / every;
        int dist = Main.maxTilesX / (amt + 2);
        int size = 70;

        int currentXOrigin = dist;

        for (int i = 0; i < amt; i++)
        {
            int y = (int)Main.rockLayer;

            ITDShapes.Ellipse ellipse = new(currentXOrigin, y, size, size);
            ellipse.LoopThroughPoints(p =>
            {
                Tile t = Main.tile[p];
                if (t.TileType == TileID.Stone)
                    t.HasTile = false;
                if (TileHelpers.EdgeTile(p) && t.TileType == TileID.Stone)
                {
                    t.TileType = (ushort)ModContent.TileType<BlackMold>();
                }
            });

            currentXOrigin += dist;
        }
    }
}
