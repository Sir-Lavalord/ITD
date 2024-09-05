using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

using ITD.Utilities;
using ITD.Content.Buffs.Debuffs;

using ITD.Content.Items.Accessories.Defensive;
using ITD.Content.Items.Accessories.Master;
using ITD.Content.Items.Weapons.Melee.Snaptraps;

namespace ITD.Content.NPCs
{
    public class ITDGlobalNPC : GlobalNPC
    {
		public override bool InstancePerEntity => true;

		public bool zapped;
        public bool toasted;
		
		public override void ResetEffects(NPC npc)
        {
			zapped = false;
            toasted = false;
		}
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (toasted)
            {
                modifiers.Defense *= ToastedBuff.DefenseMultiplier;
            }
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (toasted)
            {
                modifiers.SourceDamage += ToastedBuff.DamageMultiplier;
            }
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (toasted)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }
                npc.lifeRegen -= 60;
				
				if (damage < 10)
                    damage = 10;
            }
        }
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.EaterofSouls || npc.type == NPCID.BigEater || npc.type == NPCID.LittleEater)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Soulsnapper>(), 50));
            }
            if (npc.type == NPCID.Crimera || npc.type == NPCID.LittleCrimera || npc.type == NPCID.BigCrimera)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Crimeratrap>(), 50));
            }
			if (npc.type == NPCID.BloodNautilus)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DreadShell>(), 10));
            }
			if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail)
            {
				LeadingConditionRule IsABoss = new LeadingConditionRule(new Conditions.LegacyHack_IsABoss());
				IsABoss.OnSuccess(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<EoWTail>()));
				npcLoot.Add(IsABoss);
            }
			if (npc.type == NPCID.BrainofCthulhu)
            {
                npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Prophylaxis>()));
            }
        }
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneDeepDesert)
            {
                pool[NPCID.CaveBat] = 0f;
                pool[NPCID.BlueSlime] = 0f;
                pool[NPCID.GiantWalkingAntlion] = 0f;
                pool[NPCID.GiantFlyingAntlion] = 0f;
                pool[NPCID.FlyingAntlion] = 0f;
                pool[NPCID.WalkingAntlion] = 0f;
                pool[NPCID.Antlion] = 0f;
            }
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (toasted)
            {
                drawColor.G = 100;
            }
        }
    }
}
