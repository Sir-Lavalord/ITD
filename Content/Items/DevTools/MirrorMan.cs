using ITD.Content.UI;
using ITD.Networking;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace ITD.Content.Items.DevTools
{
    public class MirrorMan : ModItem
    {
        [Flags]
        public enum MirroringState : byte
        {
            MirrorNone = 1,
            MirrorHorizontally = 2,
            MirrorVertically = 4,
        }
        public bool select = true;
        public MirroringState State
        {
            get
            {
                MirroringState final = MirroringState.MirrorNone;
                MirrorManUI ui = UILoader.GetUIState<MirrorManUI>();
                if (ui.horiToggled)
                    final |= MirroringState.MirrorHorizontally;
                if (ui.vertiToggled)
                    final |= MirroringState.MirrorVertically;
                return final;
            }
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
        public override bool AltFunctionUse(Player player) => true;
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                MirrorManUI ui = UILoader.GetUIState<MirrorManUI>();
                ui.Toggle();
                return true;
            }
            ITDPlayer plr = player.GetITDPlayer();
            Vector2 mouse = plr.MousePosition;
            if (!plr.selectBox)
            {
                plr.selectBox = true;
                return true;
            }
            else
            {
                Main.NewText("Selected area");
                plr.selectTopLeft = Point16.Zero;
                plr.selectBottomRight = Point16.Zero;
                plr.selectBox = false;
                return true;
            }
            return base.UseItem(player);
        }
    }
}
