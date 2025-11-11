using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class BigBerthProj : ModProjectile
{
    private static Asset<Texture2D> chainTexture;
    private readonly Asset<Texture2D> gun = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/BigBerthProj_Gun");

    private enum AIState
    {
        Spinning,
        LaunchingForward,
        Retracting,
        UnusedState,
        ForcedRetracting,
        Ricochet,
        Dropping
    }

    // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
    private AIState CurrentAIState
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }
    public ref float StateTimer => ref Projectile.ai[1];
    public bool Dropped => Projectile.ai[2] != 0;

    public ref float CollisionCounter => ref Projectile.localAI[0];
    public ref float SpinningStateTimer => ref Projectile.localAI[1];

    public override void Load()
    {
        chainTexture = ModContent.Request<Texture2D>(Texture + "ChainA");
    }

    public override void SetStaticDefaults()
    {
        // These lines facilitate the trail drawing
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
        Projectile.width = 48; // The width of your projectile
        Projectile.height = 48; // The height of your projectile
        Projectile.friendly = true; // Deals damage to enemies
        Projectile.penetrate = -1; // Infinite pierce
        Projectile.DamageType = DamageClass.Melee; // Deals melee damage
        Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
        Projectile.localNPCHitCooldown = 10; // This facilitates custom hit cooldown logic

        // Vanilla flails all use aiStyle 15, but the code isn't customizable so an adaption of that aiStyle is used in the AI method
    }

    // This AI code was adapted from vanilla code: Terraria.Projectile.AI_015_Flails() 
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        // Kill the projectile if the player dies or gets crowd controlled
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
        bool shouldOwnerHitCheck = false;
        int launchTimeLimit = 35;  // How much time the projectile can go before retracting (speed and shootTimer will set the flail's range)
        float launchSpeed = 10f; // How fast the projectile can move
        float maxLaunchLength = 1600f; // How far the projectile's chain can stretch before being forced to retract when in launched state
        float retractAcceleration = 1.5f; // How quickly the projectile will accelerate back towards the player while retracting
        float maxRetractSpeed = 10f; // The max speed the projectile will have while retracting
        float forcedRetractAcceleration = 3f; // How quickly the projectile will accelerate back towards the player while being forced to retract
        float maxForcedRetractSpeed = 10f; // The max speed the projectile will have while being forced to retract
        float unusedRetractAcceleration = 1f;
        float unusedMaxRetractSpeed = 14f;
        int defaultHitCooldown = 10; // How often your flail hits when resting on the ground, or retracting
        int spinHitCooldown = 20; // How often your flail hits when spinning
        int movingHitCooldown = 10; // How often your flail hits when moving
        int ricochetTimeLimit = launchTimeLimit + 5;

        // Scaling these speeds and accelerations by the players melee speed makes the weapon more responsive if the player boosts it or general weapon speed
        float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
        launchSpeed *= meleeSpeedMultiplier;
        retractAcceleration *= meleeSpeedMultiplier;
        maxRetractSpeed *= meleeSpeedMultiplier;
        forcedRetractAcceleration *= meleeSpeedMultiplier;
        maxForcedRetractSpeed *= meleeSpeedMultiplier;
        float launchRange = launchSpeed * launchTimeLimit;
        float maxDroppedRange = launchRange + 160f;
        Projectile.localNPCHitCooldown = defaultHitCooldown;

        switch (CurrentAIState)
        {
            case AIState.Spinning:
                {
                    shouldOwnerHitCheck = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                        player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
                        if (!player.channel) // If the player releases then change to moving forward mode
                        {
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
                    // This line creates a unit vector that is constantly rotated around the player. 10f controls how fast the projectile visually spins around the player
                    Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 4f * (SpinningStateTimer / 60f) * player.direction);

                    offsetFromPlayer.Y *= 0.8f;
                    if (offsetFromPlayer.Y * player.gravDir > 0f)
                    {
                        offsetFromPlayer.Y *= 0.5f;
                    }
                    Projectile.Center = mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY);
                    Projectile.velocity = Vector2.Zero;
                    Projectile.localNPCHitCooldown = spinHitCooldown; // set the hit speed to the spinning hit speed
                    break;
                }
            case AIState.LaunchingForward:
                {
                    Projectile.Resize(48, 48);
                    bool shouldSwitchToRetracting = StateTimer++ >= launchTimeLimit;
                    shouldSwitchToRetracting |= Projectile.Distance(mountedCenter) >= maxLaunchLength;
                    if (player.controlUseItem) // If the player clicks, transition to the Dropping state
                    {
                        CurrentAIState = AIState.Dropping;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0.2f;
                        // This is where Drippler Crippler spawns its projectile
                        /*
							if (Main.myPlayer == Projectile.owner)
								Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Projectile.velocity, 928, Projectile.damage, Projectile.knockBack, Main.myPlayer);
							*/
                        break;
                    }
                    if (shouldSwitchToRetracting)
                    {
                        CurrentAIState = AIState.Retracting;
                        StateTimer = 0f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0.3f;
                        // This is also where Drippler Crippler spawns its projectile, see above code.
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    Projectile.localNPCHitCooldown = movingHitCooldown;
                    break;
                }
            case AIState.Retracting:
                {
                    Projectile.Resize(48, 48);

                    Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                    if (Projectile.Distance(mountedCenter) <= maxRetractSpeed)
                    {
                        Projectile.Kill(); // Kill the projectile once it is close enough to the player
                        return;
                    }
                    if (player.controlUseItem) // If the player clicks, transition to the Dropping state
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
                    Projectile.Resize(48, 48);

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
            case AIState.Dropping:
                Projectile.Resize(48, 48);
                if (Dropped)
                {
                    if (StateTimer++ == 15)
                    {
                        Vector2 vel = Vector2.Normalize(Main.MouseWorld - Projectile.Center) * 18;
                        Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(40, -40).RotatedBy(Projectile.rotation),
        vel, ModContent.ProjectileType<BigBerthShell>(), Projectile.damage, 3, -1);
                    }
                }
                if (CollisionCounter++ % 10 == 0 && !Dropped)
                {
                    impactIntensity++;
                }
                if (!player.controlUseItem && Dropped|| Projectile.Distance(mountedCenter) > maxDroppedRange)
                {
                    CurrentAIState = AIState.ForcedRetracting;
                    StateTimer = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Projectile.velocity.Y += 0.8f;
                    Projectile.velocity.X *= 0.95f;
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                }
                break;
        }

        // This is where Flower Pow launches projectiles. Decompile Terraria to view that code.

        Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
        Projectile.spriteDirection = Projectile.direction;
        Projectile.ownerHitCheck = shouldOwnerHitCheck; // This prevents attempting to damage enemies without line of sight to the player. The custom Colliding code for spinning makes this necessary.

        // This rotation code is unique to this flail, since the sprite isn't rotationally symmetric and has tip.
        bool freeRotation = CurrentAIState == AIState.Ricochet || CurrentAIState == AIState.Dropping;
        if (!Dropped)
        {
            if (freeRotation)
            {
                Vector2 vectorTowardsPlayer = Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero);
                Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver4;
            }
        }
        Projectile.timeLeft = 2; // Makes sure the flail doesn't die (good when the flail is resting on the ground)
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(2); // Add a delay so the player can't button mash the flail
        player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
        if (Projectile.Center.X < mountedCenter.X)
        {
            player.itemRotation += (float)Math.PI;
        }
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

    }
    int impactIntensity = 0;

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        int defaultLocalNPCHitCooldown = 10;
        Vector2 velocity = Projectile.velocity;
        float bounceFactor = 0.2f;
        if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Ricochet)
        {
            bounceFactor = 0.4f;
        }
        if (CurrentAIState == AIState.Dropping)
        {
            if (!Dropped)
            {
                SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
                StateTimer = 0f;
                float width = 250 * impactIntensity;
                float height = 60;
                Projectile explode = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Bottom - new Vector2(width/3f,0),
                    Vector2.Zero, ModContent.ProjectileType<BigBerthExplosion>(), Projectile.damage,0);
                explode.width = (int)width;
                explode.height = (int)height;
                Projectile.ai[2]++;
                for (int j = 0; j < 20 * impactIntensity; j++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Bottom + new Vector2(-30, 0), Projectile.width * 2, 10, DustID.Stone);
                    dust.noGravity = true;
                    dust.scale = 1.5f * Main.rand.NextFloat(1.25f, 2f);
                    dust.velocity.X = 4 * impactIntensity * (j % 2 == 0 ? 1 : -1) * Main.rand.NextFloat(0.25f, 2f);
                }
                for (int i = 0; i < 5 * impactIntensity; i++) 
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, 0, -Projectile.velocity.Y/4, 60, default, Main.rand.NextFloat(1f, 1.7f));
                    dust.noGravity = false;
                    dust.velocity *= impactIntensity * Main.rand.NextFloat(0.75f, 1.25f);
                    dust.velocity = dust.velocity.RotatedByRandom(MathHelper.ToRadians(20));
                    dust.scale = 1.5f * Main.rand.NextFloat(0.5f, 1.5f);

                }
            }
            bounceFactor = 0f;
        }

        // If in the Launched state, spawn sparks
        if (CurrentAIState == AIState.LaunchingForward)
        {
            CurrentAIState = AIState.Retracting;
            Projectile.localNPCHitCooldown = defaultLocalNPCHitCooldown;
            Projectile.netUpdate = true;
            Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
            Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
            impactIntensity = 2;
            Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out bool causedShockwaves);
            Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
            Projectile.position -= velocity;
        }

/*        // Here the tiles spawn dust indicating they've been hit
        if (impactIntensity > 0)
        {
            Projectile.netUpdate = true;
            for (int i = 0; i < impactIntensity; i++)
            {
                Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
            }

            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }
*/
        // Force retraction if stuck on tiles while retracting
        if (CurrentAIState != AIState.UnusedState && CurrentAIState != AIState.Spinning && CurrentAIState != AIState.Ricochet && CurrentAIState != AIState.Dropping && CollisionCounter >= 10f)
        {
            CurrentAIState = AIState.ForcedRetracting;
            Projectile.netUpdate = true;
        }

        // tModLoader currently does not provide the wetVelocity parameter, this code should make the flail bounce back faster when colliding with tiles underwater.
        //if (Projectile.wet)
        //	wetVelocity = Projectile.velocity;

        return false;
    }

    public override bool? CanDamage()
    {
        // Flails in spin mode won't damage enemies within the first 12 ticks. Visually this delays the first hit until the player swings the flail around for a full spin before damaging anything.
        if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f)
        {
            return false;
        }
        return base.CanDamage();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        // Flails do special collision logic that serves to hit anything within an ellipse centered on the player when the flail is spinning around the player. For example, the projectile rotating around the player won't actually hit a bee if it is directly on the player usually, but this code ensures that the bee is hit. This code makes hitting enemies while spinning more consistent and not reliant of the actual position of the flail projectile.
        if (CurrentAIState == AIState.Spinning)
        {
            Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            shortestVectorFromPlayerToTarget.Y /= 0.8f; // Makes the hit area an ellipse. Vertical hit distance is smaller due to this math.
            float hitRadius = 55f; // The Length of the semi-major radius of the ellipse (the long end)
            return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
        }
        // Regular collision logic happens otherwise.
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // Flails do a few custom things, you'll want to keep these to have the same feel as vanilla flails.

        // Flails do 20% more damage while spinning
        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.SourceDamage *= 1.2f;
        }
        // Flails do 100% more damage while launched or retracting. This is the damage the item tooltip for flails aim to match, as this is the most common mode of attack. This is why the item has ItemID.Sets.ToolTipDamageMultiplier[Type] = 2f;
        else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Retracting)
        {
            modifiers.SourceDamage *= 2f;
        }

        // The hitDirection is always set to hit away from the player, even if the flail damages the npc while returning
        modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

        // Knockback is only 25% as powerful when in spin mode
        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.Knockback *= 0.25f;
        }
        // Knockback is only 50% as powerful when in drop down mode
        else if (CurrentAIState == AIState.Dropping)
        {
            modifiers.Knockback *= 0.5f;
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        //Almost No tile collide
        width = 30;
        height = 30;
        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = gun.Value;
        Vector2 gunOrigin = new(texture.Width * 0.5f, texture.Width * 0.5f);
        Vector2 drawCenter = Projectile.Center - Main.screenPosition + new Vector2(18, -20).RotatedBy(Projectile.rotation);
        Dust dust = Dust.NewDustPerfect(drawCenter, DustID.AmberBolt, Vector2.Zero, Scale: 1f);
        dust.noGravity = true;
        Main.EntitySpriteDraw(texture, drawCenter, null, Color.White, Projectile.rotation, gunOrigin, Projectile.scale, SpriteEffects.None, 0);

        Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);

        // This fixes a vanilla GetPlayerArmPosition bug causing the chain to draw incorrectly when stepping up slopes. The flail itself still draws incorrectly due to another similar bug. This should be removed once the vanilla bug is fixed.
        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        Rectangle? chainSourceRectangle = null;
        // Drippler Crippler customizes sourceRectangle to cycle through sprite frames: sourceRectangle = asset.Frame(1, 6);
        float chainHeightAdjustment = 0f; // Use this to adjust the chain overlap. 

        Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
        Vector2 chainDrawPosition = Projectile.Center;
        Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
        Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
        float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
        if (chainSegmentLength == 0)
        {
            chainSegmentLength = 10; // When the chain texture is being loaded, the height is 0 which would cause infinite loops.
        }
        float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
        int chainCount = 0;
        float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;

        // This while loop draws the chain texture from the projectile to the player, looping to draw the chain texture along the path
        while (chainLengthRemainingToDraw > 0f)
        {
            // This code gets the lighting at the current tile coordinates
            Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));

            // Flaming Mace and Drippler Crippler use code here to draw custom sprite frames with custom lighting.
            // Cycling through frames: sourceRectangle = asset.Frame(1, 6, 0, chainCount % 6);
            // This example shows how Flaming Mace works. It checks chainCount and changes chainTexture and draw color at different values

            var chainTextureToDraw = chainTexture;
            if (chainCount >= 4)
            {
                // Use normal chainTexture and lighting, no changes
            }
            else
            {
                // Close to the ball, we draw a custom chain texture and draw it at full brightness glow.
                chainDrawColor = Color.White;
            }

            // Here, we draw the chain texture at the coordinates
            Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

            // chainDrawPosition is advanced along the vector back to the player by the chainSegmentLength
            chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
            chainCount++;
            chainLengthRemainingToDraw -= chainSegmentLength;
        }

        // Add a motion trail when moving forward, like most flails do (don't add trail if already hit a tile)
        if (CurrentAIState == AIState.Dropping)
        {
            Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int afterimageCount = Math.Min(Projectile.oldPos.Length - 1, (int)StateTimer);
            for (int k = afterimageCount; k > 0; k--)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
            }
        }
        return true;
    }
}