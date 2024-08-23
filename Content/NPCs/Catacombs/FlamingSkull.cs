﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ITD.Utils;

namespace ITD.Content.NPCs.Catacombs
{
    public class FlamingSkull : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.damage = 35;
            NPC.aiStyle = -1;
            NPC.width = 26;
            NPC.height = 26;
            NPC.defense = 6;
            NPC.lifeMax = 60;
            NPC.knockBackResist = 0.2f;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit3;
            NPC.DeathSound = SoundID.NPCDeath3;
        }
		public override Color? GetAlpha(Color drawColor)
        {
            return new Color(255, 255, 255, 100);
        }
        public override bool PreAI()
        {
			NPC.TargetClosest(false);
			Player player = Main.player[NPC.target];
            Vector2 toPlayerTotal = player.Center - NPC.Center;
            Vector2 toPlayer = toPlayerTotal.SafeNormalize(Vector2.Zero);
			
			NPC.velocity += toPlayer*0.1f;
			NPC.velocity *= 0.98f;
			NPC.spriteDirection = (NPC.velocity.X > 0).ToDirectionInt();
			
			if (NPC.spriteDirection == 1)
				NPC.rotation = NPC.velocity.ToRotation();
			else
				NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2*2;

            int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 0, 0, 100, default, 2f);
			Main.dust[dust].noGravity = true;
            return true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(24, 480);
        }
		public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
			{
				for (int i = 0; i < 15; ++i)
				{
					int dustId = Dust.NewDust(NPC.Center, 0, 0, DustID.Torch, 0.0f, 0.0f, 100, default, 2f);
					Main.dust[dustId].noGravity = true;
					Main.dust[dustId].velocity *= 5f;
				}
			}
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