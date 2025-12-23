using Terraria.Audio;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.Sandberus;

public class SandSpike : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 6;
    }
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.alpha = 255;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        int dustType = 0;
        int maxValue = 6;
        bool flag = Projectile.ai[0] < 30;
        bool flag2 = Projectile.ai[0] >= 30;
        bool flag3 = Projectile.ai[0] >= 35;
        Projectile.ai[0] += 1f;
        Projectile.position -= Projectile.velocity;
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frame = Main.rand.Next(maxValue);
            for (int i = 0; i < 5 * Projectile.ai[1]; i++)
            {
                Dust dust0 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), dustType, Projectile.velocity * Projectile.ai[1] * 16f * Main.rand.NextFloat(), 0, default, 1.2f + Main.rand.NextFloat() * 0.5f);
				dust0.noGravity = true;
            }
            for (int j = 0; j < 5 * Projectile.ai[1]; j++)
            {
                Dust dust1 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), dustType, Projectile.velocity * Projectile.ai[1] * 16f * Main.rand.NextFloat(), 0, default, 1.2f + Main.rand.NextFloat() * 0.5f);
                dust1.fadeIn = 1f;
				dust1.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
        }
        if (flag)
        {
            Projectile.Opacity += 0.2f;
            Projectile.scale = Projectile.Opacity * Projectile.ai[1];
        }
        if (flag2)
        {
            Projectile.Opacity -= 0.2f;
        }
        if (flag3)
        {
            Projectile.Kill();
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float num32 = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 180f * Projectile.scale, 22f * Projectile.scale, ref num32);
    }

    /*public override void OnKill(int timeLeft)
    {
			for (float num19 = 0f; num19 < 1f; num19 += 0.025f)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitY) * num19 * 200f * Projectile.scale, 0, new Vector2?(Main.rand.NextVector2Circular(3f, 3f)), 0, default(Color), 1f);
				dust.velocity.Y *= 0.2f;
				dust.velocity += Projectile.velocity * 0.2f;
			}
    }*/

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle rectangle11 = texture.Frame(1, 6, 0, Projectile.frame, 0, 0);
        Vector2 origin10 = new(16f, rectangle11.Height / 2);
        Color alpha4 = Projectile.GetAlpha(lightColor);
        Vector2 scale10 = new(Projectile.scale);
        float expr_A5D0 = 35f;
        float lerpValue4 = Utils.GetLerpValue(expr_A5D0, expr_A5D0 - 5f, Projectile.ai[0], true);
        scale10.Y *= lerpValue4;
        Vector4 value25 = lightColor.ToVector4();
        Vector4 vector35 = new Color(67, 17, 17).ToVector4();
        //Main.EntitySpriteDraw(TextureAssets.Extra[98].get_Value(), Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) - Projectile.velocity * Projectile.scale * 0.5f, null, Projectile.GetAlpha(new Microsoft.Xna.Framework.Color(vector35.X, vector35.Y, vector35.Z, vector35.W)) * 1f, Projectile.rotation + 1.57079637f, TextureAssets.Extra[98].get_Value().Size() / 2f, Projectile.scale * 0.9f, spriteEffects, 0f);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle11), alpha4, Projectile.rotation, origin10, scale10, SpriteEffects.None, 0f);

        return false;
    }
}
