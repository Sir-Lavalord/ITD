using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;

namespace ITD.Content.NPCs.Bosses
{
    public class Sandberus : ModNPC
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
		public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
			AI_State = ActionState.Chasing;
            NPC.damage = 50;
            NPC.width = 130;
            NPC.height = 110;
            NPC.defense = 2;
            NPC.lifeMax = 500;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 4);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
			NPC.boss = true;
            NPC.npcSlots = 10f;
        }
        public override void AI()
        {
			if (Main.netMode == 1)
			{
				return;
			}

			switch (AI_State)
            {
				case ActionState.Chasing:
					NPC.TargetClosest(true);
					NPC.spriteDirection = NPC.direction;
					NPC.velocity.X += NPC.direction * 0.1f;
					NPC.velocity.X = Math.Clamp(NPC.velocity.X, -4f, 4f);
					NPCHelpers.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height);
					//Player player = Main.player[NPC.target];
					//if (NPC.velocity.Y == 0f && player.position.Y < NPC.position.Y)
					//	NPC.velocity.Y = -8f;
                    break;
				case ActionState.Cooking:
					NPC.TargetClosest(true);
					NPC.spriteDirection = NPC.direction;
					NPC.velocity *= 0.9f;
                    break;
				case ActionState.Dashing:
					if (StateTimer > 30)
						NPC.velocity.X = NPC.direction * 10f;
					else
						NPC.velocity.X *= 0.9f;
					NPCHelpers.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height);
					break;
				case ActionState.Leaping:
					if (StateTimer > 10)
					{
						NPC.noTileCollide = true;
						NPC.velocity.X = NPC.ai[1];
						NPC.velocity.Y = NPC.ai[2];
						NPC.ai[2] += 0.3f;
					}
					else if (NPC.noTileCollide)
						NPC.noTileCollide = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);
					if (NPC.velocity.Y == 0f)
						StateTimer = 1;
					else if (StateTimer < 10)
						StateTimer = 10;
					break;
				case ActionState.Clawing:
					if (StateTimer < 18)
						SpikeAttack(20-StateTimer);
					NPC.velocity *= 0.9f;
					break;
			}
			
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
					StateTimer = 20;
					break;
				case ActionState.Cooking:
					Player player = Main.player[NPC.target];
					if (player.position.Y < NPC.position.Y)
					{
						AI_State = ActionState.Leaping;
						StateTimer = 60;
                        Vector2 distance;
						distance = player.Center - NPC.Center;
						distance.X = distance.X / StateTimer;
                        distance.Y = distance.Y / StateTimer - 0.18f * StateTimer;
						NPC.ai[1] = distance.X;
                        NPC.ai[2] = distance.Y;
					}
					else if (Math.Abs(player.Center.X-NPC.Center.X) > 300)
					{
						AI_State = ActionState.Dashing;
						StateTimer = 80;
					}
					else
					{
						AI_State = ActionState.Clawing;
						StateTimer = 20;
						NPC.velocity.X = NPC.direction * 8f;
					}
					SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
					break;
				default:
					AI_State = ActionState.Chasing;
					StateTimer = 100;
					break;
			}
		}
		
		private void SpikeAttack(int i)
		{
			Point point = NPC.Bottom.ToTileCoordinates();
			point.X += NPC.direction * 3;
				
			int num = 13;
			int num2 = point.X + i * NPC.direction;
			int num3 = SpikeAttackFindBestY(ref point, num2);
			if (!WorldGen.ActiveAndWalkableTile(num2, num3))
			{
				return;
			}
			Vector2 position = new Vector2((float)(num2 * 16 + 8), (float)(num3 * 16 - 8));
			Vector2 velocity = new Vector2(0f, -1f).RotatedBy((double)((float)(i * NPC.direction) * 0.7f * (0.7853982f / 20f)), default(Vector2));
			Projectile.NewProjectile(NPC.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<SandSpike>(), num, 0f, Main.myPlayer, 0f, 0.1f + Main.rand.NextFloat() * 0.1f + (float)i * 1.1f / 20f, 0f);
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
            if (AI_State == ActionState.Dashing || AI_State == ActionState.Leaping)
            {
                Texture2D texture = TextureAssets.Npc[Type].Value;
                Vector2 drawOrigin = texture.Size() / 2f;
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width*0.5f, NPC.height*0.5f) + new Vector2(0f, NPC.gfxOffY+4f);
                    Color color = drawColor * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
					SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(texture, drawPos, null, color, 0f, drawOrigin, NPC.scale, effects, 0);
                }
            }
            return true;
        }
		
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.SandBlock, 1, 20, 30));
        }
    }
}
