using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using ITD.Content.Items.Placeable.Furniture.Catacombs;

namespace ITD.Content.Tiles.Furniture.Catacombs
{
    public class CatacombChairTile : ITDChair
    {
        private readonly int BlueChair = ModContent.ItemType<BlueCatacombChair>();
        private readonly int GreenChair = ModContent.ItemType<GreenCatacombChair>();
        private readonly int PinkChair = ModContent.ItemType<PinkCatacombChair>();
        public override void SetStaticChairDefaults()
        {
            DustType = DustID.Shadowflame;
            MapColor = new Color(30, 63, 96);
            ChairItem = [BlueChair, GreenChair, PinkChair];
        }
    }
}
