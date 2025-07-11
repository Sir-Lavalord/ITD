using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Utilities;
using System;
using Terraria.DataStructures;
using ITD.Utilities.EntityAnim;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicJellyfishBlast : BigBlankExplosion
    {
        public override int Lifetime => 250;
        public override Vector2 ScaleRatio => new Vector2(1.5f, 1f);

        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.Gray * 1.6f, Color.Purple, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
        }
        public override string Texture => ITD.BlankTexture;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }
        public float ProgressZeroToOne => (Utils.GetLerpValue(Lifetime, 0f, Projectile.timeLeft, true));

        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
            }
        }
        public override void AI()
        {
            if (CurrentRadius >= MaxRadius * 0.99f)
                Projectile.Kill();
            CurrentRadius = MathHelper.Lerp(CurrentRadius, MaxRadius, EasingFunctions.OutQuad(ProgressZeroToOne));
            Projectile.scale = MathHelper.Lerp(1.2f, 5f, EasingFunctions.OutQuad(ProgressZeroToOne));
            Projectile.ExpandHitboxBy((int)(CurrentRadius * Projectile.scale), (int)(CurrentRadius * Projectile.scale));
        }
        public override void OnKill(int timeLeft)
        {
            int projectileAmount = Main.rand.Next(40, 46);
            float radius = 10f;
            float sector = (float)(MathHelper.TwoPi);
            float sectorOfSector = sector / projectileAmount;
            float startAngle = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                    for (int i = 0; i < projectileAmount; i++)
                    {
                    float angle = startAngle + sectorOfSector * i;
                    Vector2 projectileVelo = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    float rotation = 0 + (float)Math.PI * 2 / (projectileAmount) * i;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + MaxRadius * Vector2.UnitX.RotatedBy(rotation), projectileVelo,
                            ModContent.ProjectileType<CosmicVoidShard>(), 20, 0f, Main.myPlayer);
                    }
            }
            base.OnKill(timeLeft);
        }
        public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
    }
}