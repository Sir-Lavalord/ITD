using ITD.Content.Tiles;
using ITD.Networking;
using ITD.Networking.Packets;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.GameInput;
using System.Runtime.CompilerServices;

namespace ITD.Content.TileEntities
{
    public abstract class ITDChestTE : ModTileEntity
    {
        public const int FullSlotDim = 42;
        public int UIOffsetX => StorageDimensions.X - 10;
        public virtual Point8 Dimensions => new(2, 2);
        /// <summary>
        /// For reference, vanilla uses (10, 4).
        /// </summary>
        public virtual Point8 StorageDimensions => new(10, 4);
        /// <summary>
        /// 2d array (is this the best way of doing this?)
        /// </summary>
        internal Item[] items;
        public string StorageName = "";
        public short OpenedBy = -1;
        public byte frameCounter = 0;
        public byte frame = 0;
        public bool initialized = false;
        public void Toggle(Player player)
        {
            if (OpenedBy > -1)
            {
                Close(player);
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
            RecalcTrashOffset();
            bool silent = false;
            // close vanilla chest if open
            if (player.chest != -1)
            {
                silent = true;
                player.OpenChest(0, 0, -1);
            }
            // close other itdchest if open
            TileEntity possible = player.tileEntityAnchor.GetTileEntity();
            if (possible != null && possible is ITDChestTE te)
            {
                silent = true;
                te.Close(player, true);
            }
            OpenedBy = (short)player.whoAmI;
            SoundEngine.PlaySound(silent ? SoundID.MenuTick : SoundID.MenuOpen);
            player.tileEntityAnchor.Set(ID, Position.X, Position.Y);
            Main.playerInventory = true;
        }
        public void Close(Player player, bool silent = false)
        {
            OpenedBy = -1;
            if (!silent)
            {
                Main.trashSlotOffset = Point16.Zero;
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
            player.tileEntityAnchor.Clear();
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
                    Close(openedPlayer);
                    return;
                }
            }
            EnsureArrayIsInitialized();
        }
        public sealed override void SaveData(TagCompound tag)
        {
            Item firstItem = items[0];
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

            foreach (byte count in alternationCounts)
            {
                for (int i = 0; i < count; i++, slotIndex++)
                {
                    if (currentHasItem)
                        items[slotIndex] = nonEmptyItems[itemIndex++];
                    else
                        items[slotIndex] = new Item();
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
                items = new Item[StorageDimensions.X * StorageDimensions.Y];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new Item();
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
        public void RecalcTrashOffset()
        {
            Main.trashSlotOffset = new Point16(5 + (UIOffsetX * FullSlotDim), (StorageDimensions.Y * FullSlotDim));
        }
        public override void OnInventoryDraw(Player player, SpriteBatch spriteBatch)
        {
            // replicates chest ui. specifically ChestUI.Draw();
            //spriteBatch.Draw(TextureAssets.InventorySort[0].Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            if (OpenedBy > -1 && !Main.recBigList)
            {
                RecalcTrashOffset();
                Main.inventoryScale = 0.755f;
                if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, 73f, Main.instance.invBottom, StorageDimensions.X * FullSlotDim, StorageDimensions.Y * FullSlotDim))
                {
                    player.mouseInterface = true;
                }
                DrawName(spriteBatch);
                DrawButtons(spriteBatch);
                DrawSlots(spriteBatch);
            }
            else
            {
                for (int i = 0; i < ChestUI.ButtonID.Count; i++)
                {
                    ChestUI.ButtonScale[i] = 0.75f;
                    ChestUI.ButtonHovered[i] = false;
                }
            }
        }
        private void DrawSlots(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            int context = 3;
            Item[] inv = null;
            var anchor = player.tileEntityAnchor;
            TileEntity te = anchor.GetTileEntity();
            bool validTe = te != null && te is ITDChestTE;
            if (validTe)
            {
                inv = items;
            }
            Main.inventoryScale = 0.755f;
            if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, 73f, Main.instance.invBottom, 560f * Main.inventoryScale, 224f * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface)
            {
                player.mouseInterface = true;
            }
            for (int i = 0; i < StorageDimensions.X; i++)
            {
                for (int j = 0; j < StorageDimensions.Y; j++)
                {
                    int num = (int)(73f + (float)(i * 56) * Main.inventoryScale);
                    int num2 = (int)((float)Main.instance.invBottom + (float)(j * 56) * Main.inventoryScale);
                    int slot = i + j * StorageDimensions.X;
                    if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, num, num2, (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface)
                    {
                        player.mouseInterface = true;
                        ItemSlot.Handle(inv, context, slot);
                    }
                    ItemSlot.Draw(spriteBatch, inv, context, slot, new Vector2(num, num2));
                }
            }
        }
        private void DrawName(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            var anchor = player.tileEntityAnchor;
            TileEntity te = anchor.GetTileEntity();
            bool validTe = te != null && te is ITDChestTE;
            string text = string.Empty;
            if (Main.editChest)
            {
                text = Main.npcChatText;
                Main.instance.textBlinkerCount++;
                if (Main.instance.textBlinkerCount >= 20)
                {
                    if (Main.instance.textBlinkerState == 0)
                    {
                        Main.instance.textBlinkerState = 1;
                    }
                    else
                    {
                        Main.instance.textBlinkerState = 0;
                    }
                    Main.instance.textBlinkerCount = 0;
                }
                if (Main.instance.textBlinkerState == 1)
                {
                    text += "|";
                }
                Main.instance.DrawWindowsIMEPanel(new Vector2(120f, 518f));
            }
            else if (validTe)
            {
                ITDChestTE chest = te as ITDChestTE;
                if (chest.StorageName != "")
                {
                    text = chest.StorageName;
                }
                else
                {
                    Tile chestTile = Framing.GetTileSafely(te.Position);
                    if (TileLoader.GetTile(chestTile.TileType) is ITDChest)
                    {
                        text = TileLoader.DefaultContainerName(chestTile.TileType, chestTile.TileFrameX, chestTile.TileFrameY);
                    }
                }
            }
            Color color = Color.White * (1f - (255f - (float)(int)Main.mouseTextColor) / 255f * 0.5f);
            color.A = byte.MaxValue;
            Utils.WordwrapString(text, FontAssets.MouseText.Value, 200, 1, out var lineAmount);
            lineAmount++;
            for (int i = 0; i < lineAmount; i++)
            {
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(504f + (UIOffsetX * FullSlotDim), Main.instance.invBottom + i * 26), color, 0f, Vector2.Zero, Vector2.One, -1f, 1.5f);
            }
        }
        private void DrawButtons(SpriteBatch spriteBatch)
        {
            int x = 506 + (UIOffsetX * FullSlotDim);
            for (int i = 0; i < ChestUI.ButtonID.Count; i++)
            {
                UIAccessors.CallDrawChestButton(null, spriteBatch, i, x, Main.instance.invBottom + 40);
            }
        }
    }
    public static class UIAccessors
    {
        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "DrawButton")]
        public static extern void CallDrawChestButton(ChestUI type, SpriteBatch spriteBatch, int ID, int X, int Y);

        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowHue")]
        public static extern ref float[] GetInventoryGlowHue(ItemSlot type);

        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowTime")]
        public static extern ref int[] GetInventoryGlowTime(ItemSlot type);

        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowHueChest")]
        public static extern ref float[] GetInventoryGlowHueChest(ItemSlot type);

        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "inventoryGlowTimeChest")]
        public static extern ref int[] GetInventoryGlowTimeChest(ItemSlot type);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawSlotTexture")]
        public static extern void CallDrawSlotTexture(AccessorySlotLoader instance, Texture2D value6, Vector2 position, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, int slot, int context);
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetBackgroundTexture")]
        public static extern Texture2D CallGetBackgroundTexture(AccessorySlotLoader instance, int slot, int context);
        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "GetGamepadPointForSlot")]
        public static extern int CallGetGamepadPointForSlot(ItemSlot type, Item[] inv, int context, int slot);
    }
}
