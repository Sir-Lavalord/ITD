using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class WRipperRift : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = 60; Projectile.height = 60;
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
            Projectile.ai[0] += 0.1f;
        }
        else if (Projectile.timeLeft < 10)
        {
            Projectile.ai[0] -= 0.1f;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 position = Projectile.Center - Main.screenPosition;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle sourceRectangle = texture.Frame(1, 1);
        Vector2 origin = sourceRectangle.Size() / 2f;

        Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(36, 12, 34), Projectile.rotation, origin, Projectile.ai[0] * 1.5f, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(133, 50, 88), Projectile.rotation * 1.5f, origin, Projectile.ai[0] * 1.25f, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(255, 244, 191), Projectile.rotation * 2f, origin, Projectile.ai[0], SpriteEffects.None, 0f);

        return false;
    }
}