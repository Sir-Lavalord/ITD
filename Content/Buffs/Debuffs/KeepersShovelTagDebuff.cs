using ITD.Utilities.Placeholders;

namespace ITD.Content.Buffs.Debuffs;

public class KeepersShovelTagDebuff : ModBuff
{
    public const int TagDamage = 4;
    public override string Texture => Placeholder.PHDebuff;
    public override void SetStaticDefaults()
    {
        BuffID.Sets.IsATagBuff[Type] = true;
    }
}
public class KeepersShovelTaggedNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
            return;

        var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
        if (npc.HasBuff<KeepersShovelTagDebuff>())
        {
            modifiers.FlatBonusDamage += KeepersShovelTagDebuff.TagDamage * projTagMultiplier;
        }
    }
}