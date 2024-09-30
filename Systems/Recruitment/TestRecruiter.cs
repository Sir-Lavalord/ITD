using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Social.Base;
using ITD.Utilities.Placeholders;
using ITD.Detours;
using Terraria.ID;

namespace ITD.Systems.Recruitment
{
    public class TestRecruiter : ModItem
    {
        public override string Texture => Placeholder.PHGeneric;
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            Item.useAnimation = Item.useTime = 16;
            Item.useStyle = ItemUseStyleID.Swing;
        }
        public override bool? UseItem(Player player)
        {
            Vector2 mouse = player.GetITDPlayer().MousePosition;
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.isLikeATownNPC && npc.getRect().Contains(mouse.ToPoint()))
                {
                    if (npc.TryGetGlobalNPC(out TownNPCRecruitmentRunner runner) && runner.isRecruitedBy == player.whoAmI)
                    {
                        DetourManager.GetInstance<TownNPCRecruitmentLoader>().Unrecruit(npc.whoAmI, player);
                        Main.NewText($"Fired {npc.FullName}!", Color.Red);
                        return true;
                    }
                    else
                    {
                        if (DetourManager.GetInstance<TownNPCRecruitmentLoader>().TryRecruit(npc.whoAmI, player))
                        {
                            Main.NewText($"Recruited {npc.FullName}!", Color.LimeGreen);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
