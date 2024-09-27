using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Utilities.Placeholders;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Buffs.Debuffs
{
    public class ToppledDebuff : ModBuff
    {
        public override string Texture => Placeholder.PHDebuff;

        public override void SetStaticDefaults()
        {
        }

    }
    public class ToppledNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        float OriginalKB;
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            OriginalKB = npc.knockBackResist;
        }
        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff<ToppledDebuff>())
            {
                npc.knockBackResist = OriginalKB;
            }
        }
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (npc.HasBuff<ToppledDebuff>())
            {
                npc.knockBackResist += 0.1f;
            }
        }
    }
}
