using ITD.Content.Dusts;
using System;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;

public class FrostgripIceCube : ModProjectile
{
    // private int delay = 0;
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.damage = 99;
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 5;
        Projectile.timeLeft = 600;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.alpha = 255;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 99;
    }

    public override void PostDraw(Color lightColor)
    {
        Texture2D texture = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Melee/Snaptraps/Extra/FrostgripIceCube").Value;
        Vector2 origin = new(texture.Width / 2, texture.Height / 2);
        Vector2 position = Projectile.position - Main.screenPosition + origin;

        SpriteEffects effects = SpriteEffects.None; // Adjust if needed
        Main.spriteBatch.Draw(texture, position, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);
    }

    public override void AI()
    {
        float size = Projectile.scale;

        Projectile.alpha = 255;

        for (int d = 0; d < 6; d++)
        {
            float randomAngle = Main.rand.NextFloat(-24, 24) * (float)(Math.PI / 180);
            Vector2 direction = new Vector2(0, -1).RotatedBy(randomAngle);
            float speed = Main.rand.NextFloat(3f, 4f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<IceChunkDust>(), direction.X * speed, direction.Y * speed, 150, Scale: Projectile.scale);
        }

        SoundStyle shatter = new("ITD/Content/Sounds/FrostgripIceShatter");
        SoundEngine.PlaySound(shatter, Projectile.Center);

        for (int i = 0; i < (int)(size + 15); i++)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                float randomAngle = Main.rand.NextFloat(-24, 24) * (float)(Math.PI / 180);
                Vector2 direction = new Vector2(0, -1).RotatedBy(randomAngle);
                float speed = Main.rand.NextFloat(5f, 10f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, direction.X * speed, direction.Y * speed, ModContent.ProjectileType<FrostgripIcicle>(), 70, 0f, Projectile.owner, 0f, 0f);
            }
        }

        for (int d = 0; d < 5; d++)
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<IceChunkDust>(), 0f, 0f, 150, Scale: Main.rand.NextFloat(0.5f, 1f));
        }

        Projectile.Kill();
    }
}