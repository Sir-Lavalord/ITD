using ITD.Content.UI;
using ITD.Players;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Items.DevTools
{
    public class MirrorMan : DevTool
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
        public Point offset;
        public byte inputTimer;
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
        public override void ProcessInput(KeyboardState k)
        {
            if (inputTimer < 2)
            {
                inputTimer++;
                return;
            }
            inputTimer = 0;
            if (k.IsKeyDown(Keys.Right))
            {
                offset.X++;
            }
            if (k.IsKeyDown(Keys.Left))
            {
                offset.X--;
            }
            if (k.IsKeyDown(Keys.Up))
            {
                offset.Y--;
            }
            if (k.IsKeyDown(Keys.Down))
            {
                offset.Y++;
            }
        }
        public override bool? UseItem(Player player)
        {
            ITDPlayer plr = player.GetITDPlayer();
            if (player.altFunctionUse == 2)
            {
                if (plr.selectBox)
                {
                    plr.selectTopLeft = Point16.Zero;
                    plr.selectBottomRight = Point16.Zero;
                    plr.selectBox = false;
                    plr.selectBounds = Rectangle.Empty;
                    PlayerLog(player, "Cancelled select!");
                    return true;
                }
                MirrorManUI ui = UILoader.GetUIState<MirrorManUI>();
                ui.Toggle();
                return true;
            }
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
                    PlayerLog(player, $"Selected area with {tiles.Width * tiles.Height} tiles.");
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

                Point start = Main.MouseWorld.ToTileCoordinates() + offset;

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
        public override void DrawSpecialPreviews(SpriteBatch sb, Player player)
        {
            ITDPlayer plr = player.GetITDPlayer();
            if (Select)
                return;
            Vector2 MousePosition = plr.MousePosition;
            MirroringState flags = State;
            if (tilesRect != null)
            {
                bool mirrorX = flags.HasFlag(MirroringState.MirrorHorizontally);
                bool mirrorY = flags.HasFlag(MirroringState.MirrorVertically);

                int width = tilesRect.GetLength(0);
                int height = tilesRect.GetLength(1);

                Vector2 baseDrawPos = (MousePosition.ToTileCoordinates() + offset).ToWorldCoordinates(0, 0) - Main.screenPosition;
                Vector2 baseStartPosition = MousePosition.ToTileCoordinates().ToWorldCoordinates(0, 0) - Main.screenPosition;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int drawI = mirrorX ? width - 1 - i : i;
                        int drawJ = mirrorY ? height - 1 - j : j;

                        TinyTile t = tilesRect[drawI, drawJ];
                        if (t.WallType == WallID.None)
                            continue;

                        Texture2D tex = TextureAssets.Wall[t.WallType].Value;
                        Vector2 drawOffset = new(i * 16, j * 16);

                        sb.Draw(tex, baseDrawPos + drawOffset, new Rectangle(t.WallFrameX, t.WallFrameY, 32, 32),
                                Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int drawI = mirrorX ? width - 1 - i : i;
                        int drawJ = mirrorY ? height - 1 - j : j;

                        TinyTile t = tilesRect[drawI, drawJ];
                        if (!t.HasTile)
                            continue;

                        Texture2D tex = TextureAssets.Tile[t.TileType].Value;
                        Vector2 drawOffset = new(i * 16, j * 16);

                        sb.Draw(tex, baseDrawPos + drawOffset, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16),
                                Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }

                DynamicSpriteFont d = FontAssets.MouseText.Value;
                if (offset.X != 0)
                {
                    string msg = $"X: {offset.X}";
                    Vector2 siz = d.MeasureString(msg);
                    sb.DrawString(d, $"X: {offset.X}", baseStartPosition + new Vector2((offset.X < 0) ? -siz.X : 0, (offset.Y < 0) ? 6f : -24f), Color.White);
                    Vector2 point2 = baseStartPosition + new Vector2(offset.X * 16f, 0f);
                    sb.DrawLine(baseStartPosition, point2, Color.Red);
                    sb.DrawDottedLine(point2, baseDrawPos, Color.Red, 5f, 3f);
                }
                if (offset.Y != 0)
                {
                    string msg = $"Y: {offset.Y}";
                    Vector2 siz = d.MeasureString(msg);
                    sb.DrawString(d, msg, baseStartPosition + new Vector2((offset.X < 0) ? 6f : -siz.X - 6f, (offset.Y < 0) ? -18f : 0f), Color.White);
                    Vector2 point2 = baseStartPosition + new Vector2(0f, offset.Y * 16f);
                    sb.DrawLine(baseStartPosition, point2, Color.Red);
                    sb.DrawDottedLine(point2, baseDrawPos, Color.Red, 5f, 3f);
                }
            }
        }
    }
}
