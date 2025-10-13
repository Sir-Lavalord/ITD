using ITD.Content.Projectiles.Friendly.Pets;

namespace ITD.Content.Buffs.PetBuffs;

public class CosmicJamBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeededAndSetTime(buffIndex, ref unused, ModContent.ProjectileType<CosmicJamPet>());
    }
}