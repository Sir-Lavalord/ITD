using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria;

namespace ITD.Content.Tiles
{
    public abstract class ITDTable : ModTile
    {
        public Color MapColor { get; set; }

        /// <summary>
        /// Override to set DustType, MapColor
        /// </summary>
        public virtual void SetStaticTableDefaults()
        {
            DustType = DustID.WoodFurniture;
            MapColor = Color.White;
        }
        public override void SetStaticDefaults()
        {
            SetStaticTableDefaults();
            Main.tileTable[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.IgnoredByNpcStepUp[Type] = true;

            AdjTiles = [TileID.Tables];

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleWrapLimit = 1;
            TileObjectData.newTile.StyleMultiplier = 1;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.addTile(Type);

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

            AddMapEntry(MapColor, Language.GetText("MapObject.Table"));
        }

        public override void NumDust(int x, int y, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
