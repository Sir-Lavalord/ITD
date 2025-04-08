using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using ITD.Physics;
using Terraria.DataStructures;
using ITD.Content.Buffs.PetBuffs;
using Terraria.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ITD.Utilities;
using ITD.Content.Items.Weapons.Summoner;
using ITD.Content.Projectiles.Friendly.Summoner.ManuscriptUI;
using ITD.Content.Buffs.MinionBuffs;
using ITD.Players;
using SteelSeries.GameSense;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class ManuscriptDuelistProj : ModProjectile
    {
        private enum ActionState
        {
            Spawn,
            Idle,
            TargetFound,
            Dash
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[1] == 0 ? null : Main.npc[(int)Projectile.ai[1] - 1];
            set
            {
                Projectile.ai[1] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public int ShadowTier;
        public int TimeDash = 3;
        private ActionState AI_State { get { return (ActionState)Projectile.ai[0]; } set { Projectile.ai[0] = (float)value; } }
        private float lastDir;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.height = 80;
            Projectile.width = 50;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.timeLeft = 2;
            Projectile.minion = true;
        }
        public override void OnKill(int timeLeft)
        {
        }
        public Player player => Main.player[Projectile.owner];
        public void RegisterRightClick(Player player)
        {
            if (Main.mouseRight && player.GetModPlayer<WaxwellPlayer>().codexClickCD <= 0f)
            {
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            RegisterRightClick(player);
            HomingTarget ??= Projectile.FindClosestNPC(600);
            if (Projectile.Distance(player.Center) > 1000)
            {
                HomingTarget = null;
                AI_State = ActionState.Idle;
                Projectile.Center = player.Center;
                Projectile.netUpdate = true;
            }
/*            Main.NewText(AI_State);
*/            switch (AI_State)
            {
                case ActionState.Spawn:
                    SpawnBehavior();
                    break;
                case ActionState.Idle:
                    Projectile.rotation = Projectile.velocity.X / 5;
                    Projectile.frame = 0;
                    IdleBehavior();
                    Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();
                    break;
                case ActionState.TargetFound:
                    TargetFoundBehavior();
                    break;
                case ActionState.Dash:
                    DashBehavior();
                    break;
            }
            CheckActive(player);
        }
        private void SpawnBehavior()//Not onspawn
        {
            AI_State = ActionState.Idle;

        }
        Vector2 randomWander;
        int wanderTimer;
        private void IdleBehavior()//only overlap when no tree is found
        {
            Vector2 targetPoint = player.Center + new Vector2(lastDir * 128f, -64f);
            Vector2 toPlayer = targetPoint - Projectile.Center;
            Vector2 toPlayerNormalized = toPlayer.SafeNormalize(Vector2.Zero);
            float speed = toPlayer.Length();
            Projectile.velocity = toPlayerNormalized * (speed / 8);
            float overlapVelocity = 0.1f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X)
                    {
                        Projectile.velocity.X -= overlapVelocity;
                    }
                    else
                    {
                        Projectile.velocity.X += overlapVelocity;
                    }

                    if (Projectile.position.Y < other.position.Y)
                    {
                        Projectile.velocity.Y -= overlapVelocity;
                    }
                    else
                    {
                        Projectile.velocity.Y += overlapVelocity;
                    }
                }

            }
            if (HomingTarget == null)
                return;
            if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
            {
                HomingTarget = null;
                return;
            }

            AI_State = ActionState.TargetFound;

        }
        private void TargetFoundBehavior()
        {
            if (HomingTarget == null)
            {
                AI_State = ActionState.Idle;
                return;
            }
            if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
            {
                HomingTarget = null;
                AI_State = ActionState.Idle;
                return;
            }
            if (Projectile.localAI[1]++ >= 60)
            {
                AI_State = ActionState.Dash;
                Projectile.localAI[2]++;
                Vector2 toTarget = (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = toTarget * 10;
                Projectile.localAI[1] = 0;
                Projectile.rotation++;
            }
            else
            {
                Projectile.velocity *= 0.96f;
            }


        }
        public void DashBehavior()
        {
            Projectile.rotation++;
            if (Projectile.localAI[1]++ >= 20)
            {
                if (HomingTarget == null)
                {
                    AI_State = ActionState.Idle;
                    return;
                }
                if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
                {
                    HomingTarget = null;
                    AI_State = ActionState.Idle;
                    return;
                }
                Vector2 safeZone = HomingTarget.Center;
                const float safeRange = 180;
                Vector2 spawnPos = HomingTarget.Center + Main.rand.NextVector2Circular(200, 200);
                if (Vector2.Distance(safeZone, spawnPos) < safeRange)
                {
                    Vector2 directionOut = spawnPos - safeZone;
                    directionOut.Normalize();
                    spawnPos = safeZone + directionOut * Main.rand.NextFloat(safeRange, 200);
                }
                
                if (Projectile.localAI[2] >= TimeDash)
                {
                    Projectile.localAI[1] = 0;
                    Projectile.localAI[2] = 0;
                }
                else
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, Color.SandyBrown, 2f);
                        dust.velocity *= 1.2f;
                        dust.noGravity = true;
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        Dust dust = Dust.NewDustDirect(spawnPos, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, Color.SandyBrown, 2f);
                        dust.velocity *= 1.2f;
                        dust.noGravity = true;
                    }
                    Projectile.Center = spawnPos;
                    Projectile.localAI[1] = 20;

                }
                AI_State = ActionState.TargetFound;
            }
        }
        
        private bool CheckActive(Player player)
        {
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<ManuscriptDuelistBuff>());

                return false;
            }

            if (player.HasBuff(ModContent.BuffType<ManuscriptDuelistBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
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
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = Color.White * Projectile.Opacity * 0.75f * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}