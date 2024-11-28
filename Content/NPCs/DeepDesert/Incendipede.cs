using ITD.Utilities;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.DeepDesert
{
    public class IncendipedeHead : ITDNPC
    {
        public NPC FollowerNPC => Main.npc[(int)NPC.ai[0]];
        public ref float SineTimer => ref NPC.ai[1];
        public int WormLength { get { return (int)NPC.ai[2]; } set { NPC.ai[2] = value; } }
        public const int SpacingBetween = 8;
        public override void FindFrame(int frameHeight) => CommonFrameLoop(frameHeight);
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
            // i'm using this for the actual segment following logic so consider this the length limit for incendipedes
            NPCID.Sets.TrailCacheLength[Type] = 10 * SpacingBetween;
            // idc about getting rotations cuz we can just calculate those on the fly
            NPCID.Sets.TrailingMode[Type] = TrailingModeID.NPC.PosRotEveryFrame;
            // bestiary stuff
            BestiaryEntry = this.GetLocalization("Bestiary");
            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "ITD/Content/NPCs/DeepDesert/Incendipede_Bestiary",
                Position = new Vector2(40f, 24f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 12f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                // hello how do you add custom biome to this thanks
				//BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
                //BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,

				new FlavorTextBestiaryInfoElement(BestiaryEntry.Value)
            ]);
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
            bool wall = BigHitboxTiles.Points().Any(p => Framing.GetTileSafely(p).WallType != WallID.None);
            if (InvalidTarget)
                NPC.TargetClosest();
            if (wall)
                WallMovement();
            else
                GroundMovement();
        }
        public void WallMovement()
        {
            // sine...
            Vector2 toPlayer = Main.player[NPC.target].Center - NPC.Center;
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
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneDeepDesert)
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
        public override void FindFrame(int frameHeight) => CommonFrameLoop(frameHeight, maxCounter: 3f);
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 16;
            NPC.noGravity = true;
            NPC.friendly = false;
            NPC.lifeMax = 1;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.friendly = false;
        }
        public override void AI()
        {
            if (HeadNPC.Exists() && HeadNPC.type == ModContent.NPCType<IncendipedeHead>())
            {
                int wormLength = (int)HeadNPC.ai[2];
                int spacingIndex = (wormLength - 1 - ID) * IncendipedeHead.SpacingBetween;
                //Main.NewText($"ID: {ID}, SPID: {spacingIndex}");

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
                }
            }
        }
    }
    public class IncendipedeTail : ITDNPC
    {
        public NPC FollowingNPC => Main.npc[(int)NPC.ai[1]];
        public NPC HeadNPC => NPC.realLife > -1 ? Main.npc[NPC.realLife] : null;
        // no ID needed for this one cuz it's just 0 (newest position in NPC.oldPos array)
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;
        public override void FindFrame(int frameHeight) => CommonFrameLoop(frameHeight);
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
            HideFromBestiary();
        }
        public override void SetDefaults()
        {
            NPC.width = NPC.height = 16;
            NPC.noGravity = true;
            NPC.friendly = false;
            NPC.lifeMax = 1;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.friendly = false;
        }
        public override void AI()
        {
            if (HeadNPC.Exists() && HeadNPC.type == ModContent.NPCType<IncendipedeHead>())
            {
                //Main.NewText("can i see a picture of this being worm");
                int wormLength = (int)HeadNPC.ai[2];
                int spacingIndex = ((wormLength - 1) * IncendipedeHead.SpacingBetween);
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
                }
            }
            //Main.NewText("tea is leaf :)");
        }
    }
}
