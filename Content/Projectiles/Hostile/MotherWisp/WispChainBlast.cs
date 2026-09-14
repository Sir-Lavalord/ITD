using ITD.Particles;
using ITD.Particles.Projectiles;
using ITD.Systems;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Hostile.MotherWisp;

public class WispChainBlast : ModProjectile
{

    public override string Texture => "Terraria/Images/Projectile_687";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.LunarFlare];
    }

    public override void SetDefaults()
    {
        Projectile.width = 100;
        Projectile.height = 100;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.scale = 1f;
        Projectile.alpha = 0;
    }

    public override bool? CanDamage()
    {
        return Projectile.frame == 3 || Projectile.frame == 4;
    }

    public override void AI()
    {
        if (Projectile.position.HasNaNs())
        {
            Projectile.Kill();
            return;
        }

        if (++Projectile.frameCounter >= 2)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= Main.projFrames[Projectile.type])
            {
                Projectile.frame--;
                Projectile.Kill();
                return;
            }
        }

        if (Projectile.localAI[1] == 0)
        {
            SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);
            Projectile.position = Projectile.Center;
            Projectile.scale = Projectile.localAI[2] == 0 ? Main.rand.NextFloat(1.5f, 4f)
                : 3f;
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.width = (int)(Projectile.width * Projectile.scale);
            Projectile.height = (int)(Projectile.height * Projectile.scale);
            Projectile.Center = Projectile.position;
        }

        if (++Projectile.localAI[1] == 6 && Projectile.ai[1] > 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.ai[1]--;

            Vector2 baseDirection = Projectile.ai[0].ToRotationVector2();
            float random = MathHelper.ToRadians(15);

            if (Projectile.localAI[0] != 2f)
            {
                float stationaryPersistence = Math.Min(5, Projectile.ai[1]);
                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center + Main.rand.NextVector2Circular(20, 20), Vector2.Zero, Projectile.type,
                    Projectile.damage, 0f, Projectile.owner, Projectile.ai[0], stationaryPersistence);
                if (p != Main.maxProjectiles)
                    Main.projectile[p].localAI[0] = 1f;
            }

            if (Projectile.localAI[0] != 1f)
            {
                float length = Projectile.width / Projectile.scale * 10f / 7f;
                Vector2 offset = length * baseDirection.RotatedBy(Main.rand.NextFloat(-random, random));
                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center + offset, Vector2.Zero, Projectile.type,
                      Projectile.damage, 0f, Projectile.owner, Projectile.ai[0], Projectile.ai[1]);
                if (p != Main.maxProjectiles)
                    Main.projectile[p].localAI[0] = Projectile.localAI[0];
            }
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 2; ++i)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, new Color(), 1.5f);
        if (Main.rand.NextBool(8))
        {
            int i2 = Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position + new Vector2(Projectile.width * Main.rand.Next(100) / 100f, Projectile.height * Main.rand.Next(100) / 100f) - Vector2.One * 10f, new Vector2(), Main.rand.Next(61, 64), 1f);
            Main.gore[i2].velocity *= 0.3f;
            Main.gore[i2].velocity.X += Main.rand.Next(-10, 11) * 0.05f;
            Main.gore[i2].velocity.Y += Main.rand.Next(-10, 11) * 0.05f;
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        Color color;
        if (Projectile.ai[1] > 3)
            color = Color.Lerp(new Color(255, 255, 255, 0), new Color(255, 95, 46, 50), (7 - Projectile.ai[1]) / 4);
        else
            color = Color.Lerp(new Color(255, 95, 46, 50), new Color(150, 35, 0, 100), (3 - Projectile.ai[1]) / 3);

        color *= Projectile.Opacity;

        return color;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
        int y3 = num156 * Projectile.frame;
        Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
        Vector2 origin2 = rectangle.Size() / 2f;
        Color color = Projectile.GetAlpha(lightColor);
        Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color,
            Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}