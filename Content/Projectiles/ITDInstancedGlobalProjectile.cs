using ITD.Content.Projectiles.Other;
using ITD.Content.Projectiles.Friendly;
using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.DataStructures;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Friendly.Ranger;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using System.Security.Cryptography.X509Certificates;

namespace ITD.Content.Projectiles
{
    public class ITDInstancedGlobalProjectile : GlobalProjectile//fuck, there must be a better way to implement this
    {
        public override bool InstancePerEntity => true;

        public bool isFromPotshot;
        public bool isFromFwoomstick;
        public bool isFromSkyProjectileBow;

        private Color trailColor;
        private bool trailActive;

        private int ExplodeTimer = 0;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {

        }

        public void SetTrail(Color color)
        {
            if (isFromSkyProjectileBow)
            {
                trailColor = color;
                trailActive = true;
            }
        }

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

            if (isFromSkyProjectileBow)
            {
                Lighting.AddLight(projectile.Center, 2.5f, 2.5f, 2.5f);

                if (Main.rand.NextBool(2))
                {
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<CosJelDust>(), projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
                }
            }
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            Player player = Main.player[projectile.owner];

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

            if (isFromSkyProjectileBow)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    int projectileAmount = Main.rand.Next(-7, 7);
                    int numberProjectiles = 16 + projectileAmount;
                    for (int index = 0; index < numberProjectiles; ++index)
                    {
                        Vector2 vector2_1 = new Vector2((float)((double)projectile.position.X + (double)player.width * 0.5 + (double)(Main.rand.Next(201) * -player.direction) + ((double)Main.mouseX + (double)Main.screenPosition.X - (double)projectile.position.X)), (float)((double)projectile.position.Y + (double)player.height * 0.5 - 600.0));   //this defines the projectile width, direction and position
                        vector2_1.X = (float)(((double)vector2_1.X + (double)projectile.Center.X) / 2.0) + (float)Main.rand.Next(-200, 201);
                        vector2_1.Y -= (float)(100 * index);
                        float num12 = (float)Main.mouseX + Main.screenPosition.X - vector2_1.X;
                        float num13 = (float)Main.mouseY + Main.screenPosition.Y - vector2_1.Y;
                        if ((double)num13 < 0.0) num13 *= -1f;
                        if ((double)num13 < 20.0) num13 = 20f;
                        float num14 = (float)Math.Sqrt((double)num12 * (double)num12 + (double)num13 * (double)num13);
                        float num15 = 36 / num14;
                        float num16 = num12 * num15;
                        float num17 = num13 * num15;

                        Vector2 otherVelocity = projectile.velocity * 1.15f;
                        Vector2 newVelocity = otherVelocity.RotatedByRandom(MathHelper.ToRadians(15));
                        newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                        Projectile.NewProjectile(projectile.GetSource_FromThis(), vector2_1, newVelocity, ModContent.ProjectileType<SkyshooterFallingStar>(), 15, 0, Main.myPlayer, 0.0f, (float)Main.rand.Next(5));
                    }
                }
            }
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (isFromSkyProjectileBow)
            {
                Texture2D texture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/ArrowTrail").Value;
                Rectangle rectangle = texture.Frame(1, 1);
                Vector2 position = projectile.Center - Main.screenPosition;
                Main.EntitySpriteDraw(texture, position, rectangle, lightColor, projectile.rotation, rectangle.Size() / 2f, 1f, SpriteEffects.None, 0f);

                return true;
            }

            return true;
        }
    }
}
