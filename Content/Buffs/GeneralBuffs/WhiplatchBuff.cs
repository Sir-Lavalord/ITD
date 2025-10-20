namespace ITD.Content.Buffs.GeneralBuffs;

public class WhiplatchBuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.GetModPlayer<WhiplatchBuffPlayer>().hasWhiplatchBuff = true;
    }
}
internal class WhiplatchBuffPlayer : ModPlayer
{
    public bool hasWhiplatchBuff;
    public float power;
    public override void ResetEffects()
    {
        if (!hasWhiplatchBuff)
        {
            power = 0;
        }
        hasWhiplatchBuff = false;
    }
    public override void PostUpdateBuffs()
    {
        if (hasWhiplatchBuff)
        {
            Player.GetDamage<GenericDamageClass>() += (0.35f * power) + 0.05f;
        }
    }
}
