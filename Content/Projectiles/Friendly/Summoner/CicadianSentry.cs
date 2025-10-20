using ITD.Utilities;
using System;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Summoner;

public class CicadianSentry : ModProjectile
{
    public ref float AttackTimer => ref Projectile.ai[0];
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = 46;
        Projectile.height = 26;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Projectile.SentryLifeTime;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.sentry = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI()
    {
        NPC target;
        NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
        if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(this, false))
            target = ownerMinionAttackTargetNPC;
        else
            target = Projectile.FindClosestNPC(900f);
        if (target != null)
        {
            int toTarget = (target.Center.X - Projectile.Center.X > 0f).ToDirectionInt();
            double distance = Math.Abs(target.Center.X - Projectile.Center.X);
            if (distance > 300)
            {
                Projectile.velocity.X += toTarget * 0.2f; // approach slowly
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -2f, 2f);
                NPCHelpers.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height);

                if (Projectile.velocity.Y == 0) // hop
                {
                    Projectile.velocity.Y = -2f;
                }
            }
            else
            {
                Projectile.velocity.X *= 0.9f;
            }

            if (distance < 600) // snowballa
            {
                AttackTimer = ++AttackTimer % 60;
                if (AttackTimer == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
                    Vector2 toPlayer = Vector2.Normalize(target.Center - Projectile.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Snow);
                        d.scale *= 1.5f;
                        d.velocity = toPlayer - new Vector2(0, 1);
                        d.noGravity = true;
                    }
                    if (Main.myPlayer == Projectile.owner)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SummonIcyBoulder>(), Projectile.damage, 0.2f, Projectile.owner, target.whoAmI);
                    Projectile.velocity.X *= 0.2f;
                    Projectile.velocity.Y = -2f;
                }
            }


            Projectile.spriteDirection = toTarget;
        }
        else
        {
            Projectile.velocity.X *= 0.9f;
        }

        if (Projectile.velocity.Y < 16f)
        {
            Projectile.velocity.Y += 0.4f;
        }
    }

    public override bool? CanDamage() => false;
    public override bool OnTileCollide(Vector2 oldVelocity) => false;
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return true;
    }
}