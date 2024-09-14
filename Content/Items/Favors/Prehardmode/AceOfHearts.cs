using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using ITD.Systems;
using Terraria.ModLoader;
using Terraria.Localization;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class AceOfHearts : Favor
    {
        public override int FavorFatigueTime => 240;
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.rare = ModContent.RarityType<Rarities.CursedFavorRarity>();
        }
        public override string GetBarStyle()
        {
            return "AceOfHeartsBarStyle";
        }
        public override bool UseFavor(Player player)
        {
            // does nothing rn, this is just for testing something
            return true;
        }
        public override float ChargeAmount(ChargeData chargeData)
        {
            if (chargeData.Type == ChargeType.DamageGiven)
            {
                return 0.05f;
            }
            return 0f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.First(x => x.Name == "Tooltip3");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}
