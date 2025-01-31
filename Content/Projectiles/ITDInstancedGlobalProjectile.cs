using ITD.Content.Projectiles.Other;
using Terraria.ModLoader;
using Terraria;
using System;

namespace ITD.Content.Projectiles
{
    public class ITDInstancedGlobalProjectile : GlobalProjectile//fuck, there must be a better way to implement this
    {
        public override bool InstancePerEntity => true;

        public bool isFromPotshot;
        public bool isFromFwoomstick;
        public bool isFromQuasarBow;

        private int ExplodeTimer = 0;

        public override void PostAI(Projectile projectile)
        {
            if (isFromFwoomstick)
            {
                ExplodeTimer++;
                if (ExplodeTimer > 45)
                {
                    projectile.Kill();
                }
            }
        }
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (isFromFwoomstick)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float speedX = projectile.velocity.X * Main.rand.NextFloat(.4f, .7f) + Main.rand.NextFloat(-2f, 2f);
                        float speedY = projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;

                        Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + speedX, projectile.position.Y + speedY, speedX, speedY, ModContent.ProjectileType<FwoomstickSpark>(), (int)(projectile.damage * 0.5), 0f, projectile.owner, 0f, 0f);
                    }
                }
            }
        }
    }
}
