using Terraria.Audio;

namespace ITD.Content.Projectiles.Other;

public class MoldExplosion : ModProjectile
{
    public override string Texture => ITD.BlankTexture;
    public override void SetDefaults()
    {
        Projectile.width = 64; Projectile.height = 64;
        Projectile.friendly = true;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        for (int i = 0; i < 20; i++)
        {
            int num898 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
            Dust dust2 = Main.dust[num898];
            dust2.velocity *= 1.4f;
        }
        for (int i = 0; i < 10; i++)
        {
            int num900 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
            Main.dust[num900].noGravity = true;
            Dust dust2 = Main.dust[num900];
            dust2.velocity *= 7f;
            num900 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
            dust2 = Main.dust[num900];
            dust2.velocity *= 3f;
        }
        for (int i = 0; i < 2; i++)
        {
            float scaleFactor10 = 0.4f;
            if (i == 1)
            {
                scaleFactor10 = 0.8f;
            }
            for (int j = 0; j < 3; j++)
            {
                Gore gore = Main.gore[Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X, Projectile.position.Y), default, Main.rand.Next(61, 64), 1f)];
                gore.velocity *= scaleFactor10;
                gore.velocity.X++;
                gore.velocity.Y++;
            }
        }
    }
}