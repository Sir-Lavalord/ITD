using Terraria.ID;

namespace ITD
{
    public class ITDSets
    {
        public static readonly int[] ToScrapeableMoss = TileID.Sets.Factory.CreateIntSet(-1);
        public static readonly int[] LeafGrowFX = TileID.Sets.Factory.CreateIntSet(GoreID.TreeLeaf_Normal);
        public static readonly bool[] SnowpoffDiggable = TileID.Sets.Factory.CreateBoolSet(TileID.SnowBlock);
        public static readonly int[] ITDChestMergeTo = TileID.Sets.Factory.CreateIntSet(defaultState: -1);
        public static readonly bool[] LavaRainEnemy = NPCID.Sets.Factory.CreateBoolSet();
    }
}