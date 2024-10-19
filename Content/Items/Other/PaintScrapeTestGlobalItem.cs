using ITD.Content.Tiles.BlueshroomGroves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Other
{
    public class PaintScrapeTestGlobalItem : GlobalItem
    {
        public override bool? UseItem(Item item, Player player)
        {
            if (ItemID.Sets.IsPaintScraper[item.type] && player.whoAmI == Main.myPlayer)
            {
                int tX = Player.tileTargetX;
                int tY = Player.tileTargetY;
                Tile t = Framing.GetTileSafely(tX, tY);
                int toScrapableItem = ITDSets.ToScrapeableMoss[t.TileType];
                if (player.IsInTileInteractionRange(tX, tY, TileReachCheckSettings.Simple) && toScrapableItem != -1)
                {
                    player.cursorItemIconEnabled = true;
                    if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem)
                        return null;
                    WorldGen.KillTile(tX, tY);
                    if (Framing.GetTileSafely(tX, tY).HasTile)
                        return null;
                    player.ApplyItemTime(item);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tX, tY);
                    if (Main.rand.NextBool(9))
                    {
                        int number = Item.NewItem(new EntitySource_ItemUse(player, item), tX * 16, tY * 16, 16, 16, toScrapableItem);
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
                    }
                }
            }
            return null;
        }
    }
}
