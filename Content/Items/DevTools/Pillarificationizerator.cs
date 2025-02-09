using ITD.Content.Tiles.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.DevTools
{
    public class Pillarificationizerator : DevTool
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
                PlayerLog(player, "Mode changed to: " + (destroyPillars ? "Pillar Destroy" : "Pillar Create"), Color.Yellow);
                return true;
            }
            Point p = player.GetITDPlayer().MousePosition.ToTileCoordinates();
            Rectangle dustRect = new(p.X * 16, p.Y * 16, 16, 16);
            if (destroyPillars)
            {
                if (Framing.GetTileSafely(p).TileType == ModContent.TileType<ReinforcedPegmatitePillar>())
                {
                    ReinforcedPegmatitePillar.Destroy(p);
                    PlayerLog(player, $"Destroyed pillar at {p}", Color.LimeGreen);
                    return true;
                }
                PlayerLog(player, $"No pillar found to destroy at {p}", Color.Red);
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
                PlayerLog(player, "Couldn't generate pillar at the given coordinates: Pillar height can't be less than two", Color.Red);
                return true;
            }
            Dust.DrawDebugBox(dustRect);
            if (ReinforcedPegmatitePillar.Generate(origin.X, origin.Y, height))
            {
                PlayerLog(player, $"Generated pillar with height {height}", Color.LimeGreen);
            }
            origin = Point.Zero;
            return true;
        }
    }
}
