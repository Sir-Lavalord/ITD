using ITD.Content.Projectiles.Friendly.Summoner.ManuscriptUI;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Buffs.PetBuffs;
using ITD.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using rail;
using Terraria.DataStructures;
using System;
using Terraria.Map;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra
{
    public class StabtrapStingerProjectile : ModProjectile
    {
            private enum ActionState
            {
                Waiting,
                Stabbing,
                Retract
            }
            private ActionState AIState { get { return (ActionState)Projectile.ai[0]; } set { Projectile.ai[0] = (float)value; } }
        public ref float AITimer => ref Projectile.ai[1];
        public float SpawnTimer = 60;

        private readonly Asset<Texture2D> chainSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/Snaptraps/Extra/StabtrapStingerChain");

        VerletChain TailChain;
        public Player player => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.height = 26;
            Projectile.width = 26;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        Projectile proj;

        public override void AI()
        {
            
            int byIdentity = MiscHelpers.GetProjectileByIdentity(Projectile.owner, (int)Projectile.localAI[0], ModContent.ProjectileType<StabtrapProjectile>());
            if (byIdentity == -1)
            {
                if (Projectile.owner == Main.myPlayer && Projectile.rotation > 0)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                proj = Main.projectile[byIdentity];
                Vector2 chainStart = Projectile.Center;
                if (TailChain != null)
                {

                    TailChain.Update(chainStart, proj.Center);
                }
                else
                {
                    TailChain = PhysicsMethods.CreateVerletChain(22, 10, chainStart, proj.Center, endLength: 0);
                }
                if (SpawnTimer-- == 60)
                    TailChain = PhysicsMethods.CreateVerletChain(22, 10, chainStart, proj.Center, endLength: 0);

            }
            if (!player.dead && player.ownedProjectileCounts[ModContent.ProjectileType<StabtrapProjectile>()] > 0)
            {
                Projectile.timeLeft = 2;
            }
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StabtrapProjectile>()] > 0)
            {
                Projectile.Kill();
            }
            NPC HomingTarget = Main.npc[(int)proj.ai[1]];

            if (HomingTarget == null)
            {
                return;
            }
            switch (AIState)
            {
                case ActionState.Waiting:
                    Vector2 velo = Vector2.Normalize(new Vector2(HomingTarget.Center.X, HomingTarget.Center.Y) - new Vector2(Projectile.Center.X, Projectile.Center.Y));
                    Projectile.rotation = velo.ToRotation() - MathHelper.Pi/2;

                    if (AITimer++ >= 60)
                    {
                        AITimer = 0;
                        AIState = ActionState.Stabbing;
                        float length = Projectile.velocity.Length();
                        float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                        Projectile.velocity = velo.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(3)).ToRotationVector2() * 10;
                    }
                    break;
                case ActionState.Stabbing:
                    if (AITimer++ >= 30)
                    {
                        AITimer = 0;
                        AIState = ActionState.Waiting;
                    }
                    else
                    {
                        Projectile.velocity *= 0.94f;
                    }
                    break;
                case ActionState.Retract:
                    float baseAccel = 0f;
                    float RetractAccel = 1.25f;
                    Vector2 towardsOwner = Projectile.DirectionTo(player.MountedCenter).SafeNormalize(Vector2.Zero);
                    RetractAccel += baseAccel;
                    Projectile.velocity = towardsOwner * RetractAccel;
                    Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                    if (Projectile.DistanceSQ(player.MountedCenter) <= RetractAccel * RetractAccel)
                    {
                        Projectile.Kill(); // Kill the projectile once it is close enough to the player
                    }
                    break;

            }
            Vector2 vectorToTrap= proj.Center - Projectile.Center;
            float distanceToTrap = vectorToTrap.Length();
            if (distanceToTrap > 150f || player.GetSnaptrapPlayer().GetActiveSnaptrap().retracting && distanceToTrap > 30f)
            {
                int where = Projectile.Center.X < proj.Center.X ? 1 : -1;
                int where2 = Projectile.Center.Y < proj.Center.Y ? 1 : -1;
                Projectile.velocity.Y += 0.6f * where2;
                Projectile.velocity.X += 0.6f * where;
            }
            Vector2 vectorToTarget = HomingTarget.Center - Projectile.Center;
            float distanceToTarget = vectorToTarget.Length();
            if (distanceToTarget < 10f)
            {
                int where = Projectile.Center.X < HomingTarget.Center.X ? 1 : -1;
                int where2 = Projectile.Center.Y < HomingTarget.Center.Y ? 1 : -1;
                Projectile.velocity.Y -= 0.6f * where2;
                Projectile.velocity.X -= 0.6f * where;
            }
                Vector2 directionToTarget = HomingTarget.Center - Projectile.Center;
                directionToTarget.Normalize();

        }
        public override void PostDraw(Color lightColor)
        {
            TailChain?.Draw(Main.spriteBatch, Main.screenPosition, chainSprite.Value, Color.White, true, null, null, chainSprite.Value);
        }
        public override void OnKill(int timeLeft)
        {
            TailChain?.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            NPC HomingTarget = Main.npc[(int)proj.ai[1]];
            if (HomingTarget == null)
            {
                return;
            }
            if (target == HomingTarget)
            {
                if (AIState == ActionState.Stabbing)
                {
                    Projectile.velocity = -Projectile.velocity;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (AIState == ActionState.Stabbing)
            {
                for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
                {
                    Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Vector2 value4 = Projectile.oldPos[i];
                    float num165 = Projectile.oldRot[i];
                    Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
                }
            }
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.FlipVertically, 0);
            return false;
        }
    }
}