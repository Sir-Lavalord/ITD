using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Summoner;

public class UndertakerGhostProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        Main.projFrames[Projectile.type] = 5;
    }
    int formTimer = 30;
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.penetrate = 5;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = false;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Player player = Main.player[Projectile.owner];
        Projectile.spriteDirection = (player.Center.X > Projectile.Center.X) ? 1 : -1;
        Projectile.scale = 0;
        Projectile.Opacity = 0;
        Projectile.tileCollide = false;
    }
    public override bool? CanDamage()
    {
        return false;
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        Projectile.ai[0]++;
        if (Projectile.ai[0] <= formTimer)
        {
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.Center, 2, 2, DustID.CrimsonTorch, 0f, 0f, 0, default, 2f);
                Main.dust[dust].noGravity = true;
            }
            Projectile.spriteDirection = (player.Center.X > Projectile.Center.X) ? 1 : -1;
            Projectile.velocity.Y += 0.1f;
            Projectile.velocity.X *= 0.99f;
            if (Projectile.ai[0] == formTimer)
            {
                Projectile.velocity *= 0.5f;
                spawnGlow = 1;
                Projectile.netUpdate = true;
                Projectile.rotation = Projectile.spriteDirection > 0 ? 
                    Vector2.Normalize(Projectile.Center - Main.MouseWorld).ToRotation()
                    : Vector2.Normalize(Main.MouseWorld - Projectile.Center).ToRotation();
/*                Projectile.rotation = Projectile.spriteDirection < 0 ? Projectile.velocity.ToRotation() : Projectile.velocity.ToRotation() - MathHelper.Pi;
*/            }
        }
        else
        {

            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1, 0.2f);
            if (Projectile.ai[0] == formTimer + 10)
            {
                Projectile.velocity = Vector2.Normalize(Vector2.UnitX.RotatedBy(Projectile.rotation)) * 8 * Projectile.spriteDirection;
                shootGlow = 1;
                Projectile.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
                Vector2 vel  = Vector2.Normalize(Vector2.UnitX.RotatedBy(Projectile.rotation)) * -14 * Projectile.spriteDirection;
                muzzleFlashPos = Projectile.Center + new Vector2(26 * -Projectile.spriteDirection, 6).RotatedBy(Projectile.rotation);
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(),
                    Projectile.Center + new Vector2(10 * -Projectile.spriteDirection, 6).RotatedBy(Projectile.rotation),
                    vel,
                ModContent.ProjectileType<UndertakerGhostBullet>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                for (int i = 0; i < 12; i++)
                {
                    Vector2 newVelocity = Vector2.Normalize(Vector2.UnitX.RotatedBy(Projectile.rotation)) * -8 * Projectile.spriteDirection;
                    newVelocity *= Main.rand.NextFloat(2f);
                    int dust = Dust.NewDust(muzzleFlashPos, 0, 0, DustID.CrimsonTorch, 0f, 0f, 0, default, 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity = newVelocity.RotatedByRandom(MathHelper.ToRadians(30));
                }
            }
            if (Projectile.ai[0] >= formTimer + 30)
            {
                Projectile.Opacity -= 0.1f;
                Projectile.position.Y -= 1;
            }
            else
            {
                Projectile.Opacity = MathHelper.Lerp(0, 1, 1f);
            }
            Projectile.velocity *= 0.8f;
        }
        spawnGlow = MathHelper.Max(0, spawnGlow - 0.05f);
        shootGlow = MathHelper.Max(0, shootGlow - 0.05f);
    }

    public override void OnKill(int timeLeft)
    {
    }
    float spawnGlow = 0;
    float shootGlow = 0;
    private Vector2 muzzleFlashPos;
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 stretch = new(1f, 1f);
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Texture2D tex2 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Summoner/UndertakerGhostMuzzle").Value;
        Texture2D tex3 = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle frame = tex.Frame(1, 1, 0, 0);
        Rectangle frame3 = tex3.Frame(1, 1, 0, 0);
        Vector2 center = Projectile.Size / 2f;
        stretch = new Vector2(1f, 1f + Projectile.velocity.Length() * 0.05f);

        for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
        {
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
            Projectile.oldRot[i] = Projectile.rotation + MathHelper.PiOver2;
        }

        Vector2 miragePos = Projectile.position - Main.screenPosition + center;
        Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.5f;

        for (float i = 0f; i < 1f; i += 0.25f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 4f + (1 - Projectile.Opacity) * 10).RotatedBy(radians) * time, frame, new Color(85, 0, 13, 30) * Projectile.Opacity, Projectile.rotation, origin, stretch * Projectile.scale, (Projectile.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        for (float i = 0f; i < 1f; i += 0.34f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6f + (1 - Projectile.Opacity) * 20).RotatedBy(radians) * time, frame, new Color(85, 0, 13, 30) *
                Projectile.Opacity, Projectile.rotation, origin, stretch * Projectile.scale, (Projectile.spriteDirection > 0 ?
                SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }
        Main.EntitySpriteDraw(tex, miragePos, frame, new Color(255, 255, 255, 200) * Projectile.Opacity, Projectile.rotation, origin, stretch * Projectile.scale, (Projectile.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        if (Projectile.ai[0] <= formTimer)
        {
            Main.EntitySpriteDraw(tex3, miragePos, frame3, new Color(215, 0, 0, 200) * (1 - Projectile.Opacity),
               time, tex3.Size() / 2, new Vector2(0.4f, 0.4f) * Main.essScale, (Projectile.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            Main.EntitySpriteDraw(tex3, miragePos, frame3, new Color(86, 13, 13, 200) * (1 - Projectile.Opacity),
                Projectile.rotation, tex3.Size() / 2, new Vector2(0.25f, 0.25f) * Main.essScale, (Projectile.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);

        }

        if (spawnGlow > 0)
        {
            float scale = 2f * Projectile.scale * (float)Math.Cos(Math.PI / 2 * spawnGlow);
            float opacity = Projectile.Opacity * (float)Math.Sqrt(spawnGlow);
            Main.EntitySpriteDraw(tex, miragePos, frame, new Color(237, 28, 36, 50) * opacity, Projectile.rotation, origin, stretch * scale, (Projectile.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        if (shootGlow > 0)
        {
            float animProgress = 1f - shootGlow;
            float scaleMultiplier = 0f;

            if (animProgress < 0.2f)
            {
                scaleMultiplier = animProgress / 0.2f;
            }
            else
            {
                scaleMultiplier = (1f - animProgress) / 0.8f;
            }
            scaleMultiplier = (float)Math.Sin(scaleMultiplier * MathHelper.PiOver2);
            muzzleFlashPos = Projectile.Center + new Vector2(26 * -Projectile.spriteDirection , 6).RotatedBy(Projectile.rotation);
            float scale = 2f * Projectile.scale * scaleMultiplier;
            Vector2 flashOrigin = new Vector2(tex2.Width / 2f, tex2.Height);

            Vector2 lockedDrawPos = muzzleFlashPos - Main.screenPosition;

            Main.EntitySpriteDraw(tex2, lockedDrawPos, null, new Color(255, 255, 255, 20),
                Projectile.rotation + (Projectile.spriteDirection > 0 ? -1: 1) * MathHelper.PiOver2
                , flashOrigin, stretch * scale, (Projectile.spriteDirection > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        return false;
    }
}