using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Items.Accessories.Master
{
    public class Prophylaxis : ModItem
    {
        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
            Item.accessory = true;
        }
		
		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ProphylaxisPlayer>().hasProphylaxis = true;
        }
    }
	
	internal class ProphylaxisPlayer : ModPlayer
    {
        public bool hasProphylaxis;
		public bool bloodletting;
		public int lifeDegen;

        public override void ResetEffects()
        {
            hasProphylaxis = false;
			bloodletting = false;
        }

		public override void UpdateBadLifeRegen()
        {
			if (bloodletting)
			{
				if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
				
				Player.lifeRegenTime = 0;
				 
				Player.lifeRegen -= (int)(lifeDegen*0.2f);
			}
		}
		
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
			if (bloodletting)
				modifiers.FinalDamage.Base -= lifeDegen;
		}
		
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
			if (bloodletting)
			{
				g *= 0.25f;
				b *= 0.25f;
				Main.dust[Dust.NewDust(drawInfo.Position, Player.width, Player.height, DustID.Blood)].velocity *= 0.2f;
			}
		}
    }
	
	internal class ProphylaxisHealItem : GlobalItem
    {
        public override bool? UseItem(Item item, Player player)
        {
			ProphylaxisPlayer modPlayer = player.GetModPlayer<ProphylaxisPlayer>();
            if (item.healLife > 0 && modPlayer.hasProphylaxis)
            {
				modPlayer.lifeDegen = Math.Min(player.GetHealLife(item), player.statLifeMax2-player.statLife);
				player.AddBuff(ModContent.BuffType<BloodlettingBuff>(), 600, false);
            }
            return true;
        }
    }
}
