using ITD.Content.Dusts;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.CosJel;

public class CosmicChainBomb : ModProjectile
{

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 7;
    }
    public ref float lifeLeft =>ref Projectile.ai[0];
    public ref float setRotation => ref Projectile.ai[1];
    public bool doBomb => Projectile.ai[2] == 0;

    public override void SetDefaults()
    {
        Projectile.width = 92;
        Projectile.height = 92;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 80;
        Projectile.Opacity = 0.9f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 5; i++)
        {
            int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CosJelDust>(), 0f, 0f, 0, default(Color), 2f);
            Main.dust[d].noGravity = true;
            Main.dust[d].noLight = true;
            Main.dust[d].velocity *= 4f;
        }
        Projectile.rotation = Main.rand.NextFloat(MathHelper.Pi);
        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
    }
    public override void AI()
    {
        if (Projectile.frame >= 1)
        {
            if (doBomb)
            {
                Projectile.ai[2] = 1;
                lifeLeft--;
                if (lifeLeft > 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(Projectile.width/1.5f + Main.rand.NextFloat(-Projectile.width / 6, Projectile.width / 6), +Main.rand.NextFloat(-Projectile.width / 6, Projectile.width / 6)).RotatedBy(setRotation), Vector2.Zero, ModContent.ProjectileType<CosmicChainBomb>(),
                            Projectile.damage, 0f, Main.myPlayer, lifeLeft, setRotation, 0f);
                    }
                }
            }
        }

        if (++Projectile.frameCounter >= 4)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= Main.projFrames[Projectile.type])
            {
                Projectile.frame--;
                Projectile.Kill();
            }
        }
        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);
            Projectile.scale *= Main.rand.NextFloat(1f, 1.25f);
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White * Projectile.Opacity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
        int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
        Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
        Vector2 origin2 = rectangle.Size() / 2f;
        Color color = Projectile.GetAlpha(lightColor);
        color.A = 210;
        Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
            new Microsoft.Xna.Framework.Rectangle?(rectangle), color, Projectile.rotation, origin2,
            Projectile.scale * 2, SpriteEffects.None, 0);
        return false;
    }
}