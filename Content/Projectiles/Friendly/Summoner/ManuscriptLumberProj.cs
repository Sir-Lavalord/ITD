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
    public class ManuscriptLumberProj : ModProjectile
    {
        private enum ActionState
        {
            Spawn,
            Idle,
            TreeFound,
            Chopping
        }
        private ActionState AI_State;
        private Point treePos;
        private const int detectRadius = 20;
        private int ChopCD = 60;
        private int finderCD = 120;
        private float lastDir;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.height = 80;
            Projectile.width = 50;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
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
            if (Projectile.Distance(player.Center) > 1000)
            {
                treePos = Point.Zero;
                AI_State = ActionState.Idle;
                Projectile.Center = player.Center;
                Projectile.netUpdate = true;
            }
            switch (AI_State)
            {
                case ActionState.Spawn:
                    SpawnBehavior();
                    break;
                case ActionState.Idle:
                    Projectile.rotation = Projectile.velocity.X / 5;
                    Projectile.frame = 0;
                    if (finderCD-- <= 0)
                        treePos = FindTree(Projectile.Center.ToTileCoordinates(), detectRadius, Projectile);
                    IdleBehavior();
                    Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();
                    break;
                case ActionState.TreeFound:
                    Projectile.frame = 0;
                    TreeFoundBehavior();
                    Projectile.rotation = Projectile.velocity.X / 5;
                    Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();
                    break;
                case ActionState.Chopping:
                    ChoppingBehavior();
                    break;
            }
            CheckActive(player);
        }
        private void SpawnBehavior()//Not onspawn
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter > 10)
            {
                if (Projectile.frame < 5)
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
        Vector2 randomWander;
        int wanderTimer;
        private void IdleBehavior()//only overlap when no tree is found
        {
            if (treePos == Point.Zero)
            {
                Vector2 targetPoint = player.Center + new Vector2(lastDir * 128f, -64f);
                Vector2 toPlayer = targetPoint - Projectile.Center;
                Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
                float speed = toPlayer.Length();
                wanderTimer++;
                if (wanderTimer > 60)
                {
                    randomWander = Main.rand.NextVector2Circular(2f, 4f);
                    wanderTimer = 0;
                }
                Projectile.velocity = toPlayerNormalized * (speed / 8) + randomWander;
                float overlapVelocity = 0.1f;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];

                    if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                    {
                        if (Projectile.position.X < other.position.X)
                        {
                            Projectile.velocity.X -= overlapVelocity;
                        }
                        else
                        {
                            Projectile.velocity.X += overlapVelocity;
                        }

                        if (Projectile.position.Y < other.position.Y)
                        {
                            Projectile.velocity.Y -= overlapVelocity;
                        }
                        else
                        {
                            Projectile.velocity.Y += overlapVelocity;
                        }
                    }
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
                return;
            }
            else
            {
                Vector2 tree = new Point(treePos.X, treePos.Y).ToWorldCoordinates();
                Vector2 targetPoint = tree;
                Vector2 toPlayer = targetPoint - Projectile.Center;
                Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
                float speed = 8f;
                Projectile.velocity = toPlayerNormalized * (speed);


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
                        if (Main.tileAxe[t.TileType] && TileHelpers.SolidTile(i, j + 1))
                        {
                            AI_State = ActionState.Chopping;

                        }
                    }
                }
                if (Projectile.Distance(tree) > 400f)
                {
                    treePos = Point.Zero;
                }
            }
        }
        public void ChoppingBehavior()
        {
            Vector2 tree = new Point(treePos.X, treePos.Y).ToWorldCoordinates();
            Vector2 targetPoint = tree;
            Vector2 toPlayer = targetPoint - Projectile.Center;
            Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
            float speed = 4f;
            Projectile.velocity = toPlayerNormalized * (speed);

            bool treeExists = false;
            Point tileCoords = Projectile.position.ToTileCoordinates();
            Point tileSize = (Projectile.Size / 16).ToPoint();
            Rectangle rect = new Rectangle(tileCoords.X, tileCoords.Y, tileSize.X, tileSize.Y);
            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Top; j < rect.Bottom; j++)
                {
                    Tile t = Framing.GetTileSafely(i, j);

                    if (Main.tileAxe[t.TileType] && TileHelpers.SolidTile(i, j + 1))
                    {
                        treeExists = true;
                        if (++Projectile.frameCounter >= 8)
                        {
                            if (Projectile.frame < 5)
                            {
                                Projectile.frame++;
                            }
                            else
                            {
                                Projectile.frame = 0;
                            }
                            Projectile.frameCounter = 0;

                        }
                        if (FindAxe(player) != null)
                        {
                            if (ChopCD-- <= 0)
                            {
                                ChopCD = Math.Min(FindAxe(player).useTime + 5, 60);
                                player.PickTile(i, j, Math.Max(FindAxe(player).axe, 30));
                            }
                        }
                        else
                        {
                            if (ChopCD-- <= 0)
                            {
                                ChopCD = 60;
                                player.PickTile(i, j, 30);

                            }
                        }
                    }
                }
            }
            Tile t0 = Framing.GetTileSafely(tree);
            if (!t0.HasTile || !Main.tileAxe[t0.TileType])//no tree
            {
                AI_State = ActionState.Idle;
                treePos = Point.Zero;
                treeExists = false;
            }
            if (Projectile.Distance(tree) > 60f || !treeExists)//how?
            {
                AI_State = ActionState.Idle;
                treePos = Point.Zero;
            }
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
                    if (Main.tileAxe[t.TileType] && TileHelpers.SolidTile(i, j + 1))
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