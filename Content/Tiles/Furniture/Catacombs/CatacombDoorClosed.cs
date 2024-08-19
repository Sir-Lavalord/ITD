using ITD.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ITD.Content.Tiles.Furniture.Catacombs
{
	public class CatacombDoorClosed : ModTile
	{
		public override void SetStaticDefaults() {
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
			TileID.Sets.OpenDoorID[Type] = ModContent.TileType<CatacombDoorOpen>();

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

			DustType = DustID.Shadowflame;
			AdjTiles = new int[] { TileID.ClosedDoor };

			// Names
			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Door"));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));
			TileObjectData.addTile(Type);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = GetItemDrops(i, j).ElementAt(0).type;
		}
	}
}