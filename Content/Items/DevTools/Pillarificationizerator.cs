using ITD.Content.Tiles.DeepDesert;
using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.DevTools
{
    public class Pillarificationizerator : ModItem
    {
        public Point origin = new();
        public Point end = new();
        bool destroyPillars = false;
        public override void SetDefaults()
        {
            Item.width = Item.height = 16;
            Item.useTime = Item.useAnimation = 20;
            Item.autoReuse = false;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.Swing;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                destroyPillars = !destroyPillars;
                Main.NewText("Mode changed to: " + (destroyPillars ? "Pillar Destroy" : "Pillar Create"), Color.Yellow);
                return true;
            }
            Point p = player.GetITDPlayer().MousePosition.ToTileCoordinates();
            Rectangle dustRect = new(p.X * 16, p.Y * 16, 16, 16);
            if (destroyPillars)
            {
                if (Framing.GetTileSafely(p).TileType == ModContent.TileType<ReinforcedPegmatitePillar>())
                {
                    ReinforcedPegmatitePillar.Destroy(p);
                    Main.NewText($"Destroyed pillar at {p}", Color.LimeGreen);
                    return true;
                }
                Main.NewText($"No pillar found to destroy at {p}", Color.Red);
                return true;
            }
            if (origin == Point.Zero)
            {
                origin = p;
                end = Point.Zero;
                Dust.DrawDebugBox(dustRect);
                return true;
            }
            end = p;
            int height = origin.Y - end.Y + 1;
            if (height < 2)
            {
                origin = Point.Zero;
                end = Point.Zero;
                Main.NewText("Couldn't generate pillar at the given coordinates: Pillar height can't be less than two", Color.Red);
                return true;
            }
            Dust.DrawDebugBox(dustRect);
            if (ReinforcedPegmatitePillar.Generate(origin.X, origin.Y, height))
            {
                Main.NewText($"Generated pillar with height {height}", Color.LimeGreen);
            }
            origin = Point.Zero;
            return true;
        }
    }
}
