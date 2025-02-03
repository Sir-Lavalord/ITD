using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace ITD.Content.NPCs.Events.LavaRain
{
    public class Magmaripper : ITDNPC
    {
        public enum ActionState
        {
            LavaSwim,
            AirTime,
            Flopping,
        }
        public ref float AITimer => ref NPC.ai[0];
        public ActionState AIState { get { return (ActionState)NPC.ai[1]; } set { NPC.ai[1] = (float)value; } }
        public float AIRand { get { return NPC.ai[2]; } set { NPC.ai[2] = value; NPC.netUpdate = true; } }
        public ref float AIDir => ref NPC.ai[3];
        public ref float AfterImageFadeIn => ref NPC.localAI[0];
        public override void SetStaticDefaultsSafe()
        {
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = TrailingModeID.NPCTrailing.PosRotEveryFrame;
            ITDSets.LavaRainEnemy[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.lifeMax = 200;
            NPC.damage = 30;
            NPC.defense = 20;
            NPC.width = NPC.height = 32;
            NPC.value = Item.buyPrice(silver: 50);
            NPC.HitSound = SoundID.NPCHit6;
            NPC.DeathSound = SoundID.NPCDeath18;
            NPC.lavaImmune = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            // scan the area for at least a lava block
            Point found = Point.Zero;
            Rectangle area = BigHitboxTiles;
            area.Inflate(16, 16);
            area.LoopThroughPoints(p =>
            {
                if (found != Point.Zero)
                    return;
                Tile t = Framing.GetTileSafely(p);
                if (t.LiquidAmount > 0 && t.LiquidType == LiquidID.Lava)
                    found = p;
            });
            if (found != Point.Zero)
            {
                NPC.Center = found.ToWorldCoordinates();
            }
            AIRand = 100;
            AIDir = 0;
        }
        public override void AI()
        {
            bool lava = Collision.LavaCollision(NPC.position, NPC.width, NPC.height);// NPC.lavaWet;
            AITimer++;
            if (InvalidTarget)
                NPC.TargetClosest(false);
            NPC.spriteDirection = NPC.direction = NPC.velocity.X > 0f ? 1 : -1;
            Player target = Main.player[NPC.target];
            Vector2 toTarget = target.Center - NPC.Center;
            Vector2 toTargetNormalized = toTarget.SafeNormalize(Vector2.Zero);
            if (AIState != ActionState.AirTime)
            {
                NPC.noTileCollide = false;
                if (AfterImageFadeIn > 0f)
                    AfterImageFadeIn -= 0.1f;
            }
            switch (AIState)
            {
                case ActionState.LavaSwim:
                    if (!lava && AIRand > 100)
                    {
                        AIState = ActionState.Flopping;
                        AIRand = 2f;
                        AITimer = 0;
                        NPC.noGravity = false;
                        break;
                    }
                    if (lava)
                        AIRand = 0;
                    AIRand++;
                    if (!NPC.noGravity)
                        NPC.noGravity = true;
                    int toDir = Math.Sign(toTargetNormalized.X);
                    if (AIDir == 0 || (AITimer > 300 && AIDir != toDir))
                    {
                        AITimer = 0;
                        AIDir = toDir;
                    }
                    if (AITimer < 30)
                        NPC.velocity.Y += Math.Abs(NPC.velocity.X) / 7f;
                    NPC.velocity.X = AIDir * 9f;
                    if (AITimer > 30)
                    {
                        RaycastData ray = Helpers.QuickRaycast(NPC.Center, new Vector2(AIDir, 0f), maxDistTiles: 16, visualize: true);
                        if (ray.Hit)
                        {
                            NPC.velocity.Y -= ray.LengthSQ == 0f ? 0f : (1f / ray.Length) * 16f;
                        }
                        if (!lava)
                        {
                            SoundEngine.PlaySound(NPC.HitSound, NPC.Center);
                            AIState = ActionState.AirTime;
                            AITimer = 0;
                            AIRand = 0;
                        }
                    }
                    NPC.rotation = (NPC.velocity * NPC.spriteDirection).ToRotation();
                    break;
                case ActionState.AirTime:
                    NPC.noTileCollide = NPC.velocity.Y < 0f && AITimer < 40;
                    if (NPC.collideY && NPC.velocity.Y > 0f)
                    {
                        AIState = ActionState.Flopping;
                        AITimer = 0;
                        NPC.noGravity = false;
                        break;
                    }
                    if (AITimer > 20)
                    {
                        if (AIRand < 2f)
                        {
                            // quickraycast won't work for this bc no liquid collision (which would be actually kinda easy to implement? but would you want that though)
                            float stickOut = 32f;
                            Point tileQuery = new Vector2(NPC.position.X + AIDir * stickOut, NPC.position.Y).ToTileCoordinates();
                            Vector2 lavaPos = Vector2.Zero;
                            for (int i = 0; i < 64; i++)
                            {
                                Tile t = Framing.GetTileSafely(tileQuery);
                                if (t.LiquidAmount > 0 && t.LiquidType == LiquidID.Lava)
                                {
                                    lavaPos = tileQuery.ToWorldCoordinates();
                                    break;
                                }
                                tileQuery.Y++;
                            }
                            if (lavaPos != Vector2.Zero)
                            {
                                NPC.velocity += (lavaPos - NPC.Center).SafeNormalize(Vector2.Zero) * NPC.velocity.Length();
                                AIRand = 2f;
                            }
                        }
                        NPC.velocity.Y += 0.2f;
                        if (lava)
                        {
                            AIState = ActionState.LavaSwim;
                            AITimer = 300;
                            break;
                        }
                    }
                    NPC.rotation = (NPC.velocity * NPC.spriteDirection).ToRotation();
                    if (AfterImageFadeIn < 1f)
                        AfterImageFadeIn += 0.1f;
                    break;
                case ActionState.Flopping:
                    if (lava)
                    {
                        AIState = ActionState.LavaSwim;
                        AITimer = 300;
                        break;
                    }
                    if (NPC.collideY && AITimer > 10)
                    {
                        NPC.velocity.X = MathHelper.SmoothStep(NPC.velocity.X, toTargetNormalized.X * 2f, 0.5f);
                        NPC.velocity.Y -= AIRand;
                        AIRand = Main.rand.NextFloat(3f, 5f);
                        AITimer = 0;
                    }
                    NPC.rotation = NPC.velocity.Y / 32f;
                    break;
            }
        }
        public override bool? CanFallThroughPlatforms() => true;
        public override void FindFrame(int frameHeight)
        {
            CommonFrameLoop(frameHeight);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Vector2 origin = new(tex.Width * 0.5f, tex.Height / 2f / Main.npcFrameCount[Type]);
            if (AfterImageFadeIn > 0f)
            {
                for (int i = NPC.oldPos.Length - 1; i > 0; i--)
                {
                    float progress = i / (float)NPC.oldPos.Length;
                    float progressOldestIsLeast = 1f - progress;
                    Vector2 drawPos = NPC.oldPos[i] + (NPC.Size * 0.5f) - screenPos;
                    spriteBatch.Draw(tex, drawPos, NPC.frame, drawColor * progressOldestIsLeast * AfterImageFadeIn, NPC.oldRot[i], origin, NPC.scale, CommonSpriteDirection, 0f);
                }
            }
            spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, CommonSpriteDirection, 0f);
            return false;
        }
    }
}
