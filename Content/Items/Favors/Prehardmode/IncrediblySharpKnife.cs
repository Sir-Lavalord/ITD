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

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class IncrediblySharpKnife : Favor
    {
        public override int FavorFatigueTime => 0;
        public override bool IsCursedFavor => true;
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(7, 9));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override bool UseFavor(Player player)
        {
            player.Hurt(PlayerDeathReason.ByCustomReason(""), (int)(player.statLifeMax / 5f), 0, dodgeable: false, knockback: 0);
            int type = ModContent.ProjectileType<ShadowKnife>();
            for (int i = 0; i < 8; i++)
            {
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, type, 40, 0f, player.whoAmI, i + player.ownedProjectileCounts[type]);
            }
            for (int i = 0; i < 60; i++)
            {
                Vector2 pos = Main.rand.NextVector2Unit() * 64f;
                Dust d = Dust.NewDustDirect(player.Center + pos, 1, 1, DustID.Shadowflame);
                d.scale *= 1.5f;
            }
            SoundEngine.PlaySound(SoundID.NPCHit54, player.Center);
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.First(x => x.Name == "Tooltip2");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}
