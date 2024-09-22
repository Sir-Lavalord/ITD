using ITD.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.DataStructures;
using Terraria.ID;
using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Items.Favors.Hardmode
{
    public class BloodPact : Favor
    {
        public override int FavorFatigueTime => 0;
        public override bool IsCursedFavor => true;

        private int lifeConsumed;
		private int lifeTimer;
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
            return true;
        }

        public override void UpdateFavor(Player player, bool hideVisual)
        {
            int bloodPactSpirit = ModContent.ProjectileType<BloodPactSpirit>();
            if (player.ownedProjectileCounts[bloodPactSpirit] > 0)
                return;
            if (FavorPlayer.UseFavorKey.Current)
            {
                lifeTimer = ++lifeTimer % 5;
				if (lifeTimer == 1)
				{
					lifeConsumed += 1;
					player.statLife -= 5;
					CombatText.NewText(player.getRect(), CombatText.LifeRegen, 5, false, true);
					
					SoundEngine.PlaySound(SoundID.NPCHit1, player.Center);
					for (int i = 0; i < 6; i++)
					{
						Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood, 0, 0f, 40, default, 2f);
						d.noGravity = true;
					}
				}
            }
            if (FavorPlayer.UseFavorKey.JustReleased)
            {
                // I'm using Projectile.ai[0] here in the newProjectile call as timeLeft, if you wanna change the amount of time relative to lifeConsumed the projectile should exist.
				// Now with diminishing returns!
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, bloodPactSpirit, lifeConsumed * 20, 0f, player.whoAmI, (float)(Math.Pow(lifeConsumed, 0.666)));				
				SoundEngine.PlaySound(SoundID.NPCDeath5, player.Center);
				
				lifeTimer = 0;
				lifeConsumed = 0;
            }
            
			if (player.statLife <= 0 && player.whoAmI == Main.myPlayer)
			{
				string death = Language.GetTextValue($"Mods.ITD.DeathMessage.BloodPact");
				player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} {death}"), 10.0, 0, false);
			}
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.First(x => x.Name == "Tooltip0");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}