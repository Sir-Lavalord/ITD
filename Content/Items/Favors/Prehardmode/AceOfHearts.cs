using System.Collections.Generic;
using System.Linq;
using ITD.Systems;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class AceOfHearts : Favor
    {
        public override int FavorFatigueTime => 0;
        public override bool IsCursedFavor => true;
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override bool UseFavor(Player player)
        {
            // does nothing rn, this is just for testing something
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.First(x => x.Name == "Tooltip3");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}
