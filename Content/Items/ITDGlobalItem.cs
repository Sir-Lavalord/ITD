﻿using ITD.Utilities;
using Terraria;
using Terraria.ModLoader;
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
