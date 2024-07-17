using ITD.Content.Projectiles.Hostile;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.NPCs.BasicEnemies
{
    //wip
    public class Cicadian : ModNPC
    {
        private static float xSpeed = 2.2f;
        private enum ActionState
        {
            Background,
            Transition,
            Chasing,

        }
        private ActionState AI_State;
        private float transitionProgress;
        private Vector2 anchorPoint;
        private int boulderCooldown = 0;
        public int tree;
        public bool chopped = false;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
        }
        public override void SetDefaults()
        {
            AI_State = ActionState.Background;
            transitionProgress = 0f;
            NPC.width = 164;
            NPC.height = 64;
            NPC.damage = 90;
            NPC.defense = 85;
            NPC.lifeMax = 12000;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.aiStyle = -1;
            NPC.noTileCollide = true;
        }
        public override void AI()
        {
            Main.npc[tree].ai[1] = NPC.Center.X + NPC.direction * 2f;
            Main.npc[tree].ai[2] = NPC.Center.Y - 54f;
            if (!Main.npc[tree].active && !chopped)
            {
                chopped = true;
                NPC.defense -= 15;
                if (AI_State == ActionState.Background)
                    AI_State = ActionState.Transition;
            }
            switch (AI_State)
            {
                case ActionState.Background:
                    NPC.hide = true;
                    NPC.position = anchorPoint;
                    break;
                case ActionState.Transition:
                    transitionProgress += 0.02f;
                    SpawnDiggingDust();
                    NPC.position = anchorPoint - new Vector2(0f, transitionProgress*(NPC.height/1.5f));
                    if (transitionProgress >= 1f)
                        AI_State = ActionState.Chasing;
                    break;
                case ActionState.Chasing:
                    NPC.noTileCollide = false;
                    NPC.hide = false;
                    DoChase();
                    break;
                default:
                    break;
            }
        }
        private void LaunchIcyBoulder(Player player)
        {
            Vector2 toPlayer = player.Center - NPC.Center + player.velocity;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            int toPlayerDirection = Math.Sign(toPlayerNormalized.X);
            float gravity = IcyBoulder.IcyBoulderGravity;
            float distance = toPlayer.Length();
            float speed = 14f;

            float verticalDistance = Math.Abs(toPlayer.Y);

            float angle = (float)Math.Atan((Math.Pow(speed, 2) + Math.Sqrt(Math.Pow(speed, 4) - gravity * (gravity * Math.Pow(distance, 2) + 2 * verticalDistance * Math.Pow(speed, 2)))) / (gravity * distance));

            float velocityX = speed * (float)Math.Cos(angle) * toPlayerDirection;
            float velocityY = speed * (float)Math.Sin(angle);
            Vector2 velocity = new Vector2(velocityX, -velocityY);
            if (velocity.HasNaNs())
                velocity = (toPlayerNormalized * speed) + new Vector2(0f, -5f);

            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<IcyBoulder>(), 30, 0.2f);
        }
        private void DoChase()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];
            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            int playerDirectionX = NPC.Center.X < player.Center.X ? 1 : -1;
            NPC.direction = playerDirectionX;
            NPC.velocity.X = playerDirectionX * xSpeed;
            CheckForSideTiles();
            boulderCooldown--;
            if (boulderCooldown <= 0 && toPlayer.Length() > 400f && Math.Abs(toPlayer.Y) > 80f)
            {
                LaunchIcyBoulder(player);
                boulderCooldown = 120;
            }
        }
        private void CheckForSideTiles()
        {
            Tile right = Framing.GetTileSafely(new Vector2(NPC.position.X+NPC.width+0.1f, NPC.position.Y+NPC.height-0.1f));
            Tile rightUp = Framing.GetTileSafely(new Vector2(NPC.position.X + NPC.width + 0.1f, NPC.position.Y + NPC.height - 16.1f));
            Tile left = Framing.GetTileSafely(new Vector2(NPC.position.X - 0.1f, NPC.position.Y + NPC.height - 0.1f));
            Tile leftUp = Framing.GetTileSafely(new Vector2(NPC.position.X - 0.1f, NPC.position.Y + NPC.height - 16.1f));
            static bool CanCollide(Tile tile)
            {
                return tile.HasTile && Main.tileSolid[tile.TileType] && !tile.IsActuated;
            }
            bool rightIsClimbableSlope = right.Slope == SlopeType.SlopeDownRight;
            bool leftIsClimbableSlope = left.Slope == SlopeType.SlopeDownLeft;
            if (CanCollide(right) && !CanCollide(rightUp) && NPC.velocity.X > 0f && !rightIsClimbableSlope)
            {
                float tp = 16f;
                if (right.IsHalfBlock)
                {
                    tp = 8f;
                }
                NPC.position.Y -= tp;
            }
            if (CanCollide(left) && !CanCollide(leftUp) && NPC.velocity.X < 0f && !leftIsClimbableSlope)
            {
                float tp = 16f;
                if (left.IsHalfBlock)
                {
                    tp = 8f;
                }
                NPC.position.Y -= tp;
            }
        }
        private void SpawnDiggingDust()
        {
            int amount = 6;
            for (int i = 0; i < amount; i++)
            {
                if (Main.rand.NextBool(12))
                {
                    Vector2 spawnPosition = (Vector2)Helpers.QuickRaycast(NPC.position, Vector2.UnitY, 8f) + new Vector2(NPC.width / amount * i, 0f);
                    Gore.NewGoreDirect(NPC.GetSource_FromThis(), spawnPosition, Vector2.Zero, Main.rand.Next(61, 64));
                }
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            anchorPoint = NPC.position + new Vector2(0f, NPC.height / 2f);
            tree = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<CicadianTree>(), ai0: 0f, ai1: NPC.Center.X, ai2: NPC.Center.Y);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (AI_State == ActionState.Background)
                AI_State = ActionState.Transition;
            if (NPC.life <= 0)
            {
                Main.npc[tree].ai[0] = 1f;
            }
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (AI_State == ActionState.Chasing)
                return true;
            return false;
        }
        public override void DrawBehind(int index)
        {
            if (transitionProgress < 1f)
            {
                Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            }
            else
            {
                NPC.hide = false;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (AI_State == ActionState.Chasing || AI_State == ActionState.Transition)
            {
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > 5f)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > frameHeight * Main.npcFrameCount[Type] - 2)
                    {
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            if (AI_State == ActionState.Background)
            {
                NPC.frame.Y = 540;
            }
        }
        public override Color? GetAlpha(Color drawColor)
        {
            //float clamped = Helpers.Remap(transitionProgress, 0f, 1f, 0.5f, 1f);
            //return drawColor.MultiplyRGB(new Color(clamped, clamped, clamped, 1f));
            return drawColor;
        }
    }
}
