using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class BlueshroomSapling : ITDSapling
    {
        public override void SetStaticSaplingDefaults()
        {
            DustType = DustType<BlueshroomSporesDust>();
            MapColor = Color.Aquamarine;
            GrowSlow = 20;
            GrowsIntoTreeType = TileType<BlueshroomTree>();
            MinGrowHeight = 8;
            MaxGrowHeight = 14;
        }
    }
}