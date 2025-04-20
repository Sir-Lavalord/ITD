using ITD.Content.Items.Accessories.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.EquipmentBuffs
{
    public class BloodlettingBuff : ModBuff
    {		
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }
		
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ProphylaxisPlayer>().bloodletting = true;
        }
    }
}
