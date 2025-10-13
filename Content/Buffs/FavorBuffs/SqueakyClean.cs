namespace ITD.Content.Buffs.FavorBuffs;

public class SqueakyClean : ModBuff
{
    public const float SpawnrateMultiplier = 0.85f; // the actual spawnrate modification implementation is in ITDGlobalNPC
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = false;
        Main.debuff[Type] = false;
    }
    public override void Update(Player player, ref int buffIndex)
    {
        player.buffImmune[BuffID.Poisoned] = true;
        player.buffImmune[BuffID.Venom] = true;
        player.buffImmune[BuffID.OnFire] = true;
        player.buffImmune[BuffID.CursedInferno] = true;
        player.buffImmune[BuffID.Ichor] = true;
    }
}
