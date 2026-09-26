using ITD.Content.Biomes;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
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
        public Vector2 beakPos = Vector2.Zero;

        private enum ActionState
        {
            Hover = 0,
            Windup = 1,
            Dash = 2,
            Stuck = 3
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 20;
            NPCID.Sets.TrailingMode[Type] = 3;
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
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
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
        public override void OnSpawn(IEntitySource source)
        {
            beakPos = NPC.Center + NPC.velocity.SafeNormalize(Vector2.Zero) * 20f;
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

            if (AI_State == (float)ActionState.Hover /*|| AI_State == (float)ActionState.Windup*/)
            {
                NPC.direction = player.Center.X < NPC.Center.X ? -1 : 1;
                NPC.spriteDirection = NPC.direction;
            }
            beakPos = NPC.Center + NPC.velocity.SafeNormalize(Vector2.Zero) * 20f;

            switch ((ActionState)AI_State)
            {
                case ActionState.Hover:
                    float sideOffset = (NPC.Center.X > player.Center.X) ? 250f : -250f;
                    Vector2 targetPos = player.Center + new Vector2(sideOffset, -150f);

                    targetPos.X += (float)MiscHelpers.BetterEssScale(1,0.2f) * 50f;
                    targetPos.Y -= (float)MiscHelpers.BetterEssScale(1, 0.5f) * 30f;

                    Vector2 moveDir = targetPos - NPC.Center;
                    float speed = 4.5f;
                    float inertia = 40f;

                    if (moveDir.Length() > 10f)
                    {
                        moveDir.Normalize();
                        moveDir *= speed;
                        NPC.velocity = (NPC.velocity * (inertia - 1) + moveDir) / inertia;
                    }

                    NPC.rotation = NPC.velocity.X * 0.05f;

                    if (AITimer++ >= 120f)
                    {
                        AI_State = (float)ActionState.Windup;
                        AITimer = 0;
                        NPC.netUpdate = true;
                    }
                    break;

                case ActionState.Windup:
                    AITimer++;
                    NPC.knockBackResist = 0f;

                    NPC.velocity *= 0.85f;
                    Vector2 aimDir = NPC.DirectionTo(player.Center);

                    if (AITimer <= 20)
                    {
                        float targetRotation = aimDir.ToRotation();
                        if (NPC.spriteDirection == -1) targetRotation += MathHelper.Pi;

                        if (AITimer == 20)
                        {
                            SoundEngine.PlaySound(SoundID.NPCDeath7, NPC.Center);
                            spawnGlow = 1;

                        }
                        NPC.rotation = Utils.AngleLerp(NPC.rotation, targetRotation, 0.15f);
                    }

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
                    NPC.knockBackResist = 0f;

                    float dashRotation = NPC.velocity.ToRotation();
                    if (NPC.spriteDirection == -1) dashRotation += MathHelper.Pi;
                    NPC.rotation = dashRotation;

                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDustPerfect(NPC.Center - NPC.velocity * 0.5f, DustID.Ice, -NPC.velocity * 0.2f, 100, default, 1.2f).noGravity = true;
                    }
                    Tile tile = Framing.GetTileSafely(beakPos);

                    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            Vector2 dustVel = -NPC.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * Main.rand.NextFloat(0.5f, 1.1f); ;
                            Dust ice = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Ice, dustVel.X, dustVel.Y, 100, default, 2f);
                            ice.noGravity = true;
                        }
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
                        if (AITimer % 14 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.WormDig, NPC.Center);
                        }
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
                        for (int i = 0; i < 20; i++)
                        {
                            Vector2 dustVel = NPC.velocity.RotatedByRandom(MathHelper.ToRadians(20)) * Main.rand.NextFloat(0.5f, 1.1f); ;
                            Dust ice = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Ice, dustVel.X, dustVel.Y, 100, default, 2f);
                            ice.noGravity = true;
                        }
                        SoundEngine.PlaySound(SoundID.Dig, NPC.Center);
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
            spawnGlow -= 0.05f;
            if (spawnGlow <= 0)
            {
                glowRot = Main.rand.NextFloat(MathHelper.TwoPi);
                spawnGlow = 0;
            }
            if (AI_State == (float)ActionState.Hover || AI_State == (float)ActionState.Windup)
            {
                NPC.velocity = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, fallThrough: true, fall2: true);
            }
        }
        public float spawnGlow = 1;
        public float glowRot = 0;

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
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Npc[Type].Value;
            int frameHeight = texture.Height / Main.npcFrameCount[Type];

            Texture2D glowOrb = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
            Rectangle glowOrbFrame = glowOrb.Frame(1, 1, 0, 0);

            Rectangle rect = texture.Frame(1, Main.npcFrameCount[Type], 0, NPC.frame.Y / frameHeight);
            Vector2 center = NPC.Size / 2f;
            SpriteEffects flip = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (AI_State == (float)ActionState.Dash || AI_State == (float)ActionState.Stuck)
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + center;
                    Color color = new Color(131, 255, 236, 80) * NPC.Opacity * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    spriteBatch.Draw(texture, drawPos, rect, color, NPC.oldRot[k], rect.Size() / 2f, 1f, flip, 0f);
                }
            }

            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;

            time %= 4f;
            time /= 2f;

            if (time >= 1f) time = 2f - time;
            time = time * 0.5f + 0.75f;
            Color unused = new Color(131, 255, 236, 200);//eh
            if (AI_State == (float)ActionState.Dash)
            {
                for (float i = 0f; i < 1f; i += 0.5f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;
                    sb.Draw(texture, NPC.Center - Main.screenPosition + new Vector2(0f, 2f).RotatedBy(radians) * time, rect, unused * NPC.Opacity, NPC.rotation, rect.Size() / 2f, NPC.scale, flip, 0);
                }
                for (float i = 0f; i < 1f; i += 0.2f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;
                    sb.Draw(texture, NPC.Center - Main.screenPosition + new Vector2(0f, 4f).RotatedBy(radians) * time, rect, unused * NPC.Opacity, NPC.rotation, rect.Size() / 2f, NPC.scale, flip, 0);
                }
            }
            void DrawAtNPC(Texture2D tex, Rectangle drawRect, float scale)
            {
                sb.Draw(tex, NPC.Center - Main.screenPosition, drawRect, Color.White * NPC.Opacity, NPC.rotation,
                    drawRect.Size() / 2f, scale, flip, 0f);
            }

            DrawAtNPC(texture, rect, NPC.scale);

            Vector2 visualBeakPos = NPC.Center + new Vector2(36, 32 * NPC.spriteDirection).RotatedBy(NPC.rotation + (NPC.spriteDirection == 1 ? 0f : MathHelper.Pi));

            Texture2D effectTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 drawPosition = visualBeakPos - Main.screenPosition;
            Vector2 glowScale = new Vector2(1.5f, 0.1f);

            if (spawnGlow > 0)
            {
                float scale = 2f * (float)Math.Cos(Math.PI / 2 * spawnGlow);
                float opacity = NPC.Opacity * (float)Math.Sqrt(spawnGlow);

                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(255, 255, 255, 0) * opacity,
                    glowRot, effectTexture.Size() / 2f, glowScale * scale, SpriteEffects.None, 0);

                Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(131, 255, 236, 80) * opacity,
                    glowRot + MathHelper.PiOver2, effectTexture.Size() / 2f, glowScale * scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}