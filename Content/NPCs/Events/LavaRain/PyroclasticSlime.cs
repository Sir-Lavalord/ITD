using ITD.Content.Events;
using ITD.Content.Projectiles.Hostile;
using ITD.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Events.LavaRain
{
    public class PyroclasticSlime : ITDNPC
    {
        public bool canTriggerLanding = false;
        public Vector2 stretchScale = Vector2.One;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
        }
        public override void SetDefaults()
        {
            NPC.damage = 30;
            NPC.width = 42;
            NPC.height = 28;
            NPC.defense = 60;
            NPC.lifeMax = 100;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
        public override void AI()
        {
            bool aggro = true;

            if (NPC.ai[2] > 1f)
            {
                NPC.ai[2] -= 1f;
            }
            if (NPC.wet)
            {
                if (NPC.collideY)
                {
                    NPC.velocity.Y = -2f;
                }
                if (NPC.velocity.Y < 0f && NPC.ai[3] == NPC.position.X)
                {
                    NPC.direction *= -1;
                    NPC.ai[2] = 200f;
                }
                if (NPC.velocity.Y > 0f)
                {
                    NPC.ai[3] = NPC.position.X;
                }
                if (NPC.velocity.Y > 2f)
                {
                    NPC.velocity.Y = NPC.velocity.Y * 0.9f;
                }
                NPC.velocity.Y = NPC.velocity.Y - 0.5f;
                if (NPC.velocity.Y < -4f)
                {
                    NPC.velocity.Y = -4f;
                }
                if (NPC.ai[2] == 1f & aggro)
                {
                    NPC.TargetClosest(true);
                }
                canTriggerLanding = false;
            }

            NPC.aiAction = 0;
            if (NPC.ai[2] == 0f)
            {
                NPC.ai[0] = -100f;
                NPC.ai[2] = 1f;
                NPC.TargetClosest(true);
            }
            if (NPC.velocity.Y == 0f)
            {
                // slime just landed
                if (canTriggerLanding)
                {
                    canTriggerLanding = false;
                    stretchScale.X = 2f;
                    stretchScale.Y = 0.5f;
                }
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X = NPC.position.X - (NPC.velocity.X + (float)NPC.direction);
                }
                if (NPC.ai[3] == NPC.position.X)
                {
                    NPC.direction *= -1;
                    NPC.ai[2] = 200f;
                }
                NPC.ai[3] = 0f;
                NPC.velocity.X = NPC.velocity.X * 0.8f;
                if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                {
                    NPC.velocity.X = 0f;
                }
                if (aggro)
                {
                    NPC.ai[0] += 1f;
                }
                NPC.ai[0] += 1f;
                float num31 = -1000f;
                int num32 = 0;
                if (NPC.ai[0] >= 0f)
                {
                    num32 = 1;
                }
                if (NPC.ai[0] >= num31 && NPC.ai[0] <= num31 * 0.5f)
                {
                    num32 = 2;
                }
                if (NPC.ai[0] >= num31 * 2f && NPC.ai[0] <= num31 * 1.5f)
                {
                    num32 = 3;
                }
                if (num32 > 0)
                {
                    NPC.netUpdate = true;
                    if (aggro && NPC.ai[2] == 1f)
                    {
                        NPC.TargetClosest(true);
                    }
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PyroclasticTrail>(), 15, 0f);
                    }
                    // big jump
                    if (num32 == 3)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int type = ModContent.ProjectileType<PyroclasticFireball>();
                            float speed = 4.5f;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, Vector2.UnitX * speed, type, 16, 0f);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, -Vector2.UnitX * speed, type, 16, 0f);
                        }
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot, NPC.Center);
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 dustVelo = Main.rand.NextVector2Unit(-MathHelper.PiOver4, -MathHelper.PiOver2) * 12f;
                            Dust.NewDust(NPC.Center + Vector2.UnitY * NPC.height / 2, 1, 1, DustID.Torch, dustVelo.X, dustVelo.Y, Scale: 3f);
                        }
                        stretchScale.Y = 3f;
                        stretchScale.X = 0.25f;
                        NPC.velocity.Y = -12f;
                        NPC.velocity.X = NPC.velocity.X + (float)(3 * NPC.direction);
                        NPC.ai[0] = -200f;
                        NPC.ai[3] = NPC.position.X;
                    }
                    // regular jump
                    else
                    {
                        stretchScale.Y = 2f;
                        stretchScale.X = 0.5f;
                        NPC.velocity.Y = -6f;
                        NPC.velocity.X = NPC.velocity.X + (float)(2 * NPC.direction);
                        NPC.ai[0] = -120f;
                        if (num32 == 1)
                        {
                            NPC.ai[0] += num31;
                        }
                        else
                        {
                            NPC.ai[0] += num31 * 2f;
                        }
                    }
                    canTriggerLanding = true;
                }
                else
                {
                    if (NPC.ai[0] >= -30f)
                    {
                        NPC.aiAction = 1;
                        return;
                    }
                }
            }
            else
            {
                if (NPC.target < 255 && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
                {
                    if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                    {
                        NPC.position.X = NPC.position.X - 1.4f * (float)NPC.direction;
                    }
                    if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        NPC.position.X = NPC.position.X - (NPC.velocity.X + (float)NPC.direction);
                    }
                    if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.01) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.01))
                    {
                        NPC.velocity.X = NPC.velocity.X + 0.2f * (float)NPC.direction;
                        return;
                    }
                    NPC.velocity.X = NPC.velocity.X * 0.93f;
                }
            }
            /*
            // ai0 is a timer
            Main.NewText("ai0: " + NPC.ai[0]);
            // ai1 is ???
            Main.NewText("ai1: " + NPC.ai[1]);
            // ai2 is the amount of time a slime should go the opposite direction after trying to jump into a wall
            Main.NewText("ai2: " + NPC.ai[2]);
            // ai3 is the slime's x position when doing a big jump
            Main.NewText("ai3: " + NPC.ai[3]);
            */
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= NPC.lifeMax / 2 && NPC.ai[1] == 0f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.ai[1] = 1f;
                    NPC.defDefense -= 30;
                    NPC.netUpdate = true;
                }
                if (!Main.dedServ)
                {
                    // dusts in an x pattern
                    int dustsAmount = 5;
                    for (int i = 0; i < dustsAmount; i++)
                    {
                        float progress = i / (float)dustsAmount;
                        Vector2 positionDescendingLine = Vector2.Lerp(NPC.position, NPC.BottomRight, progress);
                        Vector2 positionAscendingLine = Vector2.Lerp(NPC.BottomLeft, NPC.TopRight, progress);
                        Dust d0 = Dust.NewDustPerfect(positionDescendingLine, DustID.Asphalt, NPC.velocity, Scale: 1.2f);
                        d0.noGravity = true;
                        Dust d1 = Dust.NewDustPerfect(positionAscendingLine, DustID.Asphalt, NPC.velocity, Scale: 1.2f);
                        d1.noGravity = true;
                    }
                    Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity + ((-MathHelper.Pi + Main.rand.NextFloat(MathHelper.Pi)).ToRotationVector2() * 3f), Mod.Find<ModGore>("PyroclasticSlimeGore0").Type);
                    SoundEngine.PlaySound(SoundID.Item101, NPC.Center);
                }
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (NPC.ai[1] == 1f)
                modifiers.FinalDamage *= 2f;
        }
        public override void FindFrame(int frameHeight)
        {
            stretchScale = Vector2.SmoothStep(stretchScale, Vector2.One, 0.25f);
            int minFrame = NPC.ai[1] == 1f ? 4 : 0;
            int maxFrame = NPC.ai[1] == 1f ? 7 : 3;
            if (NPC.collideY)
                CommonFrameLoop(frameHeight, minFrame, maxFrame);
            else
                NPC.frame.Y = minFrame * frameHeight;
        }
        public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            float texHeight = tex.Height / Main.npcFrameCount[Type];
            Vector2 origin = new(tex.Width / 2, texHeight);
            Vector2 offset = new(0f, NPC.gfxOffY + 1f + texHeight / 2f);
            //Main.EntitySpriteDraw(tex, NPC.Center - screenPos + offset, NPC.frame, drawColor, 0f, origin, stretchScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(tex, NPC.Center - screenPos + offset, NPC.frame, drawColor, 0f, origin, 1f, SpriteEffects.None);
            spriteBatch.Draw(tex, NPC.Center - screenPos + offset, NPC.frame, drawColor, 0f, origin, stretchScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, NPC.Center - screenPos + offset, NPC.frame, Color.White, 0f, origin, stretchScale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
