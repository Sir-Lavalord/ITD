using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;
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
			Skullraiser,
			Goodbye
        }
		private ActionState AI_State;
		private int AttackCycle = 0;
		private int StateTimer = 100;
		private Vector2 Teleposition;
		
        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
			Main.npcFrameCount[Type] = 3;
        }
        public override void SetDefaults()
        {
			AI_State = ActionState.Chasing;
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 40;
            NPC.defense = 5;
            NPC.lifeMax = 4800;
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
                Music = ITD.Instance.GetMusic("DuneBearer") ?? MusicID.Boss1;
            }
        }
		
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
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
					NPC.velocity.Y += 0.5f;
					if (StateTimer < 10)
					{
						StateTimer = 10;
						if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
							StateSwitch();
					}
					break;
				case ActionState.DarkFountain:
					if (StateTimer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						float range = 1f;
						if (Main.expertMode)
							range = 2f;
						if (Main.masterMode)
							range = 4f;
						for (int i = 0; i < range; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(4f+Main.rand.NextFloat(4f)*range, 4f*Main.rand.NextFloat()), ModContent.ProjectileType<GasLeak>(), 20, 0, -1);
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(-4f-Main.rand.NextFloat(4f)*range, 4f*Main.rand.NextFloat()), ModContent.ProjectileType<GasLeak>(), 20, 0, -1);
						}
					}
					for (int i = 0; i < 4; i++)
					{
						Vector2 dustOffset = new Vector2(0f, 60f).RotatedBy(MathHelper.ToRadians(90*i+45));
						Dust dust = Main.dust[Dust.NewDust(Teleposition, 0, 0, DustID.GiantCursedSkullBolt, 0, 0, 100, default, 1.5f)];
						dust.noGravity = true;
						dust.velocity = dustOffset*0.05f;
					}
					break;
				case ActionState.Skullraiser:
					if (StateTimer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromThis(), Main.player[NPC.target].Center + new Vector2(200f-Main.rand.NextFloat(400f), 400f), new Vector2(0, -4f), ModContent.ProjectileType<Necroskull>(), 20, 0, -1);
						if (Main.expertMode)
							Projectile.NewProjectile(NPC.GetSource_FromThis(), Main.player[NPC.target].Center + new Vector2(200f-Main.rand.NextFloat(400f), 400f), new Vector2(0, -4f), ModContent.ProjectileType<Necroskull>(), 20, 0, -1);
					}
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

                if (NPC.frame.Y > frameHeight * Main.npcFrameCount[Type]-1)
                {
                    NPC.frame.Y = frameHeight;
                }
            }
        }
		
		private void StateSwitch()
		{
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
							StateTimer = 30;
							NPC.velocity = new Vector2(0, -10f);
							break;
						case 1:
							AI_State = ActionState.Chasing;
							StateTimer = 120;
							Necromancy();
							break;
						case 2:
							NPC.TargetClosest(false);
							AI_State = ActionState.Skullraiser;
							StateTimer = 60;
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
					
					Main.player[Main.myPlayer].GetITDPlayer().Screenshake = 20;
					SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
					SoundEngine.PlaySound(SoundID.NPCDeath51, NPC.Center);
					break;
				case ActionState.DarkFountain:
					AI_State = ActionState.Chasing;
					StateTimer = 120;
					Teleport();
					break;
				case ActionState.Skullraiser:
					AI_State = ActionState.Chasing;
					StateTimer = 160;
					break;
				case ActionState.Goodbye:
					NPC.active = false;
					break;
			}
		}
		
		private void Teleport()
		{
			for (int i = 0; i < 60; i++)
			{
				Vector2 dustOffset = new Vector2(0f, 60f).RotatedBy(MathHelper.ToRadians(6*i));
				Dust dust = Main.dust[Dust.NewDust(NPC.Center + dustOffset, 0, 0, DustID.GiantCursedSkullBolt, 0, 0, 100, default, 1.5f)];
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
				Dust dust = Main.dust[Dust.NewDust(NPC.Center, 0, 0, DustID.GiantCursedSkullBolt, 0, 0, 100, default, 1.5f)];
				dust.noGravity = true;
				dust.velocity = dustOffset*0.1f;
			}
		}
		
		public static int[] TheList = new int[]
		{
			NPCID.AngryBones,
			NPCID.AngryBonesBig,
			NPCID.AngryBonesBigHelmet,
			NPCID.AngryBonesBigMuscle,
			NPCID.CursedSkull,
			NPCID.DarkCaster,
		};
		private void Necromancy()
		{
			int tombstones = 0;
			foreach (var target in Main.ActiveNPCs)
            {
                if (target.type == ModContent.NPCType<HauntedTombstone>() && target.ai[0] == NPC.whoAmI)
				{
					tombstones++;
					
					for (int l = 0; l < 10; l++)
					{
						int spawnDust = Dust.NewDust(target.Center, 16, 16, DustID.GiantCursedSkullBolt, 0, 0, 0, default, 2f);
						Main.dust[spawnDust].noGravity = true;
						Main.dust[spawnDust].velocity *= 2f;
					}
					
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						if (Main.masterMode)
							Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center, new Vector2(Main.rand.NextFloat(-3f, 3f), -4f), ModContent.ProjectileType<Necroskull>(), 25, 0f, -1);
						
						if (Main.expertMode)
						{
							for (int i = 0; i < 5; i++)
							{
								Projectile.NewProjectile(NPC.GetSource_FromThis(), target.Center, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-8f, -2f)), ProjectileID.SkeletonBone, 25, 0f, -1);
							}
						}
						
						NPC.NewNPC(NPC.GetSource_FromThis(), (int)(target.Center.X), (int)(target.Center.Y), TheList[Main.rand.Next(6)]);
					}
					target.ai[0] = -1;
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
					if (Main.netMode != NetmodeID.MultiplayerClient)
						NPC.NewNPC(NPC.GetSource_FromThis(), (int)(position.X), (int)(position.Y), ModContent.NPCType<HauntedTombstone>(), 0, NPC.whoAmI);
				}
			}
			
			SoundEngine.PlaySound(SoundID.NPCDeath17, NPC.Center);
			for (int l = 0; l < 20; l++)
			{
				int spawnDust = Dust.NewDust(NPC.Center, 0, 0, DustID.GiantCursedSkullBolt, 0, 0, 0, default, 2f);
				Main.dust[spawnDust].noGravity = true;
				Main.dust[spawnDust].velocity *= 3f;
			}
		}
		
		public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<GravekeeperBag>()));
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, oneFromOptionsDrops));
			npcLoot.Add(notExpertRule);
        }
		
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White * NPC.Opacity;
        }
    }
}