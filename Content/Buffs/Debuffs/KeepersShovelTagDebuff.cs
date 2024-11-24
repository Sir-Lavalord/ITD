using ITD.Content.Buffs.Debuffs;
using ITD.Utilities.Placeholders;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class KeepersShovelTagDebuff : ModBuff
    {
        public override string Texture => Placeholder.PHDebuff;

        public override void SetStaticDefaults()
        {
            BuffID.Sets.IsATagBuff[Type] = true;
        }
        public static int TagDamage = 4;

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