using Humanizer;
using ITD.Content.UI;
using ITD.Networking;
using ITD.Players;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        tt.CopyTo(ref t);
                        if (mirrorX)
                        {
                            if (t.Slope == SlopeType.SlopeDownLeft)
                                t.Slope = SlopeType.SlopeDownRight;
                            if (t.Slope == SlopeType.SlopeDownRight)
                                t.Slope = SlopeType.SlopeDownLeft;
                            if (t.Slope == SlopeType.SlopeUpLeft)
                                t.Slope = SlopeType.SlopeUpRight;
                            if (t.Slope == SlopeType.SlopeUpRight)
                                t.Slope = SlopeType.SlopeUpLeft;
                        }
                        if (mirrorY)
                        {
                            if (t.Slope == SlopeType.SlopeDownLeft)
                                t.Slope = SlopeType.SlopeUpLeft;
                            if (t.Slope == SlopeType.SlopeUpLeft)
                                t.Slope = SlopeType.SlopeDownLeft;
                            if (t.Slope == SlopeType.SlopeDownRight)
                                t.Slope = SlopeType.SlopeUpRight;
                            if (t.Slope == SlopeType.SlopeUpRight)
                                t.Slope = SlopeType.SlopeDownRight;
                        }
                    }
                }
                return true;
            }
            return base.UseItem(player);
        }
    }
}
