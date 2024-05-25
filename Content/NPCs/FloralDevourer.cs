﻿using System;
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

namespace ITD.Content.NPCs
{
    [AutoloadBossHead]
    public class FloralDevourer : ModNPC
    {
        //private static List<Leg> legs = [];
        //private static List<Leg> backLegs = [];
        private List<FloralDevourerSegment> floralDevourerSegments = [];
        private float xSpeed = 5f;
        private int dipProgress = 0;
        private int dipProgLimit = 60;
        private int dipCooldown = 0;
        private float raycastFloatLength = 20f;
        private float defaultRaycastFloatLength = 20f;
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
            var raycastCheck = Helpers.RecursiveRaycast(NPC.Center, raycastFloatLength, 0f);
            if (raycastCheck != null)
            {
                NPC.velocity.Y = -2f;
                //NPC.velocity.Y -= 1f;
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
                //Main.NewText("dipping");
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
            if (floralDevourerSegments.Count > 0)
            {
                foreach (FloralDevourerSegment seg in floralDevourerSegments)
                {
                    seg.Enqueue(NPC.Center);
                    seg.Update();
                }
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 5; i++)
            {
                floralDevourerSegments.Add(new FloralDevourerSegment(NPC.position, floralDevourerSegments.Count+1));
            }
        }

        public override void OnKill()
        {
            floralDevourerSegments.Clear();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            foreach (FloralDevourerSegment seg in floralDevourerSegments)
            {
                seg.PreDraw(spriteBatch, screenPos);
            }
            foreach (FloralDevourerSegment seg in floralDevourerSegments)
            {
                seg.PostDraw(spriteBatch, screenPos);
            }
            foreach (FloralDevourerSegment seg in floralDevourerSegments)
            {
                seg.PostDrawLegs(spriteBatch, screenPos);
            }
            return true;
        }
    }

    public class FloralDevourerSegment
    {
        private Queue<Vector2> path = new();
        public Vector2 legTarget;
        public Vector2 backLegTarget;
        public Vector2 legTargetTarget;
        public Vector2 backLegTargetTarget;
        public Vector2 position;
        public Vector2 direction;
        private Leg legFront;
        private Leg legBack;
        private int delay = 20;
        private int timer = 0;
        public int ID = 0;
        private Asset<Texture2D> outline;
        private Asset<Texture2D> sprite;
        public FloralDevourerSegment(Vector2 pos, int id)
        {
            outline = ModContent.Request<Texture2D>("ITD/Content/NPCs/FloralDevourerSegmentOutline");
            sprite = ModContent.Request<Texture2D>("ITD/Content/NPCs/FloralDevourerSegment");
            position = pos;
            legFront = new Leg(position.X, position.Y, 0f);
            legBack = new Leg(position.X, position.Y, 0f);
            ID = id;
            delay *= ID;
        }

        public void Enqueue(Vector2 pos)
        {
            path.Enqueue(pos);
        }

        public void Update()
        {
            var raycastCheck = Helpers.RecursiveRaycast(position, 24f, 0f);
            if (raycastCheck != null)
            {
                legTargetTarget = (Vector2)raycastCheck;
            }
            var raycastCheckBack = Helpers.RecursiveRaycast(position + direction*8f, 24f, 0f);
            if (raycastCheckBack != null)
            {
                backLegTargetTarget = (Vector2)raycastCheckBack;
            }
            if (legFront != null && legBack != null)
            {
                legTarget = Vector2.Lerp(legTarget, legTargetTarget, 0.1f);
                backLegTarget = Vector2.Lerp(backLegTarget, backLegTargetTarget, 0.1f);
                legFront.Update(legTarget);
                legBack.Update(backLegTarget);
                legFront.legBase = position;
                legBack.legBase = position + direction*8f;
            }
            timer += 1;
            if (path.Count != 0 && timer > delay)
            {
                var prev = position;
                position = path.Peek();
                direction = position - prev;
                path.Dequeue();
            }
        }

        public void PostDrawLegs(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (legFront != null)
            {
                legFront.Draw(spriteBatch, screenPos, Color.White);
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            Point tileCoords = position.ToTileCoordinates();
            spriteBatch.Draw(sprite.Value, position - screenPos, null, Lighting.GetColor(tileCoords), 0f, sprite.Size() / 2f, 1f, SpriteEffects.None, default);
        }

        public void PreDraw(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (legBack != null)
            {
                legBack.Draw(spriteBatch, screenPos, Color.Gray);
            }
            Point tileCoords = position.ToTileCoordinates();
            spriteBatch.Draw(outline.Value, position - screenPos, null, Lighting.GetColor(tileCoords), 0f, outline.Size() / 2f, 1f, SpriteEffects.None, default);
        }
    }
    public class Leg
    {
        public Segment[] segments;
        public Vector2 legBase;
        public Leg(float x, float y, float angle)
        {
            legBase = new Vector2(x, y);
            segments =
            [
                new Segment(x, y, angle, 97f, false),
                new Segment(x, y, angle, 81f, true),
            ];
        }

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

        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color)
        {
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                segments[i].Draw(spriteBatch, screenPos, color);
            }
        }
    }

    public class Segment
    {
        private static Asset<Texture2D> femurTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/FloralDevourerFemur");
        private static Asset<Texture2D> tibiaTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/FloralDevourerTibia");
        public Vector2 a { get; set; }
        public Vector2 b { get; set; }
        public float Length {  get; set; }
        public float Angle {  get; set; }

        public Vector2 Target {  get; set; }
        public bool isFemur { get; set; } = true;
        public Segment(float x, float y, float angle, float length, bool femur)
        {
            isFemur = femur;
            Target = Main.MouseWorld;
            a = new Vector2(x, y);
            Angle = angle;
            Length = length;
        }

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
        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color)
        {
            Point tileCoords = new Vector2((a.X+b.X)/2f, (a.Y+b.Y)/2f).ToTileCoordinates();
            Texture2D textureToDraw = isFemur ? femurTexture.Value : tibiaTexture.Value;
            spriteBatch.Draw(textureToDraw, a-screenPos, null, Lighting.GetColor(tileCoords.X, tileCoords.Y), Angle, new Vector2(0f, textureToDraw.Height / 2f), 1f, SpriteEffects.None, default);
        }
    }
}
