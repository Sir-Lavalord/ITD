using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispTelegraph : ModProjectile
{
    public override void SetStaticDefaults()
    {
        // DisplayName.SetDefault("Glow Line");
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

    private int aiTimer
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }
    private float rayPosY//be accurate
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    private int drawLayers = 1;
    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
    }
    public override void AI()
    {
        int maxTime = 60;
        float alphaModifier = 3;

        color = new Color(93, 255, 241, 0) * 0.75f;
        alphaModifier = 1;
        Projectile.scale = 1f;

        maxTime = 60;
        if (aiTimer < maxTime / 2)
            aiTimer = maxTime / 2;

        Projectile.position -= Projectile.velocity;
        Projectile.rotation = Projectile.velocity.ToRotation();
    

        if (++aiTimer > maxTime)
        {
            Projectile.Kill();
            return;
        }

        if (alphaModifier >= 0)
        {
            Projectile.alpha = 255 - (int)(255 * Math.Sin(Math.PI / maxTime * aiTimer) * alphaModifier);
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
        }

        color.A = 0;
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int damage = (int)(Projectile.damage * 0.75f);
            int knockBack = 3;
            Projectile rain = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.UnitY * 10, ModContent.ProjectileType<WispFireRain>(), damage, knockBack, Main.myPlayer, 1,rayPosY);
            rain.timeLeft = 300;
        }
        base.OnKill(timeLeft);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (projHitbox.Intersects(targetHitbox))
        {
            return true;
        }
        float num6 = 0f;
        if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * 3000f, 16f * Projectile.scale, ref num6))
        {
            return true;
        }
        return false;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return color * Projectile.Opacity * (Main.mouseTextColor / 255f) * 0.9f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        int num156 = texture2D13.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
        int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
        Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
        Vector2 origin2 = rectangle.Size() / 2f;

        int length = rayPosY == 0? 3000:(int)((Math.Abs(rayPosY - Projectile.Center.Y)));
        Vector2 offset = Projectile.rotation.ToRotationVector2() * length / 2f;
        Vector2 position = Projectile.Center - Main.screenLastPosition + new Vector2(0f, Projectile.gfxOffY) + offset;
        const float resolutionCompensation = 128f / 24f; //i made the image higher res, this compensates to keep original display size
        Rectangle destination = new((int)position.X, (int)position.Y, length, (int)(rectangle.Height * Projectile.scale / resolutionCompensation));

        Color drawColor = Projectile.GetAlpha(lightColor);

        for (int j = 0; j < drawLayers; j++)
            Main.EntitySpriteDraw(new DrawData(texture2D13, destination, new Rectangle?(rectangle), drawColor, Projectile.rotation, origin2, SpriteEffects.None, 0));
        return false;
    }
}
