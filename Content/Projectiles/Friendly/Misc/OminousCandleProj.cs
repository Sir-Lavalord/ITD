using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Content.Projectiles.Hostile.MotherWisp;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Misc;

public class OminousCandleProj : ModProjectile
{
    public override string Texture => "ITD/Content/Items/BossSummons/OminousCandle";
    public ParticleEmitter emitter;

    public float RiseTime = 40f;
    public float StopTime = 80f;
    public float MorphStartTime = 200f;
    public float PopStartTime = 240f;
    public float KillTime = 300f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.scale = 1f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.ai[0] = ModContent.NPCType<WispCandle>();
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = Projectile;
    }

    public override void AI()
    {
        if (emitter != null) emitter.keptAlive = true;

        Projectile.localAI[0]++;
        float timer = Projectile.localAI[0];

        if (NPC.AnyNPCs(ModContent.NPCType<WispCandle>()) || NPC.AnyNPCs(ModContent.NPCType<MotherWisp>()))
        {
            Projectile.velocity.Y += 0.2f;
            Projectile.alpha += 5;
            if (timer >= 60 || Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
            return;
        }

        if (timer < RiseTime)
        {
            Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, 4f, 0.08f);
        }
        else if (timer < StopTime)
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.05f);
        }
        else
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.1f);

            float bobMult = MathHelper.Clamp((timer - StopTime) / 30f, 0f, 1f);
            float bobbing = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 6f * bobMult;
            Vector2 mainPos = Projectile.Center + new Vector2(0, -40 + bobbing);
            int dustRings = 2;

            for (int h = 0; h < dustRings; h++)
            {
                float distanceDivisor = h + 1.5f;
                float dustDistance = 200 / distanceDivisor;
                int numDust = (int)(0.1f * MathHelper.TwoPi * dustDistance);
                float angleIncrement = MathHelper.TwoPi / numDust;
                Vector2 dustOffset = new(dustDistance, 0f);
                dustOffset = dustOffset.RotatedByRandom(MathHelper.TwoPi);
                int var = (int)dustDistance;
                float dustVelocity = 20f / distanceDivisor;

                if (timer < PopStartTime)
                {
                    for (int i = 0; i < numDust; i++)
                    {
                        if (Main.rand.NextBool(var))
                        {
                            dustOffset = dustOffset.RotatedBy(angleIncrement);
                            emitter.Emit(mainPos + dustOffset, Vector2.Normalize(mainPos - (mainPos + dustOffset)) * dustVelocity, 0, 60);
                        }
                    }
                }
            }

            if (emitter != null)
            {
                foreach (ref var p in CollectionsMarshal.AsSpan(emitter.particles))
                {
                    if (Vector2.DistanceSquared(p.position, mainPos) < Math.Sqrt(15))
                    {
                        p.timeLeft = 1;
                    }
                }
            }

            /*
            if (timer >= PopStartTime)
            {
                Projectile.alpha += 8;
                if (Projectile.alpha > 255) Projectile.alpha = 255;
            }
            */

            if (timer >= KillTime)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Roar, Projectile.Center);


                if (emitter != null)
                {
                    for (int i = 0; i < 60; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2Circular(25f, 25f);
                        emitter.Emit(mainPos, velocity, 0, (short)Main.rand.Next(30, 60));
                    }
                }


                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(0, 1000), -Vector2.UnitY, ModContent.ProjectileType<WispCandleDeadRay>(), 0, 0f, Main.myPlayer);

                    int n = NPC.NewNPC(NPC.GetBossSpawnSource(Main.myPlayer), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)Projectile.ai[0]);
                    if (n != Main.maxNPCs && Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendData(MessageID.SyncNPC, number: n);
                    }
                }
                Projectile.Kill();
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Texture2D texture2 = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;

        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Rectangle frame2 = texture2.Frame(1, 1, 0, 0);

        Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height / Main.projFrames[Type] * 0.5f);
        Vector2 origin2 = new Vector2(texture2.Width * 0.5f, texture2.Height * 0.5f);

        float timer = Projectile.localAI[0];

        if (Projectile.velocity.LengthSquared() > 0.1f)
        {
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 oldDrawPos = Projectile.oldPos[i] + new Vector2(Projectile.width / 2f, Projectile.height / 2f) - Main.screenPosition;
                Color trailColor = new Color(131, 255, 236, 80) * Projectile.Opacity * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                sb.Draw(texture, oldDrawPos, frame, trailColor, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None, 0f);
            }
        }

        float morphProgress = MathHelper.Clamp((timer - MorphStartTime) / (KillTime - MorphStartTime), 0f, 1f);
        float morphEased = morphProgress * morphProgress;

        float bobMult = MathHelper.Clamp((timer - StopTime) / 30f, 0f, 1f);
        float bobbing = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 6f * bobMult;

        Vector2 shakeOffset = Vector2.Zero;
        if (morphProgress > 0f)
        {
            shakeOffset = Main.rand.NextVector2Circular(morphEased * 6f, morphEased * 6f);
        }

        Vector2 drawCenter = Projectile.Center - Main.screenPosition + new Vector2(0, bobbing) + shakeOffset;
        Vector2 candleScale = new Vector2(Projectile.scale * (1f - morphEased * 0.6f), Projectile.scale * (1f + morphEased * 1.5f));

        Color baseAuraColor = new Color(131, 255, 236, 150);
        Color fadedAuraColor = baseAuraColor * Projectile.Opacity;

        float time = Main.GlobalTimeWrappedHourly;
        float pulseTimer = (float)Main.time / 240f + time * 0.04f;

        float pulse = (time % 2f) / 1f;
        if (pulse >= 1f) pulse = 2f - pulse;
        pulse = pulse * 0.5f + 1f;

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + pulseTimer) * MathHelper.TwoPi;
            Vector2 offset = new Vector2(0f, 2 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * pulse;
            Main.EntitySpriteDraw(texture, drawCenter + offset, frame, fadedAuraColor, Projectile.rotation, origin, candleScale, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + pulseTimer) * MathHelper.TwoPi;
            Vector2 offset = new Vector2(0f, 4 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * pulse;
            Main.EntitySpriteDraw(texture, drawCenter + offset, frame, fadedAuraColor, Projectile.rotation, origin, candleScale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(texture, drawCenter, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, candleScale, SpriteEffects.None, 0f);

        if (morphProgress > 0f)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, drawCenter, frame, Color.White * morphEased, Projectile.rotation, origin, candleScale, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /*
        if (timer >= PopStartTime)
        {
            float glowProgress = MathHelper.Clamp((timer - PopStartTime) / (KillTime - PopStartTime), 0f, 1f);
            float popScale = (float)Math.Pow(glowProgress, 4);
            Vector2 stretch = new Vector2(1f, 1.5f) * (1f + popScale * 15f);
            Color popColor = Color.Lerp(baseAuraColor, Color.White, glowProgress);

            for (int i = 0; i <= 1; i++)
            {
                Main.EntitySpriteDraw(texture2, drawCenter, frame2, popColor * (1f - glowProgress * 0.3f), Projectile.rotation, origin2, stretch, SpriteEffects.None, 0f);
            }
        }
        */

        return false;
    }
}