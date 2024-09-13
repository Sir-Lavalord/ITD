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

using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;

namespace ITD.Content.NPCs.Bosses
{
    public class Gravekeeper : ModNPC
    {
		private enum ActionState
        {
            Chasing,
			Cooking,
            ShovelSlam,
			DarkFountain,
			Skullraiser
        }
		private ActionState AI_State;
		private int AttackCycle = 0;
		private int GoonCount = 1;
		private int StateTimer = 100;
		private Vector2 Teleposition;
		
        public override void SetStaticDefaults()
        {
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }
        public override void SetDefaults()
        {
			AI_State = ActionState.Chasing;
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 40;
            NPC.defense = 5;
            NPC.lifeMax = 5000;
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
        float inertia = 20;
        public override void AI()
        {
			switch (AI_State)
            {
				case ActionState.Chasing:
					NPC.TargetClosest();
			
					Vector2 vectorToIdlePosition = Main.player[NPC.target].Center - NPC.Center;
					float distanceToIdlePosition = vectorToIdlePosition.Length();
					
					if (distanceToIdlePosition > 10f)
					{
						vectorToIdlePosition.Normalize();
						vectorToIdlePosition *= speed;
					}
					
					NPC.velocity = (NPC.velocity * (inertia - 2) + vectorToIdlePosition) / inertia;
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
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						float range = 1f;
						if (Main.expertMode)
							range = 2f;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(4f+Main.rand.NextFloat(4f)*range, 4f*Main.rand.NextFloat()), ModContent.ProjectileType<GasLeak>(), 20, 0, -1);
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(-4f-Main.rand.NextFloat(4f)*range, 4f*Main.rand.NextFloat()), ModContent.ProjectileType<GasLeak>(), 20, 0, -1);
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
			
			if (NPC.Opacity < 1f)
				NPC.Opacity += 0.05f;
        }
		
		private void StateSwitch()
		{
			switch (AI_State)
            {
				case ActionState.Chasing:
					AI_State = ActionState.Cooking;
					StateTimer = 20;
					if (NPC.lifeMax-NPC.life > GoonCount * 500)
					{
						SummonGoons();
						GoonCount++;
					}
					break;
				case ActionState.Cooking:
					AttackCycle = ++AttackCycle % 2;
					switch (AttackCycle)
					{
						case 0:
							AI_State = ActionState.ShovelSlam;
							StateTimer = 30;
							NPC.velocity = new Vector2(0, -10f);
							break;
						case 1:
							AI_State = ActionState.Skullraiser;
							StateTimer = 60;
							break;
					}
					break;
				case ActionState.ShovelSlam:
					AI_State = ActionState.DarkFountain;
					StateTimer = 32;
					NPC.velocity = new Vector2(0, 0.5f);
					
					Vector2 tpOffset = new Vector2();
					double angle = Main.rand.NextDouble() * 2d * Math.PI;
					tpOffset.X += (float)(Math.Sin(angle) * 240);
					tpOffset.Y += (float)(Math.Cos(angle) * 240);
			
					Teleposition = Main.player[NPC.target].Center - tpOffset;
					
					Main.player[Main.myPlayer].GetITDPlayer().Screenshake = 20;
					SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
					SoundEngine.PlaySound(SoundID.NPCDeath51, NPC.Center);
					break;
				case ActionState.DarkFountain:
					AI_State = ActionState.Chasing;
					StateTimer = 100;
					Teleport();
					break;
				default:
					AI_State = ActionState.Chasing;
					StateTimer = 120;
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
			
			Vector2 vectorToIdlePosition = Main.player[NPC.target].Center - NPC.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			
			if (distanceToIdlePosition > 10f)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
			}
			
			NPC.velocity = vectorToIdlePosition;
			NPC.Opacity = 0f;
			
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
		private void SummonGoons()
		{
			SoundEngine.PlaySound(SoundID.NPCDeath17, NPC.Center);
			
			Player player = Main.player[NPC.target];
			for (int i = 0; i < 6; i++)
			{
				Vector2 skeletonOffset = new Vector2(0f, 400f).RotatedBy(MathHelper.ToRadians(60*i+30));
				Vector2 position = player.Center + skeletonOffset;
				Point point = position.ToTileCoordinates();
				
				int j = 0;
				while (j < 24 && point.Y >= 10 && WorldGen.SolidTile(point.X, point.Y, false))
				{
					point.Y--;
					j++;
				}
				int k = 0;
				while (k < 24 && point.Y <= Main.maxTilesY - 10 && !WorldGen.ActiveAndWalkableTile(point.X, point.Y))
				{
					point.Y++;
					k++;
				}
				
				position = new Vector2((float)(point.X * 16 + 8), (float)(point.Y * 16 - 8));
				
				if (Collision.CanHit(player.Center, 1, 1, position, 8, 8))
				{
					for (int l = 0; l < 10; l++)
					{
						int spawnDust = Dust.NewDust(position, 16, 16, DustID.GiantCursedSkullBolt, 0, 0, 0, default, 2f);
						Main.dust[spawnDust].noGravity = true;
						Main.dust[spawnDust].velocity *= 2f;
					}
					
					NPC.NewNPC(NPC.GetSource_FromThis(), (int)(position.X), (int)(position.Y), TheList[Main.rand.Next(6)]);
				}
			}
		}
		
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White * NPC.Opacity;
        }
    }
}