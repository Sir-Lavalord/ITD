﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Content.Items.DevTools
{
    [Flags]
    public enum SimpleTileDataType : byte
    {
        None = 0,
        Tile = 1,
        Wall = 2,
        Liquid = 4,
        Wiring = 8,
    }
    public abstract class DevTool : ModItem
    {
        public virtual void DrawSpecialPreviews(SpriteBatch sb, Player player)
        {

        }
        public virtual void ProcessInput(KeyboardState k)
        {

        }
        public static void PlayerLog(Player player, object message, Color? color = null)
        {
            if (player.whoAmI != Main.myPlayer)
                return;
            color ??= Color.White;
            Main.NewText(message, color);
        }
    }
}
