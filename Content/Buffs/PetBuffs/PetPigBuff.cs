using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Utilities.Placeholders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.PetBuffs
{
    public class PetPigBuff : ModBuff
    {
        public override string Texture => Placeholder.PHBuff;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<PetPigPet>());
        }
    }
}
