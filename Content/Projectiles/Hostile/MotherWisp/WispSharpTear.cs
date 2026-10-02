using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Content.Projectiles.Hostile.MotherWisp;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispSharpTear : ModProjectile
{
    public VertexStrip TrailStrip = new();

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override string Texture => ITD.BlankTexture;

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 900;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.alpha = 0;
        Projectile.netImportant = true;
    }

    public bool IsActivated => Projectile.ai[1] == 1f;
    float spawnGlow = 1;

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }

    public override bool? CanDamage()
    {
        return spawnGlow <= 0;
    }

    public override void AI()
    {
        if (Projectile.localAI[0] == 0)
        {
            Projectile.rotation = Projectile.ai[0];
            Projectile.localAI[0] = 1;
        }

        if (spawnGlow > 0)
        {
            spawnGlow -= 0.05f;
        }

        if (IsActivated)
        {
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 28f;
        }
        else
        {
            Projectile.velocity = Vector2.Zero;
        }
    }

    private Color StripColors(float progressOnStrip)
    {
        return new Color(131, 255, 236, 10);
    }

    private float StripWidth(float progressOnStrip)
    {
        return MathHelper.Lerp(10f, 2f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
    }

    float scaleX = 1f;

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        Main.instance.DrawCacheProjsBehindNPCs.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        GameShaders.Misc["LightDisc"].Apply(null);
        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
        TrailStrip.DrawTrail();

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        Player player = Main.player[Projectile.owner];
        Texture2D effectTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;

        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(-20, 0).RotatedBy(Projectile.rotation), null, new Color(255, 255, 255, 40), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(scaleX, scaleX), SpriteEffects.None, 0);

        if (spawnGlow > 0)
        {
            float scale = 3f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * Math.Max(0, spawnGlow));
            float opacity = Projectile.Opacity * (float)Math.Sqrt(Math.Max(0, spawnGlow));
            Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(-20, 0).RotatedBy(Projectile.rotation), null, new Color(255, 255, 255, 127) * opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(scaleX, scaleX) * scale, SpriteEffects.None, 0);
        }
        return false;
    }
}