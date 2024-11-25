using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;
using System;

using ITD.Utilities;
using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Hostile;

namespace ITD.Content.NPCs.BlueshroomGroves
{
    public class ShroomishSlime : ModNPC
    {
		public bool attack = false;
		private readonly Asset<Texture2D> glow = ModContent.Request<Texture2D>("ITD/Content/NPCs/BlueshroomGroves/ShroomishSlime_Glow");
		public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
        }
        public override void SetDefaults()
        {
            NPC.damage = 26;
            //NPC.aiStyle = NPCAIStyleID.Slime;
			AnimationType = NPCID.BlueSlime;
            NPC.width = 28;
            NPC.height = 36;
            NPC.defense = 8;
            NPC.lifeMax = 80;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
		
		public override void AI() // slime ai but evil
		{
			bool aggro = false;
			if (!Main.dayTime || NPC.life != NPC.lifeMax || (double)NPC.position.Y > Main.worldSurface * 16.0 || Main.slimeRain) // this part makes the slime chase you
			{
				aggro = true;
			}
			
			if (NPC.ai[2] > 1f)
			{
				NPC.ai[2] -= 1f;
			}
			if (NPC.wet)
			{
				if (NPC.collideY)
				{
					NPC.velocity.Y = -2f;
				}
				if (NPC.velocity.Y < 0f && NPC.ai[3] == NPC.position.X)
				{
					NPC.direction *= -1;
					NPC.ai[2] = 200f;
				}
				if (NPC.velocity.Y > 0f)
				{
					NPC.ai[3] = NPC.position.X;
				}
				if (NPC.velocity.Y > 2f)
				{
					NPC.velocity.Y = NPC.velocity.Y * 0.9f;
				}
				NPC.velocity.Y = NPC.velocity.Y - 0.5f;
				if (NPC.velocity.Y < -4f)
				{
					NPC.velocity.Y = -4f;
				}
				if (NPC.ai[2] == 1f & aggro)
				{
					NPC.TargetClosest(true);
				}
				attack = false; // ow my attack
			}
			
			NPC.aiAction = 0;
			if (NPC.ai[2] == 0f)
			{
				NPC.ai[0] = -100f;
				NPC.ai[2] = 1f;
				NPC.TargetClosest(true);
			}
			if (NPC.velocity.Y == 0f)
			{
				if (attack) // the slime has landed, commence the attack
				{
					if (Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + NPC.velocity * 4, new Vector2(6f, 0), ModContent.ProjectileType<Sporeflake>(), 15, 0, -1);
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + NPC.velocity * 4, new Vector2(-6f, 0), ModContent.ProjectileType<Sporeflake>(), 15, 0, -1);
					}
					SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);
					attack = false;
				}
				if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
				{
					NPC.position.X = NPC.position.X - (NPC.velocity.X + (float)NPC.direction);
				}
				if (NPC.ai[3] == NPC.position.X)
				{
					NPC.direction *= -1;
					NPC.ai[2] = 200f;
				}
				NPC.ai[3] = 0f;
				NPC.velocity.X = NPC.velocity.X * 0.8f;
				if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
				{
					NPC.velocity.X = 0f;
				}
				if (aggro)
				{
					NPC.ai[0] += 1f;
				}
				NPC.ai[0] += 1f;
				float num31 = -1000f;
				int num32 = 0;
				if (NPC.ai[0] >= 0f)
				{
					num32 = 1;
				}
				if (NPC.ai[0] >= num31 && NPC.ai[0] <= num31 * 0.5f)
				{
					num32 = 2;
				}
				if (NPC.ai[0] >= num31 * 2f && NPC.ai[0] <= num31 * 1.5f)
				{
					num32 = 3;
				}
				if (num32 > 0)
				{
					NPC.netUpdate = true;
					if (aggro && NPC.ai[2] == 1f)
					{
						NPC.TargetClosest(true);
					}
					if (num32 == 3)
					{
						NPC.velocity.Y = -8f;
						NPC.velocity.X = NPC.velocity.X + (float)(3 * NPC.direction);
						NPC.ai[0] = -200f;
						NPC.ai[3] = NPC.position.X;
					}
					else
					{
						NPC.velocity.Y = -6f;
						NPC.velocity.X = NPC.velocity.X + (float)(2 * NPC.direction);
						NPC.ai[0] = -120f;
						if (num32 == 1)
						{
							NPC.ai[0] += num31;
						}
						else
						{
							NPC.ai[0] += num31 * 2f;
						}
					}
					attack = true; // prepare attack
				}
				else
				{
					if (NPC.ai[0] >= -30f)
					{
						NPC.aiAction = 1;
						return;
					}
				}
			}
			else
			{
				if (NPC.target < 255 && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
				{
					if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
					{
						NPC.position.X = NPC.position.X - 1.4f * (float)NPC.direction;
					}
					if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
					{
						NPC.position.X = NPC.position.X - (NPC.velocity.X + (float)NPC.direction);
					}
					if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.01) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.01))
					{
						NPC.velocity.X = NPC.velocity.X + 0.2f * (float)NPC.direction;
						return;
					}
					NPC.velocity.X = NPC.velocity.X * 0.93f;
				}
			}
		}
		
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {			
			Rectangle sourceRectangle = NPC.frame;
			Vector2 origin = sourceRectangle.Size() / 2f;

            spriteBatch.Draw(glow.Value, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + 2), sourceRectangle, Color.White * BlueshroomTree.opac, 0f, origin, 1f, SpriteEffects.None, 0f);
        }
			
		public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
			{
				int i = 0;
				while ((double)i < hit.Damage / (double)NPC.lifeMax * 50.0)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<BlueshroomSporesDust>(), hit.HitDirection, -1f, 0, default, 1f);
					i++;
				}
				return;
			}
			for (int j = 0; j < 20; ++j)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<BlueshroomSporesDust>(), 0f, 0f, 0, default, 1f);
			}
        }
			
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
            {
                return 0.45f;
            }
            return 0f;
        }
	}
}
