using System;
using Terraria.DataStructures;
using ITD.Kinematics;
using ITD.Utilities;

namespace ITD.Content.NPCs.Bosses
{
    [AutoloadBossHead]
    public class FloralDevourer : ModNPC
    {
        // todo: make front legs actually draw in front of the segments
        // (probably this class needs references to the front legs since i'm drawing the segments here)
        private static readonly Asset<Texture2D> outline = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerSegmentOutline");
        private static readonly Asset<Texture2D> segment = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerSegment");
        public NPC FollowerNPC => Main.npc[(int)NPC.ai[0]];
        private static ushort segmentType;
        private float xSpeed = 5f;
        private int dipProgress = 0;
        private int dipProgLimit = 60;
        private int dipCooldown = 0;
        private float raycastFloatLength = 9f;
        private float defaultRaycastFloatLength = 9f;
        private bool dipping = false;
        public override void SetStaticDefaults()
        {
            segmentType = (ushort)ModContent.NPCType<FloralDevourerSegment>();
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
                Music = ITD.Instance.GetMusic("WOMR") ?? MusicID.Plantera;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return true;
        }

        public override void AI()
        {
            var ray = Helpers.QuickRaycast(NPC.Center, Vector2.UnitY, maxDistTiles: raycastFloatLength);
            if (ray.Hit)
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
        private int SpawnSegment(int latest, int id, bool legs)
        {
            int oldLatest = latest;
            latest = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, segmentType, NPC.whoAmI, ai0: 0, ai1: latest, ai2: id, ai3: legs ? 1 : 0);

            Main.npc[oldLatest].ai[0] = latest;

            Main.npc[latest].realLife = NPC.whoAmI;

            return latest;
        }
        private void SpawnSegments()
        {
            int wormLength = 6;
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
                        latestNPC = SpawnSegment(latestNPC, distance, true);
                        distance--;
                    }
                    SpawnSegment(latestNPC, 0, false);

                    NPC.netUpdate = true;

                    int count = 0;
                    foreach (var n in Main.ActiveNPCs)
                    {
                        if ((n.type == Type || n.type == segmentType) && n.realLife == NPC.whoAmI)
                            count++;
                    }
                    if (count != wormLength)
                    {
                        foreach (var n in Main.ActiveNPCs)
                        {
                            if ((n.type == Type || n.type == segmentType) && n.realLife == NPC.whoAmI)
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
        public override void OnSpawn(IEntitySource source)
        {
            SpawnSegments();
        }
        public void PreDrawSegments(SpriteBatch spriteBatch, Vector2 screenPos)
        { 
            if (FollowerNPC.ModNPC is FloralDevourerSegment seg)
            {
                FloralDevourerSegment cur = seg;
                while (cur != null)
                {
                    Color color = Lighting.GetColor(cur.NPC.Center.ToTileCoordinates());
                    SpriteEffects direction = cur.NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(outline.Value, cur.NPC.Center - screenPos, null, color, 0f, outline.Size() / 2f, 1f, direction, 0f);
                    cur = cur.FollowerNPC.ModNPC as FloralDevourerSegment;
                }
            }
        }
        public void PostDrawSegments(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (FollowerNPC.ModNPC is FloralDevourerSegment seg)
            {
                FloralDevourerSegment cur = seg;
                while (cur != null)
                {
                    Color color = Lighting.GetColor(cur.NPC.Center.ToTileCoordinates());
                    SpriteEffects direction = cur.NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(segment.Value, cur.NPC.Center - screenPos, null, color, 0f, outline.Size() / 2f, 1f, direction, 0f);
                    cur = cur.FollowerNPC.ModNPC as FloralDevourerSegment;
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                PreDrawSegments(spriteBatch, screenPos);
                PostDrawSegments(spriteBatch, screenPos);
                if (FollowerNPC.ModNPC is FloralDevourerSegment seg)
                {
                    FloralDevourerSegment cur = seg;
                    while (cur != null)
                    {
                        bool isFacingRight = cur.direction.X > 0f;
                        Texture2D femur = FloralDevourerSegment.femurTexture.Value;
                        Texture2D tibia = FloralDevourerSegment.tibiaTexture.Value;
                        cur.legFront?.Draw(spriteBatch, screenPos, Color.White, isFacingRight, femur, tibia, femur);
                        cur = cur.FollowerNPC.ModNPC as FloralDevourerSegment;
                    }
                }
            }
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return;
        }
    }

    public class FloralDevourerSegment : ModNPC
    {
        public static readonly Asset<Texture2D> femurTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerFemur");
        public static readonly Asset<Texture2D> tibiaTexture = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/FloralDevourerTibia");

        public Vector2 frontLegPosition;
        public Vector2 backLegPosition;

        public Vector2 frontRayPosition;
        public Vector2 backRayPosition;

        public bool frontStepping = false;
        public bool backStepping = false;
        public NPC FollowerNPC => Main.npc[(int)NPC.ai[0]];
        public NPC FollowingNPC => Main.npc[(int)NPC.ai[1]];
        public NPC HeadNPC => NPC.realLife > -1 ? Main.npc[NPC.realLife] : null;
        public Vector2 direction;

        public bool HasLegs
        {
            get => NPC.ai[3] == 1f;
            set => NPC.ai[3] = value ? 1f : 0f;
        }
        public KineChain legFront;
        private KineChain legBack;
        public int ID
        {
            get => (int)NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new()
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
        }
        public override bool? CanBeHitByItem(Player player, Item item) => false;
        public override bool CanBeHitByNPC(NPC attacker) => false;
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
        public override void AI()
        {
            if (!Main.dedServ)
            {
                if (HasLegs && legBack is null)
                {
                    KineLimb[] leg =
                    [
                        new KineLimb(81f),
                        new KineLimb(97f),
                    ];
                    legFront = new KineChain(NPC.Center.X, NPC.Center.Y, leg);
                    legBack = new KineChain(NPC.Center.X, NPC.Center.Y, leg);
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

            NPC.Center = Vector2.Lerp(NPC.Center, FollowingNPC.Center, 0.06f);

            NPC.position.Y += (float)Math.Sin(ID + Main.GameUpdateCount / 10f) * 2f;

            direction = (FollowingNPC.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            if (HasLegs)
            {
                frontRayPosition = Helpers.QuickRaycast(NPC.Center, Vector2.UnitY, maxDistTiles: 24f).End;
                backRayPosition = Helpers.QuickRaycast(NPC.Center + new Vector2(direction.X * 32f, 0f), Vector2.UnitY, maxDistTiles: 24f).End;
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
                    legFront.GenUpdate(frontLegPosition);
                    legBack.GenUpdate(backLegPosition);
                    legFront.basePoint = NPC.Center;
                    legBack.basePoint = NPC.Center + new Vector2(direction.X * 32f, 0f);
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            bool isFacingRight = direction.X > 0f;
            legBack?.Draw(spriteBatch, screenPos, Color.Gray, isFacingRight, femurTexture.Value, tibiaTexture.Value, femurTexture.Value);
            return false;
        }
    }
}
