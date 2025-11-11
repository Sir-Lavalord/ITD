using System;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.Utilities;

namespace ITD.Content.Projectiles.Friendly.Misc;

public class Zap : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public ref float ZapTarget => ref Projectile.ai[0];
    public ref float OriginX => ref Projectile.ai[1];
    public ref float OriginY => ref Projectile.ai[2];
    public ref float Seed => ref Projectile.localAI[0];
    public ref float Chain => ref Projectile.localAI[1];

    public MiscShaderData Shader = new MiscShaderData(ModContent.Request<Effect>("ITD/Shaders/MiscShaders/ZapShader"), "ZapPass")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 197)
        .UseImage1("Images/Extra_" + 197)
        .UseImage2("Images/Extra_" + 190)
        .UseOpacity(2f);
    public VertexStrip TrailStrip = new();

    private static readonly int duration = 30;
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = duration;
    }

    public override void AI()
    {
        if (Projectile.timeLeft == duration)
        {
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
            Seed = Main.rand.Next(128);
        }
        else if (Projectile.timeLeft == duration - 5 && Chain > 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC newTarget = null;
            float reach = 600;

            foreach (var npc in Main.ActiveNPCs)
            {
                if (!npc.friendly && npc.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(npc.Center, Projectile.Center);
                    if (distance < reach && Projectile.localNPCImmunity[npc.whoAmI] == 0)
                    {
                        reach = distance;
                        newTarget = npc;
                    }
                }
            }

            if (newTarget != null)
            {
                Projectile newZap = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), newTarget.Center, new Vector2(), Type, (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner, newTarget.whoAmI, Projectile.Center.X, Projectile.Center.Y)];
                newZap.localAI[1] = Chain -= 1;
                Array.Copy(Projectile.localNPCImmunity, newZap.localNPCImmunity, Projectile.localNPCImmunity.Length);
            }
        }
    }

    public override bool? CanHitNPC(NPC target)
    {
        if (target.whoAmI != ZapTarget)
            return false;
        return null;
    }

    private Color StripColors(float progressOnStrip)
    {
        return new Color(150, 255, 255, 0) * (Projectile.timeLeft / (float)duration);
    }
    private float StripWidth(float progressOnStrip)
    {
        return 32f * Projectile.scale;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        UnifiedRandom random = new((int)Seed);

        Vector2 magVec = new Vector2(OriginX, OriginY) - Projectile.Center;
        int size = (int)Math.Floor(magVec.Length() / 25) + 1;
        float toOrigin = magVec.ToRotation();

        float[] rotations = new float[size];
        Array.Fill(rotations, toOrigin);

        Vector2[] positions = new Vector2[size];
        float progressZeroToOne = 1f - (Projectile.timeLeft / (float)duration);
        int i = 0;
        magVec.Along(Projectile.Center, 25, v =>
        {
            positions[i] = v + random.NextVector2Circular(24, 24) * progressZeroToOne;
            i++;
        });
        positions[0] = Projectile.Center;
        positions[size - 1] = new Vector2(OriginX, OriginY);

        Shader.Apply(null);
        TrailStrip.PrepareStrip(positions, rotations, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        return true;
    }
}
