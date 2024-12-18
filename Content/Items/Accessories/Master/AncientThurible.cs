using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using Terraria.Localization;
using ITD.Content.Items.Other;
using System.Collections.Generic;
namespace ITD.Content.Items.Accessories.Master
{
    public class AncientThurible : ModItem
    {

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
        }
    }
    public class AncientThuriblePlayer : ModPlayer
    {
        public bool deerclopsMasterAcc;
        public int thuribleEmptySlot;
        public bool thuribleWings;
        public bool thuribleBoots;
        public bool thuribleShield;

        public override void ResetEffects() //Resets bools if the item is unequipped
        {
            deerclopsMasterAcc = false;

        }
        public override void PostUpdateEquips() //Updates every frame
        {
            if (deerclopsMasterAcc)
            {
                Player.statLife = thuribleEmptySlot;
                bool hasShield = Player.shield != -1;
                bool hasWings = Player.wings != -1;

                for (int i = 3; i < 8 + Player.extraAccessorySlots; i++)
                {
                    var item = Player.armor[i].type;
/*                    foreach (item == ItemID.None in Main.playerInventory)
*/                    {

                    }
              
                }
                if (thuribleEmptySlot > 0)
                {
                    if (!hasWings || !thuribleWings)
                    {
                        thuribleWings = true;
                        thuribleEmptySlot--;
                        Player.wingTimeMax += 900;
                    }
                    if (!hasShield && (thuribleWings || hasWings))
                    {
                        thuribleShield = true;
                        thuribleEmptySlot--;
                        Player.statDefense += 100;
                        Player.noKnockback = true;
                    }
/*                    if (hasbo &&(hasShield || thuribleShield) && (thuribleWings || hasWings) && !thuribleBoots)
                    {
                        thuribleBoots = true;
                        thuribleEmptySlot--;
                        Player.statDefense += 100;
                        Player.noKnockback = true;
                    }*/
                }
            }
        }
    }
}

