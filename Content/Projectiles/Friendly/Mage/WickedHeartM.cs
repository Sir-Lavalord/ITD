namespace ITD.Content.Projectiles.Friendly.Mage;

public class WickedHeartM : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Magic;
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    private NPC HomingTarget
    {
        get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
        set
        {
            Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
        }
    }

    public override void AI()
    {
        Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();
        if (Projectile.spriteDirection == 1)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 * 2;
        else
            Projectile.rotation = Projectile.velocity.ToRotation();
        float maxDetectRadius = 450f;

        if (Projectile.timeLeft > 30)
        {
            HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

            if (HomingTarget == null)
                return;
            if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
            {
                HomingTarget = null;
                return;
            }

            float length = Projectile.velocity.Length();
            float targetAngle = Projectile.AngleTo(HomingTarget.Center);
            Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(4)).ToRotationVector2() * length;
            Projectile.Center += Main.rand.NextVector2Circular(1, 1);
        }
        else
        {
            Projectile.velocity *= 0f;
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 10)
            {
                Projectile.Resize(200, 200);

                Projectile.tileCollide = false;

                Projectile.position = Projectile.Center;
                Projectile.Center = Projectile.position;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        /*         SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

                 for (int i = 0; i < 5; i++)
                 {
                     Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass);
                     Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 0, default, 1.5f);
                 }*/
    }
}