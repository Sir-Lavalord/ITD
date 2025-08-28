using Terraria.GameContent;
using System;

namespace ITD.Content.NPCs.Catacombs
{
    public class BloatedSlime : ModNPC
    {
		public bool canTriggerLanding = false;
		public ref float AITimer => ref NPC.ai[0];
		public ref float AIIsActiveOrOppositeDirTimer => ref NPC.ai[1];
        public ref float AIBigJumpXPosition => ref NPC.ai[2];
		
		public Vector2 stretchScale = Vector2.One;
		public int wobbleTimer;
		
		public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
        }
        public override void SetDefaults()
        {
            NPC.damage = 26;
            //NPC.aiStyle = NPCAIStyleID.Slime;
			AnimationType = NPCID.BlueSlime;
            NPC.width = 44;
            NPC.height = 30;
            NPC.defense = 8;
            NPC.lifeMax = 80;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
		
		public override void AI()
		{				
			bool aggro = false;
			if (!Main.dayTime || NPC.life != NPC.lifeMax || (double)NPC.position.Y > Main.worldSurface * 16.0 || Main.slimeRain) // this part makes the slime chase you
			{
				aggro = true;
			}
			
			if (AIIsActiveOrOppositeDirTimer > 1f)
			{
				AIIsActiveOrOppositeDirTimer -= 1f;
			}
			/*if (NPC.wet)
			{
				if (NPC.collideY)
				{
					NPC.velocity.Y = -2f;
				}
				if (NPC.velocity.Y < 0f && AIBigJumpXPosition == NPC.position.X)
				{
					NPC.direction *= -1;
					AIIsActiveOrOppositeDirTimer = 200f;
				}
				if (NPC.velocity.Y > 0f)
				{
					AIBigJumpXPosition = NPC.position.X;
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
				if (AIIsActiveOrOppositeDirTimer == 1f & aggro)
				{
					NPC.TargetClosest(true);
				}
				canTriggerLanding = false;
			}*/
			
			NPC.aiAction = 0;
			if (AIIsActiveOrOppositeDirTimer == 0f)
			{
				AITimer = -100f;
				AIIsActiveOrOppositeDirTimer = 1f;
				NPC.TargetClosest(true);
			}
			if (NPC.velocity.Y == 0f)
			{
				if (canTriggerLanding)
                {
                    canTriggerLanding = false;
                    stretchScale.X = 2f;
                    stretchScale.Y = 0.5f;
                }
				if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
				{
					NPC.position.X = NPC.position.X - (NPC.velocity.X + (float)NPC.direction);
				}
				if (AIBigJumpXPosition == NPC.position.X)
				{
					NPC.direction *= -1;
					AIIsActiveOrOppositeDirTimer = 200f;
				}
				AIBigJumpXPosition = 0f;
				NPC.velocity.X = NPC.velocity.X * 0.8f;
				if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
				{
					NPC.velocity.X = 0f;
				}
				if (aggro)
				{
					AITimer += 1f;
				}
				AITimer += 1f;
				float num31 = -1000f;
				int num32 = 0;
				if (AITimer >= 0f)
				{
					num32 = 1;
				}
				if (AITimer >= num31 && AITimer <= num31 * 0.5f)
				{
					num32 = 2;
				}
				if (AITimer >= num31 * 2f && AITimer <= num31 * 1.5f)
				{
					num32 = 3;
				}
				if (num32 > 0)
				{
					NPC.netUpdate = true;
					if (aggro && AIIsActiveOrOppositeDirTimer == 1f)
					{
						NPC.TargetClosest(true);
					}
					// big jump
					if (num32 == 3)
					{
						stretchScale.Y = 2f;
                        stretchScale.X = 0.5f;
						NPC.velocity.Y = -8f;
						NPC.velocity.X = NPC.velocity.X + (float)(3 * NPC.direction);
						if (NPC.wet)
						{
							NPC.velocity.Y *= 1.25f;
							NPC.velocity.X *= 2f;
						}
						AITimer = -120f;
						AIBigJumpXPosition = NPC.position.X;
					}
					// regular jump
					else
					{
						stretchScale.Y = 2f;
                        stretchScale.X = 0.5f;
						NPC.velocity.Y = -6f;
						NPC.velocity.X = NPC.velocity.X + (float)(2 * NPC.direction);
						if (NPC.wet)
						{
							NPC.velocity.Y *= 1.25f;
							NPC.velocity.X *= 2f;
						}
						AITimer = -80f;
						if (num32 == 1)
						{
							AITimer += num31;
						}
						else
						{
							AITimer += num31 * 2f;
						}
					}
					canTriggerLanding = true;
				}
				else
				{
					if (AITimer >= -30f)
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
				
		public override bool CheckDead()
        {
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < 5; i++)
				{
					Projectile projectile = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, new Vector2(Main.rand.NextFloat(-12f, 12f), Main.rand.NextFloat(-6f, -4f)), ProjectileID.SkeletonBone, 15, 0, -1);
					if (NPC.wet)
					{
						projectile.velocity.Y *= 1.75f;
						projectile.velocity.X *= 1.75f;
					}
				}
			}
			return base.CheckDead();
		}
		
		public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
			{
				int i = 0;
				while ((double)i < hit.Damage / (double)NPC.lifeMax * 50.0)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, hit.HitDirection, -1f, 0, new Color(87, 149, 173), 1f);
					i++;
				}
				return;
			}
			for (int j = 0; j < 20; ++j)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.TintableDust, 0f, 0f, 0, new Color(87, 149, 173), 1f);
			}
        }
		
		public override void FindFrame(int frameHeight)
        {
			stretchScale = Vector2.SmoothStep(stretchScale, Vector2.One, 0.25f);
        }
		
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            float texHeight = tex.Height / Main.npcFrameCount[Type];
            Vector2 origin = new(tex.Width / 2, texHeight);
            Vector2 offset = new(0f, NPC.gfxOffY + 1f + texHeight / 2f);
            spriteBatch.Draw(tex, NPC.Center - screenPos + offset, NPC.frame, drawColor, 0f, origin, stretchScale, SpriteEffects.None, 0f);
            return false;
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
