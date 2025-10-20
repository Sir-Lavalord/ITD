using ITD.Systems;
using ITD.Utilities;
using System;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class MandinataProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        // Projectile.CloneDefaults(ProjectileID.MonkStaffT2);
        Projectile.width = Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.ownerHitCheck = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Player player = Main.player[Projectile.owner];
        ITDPlayer modPlayer = player.GetITDPlayer();
        if (Projectile.ai[1] == 0f && modPlayer.itemVar[0] == 0f)
        {
            modPlayer.itemVar[0] = 1f;
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(player.Center, 1, 1, DustID.Torch, 0f, 0f, 110, default, 3f);
                dust.velocity *= 4f;
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item45, player.Center);
        }
    }

    public override bool PreAI() // vanilla code of doom
    {
        Player player = Main.player[Projectile.owner];
        Vector2 vector = player.RotatedRelativePoint(player.MountedCenter, false, true);
        Projectile.direction = player.direction;
        player.heldProj = Projectile.whoAmI;
        Projectile.Center = vector;
        if (player.dead)
        {
            Projectile.Kill();
            return false;
        }
        if (!player.frozen)
        {
            Projectile.spriteDirection = Projectile.direction = player.direction;
            Vector2 vector2 = vector;
            /*Projectile.alpha -= 127;
				if (Projectile.alpha < 0)
				{
					Projectile.alpha = 0;
				}*/
            if (Projectile.localAI[0] > 0f)
            {
                Projectile.localAI[0] -= 1f;
            }
            float num = player.itemAnimation / (float)player.itemAnimationMax;
            float num2 = 1f - num;
            float num3 = Projectile.velocity.ToRotation();
            float num4 = Projectile.velocity.Length();
            float num5 = 22f;
            Vector2 spinningpoint = new Vector2(1f, 0f).RotatedBy((double)(3.14159274f + num2 * 6.28318548f), default) * new Vector2(num4, Projectile.ai[0]);
            Projectile.position += spinningpoint.RotatedBy((double)num3, default) + new Vector2(num4 + num5, 0f).RotatedBy((double)num3, default);
            Vector2 target = vector2 + spinningpoint.RotatedBy((double)num3, default) + new Vector2(num4 + num5 + 40f, 0f).RotatedBy((double)num3, default);
            Projectile.rotation = vector2.AngleTo(target) + 0.7853982f * player.direction;
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation += 3.14159274f;
            }
        }
        if (player.whoAmI == Main.myPlayer && player.itemAnimation <= 2)
        {
            Projectile.Kill();
            //player.reuseDelay = 2;
        }

        Vector2 flameVelocity = (Projectile.Center - player.Center) * 0.2f;
        if ((player.itemAnimation == 9 || player.itemAnimation == 13 || player.itemAnimation == 17) && Main.myPlayer == Projectile.owner && Projectile.ai[1] == 1f)
        {
            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), player.Center, flameVelocity, ModContent.ProjectileType<MandinataBreath>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack * 0.5f, Projectile.owner);
        }

        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 110, default, 3f);
        dust.velocity = flameVelocity * 0.25f;
        dust.noGravity = true;

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float f3 = Projectile.rotation - 0.7853982f * Math.Sign(Projectile.velocity.X) + ((Projectile.spriteDirection == -1) ? 3.14159274f : 0f);
        float num24 = 0f;
        float scaleFactor5 = -95f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + f3.ToRotationVector2() * scaleFactor5, 23f * Projectile.scale, ref num24);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Player player = Main.player[Projectile.owner];
        Vector2 position = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition - Vector2.UnitY * player.gfxOffY;
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 origin = new Vector2(texture.Width, texture.Height) / 2f;
        float rotation = Projectile.rotation;
        origin = new Vector2((Projectile.spriteDirection == 1) ? (texture.Width - -8f) : -8f, (player.gravDir == 1f) ? -8f : (texture.Height - -8f));
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
        if (player.gravDir == -1f)
        {
            spriteEffects |= SpriteEffects.FlipVertically;
            rotation += 1.57079637f * (float)-(float)Projectile.spriteDirection;
        }

        Main.EntitySpriteDraw(texture, position, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, spriteEffects, 0f);
        return false;
    }
}