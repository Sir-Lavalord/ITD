using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispFireOrb : ModProjectile
{
    private readonly Asset<Texture2D> effect = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Hostile/CosJel/CosmicSludgeBomb_Effect");
    public override string Texture => "ITD/Content/Projectiles/Hostile/MotherWisp/WispScythe";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        Main.projFrames[Projectile.type] = 1;
    }

    readonly int defaultWidthHeight = 40;
    public override void SetDefaults()
    {
        Projectile.width = defaultWidthHeight;
        Projectile.height = defaultWidthHeight;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 400;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.hide = false;
        Projectile.scale = 1f;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * (1f - Projectile.alpha / 255f);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawOrigin = new(texture.Width * 0.5f, Projectile.height * 0.5f);

        for (int k = 0; k < Projectile.oldPos.Length; k++)
        {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY + DrawOriginOffsetY) + new Vector2(DrawOffsetX, DrawOriginOffsetY) + new Vector2(4, 4);
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
        }

        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 center = Projectile.Size / 2f;

        Vector2 miragePos = Projectile.position - Main.screenPosition + center;
        Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);

        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.5f;

        for (float i = 0f; i < 1f; i += 0.35f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.5f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8).RotatedBy(radians) * time, frame, new Color(90, 70, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }

    public ref float Target => ref Projectile.ai[0];
    public ref float Timer => ref Projectile.ai[1];
    public ref float BlowTime => ref Projectile.ai[2];
    Vector2 targetPos;

    public override void AI()
    {
        if (Projectile.velocity.LengthSquared() > 0.1f)
        {
            Projectile.rotation += Projectile.velocity.ToRotation() * 0.1f;
        }
        Projectile.rotation += 0.2f;

        Player player = Main.player[(int)Target];
        Timer++;

        if (Timer < BlowTime)
        {
            targetPos = player.Center;
            Projectile.velocity *= 0.8f;
        }
        else
        {
            if (Timer == BlowTime)
            {
/*                if (Main.expertMode || Main.masterMode)
                {
                    targetPos = player.Center + player.velocity * 5f;
                }*/
                float targetAngle = Projectile.AngleTo(targetPos);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, targetAngle.ToRotationVector2(),
                        ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, targetAngle, 0, 20f);
                }

                Projectile.netUpdate = true;
            }

            if (Timer == BlowTime + 20)
            {
                float desiredSpeed = 24f;
                float targetAngle = Projectile.AngleTo(targetPos);

                Projectile.velocity = targetAngle.ToRotationVector2() * desiredSpeed;
                Projectile.netUpdate = true;
            }
        }
    }
}