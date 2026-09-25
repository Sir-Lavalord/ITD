using ITD.Content.Biomes;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.BlueshroomGroves
{
    public class Blizzer : ModNPC
    {
        public static LocalizedText BestiaryEntry { get; private set; }
        public ref float AI_State => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float StuckRotation => ref NPC.ai[2];

        private enum ActionState
        {
            Hover = 0,
            Windup = 1,
            Dash = 2,
            Stuck = 3
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 5;
            NPCID.Sets.TrailingMode[Type] = 0;
            Main.npcFrameCount[NPC.type] = 4;
            BestiaryEntry = this.GetLocalization("Bestiary");
        }

        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 34;
            NPC.damage = 25;
            NPC.defense = 6;
            NPC.lifeMax = 70;
            NPC.value = Item.buyPrice(copper: 80);
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle = -1;
            SpawnModBiomes = [ModContent.GetInstance<BlueshroomGrovesBiome>().Type];
        }

        public override void DrawBehind(int index)
        {
            if (AI_State == (float)ActionState.Stuck)
            {
                Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
                new FlavorTextBestiaryInfoElement(BestiaryEntry.Value)
            ]);
        }
        Vector2 hoverTarget;
        public override void AI()
        {
            NPC.hide = AI_State == (float)ActionState.Stuck;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }

            Player player = Main.player[NPC.target];

            if (AI_State == (float)ActionState.Hover || AI_State == (float)ActionState.Windup)
            {
                NPC.direction = player.Center.X < NPC.Center.X ? -1 : 1;
                NPC.spriteDirection = NPC.direction;
            }

            switch ((ActionState)AI_State)
            {
                case ActionState.Hover:
                    if (AITimer % 120 == 0)
                    {
                        hoverTarget = player.Center - new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(100, 250));
                    }
                    AITimer++;

                    hoverTarget.Y += MiscHelpers.BetterEssScale(4f, 30f) - 1f;

                    Main.NewText(hoverTarget);
                    Vector2 moveDir = hoverTarget - NPC.Center;
                    float length = moveDir.Length();

                    if (length > 20f)
                    {
                        moveDir.Normalize();
                        moveDir *= 4.5f;
                        NPC.velocity = (NPC.velocity * 20f + moveDir) / 21f;
                    }

                    NPC.rotation = NPC.velocity.X * 0.05f;

                    if (AITimer >= 180f)
                    {
                        AI_State = (float)ActionState.Windup;
                        AITimer = 0;
                        NPC.netUpdate = true;
                    }
                    break;

                case ActionState.Windup:
                    AITimer++;

                    NPC.velocity *= 0.85f;

                    Vector2 aimDir = NPC.DirectionTo(player.Center);

                    float targetRotation = aimDir.ToRotation();
                    if (NPC.spriteDirection == -1) targetRotation += MathHelper.Pi;

                    NPC.rotation = Utils.AngleLerp(NPC.rotation, targetRotation, 0.15f);

                    if (Main.rand.NextBool(2))
                    {
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SnowflakeIce, 0f, 0f, 100, default, 1f);
                    }

                    if (AITimer >= 45f)
                    {
                        AI_State = (float)ActionState.Dash;
                        AITimer = 0;

                        SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
                        NPC.velocity = aimDir * 16f;
                        NPC.netUpdate = true;
                    }
                    break;

                case ActionState.Dash:
                    AITimer++;

                    float dashRotation = NPC.velocity.ToRotation();
                    if (NPC.spriteDirection == -1) dashRotation += MathHelper.Pi;
                    NPC.rotation = dashRotation;

                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDustPerfect(NPC.Center - NPC.velocity * 0.5f, DustID.Ice, -NPC.velocity * 0.2f, 100, default, 1.2f).noGravity = true;
                    }
                    Vector2 beakPos = NPC.Center + NPC.velocity.SafeNormalize(Vector2.Zero) * 20f;
                    Tile tile = Framing.GetTileSafely(beakPos);

                    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        NPC.localAI[0] = NPC.velocity.X;
                        NPC.localAI[1] = NPC.velocity.Y;

                        AI_State = (float)ActionState.Stuck;
                        AITimer = 0;
                        NPC.position += NPC.velocity.SafeNormalize(Vector2.Zero) * 8f;
                        NPC.velocity = Vector2.Zero;
                        StuckRotation = NPC.rotation;
                        NPC.netUpdate = true;

                        SoundEngine.PlaySound(SoundID.Dig, NPC.Center);
                    }
                    else if (AITimer >= 90f)
                    {
                        AI_State = (float)ActionState.Hover;
                        AITimer = 0;
                        NPC.netUpdate = true;
                    }
                    break;

                case ActionState.Stuck:
                    AITimer++;

                    if (AITimer < 150f)
                    {
                        NPC.velocity = Vector2.Zero;
                        float wiggle = MiscHelpers.BetterEssScale(20f, 0.1f) - 1f;
                        NPC.rotation = Utils.AngleLerp(NPC.rotation, StuckRotation + wiggle, 0.15f);

                        if (Main.rand.NextBool(4))
                        {
                            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ice, 0f, 0f, 100, default, 0.8f);
                        }
                    }
                    else if (AITimer == 150f)
                    {
                        Vector2 plungeVel = new Vector2(NPC.localAI[0], NPC.localAI[1]);
                        NPC.velocity = -plungeVel.SafeNormalize(Vector2.UnitY) * 14f;
                        NPC.rotation = StuckRotation;
                        NPC.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    }
                    else
                    {
                        NPC.velocity *= 0.93f;
                        NPC.rotation = StuckRotation;

                        if (AITimer >= 185f)
                        {
                            AI_State = (float)ActionState.Hover;
                            AITimer = 0;
                            NPC.netUpdate = true;
                        }
                    }
                    break;
            }

            if (AI_State == (float)ActionState.Hover || AI_State == (float)ActionState.Windup)
            {
                NPC.velocity = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, fallThrough: true, fall2: true);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
            {
                for (int i = 0; i < hit.Damage / (double)NPC.lifeMax * 30.0; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ice, hit.HitDirection, -1f, 100, default, 1f);
                }
                return;
            }

            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ice, 2f * hit.HitDirection, -2f, 100, default, 1.5f);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (AI_State == (float)ActionState.Dash || AI_State == (float)ActionState.Stuck)
            {
                NPC.frame.Y = 3 * frameHeight;
            }
            else
            {
                NPC.frameCounter++;
                if (NPC.frameCounter >= 5)
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0;
                }

                if (NPC.frame.Y >= 3 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
    }
}