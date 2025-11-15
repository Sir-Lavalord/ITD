
using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicSwordStar2 : ModProjectile
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
        Projectile.penetrate = 1;
        Projectile.timeLeft = 6000;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.alpha = 0;

    }
    public ref float rotation =>  ref Projectile.ai[0];
    public bool getGoing =>  Projectile.ai[1] != 0;
    public float startTime => Projectile.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
    }
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
        if (Projectile.localAI[0]++ == startTime)
        {
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 2;
            Projectile.ai[1]++;
            spawnGlow = 1; 
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), 0, 0, 0, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Vector2.UnitX.RotatedByRandom(Math.PI) * Main.rand.NextFloat(0.9f, 1.1f) * 10;
            }
        }
        if (getGoing)
        {
            Projectile.velocity *= 1.05f;
        }
        else
        {
            Projectile.velocity = Vector2.Zero;
        }

        spawnGlow -= 0.05f;
    }
    private Color StripColors(float progressOnStrip)
    {
        return new Color(90, 70, 255, 10);
    }
    private float StripWidth(float progressOnStrip)
    {
        return MathHelper.Lerp(10f, 2f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
    }
    float scaleX = 0.75f;
    float scaleY = 2f;
    float spawnGlow = 0;
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
        Vector2 effectOrigin = effectTexture.Size() / 2f;
        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(-20, 0).RotatedBy(Projectile.rotation), null, new Color(255, 255, 255, 40), Projectile.rotation, effectTexture.Size() / 2f, new Vector2(scaleX, scaleX), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(90, 70, 255, 30), Projectile.rotation + MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
        Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(20, 0).RotatedBy(Projectile.rotation), null, new Color(255, 255, 255, 127), Projectile.rotation + MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(0.2f, 3), SpriteEffects.None, 0);

        if (spawnGlow > 0)//fargo eridanus epic
        {
            float scale = 3f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * spawnGlow);
            float opacity = Projectile.Opacity * (float)Math.Sqrt(spawnGlow);
            Main.EntitySpriteDraw(effectTexture, drawPosition + new Vector2(-20, 0).RotatedBy(Projectile.rotation), null, new Color(255, 255, 255, 127) * opacity, Projectile.rotation, effectTexture.Size() / 2f, new Vector2(scaleX, scaleX) * scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(effectTexture, drawPosition, null, new Color(90, 70, 255, 50) * opacity, Projectile.rotation + MathHelper.PiOver2, effectTexture.Size() / 2f, new Vector2(scaleX, scaleY) * scale, SpriteEffects.None, 0);
        }
        return false;
    }
}

