using System;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Audio;
using ITD.Systems.DataStructures;
using ITD.Utilities;
using ITD.Content.TileEntities;
using System.Linq;
using Terraria.UI.Chat;
using Terraria.Chat;

namespace ITD.Content.Tiles
{
    public abstract class ITDChest : ModTile
    {
        /// <summary>
        /// The ID of the item that this tile will drop. This is necessary because of chest locked styles.
        /// </summary>
        public abstract int ItemType { get; }
        /// <summary>
        /// The default behavior of this property is what you want in most cases.
        /// </summary>
        public virtual ITDChestTE TE => ModContent.GetInstance<ITDChestTE>();
        public virtual int KeyType => ItemID.DirtBlock;
        public virtual (Color, Color) UnlockedAndLockedMapColors => (new(191, 142, 111), new(191, 142, 111));
        public virtual Point8 Dimensions => new(2, 2);
        public virtual Point8 StorageDimensions => new(10, 4);
        public virtual bool LavaDeath => true;
        public sealed override void SetStaticDefaults()
        {
            Main.tileSpelunker[Type] = true;
            // i found this really funny, but check Player.PickTile. if the tile is in this set then it calls WorldGen.KillTile with fail set to true for some reason.
            // i'm guessing this is a bug.
            //Main.tileContainer[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1200;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileOreFinderPriority[Type] = 500;
            TileID.Sets.HasOutlines[Type] = true;
            // no basicchest
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.AvoidedByNPCs[Type] = true;
            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileID.Sets.IsAContainer[Type] = true;
            TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

            AdjTiles = [TileID.Containers];
            (Color color0, Color color1) = UnlockedAndLockedMapColors;
            AddMapEntry(color0, this.GetLocalization("MapEntryUnlocked"), MapChestName);
            AddMapEntry(color1, this.GetLocalization("MapEntryLocked"), MapChestName);
            // chest item if a locked chest is somehow destroyed
            RegisterItemDrop(ItemType, 1);
            // fallback item if this chest type is removed from the mod
            RegisterItemDrop(ItemID.Chest);

            //TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

            Point8 dims = Dimensions;
            TileObjectData.newTile.Width = dims.X;
            TileObjectData.newTile.Height = dims.Y;
            TileObjectData.newTile.Origin = new Point16(0, dims.Y - 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;

            int[] coordinateHeights = new int[dims.Y];
            Array.Fill(coordinateHeights, 16);
            // unlike vanilla chests, we will allow an extra pixel for proper grounding
            coordinateHeights[^1] = 18;

            TileObjectData.newTile.CoordinateHeights = coordinateHeights;

            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.LavaDeath = LavaDeath;

            // no hookcheckifcanplace
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(TE.Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.AnchorInvalidTiles = [
                TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
            ];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaPlacement = LavaDeath ? LiquidPlacement.NotAllowed : LiquidPlacement.Allowed;

            AnimationFrameHeight = TileObjectData.newTile.CoordinateFullHeight;

            TileObjectData.addTile(Type);

            SetStaticDefaultsSafe();
        }
        public virtual void SetStaticDefaultsSafe()
        {

        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            if (type != Type)
                return;
            ITDChestTE te = GetTE(i, j);
            if (te is null)
                return;
            int speed = 1;
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out var t) && t.ID == te.ID)
            {
                if (!te.forcedOpen)
                {
                    if (te.OpenedBy > -1 && te.frame < 2)
                    {
                        if (++te.frameCounter > speed)
                        {
                            te.frameCounter = 0;
                            te.frame++;
                        }
                    }
                    if (te.OpenedBy < 0 && te.frame > 0)
                    {
                        if (++te.frameCounter > speed)
                        {
                            te.frameCounter = 0;
                            te.frame--;
                        }
                    }
                }
                else
                {
                    switch (te.frame)
                    {
                        case 2:
                            if (++te.frameCounter > 10)
                            {
                                te.frameCounter = 0;
                                te.frame--;
                            }
                            break;
                        case 1:
                            if (++te.frameCounter > 4)
                            {
                                te.frameCounter = 0;
                                te.frame--;
                            }
                            break;
                        case 0:
                            te.forcedOpen = false;
                            SoundEngine.PlaySound(SoundID.Grab, te.Position.ToWorldCoordinates(0, 0) + (new Vector2(Dimensions.X * 16f, Dimensions.Y * 16f) * 0.5f));
                            break;
                    }
                }
            }
            frameYOffset = AnimationFrameHeight * te.frame;
        }
        public override ushort GetMapOption(int i, int j)
        {
            return (ushort)(Main.tile[i, j].TileFrameX / (18 * Dimensions.X));
        }

        public override LocalizedText DefaultContainerName(int frameX, int frameY)
        {
            int option = frameX / (18 * Dimensions.X);
            return this.GetLocalization("MapEntry" + (option == 0 ? "Unlocked" : "Locked"));
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override bool IsLockedChest(int i, int j) => Main.tile[i, j].TileFrameX / (18 * Dimensions.X) == 1;
        public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual)
        {
            int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
            // We need to return true only if the tile style is the unlocked variant of a chest that supports locking. 
            if (style == 0)
            {
                // We can check other conditions as well, such as how biome chests can't be locked until Plantera is defeated
                return true;
            }
            return false;
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            ITDChestTE te = GetTE(i, j);

            if (te != null && te.items.Any(it => it.Exists()))
                fail = true;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // We override KillMultiTile to handle additional logic other than the item drop. In this case, unregistering the Chest from the world
            TE.Kill(i, j);
        }
        public ITDChestTE GetTE(int i, int j)
        {
            ITDChestTE te = null;
            if (TileHelpers.TryGetTileEntityAs(TE.GetType(), i, j, out var entity))
            {
                te = entity as ITDChestTE;
            }
            return te;
        }
        public sealed override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;

            // Should your tile entity bring up a UI, this line is useful to prevent item slots from misbehaving
            Main.mouseRightRelease = false;

            ITDChestTE chest = GetTE(i, j);

            bool canToggle = chest != null && (chest.OpenedBy == player.whoAmI || chest.OpenedBy < 0);

            if (canToggle && chest.OpenedBy < 0)
            {
                // The following four (4) if-blocks are recommended to be used if your multitile opens a UI when right clicked:
                if (player.sign > -1)
                {
                    SoundEngine.PlaySound(SoundID.MenuClose);
                    player.sign = -1;
                    Main.editSign = false;
                    Main.npcChatText = string.Empty;
                }
                if (Main.editChest)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    Main.editChest = false;
                    Main.npcChatText = string.Empty;
                }
                if (player.editedChestName)
                {
                    NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
                    player.editedChestName = false;
                }
                if (player.talkNPC > -1)
                {
                    player.SetTalkNPC(-1);
                    Main.npcChatCornerItem = 0;
                    Main.npcChatText = string.Empty;
                }
            }

            if (canToggle)
                chest.Toggle(player);

            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            Point16 topLeft = TileHelpers.GetTopLeftTileInMultitile(i, j);

            ITDChestTE chest = GetTE(i, j);
            if (chest != null)
            {
                player.cursorItemIconID = -1;
                string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language
                player.cursorItemIconText = chest.StorageName.Length > 0 ? chest.StorageName : defaultName;
                if (player.cursorItemIconText == defaultName)
                {
                    player.cursorItemIconID = ItemType;
                    if (Framing.GetTileSafely(topLeft).TileFrameX / (18 * Dimensions.X) == 1)
                    {
                        player.cursorItemIconID = KeyType;
                    }

                    player.cursorItemIconText = "";
                }
            }
            else
            {
                player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
            }

            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }

        public override void MouseOverFar(int i, int j)
        {
            MouseOver(i, j);
            Player player = Main.LocalPlayer;
            if (player.cursorItemIconText == "")
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
        public static string MapChestName(string name, int i, int j)
        {
            Tile tile = Main.tile[i, j];
            ITDChest chestTile = TileLoader.GetTile(tile.TileType) as ITDChest;

            if (TileHelpers.TryGetTileEntityAs(chestTile.TE.GetType(), i, j, out var entity) && entity is ITDChestTE chest)
            {
                if (string.IsNullOrEmpty(chest.StorageName))
                    return name;
                return name + ": " + chest.StorageName;
            }
            return Language.GetTextValue("LegacyChestType.0");
        }
    }
}
