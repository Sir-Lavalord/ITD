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

namespace ITD.Content.NPCs
{
    [AutoloadBossHead]
    public class FloralDevourer : ModNPC
    {
        public static List<Segment> segments = new List<Segment>();
        public static Asset < Texture2D > femurTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/FloralDevourerFemur");
        public static Asset<Texture2D> tibiaTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/FloralDevourerTibia");
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 180;
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
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }

        public Vector2? RecursiveRaycast(Vector2 startWorldPos, float approxLengthTiles, float currentLengthTiles)
        {
            //Vector2 possibleTilePosition = new Vector2((int)(startWorldPos.X / 16), (int)(startWorldPos.Y / 16));
            if (!(currentLengthTiles > approxLengthTiles))
            {
                currentLengthTiles++;
                if (Collision.SolidCollision(startWorldPos, 1, 1))
                {
                    return startWorldPos;
                }
                else
                {
                    return RecursiveRaycast(startWorldPos + new Vector2(0f, 16f), approxLengthTiles, currentLengthTiles);
                }
            }
            return null;
        }

        public override void AI()
        {
            foreach (Segment segment in segments)
            {
                segment.Update();
                //Main.NewText(segment.a.ToString());
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
            NPC.velocity.X = playerDirectionX * 2f;
            NPC.direction = NPC.spriteDirection = playerDirectionX;
            var raycastCheck = RecursiveRaycast(NPC.Center, 10f, 0f);
            if (raycastCheck != null)
            {
                NPC.velocity.Y = -1f;
                Dust.NewDustPerfect((Vector2)raycastCheck, DustID.BlueTorch);
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            segments.Add(new Segment(NPC.position.X, NPC.position.Y, 0f, 97f, false, null));
            segments.Add(new Segment(NPC.position.X, NPC.position.Y, 0f, 81f, true, segments[0]));
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            foreach(Segment segment in segments)
            {
                segment.Draw(spriteBatch, segment.isFemur? femurTexture.Value: tibiaTexture.Value, screenPos);
            }
        }
    }

    public class FloralDevourerSegment : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 180;
            NPC.damage = 8;
            NPC.defense = 5;
            NPC.lifeMax = 3000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }

    }
    public class Leg
    {
        public Segment[] segments = new Segment[2];
    }

    public class Segment
    {
        public Vector2 a { get; set; }
        public Vector2 b { get; set; }
        public float Length {  get; set; }
        public float Angle {  get; set; }

        public Vector2 Target {  get; set; }

        public Segment Parent {  get; set; }
        public bool isFemur { get; set; } = true;
        public Segment(float x, float y, float angle, float length, bool femur, Segment? parent)
        {
            isFemur = femur;
            Parent = parent;
            Target = Main.MouseWorld;
            a = new Vector2(x, y);
            Angle = angle;
            Length = length;
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
            Follow(Parent == null? Main.MouseWorld: Parent.a);
            //Dust.NewDustPerfect(a, DustID.RedTorch);
            //Dust.NewDustPerfect(b, DustID.BlueTorch);
        }
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 screenPos)
        {
            Point tileCoords = new Vector2((a.X+b.X)/2f, (a.Y+b.Y)/2f).ToTileCoordinates();
            spriteBatch.Draw(texture, a-screenPos, null, Lighting.GetColor(tileCoords.X, tileCoords.Y), Angle, new Vector2(0f, texture.Height / 2f), 1f, SpriteEffects.None, default);
        }
    }
}
