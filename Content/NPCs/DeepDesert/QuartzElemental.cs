using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;

using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;

namespace ITD.Content.NPCs.DeepDesert
{
    public class QuartzElemental : ModNPC
    {
		public float speed = 3f;
		public float hoverPower = 1.5f;
		public int hoverDistance = 3;
        public override void SetDefaults()
        {
            NPC.damage = 50;
            NPC.aiStyle = -1;
            NPC.width = 36;
            NPC.height = 34;
            NPC.defense = 12;
            NPC.lifeMax = 120;
            NPC.knockBackResist = 0.5f;
			NPC.noGravity = true;
            NPC.value = 400;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;
        }
        public override void AI() // Hovering AI but evil
        {			
			if (NPC.ai[2] >= 0f)
			{
				/*int offset = 16; part that makes it run away in fear
				bool flagX = false;
				bool flagY = false;
				if (NPC.position.X > NPC.ai[0] - (float)offset && NPC.position.X < NPC.ai[0] + (float)offset)
				{
					flagX = true;
				}
				else
				{
					if ((NPC.velocity.X < 0f && NPC.direction > 0) || (NPC.velocity.X > 0f && NPC.direction < 0))
					{
						flagX = true;
					}
				}
				offset += 24;
				if (NPC.position.Y > NPC.ai[1] - (float)offset && NPC.position.Y < NPC.ai[1] + (float)offset)
				{
					flagY = true;
				}
				if (flagX & flagY)
				{
					NPC.ai[2] += 1f;
					if (NPC.ai[2] >= 60f)
					{
						NPC.ai[2] = -200f;
						NPC.direction *= -1;
						NPC.velocity.X = NPC.velocity.X * -1f;
						NPC.collideX = false;
					}
				}
				else
				{
					NPC.ai[0] = NPC.position.X;
					NPC.ai[1] = NPC.position.Y;
					NPC.ai[2] = 0f;
				}*/
				NPC.TargetClosest(true);
			}
			else
			{
				NPC.ai[2] += 1f;
				if (Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2) > NPC.position.X + (float)(NPC.width / 2))
				{
					NPC.direction = -1;
				}
				else
				{
					NPC.direction = 1;
				}
			}
			
			int posX = (int)((NPC.position.X + (float)(NPC.width / 2)) / 16f) + NPC.direction * 2;
			int posY = (int)((NPC.position.Y + (float)NPC.height) / 16f);
			bool fall = true;
			
			//if (NPC.position.Y + (float)NPC.height > Main.player[NPC.target].position.Y)
			//{
				int num;
				for (int i = posY; i < posY + hoverDistance; i = num + 1)
				{
					Tile tile = Main.tile[posX, i];
					if (tile == null)
						tile = new Tile();
					if ((!tile.IsActuated && Main.tileSolid[(int)tile.TileType]) || tile.LiquidAmount > 0)
					{
						fall = false;
						break;
					}
					num = i;
				}
			//}

			if ((Main.player[NPC.target].position - NPC.position).Length() < 200 && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height)) // the part where it attacks
			{
				if (NPC.ai[3] >= 32f)
					NPC.velocity *= 0.9f;
				
				NPC.ai[3] += 1f;
				if (NPC.ai[3] >= 64f)
				{
					NPC.ai[3] = 0f;
					SoundEngine.PlaySound(SoundID.NPCHit54, NPC.Center);
					for (int j = 0; j < 8; ++j)
					{
						int dust = Dust.NewDust(NPC.Center, 0, 0, DustID.GiantCursedSkullBolt, 0f, 0f, 100, default(Color), 2f);
						Main.dust[dust].noGravity = true;
						Main.dust[dust].velocity *= 3f;
						if (Main.netMode != NetmodeID.MultiplayerClient)
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(12f, 0f).RotatedBy(MathHelper.PiOver4*j), ModContent.ProjectileType<QuartzBlast>(), 30, 0, -1);
					}
				}
			}
			else if (NPC.ai[3] > 0f)
				NPC.ai[3]--;
			
			if (fall)
			{
				NPC.velocity.Y = NPC.velocity.Y + 0.1f;
				if (NPC.velocity.Y > 3f)
				{
					NPC.velocity.Y = 3f;
				}
			}
			else
			{
				if (NPC.directionY < 0 && NPC.velocity.Y > 0f)
				{
					NPC.velocity.Y = NPC.velocity.Y - 0.1f;
				}
				if (NPC.velocity.Y < -4f)
				{
					NPC.velocity.Y = -4f;
				}
			}
			
			if (NPC.collideX)
			{
				NPC.velocity.X = NPC.oldVelocity.X * -0.4f;
				if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 1f)
				{
					NPC.velocity.X = 1f;
				}
				if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -1f)
				{
					NPC.velocity.X = -1f;
				}
			}
			if (NPC.collideY)
			{
				NPC.velocity.Y = NPC.oldVelocity.Y * -0.25f;
				if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f)
				{
					NPC.velocity.Y = 1f;
				}
				if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f)
				{
					NPC.velocity.Y = -1f;
				}
			}
			
			if (NPC.direction == -1 && NPC.velocity.X > -speed)
			{
				NPC.velocity.X = NPC.velocity.X - 0.1f;
				if (NPC.velocity.X > speed)
				{
					NPC.velocity.X = NPC.velocity.X - 0.1f;
				}
				else
				{
					if (NPC.velocity.X > 0f)
					{
						NPC.velocity.X = NPC.velocity.X + 0.05f;
					}
				}
				if (NPC.velocity.X < -speed)
				{
					NPC.velocity.X = -speed;
				}
			}
			else
			{
				if (NPC.direction == 1 && NPC.velocity.X < speed)
				{
					NPC.velocity.X = NPC.velocity.X + 0.1f;
					if (NPC.velocity.X < -speed)
					{
						NPC.velocity.X = NPC.velocity.X + 0.1f;
					}
					else
					{
						if (NPC.velocity.X < 0f)
						{
							NPC.velocity.X = NPC.velocity.X - 0.05f;
						}
					}
					if (NPC.velocity.X > speed)
					{
						NPC.velocity.X = speed;
					}
				}
			}
			
			if (NPC.directionY == -1 && NPC.velocity.Y > -hoverPower)
			{
				NPC.velocity.Y = NPC.velocity.Y - 0.04f;
				if (NPC.velocity.Y > hoverPower)
				{
					NPC.velocity.Y = NPC.velocity.Y - 0.05f;
				}
				else
				{
					if (NPC.velocity.Y > 0f)
					{
						NPC.velocity.Y = NPC.velocity.Y + 0.03f;
					}
				}
				if (NPC.velocity.Y < -hoverPower)
				{
					NPC.velocity.Y = -hoverPower;
				}
			}
			else
			{
				if (NPC.directionY == 1 && NPC.velocity.Y < hoverPower)
				{
					NPC.velocity.Y = NPC.velocity.Y + 0.04f;
					if (NPC.velocity.Y < -hoverPower)
					{
						NPC.velocity.Y = NPC.velocity.Y + 0.05f;
					}
					else
					{
						if (NPC.velocity.Y < 0f)
						{
							NPC.velocity.Y = NPC.velocity.Y - 0.03f;
						}
					}
					if (NPC.velocity.Y > hoverPower)
					{
						NPC.velocity.Y = hoverPower;
					}
				}
			}
			
			float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
			NPC.rotation = rotationFactor;
        }
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 position = NPC.Center - screenPos;
			Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            Rectangle glowSourceRectangle = glowTexture.Frame(1, 1);
            Vector2 glowOrigin = glowSourceRectangle.Size() / 2f;
			
			float progress = Math.Clamp((NPC.ai[3] - 32f) / 32f, 0, 1);
			
            Color color = new Color(200, 100, 255, 0) * progress;
			
            Main.EntitySpriteDraw(glowTexture, position, glowSourceRectangle, color, MathHelper.Pi * progress, glowOrigin, 2.4f - progress * 1.6f, SpriteEffects.None, 0f);
			
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;
			
            Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.Lerp(Color.White, drawColor, 0.5f), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
			return false;
        }
		public override bool? CanFallThroughPlatforms() => true;
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneDeepDesert && Main.hardMode)
            {
                return 0.25f;
            }
            return 0f;
        }
    }
}
