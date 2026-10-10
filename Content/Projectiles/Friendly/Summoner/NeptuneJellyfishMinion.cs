using ITD.Content.Buffs.MinionBuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class NeptuneJellyfishMinion : ModProjectile
    {
        public VertexStrip TrailStrip = new();

        private readonly int PortalCooldownTimer = 60;
        private readonly int DashDurationTimer = 20;
        private readonly float EntryDashSpeed = 16f;
        private readonly float ExitDashSpeed = 30f;
        private readonly float IdleFlySpeed = 6f;
        private readonly float PushForce = 0.25f;
        private readonly float DetectionRange = 1200f;
        private readonly int FlingDuration = 30;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 54;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.scale = 0.75f;
            Projectile.timeLeft = 18000;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool MinionContactDamage()
        {
            return true;
        }

        public override bool? CanDamage()
        {
            if (Projectile.ai[0] != 2)
            {
                return false;
            }
            return base.CanDamage();
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
            {
                return;
            }

            if (Projectile.localAI[2] == 0)
            {
                Projectile.localAI[2] = FlingDuration;
            }

            Visuals();

            if (Projectile.localAI[2] > 1)
            {
                Projectile.localAI[2]--;
                Projectile.velocity *= 0.94f;
                float spinRate = (Projectile.localAI[2] / (float)FlingDuration) * 0.6f;
                Projectile.rotation += (Projectile.velocity.X > 0 ? spinRate : -spinRate);
                return;
            }

            FixOverlap();
            SearchForTargets(owner, out bool foundTarget, out NPC target);
            LeaderPortalLogic(foundTarget, target);
            MovementStateMachine(foundTarget, target, owner);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<NeptuneJellyfishBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<NeptuneJellyfishBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private void Visuals()
        {
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }

        private void FixOverlap()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (i != Projectile.whoAmI && other.active && other.type == Projectile.type && other.owner == Projectile.owner)
                {
                    if (Projectile.Hitbox.Intersects(other.Hitbox))
                    {
                        Vector2 push = Projectile.Center - other.Center;
                        if (push == Vector2.Zero) push = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                        Projectile.velocity += push.SafeNormalize(Vector2.Zero) * PushForce;
                    }
                }
            }
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out NPC target)
        {
            target = null;
            foundTarget = false;
            float closestDist = DetectionRange;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC n = Main.npc[owner.MinionAttackTargetNPC];
                if (n.CanBeChasedBy() && Projectile.Distance(n.Center) < DetectionRange * 1.5f)
                {
                    target = n;
                    foundTarget = true;
                    return;
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy() && Projectile.Distance(n.Center) < closestDist)
                {
                    closestDist = Projectile.Distance(n.Center);
                    target = n;
                    foundTarget = true;
                }
            }
        }

        private void LeaderPortalLogic(bool foundTarget, NPC target)
        {
            bool isLeader = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    if (i == Projectile.whoAmI) isLeader = true;
                    break;
                }
            }

            if (!isLeader) return;

            Projectile.localAI[0]++;

            if (foundTarget && target != null && Projectile.localAI[0] >= PortalCooldownTimer)
            {
                Vector2 predictedTargetPos = target.Center + target.velocity * 15f;

                Vector2 entryPos = Projectile.Center + Main.rand.NextVector2Circular(250f, 250f);
                Vector2 exitPos = predictedTargetPos + Main.rand.NextVector2Circular(250f, 250f);

                if (Vector2.Distance(entryPos, exitPos) < 300f)
                {
                    Vector2 push = (exitPos - entryPos).SafeNormalize(Main.rand.NextVector2CircularEdge(1f, 1f));
                    entryPos -= push * 150f;
                    exitPos += push * 150f;
                }

                int entryId = Projectile.NewProjectile(Projectile.GetSource_FromThis(), entryPos, Vector2.Zero, ModContent.ProjectileType<NeptunePortal>(), 0, 0, Projectile.owner);
                int exitId = Projectile.NewProjectile(Projectile.GetSource_FromThis(), exitPos, Vector2.Zero, ModContent.ProjectileType<NeptunePortal>(), 0, 0, Projectile.owner);

                Main.projectile[entryId].rotation = (exitPos - entryPos).ToRotation() + MathHelper.PiOver2;
                Main.projectile[exitId].rotation = (predictedTargetPos - exitPos).ToRotation() + MathHelper.PiOver2;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile minion = Main.projectile[i];
                    if (minion.active && minion.type == Projectile.type && minion.owner == Projectile.owner)
                    {
                        minion.ai[0] = 1;
                        minion.ai[1] = entryId;
                        minion.ai[2] = exitId;
                        minion.localAI[1] = 0;
                    }
                }
                Projectile.localAI[0] = 0;
            }
        }

        private void MovementStateMachine(bool foundTarget, NPC target, Player owner)
        {
            if (Projectile.ai[0] == 1)
            {
                Projectile entryPortal = Main.projectile[(int)Projectile.ai[1]];
                Projectile exitPortal = Main.projectile[(int)Projectile.ai[2]];

                if (entryPortal.active && entryPortal.type == ModContent.ProjectileType<NeptunePortal>())
                {
                    Vector2 toPortal = entryPortal.Center - Projectile.Center;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPortal.SafeNormalize(Vector2.Zero) * EntryDashSpeed, 0.15f);

                    if (Projectile.Hitbox.Intersects(entryPortal.Hitbox))
                    {
                        if (exitPortal.active)
                        {
                            Projectile.Center = exitPortal.Center;

                            Vector2 dashDir = (exitPortal.rotation - MathHelper.PiOver2).ToRotationVector2();
                            Projectile.velocity = dashDir * ExitDashSpeed;

                            SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);
                            for (int i = 0; i < 15; i++)
                            {
                                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonWater);
                            }

                            for (int i = 0; i < Projectile.oldPos.Length; i++)
                            {
                                Projectile.oldPos[i] = Projectile.position;
                                Projectile.oldRot[i] = Projectile.velocity.ToRotation();
                            }
                        }
                        Projectile.ai[0] = 2;
                    }
                }
                else
                {
                    Projectile.ai[0] = 0;
                }
            }
            else if (Projectile.ai[0] == 2)
            {
                if (foundTarget && target != null)
                {
                    Vector2 predictedTargetPos = target.Center + target.velocity * 10f;
                    Vector2 homingDir = (predictedTargetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, homingDir * Projectile.velocity.Length(), 0.08f);
                }

                Projectile.velocity *= 0.92f;
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] > DashDurationTimer)
                {
                    Projectile.ai[0] = 0;
                    Projectile.localAI[1] = 0;
                }
            }
            else
            {
                float timeOffset = Main.GlobalTimeWrappedHourly * 2f + (Projectile.whoAmI * 0.4f);
                float radius = 120f;

                Vector2 figure8Offset = new Vector2((float)Math.Cos(timeOffset) * radius, (float)Math.Sin(timeOffset * 2f) * (radius * 0.5f));

                Vector2 anchorPos = (foundTarget && target != null) ? target.Center : owner.Center + new Vector2(0, -80f);
                Vector2 idlePosition = anchorPos + figure8Offset;

                if (Projectile.Distance(owner.Center) > 2000f)
                {
                    Projectile.Center = owner.Center;
                    for (int i = 0; i < Projectile.oldPos.Length; i++)
                    {
                        Projectile.oldPos[i] = Projectile.position;
                        Projectile.oldRot[i] = Projectile.rotation;
                    }
                }

                Vector2 toIdle = idlePosition - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * IdleFlySpeed, 0.04f);
            }
        }

        private Color StripColors(float progressOnStrip)
        {
            return new Color(10, 100, 255, 10);
        }

        private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(20f, 2f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 1 && Projectile.Hitbox.Intersects(Main.projectile[(int)Projectile.ai[1]].Hitbox))
                return false;

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 center = Projectile.Size / 2f;

            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldRot[i] = Projectile.oldRot[i - 1];
                Projectile.oldRot[i] = Projectile.rotation;
            }

            GameShaders.Misc["LightDisc"].Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            Vector2 miragePos = Projectile.position - Main.screenPosition + center;
            Vector2 origin = new(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f);

            float time = Main.GlobalTimeWrappedHourly;
            float timer = (float)Main.time / 240f + time * 0.04f;
            time %= 4f; time /= 2f;
            if (time >= 1f) time = 2f - time;
            time = time * 0.5f + 0.5f;

            for (float i = 0f; i < 1f; i += 0.35f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;
                Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 6).RotatedBy(radians) * time, frame, new Color(10, 100, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float radians = (i + timer) * MathHelper.TwoPi;
                Main.EntitySpriteDraw(tex, miragePos + new Vector2(0f, 8).RotatedBy(radians) * time, frame, new Color(10, 100, 255, 50) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, miragePos, frame, Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}