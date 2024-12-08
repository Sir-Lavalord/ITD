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

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;

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
            Projectile.alpha = 255;
            Projectile.light = 0.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }
        bool grounded;
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.Center + Projectile.velocity / 2, Projectile.velocity, Projectile.width, Projectile.height);
            Projectile.rotation = Main.rand.NextFloat(float.Pi);
            Projectile.velocity *= 0f;
            if (!grounded)
            {
                Projectile.frame = Main.rand.Next(1, 3);
                Projectile.timeLeft = 300;
                grounded = true;

                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            }
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.Honey, 0, 0, 60, default, Main.rand.NextFloat(1f, 1.7f));
                dust.noGravity = true;
                dust.velocity *= 4f;
                Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.Honey, 0, 0, 60, default, Main.rand.NextFloat(1f, 1.7f));
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

            UpdateAlpha();
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
                Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) + 1.57f;
                for (int i = 0; i < 6; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center, Projectile.width / 4, Projectile.height / 4, DustID.Honey, 0, 0, 60, default, Main.rand.NextFloat(0.4f, 1.2f));
                    dust.noGravity = false;
                    dust.velocity *= 1f;
                }
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

            for (int i = 0; i < 16; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Honey, 0, 0, 60, default, Main.rand.NextFloat(1f, 1.7f));
                dust.noGravity = true;
                dust.velocity *= 10f;
                Dust.NewDustDirect(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Honey2, 0, 0, 60, default, Main.rand.NextFloat(1f, 1.7f));
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

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, Projectile.width, Projectile.height, DustID.Honey, 0, 0, 80, default, Main.rand.NextFloat(1, 1.8f));
                dust.noGravity = true;
                dust.velocity *= 4f;
            }
            target.AddBuff(ModContent.BuffType<RoyalJellyDebuff>(), 600);
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            if (!IsStickingToTarget)
            {
                Projectile.frame = Main.rand.Next(1, 3);

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
        private const int AlphaFadeInSpeed = 25;

        private void UpdateAlpha()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= AlphaFadeInSpeed;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
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