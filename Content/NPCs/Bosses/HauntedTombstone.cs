using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.NPCs.Bosses
{
    public class HauntedTombstone : ModNPC
    {
        public override void SetStaticDefaults()
        {
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }
        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 34;
            NPC.damage = 75;
            NPC.defense = 0;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath44;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
			NPC.hide = true;
        }
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = 500;
        }
		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return Main.masterMode;
        }
		
        public override void AI()
        {
			NPC npc = Main.npc[(int)(NPC.ai[0])];
			if (!npc.active)
			{
				NPC.StrikeNPC(new NPC.HitInfo
				{
					Damage = 500,
					Knockback = 0f,
					HitDirection = 0,
					Crit = true
				});
			}
			
			NPC.localAI[0]++;
			NPC.position += new Vector2(0, (float)(Math.Sin(NPC.localAI[0]*0.2f))*0.2f);
        }
		
		public override void OnKill()
        {
			int gore0 = Mod.Find<ModGore>("TombstoneGore0").Type;
            int gore1 = Mod.Find<ModGore>("TombstoneGore1").Type;
            int gore2 = Mod.Find<ModGore>("TombstoneGore2").Type;
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2(-2f, -2f), gore0);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2(0f, -2f), gore1);
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2(2f, -2f), gore2);
			
			NPC npc = Main.npc[(int)(NPC.ai[0])];
			npc.StrikeNPC(new NPC.HitInfo
				{
					Damage = 500,
					Knockback = 0f,
					HitDirection = 0,
					Crit = true
				});
			for (int l = 0; l < 20; l++)
			{
				int spawnDust = Dust.NewDust(npc.Center, 0, 0, DustID.GiantCursedSkullBolt, 0, 0, 0, default, 2f);
				Main.dust[spawnDust].noGravity = true;
				Main.dust[spawnDust].velocity *= 3f;
			}
        }
		
		public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
			Vector2 position = NPC.Center - Main.screenPosition;
			
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			Vector2 origin = sourceRectangle.Size() / 2f;
			
			Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Rectangle glowRectangle = glowTexture.Frame(1, 1);
			Vector2 glowOrigin = glowRectangle.Size() / 2f;
			
			Main.EntitySpriteDraw(glowTexture, position, glowRectangle, Color.White, 0, glowOrigin, NPC.scale, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.Lerp(Color.White, drawColor, 0.5f), 0, origin, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}