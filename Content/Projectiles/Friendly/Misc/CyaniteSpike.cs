using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Misc;

public class CyaniteSpike : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 5;
    }
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.alpha = 255;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.Frostburn2, 600);
    }

    public override void AI()
    {
        int num6 = 20;
        int num7 = 20;
        int num8 = 30;
        int maxValue = 5;
        bool flag = Projectile.ai[0] < num6;
        bool flag2 = Projectile.ai[0] >= num7;
        bool flag3 = Projectile.ai[0] >= num8;
        Projectile.ai[0] += 1f;
        Projectile.position -= Projectile.velocity;
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frame = Main.rand.Next(maxValue);
            /*for (int i = 0; i < num2 * Projectile.ai[1]; i++)
				{
					Dust expr_148 = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), dustType, Projectile.velocity * Projectile.ai[1] * 4f * Main.rand.NextFloat(), 0, default(Color), 0.8f + Main.rand.NextFloat() * 0.5f);
				}
				for (int j = 0; j < num3 * Projectile.ai[1]; j++)
				{
					Dust expr_21B = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), dustType, Projectile.velocity * Projectile.ai[1] * 4f * Main.rand.NextFloat(), 0, default(Color), 0.8f + Main.rand.NextFloat() * 0.5f);
					expr_21B.fadeIn = 1f;
				}
				SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);*/
        }
        if (flag)
        {
            Projectile.Opacity += 0.1f;
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
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 200f * Projectile.scale, 22f * Projectile.scale, ref num32);
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
        Rectangle rectangle11 = texture.Frame(1, 5, 0, Projectile.frame, 0, 0);
        Vector2 origin10 = new(16f, rectangle11.Height / 2);
        Color color = Projectile.GetAlpha(lightColor);
        Vector2 scale10 = new(Projectile.scale);
        float expr_A5D0 = 35f;
        float lerpValue4 = Utils.GetLerpValue(expr_A5D0, expr_A5D0 - 5f, Projectile.ai[0], true);
        scale10.Y *= lerpValue4;
        Vector4 value25 = lightColor.ToVector4();
        Vector4 vector35 = new Color(67, 17, 17).ToVector4();
        for (float i = 0f; i < 1f; i += 0.25f)
        {
            float radians = (i + Main.GlobalTimeWrappedHourly) * MathHelper.TwoPi;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(0f, 6f).RotatedBy(radians), new Microsoft.Xna.Framework.Rectangle?(rectangle11), new Color(200, 255, 255, 0) * Projectile.Opacity, Projectile.rotation, origin10, scale10, SpriteEffects.None, 0);
        }
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle11), color, Projectile.rotation, origin10, scale10, SpriteEffects.None, 0f);

        return false;
    }
}
