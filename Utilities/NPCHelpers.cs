using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Utilities
{
    public static class NPCHelpers
    {
		public static void StepUp(ref Vector2 position, ref Vector2 velocity, int width, int height)
		{
			int num = 0;
			if (velocity.X < 0f)
			{
				num = -1;
			}
			if (velocity.X > 0f)
			{
				num = 1;
			}
			Vector2 vector = position;
			vector.X += velocity.X;
			int num2 = (int)((vector.X + (float)(width / 2) + (float)((width / 2 + 1) * num)) / 16f);
			int num3 = (int)((vector.Y + (float)height - 1f) / 16f);
			int num4 = height / 16 + ((height % 16 == 0) ? 0 : 1);
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
			flag3 = (flag3 && (!tile.HasUnactuatedTile || !Main.tileSolid[(int)tile.TileType] || Main.tileSolidTop[(int)tile.TileType] || (tile.Slope == SlopeType.SlopeDownLeft && position.X + (float)(width / 2) > (float)(num2 * 16)) || (tile.Slope == SlopeType.SlopeDownRight && position.X + (float)(width / 2) < (float)(num2 * 16 + 16)) || (tile.IsHalfBlock && (!tile2.HasUnactuatedTile || !Main.tileSolid[(int)tile2.TileType] || Main.tileSolidTop[(int)tile2.TileType]))));
			tile = Main.tile[num2, num3];
			tile2 = Main.tile[num2, num3 - 1];
			flag4 = (flag4 && ((tile.HasUnactuatedTile && (!tile.TopSlope || (tile.Slope == SlopeType.SlopeDownLeft && position.X + (float)(width / 2) < (float)(num2 * 16)) || (tile.Slope == SlopeType.SlopeDownRight && position.X + (float)(width / 2) > (float)(num2 * 16 + 16))) && (!tile.TopSlope || position.Y + (float)height > (float)(num3 * 16)) && ((Main.tileSolid[(int)tile.TileType] && !Main.tileSolidTop[(int)tile.TileType]))) || (tile2.IsHalfBlock && tile2.HasUnactuatedTile)));
			flag4 &= (!Main.tileSolidTop[(int)tile.TileType] || !Main.tileSolidTop[(int)tile2.TileType]);
			if ((float)(num2 * 16) < vector.X + (float)width && (float)(num2 * 16 + 16) > vector.X)
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
					if (num5 < vector.Y + (float)height)
					{
						float num6 = vector.Y + (float)height - num5;
						if ((double)num6 <= 16.1)
						{
							position.Y = num5 - (float)height;
							return;
						}
					}
				}
			}
		}
        //Copy from fargo test 
        public static int NewNPCEasy(IEntitySource source, Vector2 spawnPos, int type, int start = 0, float ai0 = 0, float ai1 = 0, float ai2 = 0, float ai3 = 0, int target = 255, Vector2 velocity = default)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return Main.maxNPCs;

            int n = NPC.NewNPC(source, (int)spawnPos.X, (int)spawnPos.Y, type, start, ai0, ai1, ai2, ai3, target);
            if (n != Main.maxNPCs)
            {
                if (velocity != default)
                {
                    Main.npc[n].velocity = velocity;
                }

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
            }

            return n;
        }

    }
}
