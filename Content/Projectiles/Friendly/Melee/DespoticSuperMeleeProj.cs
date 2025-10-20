using ITD.Systems.DataStructures;
using ITD.Systems.Extensions;
using ITD.Utilities;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class DespoticSuperMeleeProj : ModProjectile
{
    public override string Texture => "ITD/Content/Items/Weapons/Melee/DespoticSuperMeleeSword";
    public const float visualLength = 90f;

    public ref float Special => ref Projectile.ai[0];
    public ref float Direction => ref Projectile.ai[1];

    public int maxTime = 30;
    public float speed;

    public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
        .UseProjectionMatrix(true)
        .UseImage0("Images/Extra_" + 201)
        .UseImage1("Images/Extra_" + 193)
        .UseImage2("Images/Extra_" + 252)
        .UseSaturation(-2.8f)
        .UseOpacity(2f);
    public static VertexStrip vertexStrip = new();

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 30;
        Projectile.Opacity = 0f;
        Projectile.scale = 1.75f;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        if (Projectile.timeLeft == maxTime)
        {
            maxTime = player.itemAnimationMax;
            speed = 30f / maxTime;
            if (Special == 1f)
            {
                maxTime *= 2;
                Projectile.extraUpdates = 1;
            }
            Projectile.timeLeft = maxTime;
        }

        if (Projectile.timeLeft < 5)
            Projectile.Opacity -= 0.2f;
        else if (Projectile.Opacity < 1f)
            Projectile.Opacity += 0.2f;

        Projectile.Center = player.MountedCenter;
        Projectile.velocity = Projectile.velocity.RotatedBy(0.3f * Projectile.timeLeft / maxTime * Direction * speed);
        Projectile.rotation = Projectile.velocity.ToRotation();

        for (int i = Projectile.oldPos.Length - 1; i > 0; i--) // custom trailing
        {
            Projectile.oldPos[i] = Projectile.oldPos[i - 1];
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
        }
        Projectile.oldPos[0] = Projectile.Center + Projectile.velocity * 3f + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 36f;
        Projectile.oldRot[0] = Projectile.rotation + MathHelper.PiOver2;

        player.heldProj = Projectile.whoAmI;
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float num32 = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * visualLength * Projectile.scale, 32f * Projectile.scale, ref num32);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.LocalPlayer.GetITDPlayer().BetterScreenshake(4, 4, 4, false);
        SoundEngine.PlaySound(SoundID.Item92, target.Center);
        for (int j = 0; j < 8; ++j)
        {
            int dust = Dust.NewDust(target.Center, 0, 0, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.5f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 3f;
        }
        if (Special == 1f)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center.X, target.Center.Y - 320, 0f, 24f, ModContent.ProjectileType<DespoticSuperSpecialProj>(), Projectile.damage, 0f, Projectile.owner);
    }

    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Aqua;
        result.A /= 3;
        return result * Projectile.Opacity * Projectile.Opacity;
    }
    private float StripWidth(float progressOnStrip)
    {
        return 64f;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        string path = "ITD/Content/Projectiles/Friendly/Melee/";
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D glow = ModContent.Request<Texture2D>(path + "DespoticSword_Glow").Value;

        Shader.Apply(null);
        vertexStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, -Main.screenPosition, new int?(Projectile.oldPos.Length), true);
        vertexStrip.DrawTrail();

        Main.spriteBatch.End(out SpriteBatchData spriteBatchData); // unapply shaders
        Main.spriteBatch.Begin(spriteBatchData);

        Vector2 extraHoldout = Projectile.velocity * 2f;
        Vector2 drawPos = Projectile.Center + extraHoldout - Main.screenPosition + new Vector2(0f, Main.player[Projectile.owner].gfxOffY);
        Main.EntitySpriteDraw(tex, drawPos, null, lightColor * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, tex.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
        Main.EntitySpriteDraw(glow, drawPos, null, Color.White * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver4, glow.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }
}
