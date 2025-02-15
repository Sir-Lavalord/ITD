using ITD.Content.Tiles;
using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.DataStructures;

namespace ITD.Content.TileEntities
{
    public abstract class ITDChestTE : ModTileEntity
    {
        public virtual Point8 Dimensions => new(2, 2);
        /// <summary>
        /// For reference, vanilla uses (10, 4).
        /// </summary>
        public virtual Point8 StorageDimensions => new(10, 4);
        /// <summary>
        /// Not an array. Impossible to initialize well.
        /// Not a 2D array either, for the same reason, plus we can deduce item positions from the chest's storage dimensions.
        /// </summary>
        internal Item[,] items;
        public string StorageName = "";
        public short OpenedBy = -1;
        public byte frameCounter = 0;
        public byte frame = 0;
        public bool initialized = false;
        public void Toggle(Player player)
        {
            if (OpenedBy > -1)
            {
                Close();
                return;
            }
            Open(player);
        }
        /// <summary>
        /// Player here is always just Main.LocalPlayer
        /// </summary>
        /// <param name="player"></param>
        public void Open(Player player)
        {
            OpenedBy = (short)player.whoAmI;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            Main.playerInventory = true;
        }
        public void Close()
        {
            OpenedBy = -1;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        /// <summary>
        /// Only runs on the server
        /// </summary>
        public override void Update()
        {
            if (OpenedBy > -1)
            {
                Player openedPlayer = Main.player[OpenedBy];
                TileReachCheckSettings settings = TileReachCheckSettings.Simple;

                // burh...
                bool inAnyRange = openedPlayer.IsInTileInteractionRange(Position.X, Position.Y, settings);
                if (Dimensions.X > 1)
                    inAnyRange = inAnyRange || openedPlayer.IsInTileInteractionRange(Position.X + Dimensions.X - 1, Position.Y, settings);
                if (Dimensions.Y > 1)
                    inAnyRange = inAnyRange || openedPlayer.IsInTileInteractionRange(Position.X, Position.Y + Dimensions.Y - 1, settings);
                if (Dimensions > Point8.One)
                    inAnyRange = inAnyRange || openedPlayer.IsInTileInteractionRange(Position.X + Dimensions.X - 1, Position.Y + Dimensions.Y - 1, settings);

                if (!inAnyRange)
                {
                    Close();
                    return;
                }
            }
            EnsureArrayIsInitialized();
        }
        public sealed override void SaveData(TagCompound tag)
        {
            Item firstItem = items[0, 0];
            tag["firstSlotHasItem"] = firstItem.Exists();
            tag["name"] = StorageName;

            List<byte> alternationCounts = [];
            List<Item> nonEmptyItems = [];

            bool previousExists = firstItem.Exists();
            byte count = 0;

            foreach (Item item in items)
            {
                bool exists = item.Exists();

                if (exists != previousExists)
                {
                    alternationCounts.Add(count);
                    count = 0;
                    previousExists = exists;
                }

                count++;
                if (exists)
                    nonEmptyItems.Add(item);
            }

            if (count > 0)
                alternationCounts.Add(count);

            tag["alternationCounts"] = alternationCounts;
            tag["items"] = nonEmptyItems;
        }
        public sealed override void LoadData(TagCompound tag)
        {
            EnsureArrayIsInitialized();

            bool firstSlotHasItem = tag.GetBool("firstSlotHasItem");
            StorageName = tag.GetString("name");

            List<byte> alternationCounts = [.. tag.GetList<byte>("alternationCounts")];
            List<Item> nonEmptyItems = [.. tag.GetList<Item>("items")];

            int itemIndex = 0;
            bool currentHasItem = firstSlotHasItem;
            int slotIndex = 0;

            byte vert = (byte)items.GetLength(1);

            foreach (byte count in alternationCounts)
            {
                for (int i = 0; i < count; i++, slotIndex++)
                {
                    if (currentHasItem)
                        items[slotIndex / vert, slotIndex % vert] = nonEmptyItems[itemIndex++];
                    else
                        items[slotIndex / vert, slotIndex % vert] = new Item();
                }
                currentHasItem = !currentHasItem;
            }
        }
        public sealed override bool IsTileValidForEntity(int x, int y)
        {
            Tile t = Framing.GetTileSafely(x, y);
            return TileObjectData.IsTopLeft(t) && TileLoader.GetTile(t.TileType) is ITDChest;
        }
        public void EnsureArrayIsInitialized()
        {
            if (items is null)
            {
                items = new Item[StorageDimensions.X, StorageDimensions.Y];
                for (int i = 0; i < items.GetLength(0); i++)
                {
                    for (int j = 0; j < items.GetLength(1); j++)
                    {
                        items[i, j] = new Item();
                    }
                }
                if (Main.dedServ)
                {
                    NetSystem.SendPacket(new InitializeITDChestPacket(ID));
                }
            }
        }
        public sealed override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            // chests aren't placed from the top left like most tiles, so we need to account for that here in the placement.
            if (TileLoader.GetTile(Framing.GetTileSafely(i, j).TileType) is ITDChest chest)
                j -= chest.Dimensions.Y - 1;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles
                int width = Dimensions.X;
                int height = Dimensions.Y;
                NetMessage.SendTileSquare(Main.myPlayer, i, j, width, height);

                // Sync the placement of the tile entity with other clients
                // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
                return -1;
            }

            // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
            int placedEntity = Place(i, j);

            if (ByID[placedEntity] is ITDChestTE c)
                c.EnsureArrayIsInitialized();

            return placedEntity;
        }
        public sealed override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }
    }
}
