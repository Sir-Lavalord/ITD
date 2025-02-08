using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Content.Items.DevTools
{
    public class MirrorMan : ModItem
    {
        public enum MirrorManState : byte
        {
            Select,
            MirrorHorizontally,
            MirrorVertically,
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.useTime = Item.useAnimation = 20;
            Item.autoReuse = false;
            Item.consumable = false;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Swing;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
    }
}
