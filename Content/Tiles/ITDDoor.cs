using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;

namespace ITD.Content.Tiles;

public abstract class ITDDoor : ModType
{
    protected sealed override void Register()
    {
        ModTypeLookup<ITDDoor>.Register(this);
    }
    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }
    public sealed override void Load()
    {
        AddContent();
        LoadSafe();
    }
    public virtual void LoadSafe()
    {

    }
    public virtual string TexturesPath => (GetType().Namespace + ".").Replace('.', '/');
    /// <summary>
    /// If the length of this array is not the same as the number of tile styles, the overflowing styles will use the latest valid dust type.
    /// </summary>
    public virtual int[] DustTypes => [DustID.WoodFurniture];
    /// <summary>
    /// If the length of this array is not the same as the number of tile styles, the overflowing styles will use the latest valid nullable <see cref="Color"/>.
    /// </summary>
    public virtual Color?[] MapColors => [null];
    /// <summary>
    /// The length of this array MUST be the same as the dumber of tile styles.
    /// </summary>
    public virtual int[] DropItems => [ItemID.WoodenDoor];
    public ITDOpenedDoor OpenedDoor { get; private set; }
    public int OpenedType => OpenedDoor.Type;
    public ITDClosedDoor ClosedDoor { get; private set; }
    public int ClosedType => ClosedDoor.Type;
    public void AddContent()
    {
        OpenedDoor = new(this);
        Mod.AddContent(OpenedDoor);
        ClosedDoor = new(this);
        Mod.AddContent(ClosedDoor);
    }
}
[Autoload(false)]
public sealed class ITDOpenedDoor(ITDDoor parent) : ModTile
{
    public override string Texture => Parent.TexturesPath + Name;
    public override string Name => Parent.Name + "Open";
    public ITDDoor Parent { get; private set; } = parent;
    private static ushort Style(int i, int j) => (ushort)TileObjectData.GetTileStyle(Framing.GetTileSafely(i, j));
    private readonly int[] dropItems = parent.DropItems;
    private readonly Color?[] mapColors = parent.MapColors;
    private readonly int[] dustTypes = parent.DustTypes;
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoSunLight[Type] = true;
        TileID.Sets.HousingWalls[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.CloseDoorID[Type] = Parent.ClosedType;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

        AdjTiles = [TileID.OpenDoor];

        for (int i = 0; i < dropItems.Length; i++)
        {
            RegisterItemDrop(dropItems[i], i);
        }

        for (int i = 0; i < mapColors.Length; i++)
        {
            // change the default color to the wooden door color please qangel please
            AddMapEntry(mapColors[i] ?? Color.White, Language.GetText("MapObject.Door"));
        }

        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(0, 1);
        TileObjectData.addAlternate(0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(0, 2);
        TileObjectData.addAlternate(0);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 0);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 1);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Origin = new Point16(1, 2);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);
    }
    public override ushort GetMapOption(int i, int j)
    {
        ushort style = Style(i, j);
        return (ushort)Math.Clamp(style, 0, mapColors.Length - 1);
    }
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
    public override bool CreateDust(int i, int j, ref int type)
    {
        ushort style = Style(i, j);
        type = dustTypes[Math.Clamp(style, 0, dustTypes.Length - 1)];
        return true;
    }
    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = dropItems[Style(i, j)];
    }
}
[Autoload(false)]
public sealed class ITDClosedDoor(ITDDoor parent) : ModTile
{
    public override string Texture => Parent.TexturesPath + Name;
    public override string Name => Parent.Name + "Closed";
    public ITDDoor Parent { get; private set; } = parent;
    private static ushort Style(int i, int j) => (ushort)TileObjectData.GetTileStyle(Framing.GetTileSafely(i, j));
    private readonly Color?[] mapColors = parent.MapColors;
    private readonly int[] dustTypes = parent.DustTypes;
    private readonly int[] dropItems = parent.DropItems;
    public override void SetStaticDefaults()
    {
        // Properties
        Main.tileFrameImportant[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.NotReallySolid[Type] = true;
        TileID.Sets.DrawsWalls[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.OpenDoorID[Type] = Parent.OpenedType;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

        AdjTiles = [TileID.ClosedDoor];

        // Names
        for (int i = 0; i < mapColors.Length; i++)
        {
            // change the default color to the wooden door color please qangel please
            AddMapEntry(mapColors[i] ?? Color.White, Language.GetText("MapObject.Door"));
        }

        // Placement
        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));
        TileObjectData.addTile(Type);
    }
    public override ushort GetMapOption(int i, int j)
    {
        ushort style = Style(i, j);
        return (ushort)Math.Clamp(style, 0, mapColors.Length - 1);
    }
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
    public override bool CreateDust(int i, int j, ref int type)
    {
        ushort style = Style(i, j);
        type = dustTypes[Math.Clamp(style, 0, dustTypes.Length - 1)];
        return true;
    }
    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = dropItems[Style(i, j)];
    }
}
