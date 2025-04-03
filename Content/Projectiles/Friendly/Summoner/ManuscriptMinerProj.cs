using System;
using System.Collections.Generic;
using System.Linq;
using ITD.Utilities;
using ITD.Content.Items.Weapons.Summoner;
using ITD.Content.Buffs.MinionBuffs;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class ManuscriptMinerProj : ModProjectile
    {
        private enum ActionState
        {
            Spawn,
            Idle,
            OreFound,
            Digging
        }
        private ActionState AI_State { get { return (ActionState)Projectile.ai[0]; } set { Projectile.ai[0] = (float)value; } }
        private Point orePos;
        private const int detectRadius = 14;
        private int ChopCD = 60;
        private int finderCD = 120;
        private float lastDir;
        public bool hasLeftover;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
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
            if (Projectile.Distance(player.Center) > 1000 || !HasOre(player, out _))
            {
                orePos = Point.Zero;
                AI_State = ActionState.Idle;
                Projectile.Center = player.Center;
                Projectile.netUpdate = true;
            }
            if (!HasOre(player, out _))
            {
                orePos = Point.Zero;
                AI_State = ActionState.Idle;
            }
            switch (AI_State)
            {
                case ActionState.Spawn:
                    SpawnBehavior();
                    break;
                case ActionState.Idle:
                    if (finderCD-- <= 0)
                    {
                        finderCD = 0;
                        if (HasOre(player, out _))
                            orePos = FindOre(Projectile.Center.ToTileCoordinates(), detectRadius, Projectile, player, false);
                    }
                    Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();
                    hasLeftover = false;
                    ChopCD = 0;
                    Projectile.frame = 0;
                    IdleBehavior();

                    Projectile.rotation = Projectile.velocity.X / 5;
                    break;
                case ActionState.OreFound:
                    finderCD = 0;
                    Projectile.frame = 0;
                    OreFoundBehavior();
                    Projectile.rotation = Projectile.velocity.X / 5;
                    Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();
                    break;
                case ActionState.Digging:
                    DiggingBehavior();
                    break;
            }
            
            CheckActive(player);
        }
        private void SpawnBehavior()
        {
            Projectile.frameCounter++;

            if (Projectile.frameCounter > 10)
            {
                if (Projectile.frame < 7)
                {
                    Projectile.frame++;
                }
                else
                {
                    AI_State = ActionState.Idle;
                    Projectile.netUpdate = true;
                }
                Projectile.frameCounter = 0;
            }
        }
        Vector2 randomWander;
        int wanderTimer;
        private void IdleBehavior()
        {
            if (orePos == Point.Zero)
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
                AI_State = ActionState.OreFound;
                Projectile.netUpdate = true;
            }
        }
        private void OreFoundBehavior()
        {
            if (orePos == Point.Zero)
            {
                AI_State = ActionState.Idle;
                Projectile.netUpdate = true;
                return;
            }
            else
            {
                Vector2 ore = new Point(orePos.X, orePos.Y).ToWorldCoordinates();
                Vector2 targetPoint = ore;
                Vector2 toPlayer = targetPoint - Projectile.Center;
                Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
                float speed = 8f;
                Projectile.velocity = toPlayerNormalized * (speed);

                Rectangle rect = Projectile.TileRectangle();

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
                        if (TileID.Sets.Ore[t.TileType] && TileHelpers.SolidTile(i, j) && t.HasTile)
                        {
                            AI_State = ActionState.Digging;
                            Projectile.netUpdate = true;

                        }
                    }
                }
                if (Projectile.Distance(ore) > 400f)
                {
                    orePos = Point.Zero;
                    Projectile.netUpdate = true;
                }
            }
        }
        public void DiggingBehavior()
        {
            Vector2 ore = new Point(orePos.X, orePos.Y).ToWorldCoordinates();
            Vector2 targetPoint = ore;
            Vector2 toPlayer = targetPoint - Projectile.Center;
            Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
            float speed = 4f;
            Projectile.velocity = toPlayerNormalized * (speed);


            if (orePos != Point.Zero)
            {
                bool oreExists = false;
                Rectangle rect = Projectile.TileRectangle();

                for (int i = rect.Left; i < rect.Right; i++)
                {
                    for (int j = rect.Top; j < rect.Bottom; j++)
                    {
                        Tile t = Framing.GetTileSafely(i, j);
                        if (t.HasTile && TileID.Sets.Ore[t.TileType])
                        {
                            oreExists = true;
                            Main.NewText((t.HasTile, TileHelpers.SolidTile(i, j)));
                            if (++Projectile.frameCounter >= 8)
                            {
                                if (Projectile.frame <= 6)
                                {
                                    Projectile.frame++;
                                }
                                else
                                {
                                    Projectile.frame = 1;
                                }
                                Projectile.frameCounter = 0;

                            }
                            Item pick = player.GetBestPickaxe();
                            if (pick != null)
                            {
                                if (ChopCD-- <= 0)
                                {
                                    ChopCD = Math.Min(pick.useTime + 10, 60);
                                    player.PickTile(i, j, Math.Max(pick.pick, 35));
                                }

                            }
                            else
                            {
                                if (ChopCD-- <= 0)
                                {
                                    ChopCD = 90;
                                    player.PickTile(i, j, 30);

                                }
                            }
                        }
                        else
                        {
                            Projectile.frame = 0;
                        }
                    }
                }
                if (!oreExists)
                {
                    FindOre(Projectile.Center.ToTileCoordinates(), 6, Projectile, player, true);
                    if (!hasLeftover)
                    {
                        orePos = Point.Zero;
                        Projectile.netUpdate = true;
                    }
                }
                else if (hasLeftover)
                {
                    orePos = FindOre(Projectile.Center.ToTileCoordinates(), 6, Projectile, player, false);
                    Projectile.netUpdate = true;
                }
                if (Projectile.Distance(ore) > 80f || !HasOre(player, out _))//how?
                {
                    orePos = Point.Zero;
                    Projectile.netUpdate = true;
                    return;
                }
            }
            else
            {
                finderCD = 120;
                AI_State = ActionState.Idle;
                Projectile.netUpdate = true;
            }
        }
        public static bool HasOre(Player player, out int ore)
        {
            ore = player.inventory[9].createTile;
            return ore >= 0 && TileID.Sets.Ore[ore];
        }

        private Point FindOre(Point tile, int radius, Projectile projectile, Player player, bool findLeftover)
        {
            if (!HasOre(player, out int ore))
                return Point.Zero;

            List<Point> detectedOres = [];

            Rectangle rect = new(tile.X - radius, tile.Y - radius, radius * 2, radius * 2);

            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Top; j < rect.Bottom; j++)
                {
                    Tile t = Framing.GetTileSafely(i, j);
                    if (!t.HasTile || t.TileType != ore)
                    {
                        continue;
                    }
                    detectedOres.Add(new Point(i, j));
                }
            }
            if (findLeftover)
            {
                if (detectedOres.Count <= 0)
                    hasLeftover = false;
                else
                    hasLeftover = true;
            }
            if (detectedOres.Count <= 0)
            {
                return Point.Zero;
            }

            return detectedOres
                .OrderBy(p => Vector2.Distance(projectile.Center, p.ToWorldCoordinates()))
                .FirstOrDefault();
        }
        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<ManuscriptMinerBuff>());

                return false;
            }

            if (player.HasBuff(ModContent.BuffType<ManuscriptMinerBuff>()))
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