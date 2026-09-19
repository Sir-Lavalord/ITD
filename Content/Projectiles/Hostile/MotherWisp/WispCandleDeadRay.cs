using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispCandleDeadRay : ModProjectile
{
    public override string Texture => "ITD/Content/Projectiles/Hostile/MotherWisp/WispTelegraph";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 5000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.alpha = 255;
        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindProjectiles.Add(index);
    }

    public Color color = Color.White;

    public override bool? CanDamage()
    {
        return false;
    }

    private float aiTimer
    {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private float maxTime
    {
        get => Projectile.ai[2] == 0f ? 60f : Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    public override void AI()
    {
        if (aiTimer < maxTime / 2)
            aiTimer = maxTime / 2;

        Projectile.position -= Projectile.velocity;
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (++aiTimer > maxTime)
        {
            Projectile.Kill();
            return;
        }

        float sineValue = (float)Math.Sin(Math.PI / maxTime * aiTimer);
        Projectile.scale = sineValue * 4f;

        Projectile.alpha = 255 - (int)(255 * sineValue);
        if (Projectile.alpha < 0)
            Projectile.alpha = 0;

        color.A = 0;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (projHitbox.Intersects(targetHitbox))
            return true;

        float num6 = 0f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * 3000f, 16f * Projectile.scale, ref num6))
            return true;

        return false;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return color * Projectile.Opacity * (Main.mouseTextColor / 255f) * 0.9f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        int num156 = texture2D13.Height / Main.projFrames[Projectile.type];
        int y3 = num156 * Projectile.frame;
        Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
        Vector2 origin2 = rectangle.Size() / 2f;

        int length = 3000;
        Vector2 offset = Projectile.rotation.ToRotationVector2() * length / 2f;
        Vector2 position = Projectile.Center - Main.screenLastPosition + new Vector2(0f, Projectile.gfxOffY) + offset;
        const float resolutionCompensation = 128f / 24f;

        float alphaMult = (255f - Projectile.alpha) / 255f;
        Color unused = new Color(53, 247, 180, 200);//eh

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        Rectangle outerDest = new((int)position.X, (int)position.Y, length, (int)(rectangle.Height * Projectile.scale * 8 / resolutionCompensation));
        sb.Draw(texture2D13, outerDest, rectangle, Color.White, Projectile.rotation, origin2, SpriteEffects.None, 0);

        Rectangle coreDest = new((int)position.X, (int)position.Y, length, (int)(rectangle.Height * Projectile.scale * 4f / resolutionCompensation));
        sb.Draw(texture2D13, coreDest, rectangle, new Color(207, 254, 200, 255) * alphaMult, Projectile.rotation, origin2, SpriteEffects.None, 0);

        return false;
    }
}