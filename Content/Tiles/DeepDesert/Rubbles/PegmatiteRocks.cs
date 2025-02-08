using ITD.Content.Items.Placeable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ObjectData;
using Terraria.Enums;

namespace ITD.Content.Tiles.DeepDesert.Rubbles
{
    // Example3x2RubbleBase is an abstract class, it is not an actual tile, but the other 2 classes in this file will reuse the Texture and SetStaticDefaults code shown here because they inherit from it. 
    public abstract class Example3x2RubbleBase : ModTile
    {
        public sealed override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            SetStaticDefaultsSafe();
        }
        public virtual void SetStaticDefaultsSafe()
        {

        }
    }
    public class PegmatiteRubble3x2 : Example3x2RubbleBase
    {
        public override void SetStaticDefaultsSafe()
        {
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Pegmatite>(), Type, 0, 1);
        }
    }
    public class PegmatiteRubble4x2 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Pegmatite>(), Type, 0);
        }
    }
}
