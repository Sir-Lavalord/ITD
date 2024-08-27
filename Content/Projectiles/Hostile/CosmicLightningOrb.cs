using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;

namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicLightningOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_465";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.CultistBossLightningOrb];
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        private Player HomingTarget
        {
            get => Projectile.ai[2] == 0 ? null : Main.player[(int)Projectile.ai[2] - 1];
            set
            {
                Projectile.ai[2] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public int iStart;
        public bool bStop;
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

        }
        public override void AI()
        {
            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {
                if (HomingTarget == null)
                {
                    HomingTarget = Main.player[CosJel.target];
                }

                if (HomingTarget == null)
                {
                    return;
                }

                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                if (iStart++ >= 10 && !bStop)
                {
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(1)).ToRotationVector2() * length;
                    Projectile.Center += Main.rand.NextVector2Circular(2, 2);
                }
                else if (iStart >= Projectile.timeLeft * 5 / 6)
                {
                    bStop = true;
                }
            }
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.position = Projectile.oldPos[0];
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
