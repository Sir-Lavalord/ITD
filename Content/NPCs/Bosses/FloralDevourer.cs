using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using System.Collections;
using Terraria.GameContent.Animations;
using Microsoft.Extensions.Logging.Abstractions;
using ITD.Utilities;
using Terraria.GameContent.Bestiary;

namespace ITD.Content.NPCs.Bosses
{
    [AutoloadBossHead]
    public class FloralDevourer : ModNPC
    {
        // todo: make front legs actually draw in front of the segments
        // (probably this class needs references to the front legs since i'm drawing the segments here)
        private static readonly Asset<Texture2D> outline = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerSegmentOutline");
        private static readonly Asset<Texture2D> segment = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerSegment");
        private int[] floralDevourerSegments;
        private float xSpeed = 5f;
        private int dipProgress = 0;
        private int dipProgLimit = 60;
        private int dipCooldown = 0;
        private float raycastFloatLength = 9f;
        private float defaultRaycastFloatLength = 9f;
        private bool dipping = false;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 140;
            NPC.damage = 8;
            NPC.defense = 5;
            NPC.lifeMax = 3000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
            DrawOffsetY = -10f;
            Main.npcFrameCount[NPC.type] = 1;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/WOMR");
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }

        public override void AI()
        {
            Vector2 raycastCheck = Helpers.QuickRaycast(NPC.Center, Vector2.UnitY, false, raycastFloatLength);
            float length = (raycastCheck - NPC.Center).Length();
            if (length < raycastFloatLength*15.8f)
            {
                NPC.velocity.Y = -2f;
            }

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Player player = Main.player[NPC.target];

            if (player.dead)
            {
                NPC.EncourageDespawn(10);
                return;
            }

            Vector2 toPlayer = player.Center - NPC.Center;
            Vector2 toPlayerNormalized = Vector2.Normalize(toPlayer);
            int playerDirectionX = NPC.Center.X < player.Center.X ? 1 : -1;

            NPC.velocity.Y += 0.2f;
            NPC.velocity.X = playerDirectionX * xSpeed;
            NPC.direction = NPC.spriteDirection = playerDirectionX;

            if (toPlayer.Length() < 185f && dipping == false && dipCooldown < 0)
            {
                dipping = true;
                dipCooldown = 200;
                dipProgress = 0;
            }
            dipCooldown--;
            if (dipping)
            {
                dipProgress++;
                raycastFloatLength = defaultRaycastFloatLength - (float)Math.Sin((dipProgress / (float)dipProgLimit)*Math.PI)*16f;
                if (dipProgress >= dipProgLimit)
                {
                    dipProgress = 0;
                    dipping = false;
                }
            }
            else
            {
                raycastFloatLength = defaultRaycastFloatLength;
            }
        }

        public override void PostAI()
        {
            for (int i = 0; i < floralDevourerSegments.Length; i++)
            {
                if (IsSegment(floralDevourerSegments[i], out NPC segment))
                {
                    Vector2 targetPosition;
                    if (i == 0)
                    {
                        targetPosition = NPC.Center - NPC.velocity;
                    }
                    else
                    {
                        targetPosition = Main.npc[floralDevourerSegments[i - 1]].Center;
                    }

                    segment.ai[1] = targetPosition.X;
                    segment.ai[2] = targetPosition.Y;
                }
            }
        }
        public static bool IsSegment(int whoAmI, out NPC npc)
        {
            NPC nPC = Main.npc[whoAmI];
            if (nPC.active && nPC.type == ModContent.NPCType<FloralDevourerSegment>())
            {
                npc = nPC;
                return true;
            }
            npc = null;
            return false;
        }
        public override void OnSpawn(IEntitySource source)
        {
            floralDevourerSegments = new int[6];
            for (int i = 0; i < 5; i++)
            {
                floralDevourerSegments[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<FloralDevourerSegment>(), NPC.whoAmI, i, NPC.Center.X, NPC.Center.Y, 1f);
            }
            floralDevourerSegments[5] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<FloralDevourerSegment>(), NPC.whoAmI, 5, NPC.Center.X, NPC.Center.Y, 0f);
        }

        public override void OnKill()
        {
            for (int i = 0; i < floralDevourerSegments.Length; i++)
            {
                if (IsSegment(floralDevourerSegments[i], out NPC npc))
                {
                    npc.StrikeInstantKill();
                }
            }
        }
        public void PreDrawSegments(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            for (int i = 0; i < floralDevourerSegments.Length; i++)
            {
                if (IsSegment(floralDevourerSegments[i], out NPC npc))
                {
                    Color color = Lighting.GetColor(npc.Center.ToTileCoordinates());
                    SpriteEffects direction = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(outline.Value, npc.Center - screenPos, null, color, 0f, outline.Size() / 2f, 1f, direction, 0f);
                }
            }
        }
        public void PostDrawSegments(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            for (int i = 0; i < floralDevourerSegments.Length; i++)
            {
                if (IsSegment(floralDevourerSegments[i], out NPC npc))
                {
                    Color color = Lighting.GetColor(npc.Center.ToTileCoordinates());
                    SpriteEffects direction = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(segment.Value, npc.Center - screenPos, null, color, 0f, segment.Size() / 2f, 1f, direction, 0f);
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                PreDrawSegments(spriteBatch, screenPos);
                PostDrawSegments(spriteBatch, screenPos);
            }
            return true;
        }
    }

    public class FloralDevourerSegment : ModNPC
    {
        public Vector2 frontLegPosition;
        public Vector2 backLegPosition;

        public Vector2 frontRayPosition;
        public Vector2 backRayPosition;

        public bool frontStepping = false;
        public bool backStepping = false;
        public Vector2 pathPosition
        {
            get { return new Vector2(NPC.ai[1], NPC.ai[2]); }
            set { NPC.ai[1] = value.X; NPC.ai[2] = value.Y; } 
        }
        public Vector2 direction;

        public bool HasLegs
        {
            get => NPC.ai[3] == 1f;
            set => NPC.ai[3] = value ? 1f : 0f;
        }
        private Leg legFront;
        private Leg legBack;
        public int ID
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }
        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 140;
            NPC.damage = 8;
            NPC.defense = 5;
            NPC.lifeMax = 3000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.aiStyle = -1;
            Main.npcFrameCount[NPC.type] = 1;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Content/Music/WOMR");
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (HasLegs)
            {
                legFront = new Leg(NPC.Center.X, NPC.Center.Y, 0f);
                legBack = new Leg(NPC.Center.X, NPC.Center.Y, 0f);
            }
        }
        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool CanBeHitByNPC(NPC attacker) => false;
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
        public override void AI()
        {
            NPC.Center = Vector2.Lerp(NPC.Center, pathPosition, 0.06f);

            NPC.position.Y += (float)Math.Sin(ID + Main.GameUpdateCount / 10f) * 2f;

            direction = (pathPosition - NPC.Center).SafeNormalize(Vector2.Zero);
            if (HasLegs)
            {
                frontRayPosition = Helpers.QuickRaycast(NPC.Center, Vector2.UnitY, false, 24f);
                backRayPosition = Helpers.QuickRaycast(NPC.Center + new Vector2(direction.X * 32f, 0f), Vector2.UnitY, false, 24f);
                float step = 80f;
                if (legFront != null && legBack != null)
                {
                    if ((frontRayPosition - frontLegPosition).Length() > step)
                    {
                        frontStepping = true;
                    }
                    if ((backRayPosition - backLegPosition).Length() > step)
                    {
                        backStepping = true;
                    }
                    if (frontStepping)
                    {
                        frontLegPosition = Vector2.Lerp(frontLegPosition, frontRayPosition, 0.6f);
                        if ((frontRayPosition - frontLegPosition).Length() < 12f)
                        {
                            frontStepping = false;
                        }
                    }
                    if (backStepping)
                    {
                        backLegPosition = Vector2.Lerp(backLegPosition, backRayPosition, 0.6f);
                        if ((backRayPosition - backLegPosition).Length() < 12f)
                        {
                            backStepping = false;
                        }
                    }
                    legFront.Update(frontLegPosition);
                    legBack.Update(backLegPosition);
                    legFront.legBase = NPC.Center;
                    legBack.legBase = NPC.Center + new Vector2(direction.X * 32f, 0f);
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool isFacingRight = direction.X > 0f;
            legBack?.Draw(spriteBatch, screenPos, Color.Gray, isFacingRight);
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool isFacingRight = direction.X > 0f;
            legFront?.Draw(spriteBatch, screenPos, Color.White, isFacingRight);
        }
    }
    public class Leg(float x, float y, float angle) : IDisposable
    {
        public Segment[] segments =
            [
                new Segment(x, y, angle, 97f, false),
                new Segment(x, y, angle, 81f, true),
            ];
        public Vector2 legBase = new Vector2(x, y);

        public void Update(Vector2 targetPos)
        {
            segments[0].Update();
            segments[0].Follow(targetPos);

            for (int i = 1; i < segments.Length; i++)
            {
                segments[i].Update();
                segments[i].FollowSegment(segments[i - 1]);
            }
            int last = segments.Length - 1;
            Segment s = segments[last];
            s.a = legBase;
            s.Update();

            for (int i = last - 1; i >= 0; i--)
            {
                Segment seg = segments[i];
                Segment next = segments[i + 1];
                seg.a = next.b;
                seg.Update();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color, bool shouldBeMirrored)
        {
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                segments[i].Draw(spriteBatch, screenPos, color, shouldBeMirrored);
            }
        }

        public void Dispose()
        {
            segments = null;
            GC.SuppressFinalize(this);
        }
    }

    public class Segment(float x, float y, float angle, float length, bool femur)
    {
        private static Asset<Texture2D> femurTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerFemur");
        private static Asset<Texture2D> tibiaTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerTibia");
        public Vector2 a { get; set; } = new Vector2(x, y);
        public Vector2 b { get; set; }
        public float Length { get; set; } = length;
        public float Angle { get; set; } = angle;

        public Vector2 Target { get; set; } = Main.MouseWorld;
        public bool isFemur { get; set; } = femur;

        public void FollowSegment(Segment child)
        {
            Follow(new Vector2(child.a.X, child.a.Y));
        }
        public void Follow(Vector2 target)
        {
            Vector2 dir = target - a;
            Angle = (float)Math.Atan2(dir.Y, dir.X);
            dir.Normalize();
            dir *= Length;
            dir *= -1f;
            a = target + dir;
        }

        public void Update()
        {
            float dx = Length * (float)Math.Cos(Angle);
            float dy = Length * (float)Math.Sin(Angle);
            b = new Vector2(a.X+dx, a.Y+dy);
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color, bool shouldBeMirrored)
        {
            Point tileCoords = new Vector2((a.X+b.X)/2f, (a.Y+b.Y)/2f).ToTileCoordinates();
            Texture2D textureToDraw = isFemur ? femurTexture.Value : tibiaTexture.Value;
            spriteBatch.Draw(textureToDraw, a-screenPos, null, Lighting.GetColor(tileCoords.X, tileCoords.Y).MultiplyRGB(color), Angle, new Vector2(0f, textureToDraw.Height / 2f), 1f, shouldBeMirrored? SpriteEffects.FlipVertically : SpriteEffects.None, default);
        }
    }
}
