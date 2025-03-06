using Terraria.ModLoader;
using Terraria;
using System;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Audio;

using ITD.Content.Projectiles.Other;
using ITD.Content.Projectiles.Friendly;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Friendly.Ranger;

namespace ITD.Content.Projectiles
{
    public class ITDInstancedGlobalProjectile : GlobalProjectile//fuck, there must be a better way to implement this
    {
        public override bool InstancePerEntity => true;

        public bool isFromPotshot;
        public bool isFromTheEpicenter;//I don't want to do this
        public bool isFromFwoomstick;
        public bool isFromSkyProjectileBow;

        private int ExplodeTimer = 0;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {

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
					for (int i = 0; i < 3; i++)
					{
						float speedX = Main.rand.NextFloat(-4f, 4f) + projectile.velocity.X;//Projectile.velocity.X * Main.rand.NextFloat(.4f, .7f) + Main.rand.NextFloat(-2f, 2f);
						float speedY = Main.rand.NextFloat(-4f, 4f) + projectile.velocity.Y;//Projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
						Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X, projectile.Center.Y, speedX, speedY, ModContent.ProjectileType<FwoomstickSpark>(), (int)(projectile.damage * 0.5), 0f, projectile.owner, 0f, 0f);
					}
				}
				for (int i = 0; i < 10; i++)
				{
					int dust = Dust.NewDust(projectile.Center, 1, 1, DustID.Torch, 0f, 0f, 0, default, 2f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity -= projectile.velocity * Main.rand.NextFloat(1f);
				}
				SoundEngine.PlaySound(SoundID.Item45, projectile.Center);
            }

            if (isFromSkyProjectileBow)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    int projectileAmount = Main.rand.Next(-7, 7);
                    int numberProjectiles = 16 + projectileAmount;
					Vector2 offset = new Vector2(-200f * projectile.direction, -600f);
					Vector2 toImpact = offset * -1f;
					toImpact.Normalize();
                    for (int index = 0; index < numberProjectiles; ++index)
                    {
                        Vector2 position = projectile.Center + offset + Main.rand.NextVector2Circular(400f, 400f);

                        Projectile.NewProjectile(projectile.GetSource_FromThis(), position, new Vector2(), ModContent.ProjectileType<SkyshooterFallingStar>(), 15, 0, Main.myPlayer, Main.rand.NextFloat(0.5f, 1f), toImpact.X, toImpact.Y);
                    }
                }
				 SoundEngine.PlaySound(SoundID.Item105, projectile.position);
            }
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (isFromSkyProjectileBow)
            {
                Texture2D texture = TextureAssets.Extra[91].Value;
                Rectangle rectangle = texture.Frame(1, 1);
                Vector2 position = projectile.Center - Main.screenPosition;
                Main.EntitySpriteDraw(texture, position, rectangle, new Color(200, 200, 200, 100), projectile.rotation, new Vector2(rectangle.Size().X / 2f, rectangle.Size().Y / 6f), 1f, SpriteEffects.None, 0f);
            }

            return true;
        }
    }
}
