using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Content.Projectiles.Friendly.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.SummonBuffs
{
    public class BloodPactBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<BloodPactSpirit>());
        }
    }
}
