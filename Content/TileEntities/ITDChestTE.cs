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
using System.Linq;
using System;

namespace ITD.Content.TileEntities
{
    public class ITDChestTE : ModTileEntity
    {
        public static bool IsActiveForLocalPlayer
        { 
            get
            {
                TileEntity possible = Main.LocalPlayer.tileEntityAnchor.GetTileEntity();
                return possible != null && possible is ITDChestTE;
            }
        }
        /// <summary>
        /// Only safe to call if <see cref="IsActiveForLocalPlayer"/>.
        /// </summary>
        /// <returns></returns>
        public static ITDChestTE GetITDChest() => Main.LocalPlayer.tileEntityAnchor.GetTileEntity() as ITDChestTE;
        public const int FullSlotDim = 42;
        public int UIOffsetX => StorageDimensions.X - 10;
        public Point8 Dimensions = new(0);
        /// <summary>
        /// For reference, vanilla uses (10, 4).
        /// </summary>
        public Point8 StorageDimensions = new(10, 4);
        public int TotalSlots => StorageDimensions.X * StorageDimensions.Y;
        internal Item[] items;
        public string StorageName = "";
        public short OpenedBy = -1;
        public byte frameCounter = 0;
        public byte frame = 0;
        public bool initialized = false;
        public bool forcedOpen = false;
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
            if (Dimensions == Point8.Zero)
            {
                if (TileLoader.GetTile(Framing.GetTileSafely(Position).TileType) is ITDChest chest)
                    Dimensions = chest.Dimensions;
            }
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
            if (!string.IsNullOrEmpty(StorageName))
                tag["name"] = StorageName;

            if (items.Any(i => i.Exists()))
                tag["items"] = items;

            tag["dx"] = (byte)StorageDimensions.X;
            tag["dy"] = (byte)StorageDimensions.Y;
        }
        public sealed override void LoadData(TagCompound tag)
        {
            EnsureArrayIsInitialized();

            if (tag.ContainsKey("name"))
                StorageName = tag.GetString("name");

            if (tag.ContainsKey("items"))
                items = [..tag.GetList<Item>("items")];

            StorageDimensions = new(tag.GetByte("dx"), tag.GetByte("dy"));

            if (items.Length > StorageDimensions.X * StorageDimensions.Y)
            {
                for (int i = StorageDimensions.X * StorageDimensions.Y; i < items.Length; i++)
                {
                    Item item = items[i];

                    Item.NewItem(item.GetSource_DropAsItem(), Position.ToWorldCoordinates(), item, noBroadcast: false);

                    item.TurnToAir();
                }

                Array.Resize(ref items, StorageDimensions.X * StorageDimensions.Y);
            }
        }
        public ref Item this[int i] => ref items[i];
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
            {
                Dimensions = chest.Dimensions;
                j -= Dimensions.Y - 1;
                StorageDimensions = chest.StorageDimensions;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles

                if (TileLoader.GetTile(Framing.GetTileSafely(i, j).TileType) is ITDChest ch)
                {
                    Dimensions = ch.Dimensions;
                }

                NetMessage.SendTileSquare(Main.myPlayer, i, j, Math.Max((byte)Dimensions.X, (byte)1), Math.Max((byte)Dimensions.Y, (byte)1));

                // Sync the placement of the tile entity with other clients
                // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
                return -1;
            }

            // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
            int placedEntity = Place(i, j);

            TileEntity placed = ByID[placedEntity];

            if (placed is ITDChestTE c)
            {
                c.EnsureArrayIsInitialized();
                if (TileLoader.GetTile(Framing.GetTileSafely(placed.Position).TileType) is ITDChest ches)
                    c.StorageDimensions = ches.StorageDimensions;
            }

            return placedEntity;
        }
        public sealed override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }
        public override bool OverrideItemSlotHover(Item[] inv, int context = 0, int slot = 0)
        {
            if (context >= ItemSlot.Context.InventoryItem && context <= ItemSlot.Context.InventoryAmmo && inv[slot].Exists() && ChestUI.TryPlacingInChest(inv[slot], true, context))
            {
                Main.cursorOverride = 9;
                return true;
            }
            return false;
        }
        public void OpenToReceiveParticles()
        {
            forcedOpen = true;
            frame = 2;
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
        public static long MoveCoinsITD(Item[] pInv, Item[] cInv, ContainerTransferContext context)
        {
            bool flag = false;
            int[] array = new int[4];
            List<int> list = [];
            List<int> list2 = [];
            bool flag2 = false;
            int[] array2 = new int[cInv.Length];
            bool overFlowing;
            long num = Utils.CoinsCount(out overFlowing, pInv);
            for (int i = 0; i < cInv.Length; i++)
            {
                array2[i] = -1;
                if (cInv[i].stack < 1 || cInv[i].type < ItemID.IronPickaxe)
                {
                    list2.Add(i);
                    cInv[i] = new Item();
                }

                if (cInv[i] != null && cInv[i].stack > 0)
                {
                    int num2 = 0;
                    if (cInv[i].type == ItemID.CopperCoin)
                        num2 = 1;

                    if (cInv[i].type == ItemID.SilverCoin)
                        num2 = 2;

                    if (cInv[i].type == ItemID.GoldCoin)
                        num2 = 3;

                    if (cInv[i].type == ItemID.PlatinumCoin)
                        num2 = 4;

                    array2[i] = num2 - 1;
                    if (num2 > 0)
                    {
                        array[num2 - 1] += cInv[i].stack;
                        list2.Add(i);
                        cInv[i] = new Item();
                        flag2 = true;
                    }
                }
            }

            if (!flag2)
                return 0L;

            for (int j = 0; j < pInv.Length; j++)
            {
                if (j != 58 && pInv[j] != null && pInv[j].stack > 0 && !pInv[j].favorited)
                {
                    int num3 = 0;
                    if (pInv[j].type == ItemID.CopperCoin)
                        num3 = 1;

                    if (pInv[j].type == ItemID.SilverCoin)
                        num3 = 2;

                    if (pInv[j].type == ItemID.GoldCoin)
                        num3 = 3;

                    if (pInv[j].type == ItemID.PlatinumCoin)
                        num3 = 4;

                    if (num3 > 0)
                    {
                        flag = true;
                        array[num3 - 1] += pInv[j].stack;
                        list.Add(j);
                        pInv[j] = new Item();
                    }
                }
            }

            for (int k = 0; k < 3; k++)
            {
                while (array[k] >= 100)
                {
                    array[k] -= 100;
                    array[k + 1]++;
                }
            }

            for (int l = 0; l < 40; l++)
            {
                if (array2[l] < 0 || cInv[l].type != 0)
                    continue;

                int num4 = l;
                int num5 = array2[l];
                if (array[num5] > 0)
                {
                    cInv[num4].SetDefaults(71 + num5);
                    cInv[num4].stack = array[num5];
                    if (cInv[num4].stack > cInv[num4].maxStack)
                        cInv[num4].stack = cInv[num4].maxStack;

                    array[num5] -= cInv[num4].stack;
                    array2[l] = -1;
                }

                if (!Main.dedServ && IsActiveForLocalPlayer)
                    NetSystem.SendPacket(new SyncITDChestItemPacket(GetITDChest().ID, num4));

                list2.Remove(num4);
            }

            for (int m = 0; m < 40; m++)
            {
                if (array2[m] < 0 || cInv[m].type != 0)
                    continue;

                int num6 = m;
                int num7 = 3;
                while (num7 >= 0)
                {
                    if (array[num7] > 0)
                    {
                        cInv[num6].SetDefaults(71 + num7);
                        cInv[num6].stack = array[num7];
                        if (cInv[num6].stack > cInv[num6].maxStack)
                            cInv[num6].stack = cInv[num6].maxStack;

                        array[num7] -= cInv[num6].stack;
                        array2[m] = -1;
                        break;
                    }

                    if (array[num7] == 0)
                        num7--;
                }

                if (Main.netMode == 1 && Main.player[Main.myPlayer].chest > -1)
                    NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, num6);

                list2.Remove(num6);
            }

            while (list2.Count > 0)
            {
                int num8 = list2[0];
                int num9 = 3;
                while (num9 >= 0)
                {
                    if (array[num9] > 0)
                    {
                        cInv[num8].SetDefaults(71 + num9);
                        cInv[num8].stack = array[num9];
                        if (cInv[num8].stack > cInv[num8].maxStack)
                            cInv[num8].stack = cInv[num8].maxStack;

                        array[num9] -= cInv[num8].stack;
                        break;
                    }

                    if (array[num9] == 0)
                        num9--;
                }

                if (Main.netMode == 1 && Main.player[Main.myPlayer].chest > -1)
                    NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, list2[0]);

                list2.RemoveAt(0);
            }

            int num10 = 3;
            while (num10 >= 0 && list.Count > 0)
            {
                int num11 = list[0];
                if (array[num10] > 0)
                {
                    pInv[num11].SetDefaults(71 + num10);
                    pInv[num11].stack = array[num10];
                    if (pInv[num11].stack > pInv[num11].maxStack)
                        pInv[num11].stack = pInv[num11].maxStack;

                    array[num10] -= pInv[num11].stack;
                    flag = false;
                    list.RemoveAt(0);
                }

                if (array[num10] == 0)
                    num10--;
            }

            if (flag)
                SoundEngine.PlaySound(SoundID.Grab);

            bool overFlowing2;
            long num12 = Utils.CoinsCount(out overFlowing2, pInv);
            if (overFlowing || overFlowing2)
                return 0L;

            return num - num12;
        }
    }
    public static class UIAccessors
    {
        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Sort")]
        public static extern void CallSort(ItemSorting type, Item[] inv, params int[] ignoreSlots);
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
