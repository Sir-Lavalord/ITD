using ITD.Utilities;
using System;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Misc;

//dumbass
public abstract class BigBlankExplosion : ModProjectile
{
    public ref float CurrentRadius => ref Projectile.ai[0];
    public ref float MaxRadius => ref Projectile.ai[1];
    public virtual float Fadeout(float completion) => (1f - (float)Math.Sqrt(completion)) * 0.8f;
    public abstract int Lifetime { get; }
    public abstract Color GetCurrentExplosionColor(float pulseCompletionRatio);
    public override string Texture => ITD.BlankTexture;
    public abstract Vector2 ScaleRatio { get; }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.Explosive[Projectile.type] = true;
    }
    public override void AI()
    {
        CurrentRadius = MathHelper.Lerp(CurrentRadius, MaxRadius, 0.25f);
        Projectile.scale = MathHelper.Lerp(1.2f, 5f, Utils.GetLerpValue(Lifetime, 0f, Projectile.timeLeft, true));
        Projectile.ExpandHitboxBy((int)(CurrentRadius * Projectile.scale), (int)(CurrentRadius * Projectile.scale));
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

        float pulseCompletionRatio = Utils.GetLerpValue(Lifetime, 0f, Projectile.timeLeft, true);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.Size * ScaleRatio * 0.5f;
        Rectangle drawArea = new(0, 0, Projectile.width, Projectile.height);
        Color fadeoutColor = new(new Vector4(Fadeout(pulseCompletionRatio)) * Projectile.Opacity);
        DrawData drawData = new(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin").Value, drawPosition, drawArea, fadeoutColor, Projectile.rotation, Projectile.Size, ScaleRatio, SpriteEffects.None, 0);

        GameShaders.Misc["ForceField"].UseColor(GetCurrentExplosionColor(pulseCompletionRatio));
        GameShaders.Misc["ForceField"].Apply(drawData);
        drawData.Draw(Main.spriteBatch);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
}