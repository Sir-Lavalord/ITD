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

using ITD.Content.Rarities;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Systems;
using ITD.Players;
using ITD.Utilities;
using ITD.Content.Items;

namespace ITD.Content.Items.Favors.Hardmode
{
    public class Cornucopia : Favor
    {
        public override int FavorFatigueTime => 60;
        public override bool IsCursedFavor => false;
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override string GetBarStyle()
        {
            return "DefaultBarStyle";
        }
        public override string GetChargeSound()
        {
            return "DefaultChargeSound";
        }
        public override bool UseFavor(Player player)
        {
			List<Projectile> affected = new List<Projectile>();
			foreach (var target in Main.ActiveProjectiles)
            {
				if (target.Reflectable())
				{
					affected.Add(target);
				}
			}
			
			foreach (var target in affected)
            {
				switch(target.whoAmI % 3) {
					case 0:
						Item heart = Main.item[Item.NewItem(Item.GetSource_FromThis(), target.Center, ItemID.Heart)];
						heart.GetGlobalItem<ITDTermporaryItem>().temporary = true;
						break;
					case 1:
						Item star = Main.item[Item.NewItem(Item.GetSource_FromThis(), target.Center, ItemID.Star)];
						star.GetGlobalItem<ITDTermporaryItem>().temporary = true;
						break;
					case 2:
						Projectile.NewProjectile(Item.GetSource_FromThis(), target.Center, new Vector2((float)Main.rand.Next(-30, 31) * 0.1f, (float)Main.rand.Next(-40, -15) * 0.1f), ModContent.ProjectileType<CornucopiaProjectile>(), target.damage, 0, player.whoAmI);
						break;
				}
				target.Kill();
			}
			
			SoundEngine.PlaySound(SoundID.Item29, player.Center);
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
            var line = tooltips.First(x => x.Name == "Tooltip1");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}