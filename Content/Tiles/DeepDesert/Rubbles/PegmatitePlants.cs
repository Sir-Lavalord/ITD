using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.GameContent;
using ITD.Content.Items.Placeable;

namespace ITD.Content.Tiles.DeepDesert.Rubbles
{
    public class PegmatitePlant4x4 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LightPyracotta>(), Type, 0);
        }
    }
    public class PegmatitePlant3x2 : Example3x2RubbleBase
    {
        public override void SetStaticDefaultsSafe()
        {
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LightPyracotta>(), Type, 0);
        }
    }
    public class PegmatitePlant2x2 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LightPyracotta>(), Type, 0);
        }
    }
}
