using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using ITD.Utilities;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class VocalZero : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<VocalZeroProjectile>(), 16f, 16, 3200, SoundID.NPCDeath13);
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 0, 25);
        }
        public override bool CanUseItem(Player player) => player.GetSnaptrapPlayer().CanUseSnaptrap;
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.GetSnaptrapPlayer().ShootSnaptrap();
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float pulseAmount = Main.mouseTextColor / 255f;
            Color textColor = Color.LightPink * pulseAmount;
            var line = tooltips.First(x => x.Name == "Tooltip0");
            string coloredText = string.Format(line.Text, textColor.Hex3());
            line.Text = coloredText;
        }
    }
}