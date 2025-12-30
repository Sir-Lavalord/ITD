using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicFistSetPunch : ModProjectile
{
    public override string Texture => "ITD/Content/Projectiles/Hostile/CosJel/CosmicFistBump";

    private enum ActionState
    {
        Phasing,
        Tracking,
        Punching,
        
    }
    private ActionState AI_State;
    public int OwnerIndex => (int)Projectile.ai[0];
    //time to release
    public bool isLeftHand => Projectile.ai[1] <= 0;

    public ref float AttackTime => ref Projectile.ai[2];

    public Vector2 targetPos = Vector2.Zero;
    public VertexStrip TrailStrip = new();

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        Main.projFrames[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 900;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.scale = 1.5f;
        Projectile.Opacity = 0;
        Projectile.gfxOffY = -20;
    }
    public override bool? CanDamage()
    {
        return true;
    }
    public override void OnSpawn(IEntitySource source)
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
    }
    static bool isExpert => Main.expertMode;
    static bool isMaster => Main.masterMode;
    public override void AI()
    {
        NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
        if (owner == null)
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        if (owner.ai[3] == -1)//trout
        {
            Projectile.timeLeft = 0;
            Projectile.active = false;
            return;
        }
        Player player = Main.player[owner.target];
        float dir = (isLeftHand ? -1 : 1);
        switch (AI_State)
        {
            case ActionState.Phasing://phasing into existence, can't damage player
                Projectile.frame = 1;

                Projectile.Opacity = Projectile.localAI[1] / 40;

                if (Projectile.localAI[1]++ >= 40)
                {
                    AI_State = ActionState.Tracking;
                    Projectile.localAI[1] = 0;
                }
                int dustRings = 3;
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
                    for (int i = 0; i < numDust; i++)
                    {
                        if (Main.rand.NextBool(var))
                        {
                            dustOffset = dustOffset.RotatedBy(angleIncrement);
                            int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<CosJelDust>());
                            Main.dust[dust].position = Projectile.Center + dustOffset;
                            Main.dust[dust].fadeIn = 1f;
                            Main.dust[dust].velocity = Vector2.Normalize(Projectile.Center - Main.dust[dust].position) * dustVelocity;
                            Main.dust[dust].scale = 1.5f - h;
                        }
                    }
                }
                Projectile.Center = owner.Center + new Vector2(dir * 150, -150);
                Projectile.rotation = Projectile.AngleTo(player.Center) - MathHelper.PiOver2;
                break;
            case ActionState.Tracking:
                Projectile.frame = 1;
                if (Projectile.localAI[1]++ < AttackTime)
                {
                    Projectile.rotation = Projectile.AngleTo(player.Center) - MathHelper.PiOver2;
                }
                else
                {
                    if (Projectile.localAI[1] == AttackTime + 1)
                    {
                        targetPos = player.Center;
                        spawnGlow = 1;
                    }
                    else if (Projectile.localAI[1] >= AttackTime + 30)
                    {

                        Projectile.velocity = Vector2.Normalize(player.Center - Projectile.Center) * 18;
                        Projectile.localAI[1] = 0;
                        AI_State = ActionState.Punching;
                    }
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, PitchVariance = 0.3f }, Projectile.Center);
                }
                Projectile.Center = owner.Center + new Vector2(dir * 150, -150);
                break;
            case ActionState.Punching:
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                if (Projectile.localAI[1]++ >= 120)
                {
                    if (Projectile.alpha < 180)
                        Projectile.alpha += 2;
                    else Projectile.Kill();
                }
                break;
        }
        spawnGlow -= 0.025f;

    }
    float spawnGlow = 0;
    public override void OnKill(int timeLeft)
    {
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }
    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(new Color(36, 12, 34), new Color(84, 73, 255), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
        result.A /= 2;
        return result * 0.5f;
    }
    private float StripWidth(float progressOnStrip)
    {
        return 36 * Projectile.scale;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 center = Projectile.Size / 2f;
        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;

        }

        Vector2 miragePos = Projectile.position - Main.screenPosition + center;
        Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);

        GameShaders.Misc["LightDisc"].Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        //old treasure bag draw code, augh
        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 1f;
        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, isLeftHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;

            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8 + 50f * (1 - Projectile.Opacity)).RotatedBy(radians) * time, frame,
                new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, isLeftHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }
        if (spawnGlow > 0)
        {
            float scale = 2f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * spawnGlow);
            float opacity = Projectile.Opacity * (float)Math.Sqrt(spawnGlow);
            Main.EntitySpriteDraw(tex, miragePos, frame, new Color(90, 70, 255, 50) * opacity, Projectile.rotation, origin, Projectile.scale * scale, isLeftHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        }
        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale,
            isLeftHand ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}