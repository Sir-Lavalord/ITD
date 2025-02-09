using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace ITD.Systems.DataStructures
{
    public struct TinyTile(Tile t)
    {
        public ushort TileType = t.TileType;
        public BlockType BlockType = t.BlockType;
        public bool HasTile = t.HasTile;
        public bool IsTileInvisible = t.IsTileInvisible;
        public bool IsTileFullbright = t.IsTileFullbright;
        public bool IsWallInvisible = t.IsWallInvisible;
        public bool IsWallFullbright = t.IsWallFullbright;
        public byte LiquidAmount = t.LiquidAmount;
        public int LiquidType = t.LiquidType;
        public int TileFrameNumber = t.TileFrameNumber;
        public short TileFrameX = t.TileFrameX;
        public short TileFrameY = t.TileFrameY;
        public ushort WallType = t.WallType;
        public byte WallColor = t.WallColor;
        public byte TileColor = t.TileColor;
        public int WallFrameNumber = t.WallFrameNumber;
        public int WallFrameX = t.WallFrameX;
        public int WallFrameY = t.WallFrameY;
        public bool IsActuated = t.IsActuated;
        public bool HasActuator = t.HasActuator;
        public bool RedWire = t.RedWire;
        public bool BlueWire = t.BlueWire;
        public bool GreenWire = t.GreenWire;
        public bool YellowWire = t.YellowWire;
        public readonly void CopyTo(ref Tile t)
        {
            t.TileType = TileType;
            t.BlockType = BlockType;
            t.HasTile = HasTile;
            t.IsTileInvisible = IsTileInvisible;
            t.IsTileFullbright = IsTileFullbright;
            t.IsWallInvisible = IsWallInvisible;
            t.IsWallFullbright = IsWallFullbright;
            t.LiquidAmount = LiquidAmount;
            t.LiquidType = LiquidType;
            t.TileFrameNumber = TileFrameNumber;
            t.TileFrameX = TileFrameX;
            t.TileFrameY = TileFrameY;
            t.WallType = WallType;
            t.WallColor = WallColor;
            t.TileColor = TileColor;
            t.WallFrameNumber = WallFrameNumber;
            t.WallFrameX = WallFrameX;
            t.WallFrameY = WallFrameY;
            t.IsActuated = IsActuated;
            t.HasActuator = HasActuator;
            t.RedWire = RedWire;
            t.BlueWire = BlueWire;
            t.GreenWire = GreenWire;
            t.YellowWire = YellowWire;
        }
    }
}
