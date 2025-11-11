using ITD.Content.Projectiles.Hostile;
using System;
using System.IO;
using System.Linq;
using Terraria.GameContent;

namespace ITD.Content.NPCs.DeepDesert;

public class IncendipedeHead : ITDNPC
{
    public NPC FollowerNPC => Main.npc[(int)NPC.ai[0]];
    public ref float SineTimer => ref NPC.ai[1];
    public int WormLength { get { return (int)NPC.ai[2]; } set { NPC.ai[2] = value; } }
    public ref float AITimer => ref NPC.ai[3];
    public bool Wall;
    public const int SpacingBetween = 8;
    public override void FindFrame(int frameHeight) => CommonFrameLoop(frameHeight);
    public override void SetStaticDefaultsSafe()
    {
        Main.npcFrameCount[Type] = 3;
        // i'm using this for the actual segment following logic so consider this the Length limit for incendipedes
        NPCID.Sets.TrailCacheLength[Type] = 10 * SpacingBetween;
        // idc about getting rotations cuz we can just calculate those on the fly
        NPCID.Sets.TrailingMode[Type] = NPCTrailingID.PosRotEveryFrame;
        // bestiary stuff
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            CustomTexturePath = "ITD/Content/NPCs/DeepDesert/Incendipede_Bestiary",
            Position = new Vector2(40f, 24f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 12f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
    }
    public override void SetDefaults()
    {
        NPC.width = NPC.height = 16;
        NPC.damage = 90;
        NPC.defense = 85;
        NPC.lifeMax = 400;
        NPC.HitSound = SoundID.NPCHit31;
        NPC.DeathSound = SoundID.NPCDeath34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.friendly = false;
    }
    // basically copied from exampleworm
    private int SpawnSegment(int latest, int type, int id)
    {
        int oldLatest = latest;
        latest = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latest, id);

        Main.npc[oldLatest].ai[0] = latest;

        Main.npc[latest].realLife = NPC.whoAmI;

        return latest;
    }
    public void SpawnSegments(int wormLength)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            bool hasFollower = NPC.ai[0] > 0;
            if (!hasFollower)
            {
                NPC.realLife = NPC.whoAmI;
                int latestNPC = NPC.whoAmI;

                int distance = wormLength - 2;

                while (distance > 0)
                {
                    latestNPC = SpawnSegment(latestNPC, ModContent.NPCType<IncendipedeBodySegment>(), distance);
                    distance--;
                }
                SpawnSegment(latestNPC, ModContent.NPCType<IncendipedeTail>(), 0);

                NPC.netUpdate = true;

                int count = 0;
                foreach (var n in Main.ActiveNPCs)
                {
                    if ((n.type == Type || n.type == ModContent.NPCType<IncendipedeBodySegment>() || n.type == ModContent.NPCType<IncendipedeTail>()) && n.realLife == NPC.whoAmI)
                        count++;
                }
                if (count != wormLength)
                {
                    foreach (var n in Main.ActiveNPCs)
                    {
                        if ((n.type == Type || n.type == ModContent.NPCType<IncendipedeBodySegment>() || n.type == ModContent.NPCType<IncendipedeTail>()) && n.realLife == NPC.whoAmI)
                        {
                            n.active = false;
                            n.netUpdate = true;
                        }
                    }
                }
                NPC.realLife = -1;
                NPC.TargetClosest(true);
            }
        }
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Wall);
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Wall = reader.ReadBoolean();
    }
    public override void AI()
    {
        //Main.NewText($"WormLength: {WormLength}");
        // spawn segments here cuz this hook runs on the server
        if (WormLength == 0)
        {
            WormLength = Main.rand.Next(8, 11);
            SpawnSegments(WormLength);
        }
        // this bool returns whether or not the hitbox contains any tiles with a wall, in which case keep moving on the wall
        Wall = BigHitboxTiles.Points().Any(p => Framing.GetTileSafely(p).WallType != WallID.None);
        if (InvalidTarget)
            NPC.TargetClosest();
        if (Wall)
            WallMovement();
        else
            GroundMovement();
        Attacks(Main.player[NPC.target]);
    }
    public void WallMovement()
    {
        Player plr = Main.player[NPC.target];
        // sine...
        Vector2 toPlayer = plr.Center - NPC.Center;
        Vector2 toPlayerNorm = toPlayer.SafeNormalize(Vector2.Zero);

        // actual movement speed
        float speed = 2f;

        // wave properties
        float sineAmplitude = 32f;
        float sineFrequency = 0.1f;

        // so we can actually make the NPC move in a sine wave
        Vector2 perpendicular = toPlayerNorm.RotatedBy(Math.PI / 2d);

        float sineOffset = (float)Math.Sin(SineTimer * sineFrequency) * sineAmplitude;

        NPC.velocity = toPlayerNorm * speed + perpendicular * sineOffset / 16f;

        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

        SineTimer++;
    }
    public void GroundMovement()
    {
        // fall down
        NPC.velocity.Y += 0.1f;
        // move towards player
        Player plr = Main.player[NPC.target];

        int playerDirectionX = NPC.Center.X < plr.Center.X ? 1 : -1;
        float xSpeed = 3f;

        NPC.direction = NPC.spriteDirection = playerDirectionX;
        NPC.velocity.X = Math.Clamp(NPC.velocity.X + playerDirectionX, -xSpeed, xSpeed);
        NPC.rotation = 0f;

        StepUp();
    }
    public void Attacks(Player plr)
    {
        if (Collision.CanHitLine(NPC.Center, 1, 1, plr.Center, 1, 1))
        {
            AITimer++;
        }
        else
        {
            AITimer = 0;
        }
        if (AITimer > 120)
        {
            if (AITimer % 4 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 direction = NPC.velocity.SafeNormalize(Vector2.UnitX);
                    direction = direction.RotatedByRandom(MathHelper.ToRadians(10));
                    int projectile = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, direction * 8, ModContent.ProjectileType<IncendipedeBreath>(), 20, 0);
                    NPC.netUpdate = true;
                }
            }
            if (AITimer > 160)
                AITimer = 0;
        }
    }
    public override bool? CanFallThroughPlatforms() => true;
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        int frameX = Wall ? 0 : tex.Width / 2;
        Rectangle frame = new(frameX, NPC.frame.Y, tex.Width / 2, NPC.frame.Height);
        Vector2 origin = new(tex.Width / 4, tex.Height / Main.npcFrameCount[Type] / 2);
        Vector2 offset = new(!Wall ? -8f * NPC.spriteDirection : 0f, NPC.gfxOffY);
        Main.EntitySpriteDraw(tex, NPC.Center - screenPos + offset, frame, drawColor, NPC.rotation, origin, NPC.scale, CommonSpriteDirection);
        return false;
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.ITD().ZoneDeepDesert)
        {
            return 0.25f;
        }
        return 0f;
    }
}
public class IncendipedeBodySegment : ITDNPC
{
    public NPC FollowingNPC => Main.npc[(int)NPC.ai[1]];
    public NPC FollowerNPC => Main.npc[(int)NPC.ai[0]];
    public NPC HeadNPC => NPC.realLife > -1 ? Main.npc[NPC.realLife] : null;
    public int ID => (int)NPC.ai[2];
    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;
    public override void FindFrame(int frameHeight) => CommonFrameLoop(frameHeight, maxCounter: 2f);
    public override void SetStaticDefaultsSafe()
    {
        HiddenFromBestiary = true;
        Main.npcFrameCount[Type] = 8;
        NPCID.Sets.TrailCacheLength[Type] = 2;
        NPCID.Sets.TrailingMode[Type] = NPCTrailingID.PosEveryFrame;
    }
    public bool HasSyncedRealLife = false;
    public override void SetDefaults()
    {
        NPC.width = NPC.height = 16;
        NPC.lifeMax = 1;
        NPC.HitSound = SoundID.NPCHit31;
        NPC.DeathSound = SoundID.NPCDeath34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.friendly = false;
        NPC.noTileCollide = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        if (HasSyncedRealLife)
            return;
        writer.Write((byte)NPC.realLife);
        // set this flag on the server
        HasSyncedRealLife = true;
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        if (HasSyncedRealLife)
            return;
        NPC.realLife = reader.ReadByte();
        // set this flag on the client
        HasSyncedRealLife = true;
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life > 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.ai[3] = Main.rand.Next(1, 3);
            NPC.netUpdate = true;
            return;
        }
    }
    public override void AI()
    {
        if (NPC.ai[3] > 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float speed = 4f;
                Vector2 perp = (NPC.position - NPC.oldPos[1]).RotatedBy((NPC.ai[3] == 1 ? Math.PI : -Math.PI) / 2d).SafeNormalize(Vector2.Zero) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perp, ModContent.ProjectileType<IncendipedeFireSpike>(), 20, 0f);
            }
            NPC.ai[3] = 0;
        }

        if (HeadNPC.Exists() && HeadNPC.type == ModContent.NPCType<IncendipedeHead>())
        {
            int wormLength = (int)HeadNPC.ai[2];
            int spacingIndex = (wormLength - 1 - ID) * IncendipedeHead.SpacingBetween;

            Vector2 possiblePos = HeadNPC.oldPos[spacingIndex];
            if (possiblePos != Vector2.Zero)
            {
                NPC.position = HeadNPC.oldPos[spacingIndex];
                NPC.rotation = HeadNPC.oldRot[spacingIndex];
            }
        }
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (HeadNPC != null && HeadNPC.life <= 0)
            {
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }
        NPC.spriteDirection = (NPC.position - NPC.oldPos[1]).X > 0 ? 1 : -1;
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        bool Wall = Framing.GetTileSafely(NPC.Center.ToTileCoordinates()).WallType != WallID.None;
        int frameX = Wall ? 0 : tex.Width / 2;
        Rectangle frame = new(frameX, NPC.frame.Y, tex.Width / 2, NPC.frame.Height);
        Vector2 origin = new(tex.Width / 4, tex.Height / Main.npcFrameCount[Type] / 2);
        Vector2 offset = new(0, NPC.gfxOffY);
        Main.EntitySpriteDraw(tex, NPC.Center - screenPos + offset, frame, drawColor, NPC.rotation, origin, NPC.scale, CommonSpriteDirection);
        return false;
    }
}
public class IncendipedeTail : ITDNPC
{
    public NPC FollowingNPC => Main.npc[(int)NPC.ai[1]];
    public NPC HeadNPC => NPC.realLife > -1 ? Main.npc[NPC.realLife] : null;
    // no ID needed for this one cuz it's just 0 (newest position in NPC.oldPos array)
    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;
    public override void FindFrame(int frameHeight) => CommonFrameLoop(frameHeight);
    public override void SetStaticDefaultsSafe()
    {
        HiddenFromBestiary = true;
        Main.npcFrameCount[Type] = 3;
        NPCID.Sets.TrailCacheLength[Type] = 2;
        NPCID.Sets.TrailingMode[Type] = NPCTrailingID.PosEveryFrame;
    }
    public bool HasSyncedRealLife = false;
    public override void SetDefaults()
    {
        NPC.width = NPC.height = 16;
        NPC.lifeMax = 1;
        NPC.HitSound = SoundID.NPCHit31;
        NPC.DeathSound = SoundID.NPCDeath34;
        NPC.knockBackResist = 0f;
        NPC.value = Item.buyPrice(silver: 4);
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.friendly = false;
        NPC.noTileCollide = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        if (HasSyncedRealLife)
            return;
        writer.Write((byte)NPC.realLife);
        // set this flag on the server
        HasSyncedRealLife = true;
    }
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        if (HasSyncedRealLife)
            return;
        NPC.realLife = reader.ReadByte();
        // set this flag on the client
        HasSyncedRealLife = true;
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.life > 0)
        {
            NPC.ai[3] = 1;
            return;
        }
    }
    public override void AI()
    {
        if (NPC.ai[3] > 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float speed = 4f;
                Vector2 perp = (NPC.position - NPC.oldPos[1]).RotatedBy(Math.PI / 2d).SafeNormalize(Vector2.Zero) * speed;
                Vector2 behind = (NPC.oldPos[1] - NPC.position).SafeNormalize(Vector2.Zero) * speed;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perp, ModContent.ProjectileType<IncendipedeFireSpike>(), 20, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perp.RotatedBy(Math.PI), ModContent.ProjectileType<IncendipedeFireSpike>(), 20, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, behind, ModContent.ProjectileType<IncendipedeFireSpike>(), 20, 0f);
            }
            NPC.ai[3] = 0;
        }
        if (HeadNPC.Exists() && HeadNPC.type == ModContent.NPCType<IncendipedeHead>())
        {
            //Main.NewText("can i see a picture of this being worm");
            int wormLength = (int)HeadNPC.ai[2];
            int spacingIndex = (wormLength - 1) * IncendipedeHead.SpacingBetween;
            //Main.NewText($"ID: {0}, SPID: {spacingIndex}");

            Vector2 possiblePos = HeadNPC.oldPos[spacingIndex];
            if (possiblePos != Vector2.Zero)
            {
                NPC.position = HeadNPC.oldPos[spacingIndex];
                NPC.rotation = HeadNPC.oldRot[spacingIndex];
            }
        }
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (HeadNPC != null && HeadNPC.life <= 0)
            {
                NPC.HitEffect();
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }
        NPC.spriteDirection = (NPC.position - NPC.oldPos[1]).X > 0 ? 1 : -1;
        //Main.NewText("tea is leaf :)");
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        bool Wall = Framing.GetTileSafely(NPC.Center.ToTileCoordinates()).WallType != WallID.None;
        int frameX = Wall ? 0 : tex.Width / 2;
        Rectangle frame = new(frameX, NPC.frame.Y, tex.Width / 2, NPC.frame.Height);
        Vector2 origin = new(tex.Width / 4, tex.Height / Main.npcFrameCount[Type] / 2);
        Vector2 offset = new(0, NPC.gfxOffY);
        Main.EntitySpriteDraw(tex, NPC.Center - screenPos + offset, frame, drawColor, NPC.rotation, origin, NPC.scale, CommonSpriteDirection);
        return false;
    }
}
