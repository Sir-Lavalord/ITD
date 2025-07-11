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
using Terraria.UI.Gamepad;
using System.IO;

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

            NetSystem.SendPacket(new ITDChestOpenedStatePacket((ushort)ID, (byte)player.whoAmI));
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

            NetSystem.SendPacket(new ITDChestOpenedStatePacket((ushort)ID, 255));
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
        }
        public sealed override void SaveData(TagCompound tag)
        {
            tag["dx"] = (byte)StorageDimensions.X;
            tag["dy"] = (byte)StorageDimensions.Y;

            if (!string.IsNullOrEmpty(StorageName))
                tag["name"] = StorageName;

            if (items.Any(i => i.Exists()))
                tag["items"] = items;
        }
        public sealed override void LoadData(TagCompound tag)
        {
            StorageDimensions = new(tag.GetByte("dx"), tag.GetByte("dy"));

            if (tag.ContainsKey("name"))
                StorageName = tag.GetString("name");

            if (tag.ContainsKey("items"))
                items = [..tag.GetList<Item>("items")];
            else
                EnsureArrayIsInitialized();

            int max = StorageDimensions.X * StorageDimensions.Y;

            // if the read items array is for some reason larger than the storage dimensions, drop all the overflow items onto the ground nearby
            if (items.Length > max)
            {
                for (int i = max; i < items.Length; i++)
                {
                    Item item = items[i];

                    Item.NewItem(item.GetSource_DropAsItem(), Position.ToWorldCoordinates(), item, noBroadcast: false);

                    item.TurnToAir();
                }

                Array.Resize(ref items, max);
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
                    //NetSystem.SendPacket(new InitializeITDChestPacket(ID, StorageDimensions));
                }
            }
        }
        public sealed override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            Point8 dims = new();
            Point8 storageDims = new();

            // chests aren't placed from the top left like most tiles, so we need to account for that here in the placement.
            if (TileLoader.GetTile(Framing.GetTileSafely(i, j).TileType) is ITDChest chest)
            {
                dims = chest.Dimensions;
                j -= dims.Y - 1;
                storageDims = chest.StorageDimensions;
            }

            if (Main.dedServ)
            {

            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Sync the entire multitile's area.  Modify "width" and "height" to the size of your multitile in tiles

                NetMessage.SendTileSquare(Main.myPlayer, i, j, Math.Max((byte)dims.X, (byte)1), Math.Max((byte)dims.Y, (byte)1));

                // Sync the placement of the tile entity with other clients
                // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
                return -1;
            }

            // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you

            // runs on server

            int placedEntity = Place(i, j);

            TileEntity placed = ByID[placedEntity];

            if (placed is ITDChestTE c)
            {
                c.Dimensions = dims;
                c.StorageDimensions = storageDims;
                c.EnsureArrayIsInitialized();
            }

            return placedEntity;
        }
        public sealed override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                if (TileLoader.GetTile(Framing.GetTileSafely(Position).TileType) is ITDChest chest)
                {
                    StorageDimensions = chest.StorageDimensions;
                }
                EnsureArrayIsInitialized();
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
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(StorageDimensions.X);
            writer.Write(StorageDimensions.Y);
            // urghh
            writer.Write((byte)items.Count(i => i.Exists()));
            for (int i = 0; i < items.Length; i++)
            {
                Item item = items[i];
                if (item.Exists())
                {
                    writer.Write((byte)i);
                    ItemIO.Send(item, writer, true);
                }
            }

        }
        public override void NetReceive(BinaryReader reader)
        {
            StorageDimensions = new(reader.ReadByte(), reader.ReadByte());
            EnsureArrayIsInitialized();
            byte length = reader.ReadByte();
            for (int i = 0; i < length; i++)
            {
                byte slot = reader.ReadByte();
                items[slot] = ItemIO.Receive(reader, true);
            }
            Main.NewText(StorageDimensions);
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
        #region DEBUGGING
        public static void Handle(Item[] inv, int context = 0, int slot = 0)
        {
            ItemSlot.OverrideHover(inv, context, slot);
            ItemSlot.LeftClick(inv, context, slot);
            ItemSlot.RightClick(inv, context, slot);
            if (Main.mouseLeftRelease && Main.mouseLeft)
                Recipe.FindRecipes();

            ItemSlot.MouseHover(inv, context, slot);
        }
        public static void Draw(SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor = default)
        {
            Player player = Main.player[Main.myPlayer];
            Item item = inv[slot];
            float inventoryScale = Main.inventoryScale;
            Color color = Color.White;
            if (lightColor != Color.Transparent)
                color = lightColor;

            bool flag = false;
            int num = 0;
            int gamepadPointForSlot = UIAccessors.CallGetGamepadPointForSlot(null, inv, context, slot);
            if (PlayerInput.UsingGamepadUI)
            {
                flag = UILinkPointNavigator.CurrentPoint == gamepadPointForSlot;
                if (PlayerInput.SettingsForUI.PreventHighlightsForGamepad)
                    flag = false;

                if (context == 0)
                {
                    num = player.DpadRadial.GetDrawMode(slot);
                    if (num > 0 && !PlayerInput.CurrentProfile.UsingDpadHotbar())
                        num = 0;
                }
            }

            Texture2D value = TextureAssets.InventoryBack.Value;
            Color color2 = Main.inventoryBack;
            bool flag2 = false;
            bool highlightThingsForMouse = PlayerInput.SettingsForUI.HighlightThingsForMouse;
            if (item.type > ItemID.None && item.stack > 0 && item.favorited && context != 13 && context != 21 && context != 22 && context != 14)
            {
                value = TextureAssets.InventoryBack10.Value;
                if (context == 32)
                    value = TextureAssets.InventoryBack19.Value;
            }
            else if (item.type > ItemID.None && item.stack > 0 && ItemSlot.Options.HighlightNewItems && item.newAndShiny && context != 13 && context != 21 && context != 14 && context != 22)
            {
                value = TextureAssets.InventoryBack15.Value;
                float num2 = (float)(int)Main.mouseTextColor / 255f;
                num2 = num2 * 0.2f + 0.8f;
                color2 = color2.MultiplyRGBA(new Color(num2, num2, num2));
            }
            else if (!highlightThingsForMouse && item.type > ItemID.None && item.stack > 0 && num != 0 && context != 13 && context != 21 && context != 22)
            {
                value = TextureAssets.InventoryBack15.Value;
                float num3 = (float)(int)Main.mouseTextColor / 255f;
                num3 = num3 * 0.2f + 0.8f;
                color2 = ((num != 1) ? color2.MultiplyRGBA(new Color(num3 / 2f, num3, num3 / 2f)) : color2.MultiplyRGBA(new Color(num3, num3 / 2f, num3 / 2f)));
            }
            else if (context == 0 && slot < 10)
            {
                value = TextureAssets.InventoryBack9.Value;
            }
            else
            {
                switch (context)
                {
                    case 28:
                        value = TextureAssets.InventoryBack7.Value;
                        color2 = Color.White;
                        break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                        value = TextureAssets.InventoryBack3.Value;
                        break;
                    case 8:
                    case 10:
                        value = TextureAssets.InventoryBack13.Value;
                        color2 = ItemSlot.GetColorByLoadout(slot, context);
                        break;
                    case 23:
                    case 24:
                    case 26:
                        value = TextureAssets.InventoryBack8.Value;
                        break;
                    case 9:
                    case 11:
                        value = TextureAssets.InventoryBack13.Value;
                        color2 = ItemSlot.GetColorByLoadout(slot, context);
                        break;
                    case 25:
                    case 27:
                    case 33:
                        value = TextureAssets.InventoryBack12.Value;
                        break;
                    case 12:
                        value = TextureAssets.InventoryBack13.Value;
                        color2 = ItemSlot.GetColorByLoadout(slot, context);
                        break;
                    case ItemSlot.Context.ModdedAccessorySlot:
                    case ItemSlot.Context.ModdedVanityAccessorySlot:
                    case ItemSlot.Context.ModdedDyeSlot:
                        value = UIAccessors.CallGetBackgroundTexture(LoaderManager.Get<AccessorySlotLoader>(), slot, context);
                        break;
                    case 3:
                        value = TextureAssets.InventoryBack5.Value;
                        break;
                    case 4:
                    case 32:
                        value = TextureAssets.InventoryBack2.Value;
                        break;
                    case 5:
                    case 7:
                        value = TextureAssets.InventoryBack4.Value;
                        break;
                    case 6:
                        value = TextureAssets.InventoryBack7.Value;
                        break;
                    case 13:
                        {
                            byte b = 200;
                            if (slot == Main.player[Main.myPlayer].selectedItem)
                            {
                                value = TextureAssets.InventoryBack14.Value;
                                b = byte.MaxValue;
                            }

                            color2 = new Color(b, b, b, b);
                            break;
                        }
                    case 14:
                    case 21:
                        flag2 = true;
                        break;
                    case 15:
                        value = TextureAssets.InventoryBack6.Value;
                        break;
                    case 29:
                        color2 = new Color(53, 69, 127, 255);
                        value = TextureAssets.InventoryBack18.Value;
                        break;
                    case 30:
                        flag2 = !flag;
                        break;
                    case 22:
                        value = TextureAssets.InventoryBack4.Value;
                        if (ItemSlot.DrawGoldBGForCraftingMaterial)
                        {
                            ItemSlot.DrawGoldBGForCraftingMaterial = false;
                            value = TextureAssets.InventoryBack14.Value;
                            float num4 = (float)(int)color2.A / 255f;
                            num4 = ((!(num4 < 0.7f)) ? 1f : Utils.GetLerpValue(0f, 0.7f, num4, clamped: true));
                            color2 = Color.White * num4;
                        }
                        break;
                }
            }

            if ((context == 0 || context == 2) && UIAccessors.GetInventoryGlowTime(null)[slot] > 0 && !inv[slot].favorited && !inv[slot].IsAir)
            {
                float num5 = Main.invAlpha / 255f;
                Color value2 = new Color(63, 65, 151, 255) * num5;
                Color value3 = Main.hslToRgb(UIAccessors.GetInventoryGlowHue(null)[slot], 1f, 0.5f) * num5;
                float num6 = (float)UIAccessors.GetInventoryGlowTime(null)[slot] / 300f;
                num6 *= num6;
                color2 = Color.Lerp(value2, value3, num6 / 2f);
                value = TextureAssets.InventoryBack13.Value;
            }

            if ((context == 4 || context == 32 || context == 3) && UIAccessors.GetInventoryGlowTimeChest(null)[slot] > 0 && !inv[slot].favorited && !inv[slot].IsAir)
            {
                float num7 = Main.invAlpha / 255f;
                Color value4 = new Color(130, 62, 102, 255) * num7;
                if (context == 3)
                    value4 = new Color(104, 52, 52, 255) * num7;

                Color value5 = Main.hslToRgb(UIAccessors.GetInventoryGlowHueChest(null)[slot], 1f, 0.5f) * num7;
                float num8 = (float)UIAccessors.GetInventoryGlowTimeChest(null)[slot] / 300f;
                num8 *= num8;
                color2 = Color.Lerp(value4, value5, num8 / 2f);
                value = TextureAssets.InventoryBack13.Value;
            }

            if (flag)
            {
                value = TextureAssets.InventoryBack14.Value;
                color2 = Color.White;
                if (item.favorited)
                    value = TextureAssets.InventoryBack17.Value;
            }

            if (context == 28 && Main.MouseScreen.Between(position, position + value.Size() * inventoryScale) && !player.mouseInterface)
            {
                value = TextureAssets.InventoryBack14.Value;
                color2 = Color.White;
            }

            if (!flag2)
                spriteBatch.Draw(value, position, null, color2, 0f, default, inventoryScale, SpriteEffects.None, 0f);

            int num9 = -1;
            switch (context)
            {
                case 8:
                case 23:
                    if (slot == 0)
                        num9 = 0;
                    if (slot == 1)
                        num9 = 6;
                    if (slot == 2)
                        num9 = 12;
                    break;
                case 26:
                    num9 = 0;
                    break;
                case 9:
                    if (slot == 10)
                        num9 = 3;
                    if (slot == 11)
                        num9 = 9;
                    if (slot == 12)
                        num9 = 15;
                    break;
                case 10:
                case 24:
                    num9 = 11;
                    break;
                case 11:
                    num9 = 2;
                    break;
                case 12:
                case 25:
                case 27:
                case 33:
                    num9 = 1;
                    break;
                case ItemSlot.Context.ModdedAccessorySlot:
                    // 'num9' is the vertical frame of some texture?
                    num9 = 11;
                    break;
                case ItemSlot.Context.ModdedVanityAccessorySlot:
                    num9 = 2;
                    break;
                case ItemSlot.Context.ModdedDyeSlot:
                    num9 = 1;
                    break;
                case 16:
                    num9 = 4;
                    break;
                case 17:
                    num9 = 13;
                    break;
                case 19:
                    num9 = 10;
                    break;
                case 18:
                    num9 = 7;
                    break;
                case 20:
                    num9 = 17;
                    break;
            }

            if ((item.type <= ItemID.None || item.stack <= 0) && num9 != -1)
            {
                Texture2D value6 = TextureAssets.Extra[54].Value;
                Rectangle rectangle = value6.Frame(3, 6, num9 % 3, num9 / 3);
                rectangle.Width -= 2;
                rectangle.Height -= 2;

                if (context is ItemSlot.Context.ModdedAccessorySlot or ItemSlot.Context.ModdedVanityAccessorySlot or ItemSlot.Context.ModdedDyeSlot)
                {
                    UIAccessors.CallDrawSlotTexture( LoaderManager.Get<AccessorySlotLoader>(), value6, position + value.Size() / 2f * inventoryScale, rectangle, Color.White * 0.35f, 0f, rectangle.Size() / 2f, inventoryScale, SpriteEffects.None, 0f, slot, context);
                    goto SkipVanillaDraw;
                }

                spriteBatch.Draw(value6, position + value.Size() / 2f * inventoryScale, rectangle, Color.White * 0.35f, 0f, rectangle.Size() / 2f, inventoryScale, SpriteEffects.None, 0f);
            SkipVanillaDraw:;
            }

            Vector2 vector = value.Size() * inventoryScale;
            if (item.type > 0 && item.stack > 0)
            {
                float scale = ItemSlot.DrawItemIcon(item, context, spriteBatch, position + vector / 2f, inventoryScale, 32f, color);
                if (ItemID.Sets.TrapSigned[item.type])
                    spriteBatch.Draw(TextureAssets.Wire.Value, position + new Vector2(40f, 40f) * inventoryScale, new Rectangle(4, 58, 8, 8), color, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);

                if (ItemID.Sets.DrawUnsafeIndicator[item.type])
                {
                    Vector2 vector2 = new Vector2(-4f, -4f) * inventoryScale;
                    Texture2D value7 = TextureAssets.Extra[258].Value;
                    Rectangle rectangle2 = value7.Frame();
                    spriteBatch.Draw(value7, position + vector2 + new Vector2(40f, 40f) * inventoryScale, rectangle2, color, 0f, rectangle2.Size() / 2f, 1f, SpriteEffects.None, 0f);
                }

                if (item.type == 5324 || item.type == 5329 || item.type == 5330)
                {
                    Vector2 vector3 = new Vector2(2f, -6f) * inventoryScale;
                    switch (item.type)
                    {
                        case 5324:
                            {
                                Texture2D value10 = TextureAssets.Extra[257].Value;
                                Rectangle rectangle5 = value10.Frame(3, 1, 2);
                                spriteBatch.Draw(value10, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle5, color, 0f, rectangle5.Size() / 2f, 1f, SpriteEffects.None, 0f);
                                break;
                            }
                        case 5329:
                            {
                                Texture2D value9 = TextureAssets.Extra[257].Value;
                                Rectangle rectangle4 = value9.Frame(3, 1, 1);
                                spriteBatch.Draw(value9, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle4, color, 0f, rectangle4.Size() / 2f, 1f, SpriteEffects.None, 0f);
                                break;
                            }
                        case 5330:
                            {
                                Texture2D value8 = TextureAssets.Extra[257].Value;
                                Rectangle rectangle3 = value8.Frame(3);
                                spriteBatch.Draw(value8, position + vector3 + new Vector2(40f, 40f) * inventoryScale, rectangle3, color, 0f, rectangle3.Size() / 2f, 1f, SpriteEffects.None, 0f);
                                break;
                            }
                    }
                }

                if (item.stack > 1)
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), position + new Vector2(10f, 26f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);

                int num10 = -1;
                if (context == 13)
                {
                    if (item.DD2Summon)
                    {
                        for (int i = 0; i < 58; i++)
                        {
                            if (inv[i].type == ItemID.DD2EnergyCrystal)
                                num10 += inv[i].stack;
                        }

                        if (num10 >= 0)
                            num10++;
                    }

                    if (item.useAmmo > 0)
                    {
                        num10 = 0;
                        for (int j = 0; j < 58; j++)
                        {
                            if (inv[j].stack > 0 && ItemLoader.CanChooseAmmo(item, inv[j], player))
                                num10 += inv[j].stack;
                        }
                    }

                    if (item.fishingPole > 0)
                    {
                        num10 = 0;
                        for (int k = 0; k < 58; k++)
                        {
                            if (inv[k].bait > 0)
                                num10 += inv[k].stack;
                        }
                    }

                    if (item.tileWand > 0)
                    {
                        int tileWand = item.tileWand;
                        num10 = 0;
                        for (int l = 0; l < 58; l++)
                        {
                            if (inv[l].type == tileWand)
                                num10 += inv[l].stack;
                        }
                    }

                    if (item.type == ItemID.Wrench || item.type == ItemID.GreenWrench || item.type == ItemID.BlueWrench || item.type == ItemID.YellowWrench || item.type == ItemID.MulticolorWrench || item.type == ItemID.WireKite)
                    {
                        num10 = 0;
                        for (int m = 0; m < 58; m++)
                        {
                            if (inv[m].type == ItemID.Wire)
                                num10 += inv[m].stack;
                        }
                    }
                }

                if (num10 != -1)
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, num10.ToString(), position + new Vector2(8f, 30f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale * 0.8f), -1f, inventoryScale);

                if (context == 13)
                {
                    string text = string.Concat(slot + 1);
                    if (text == "10")
                        text = "0";

                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, position + new Vector2(8f, 4f) * inventoryScale, color, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);
                }

                if (context == 13 && item.potion)
                {
                    Vector2 position2 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                    Color color3 = item.GetAlpha(color) * ((float)player.potionDelay / (float)player.potionDelayTime);
                    spriteBatch.Draw(TextureAssets.Cd.Value, position2, null, color3, 0f, default, scale, SpriteEffects.None, 0f);

                    // Extra context.
                }

                // TML: Added handling of the new 'masterOnly' item field.
                if ((Math.Abs(context) == 10 || context == 18) && ((item.expertOnly && !Main.expertMode) || (item.masterOnly && !Main.masterMode)))
                {
                    Vector2 position3 = position + value.Size() * inventoryScale / 2f - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                    Color white = Color.White;
                    spriteBatch.Draw(TextureAssets.Cd.Value, position3, null, white, 0f, default, scale, SpriteEffects.None, 0f);
                }

                // Extra context.
            }
            else if (context == 6)
            {
                Texture2D value11 = TextureAssets.Trash.Value;
                Vector2 position4 = position + value.Size() * inventoryScale / 2f - value11.Size() * inventoryScale / 2f;
                spriteBatch.Draw(value11, position4, null, new Color(100, 100, 100, 100), 0f, default, inventoryScale, SpriteEffects.None, 0f);
            }

            if (context == 0 && slot < 10)
            {
                float num11 = inventoryScale;
                string text2 = string.Concat(slot + 1);
                if (text2 == "10")
                    text2 = "0";

                Color baseColor = Main.inventoryBack;
                int num12 = 0;
                if (Main.player[Main.myPlayer].selectedItem == slot)
                {
                    baseColor = Color.White;
                    baseColor.A = 200;
                    num12 -= 2;
                }

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text2, position + new Vector2(6f, 4 + num12) * inventoryScale, baseColor, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, inventoryScale);
            }

            /*
            if (gamepadPointForSlot != -1)
                UILinkPointNavigator.SetPosition(gamepadPointForSlot, position + vector * 0.75f);
            */
        }
        #endregion
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
            long num = Utils.CoinsCount(out bool overFlowing, pInv);
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
                if (array2[l] < 0 || cInv[l].type != ItemID.None)
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

                if (!Main.dedServ && IsActiveForLocalPlayer)
                    NetSystem.SendPacket(new SyncITDChestItemPacket(GetITDChest().ID, num6));

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

                if (!Main.dedServ && IsActiveForLocalPlayer)
                    NetSystem.SendPacket(new SyncITDChestItemPacket(GetITDChest().ID, list2[0]));

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

            long num12 = Utils.CoinsCount(out bool overFlowing2, pInv);
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
