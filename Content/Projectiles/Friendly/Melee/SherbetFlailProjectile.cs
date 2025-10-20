using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.GameContent;
//using ITD.Particles.Projectile;
//using ITD.Particles;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class SherbetFlailProjectile : ModProjectile
{
    private static Asset<Texture2D> chainTexture;

    private enum AIState
    {
        Spinning,
        LaunchingForward,
        Retracting
    }

    private AIState CurrentAIState
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }
    public ref float StateTimer => ref Projectile.ai[1];
    public ref float ChargeLevel => ref Projectile.ai[2];
    public ref float CollisionCounter => ref Projectile.localAI[0];
    public ref float SpinningStateTimer => ref Projectile.localAI[1];

    //public ParticleEmitter emitter;

    public override void Load()
    {
        chainTexture = ModContent.Request<Texture2D>(Texture + "Chain");
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        //emitter = ParticleSystem.NewEmitter<SherbetFlash>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        //emitter.tag = Projectile;
    }

    public override void AI()
    {
        //if (emitter != null)
        //    emitter.keptAlive = true;

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
        int launchTimeLimit = 20;  // How much time the projectile can go before retracting (speed and shootTimer will set the flail's range)
        float launchSpeed = 20f; // How fast the projectile can move
        float maxRetractSpeed = 30f; // The max speed the projectile will have while retracting
        float retractAcceleration = 5f; // How quickly the projectile will accelerate back towards the player while being forced to retract
        int defaultHitCooldown = 10; // How often your flail hits when resting on the ground, or retracting
        int spinHitCooldown = 20; // How often your flail hits when spinning
        int movingHitCooldown = 10; // How often your flail hits when moving

        // Scaling these speeds and accelerations by the players melee speed makes the weapon more responsive if the player boosts it or general weapon speed
        float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
        launchSpeed *= meleeSpeedMultiplier;
        maxRetractSpeed *= meleeSpeedMultiplier;
        retractAcceleration *= meleeSpeedMultiplier;
        float launchRange = launchSpeed * launchTimeLimit;
        Projectile.localNPCHitCooldown = defaultHitCooldown;

        switch (CurrentAIState)
        {
            case AIState.Spinning:
                {
                    Projectile.tileCollide = false;
                    shouldOwnerHitCheck = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                        player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
                        if (!player.channel && ChargeLevel > 0) // If the player releases then change to moving forward mode
                        {
                            CurrentAIState = AIState.LaunchingForward;
                            StateTimer = 0f;
                            Projectile.velocity = unitVectorTowardsMouse * launchSpeed + (player.velocity * 0.5f);
                            Projectile.Center = mountedCenter;
                            Projectile.netUpdate = true;
                            Projectile.ResetLocalNPCHitImmunity();
                            Projectile.localNPCHitCooldown = movingHitCooldown;
                            SoundEngine.PlaySound(SoundID.Item69, player.Center);
                            break;
                        }
                    }
                    SpinningStateTimer += 1f;
                    // This line creates a unit vector that is constantly rotated around the player. 10f controls how fast the projectile visually spins around the player
                    Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);
                    if (SpinningStateTimer % 25 == 0 && ChargeLevel < 3f)
                    {
                        ChargeLevel++;
                        SoundEngine.PlaySound(SoundID.Item72, player.Center);
                        for (int i = 0; i < 3; i++)
                        {
                            Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Firework_Pink, 0, 0, 150, default, 1f + (0.5f * ChargeLevel));
                            dust.noGravity = true;
                            dust.velocity *= 2f;
                        }
                    }

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
                    Projectile.tileCollide = true;
                    bool shouldSwitchToRetracting = StateTimer++ >= launchTimeLimit;
                    if (shouldSwitchToRetracting)
                    {
                        CurrentAIState = AIState.Retracting;
                        StateTimer = 3f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0f;
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    Projectile.localNPCHitCooldown = movingHitCooldown;
                    break;
                }
            case AIState.Retracting:
                {
                    Projectile.tileCollide = false;
                    if (StateTimer++ > 3f)
                    {
                        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= maxRetractSpeed)
                        {
                            Projectile.Kill(); // Kill the projectile once it is close enough to the player
                            return;
                        }
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxRetractSpeed, retractAcceleration);
                        Vector2 target = Projectile.Center + Projectile.velocity;
                        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                        if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f)
                        {
                            Projectile.Kill(); // Kill projectile if it will pass the player
                            return;
                        }
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    break;
                }
        }

        // This is where Flower Pow launches projectiles. Decompile Terraria to view that code.

        Projectile.ownerHitCheck = shouldOwnerHitCheck; // This prevents attempting to damage enemies without line of sight to the player. The custom Colliding code for spinning makes this necessary.

        Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver2;

        // If you have a ball shaped flail, you can use this simplified rotation code instead
        /*
			if (Projectile.velocity.Length() > 1f)
				Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f; // skid
			else
				Projectile.rotation += Projectile.velocity.X * 0.1f; // roll
			*/

        Projectile.timeLeft = 2; // Makes sure the flail doesn't die (good when the flail is resting on the ground)
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(2); //Add a delay so the player can't button mash the flail
        player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
        if (Projectile.Center.X < mountedCenter.X)
        {
            player.itemRotation += (float)Math.PI;
        }
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

        // Spawning dust. We spawn dust more often when in the LaunchingForward state
        //int dustRate = 15;
        //if (doFastThrowDust)
        //	dustRate = 1;

        //if (Main.rand.NextBool(dustRate))
        //	Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ambient_DarkBrown, 0f, 0f, 150, default(Color), 1.3f);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.netUpdate = true;
        for (int i = 0; i < 3; i++)
        {
            Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
        }

        for (int i = 0; i < 7; i++)
        {
            Vector2 randVelocity = oldVelocity * 0.5f * Main.rand.NextFloat();
            Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Firework_Pink, randVelocity.X, randVelocity.Y, 150, default, 1f + (0.5f * ChargeLevel));
            dust.noGravity = true;
            dust.velocity *= 2f;
        }

        Impact();
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
            float hitRadius = 55f; // The length of the semi-major radius of the ellipse (the long end)
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
            for (int i = 0; i < 7; i++)
            {
                Vector2 randVelocity = Projectile.velocity * 0.5f * Main.rand.NextFloat();
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Firework_Pink, randVelocity.X, randVelocity.Y, 150, default, 1f + (0.5f * ChargeLevel));
                dust.noGravity = true;
                dust.velocity *= 2f;
            }

            modifiers.SourceDamage *= 2f * ChargeLevel;
            modifiers.Knockback *= ChargeLevel;
        }

        // The hitDirection is always set to hit away from the player, even if the flail damages the npc while returning
        modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

        // Knockback is only 25% as powerful when in spin mode
        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.Knockback *= 0.25f;
        }

        if (CurrentAIState == AIState.LaunchingForward && target.lifeMax > 1)
        {
            Impact();
        }
    }

    private void Impact()
    {
        Main.LocalPlayer.GetITDPlayer().BetterScreenshake(4, 4, 4, false);
        SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
        //emitter?.Emit(Projectile.Center, new Vector2(), 0.25f + (0.25f * ChargeLevel), 25);
        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Firework_Pink, 0f, 0f, 150, default, 1f + (0.5f * ChargeLevel));
            dust.noGravity = true;
            dust.velocity *= 2f;
        }
        for (int i = 0; i < 8; i++)
        {
            Vector2 direction = Vector2.UnitX.RotatedBy(MathHelper.PiOver4 * i + Projectile.rotation);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + direction * (14f + 7f * ChargeLevel), direction, ModContent.ProjectileType<SherbetSpike>(), (int)(Projectile.damage * ChargeLevel), Projectile.knockBack * 0.25f, Projectile.owner, 0f, 0.4f + 0.2f * ChargeLevel, 0f);
        }

        CurrentAIState = AIState.Retracting;
        StateTimer = 0f;
        Projectile.netUpdate = true;
        Projectile.velocity *= 0f;
    }

    // PreDraw is used to draw a chain and trail before the projectile is drawn normally.
    public override bool PreDraw(ref Color lightColor)
    {
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

            // Here, we draw the chain texture at the coordinates
            Main.spriteBatch.Draw(chainTexture.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

            // chainDrawPosition is advanced along the vector back to the player by the chainSegmentLength
            chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
            chainCount++;
            chainLengthRemainingToDraw -= chainSegmentLength;
        }

        // Add a motion trail when moving forward, like most flails do (don't add trail if already hit a tile)
        Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
        Vector2 drawOrigin = new(projectileTexture.Width * 0.5f, projectileTexture.Height * 0.5f);
        if (CurrentAIState == AIState.LaunchingForward)
        {
            for (int k = 0; k < Projectile.oldPos.Length && k < StateTimer; k++)
            {
                Vector2 trailPos = Projectile.oldPos[k] - Main.screenPosition + (Projectile.Size * 0.5f) + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(projectileTexture, trailPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, SpriteEffects.None, 0f);
            }
            Texture2D texture = TextureAssets.Extra[ExtrasID.FallingStar].Value;
            Rectangle rectangle = texture.Frame(1, 1);
            Vector2 effectDrawOrigin = new(rectangle.Size().X / 2f, rectangle.Size().Y / 5f);
            Vector2 position = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, position, rectangle, new Color(100, 100, 100, 50), Projectile.velocity.ToRotation() + MathHelper.PiOver2, effectDrawOrigin, 1.5f, SpriteEffects.None, 0f);
            Main.EntitySpriteDraw(texture, position, rectangle, new Color(150, 150, 150, 50), Projectile.velocity.ToRotation() + MathHelper.PiOver2, effectDrawOrigin, 1.2f, SpriteEffects.None, 0f);
        }
        Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        Main.spriteBatch.Draw(projectileTexture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}
