﻿using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Audio;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Content.Items.Accessories.Combat.All;

namespace ITD.Content.Projectiles
{
    public class ITDInstancedGlobalProjectile : GlobalProjectile//there is now, thanks qDangle
    {
        public override bool InstancePerEntity => true;

        public enum ProjectileItemSource : byte
        {
            Potshot,
            TheEpicenter,
            Fwoomstick,
            Skyshot
        }
        public ProjectileItemSource ProjectileSource;
        private int ExplodeTimer = 0;
        public override void Load()
        {
            On_Main.GetProjectileDesiredShader += ITDProjectileShaderHook;
        }

        private static int ITDProjectileShaderHook(On_Main.orig_GetProjectileDesiredShader orig, Projectile proj)
        {
            int originalShader = orig(proj);
            if (proj.ModProjectile is ITDProjectile modProjectile)
                return modProjectile.ProjectileShader(originalShader);
            return originalShader;
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Player player = Main.player[projectile.owner];
            if (projectile.penetrate > 1)
            {
                if (player.GetModPlayer<OrionsRingPlayer>().orionsRingA)
                {
                    projectile.penetrate += player.GetModPlayer<OrionsRingPlayer>().activeBosses.Count;
                }
            }
        }

        public override void PostAI(Projectile projectile)
        {
			if (ProjectileSource == ProjectileItemSource.Fwoomstick)
            {
                ExplodeTimer++;
                if (ExplodeTimer > 45)
                {
                    projectile.Kill();
                }
            }

            if (ProjectileSource == ProjectileItemSource.Skyshot)
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

			if (ProjectileSource == ProjectileItemSource.Fwoomstick)
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

            if (ProjectileSource == ProjectileItemSource.Skyshot)
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
            if (ProjectileSource == ProjectileItemSource.Skyshot)
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
