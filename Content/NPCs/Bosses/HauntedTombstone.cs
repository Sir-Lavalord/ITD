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
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit42;
            NPC.DeathSound = SoundID.NPCDeath44;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.netAlways = true;
            NPC.aiStyle = -1;
			NPC.hide = true;
        }

        public override void AI()
        {
			NPC npc = Main.npc[(int)(NPC.ai[0])];
			if (!npc.active)
				NPC.life = 0;
			
			NPC.localAI[0]++;
			NPC.position += new Vector2(0, (float)(Math.Sin(NPC.localAI[0]*0.2f))*0.2f);
        }
		
		public override void OnKill()
        {
			NPC npc = Main.npc[(int)(NPC.ai[0])];
			npc.StrikeNPC(new NPC.HitInfo
				{
					Damage = 500,
					Knockback = 0f,
					HitDirection = 0,
					Crit = true
				});
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