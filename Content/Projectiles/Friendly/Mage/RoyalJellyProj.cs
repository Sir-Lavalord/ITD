using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;
using Microsoft.Build.Evaluation;
using ITD.Content.Buffs.Debuffs;
using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectile;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class RoyalJellyProj : ModProjectile
    {
        public bool IsStickingToTarget
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }
        public int TargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public float StickTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public ParticleEmitter emitter;
        public int chosenFrame;
        int fakeTimer; //why
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;

            ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = 0;
            Projectile.friendly = true; 
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 900;
            Projectile.light = 0.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            emitter = ParticleSystem.NewEmitter<HoneyParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = Projectile;
        }
        bool grounded;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.Center + Projectile.velocity / 2, Projectile.velocity, Projectile.width, Projectile.height);
            Projectile.rotation = Main.rand.NextFloat(float.Pi);
            Projectile.velocity *= 0f;
            if (!grounded)
            {
                if (Main.rand.NextBool(2))
                {
                    chosenFrame = 4;
                }
                else chosenFrame = 0;
                Projectile.frame += chosenFrame;
                grounded = true;
                Projectile.timeLeft = 300;

                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            }
            
            for (int i = 0; i < 12; i++)
            {
                emitter?.Emit(Projectile.Center, (Projectile.velocity / 3).RotatedByRandom(4f) * Main.rand.NextFloat(0.9f, 1.1f));
            }
            return false;
        }
        Vector2 spawnvel;
        float spawnrotation;
        public override void OnSpawn(IEntitySource source)
        {
            spawnrotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;
            spawnvel = Projectile.velocity;
            spawnposdefault = Main.MouseWorld;

        }
        private const int StickTime = 60 * 16;
        //Example mod looking ass
        public override void AI()
        {
            if (emitter != null)
                emitter.keptAlive = true;

            if (IsStickingToTarget)
            {
                StickyAI();
            }
            else
            {
                NormalAI();
            }
            if (grounded || IsStickingToTarget)
            {
                if (Projectile.frameCounter++ >= 1)
                {
                    Projectile.frameCounter = 0;
                    if (chosenFrame == 0)
                    {
                        if (Projectile.frame < Main.projFrames[Projectile.type] - 4)
                        {
                            Projectile.frame++;
                        }
                    }
                    else
                    {
                        if (Projectile.frame < Main.projFrames[Projectile.type] - 1)
                        {
                            Projectile.frame++;
                        }
                    }
                    }
                //do everything above this line
                    float maxDetectRadius = 30;
                Player closestPlayer = FindClosestPlayer(maxDetectRadius);
                if (closestPlayer == null)
                    return;
                else
                {
                    if (closestPlayer.team != Main.player[Projectile.owner].team)
                    {

                    }
                    else
                    {
                        if (!closestPlayer.HasBuff(BuffID.Honey))
                        {
                            closestPlayer.AddBuff(BuffID.Honey, 300);

                            Projectile.Kill();
                        }
                        closestPlayer.honey = true;
                        if (!closestPlayer.HasBuff(ModContent.BuffType<RoyalJellyBuff>()))
                        {
                            closestPlayer.AddBuff(ModContent.BuffType<RoyalJellyBuff>(), 1200);
                            if (grounded)
                            {
                                closestPlayer.Heal(25);
                            }
                            if (IsStickingToTarget)
                            {
                                closestPlayer.Heal(100);

                            }
                        }
                    }
                }
            }
        }

        private void NormalAI()
        {
            if (!grounded)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 velo = Projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                    Vector2 veloDelta = Projectile.velocity;
                    emitter?.Emit(Projectile.Center + new Vector2(0f, Projectile.height / 2 - 14), ((velo * 1.25f) + veloDelta).RotatedByRandom(0.1f));
                }
                Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) + 1.57f;

            }
        }

        private void StickyAI()
        {
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            StickTimer += 1f;
            int npcTarget = TargetWhoAmI;
            if (StickTimer >= StickTime || npcTarget < 0 || npcTarget >= 200)
            {
                int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0, ModContent.ProjectileType<BigBlankExplosion>(), Projectile.damage, 0, Main.myPlayer, 0, 1);
                Main.projectile[projID].scale = 0.5f;
                Main.projectile[projID].CritChance = Projectile.CritChance;
                Projectile.Kill();
            }
            else if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage)
            {
                Projectile.Center = Main.npc[npcTarget].Center - Projectile.velocity * 2f;
                Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
            }
            else
            {
                Projectile.Kill();
            }
        }
        public override bool? CanDamage()
        {
            return !IsStickingToTarget;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);

            for (int i = 0; i < 10; i++)
            {
                emitter?.Emit(Projectile.Center, (Projectile.velocity/3).RotatedByRandom(4f) * Main.rand.NextFloat(0.9f,1.1f));
            }

        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 3;
            height = 3;
            Tile tile = Framing.GetTileSafely(Projectile.Center);
            fallThrough = spawnposdefault.Y >= Projectile.Bottom.Y && !grounded;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        Vector2 spawnposdefault;

        private const int MaxStickingJavelin = 4;
        private readonly Point[] stickingJavelins = new Point[MaxStickingJavelin];
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                emitter?.Emit(Projectile.Center, (Projectile.velocity/3).RotatedByRandom(4f) * Main.rand.NextFloat(0.9f,1.1f));
            }
            target.AddBuff(ModContent.BuffType<RoyalJellyDebuff>(), 600);
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            if (!IsStickingToTarget)
            {
                if (Main.rand.NextBool(2))
                {
                    chosenFrame = 4;
                }
                else chosenFrame = 0;
                Projectile.frame += chosenFrame;

                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                IsStickingToTarget = true;

            }
            TargetWhoAmI = target.whoAmI;
            Projectile.velocity = (target.Center - Projectile.Center) * 0.9f;
            Projectile.netUpdate = true;
            Projectile.KillOldestJavelin(Projectile.whoAmI, Type, target.whoAmI, stickingJavelins);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Width = 20;
            hitbox.Height = 20;
            hitbox.X += 12;
            hitbox.Y += 12;
            base.ModifyDamageHitbox(ref hitbox);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (IsStickingToTarget)
            {
                int npcIndex = TargetWhoAmI;
                if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
                {
                    if (Main.npc[npcIndex].behindTiles)
                    {
                        overPlayers.Add(index);
                    }
                    else
                    {
                        overPlayers.Add(index);
                    }

                    return;
                }
            }
            overPlayers.Add(index);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 6 && targetHitbox.Height > 6)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 6, -targetHitbox.Height / 6);
            }
            return projHitbox.Intersects(targetHitbox);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 center = Projectile.Size / 2f;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + center;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Vector2 origin = new(outline.Width * 0.5f, (outline.Height / Main.projFrames[Type]) * 0.5f);
                sb.Draw(outline, drawPos, frame, color, Projectile.oldRot[k], origin, Projectile.scale, SpriteEffects.None, 0f);
            }
            void DrawAtProj(Texture2D tex)
            {
                sb.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(tex.Width * 0.5f, (tex.Height / Main.projFrames[Type]) * 0.5f), Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtProj(outline));
            emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtProj(texture));
            return false;
        }
        public Player FindClosestPlayer(float maxDetectDistance)
        {
            Player closestTarget = null;
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, (int)maxDetectDistance, (int)maxDetectDistance)];            
                float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                if (sqrDistanceToTarget < sqrMaxDetectDistance)
                {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        closestTarget = target;
                }
            }

            return closestTarget;
        }
    }
}