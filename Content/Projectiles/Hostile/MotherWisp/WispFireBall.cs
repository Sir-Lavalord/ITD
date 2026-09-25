using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispFireBall : ModProjectile
{
    public VertexStrip TrailStrip = new();
    public VertexStrip TrailStrip2 = new();

    public ref float Target => ref Projectile.ai[0];
    public ref float Timer => ref Projectile.ai[1];
    public ref float BlowTime => ref Projectile.ai[2];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        Main.projFrames[Projectile.type] = 1;
    }
    readonly int defaultWidthHeight = 8;
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 400;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        DrawOffsetX = -16;
        DrawOriginOffsetY = -16;
        Projectile.hide = true;
        Projectile.scale = 1f;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindProjectiles.Add(index);
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return new Color(255, 170, 90);
    }

    private Color StripColors(float progressOnStrip) => new Color(53, 247, 180);
    private Color StripColors2(float progressOnStrip) => Color.White;

    private float StripWidth(float progressOnStrip) => MathHelper.Lerp(16f, 0f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true));

    private float StripWidth2(float progressOnStrip)
    {
        return MathHelper.Lerp(8f, 0f, Utils.GetLerpValue(0f, 0.4f, progressOnStrip, true));

    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 origin = new(texture.Width * 0.5f, texture.Height / Main.projFrames[Type] * 0.5f);
        Vector2 offset = Projectile.Size * 0.5f - Main.screenPosition;
        SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Texture2D texture2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle frame2 = texture2.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Main.EntitySpriteDraw(texture2, Projectile.Center - Main.screenPosition, frame2, new Color(207, 255, 200, 180), Projectile.rotation, new Vector2(texture2.Width * 0.5f, texture2.Height / Main.projFrames[Type] * 0.5f), Projectile.scale * 0.6f, SpriteEffects.None, 0f);
        sb.Draw(texture, Projectile.Center + Main.rand.NextVector2Circular(2,2) - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0f);

        GameShaders.Misc["LightDisc"].Apply(null);

        TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, offset, Projectile.oldPos.Length, true);
        TrailStrip2.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors2, StripWidth2, offset, Projectile.oldPos.Length, true);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        TrailStrip.DrawTrail();
        TrailStrip2.DrawTrail();

        return false;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Timer++;

        if (Timer >= 20 && Timer < 45)
        {
            Projectile.velocity *= 0.95f;
        }
        else if (Timer == 45)
        {
            Projectile.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitY, ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, 0, 0, 45f);
            }
        }
        else if (Timer == 90)
        {
            Projectile.velocity = Vector2.UnitY * 30f;
        }
    }
}