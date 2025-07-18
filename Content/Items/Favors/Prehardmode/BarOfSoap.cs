﻿using ITD.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using ITD.Content.Buffs.FavorBuffs;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class BarOfSoap : Favor
    {
        public override int FavorFatigueTime => 60;
        public override bool IsCursedFavor => false;
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(7, 13));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override string GetBarStyle()
        {
            return "SoapBarStyle";
        }
        public override string GetChargeSound()
        {
            return "SoapBubbles";
        }
        public override bool UseFavor(Player player)
        {
            for (int i = 0; i < 8; i++)
            {
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 2f, (float)Math.Sin(MathHelper.PiOver4 * i) * 2f), ProjectileID.PureSpray, 0, 0.1f, player.whoAmI);
            }
            player.AddBuff(ModContent.BuffType<SqueakyClean>(), 60 * 60 * 4);
            return true;
        }
        public override float ChargeAmount(ChargeData chargeData)
        {
            if (chargeData.Type == ChargeType.DamageGiven)
            {
                return 0.01f;
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