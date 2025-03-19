using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
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
using Terraria.Audio;

using ITD.Systems;
using ITD.Content.Rarities;
using ITD.Content.Buffs.FavorBuffs;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class Trailblazer : Favor
    {
        public override int FavorFatigueTime => 60;
        public override bool IsCursedFavor => false;
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override string GetBarStyle()
        {
            return "FireBarStyle";
        }
        public override string GetChargeSound()
        {
            return "DefaultChargeSound";
        }
        public override bool UseFavor(Player player)
        {
			if (player.velocity.X == 0f)
				return false;
			if (player.velocity.X > 0) {
				if (player.velocity.X < 16f)
					player.velocity.X = 16f;
			}
            else {
				if (player.velocity.X > -16f)
					player.velocity.X = -16f;
			}
			player.AddBuff(ModContent.BuffType<Trailblazing>(), 60 * 5);
			
			for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(player.Center, 1, 1, DustID.Torch, 0f, 0f, 0, default, 4f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 5f;
				Main.dust[dust].velocity -= player.velocity * Main.rand.NextFloat();
            }
			SoundEngine.PlaySound(SoundID.Item45, player.Center);
            return true;
        }
        public override float ChargeAmount(ChargeData chargeData)
        {
            if (chargeData.Type == ChargeType.DistanceTravelled && Math.Abs(chargeData.AmountX) > 2f)
            {
                return 0.002f;
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