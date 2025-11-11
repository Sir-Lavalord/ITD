using ITD.Content.UI;
using ITD.Systems;
using ITD.Systems.DataStructures;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Items.DevTools;

public class MirrorMan : DevTool
{
    [Flags]
    public enum MirroringState : byte
    {
        MirrorNone = 1,
        MirrorHorizontally = 2,
        MirrorVertically = 4,
    }
    public static bool Select => UILoader.GetUIState<MirrorManUI>().selectToggled;
    public static bool Cut => UILoader.GetUIState<MirrorManUI>().cutToggle.toggled;
    public static MirroringState State
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
    public TinyTile[,] undoHistory;
    public Point undoStart;
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
        ITDPlayer plr = player.ITD();
        if (player.altFunctionUse == 2)
        {
            if (plr.selectBox)
            {
                plr.selectTopLeft = Point16.Zero;
                plr.selectBottomRight = Point16.Zero;
                plr.selectBox = false;
                plr.selectBounds = Rectangle.Empty;
                PlayerLog(player, "Cancelled Select!");
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
                        tilesRect[xNormal, yNormal] = new(ref original);
                        if (Cut)
                        {
                            TileDataType remove = 0;

                            SimpleTileDataType f = plr.tileDataSelection;

                            bool removeTile = (f & SimpleTileDataType.Tile) == SimpleTileDataType.Tile;
                            bool removeWall = (f & SimpleTileDataType.Wall) == SimpleTileDataType.Wall;
                            bool removeLiquid = (f & SimpleTileDataType.Liquid) == SimpleTileDataType.Liquid;
                            bool removeWiring = (f & SimpleTileDataType.Wiring) == SimpleTileDataType.Wiring;

                            if (removeTile)
                                remove |= TileDataType.Tile | TileDataType.TilePaint | TileDataType.Slope;

                            if (removeWall)
                                remove |= TileDataType.Wall | TileDataType.WallPaint;

                            if (removeLiquid)
                                remove |= TileDataType.Liquid;

                            if (removeWiring)
                                remove |= TileDataType.Wiring;

                            original.Clear(remove);

                            // let's handle actuator data separately. actuators themselves should be wiring, while actuated state should be tile

                            if (removeTile)
                                original.IsActuated = false;

                            if (removeWiring)
                                original.HasActuator = false;
                        }
                    }
                }
                PlayerLog(player, $"Selected area with {tiles.Width * tiles.Height} tiles.");
                if (Cut)
                    TileHelpers.Sync(plr.selectBounds);
                plr.selectBounds = Rectangle.Empty;
                return true;
            }
        }
        else
        {
            if (tilesRect == null)
                return true;

            MirroringState flags = State;
            bool mirrorX = (flags & MirroringState.MirrorHorizontally) == MirroringState.MirrorHorizontally;
            bool mirrorY = (flags & MirroringState.MirrorVertically) == MirroringState.MirrorVertically;

            Point start = Main.MouseWorld.ToTileCoordinates() + offset;

            int width = tilesRect.GetLength(0);
            int height = tilesRect.GetLength(1);
            undoHistory = new TinyTile[width, height];
            undoStart = start;
            UILoader.GetUIState<MirrorManUI>().undo.canBeToggled = true;

            SimpleTileDataType f = plr.tileDataSelection;
            bool tileData = (f & SimpleTileDataType.Tile) == SimpleTileDataType.Tile;
            bool wallData = (f & SimpleTileDataType.Wall) == SimpleTileDataType.Wall;
            bool liquidData = (f & SimpleTileDataType.Liquid) == SimpleTileDataType.Liquid;
            bool wiringData = (f & SimpleTileDataType.Wiring) == SimpleTileDataType.Wiring;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int placeI = mirrorX ? width - 1 - i : i;
                    int placeJ = mirrorY ? height - 1 - j : j;
                    Tile t = Framing.GetTileSafely(start.X + i, start.Y + j);
                    undoHistory[i, j] = new(ref t);
                    TinyTile tt = tilesRect[placeI, placeJ];

                    if (tileData && tt.HasTile)
                        tt.CopyTileTo(ref t);

                    if (wallData && tt.WallType != WallID.None)
                        tt.CopyWallTo(ref t);

                    if (liquidData)
                        tt.CopyLiquidTo(ref t);

                    if (wiringData)
                        tt.CopyWiringTo(ref t);

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
    public void DoUndo()
    {
        int width = undoHistory.GetLength(0);
        int height = undoHistory.GetLength(1);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TinyTile source = undoHistory[i, j];
                Tile go = Framing.GetTileSafely(undoStart.X + i, undoStart.Y + j);
                source.CopyTo(ref go);
            }
        }
        TileHelpers.Sync(undoStart.X, undoStart.Y, width, height);
        undoStart = Point.Zero;
        undoHistory = null;
    }
    public override void DrawSpecialPreviews(SpriteBatch sb, Player player)
    {
        ITDPlayer plr = player.ITD();
        if (Select)
            return;
        Vector2 MousePosition = plr.MousePosition;
        MirroringState flags = State;
        if (tilesRect != null)
        {
            SimpleTileDataType f = plr.tileDataSelection;

            bool tileData = (f & SimpleTileDataType.Tile) == SimpleTileDataType.Tile;
            bool wallData = (f & SimpleTileDataType.Wall) == SimpleTileDataType.Wall;
            bool liquidData = (f & SimpleTileDataType.Liquid) == SimpleTileDataType.Liquid;
            bool wiringData = (f & SimpleTileDataType.Wiring) == SimpleTileDataType.Wiring;

            bool mirrorX = (flags & MirroringState.MirrorHorizontally) == MirroringState.MirrorHorizontally;
            bool mirrorY = (flags & MirroringState.MirrorVertically) == MirroringState.MirrorVertically;

            int width = tilesRect.GetLength(0);
            int height = tilesRect.GetLength(1);

            TinyTile GetTileSafely(int i, int j)
            {
                if (i >= 0 && i < width && j >= 0 && j < height)
                    return tilesRect[i, j];
                return new TinyTile();
            }

            Vector2 baseDrawPos = (MousePosition.ToTileCoordinates() + offset).ToWorldCoordinates(0, 0) - Main.screenPosition;
            Vector2 baseStartPosition = MousePosition.ToTileCoordinates().ToWorldCoordinates(0, 0) - Main.screenPosition;

            if (wallData || liquidData)
            {
                Color water = Color.Blue;
                Color lava = Color.OrangeRed;
                Color honey = Color.Orange;
                Color shimmer = Color.Pink;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int drawI = mirrorX ? width - 1 - i : i;
                        int drawJ = mirrorY ? height - 1 - j : j;

                        TinyTile t = tilesRect[drawI, drawJ];

                        Vector2 drawOffset = new(i * 16, j * 16);
                        Vector2 drawPos = baseDrawPos + drawOffset;

                        if (wallData && t.WallType != WallID.None)
                        {
                            Texture2D tex = TextureAssets.Wall[t.WallType].Value;

                            sb.Draw(tex, drawPos, new Rectangle(t.WallFrameX, t.WallFrameY, 32, 32),
                                    Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        }
                        if (liquidData && t.LiquidAmount > 0)
                        {
                            float amount = t.LiquidAmount / 255f;
                            Rectangle target = new((int)drawPos.X, (int)drawPos.Y + (int)(16f * (1f - amount)), 16, (int)(16f * amount));
                            Color color = t.LiquidType switch
                            {
                                LiquidID.Water => water,
                                LiquidID.Lava => lava,
                                LiquidID.Honey => honey,
                                LiquidID.Shimmer => shimmer,
                                _ => Color.White,
                            };
                            sb.Draw(ITD.TrueMagicPixel.Value, target, color * 0.5f);
                        }
                    }
                }
            }

            if (tileData || wiringData)
            {
                Texture2D wireTex = TextureAssets.WireNew.Value;
                Texture2D actuatorTex = TextureAssets.Actuator.Value;
                Rectangle wireTileFrame = new(0, 0, 16, 16);

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int drawI = mirrorX ? width - 1 - i : i;
                        int drawJ = mirrorY ? height - 1 - j : j;

                        TinyTile t = tilesRect[drawI, drawJ];

                        Vector2 drawOffset = new(i * 16, j * 16);
                        Vector2 drawPos = baseDrawPos + drawOffset;

                        if (tileData && t.HasTile)
                        {
                            Texture2D tex = TextureAssets.Tile[t.TileType].Value;

                            Color drawColor = t.IsActuated ? Color.LightGray : Color.White;
                            sb.Draw(tex, drawPos, new Rectangle(t.TileFrameX, t.TileFrameY, 16, 16),
                                    drawColor * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        }

                        if (wiringData)
                        {
                            // Main.DrawWires is a NIGHTMARE

                            byte frameOffset = 0;

                            if (t.HasTile)
                            {
                                if (t.TileType == TileID.WirePipe)
                                {
                                    switch (t.TileFrameX / 18)
                                    {
                                        case 0:
                                            frameOffset += 72;
                                            break;
                                        case 1:
                                            frameOffset += 144;
                                            break;
                                        case 2:
                                            frameOffset += 216;
                                            break;
                                    }
                                }
                                else if (t.TileType == TileID.PixelBox)
                                    frameOffset += 72;
                            }

                            #region Draw Wires
                            if (t.RedWire)
                            {
                                ushort wireTileFrameX = 0;
                                if (GetTileSafely(drawI, drawJ - 1).RedWire)
                                {
                                    wireTileFrameX += 18;
                                }
                                if (GetTileSafely(drawI + 1, drawJ).RedWire)
                                {
                                    wireTileFrameX += 36;
                                }
                                if (GetTileSafely(drawI, drawJ + 1).RedWire)
                                {
                                    wireTileFrameX += 72;
                                }
                                if (GetTileSafely(drawI - 1, drawJ).RedWire)
                                {
                                    wireTileFrameX += 144;
                                }

                                wireTileFrame.Y = frameOffset;
                                wireTileFrame.X = wireTileFrameX;

                                sb.Draw(wireTex, drawPos, wireTileFrame, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            }
                            if (t.BlueWire)
                            {
                                ushort wireTileFrameX = 0;
                                if (GetTileSafely(drawI, drawJ - 1).BlueWire)
                                {
                                    wireTileFrameX += 18;
                                }
                                if (GetTileSafely(drawI + 1, drawJ).BlueWire)
                                {
                                    wireTileFrameX += 36;
                                }
                                if (GetTileSafely(drawI, drawJ + 1).BlueWire)
                                {
                                    wireTileFrameX += 72;
                                }
                                if (GetTileSafely(drawI - 1, drawJ).BlueWire)
                                {
                                    wireTileFrameX += 144;
                                }

                                wireTileFrame.Y = frameOffset + 18;
                                wireTileFrame.X = wireTileFrameX;

                                sb.Draw(wireTex, drawPos, wireTileFrame, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            }
                            if (t.GreenWire)
                            {
                                ushort wireTileFrameX = 0;
                                if (GetTileSafely(drawI, drawJ - 1).GreenWire)
                                {
                                    wireTileFrameX += 18;
                                }
                                if (GetTileSafely(drawI + 1, drawJ).GreenWire)
                                {
                                    wireTileFrameX += 36;
                                }
                                if (GetTileSafely(drawI, drawJ + 1).GreenWire)
                                {
                                    wireTileFrameX += 72;
                                }
                                if (GetTileSafely(drawI - 1, drawJ).GreenWire)
                                {
                                    wireTileFrameX += 144;
                                }

                                wireTileFrame.Y = frameOffset + 36;
                                wireTileFrame.X = wireTileFrameX;

                                sb.Draw(wireTex, drawPos, wireTileFrame, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            }
                            if (t.YellowWire)
                            {
                                ushort wireTileFrameX = 0;
                                if (GetTileSafely(drawI, drawJ - 1).YellowWire)
                                {
                                    wireTileFrameX += 18;
                                }
                                if (GetTileSafely(drawI + 1, drawJ).YellowWire)
                                {
                                    wireTileFrameX += 36;
                                }
                                if (GetTileSafely(drawI, drawJ + 1).YellowWire)
                                {
                                    wireTileFrameX += 72;
                                }
                                if (GetTileSafely(drawI - 1, drawJ).YellowWire)
                                {
                                    wireTileFrameX += 144;
                                }

                                wireTileFrame.Y = frameOffset + 54;
                                wireTileFrame.X = wireTileFrameX;

                                sb.Draw(wireTex, drawPos, wireTileFrame, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            }
                            #endregion

                            if (t.HasActuator)
                                sb.Draw(actuatorTex, drawPos, null, Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        }
                    }
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
