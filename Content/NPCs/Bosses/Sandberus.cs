﻿using System;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;

using ITD.Content.Items.Weapons.Ranger;
using ITD.Content.Items.Other;
using ITD.Content.Projectiles.Hostile.Sandberus;
using ITD.Utilities;
using ITD.Content.Items.Accessories.Defensive.Defense;

namespace ITD.Content.NPCs.Bosses
{
	[AutoloadBossHead]
    public class Sandberus : ITDNPC
    {
		private enum ActionState
        {
            Chasing,
			Cooking,
            Dashing,
			Leaping,
			Clawing
        }
		private ActionState AI_State;
		private int StateTimer = 200;
		private int AttackCycle = 0;
		//private int ShootCycle = 0;
		private float JumpX = 0;
		private float JumpY = 0;
		private bool ValidTargets = true;
		public override void SetStaticDefaultsSafe()
        {
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
			AI_State = ActionState.Chasing;
            NPC.damage = 30;
            NPC.width = 130;
            NPC.height = 110;
            NPC.defense = 2;
            NPC.lifeMax = 750;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 4);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
			NPC.boss = true;
            NPC.npcSlots = 10f;
			if (!Main.dedServ)
            {
                Music = ITD.Instance.GetMusic("DuneBearer") ?? MusicID.Boss1;
            }
        }
		
		public override void OnSpawn(IEntitySource source)
        {
            if (Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(), ModContent.ProjectileType<SuffocationAura>(), 0, 0, -1, NPC.whoAmI);
			}
        }
		
        public override void AI()
        {			
			switch (AI_State)
            {
				case ActionState.Chasing:
					NPC.TargetClosest(false);
					NPC.direction = (NPC.Center.X < Main.player[NPC.target].Center.X).ToDirectionInt();
					NPC.spriteDirection = NPC.direction;
					NPC.velocity.X += NPC.direction * 0.1f;
					NPC.velocity.X = Math.Clamp(NPC.velocity.X, -4f, 4f);
					StepUp();
					//Player player = Main.player[NPC.target];
					//if (NPC.velocity.Y == 0f && player.position.Y < NPC.position.Y)
					//	NPC.velocity.Y = -8f;
                    break;
				case ActionState.Cooking:
					NPC.TargetClosest(false);
					NPC.direction = (NPC.Center.X < Main.player[NPC.target].Center.X).ToDirectionInt();
					NPC.spriteDirection = NPC.direction;
					NPC.velocity *= 0.9f;
					//if (StateTimer == 12 && ShootCycle == 0 && NPC.life < NPC.lifeMax*0.66f && Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
					//	ShootAttack();
					//if (StateTimer == 4 && ShootCycle == 0 && NPC.life < NPC.lifeMax*0.33f && Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
					//	ShootAttack();
                    break;
				case ActionState.Dashing:
					if (StateTimer > 30 && (StateTimer < 70 || Main.expertMode))
					{
						NPC.velocity.X = NPC.direction * 10f;
						if (Main.expertMode)
							NPC.velocity.X *= 1.2f;
					}
					else
						NPC.velocity.X *= 0.9f;
					StepUp();
					if (NPC.collideX)
					{
						StateTimer = 1;
						Main.player[Main.myPlayer].GetITDPlayer().BetterScreenshake(20, 4, 4, false);
						SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
					}
					else if (Math.Abs(NPC.velocity.X) > 6 && Main.netMode != NetmodeID.MultiplayerClient)
						SpikeTrail();
					break;
				case ActionState.Leaping:
					if (StateTimer > 10)
					{
						NPC.noTileCollide = true;
						NPC.velocity.X = JumpX;
						NPC.velocity.Y = JumpY;
						JumpY += 0.3f;
					}
					else if (NPC.noTileCollide && ValidTargets)
						NPC.noTileCollide = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);
					if (NPC.velocity.Y == 0f)
					{
						StateTimer = 1;
						for (int i = 0; i < 20; i++)
						{
							int landingDust = Dust.NewDust(NPC.Center + new Vector2(-NPC.width, NPC.height*0.5f), NPC.width*2, 0, DustID.Dirt, 0f, 0f, 0, default, 1.5f);
							Main.dust[landingDust].noGravity = true;
							Main.dust[landingDust].velocity.Y = -5f * Main.rand.NextFloat();
						}
						for (int j = 0; j < 10; j++)
						{
							Vector2 position = NPC.Center + new Vector2(-NPC.width+(NPC.width*0.2f*j), NPC.height*0.5f);
							Gore.NewGore(NPC.GetSource_FromThis(), position, new Vector2(0,0), 61 + j % 3);
						}
						if (Main.expertMode && Main.netMode != NetmodeID.MultiplayerClient)
						{
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.width, NPC.height*0.5f), new Vector2(4f, -12f), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-NPC.width, NPC.height*0.5f), new Vector2(-4f, -12f), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.width*0.5f, NPC.height*0.5f), new Vector2(2f, -16f), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(-NPC.width*0.5f, NPC.height*0.5f), new Vector2(-2f, -16f), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
						}
						Main.player[Main.myPlayer].GetITDPlayer().BetterScreenshake(20, 4, 4, false);
						SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
					}
					else if (StateTimer < 10)
						StateTimer = 10;
					break;
				case ActionState.Clawing:
					if (StateTimer < 18 && Main.netMode != NetmodeID.MultiplayerClient)
						SpikeAttack(20-StateTimer);
						if (Main.masterMode && Main.netMode != NetmodeID.MultiplayerClient)
						{
							if (StateTimer == 16)
								Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.width*NPC.direction, NPC.height*0.5f), new Vector2(6f*NPC.direction*Main.rand.NextFloat(0.8f, 1f), -8f*Main.rand.NextFloat(0.8f, 1f)), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
							if (StateTimer == 10)
								Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.width*NPC.direction, NPC.height*0.5f), new Vector2(8f*NPC.direction*Main.rand.NextFloat(0.8f, 1f), -12f*Main.rand.NextFloat(0.8f, 1f)), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
							if (StateTimer == 4)
								Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(NPC.width*NPC.direction, NPC.height*0.5f), new Vector2(12f*NPC.direction*Main.rand.NextFloat(0.8f, 1f), -14f*Main.rand.NextFloat(0.8f, 1f)), ModContent.ProjectileType<SandBoulder>(), 15, 0, -1);
						}

					NPC.velocity *= 0.9f;
					break;
			}
			
			int dust = Dust.NewDust(NPC.position + new Vector2(0f, NPC.height), NPC.width, 0, DustID.Dirt, 0f, 0f, 0, default, 1.5f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity.X = NPC.velocity.X *0.5f;
			Main.dust[dust].velocity.Y = -2f * Main.rand.NextFloat();
		
			StateTimer--;
			if (StateTimer == 0)
				StateSwitch();
        }
		
		private void StateSwitch()
		{
			switch (AI_State)
            {
				case ActionState.Chasing:
					AI_State = ActionState.Cooking;
					StateTimer = 25;
					//if (Main.expertMode)
					//	ShootCycle = ++ShootCycle % 2;
					//else
					//	ShootCycle = ++ShootCycle % 3;
					//if (ShootCycle == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					//	ShootAttack();
					break;
				case ActionState.Cooking:
					AttackCycle = ++AttackCycle % 3;
					NPC.TargetClosest(false);
					if (Main.player[NPC.target].dead)
					{
						ValidTargets = false;
						AttackCycle = 1;
					}
					NPC.direction = (NPC.Center.X < Main.player[NPC.target].Center.X).ToDirectionInt();
					NPC.spriteDirection = NPC.direction;
					switch (AttackCycle)
					{
						case 0:
							AI_State = ActionState.Dashing;
							StateTimer = 80;
							SoundEngine.PlaySound(SoundID.NPCDeath17, NPC.Center);
							break;
						case 1:
							AI_State = ActionState.Leaping;
							StateTimer = 60;
							Vector2 distance;
							distance = Main.player[NPC.target].Center - NPC.Center;
							distance.X /= StateTimer;
                            distance.Y = distance.Y / StateTimer - 0.18f * StateTimer;
							JumpX = Math.Clamp(distance.X, -16, 16);
							JumpY = distance.Y;
							for (int j = 0; j < 5; j++)
							{
								float offset = -NPC.width+(NPC.width*Main.rand.NextFloat(2f));
								Vector2 position = NPC.Center + new Vector2(offset, NPC.height*0.5f - 20f);
								Gore.NewGorePerfect(NPC.GetSource_FromThis(), position, new Vector2(offset*0.02f, Main.rand.NextFloat()), 61 + j % 3);
							}
							SoundEngine.PlaySound(SoundID.NPCDeath17, NPC.Center);
							break;
						case 2:
							AI_State = ActionState.Clawing;
							StateTimer = 20;
							NPC.velocity.X = NPC.direction * 12f;
							SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
							break;

					}
					break;
				default:
					AI_State = ActionState.Chasing;
					StateTimer = 100;
					break;
			}
		}
		
		//private void ShootAttack()
		//{
		//	Vector2 toPlayer = Main.player[NPC.target].Center - NPC.Center;
		//	toPlayer.Normalize();
		//	Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, toPlayer * 4f, ModContent.ProjectileType<SandberusSkull>(), 15, 0, -1);
		//}
		
		private void SpikeTrail()
		{
			Point point = NPC.Bottom.ToTileCoordinates();
			point.X += -NPC.direction * 3;
				
			int bestY = SpikeAttackFindBestY(ref point, point.X);
			if (!WorldGen.ActiveAndWalkableTile(point.X, bestY))
			{
				return;
			}
			Vector2 position = new Vector2((float)(point.X * 16 + 8), (float)(bestY * 16 - 8));
			Vector2 velocity = new Vector2(0f, -1f).RotatedBy((double)((float)(20 * -NPC.direction) * 0.7f * (0.7853982f / 20f)), default(Vector2));
			Projectile.NewProjectile(NPC.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<SandSpike>(), 15, 0f, -1, 0f, Main.rand.NextFloat(0.4f, 0.6f), 0f);
		}
		
		private void SpikeAttack(int i)
		{
			Point point = NPC.Bottom.ToTileCoordinates();
			point.X += NPC.direction * 3;
				
			int num = point.X + i * NPC.direction;
			int bestY = SpikeAttackFindBestY(ref point, num);
			if (!WorldGen.ActiveAndWalkableTile(num, bestY))
			{
				return;
			}
			Vector2 position = new Vector2((float)(num * 16 + 8), (float)(bestY * 16 - 8));
			Vector2 velocity = new Vector2(0f, -1f).RotatedBy((double)((float)(i * NPC.direction) * 0.7f * (0.7853982f / 20f)), default(Vector2));
			Projectile.NewProjectile(NPC.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<SandSpike>(), 15, 0f, -1, 0f, Main.rand.NextFloat(0.1f, 0.2f) + (float)i * 1.1f / 20f, 0f);
		}
		
		private int SpikeAttackFindBestY(ref Point sourceTileCoords, int x)
		{
			int num = sourceTileCoords.Y;
			int num8 = 0;
			while (num8 < 20 && num >= 10 && WorldGen.SolidTile(x, num, false))
			{
				num--;
				num8++;
			}
			int num9 = 0;
			while (num9 < 20 && num <= Main.maxTilesY - 10 && !WorldGen.ActiveAndWalkableTile(x, num))
			{
				num++;
				num9++;
			}
			return num;
		}
		
		public override bool? CanFallThroughPlatforms ()
        {
			Player player = Main.player[NPC.target];
            return AI_State == ActionState.Chasing && player.position.Y > NPC.position.Y + NPC.height;
        }
		
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
			Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = texture.Size() / 2f;
			
			SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			
            if (AI_State == ActionState.Dashing || AI_State == ActionState.Leaping)
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 trailPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width*0.5f, NPC.height*0.5f) + new Vector2(0f, NPC.gfxOffY-4f);
                    Color color = drawColor * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(texture, trailPos, null, color, 0f, drawOrigin, NPC.scale, effects, 0);
                }
            }
			else if (AI_State == ActionState.Cooking) {
				screenPos += new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, 8f));
			}
			
			Vector2 drawPos = NPC.position - screenPos + new Vector2(NPC.width*0.5f, NPC.height*0.5f) + new Vector2(0f, NPC.gfxOffY-4f);
            spriteBatch.Draw(texture, drawPos, null, drawColor, 0f, drawOrigin, NPC.scale, effects, 0);
			
            return false;
        }
				
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<BottomlessSandBucket>(), 1));
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Skullmet>(), 1));
			notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Dunebarrel>(), 5));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SandBlock, 1, 20, 30));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.DesertFossil, 1, 5, 15));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.FossilOre, 1, 5, 8));
			npcLoot.Add(notExpertRule);
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<SandberusBag>()));
        }
    }
}
