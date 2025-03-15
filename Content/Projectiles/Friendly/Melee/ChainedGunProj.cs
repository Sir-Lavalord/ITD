using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using ITD.Utilities;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Friendly.Ranger;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class ChainedGunProj : ModProjectile
    {
        private const string ChainTexturePath = "ITD/Content/Projectiles/Friendly/Melee/ChainedGunChainA"; // The folder path to the flail chain sprite

        private static Asset<Texture2D> chainTexture;
        private static Asset<Texture2D> chainTextureExtra; 

        private enum AIState
        {
            Spinning,
            LaunchingForward,
            Retracting,
            ForcedRetracting,
            Ricochet,
            Barrage,
            Dropping
        }
        private AIState CurrentAIState
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        public ref float StateTimer => ref Projectile.ai[1];
        public ref float CurrentBullet => ref Projectile.ai[2];

        public float CollisionCounter;
        public ref float SpinningStateTimer => ref Projectile.localAI[1];
        private NPC HomingTarget
        {
            get => Projectile.localAI[0] == 0 ? null : Main.npc[(int)Projectile.localAI[0] - 1];
            set
            {
                Projectile.localAI[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public override void Load()
        {
            chainTexture = ModContent.Request<Texture2D>(ChainTexturePath);
            chainTextureExtra = ModContent.Request<Texture2D>(ChainTexturePath);
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;

            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
            {
                Projectile.Kill();
                return;
            }
            if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
            {
                Projectile.Kill();
                return;
            }

            Vector2 mountedCenter = player.MountedCenter;
            bool doFastThrowDust = false;
            bool shouldOwnerHitCheck = false;
            int launchTimeLimit = 15;
            float launchSpeed = 14f;
            float maxLaunchLength = 800f;
            float retractAcceleration = 3f;
            float maxRetractSpeed = 10f;
            float forcedRetractAcceleration = 6f;
            float maxForcedRetractSpeed = 15f;
            int defaultHitCooldown = 10;
            int spinHitCooldown = 20;
            int movingHitCooldown = 10;
            int ricochetTimeLimit = launchTimeLimit + 5;
            float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
            launchSpeed *= meleeSpeedMultiplier;
            retractAcceleration *= meleeSpeedMultiplier;
            maxRetractSpeed *= meleeSpeedMultiplier;
            forcedRetractAcceleration *= meleeSpeedMultiplier;
            maxForcedRetractSpeed *= meleeSpeedMultiplier;
            float launchRange = launchSpeed * launchTimeLimit;
            float maxDroppedRange = launchRange + 160f;
            Projectile.localNPCHitCooldown = defaultHitCooldown;
            float rotatard = 0;

            switch (CurrentAIState)
            {
                case AIState.Spinning:
                    {
                        shouldOwnerHitCheck = true;
                        Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);
                        offsetFromPlayer.Y *= 0.8f;
                        if (offsetFromPlayer.Y * player.gravDir > 0f)
                        {
                            offsetFromPlayer.Y *= 0.5f;
                        }
                        Projectile.Center = mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY);
                        rotatard += 0.25f;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            if (Projectile.localAI[2]++ % 3 == 0)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation) * 10,(int)Projectile.ai[2], Projectile.damage, Projectile.knockBack, Projectile.owner);

                                for (int i = 0; i < 6; i++)
                                {
                                    int dust = Dust.NewDust(mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY), 1, 1, DustID.Smoke, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust].noGravity = true;
                                    Main.dust[dust].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                    int dust1 = Dust.NewDust(mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY), 1, 1, DustID.Torch, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust1].noGravity = true;
                                    Main.dust[dust1].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                }
                                SoundEngine.PlaySound(SoundID.Item11, Projectile.position);
                                if (Main.rand.NextBool(3))
                                    player.PickAmmo(player.inventory[player.selectedItem], out int _, out float _, out int _, out float _, out int _);

                            }
                            Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                            player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
                            if (!player.channel)
                            {
                                Projectile.localAI[2] = 0;
                                CurrentAIState = AIState.LaunchingForward;
                                StateTimer = 0f;
                                Projectile.velocity = unitVectorTowardsMouse * launchSpeed + player.velocity;
                                Projectile.Center = mountedCenter;
                                Projectile.netUpdate = true;
                                Projectile.ResetLocalNPCHitImmunity();
                                Projectile.localNPCHitCooldown = movingHitCooldown;
                                break;
                            }
                        }
                        SpinningStateTimer += 1f;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.localNPCHitCooldown = spinHitCooldown;
                        break;
                    }
                case AIState.LaunchingForward:
                    {
                        doFastThrowDust = true;
                        bool shouldSwitchToBarrage = StateTimer++ >= launchTimeLimit;
                        shouldSwitchToBarrage |= Projectile.Distance(mountedCenter) >= maxLaunchLength;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            if (Projectile.localAI[2]++ % 6 == 0)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation) * 10,(int)Projectile.ai[2], Projectile.damage, Projectile.knockBack, Projectile.owner);

                                for (int i = 0; i < 6; i++)
                                {
                                    int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Smoke, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust].noGravity = true;
                                    Main.dust[dust].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                    int dust1 = Dust.NewDust(Projectile.Center, 1, 1, DustID.Torch, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust1].noGravity = true;
                                    Main.dust[dust1].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                }
                                SoundEngine.PlaySound(SoundID.Item11, Projectile.position);
                                if (Main.rand.NextBool(3))
                                    player.PickAmmo(player.inventory[player.selectedItem], out int _, out float _, out int _, out float _, out int _);
                            }
                        }
                        if (player.controlUseItem)
                        {
                            Projectile.localAI[2] = 0;
                            CurrentAIState = AIState.Dropping;
                            StateTimer = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                            break;
                        }
                        if (shouldSwitchToBarrage)
                        {
                            Projectile.localAI[2] = 0;
                            CurrentAIState = AIState.Barrage;
                            StateTimer = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.9f;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                        Projectile.localNPCHitCooldown = movingHitCooldown;
                        break;
                    }
                case AIState.Barrage:
                    {
                        bool shouldSwitchToRetract = StateTimer++ >= launchTimeLimit + 30;
                        Projectile.velocity *= 0.9f;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            if (Projectile.localAI[2]++ % 3 == 0)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation) * 10,(int)Projectile.ai[2], Projectile.damage, Projectile.knockBack, Projectile.owner);

                                for (int i = 0; i < 6; i++)
                                {
                                    int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Smoke, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust].noGravity = true;
                                    Main.dust[dust].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                    int dust1 = Dust.NewDust(Projectile.Center, 1, 1, DustID.Torch, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust1].noGravity = true;
                                    Main.dust[dust1].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                }
                                SoundEngine.PlaySound(SoundID.Item11, Projectile.position);
                                if (Main.rand.NextBool(3))
                                    player.PickAmmo(player.inventory[player.selectedItem], out int _, out float _, out int _, out float _, out int _);
                            }
                        }
                        if (shouldSwitchToRetract)
                        {
                            Projectile.localAI[2] = 0;
                            CurrentAIState = AIState.Retracting;
                            StateTimer = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.3f;
                        }
                        break;
                    }
                case AIState.Retracting:
                    {
                        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= maxRetractSpeed)
                        {
                            Projectile.Kill();
                            return;
                        }
                        if (player.controlUseItem)
                        {
                            CurrentAIState = AIState.Dropping;
                            StateTimer = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                        }
                        else
                        {
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxRetractSpeed, retractAcceleration);
                            player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                        }
                        break;
                    }
                case AIState.ForcedRetracting:
                    {
                        Projectile.tileCollide = false;
                        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= maxForcedRetractSpeed)
                        {
                            Projectile.Kill(); // Kill the projectile once it is close enough to the player
                            return;
                        }
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxForcedRetractSpeed, forcedRetractAcceleration);
                        Vector2 target = Projectile.Center + Projectile.velocity;
                        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                        if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f)
                        {
                            Projectile.Kill(); // Kill projectile if it will pass the player
                            return;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                        break;
                    }
                case AIState.Ricochet:
                    if (StateTimer++ >= ricochetTimeLimit)
                    {
                        CurrentAIState = AIState.Barrage;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.localNPCHitCooldown = movingHitCooldown;
                        Projectile.velocity.Y += 0.6f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    }
                    break;
                case AIState.Dropping:
                    if (!player.controlUseItem || Projectile.Distance(mountedCenter) > maxDroppedRange)
                    {
                        CurrentAIState = AIState.ForcedRetracting;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.velocity.Y += 0.8f;
                        Projectile.velocity.X *= 0.95f;
                        /*HomingTarget ??= Projectile.FindClosestNPC(600);
                        if (HomingTarget == null)
                        {
                            if (Projectile.velocity.Length() > 1f)
                                Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f; // skid
                            else
                                Projectile.rotation += Projectile.velocity.X * 0.1f; // roll
                            return;
                        }
                        if (!HomingTarget.active || HomingTarget.life <= 0 || !HomingTarget.CanBeChasedBy())
                        {
                            HomingTarget = null;
                            return;
                        }
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile.rotation = (HomingTarget.Center - Projectile.Center).ToRotation() - MathHelper.PiOver2;

                            if (Projectile.localAI[2]++ % 10 == 0)
                            {
                                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
                                (HomingTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.ToRadians(3)) * 22f,
                                (int)Projectile.ai[2], Projectile.damage, Projectile.knockBack, Projectile.owner, 1);
                                proj.CritChance = (int)player.GetTotalCritChance<RangedDamageClass>();
                                player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                                for (int i = 0; i < 6; i++)
                                {
                                    int dust = Dust.NewDust(Projectile.Center + new Vector2(0, player.gfxOffY), 1, 1, DustID.Smoke, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust].noGravity = true;
                                    Main.dust[dust].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                    int dust1 = Dust.NewDust(Projectile.Center + new Vector2(0, player.gfxOffY), 1, 1, DustID.Torch, 0f, 0f, 0, default, 1.1f);
                                    Main.dust[dust1].noGravity = true;
                                    Main.dust[dust1].velocity = Vector2.UnitX.RotatedBy(Projectile.rotation) * 20;
                                }
                                SoundEngine.PlaySound(SoundID.Item11, Projectile.position);
                                if (Main.rand.NextBool(3))
                                    player.PickAmmo(player.inventory[player.selectedItem], out int _, out float _, out int _, out float _, out int _);
                            }
                        }
                        }*/
                    }
                        break;
            }

            Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
            Projectile.spriteDirection = Projectile.direction;
            Projectile.ownerHitCheck = shouldOwnerHitCheck;
            if (CurrentAIState == AIState.Spinning)
            {
                Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver2;
            }
            else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Barrage)
            {
                Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                Projectile.rotation += 0.25f;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
            if (Projectile.Center.X < mountedCenter.X)
            {
                player.itemRotation += (float)Math.PI;
            }
            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
            int dustRate = 15;
            if (doFastThrowDust)
                dustRate = 1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int defaultLocalNPCHitCooldown = 10;
            int impactIntensity = 0;
            Vector2 velocity = Projectile.velocity;
            float bounceFactor = 0.2f;
            if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Ricochet)
            {
                bounceFactor = 0.4f;
            }

            if (CurrentAIState == AIState.Dropping)
            {
                bounceFactor = 0f;
            }

            if (oldVelocity.X != Projectile.velocity.X)
            {
                if (Math.Abs(oldVelocity.X) > 4f)
                {
                    impactIntensity = 1;
                }

                Projectile.velocity.X = (0f - oldVelocity.X) * bounceFactor;
                CollisionCounter += 1f;
            }

            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                if (Math.Abs(oldVelocity.Y) > 4f)
                {
                    impactIntensity = 1;
                }

                Projectile.velocity.Y = (0f - oldVelocity.Y) * bounceFactor;
                CollisionCounter += 1f;
            }
            if (CurrentAIState == AIState.LaunchingForward)
            {
                StateTimer = 0;
                CurrentAIState = AIState.Ricochet;
                Projectile.localNPCHitCooldown = defaultLocalNPCHitCooldown;
                Projectile.netUpdate = true;
                Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
                Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
                impactIntensity = 2;
                Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out bool causedShockwaves);
                Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
                Projectile.position -= velocity;
            }

            if (impactIntensity > 0)
            {
                Projectile.netUpdate = true;
                for (int i = 0; i < impactIntensity; i++)
                {
                    Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
                }

                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            }
            if (CurrentAIState != AIState.Spinning && CurrentAIState != AIState.Ricochet && CurrentAIState != AIState.Dropping && CurrentAIState != AIState.Barrage && CollisionCounter >= 10f)
            {
                CurrentAIState = AIState.ForcedRetracting;
                Projectile.netUpdate = true;
            }
            return false;
        }

        public override bool? CanDamage()
        {
            if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f)
            {
                return false;
            }
            return base.CanDamage();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CurrentAIState == AIState.Spinning)
            {
                Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
                Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
                shortestVectorFromPlayerToTarget.Y /= 0.8f;
                float hitRadius = 55f;
                return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (CurrentAIState == AIState.Spinning)
            {
                modifiers.SourceDamage *= 1.2f;
            }
            else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Retracting)
            {
                modifiers.SourceDamage *= 2f;
            }
            modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();
            if (CurrentAIState == AIState.Spinning)
            {
                modifiers.Knockback *= 0.25f;
            }
            else if (CurrentAIState == AIState.Dropping)
            {
                modifiers.Knockback *= 0.5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
            playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;
            Rectangle? chainSourceRectangle = null;
            float chainHeightAdjustment = 0f;
            Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
            Vector2 chainDrawPosition = Projectile.Center;
            Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
            Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
            float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
            if (chainSegmentLength == 0)
            {
                chainSegmentLength = 10;
            }
            float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
            int chainCount = 0;
            float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;
            while (chainLengthRemainingToDraw > 0f)
            {
                Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));
                var chainTextureToDraw = chainTexture;
                if (chainCount >= 4)
                {
                }
                else if (chainCount >= 2)
                {
                    chainTextureToDraw = chainTextureExtra;
                    byte minValue = 140;
                    if (chainDrawColor.R < minValue)
                        chainDrawColor.R = minValue;

                    if (chainDrawColor.G < minValue)
                        chainDrawColor.G = minValue;

                    if (chainDrawColor.B < minValue)
                        chainDrawColor.B = minValue;
                }
                else
                {
                    chainTextureToDraw = chainTextureExtra;
                }
                Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);
                chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
                chainCount++;
                chainLengthRemainingToDraw -= chainSegmentLength;
            }
            if (CurrentAIState == AIState.LaunchingForward)
            {
                Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
                Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
                SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                int afterimageCount = Math.Min(Projectile.oldPos.Length - 1, (int)StateTimer);
                for (int k = afterimageCount; k > 0; k--)
                {
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
                }
            }
            return true;
        }
    }
}