using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Items.Accessories.Expert
{
    public class CharmOfTheAccursed : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.sellPrice(50000);
            Item.expert = true;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
             player.GetModPlayer<GravelyCharmPlayer>().Accursed = true;
        }
    }
	
	public class GravelyCharmPlayer : ModPlayer
    {
        public bool Accursed;
        public override void ResetEffects()
        {
            Accursed = false;
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (Accursed && info.Damage > 100)
            {
                SoundEngine.PlaySound(SoundID.NPCHit54, Player.Center);
				for (int i = 0; i < 6; i++)
				{
					Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, new Vector2(0, 8f).RotatedByRandom(MathHelper.TwoPi), ModContent.ProjectileType<AccursedSpirit>(), 80, 5f, Player.whoAmI);
				}
            }
        }
    }
}
