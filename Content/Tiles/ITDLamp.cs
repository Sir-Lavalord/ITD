using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Tiles
{
    public abstract class ITDLamp : ModTile
    {
        public Color MapColor { get; set; }
        /// <summary>
        /// Determines whether when placed, every other lamp in the X axis will alternate its draw direction.
        /// </summary>
        public bool AlternateDirection { get; set; }
        public virtual Texture2D FlameTexture => TextureAssets.Flames[1].Value;
        public Vector3[] LightColor { get; set; }
        public int[] EmitDust {  get; set; }
        /// <summary>
        /// Override to set DustType, MapColor, LightColor, EmitDust and AlternateDirection. To change flame texture, override FlameTexture.
        /// </summary>
        public virtual void SetStaticLampDefaults()
        {
            DustType = DustID.WoodFurniture;
            MapColor = Color.White;
            LightColor = [new Vector3(0.5f, 0.5f, 0.5f)];
            EmitDust = [DustID.Torch];
            AlternateDirection = false;
        }
        public override void SetStaticDefaults()
        {
            SetStaticLampDefaults();
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.DrawFlipHorizontal = AlternateDirection;
            //TileObjectData.newTile.StyleLineSkip = 2;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.addTile(Type);

            AddMapEntry(MapColor, Language.GetText("MapObject.FloorLamp"));
        }
        public override void HitWire(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int topY = j - tile.TileFrameY / 18 % 3;
            short frameAdjustment = (short)(tile.TileFrameX > 0 ? -18 : 18);

            Framing.GetTileSafely(i, topY).TileFrameX += frameAdjustment;
            Framing.GetTileSafely(i, topY + 1).TileFrameX += frameAdjustment;
            Framing.GetTileSafely(i, topY + 2).TileFrameX += frameAdjustment;

            Wiring.SkipWire(i, topY);
            Wiring.SkipWire(i, topY + 1);
            Wiring.SkipWire(i, topY + 2);

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
            }
        }
        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 0 && AlternateDirection)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX == 0)
            {
                int style = tile.TileFrameY / 54;
                Vector3 lightColor = LightColor[style];
                r = lightColor.X;
                g = lightColor.Y;
                b = lightColor.Z;
            }
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.gamePaused || !Main.instance.IsActive || Lighting.UpdateEveryFrame && !Main.rand.NextBool(4))
            {
                return;
            }

            Tile tile = Framing.GetTileSafely(i, j);

            if (!TileDrawing.IsVisible(tile))
            {
                return;
            }

            short frameX = tile.TileFrameX;
            short frameY = tile.TileFrameY;

            if (frameX != 0 || !Main.rand.NextBool(40))
            {
                return;
            }

            int style = frameY / 54;

            if (frameY / 18 % 3 == 0)
            {
                int dustChoice = -1;

                dustChoice = EmitDust[style];

                if (dustChoice != -1)
                {
                    var dust = Dust.NewDustDirect(new Vector2(i * 16 + 4, j * 16 + 2), 4, 4, dustChoice, 0f, 0f, 100, default, 1f);

                    if (!Main.rand.NextBool(3))
                    {
                        dust.noGravity = true;
                    }

                    dust.velocity *= 0.3f;
                    dust.velocity.Y = dust.velocity.Y - 1.5f;
                }
            }
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (!TileDrawing.IsVisible(tile))
            {
                return;
            }

            SpriteEffects effects = SpriteEffects.None;

            if (i % 2 == 0 && AlternateDirection)
            {
                effects = SpriteEffects.FlipHorizontally;
            }

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);

            int width = 16;
            int offsetY = 0;
            int height = 16;
            short frameX = tile.TileFrameX;
            short frameY = tile.TileFrameY;

            TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);

            ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);

            for (int c = 0; c < 7; c++)
            {
                float shakeX = Terraria.Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                float shakeY = Terraria.Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;
                Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero;
                spriteBatch.Draw(FlameTexture, position, new Rectangle(frameX, frameY, width, height), Color.White, 0f, default, 1f, effects, 0f);
            }
        }
    }
}
