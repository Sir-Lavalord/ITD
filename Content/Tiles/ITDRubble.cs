using System;
using Terraria.DataStructures;
using Terraria.Enums;
using ITD.Systems.DataStructures;
using Terraria.ObjectData;

namespace ITD.Content.Tiles
{
    public abstract class ITDRubble : ModTile
    {
        public abstract Point8 Dimensions { get; }
        public sealed override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Width = Dimensions.X;
            TileObjectData.newTile.Height = Dimensions.Y;
            TileObjectData.newTile.CoordinateWidth = 16;

            int[] coordinateHeights = new int[Dimensions.Y];
            Array.Fill(coordinateHeights, 16);
            coordinateHeights[^1] = 18;

            TileObjectData.newTile.CoordinateHeights = coordinateHeights;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            SetStaticDefaultsSafe();
        }
        public virtual void SetStaticDefaultsSafe()
        {

        }
    }
}
