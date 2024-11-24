using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.NPCs;
using ITD.Players;
using ITD.Utilities.Placeholders;
using Terraria.ID;

namespace ITD.Content.Items.Accessories.Combat.All
{
    public class SoulTalisman : ModItem
    {
/*        public override string Texture => Placeholder.PHAxe;*/

        public override void SetDefaults()
        {
            Item.DefaultToAccessory(28, 38);
            Item.rare = ItemRarityID.Yellow;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ITDPlayer itdPlayer = player.GetITDPlayer();
            itdPlayer.soulTalisman = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
