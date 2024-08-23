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
using ITD.Utils;

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
        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Point tileCoords = Main.MouseWorld.ToTileCoordinates();
                if (player.IsInTileInteractionRange(tileCoords.X, tileCoords.Y, TileReachCheckSettings.Simple))
                {
                    if (Helpers.GrowBluegrass(tileCoords.X, tileCoords.Y))
                    {
                        SoundEngine.PlaySound(SoundID.Dig, Main.MouseWorld);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
