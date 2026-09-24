using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp
{
    public class WispOrbSpawner : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;

        private float scaleX = 2f;
        private float scaleY = 2f;
        private float spawnGlow = 1f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
            Projectile.netImportant = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            Projectile.scale -= 0.025f;
            if (Projectile.scale <= 0)
            {
                Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[1] == 0) SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float fireAngle = Projectile.ai[0];
                float chevronIndex = Projectile.ai[1];

                float initialSpeed = 18f;
                Vector2 shootVel = fireAngle.ToRotationVector2() * initialSpeed;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, ModContent.ProjectileType<WispChevronBullet>(), Projectile.damage, 0, Main.myPlayer, chevronIndex);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ModContent.RequestIfExists<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing", out var effectTexture))
            {
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;


                Main.EntitySpriteDraw(
                    effectTexture.Value,
                    drawPosition,
                    null,
                    new Color(37, 255, 152, 0),
                    Projectile.rotation,
                    effectTexture.Value.Size() / 2f,
                    new Vector2(scaleX, scaleY) * Projectile.scale,
                    SpriteEffects.None,
                    0
                );

            }
            return false;
        }
    }
}