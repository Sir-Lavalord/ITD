using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispArena : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public ParticleEmitter emitter;
    public VertexStrip RingStrip = new VertexStrip();

    public float TargetRadius
    {
        get => Projectile.ai[1] <= 0 ? 600f : Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }

    public float currentRadius = 0f;

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.hostile = false;
        Projectile.friendly = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.netImportant = true;
        Projectile.timeLeft = 60;
        emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = Projectile;
    }

    public override void AI()
    {
        if (emitter != null) emitter.keptAlive = true;

        NPC boss = Main.npc[(int)Projectile.ai[0]];
        if (!boss.active || boss.type != ModContent.NPCType<NPCs.Bosses.MotherWisp>())
        {
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 60;

        Projectile.localAI[0]++;
        float expandTime = 180f;
        float progress = MathHelper.Clamp(Projectile.localAI[0] / expandTime, 0f, 1f);

        if (progress < 1f)
        {
            float easeOut = 1f - (float)Math.Pow(1f - progress, 3);
            currentRadius = MathHelper.Lerp(0f, TargetRadius, easeOut);
        }
        else
        {
            currentRadius = MathHelper.Lerp(currentRadius, TargetRadius, 0.05f);
        }

        Projectile.Center = Vector2.Lerp(Projectile.Center, boss.Center, 0.02f);

        float maxDragDist = 3000f * 16f;

        if (progress >= 1f)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Player p = Main.LocalPlayer;
                if (p.active && !p.dead)
                {
                    float dist = Vector2.Distance(p.Center, Projectile.Center);
                    if (dist > currentRadius && dist < maxDragDist)
                    {
                        Vector2 pullDir = Vector2.Normalize(Projectile.Center - p.Center);

                        if (dist < currentRadius + 40f)
                        {
                            p.Center = Projectile.Center - pullDir * currentRadius;

                            float pushForce = Vector2.Dot(p.velocity, pullDir);
                            if (pushForce < 0)
                            {
                                p.velocity -= pushForce * pullDir;

                                int sparkCount = (int)MathHelper.Clamp(Math.Abs(pushForce), 2f, 8f);
                                for (int i = 0; i < sparkCount; i++)
                                {
                                    Vector2 sparkVel = pullDir.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, Math.Abs(pushForce) + 2f);
                                    emitter?.Emit(p.Center, sparkVel, 0f, (short)Main.rand.Next(15, 30));
                                }
                            }
                            else if (p.velocity.LengthSquared() > 0.5f && Main.rand.NextBool(4))
                            {
                                Vector2 sparkVel = pullDir.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(1f, 3f);
                                emitter?.Emit(p.Center, sparkVel, 0f, (short)Main.rand.Next(10, 20));
                            }
                        }
                        else
                        {
                            p.velocity = pullDir * 20f;
                        }
                    }
                }
            }
        }
    }

    private Color StripColors(float progressOnStrip)
    {
        float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.2f + 0.8f;
        Color result = new Color(165, 255, 185) * pulse;

        float overlapRatio = 15f / 75f;
        float fade = 1f;

        if (progressOnStrip < overlapRatio)
        {
            fade = MathHelper.SmoothStep(0f, 1f, progressOnStrip / overlapRatio);
        }
        else if (progressOnStrip > 1f - overlapRatio)
        {
            fade = MathHelper.SmoothStep(1f, 0f, (progressOnStrip - (1f - overlapRatio)) / overlapRatio);
        }

        return result * Projectile.Opacity * fade;
    }

    private float StripWidth(float progressOnStrip)
    {
        float expandTime = 180f;
        float progress = MathHelper.Clamp(Projectile.localAI[0] / expandTime, 0f, 1f);
        return MathHelper.Lerp(4f, 180f, progress);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        int baseSegments = 60;
        int overlapSegments = 15;
        int totalSegments = baseSegments + overlapSegments;

        Vector2[] circlePos = new Vector2[totalSegments + 1];
        float[] circleRot = new float[totalSegments + 1];

        float rotationOffset = Main.GlobalTimeWrappedHourly * 0.5f;

        for (int i = 0; i <= totalSegments; i++)
        {
            float angle = MathHelper.TwoPi * ((float)i / baseSegments) + rotationOffset;

            // Uses currentRadius directly so it draws seamlessly during dynamic adjustments
            circlePos[i] = Projectile.Center + angle.ToRotationVector2() * currentRadius;
            circleRot[i] = angle + MathHelper.PiOver2;
        }

        GameShaders.Misc["MagicMissile"].Apply(null);

        RingStrip.PrepareStrip(circlePos, circleRot, StripColors, StripWidth, -Main.screenPosition, circlePos.Length, true);
        RingStrip.DrawTrail();

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        return false;
    }
}