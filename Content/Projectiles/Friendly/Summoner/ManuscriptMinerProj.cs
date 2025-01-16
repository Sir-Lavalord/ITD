using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using ITD.Physics;
using Terraria.DataStructures;
using ITD.Content.Buffs.PetBuffs;
using Terraria.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ITD.Utilities;
using ITD.Content.Items.Weapons.Summoner;
using ITD.Content.Projectiles.Friendly.Summoner.ManuscriptUI;
using ITD.Content.Buffs.MinionBuffs;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class ManuscriptMinerProj : ModProjectile
    {
        //reverted kys 
        private enum ActionState
        {
            Spawn,
            Idle,
            TreeFound,
            Chopping
        }
        private ActionState AI_State;
        private Point treePos;
        private const int detectRadius = 40;
        private int ChopCD = 30;
        private float JumpX = 0;
        private float JumpY = 0;
        private float lastDir;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 13;
            Main.projPet[Type] = true;
            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
                .WithOffset(-10, -20f)
                .WithSpriteDirection(-1)
                .WithCode(DelegateMethods.CharacterPreview.Float);
        }
        public override void SetDefaults()
        {
            Projectile.height = 80;
            Projectile.width = 80;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.timeLeft = 2;
            Projectile.minion = true;
        }
        public override void OnKill(int timeLeft)
        {
        }
        public Player player => Main.player[Projectile.owner];
        public void RegisterRightClick(Player player)
        {
            if (Main.mouseRight && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f)
            {
                /*                Projectile.Kill();
                */
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            RegisterRightClick(player);
            switch (AI_State)
            {
                case ActionState.Spawn:
                    SpawnBehavior();
                    break;
                case ActionState.Idle:
                    Projectile.frame = 5;
                    IdleBehavior();
                    break;
                case ActionState.TreeFound:
                    Projectile.frame = 5;
                    TreeFoundBehavior();
                    break;
                case ActionState.Chopping:
                    ChoppingBehavior();
                    break;
            }
            //always scan, sorry pc
            treePos = FindTree(Projectile.Center.ToTileCoordinates(), detectRadius, Projectile);

            if (Projectile.velocity.X > 0.25f)
                Projectile.spriteDirection = 1;
            else if (Projectile.velocity.X < -0.25f)
                Projectile.spriteDirection = -1;
            CheckActive(player);
            Projectile.velocity.Y += 0.6f;
        }
        private void SpawnBehavior()//Not onspawn
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter > 8)
            {
                if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
                {
                    Projectile.frame++;
                }
                else
                {
                    AI_State = ActionState.Idle;
                }
                Projectile.frameCounter = 0;
            }
        }
        private void IdleBehavior()
        {
            if (treePos == Point.Zero)
            {
                Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY, 1, false, 0);
                if (Projectile.velocity.Y == 0 && (HasObstacle() || (Projectile.Distance(player.Center) > 205f && Projectile.position.X == Projectile.oldPosition.X)))
                {
                    Projectile.velocity.Y = -10f;
                }
                if (Projectile.velocity.Y > -16f)
                {
                    Projectile.velocity.Y += 0.3f;
                }
                if (Math.Abs(player.Center.X - Projectile.Center.X + 40f * Projectile.minionPos) > 160f)
                {
                    Projectile.velocity.X += Main.rand.NextFloat(0.11f, 0.16f) * (player.Center.X - Projectile.Center.X + 40f * Projectile.minionPos > 0f).ToDirectionInt();
                    Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -13f, 13f);
                }
                else
                {
                    Projectile.velocity.X *= 0.95f;
                }
                if (Projectile.Distance(player.Center) > 1600f)
                {
                    Projectile.Center = player.Center;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                AI_State = ActionState.TreeFound;
            }
        }
        private void TreeFoundBehavior()
        {
            if (treePos == Point.Zero)
            {
                AI_State = ActionState.Idle;
            }
            else
            {
                Vector2 tree = new Point(treePos.X, treePos.Y).ToWorldCoordinates();
                Vector2 targetPoint = tree + new Vector2(lastDir * 64f, -64f);
                Vector2 toPlayer = targetPoint - Projectile.Center;
                Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
                float speed = 16f;
                Projectile.velocity.X = toPlayerNormalized.X * (speed);

                Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY, 1, false, 0);
                if (Projectile.velocity.Y == 0 && (HasObstacle() || (Projectile.Distance(tree) > 205f && Projectile.position.X == Projectile.oldPosition.X)))
                {
                    Projectile.velocity.Y = -10f;
                }
                if (Projectile.velocity.Y > -16f)
                {
                    Projectile.velocity.Y += 0.3f;
                }
                Point tileCoords = Projectile.position.ToTileCoordinates();
                Point tileSize = (Projectile.Size / 16).ToPoint();
                Rectangle rect = new Rectangle(tileCoords.X, tileCoords.Y, tileSize.X, tileSize.Y);
                for (int i = rect.Left; i < rect.Right; i++)
                {
                    for (int j = rect.Top; j < rect.Bottom; j++)
                    {
                        Tile t = Framing.GetTileSafely(i, j);
                        if (t == null)
                        {
                            Projectile.frame = 0;
                            continue;
                        }
                        if (TileID.Sets.IsATreeTrunk[t.TileType] && TileHelpers.SolidTile(i, j + 1))
                        {
                            AI_State = ActionState.Chopping;

                        }
                    }
                }
                if (Projectile.Distance(tree) > 600f)
                {
                    treePos = Point.Zero;
                }
            }
        }
        public void ChoppingBehavior()
        {
            Vector2 tree = new Point(treePos.X, treePos.Y).ToWorldCoordinates();
            Vector2 targetPoint = tree + new Vector2(lastDir * 64f, -64f);
            Vector2 toPlayer = targetPoint - Projectile.Center;
            Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
            float speed = 16f;
            Projectile.velocity.X = toPlayerNormalized.X * (speed);

            if (treePos != Point.Zero)
            {
                Point tileCoords = Projectile.position.ToTileCoordinates();
                Point tileSize = (Projectile.Size / 16).ToPoint();
                Rectangle rect = new Rectangle(tileCoords.X, tileCoords.Y, tileSize.X, tileSize.Y);
                for (int i = rect.Left; i < rect.Right; i++)
                {
                    for (int j = rect.Top; j < rect.Bottom; j++)
                    {
                        Tile t = Framing.GetTileSafely(i, j);
                        if (t == null)
                        {
                            Projectile.frame = 5;
                            continue;
                        }
                        if (TileID.Sets.IsATreeTrunk[t.TileType] && TileHelpers.SolidTile(i, j + 1))
                        {
                            if (++Projectile.frameCounter >= 8)
                            {
                                if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
                                {
                                    Projectile.frame++;
                                }
                                else
                                {
                                    Projectile.frame = 8;
                                }
                                Projectile.frameCounter = 0;

                            }
                            if (ChopCD-- <= 0)
                            {
                                if (FindAxe(player).useTime < 60)
                                {
                                    ChopCD = FindAxe(player).useTime;
                                }
                                else
                                {
                                    ChopCD = 60;
                                }
                                    if (FindAxe(player).axe > 30)
                                {
                                    player.PickTile(i, j, FindAxe(player).axe);
                                }
                                else
                                    player.PickTile(i, j, 30);
                            }
                        }
                    }
                }
                if (Projectile.Distance(tree) > 600f)//how?
                {
                    treePos = Point.Zero;
                }
            }
            else
            {
                AI_State = ActionState.Idle;
            }
        }

        public bool HasObstacle()
        {
            int tileWidth = 2;
            int tileX = (int)(Projectile.Center.X / 16f) - tileWidth;
            if (Projectile.velocity.X > 0)
            {
                tileX += tileWidth;
            }
            int tileY = (int)((Projectile.position.Y + Projectile.height) / 16f);
            for (int y = tileY; y < tileY + 2; y++)
            {
                for (int x = tileX; x < tileX + tileWidth; x++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        return false;
                    }
                }

            }
            return true;
        }
        public Item FindAxe(Player player)
        {
            Item item = null;
            for (int i = 0; i < 50; i++)
            {
                if (player.inventory[i].stack > 0 && player.inventory[i].axe > 0 && (item == null || player.inventory[i].axe > item.axe))
                    item = player.inventory[i];
            }
            return item;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = AI_State != ActionState.Spawn ? Projectile.Bottom.Y < Main.player[Projectile.owner].Top.Y - 120f : false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        private static Point FindTree(Point tile, int radius, Projectile projectile)
        {
            List<Point> detectedPoints = new List<Point>();

            Rectangle rect = new(tile.X - radius, tile.Y - radius, radius + radius, radius + radius);
            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Top; j < rect.Bottom; j++)
                {
                    Tile t = Framing.GetTileSafely(i, j);
                    if (t == null)
                    {
                        continue;
                    }
                    if (TileID.Sets.IsATreeTrunk[t.TileType] && TileHelpers.SolidTile(i, j + 1))
                    {
                        detectedPoints.Add(new Point(i, j));
                    }
                }
            }

            if (detectedPoints.Count == 0)
            {
                return Point.Zero;
            }

            Point closestPoint = detectedPoints
                            .OrderBy(p => Vector2.Distance(projectile.Center, p.ToWorldCoordinates()))
                            .FirstOrDefault();
            return closestPoint;
        }
        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<ManuscriptLumberBuff>());

                return false;
            }

            if (player.HasBuff(ModContent.BuffType<ManuscriptLumberBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
}