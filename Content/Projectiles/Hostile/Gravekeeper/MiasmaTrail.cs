using ITD.Content.Buffs.Debuffs;
using System;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Hostile.Gravekeeper;

public class MiasmaTrail : ModProjectile
{
    private static readonly float Lifespan = 80f;
    private static readonly float Halflife = 50f;
    public ref float Life => ref Projectile.localAI[0];
    public ref float DirX => ref Projectile.ai[0];
    public ref float DirY => ref Projectile.ai[1];
    public ref float Chain => ref Projectile.ai[2];
    public override void SetDefaults()
    {
        Projectile.width = 70;
        Projectile.height = 70;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        target.AddBuff(ModContent.BuffType<NecrosisBuff>(), 300, false);
    }

    public override bool CanHitPlayer(Player target)
    {
        return Life >= Halflife;
    }

    public override void AI()
    {
        Life += 1f;

        if (Life == Halflife)
            SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);

        if (Life == 5f && Chain > 0f && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 offset = new Vector2(DirX, DirY).RotatedByRandom(MathHelper.ToRadians(60));
            offset.Normalize();
            offset *= 100f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset, new Vector2(), Type, Projectile.damage, 0, -1, offset.X, offset.Y, Chain - 1f);
        }

        if (Life >= Lifespan)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)  // horrible evil vanilla code
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D skullTexture = ModContent.Request<Texture2D>(Texture + "_Skull").Value;
        Color value2 = new(100, 120, 255, 70);
        Color color2 = new(100, 120, 255, 70);
        Color color3 = Color.Lerp(new Color(100, 80, 255, 100), color2, 0.25f);
        Color color4 = new(80, 80, 80, 100);
        float num3 = 0.35f;
        float num4 = 0.7f;
        float num5 = 0.85f;
        float opacity;
        if (Life < Halflife)
            opacity = Utils.Remap(Life, 0f, Halflife * 0.2f, 0f, 0.8f, true);
        else
            opacity = Utils.Remap(Life, Halflife + 24f, Lifespan, 1f, 0f, true);
        float progress = Utils.Remap(Life, Halflife, Lifespan, 0f, 1f, true);
        float skullScale = Utils.Remap(progress, 0.2f, 1f, 0f, 1f, true);
        float scale = Utils.Remap(progress, 0.2f, 0.4f, 0.3f, 1f, true);
        Rectangle rectangle = texture.Frame(1, 1);
        Rectangle skullRectangle = skullTexture.Frame(1, 1);
        if (progress < 1f)
        {
            for (int i = 0; i < 2; i++)
            {
                for (float j = 1f; j >= 0f; j -= 0.2f)
                {
                    if (progress < num3)
                    {
                        value2 = color2;
                    }
                    else if (progress < num4)
                    {
                        value2 = Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, progress, true));
                    }
                    else if (progress < num5)
                    {
                        value2 = Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, progress, true));
                    }
                    else if (progress < 1f)
                    {
                        value2 = Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, progress, true));
                    }
                    float num11 = 1f - j;// * Utils.Remap(progress, 0f, 0.2f, 0f, 1f, true);
                    Vector2 position = Projectile.Center - Main.screenPosition;
                    Color primaryColor = value2 * num11;
                    Color secondaryColor = primaryColor;
                    secondaryColor.G /= 2;
                    secondaryColor.B /= 2;
                    secondaryColor.A = (byte)Math.Min(primaryColor.A + 80f * num11, 255f);
                    Utils.Remap(Life, 20f, Lifespan, 0f, 1f, true);
                    float num12 = 1f / 0.2f * (j + 1f);
                    float rotation = Projectile.rotation + j * 1.57079637f + Main.GlobalTimeWrappedHourly * num12 * 2f * Projectile.direction;
                    if (i == 0)
                    {
                        Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), secondaryColor * opacity * 0.5f, rotation + 0.5f, rectangle.Size() / 2f, scale, SpriteEffects.None, 0f);
                        Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), secondaryColor * opacity, rotation * 0.5f, rectangle.Size() / 2f, scale, SpriteEffects.None, 0f);
                    }
                    else if (i == 1)
                    {
                        Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), primaryColor * opacity, rotation + 1.57079637f, rectangle.Size() / 2f, scale * 0.75f, SpriteEffects.None, 0f);
                        Main.EntitySpriteDraw(skullTexture, position, new Rectangle?(skullRectangle), secondaryColor * opacity, 0f, skullRectangle.Size() / 2f, scale * skullScale * 0.75f, SpriteEffects.None, 0f);
                    }
                }
            }
        }
        return false;
    }
}
