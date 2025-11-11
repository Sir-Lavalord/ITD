using ITD.Systems;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.NPCs.Events.LavaRain;

public class Magmaripper : ITDNPC
{
    public enum ActionState
    {
        LavaSwim,
        AirTime,
        Flopping,
        SwimToPlayer,
    }
    public ref float AITimer => ref NPC.ai[0];
    public ActionState AIState { get { return (ActionState)NPC.ai[1]; } set { NPC.ai[1] = (float)value; } }
    public float AIRand
    {
        get
        {
            return NPC.ai[2];
        }
        set
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            NPC.ai[2] = value;
            NPC.netUpdate = true;
        }
    }
    public ref float AIDir => ref NPC.ai[3];
    public ref float AfterImageFadeIn => ref NPC.localAI[0];
    public override void SetStaticDefaultsSafe()
    {
        Main.npcFrameCount[Type] = 5;
        NPCID.Sets.TrailCacheLength[Type] = 8;
        NPCID.Sets.TrailingMode[Type] = NPCTrailingID.PosRotEveryFrame;
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
        NPC.behindTiles = true;
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
    private static readonly float grav = 0.2f;
    public override void AI()
    {
        AITimer++;

        if (InvalidTarget)
            NPC.TargetClosest(false);

        bool lava = Collision.LavaCollision(NPC.position, NPC.width, NPC.height);
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

        AIState = AIState switch
        {
            ActionState.LavaSwim => LavaSwim(target, toTargetNormalized, lava),
            ActionState.AirTime => AirTime(lava),
            ActionState.Flopping => Flopping(toTargetNormalized, lava),
            ActionState.SwimToPlayer => SwimToPlayer(toTargetNormalized, lava),
            _ => AIState
        };
    }
    private ActionState LavaSwim(Player target, Vector2 toTargetNormalized, bool lava)
    {
        if (!lava && AIRand > 100)
        {
            AIRand = 2f;
            AITimer = 0;
            NPC.noGravity = false;
            return ActionState.Flopping;
        }
        if (lava)
            AIRand = 0;
        if (target.lavaWet)
        {
            return ActionState.SwimToPlayer;
        }
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
            NPC.velocity.Y -= 0.3f;
            if (!lava)
            {
                float stickOut = 16f * 38f;
                Point tileQuery = new Vector2(NPC.position.X + AIDir * stickOut, NPC.position.Y).ToTileCoordinates();
                Point finalQueryPos = Point.Zero;
                Vector2 lavaPos = Vector2.Zero;

                for (int j = 0; j < 32; j++)
                {
                    Point realQuery = tileQuery + new Point(0, j);
                    Tile t = Framing.GetTileSafely(realQuery);
                    int amt = 2;
                    Rectangle checkClear = new(realQuery.X, realQuery.Y - amt, amt, amt);
                    if (TileHelpers.TileLiquid(realQuery, LiquidID.Lava) && TileHelpers.AreaClear(checkClear))
                    {
                        lavaPos = realQuery.ToWorldCoordinates();
                        finalQueryPos = realQuery;
                        break;
                    }
                    Dust.NewDustPerfect(realQuery.ToWorldCoordinates(), DustID.WhiteTorch);
                }
                if (lavaPos != Vector2.Zero)
                {
                    Vector2 lavaPoolCenter = MiscHelpers.ComputeLiquidPool(finalQueryPos, LiquidID.Lava).CenterAverage;
                    if (lavaPoolCenter != Vector2.Zero)
                    {
                        float jumpHeight = 130f;
                        float maxJumpHeight = 160f;
                        NPC.velocity = MiscHelpers.GetArcVel(NPC.Center, lavaPoolCenter, grav, jumpHeight, maxJumpHeight, 13f);
                        SoundEngine.PlaySound(NPC.HitSound, NPC.Center);
                        AITimer = 0;
                        AIRand = 0;
                        return ActionState.AirTime;
                    }
                    else
                    {
                        AITimer = 0;
                        AIDir *= -1f;
                    }
                }
                else
                {
                    AITimer = 0;
                    AIDir *= -1f;
                }
            }
        }
        NPC.rotation = (NPC.velocity * NPC.spriteDirection).ToRotation();
        return ActionState.LavaSwim;
    }
    private ActionState AirTime(bool lava)
    {
        NPC.noTileCollide = true;
        if (NPC.collideY && NPC.velocity.Y > 0f)
        {
            AITimer = 0;
            NPC.noGravity = false;
            return ActionState.Flopping;
        }
        if (AITimer > -1)
        {
            NPC.velocity.Y += grav;
            if (lava)
            {
                AITimer = 300;
                return ActionState.LavaSwim;
            }
        }
        NPC.rotation = (NPC.velocity * NPC.spriteDirection).ToRotation();
        if (AfterImageFadeIn < 1f)
            AfterImageFadeIn += 0.1f;
        return ActionState.AirTime;
    }
    private ActionState Flopping(Vector2 toTargetNormalized, bool lava)
    {
        if (lava)
        {
            AITimer = 300;
            return ActionState.LavaSwim;
        }
        if (NPC.collideY && AITimer > 10)
        {
            NPC.velocity.X = MathHelper.SmoothStep(NPC.velocity.X, toTargetNormalized.X * 2f, 0.5f);
            NPC.velocity.Y -= AIRand;
            AIRand = Main.rand.NextFloat(3f, 5f);
            AITimer = 0;
        }
        NPC.rotation = NPC.velocity.Y / 32f;
        return ActionState.Flopping;
    }
    private ActionState SwimToPlayer(Vector2 toTargetNormalized, bool lava)
    {
        if (!lava)
            return ActionState.LavaSwim;
        if (!NPC.noGravity)
            NPC.noGravity = true;
        float swimSpeed = 8f;
        NPC.velocity = toTargetNormalized * swimSpeed;
        NPC.rotation = (NPC.velocity * NPC.spriteDirection).ToRotation();
        return ActionState.SwimToPlayer;
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
