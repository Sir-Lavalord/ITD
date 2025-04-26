using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ReLogic.Content;
using ITD.Content.NPCs.Bosses;
using SteelSeries.GameSense;
using Microsoft.Build.Evaluation;
using Terraria.DataStructures;
using ITD.Utilities;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class MangledSheerthornProj : ModProjectile
    {
        bool isStuck;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
            Main.projFrames[Projectile.type] = 5;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22; Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 1200;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.alpha = 80;
            DrawOffsetX = -16;
/*            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;*/
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (int)Projectile.ai[0];
            if (Projectile.ai[0] == 4)//rare
            {
                if (Main.rand.NextBool(4))
                { }
                else
                Projectile.ai[0] -= Main.rand.Next(1, 4);
            }
        }
        public override void AI()
        {
            Projectile.frame = (int)Projectile.ai[0];

            if (!isStuck)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();

                if (Projectile.ai[1]++ >= 15)
                {
                    Projectile.velocity.Y += 0.25f;
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!isStuck)
            {
                Projectile.rotation = Projectile.oldVelocity.ToRotation();
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.SilverBulletSparkle,
                                    new ParticleOrchestraSettings { PositionInWorld = Projectile.Center }, Projectile.owner);
                for (int i = 0; i < 1; i++)
                {
                    Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
                }

                for (int i = 0; i < 1; i++)
                {
                    Vector2 randVelocity = oldVelocity * 0.5f * Main.rand.NextFloat();
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Copper, randVelocity.X, randVelocity.Y, 150, default(Color), 2f);
                    dust.noGravity = true;
                    dust.velocity *= 2f;
                }
                isStuck = true;
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            }
            Projectile.velocity.X *= 0f;
            Projectile.netUpdate = true;
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 8;
            height = 8;
            fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = new(texture.Width * 0.5f, (texture.Height / Main.projFrames[Type]) * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 center = Projectile.Size / 2f;
                Vector2 drawPosition = Projectile.oldPos[k] - Main.screenPosition + center;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Vector2 draworigin = new(texture.Width * 0.5f, (texture.Height / Main.projFrames[Type]) * 0.5f);
                sb.Draw(texture, drawPosition, frame, color, Projectile.oldRot[k], draworigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.spriteBatch.Draw(texture, drawPos, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Copper, 0, 0, 150, default(Color), 2f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        }
    }
}
