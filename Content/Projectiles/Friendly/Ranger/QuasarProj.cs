using ITD.Content.Dusts;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Ranger;

public class QuasarProj : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.width = 40; Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 120;
    }

    public override void AI()
    {
        Projectile.rotation += 0.05f;
        if (Projectile.timeLeft > 110)
        {
            Projectile.localAI[0] += 0.1f;
        }
        else if (Projectile.timeLeft < 5)
        {
            Projectile.localAI[0] -= 0.1f;
        }
        if (Projectile.localAI[1] > 0f)
            Projectile.localAI[1] -= 0.025f;

        Projectile.localAI[0] -= 0.005f;

        if (Projectile.timeLeft % 35 == 0)
        {
            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Projectile.ai[1], Projectile.ai[2]).RotatedByRandom(0.1f), (int)Projectile.ai[0], Projectile.damage, Projectile.knockBack, Projectile.owner);

            Projectile.localAI[1] += 0.2f;
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, ModContent.DustType<StarlitDust>(), 0f, 0f, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.Item5, Projectile.position);
        }
        Projectile.velocity *= 0.95f;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 position = Projectile.Center - Main.screenPosition;

        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle sourceRectangle = texture.Frame(1, 1);
        Vector2 origin = sourceRectangle.Size() / 2f;

        Texture2D beamTexture = ModContent.Request<Texture2D>(Texture + "_Beam").Value;
        Rectangle beamRectangle = beamTexture.Frame(1, 1);
        Vector2 beamOrigin = beamRectangle.Size() / 2f;

        float scale = Projectile.localAI[0] + Projectile.localAI[1];

        Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(36, 12, 34), Projectile.rotation, origin, scale * 1.25f, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(133, 50, 88), Projectile.rotation * 1.5f, origin, scale, SpriteEffects.None, 0f);

        Main.EntitySpriteDraw(beamTexture, position, beamRectangle, new Color(255, 244, 191, 0), new Vector2(Projectile.ai[1], Projectile.ai[2]).ToRotation(), beamOrigin, scale, SpriteEffects.None, 0f);

        Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(255, 244, 191), Projectile.rotation * 2f, origin, scale * 0.75f, SpriteEffects.None, 0f);

        return false;
    }
}