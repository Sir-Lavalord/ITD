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

using ITD.Content.Projectiles.Hostile;

namespace ITD.Content.NPCs.Bosses
{
    public class Gravekeeper : ModNPC
    {
		private enum ActionState
        {
            Chasing,
			Cooking,
            ShovelSlam,
			DarkFountain
        }
		private ActionState AI_State;
		private int StateTimer = 100;

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
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }
        float speed = 10;
        float inertia = 100;
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
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(4f+Main.rand.NextFloat(4f), 4f*Main.rand.NextFloat()), ModContent.ProjectileType<GasLeak>(), 20, 0, -1);
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Bottom, new Vector2(-4f-Main.rand.NextFloat(4f), 4f*Main.rand.NextFloat()), ModContent.ProjectileType<GasLeak>(), 20, 0, -1);
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
					AI_State = ActionState.ShovelSlam;
					StateTimer = 30;
					NPC.velocity = new Vector2(0, -10f);
					break;
				case ActionState.ShovelSlam:
					AI_State = ActionState.DarkFountain;
					StateTimer = 32;
					NPC.velocity = new Vector2(0, 0.5f);
					SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
					break;
				case ActionState.DarkFountain:
					AI_State = ActionState.Chasing;
					StateTimer = 160;
					break;
			}
		}
		
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
    }
}