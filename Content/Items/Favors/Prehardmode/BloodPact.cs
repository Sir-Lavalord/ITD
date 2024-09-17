using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;
using ITD.Content.Rarities;
using ITD.Content.Projectiles.Friendly.Summoner;
using Microsoft.Xna.Framework.Input;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class BloodPact : Favor
    {
        public override int FavorFatigueTime => 0;
        public override bool IsCursedFavor => true;

        private bool favorActivated;

        private int lifeConsumed;
        public override void SetStaticDefaults()
        {

        }
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override string GetBarStyle()
        {
            return "BloodPactBarStyle";
        }
        public override string GetChargeSound()
        {
            return "SoapBubbles";
        }

        public override bool UseFavor(Player player)
        {
            favorActivated = true;
            lifeConsumed = 0;

            return true;
        }

        public override void UpdateFavor(Player player, bool hideVisual)
        {
            base.UpdateFavor(player, hideVisual);

            if (FavorPlayer.UseFavorKey.Current)
            {

                player.lifeRegen = 0;
                player.statLife -= 1;
                lifeConsumed += 1;
            }
            else if (favorActivated == true)
            {
                player.AddBuff(ModContent.BuffType<Content.Buffs.SummonBuffs.BloodPactBuff>(), 60 * lifeConsumed);
                Item.shoot = ModContent.ProjectileType<BloodPactSpirit>();
                favorActivated = false;
            }
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
            var line = tooltips.First(x => x.Name == "Tooltip5");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}