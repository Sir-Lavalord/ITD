using Terraria.ID;

namespace ITD
{
    public class ITDSets
    {
        public static readonly int[] ToScrapeableMoss = TileID.Sets.Factory.CreateIntSet(-1);
        public static readonly int[] LeafGrowFX = TileID.Sets.Factory.CreateIntSet(GoreID.TreeLeaf_Normal);
        public static readonly bool[] SnowpoffDiggable = TileID.Sets.Factory.CreateBoolSet(TileID.SnowBlock);
    }
}