using ITD.Content.Projectiles.Friendly.Pets;

namespace ITD.Content.Buffs.PetBuffs
{
    public class SnowyLanternBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            bool unused = false;
            player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<WingedSnowpoffPet>());
        }
    }
}
