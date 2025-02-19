using ITD.Systems.DataStructures;
using Terraria.DataStructures;
using ITD.Content.TileEntities;
using Terraria;

namespace ITD.Content.Tiles
{
    public class ITDGlobalTile : GlobalTile
    {
        public override void PlaceInWorld(int i, int j, int type, Item item)
        {
            if (!ITD.ServerConfig.AutoMergeChestsIntoDoubleChest)
                return;

            // i, j is bottom left for chests here

            int possible = ITDSets.ITDChestMergeTo[type];
            if (possible > -1)
            {
                // now we can get the dimensions of this chest with tileloader
                if (TileLoader.GetTile(type) is ITDChest chest)
                {
                    Point8 dimensions = chest.Dimensions;
                    // now we can try to merge with the chest at the left, then the one at the right. the fun thing here is adding the inventories together
                    Point16 bottomLeftLeft = new(i - dimensions.X, j);
                    Point16 bottomLeftRight = new(i + dimensions.X, j);

                    // prioritize merging with the left chest first
                    Tile leftChest = Framing.GetTileSafely(bottomLeftLeft);

                    // now we can check for merge. this check might seem weird to you,
                    // but consider the possibility of two chests being on different levels, with the left one being one tile lower.
                    // when the right one is placed, the game might mistakenly think that we can merge, and cause havoc. so avoid doing that.
                    if (leftChest.TileType == type && Framing.GetTileSafely(bottomLeftLeft + new Point16(0, 1)).TileType != type) // we can merge!
                    {
                        MergeChests(bottomLeftLeft, new Point16(i, j), dimensions, possible);
                    }
                }
            }
        }
        public static void MergeChests(Point16 bottomLeft1, Point16 bottomLeft2, Point8 dimensions, int newType)
        {
            // we can use dimensions again to get the height for the TEs
            Point16 offset = new(0, dimensions.Y - 1);

            Point16 TE1 = bottomLeft1 - offset;
            Point16 TE2 = bottomLeft2 - offset;
            if (TileEntity.ByPosition.TryGetValue(TE1, out TileEntity te1) && te1 is ITDChestTE otherTe)
            {
                if (TileEntity.ByPosition.TryGetValue(TE2, out TileEntity te2) && te2 is ITDChestTE myTe)
                {
                    // we need to store both TEs' inventories, then kill both TEs before making ours

                    Item[] inv1 = otherTe.items;
                    Item[] inv2 = myTe.items;

                    // kill TEs

                    ITDChestTE chest = ModContent.GetInstance<ITDChestTE>();

                    chest.Kill(TE1.X, TE1.Y);
                    chest.Kill(TE2.X, TE2.Y);

                    // kill tiles, then place our own tile

                    for (int l = TE1.X; l < TE2.X + dimensions.X; l++)
                    {
                        for (int m = TE1.Y; m < TE1.Y + dimensions.Y; m++)
                        {
                            Tile delete = Framing.GetTileSafely(l, m);
                            delete.TileType = 0;
                            delete.HasTile = false;
                        }
                    }

                    // place our new tile

                    WorldGen.PlaceObject(bottomLeft1.X, bottomLeft1.Y, newType);
                    int te = chest.Hook_AfterPlacement(bottomLeft1.X, bottomLeft1.Y, Framing.GetTileSafely(TE1).TileType, 0, 0, 0);
                    if (TileEntity.ByID.TryGetValue(te, out TileEntity ne) && ne is ITDChestTE newChest)
                    {
                        // reconstitute the inventories

                        for (int l = 0; l < inv1.Length; l++)
                        {
                            newChest.items[l] = inv1[l];
                        }
                        for (int m = inv1.Length; m < inv1.Length + inv2.Length; m++)
                        {
                            newChest.items[m] = inv2[m - inv1.Length];
                        }
                    }
                }
            }
        }
    }
}