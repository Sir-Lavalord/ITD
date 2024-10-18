using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using ITD.Utilities;
using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Placeable
{
    public class BluegrassSeeds : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.DefaultToSeeds();
        }
        public override bool? UseItem(Player player) => Helpers.UseItem_PlaceSeeds(player, ModContent.TileType<Bluegrass>(), TileID.SnowBlock);
    }
}
