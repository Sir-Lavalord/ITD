using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Audio;
using System;

using ITD.Utilities;
using ITD.Content.Dusts;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.NPCs.Minibiomes.BlackMold
{
    public class MoldSpore : ModNPC
    {		
		public ref float ImmuneTimer => ref NPC.ai[0];
		public override void SetStaticDefaults()
        {
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<MelomycosisBuff>()] = true;
        }
        public override void SetDefaults()
        {
            NPC.damage = 26;
            NPC.aiStyle = NPCAIStyleID.Spore;
			//AnimationType = NPCID.BlueSlime;
            NPC.width = 12;
            NPC.height = 12;
            NPC.lifeMax = 1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.npcSlots = 0f;
        }
		
		public override bool PreAI()
		{
			if (ImmuneTimer > 0)
				ImmuneTimer--;
			//if (Main.rand.NextBool())
			//{
				Dust dust = Main.dust[Dust.NewDust(NPC.Top, 0, 0, DustID.Ambient_DarkBrown, 0f, 0f, 0, default, 1f)];
				dust.velocity = new Vector2();
				dust.noGravity = true;
			//}
			return true;
		}
		
		public override bool? CanBeHitByItem(Player player, Item item) // invulnerability so it doesn't die instantly when spawned in by mold slime
		{
			if (ImmuneTimer > 0)
				return false;
			return null;
		}
		
		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			if (ImmuneTimer > 0)
				return false;
			return null;
		}
		
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 60 * 8);
        }
		
		public override void HitEffect(NPC.HitInfo hit)
        {
			for (int j = 0; j < 15; ++j)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ambient_DarkBrown, 0f, 0f, 0, default, 1f);
			}
        }
		
        //public override float SpawnChance(NPCSpawnInfo spawnInfo)
        //{
        //    if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
        //    {
        //        return 0.45f;
        //    }
        //    return 0f;
        //}
	}
}
