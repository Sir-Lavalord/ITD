using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ITD.Utils;

namespace ITD.Content.NPCs.Catacombs
{
    public class AmbushBones : ModNPC
    {
		public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 15;
        }
        public override void SetDefaults()
        {
            NPC.damage = 26;
            NPC.aiStyle = NPCAIStyleID.Fighter;
			AIType = NPCID.AngryBones;
			AnimationType = NPCID.AngryBones;
            NPC.width = 26;
            NPC.height = 40;
            NPC.defense = 8;
            NPC.lifeMax = 80;
            NPC.knockBackResist = 0.8f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
        }
		public override Color? GetAlpha(Color drawColor)
        {
			Player player = Main.player[Main.myPlayer];
			drawColor *= Math.Clamp(4f - NPC.Distance(player.Center)/50f, 0.2f, 1f);
            return drawColor;
        }
		
		public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
			{
				int i = 0;
				while ((double)i < hit.Damage / (double)NPC.lifeMax * 50.0)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, hit.HitDirection, -1f, 0, default, 1f);
					i++;
				}
				return;
			}
			for (int j = 0; j < 20; ++j)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, 26, 2.5f * hit.HitDirection, -2.5f, 0, default, 1f);
			}
			Gore.NewGore(NPC.GetSource_FromThis(), NPC.position, NPC.velocity, 42, NPC.scale);
			Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
			Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
			Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
			Gore.NewGore(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
        }
		
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneCatacombs)
            {
                return 0.25f;
            }
            return 0f;
        }
    }
}
