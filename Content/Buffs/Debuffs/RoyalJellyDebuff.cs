namespace ITD.Content.Buffs.Debuffs;

public class RoyalJellyDebuff : ModBuff
{
    public const int TagDamage = 6;//Raw tag damage, might be op
    public override void SetStaticDefaults()
    {
        BuffID.Sets.IsATagBuff[Type] = true;
    }
}
public class RoyalJellyDebuffTag : GlobalNPC
{
    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (projectile.npcProj || projectile.trap)
            return;


        var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
        if (npc.HasBuff<RoyalJellyDebuff>())
        {
            modifiers.FlatBonusDamage += RoyalJellyDebuff.TagDamage * projTagMultiplier;
        }
    }
    public override void PostAI(NPC npc)
    {
        if (npc.HasBuff<RoyalJellyDebuff>())
        {
            npc.color = Color.Orange;
        }
        else npc.color = default;
    }
}

