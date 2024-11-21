using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITD.Utilities;
using ITD.Utilities.Placeholders;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.DetoursIL;
using ITD.Players;

namespace ITD.Content.Items
{
    public class ITDGlobalItem : GlobalItem
    {
        public override bool? UseItem(Item item, Player player)
        {
            ITDPlayer modPlayer = player.GetITDPlayer();
            if ((item.buffTime > 0))
            {
                if (modPlayer.portableLab)
                {
                    player.AddBuff(item.buffType, (int)(item.buffTime * 1.1f), true);
                }
            }
            return base.UseItem(item, player);
        }
    }
}
