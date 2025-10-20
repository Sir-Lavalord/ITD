using ITD.Utilities;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class WindTridentProjectile : ModProjectile
{
    protected virtual float HoldoutRangeMin => 24f;
    protected virtual float InitialHoldoutRange => 64f;
    protected virtual float HoldoutRangeMax => 128f;
    protected virtual float StoppingPoint => 8f;

    private float Charge = 0f;
    public int InitialDamage = 0;
    private int timesToHit;
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.scale = 1.2f;
        Projectile.ownerHitCheck = true;
        Projectile.timeLeft = 20;
    }
    public override void OnSpawn(IEntitySource source)
    {
        InitialDamage = Projectile.damage;
    }
    private float Progress(int time)
    {
        float progress;

        if (time < StoppingPoint)
        {
            progress = time / StoppingPoint;
        }
        else
        {
            progress = (20 - time) / StoppingPoint;
        }

        return progress;
    }
    private Vector2 Holdout(Vector2 direction, int time)
    {
        Player player = Main.player[Projectile.owner];
        return direction * InitialHoldoutRange + Vector2.SmoothStep(direction * HoldoutRangeMin * (-Charge * 0.5f), direction * HoldoutRangeMax, Progress(time)) + Main.rand.NextVector2Circular(player.channel ? Charge * 1.5f : 0f, player.channel ? Charge * 3f : 0f);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float num32 = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity.SafeNormalize(-Vector2.UnitY) * 100f * Projectile.scale, 96f * Projectile.scale, ref num32) && !Main.player[Projectile.owner].channel;
    }
    public override bool PreAI()
    {
        float chargeSpeed = 0.04f;
        Player player = Main.player[Projectile.owner];
        player.heldProj = Projectile.whoAmI;
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
        if (player.channel)
        {
            Projectile.timeLeft = 20;
            Charge = Math.Clamp(Charge + chargeSpeed, 0f, 4f);
            Projectile.velocity = (player.GetITDPlayer().MousePosition - player.MountedCenter).SafeNormalize(Vector2.Zero);
            player.ChangeDir(Projectile.direction);
            player.SetDummyItemTime(2);
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            Projectile.damage = (int)(InitialDamage * (Charge * 0.5f));
        }
        else
        {
            if (Projectile.timeLeft > StoppingPoint && Main.GameUpdateCount % 2 == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 velo = Main.rand.NextVector2Unit(Projectile.rotation, MathHelper.PiOver2) * (Charge * 0.5f) * 3f;
                    Dust d = Dust.NewDustPerfect(Projectile.Center - direction * 128f, DustID.Cloud, velo);
                    d.scale *= 0.6f;
                }
            }
        }
        Projectile.Center = player.MountedCenter + Holdout(direction, Projectile.timeLeft);
        CreateTwisterDust(Projectile.Center, direction.ToRotation());
        UpdateTwisterDust();
        Projectile.rotation = direction.ToRotation();
        if (Projectile.spriteDirection == -1)
        {
            Projectile.rotation += MathHelper.ToRadians(45f);
        }
        else
        {
            Projectile.rotation += MathHelper.ToRadians(135f);
        }
        return false;
    }
    public void CreateTwisterDust(Vector2 createPoint, float angle)
    {
        Color dustColor = Color.Lerp(Color.White, Color.LightBlue, Main.rand.NextFloat(0.7f));
        float radius = 32f;

        Dust dust = Dust.NewDustPerfect(createPoint, DustID.Cloud, Vector2.Zero, 0, dustColor * (Charge * 0.5f), 1f);

        dust.noGravity = true;
        dust.fadeIn = Main.rand.NextFloat(0.8f, 1.2f) * (Charge * 0.5f);

        dust.customData = new TwisterDustData
        {
            InitialPosition = createPoint,
            Angle = angle,
            Radius = radius,
            RotationSpeed = Main.rand.NextFloat(0.08f, 0.12f),
        };
    }
    public void UpdateTwisterDust()
    {
        for (int i = 0; i < Main.maxDust; i++)
        {
            Dust dust = Main.dust[i];
            if (dust.active && dust.customData is TwisterDustData data)
            {
                data.InitialPosition = Vector2.Lerp(data.InitialPosition, Projectile.Center, 0.03f);
                UpdateDustPosition(dust, data);
            }
        }
    }
    public void UpdateDustPosition(Dust dust, TwisterDustData data)
    {
        if (Main.player[Projectile.owner].channel)
        {
            data.Angle += data.RotationSpeed;
            Vector2 newOffset = new((float)Math.Cos(data.Angle) * data.Radius, (float)Math.Sin(data.Angle) * data.Radius);
            dust.position = data.InitialPosition + newOffset;
        }
        else
        {
            dust.customData = null;
            dust.velocity = Projectile.velocity * 10f;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Player player = Main.player[Projectile.owner];
        lightColor = Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16);

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle rectangle = texture.Frame(1, 1);

        Vector2 rotatedDirection = Vector2.Normalize(Projectile.velocity);

        Vector2 position = player.MountedCenter + Holdout(rotatedDirection, Projectile.timeLeft) - Main.screenPosition;

        Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), lightColor, Projectile.rotation, rectangle.Size() / 6f, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
    public class TwisterDustData
    {
        public Vector2 InitialPosition;
        public float Angle;
        public float Radius;
        public float RotationSpeed;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        timesToHit = 20 - (int)(Charge * 4f);
        target.immune[Projectile.owner] = timesToHit;
    }
}
