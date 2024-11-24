using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using Terraria.Localization;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class Bee17 : Favor
    {
        public override string Texture => Placeholder.PHAxe;
        public override int FavorFatigueTime => 20;
        public override bool IsCursedFavor => true;
        public override void SetStaticDefaults()
        {
        }
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
            Item.master = true;
        }
        int MinionCount = 0;
        public override bool UseFavor(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p != null && p.active && p.minion && p.owner == player.whoAmI && p.type != ModContent.ProjectileType<GrumbleBee>() && p.minionSlots > 0)
                {
                    for (int f = 0; f < player.slotsMinions; f++)
                    {
                        Projectile bee = Projectile.NewProjectileDirect(player.GetSource_FromThis(), p.Center, Vector2.Zero,
                            ModContent.ProjectileType<GrumbleBee>(), 20, p.knockBack, player.whoAmI);
                        bee.minionSlots = p.minionSlots;
                        p.Kill();
                    }
                }
            }
            return true;
        }
        public override void UpdateFavor(Player player, bool hideVisual)
        {
            if (!FavorPlayer.UseFavorKey.Current)
            {
                MinionCount = 0;
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.First(x => x.Name == "Tooltip2");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}