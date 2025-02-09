using ITD.Content.UI;
using ITD.Players;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using System;
using Terraria.DataStructures;

namespace ITD.Content.Items.DevTools
{
    public class MirrorMan : ModItem
    {
        [Flags]
        public enum MirroringState : byte
        {
            MirrorNone = 1,
            MirrorHorizontally = 2,
            MirrorVertically = 4,
        }
        public bool Select => UILoader.GetUIState<MirrorManUI>().selectToggled;
        public MirroringState State
        {
            get
            {
                MirroringState final = MirroringState.MirrorNone;
                MirrorManUI ui = UILoader.GetUIState<MirrorManUI>();
                if (ui.horiToggled)
                    final |= MirroringState.MirrorHorizontally;
                if (ui.vertiToggled)
                    final |= MirroringState.MirrorVertically;
                return final;
            }
        }
        public TinyTile[,] tilesRect;
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.useTime = Item.useAnimation = 8;
            Item.autoReuse = false;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.Swing;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                MirrorManUI ui = UILoader.GetUIState<MirrorManUI>();
                ui.Toggle();
                return true;
            }
            ITDPlayer plr = player.GetITDPlayer();
            Vector2 mouse = plr.MousePosition;
            if (Select)
            {
                if (!plr.selectBox)
                {
                    plr.selectBox = true;
                    return true;
                }
                else
                {
                    plr.selectTopLeft = Point16.Zero;
                    plr.selectBottomRight = Point16.Zero;
                    plr.selectBox = false;
                    Rectangle tiles = plr.selectBounds;
                    tilesRect = new TinyTile[tiles.Width, tiles.Height];
                    Point16 tl = new(tiles.X, tiles.Y);
                    for (int i = tiles.X; i < tiles.Right; i++)
                    {
                        for (int j = tiles.Y; j < tiles.Bottom; j++)
                        {
                            int xNormal = i - tl.X;
                            int yNormal = j - tl.Y;
                            Tile original = Framing.GetTileSafely(i, j);
                            tilesRect[xNormal, yNormal] = new(original);
                        }
                    }
                    plr.selectBounds = Rectangle.Empty;
                    Main.NewText($"Selected area with {tiles.Width * tiles.Height} tiles.");
                    return true;
                }
            }
            else
            {
                if (tilesRect == null)
                    return true;

                MirroringState flags = State;
                bool mirrorX = flags.HasFlag(MirroringState.MirrorHorizontally);
                bool mirrorY = flags.HasFlag(MirroringState.MirrorVertically);

                Point start = Main.MouseWorld.ToTileCoordinates();

                int width = tilesRect.GetLength(0);
                int height = tilesRect.GetLength(1);

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int placeI = mirrorX ? width - 1 - i : i;
                        int placeJ = mirrorY ? height - 1 - j : j;
                        Tile t = Framing.GetTileSafely(start.X + i, start.Y + j);
                        TinyTile tt = tilesRect[placeI, placeJ];

                        if (tt.HasTile)
                            tt.CopyTileTo(ref t);

                        if (tt.WallType != WallID.None)
                            tt.CopyWallTo(ref t);

                        tt.CopyWiringTo(ref t);
                        tt.CopyLiquidTo(ref t);

                        if (!tt.HasTile)
                            continue;

                        SlopeType originalSlope = t.Slope;

                        if (mirrorX)
                        {
                            switch (originalSlope)
                            {
                                case SlopeType.SlopeDownLeft: originalSlope = SlopeType.SlopeDownRight; break;
                                case SlopeType.SlopeDownRight: originalSlope = SlopeType.SlopeDownLeft; break;
                                case SlopeType.SlopeUpLeft: originalSlope = SlopeType.SlopeUpRight; break;
                                case SlopeType.SlopeUpRight: originalSlope = SlopeType.SlopeUpLeft; break;
                            }
                        }

                        if (mirrorY)
                        {
                            switch (originalSlope)
                            {
                                case SlopeType.SlopeDownLeft: originalSlope = SlopeType.SlopeUpLeft; break;
                                case SlopeType.SlopeUpLeft: originalSlope = SlopeType.SlopeDownLeft; break;
                                case SlopeType.SlopeDownRight: originalSlope = SlopeType.SlopeUpRight; break;
                                case SlopeType.SlopeUpRight: originalSlope = SlopeType.SlopeDownRight; break;
                            }
                        }

                        t.Slope = originalSlope;
                    }
                }
                TileHelpers.CallFraming(start.X, start.Y, width, height);
                TileHelpers.Sync(start.X, start.Y, width, height);
            }
            return true;
        }
    }
}
