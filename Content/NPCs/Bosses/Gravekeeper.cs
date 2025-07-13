using System;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;

using ITD.Utilities;
using ITD.ItemDropRules.Conditions;
using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile.Gravekeeper;
using ITD.Content.Items.Other;
using ITD.Content.Items.Weapons.Mage;
using ITD.Content.Items.Weapons.Melee;
using ITD.Content.Items.Weapons.Summoner;

namespace ITD.Content.NPCs.Bosses
{
	[AutoloadBossHead]
    public class Gravekeeper : ModNPC
    {
		public static int[] oneFromOptionsDrops =
        {
            ModContent.ItemType<Ectoblade>(),
            ModContent.ItemType<Necromatome>(),
            ModContent.ItemType<KeepersShovel>(),
        };
		
		private enum ActionState
        {
            Chasing,
			Cooking,
            ShovelSlam,
			DarkFountain,
			Skullduggery,
			TrailOfHell,
			Goodbye
        }
		private ActionState AI_State;
		private int AttackCycle = 0;
		private int StateTimer = 100;
		private int StateExtra = 0;
		private Vector2 Teleposition;
		//private int Form = 0;
		
        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
			Main.npcFrameCount[Type] = 7;
        }
        public override void SetDefaults()
        {
			AI_State = ActionState.Chasing;
            NPC.width = 80;
            NPC.height = 80;
            NPC.damage = 40;
            NPC.defense = 5;
            NPC.lifeMax = 2400;
			NPC.dontTakeDamage = true;
			NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
			NPC.boss = true;
            NPC.npcSlots = 10f;
			
			if (!Main.dedServ)
            {
                Music = ITD.Instance.GetMusic("Gravekeeper") ?? MusicID.Boss1;
            }
        }
		/*public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
			if (NPC.downedPlantBoss)
			{
				Form = 1;
				NPC.lifeMax *= 5;
				NPC.life *= 5;
				NPC.damage *= 2;
				NPC.defense *= 2;
				NPC.frame.Y = (int)(NPC.frame.Size().Y)*Main.npcFrameCount[Type];
			}
        }*/
		
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
			//if (Form == 1)
			//	target.AddBuff(ModContent.BuffType<SoulRotBuff>(), 60 * 8);
			//else
				target.AddBuff(ModContent.BuffType<NecrosisBuff>(), 60 * 8);
        }
		
        float speed = 10;
        float inertia = 100;
        public override void AI()
        {
			switch (AI_State)
            {
				case ActionState.Chasing:
					NPC.TargetClosest(false);
					Vector2 vectorToTarget = Main.player[NPC.target].Center - NPC.Center;
					float distanceToTarget = vectorToTarget.Length();
					if (Main.player[NPC.target].dead || distanceToTarget > 2000f)
					{
						AI_State = ActionState.Goodbye;
						StateTimer = 40;
						NPC.velocity = new Vector2(0, 4f);
					}
					else
					{
						if (distanceToTarget > 10f)
						{
							vectorToTarget.Normalize();
							vectorToTarget *= speed;
						}
						NPC.velocity = (NPC.velocity * (inertia - 2) + vectorToTarget) / inertia;
					}
					break;
				case ActionState.Cooking:
					NPC.velocity *= 0.9f;
                    break;
				case ActionState.ShovelSlam:
					if (StateTimer < 10)
					{
						NPC.velocity.Y += 0.5f;
						StateTimer = 10;
						if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
							StateSwitch();
					}
					else
					{
						Vector2 offset = new Vector2();
						double angle = Main.rand.NextDouble() * 2d * Math.PI;
						offset.X += (float)(Math.Sin(angle) * 100);
						offset.Y += (float)(Math.Cos(angle) * 100);
						Vector2 spawnPos = NPC.Center + offset - new Vector2(4, 0);
						int dustType = DustID.GiantCursedSkullBolt;
						//if (Form == 1)
						//	dustType = DustID.DungeonSpirit;
						Dust dust = Main.dust[Dust.NewDust(
							spawnPos, 0, 0,
							dustType, 0, 0, 0, default, 2f
							)];
						dust.velocity = -offset * 0.08f + NPC.velocity;
						dust.noGravity = true;
					}
					break;
				case ActionState.DarkFountain:
					if (StateTimer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						float range = 1f;
						if (Main.expertMode)
							range = 2f;
						if (Main.masterMode)
							range = 3f;
						int type = ModContent.ProjectileType<GasLeak>();
						int damage = 20;
						//if (Form == 1)
						//{
						//	type = ModContent.ProjectileType<SoulLeak>();
						//	damage = 40;
						//}
						for (int i = 0; i < range; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(6f+Main.rand.NextFloat(4f)*range, 4f*Main.rand.NextFloat()), type, damage, 0, -1);
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(-6f-Main.rand.NextFloat(4f)*range, 4f*Main.rand.NextFloat()), type, damage, 0, -1);
						}
					}
					for (int i = 0; i < 4; i++)
					{
						Vector2 dustOffset = new Vector2(0f, 60f).RotatedBy(MathHelper.ToRadians(90*i+45));
						int dustType = DustID.GiantCursedSkullBolt;
						//if (Form == 1)
						//	dustType = DustID.DungeonSpirit;
						Dust dust = Main.dust[Dust.NewDust(Teleposition, 0, 0, dustType, 0, 0, 100, default, 1.5f)];
						dust.noGravity = true;
						dust.velocity = dustOffset*0.05f;
					}
					break;
				case ActionState.Skullduggery:
					if (StateTimer < 10)
					{
						if (StateExtra < 3)
						{
							NPC.velocity.Y += 1f;
							StateTimer = 10;
							if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
							{
								StateExtra++;
								NPC.velocity.Y = -8f;
								StateTimer = 35;
								
								if (Main.netMode != NetmodeID.MultiplayerClient)
								{
									int type = ModContent.ProjectileType<NecroSkull>();
									int damage = 20;
									//if (Form == 1)
									//{
									//	type = ModContent.ProjectileType<SoulSkull>();
									//	damage = 40;
									//}
									int skullCount = 3;
									if (Main.expertMode)
										skullCount += 1;
									for (int l = 0; l < skullCount; l++)
									{
										float offset = Main.rand.NextFloat(-200f, 200f);
										Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(offset, 0f), new Vector2(offset*0.01f, -2f), type, damage, 0, -1);
									}
									
								}
								SoundEngine.PlaySound(SoundID.Item70, NPC.Center);
								int dustType = DustID.GiantCursedSkullBolt;
								//if (Form == 1)
								//	dustType = DustID.DungeonSpirit;
								for (int l = 0; l < 8; l++)
								{
									int spawnDust = Dust.NewDust(NPC.Center, 0, 0, dustType, 0, 0, 0, default, 2f);
									Main.dust[spawnDust].noGravity = true;
									Main.dust[spawnDust].velocity *= 3f;
								}
							}
						}
						else
							StateSwitch();
					}
					else
						NPC.velocity *= 0.9f;
					break;
				case ActionState.TrailOfHell:
					if (StateTimer == 30)
					{
						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							int type = ModContent.ProjectileType<MiasmaTrail>();
							int damage = 20;
							int trailCount = 6;
							Vector2 vectorToTarget2 = Main.player[NPC.target].Center - NPC.Center;
							for (int l = 0; l < trailCount; l++)
							{
								Vector2 offset = vectorToTarget2.RotatedByRandom(MathHelper.ToRadians(60));
								offset.Normalize();
								offset *= 100f;
								Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + offset, new Vector2(), type, damage, 0, -1, vectorToTarget2.X, vectorToTarget2.Y, 12);
							}
						}
						SoundEngine.PlaySound(SoundID.Item103, NPC.Center);
						int dustType = DustID.GiantCursedSkullBolt;
						//if (Form == 1)
						//	dustType = DustID.DungeonSpirit;
						for (int l = 0; l < 8; l++)
						{
							int spawnDust = Dust.NewDust(NPC.Center, 0, 0, dustType, 0, 0, 0, default, 2f);
							Main.dust[spawnDust].noGravity = true;
							Main.dust[spawnDust].velocity *= 3f;
						}
					}
					NPC.velocity *= 0.9f;
					break;
			}
			
			StateTimer--;
			if (StateTimer == 0)
				StateSwitch();
			
			float maxRotation = MathHelper.Pi / 6;
			float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);

			float rotation = rotationFactor * maxRotation;
			NPC.rotation = rotation;
			NPC.spriteDirection = (NPC.Center.X < Main.player[NPC.target].Center.X).ToDirectionInt();
			
			if (AI_State == ActionState.Goodbye)
			{
				if (NPC.Opacity > 0f)
					NPC.Opacity -= 0.025f;
			}
			else
			{
				if (NPC.Opacity < 1f)
					NPC.Opacity += 0.05f;
			}
        }
		
		public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1f;
            if (NPC.frameCounter > 5f)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y > frameHeight * (Main.npcFrameCount[Type] - 1))
                {
                    NPC.frame.Y = frameHeight;
                }
            }
        }
		
		private void StateSwitch()
		{
			StateExtra = 0;
			switch (AI_State)
            {
				case ActionState.Chasing:
					AI_State = ActionState.Cooking;
					StateTimer = 20;
					break;
				case ActionState.Cooking:
					AttackCycle = ++AttackCycle % 3;
					switch (AttackCycle)
					{
						case 0:
							AI_State = ActionState.ShovelSlam;
							StateTimer = 40;
							NPC.velocity = new Vector2(0, -3f);
							SoundEngine.PlaySound(SoundID.Zombie54, NPC.Center);
							break;
						case 1:
							AI_State = ActionState.Chasing;
							StateTimer = 120;
							Necromancy();
							break;
						//case 2:
						//	NPC.TargetClosest(false);
						//	AI_State = ActionState.Skullduggery;
						//	StateTimer = 10;
						//	break;
						case 2:
							NPC.TargetClosest(false);
							AI_State = ActionState.TrailOfHell;
							StateTimer = 30;
							break;
					}
					break;
				case ActionState.ShovelSlam:
					NPC.TargetClosest(false);
					Vector2 vectorToTarget = Main.player[NPC.target].Center - NPC.Center;
					float distanceToTarget = vectorToTarget.Length();
					if (Main.player[NPC.target].dead || distanceToTarget > 2000f)
					{
						AI_State = ActionState.Goodbye;
						StateTimer = 40;
						NPC.velocity = new Vector2(0, 4f);
					}
					else
					{
						AI_State = ActionState.DarkFountain;
						StateTimer = 32;
						NPC.velocity = new Vector2(0, 0.5f);
						
						/*Vector2 tpOffset = new Vector2();
						double angle = Main.rand.NextDouble() * 2d * Math.PI;
						tpOffset.X += (float)(Math.Sin(angle) * 240);
						tpOffset.Y += (float)(Math.Cos(angle) * 240);*/
				
						Teleposition = Main.player[NPC.target].Center - new Vector2(0, 240f);
					}
					
					Main.player[Main.myPlayer].GetITDPlayer().BetterScreenshake(20, 4, 4, false);
					SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
					SoundEngine.PlaySound(SoundID.NPCDeath51, NPC.Center);
					break;
				case ActionState.DarkFountain:
					AI_State = ActionState.Chasing;
					StateTimer = 120;
					Teleport();
					break;
				case ActionState.Skullduggery:
					AI_State = ActionState.Chasing;
					StateTimer = 160;
					break;
				case ActionState.TrailOfHell:
					AI_State = ActionState.Chasing;
					StateTimer = 200;
					break;
				case ActionState.Goodbye:
					NPC.active = false;
					break;
			}
		}
		
		private void Teleport()
		{
			int dustType = DustID.GiantCursedSkullBolt;
			//if (Form == 1)
			//	dustType = DustID.DungeonSpirit;
			for (int i = 0; i < 60; i++)
			{
				Vector2 dustOffset = new Vector2(0f, 60f).RotatedBy(MathHelper.ToRadians(6*i));
				Dust dust = Main.dust[Dust.NewDust(NPC.Center + dustOffset, 0, 0, dustType, 0, 0, 100, default, 1.5f)];
				dust.noGravity = true;
				dust.velocity = -dustOffset*0.05f;
			}			
			NPC.Center = Teleposition;
			
			Vector2 vectorToTarget = Main.player[NPC.target].Center - NPC.Center;
			float distanceToTarget = vectorToTarget.Length();
			
			if (distanceToTarget > 10f)
			{
				vectorToTarget.Normalize();
				vectorToTarget *= speed;
			}
			
			NPC.velocity = vectorToTarget;
			NPC.Opacity = 0f;
			
			SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
			for (int i = 0; i < 60; i++)
			{
				Vector2 dustOffset = new Vector2(0f, 60f).RotatedBy(MathHelper.ToRadians(6*i));
				Dust dust = Main.dust[Dust.NewDust(NPC.Center, 0, 0, dustType, 0, 0, 100, default, 1.5f)];
				dust.noGravity = true;
				dust.velocity = dustOffset*0.1f;
			}
		}
		
		private void Necromancy()
		{
			SoundEngine.PlaySound(SoundID.NPCDeath17, NPC.Center);
			int dustType = DustID.GiantCursedSkullBolt;
			//if (Form == 1)
			//	dustType = DustID.DungeonSpirit;
			for (int l = 0; l < 20; l++)
			{
				int spawnDust = Dust.NewDust(NPC.Center, 0, 0, dustType, 0, 0, 0, default, 2f);
				Main.dust[spawnDust].noGravity = true;
				Main.dust[spawnDust].velocity *= 3f;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			int tombstones = 0;
			foreach (var target in Main.ActiveNPCs)
            {
                if ((target.type == ModContent.NPCType<HauntedTombstone>() || target.type == ModContent.NPCType<GhastlyTombstone>()) && target.ai[0] == NPC.whoAmI)
				{
					tombstones++;
					target.ai[0] = -1;
					NetMessage.SendData(23, -1, -1, null, target.whoAmI, 0f, 0f, 0f, 0, 0, 0);
					NetMessage.SendStrikeNPC(target, new NPC.HitInfo // horrible evil networking so the client can see the tombstone dying
						{
							HideCombatText = true,
							InstantKill = true,
						});
				}
            }
			byte numPlayers = 0;
			for (int i = 0; i < 255; i++)
			{
				if (Main.player[i].active)
				{
					numPlayers++;
				}
			}
			while (tombstones < 4 + numPlayers * 2)
			{
				tombstones++;
				
				Vector2 position = NPC.Center + new Vector2(Main.rand.NextFloat(-600f, 600), Main.rand.NextFloat(-300f, 300));
				Point point = position.ToTileCoordinates();
				
				int j = 0;
				while (j < 32 && point.Y >= 10 && WorldGen.SolidTile(point.X, point.Y, false))
				{
					point.Y--;
					j++;
				}
				int k = 0;
				while (k < 32 && point.Y <= Main.maxTilesY - 10 && !WorldGen.ActiveAndWalkableTile(point.X, point.Y))
				{
					point.Y++;
					k++;
				}
				
				position = new Vector2((float)(point.X * 16 + 8), (float)(point.Y * 16 - 8));
				if (WorldGen.ActiveAndWalkableTile(point.X, point.Y) && !WorldGen.SolidTile(point.X, point.Y-1, false))
				{
					int type = ModContent.NPCType<HauntedTombstone>();
					//if (Form == 1)
					//	type = ModContent.NPCType<GhastlyTombstone>();
					NPC.NewNPC(NPC.GetSource_FromThis(), (int)(position.X), (int)(position.Y), type, 0, NPC.whoAmI);
				}
			}
		}
		
		public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
			/*LeadingConditionRule downedPlanteraRule = new LeadingConditionRule(new DownedPlanteraButBetter()); // woe is me right now
            downedPlanteraRule.OnSuccess(ItemDropRule.BossBag(ModContent.ItemType<HardmodeGravekeeperBag>()));
			npcLoot.Add(downedPlanteraRule);
			LeadingConditionRule downedPlanteraNotRule = new LeadingConditionRule(new DownedPlanteraNot());
			downedPlanteraNotRule.OnSuccess(ItemDropRule.BossBag(ModContent.ItemType<GravekeeperBag>()));
			npcLoot.Add(downedPlanteraNotRule);*/
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<GravekeeperBag>()));
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, oneFromOptionsDrops));
			notExpertRule.OnSuccess(ItemDropRule.ByCondition(new DownedPlanteraButBetter(), ItemID.Ectoplasm, 1, 5, 10));
			npcLoot.Add(notExpertRule);
        }
		
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White * NPC.Opacity;
        }
    }
}