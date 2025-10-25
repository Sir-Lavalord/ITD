using ITD.Systems;
using ITD.Utilities;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class BigBerthShell : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1; 
        ProjectileID.Sets.Explosive[Projectile.type] = true;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.width = 16; Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
    }

    public override void AI()
    {
        for (int i = 0; i < 6; i++)
        {
            float X = Projectile.Center.X - Projectile.velocity.X / 20f * (float)i;
            float Y = Projectile.Center.Y - Projectile.velocity.Y / 20f * (float)i;


            int dust2 = Dust.NewDust(new Vector2(X, Y), 0, 0, DustID.Smoke, 0, 0, 160, Color.WhiteSmoke, 2);
            Main.dust[dust2].position.X = X;
            Main.dust[dust2].position.Y = Y;
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].velocity *= 0f;
        }
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Projectile.velocity.X *= 0.98f;
        Projectile.velocity.Y += 0.8f;

        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
        {
            Projectile.Resize(200, 200);
            if (Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                for (int i = 0; i < 40; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
                    dust.noGravity = true;
                    dust.velocity *= 1f;
                    dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1f);
                    dust.velocity *= 0.5f;
                    dust.noGravity = true;

                }
            }
            Projectile.Opacity = 0;
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.timeLeft > 3)
        {
            Projectile.timeLeft = 3;
        }
        return false;
    }
    public override void OnKill(int timeLeft)
    {
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Projectile.timeLeft > 3)
        {
            Projectile.timeLeft = 3;
        }
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (Projectile.timeLeft > 3)
        {
            Projectile.timeLeft = 3;
        }
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!target.collideY && !target.noGravity)
        {
            Projectile.damage *= 2;

            Projectile.CritChance = 100;
        }
        modifiers.HitDirectionOverride = (Projectile.Center.X < target.Center.X).ToDirectionInt();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
        int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
        Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
        Vector2 origin2 = rectangle.Size() / 2f;

        Color color26 = lightColor;
        color26 = Projectile.GetAlpha(color26);

        SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
        {
            Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
            color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
            Vector2 value4 = Projectile.oldPos[i];
            float num165 = Projectile.oldRot[i];
            Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
        }
        Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
        return false;
    }
}