using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Buffs.Debuffs;

public class WaxWhipTagDebuff : ModBuff
{
    public const int TagDamage = 4;
    public override void SetStaticDefaults()
    {
        BuffID.Sets.IsATagBuff[Type] = true;
    }
}
public class WaxWhipTaggedNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (projectile.npcProj || projectile.trap)
            return;

        if (npc.HasBuff<WaxWhipTagDebuff>())
        {
            if (ProjectileID.Sets.IsAWhip[projectile.type])
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int debuffType = ModContent.BuffType<WaxWhipTagDebuff>();
                    int buffIndex = npc.FindBuffIndex(debuffType);
                    if (buffIndex != -1)
                    {
                        npc.DelBuff(buffIndex);
                    }
                    Projectile boom = Projectile.NewProjectileDirect(projectile.GetSource_FromThis(), npc.Center,
                        Vector2.Zero, ModContent.ProjectileType<WaxWhipExplosion>(), (int)(projectile.damage * 1.5f), projectile.knockBack, projectile.owner);
                    boom.hostile = false;
                    boom.friendly = true;
                }
            }
        }

        var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];
        if (npc.HasBuff<WaxWhipTagDebuff>())
        {
            modifiers.FlatBonusDamage += WaxWhipTagDebuff.TagDamage * projTagMultiplier;
        }
    }
}