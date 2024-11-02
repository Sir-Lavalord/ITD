using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.NPCs
{
    public abstract class ITDNPC : ModNPC
    {
        public Rectangle HitboxTiles { get { return new Rectangle((int)(NPC.position.X / 16), (int)(NPC.position.Y / 16), NPC.width / 16, NPC.height / 16); } }
        public void CommonFrameLoop(int frameHeight, int maxFrame, float max = 5f, float increment = 1f)
        {
            NPC.frameCounter += increment;
            if (NPC.frameCounter > max)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > frameHeight * maxFrame)
                {
                    NPC.frame.Y = frameHeight;
                }
            }
        }
        public void StepUp()
        {
            int num = 0;
            if (NPC.velocity.X < 0f)
            {
                num = -1;
            }
            if (NPC.velocity.X > 0f)
            {
                num = 1;
            }
            Vector2 vector = NPC.position;
            vector.X += NPC.velocity.X;
            int num2 = (int)((vector.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * num)) / 16f);
            int num3 = (int)((vector.Y + (float)NPC.height - 1f) / 16f);
            int num4 = NPC.height / 16 + ((NPC.height % 16 == 0) ? 0 : 1);
            bool flag = true;
            bool flag2 = true;
            if (Main.tile[num2, num3] == null)
            {
                return;
            }
            for (int i = 1; i < num4 + 2; i++)
            {
                if (!WorldGen.InWorld(num2, num3 - i, 0) || Main.tile[num2, num3 - i] == null)
                {
                    return;
                }
            }
            if (!WorldGen.InWorld(num2 - num, num3 - num4, 0) || Main.tile[num2 - num, num3 - num4] == null)
            {
                return;
            }
            Tile tile;
            for (int j = 2; j < num4 + 1; j++)
            {
                if (!WorldGen.InWorld(num2, num3 - j, 0))
                {
                    return;
                }
                if (Main.tile[num2, num3 - j] == null)
                {
                    return;
                }
                tile = Main.tile[num2, num3 - j];
                flag = (flag && (!tile.HasUnactuatedTile || !Main.tileSolid[(int)tile.TileType] || Main.tileSolidTop[(int)tile.TileType]));
            }
            tile = Main.tile[num2 - num, num3 - num4];
            flag2 = (flag2 && (!tile.HasUnactuatedTile || !Main.tileSolid[(int)tile.TileType] || Main.tileSolidTop[(int)tile.TileType]));
            bool flag3 = true;
            bool flag4 = true;
            Tile tile2;
            if (Main.tile[num2, num3] == null)
            {
                return;
            }
            if (Main.tile[num2, num3 - (num4 + 1)] == null)
            {
                return;
            }
            tile = Main.tile[num2, num3 - 1];
            tile2 = Main.tile[num2, num3 - (num4 + 1)];
            flag3 = (flag3 && (!tile.HasUnactuatedTile || !Main.tileSolid[(int)tile.TileType] || Main.tileSolidTop[(int)tile.TileType] || (tile.Slope == SlopeType.SlopeDownLeft && NPC.position.X + (float)(NPC.width / 2) > (float)(num2 * 16)) || (tile.Slope == SlopeType.SlopeDownRight && NPC.position.X + (float)(NPC.width / 2) < (float)(num2 * 16 + 16)) || (tile.IsHalfBlock && (!tile2.HasUnactuatedTile || !Main.tileSolid[(int)tile2.TileType] || Main.tileSolidTop[(int)tile2.TileType]))));
            tile = Main.tile[num2, num3];
            tile2 = Main.tile[num2, num3 - 1];
            flag4 = (flag4 && ((tile.HasUnactuatedTile && (!tile.TopSlope || (tile.Slope == SlopeType.SlopeDownLeft && NPC.position.X + (float)(NPC.width / 2) < (float)(num2 * 16)) || (tile.Slope == SlopeType.SlopeDownRight && NPC.position.X + (float)(NPC.width / 2) > (float)(num2 * 16 + 16))) && (!tile.TopSlope || NPC.position.Y + (float)NPC.height > (float)(num3 * 16)) && ((Main.tileSolid[(int)tile.TileType] && !Main.tileSolidTop[(int)tile.TileType]))) || (tile2.IsHalfBlock && tile2.HasUnactuatedTile)));
            flag4 &= (!Main.tileSolidTop[(int)tile.TileType] || !Main.tileSolidTop[(int)tile2.TileType]);
            if ((float)(num2 * 16) < vector.X + (float)NPC.width && (float)(num2 * 16 + 16) > vector.X)
            {
                if (flag4 & flag3 & flag & flag2)
                {
                    float num5 = (float)(num3 * 16);
                    if (Main.tile[num2, num3 - 1].IsHalfBlock)
                    {
                        num5 -= 8f;
                    }
                    else
                    {
                        if (Main.tile[num2, num3].IsHalfBlock)
                        {
                            num5 += 8f;
                        }
                    }
                    if (num5 < vector.Y + (float)NPC.height)
                    {
                        float num6 = vector.Y + (float)NPC.height - num5;
                        if ((double)num6 <= 16.1)
                        {
                            NPC.position.Y = num5 - (float)NPC.height;
                            return;
                        }
                    }
                }
            }
        }
    }
}
