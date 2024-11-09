using ITD.Content.Buffs.Debuffs;
using ITD.Content.Items.Weapons.Ranger;
using ITD.Content.NPCs;
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
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ITDGlobalNPC>().toppled = true;
        }

    }
}
