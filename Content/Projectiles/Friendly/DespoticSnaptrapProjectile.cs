using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Dusts;

namespace ITD.Content.Projectiles
{
    public class DespoticSnaptrapProjectile : ModProjectile
    {
        private const string ChainTexturePath = "ITD/Content/Projectiles/Friendly/DespoticSnaptrapChain";
        private const string ChainTextureExtraPath = "ITD/Content/Projectiles/Friendly/DespoticSnaptrapChain1";
        private const string ChainTextureExtra2Path = "ITD/Content/Projectiles/Friendly/DespoticSnaptrapChain2";
        SoundStyle snaptrapMetal = new SoundStyle("ITD/Content/Sounds/SnaptrapMetal", SoundType.Sound);
        SoundStyle snaptrapForcedRetract = new SoundStyle("ITD/Content/Sounds/SnaptrapForcedRetract", SoundType.Sound);
        SlotId chainUnwindSlot;
        SoundStyle snaptrapChain = new SoundStyle("ITD/Content/Sounds/SnaptrapUnwind", SoundType.Sound)
        {
            IsLooped = true,
        };
        SlotId snaptrapWarningSlot;
        SoundStyle snaptrapWarning = new SoundStyle("ITD/Content/Sounds/SnaptrapWarning", SoundType.Sound)
        {
            IsLooped = true,
        };

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
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            DrawOffsetX = -20;
            DrawOriginOffsetY = -24;
            Projectile.hide = true;
            Projectile.extraUpdates = 1;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            var tileCoords = Projectile.Center.ToTileCoordinates();
            return new Color(Color.White.ToVector4() * Lighting.GetColor(tileCoords.X, tileCoords.Y).ToVector4());
        }

        Player myPlayer = Main.player[Main.myPlayer];
        float shootRange = 16f*20f; // 16f*12f Means 12 tiles (each tile is 16 pixels)
        float staticRotation; //
        bool retracting = false; //
        float retractAccel = 1.5f; // Acceleration of the Snaptrap while retracting
        int framesUntilRetractable = 10;
        float extraFlexibility = 16f * 6f; // The amount of tiles you can go outside shootRange without forced retraction.
        int framesBetweenHits = 16; // 60 = 1 second between each hit
        int damageTimer = 0; //
        int minDamage = 3560; // Damage on first hit. (Most are 40% of its maxDamage but I'm keeping it customizable)
        int maxDamage = 8900; // Damage dealt at full power.
        int currentDamageAmount = 0; //
        int fullPowerDamageAmount = 10;  // How many times should the Snaptrap deal damage before it's at full power?
        int warningFrames = 200; // Time it takes for the Snaptrap to retract once the warning has started
        int warningTimer = 0; //
        bool shouldBeWarning = false; //
        bool hasDoneLatchEffect = false;
        int chompDust = DustID.IceTorch;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage = minDamage;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (retracting == false)
            { 
                staticRotation = Projectile.rotation;
                IsStickingToTarget = true;
                TargetWhoAmI = target.whoAmI;
                Projectile.velocity = target.velocity;
                Projectile.netUpdate = true;
                if (damageDone > 0)
                {
                    Projectile.damage = 0;
                }
            }
        }

        public bool Chomped = false;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Projectile.damage = 0;
            retracting = true;
            return false;
        }

        private int ReMapDamage(int currentHitNum, int maxHitNum, int minDmg, int maxDmg)
        {
            return (int)(minDmg + ((maxDmg - minDmg) * ((currentHitNum - 1) / (float)(maxHitNum - 1))));
        }
        public void PerHitLatchEffect()
        {

        }

        public void OneTimeLatchEffect()
        {
            Projectile.CritChance += 4;
            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
            AdvancedPopupRequest popupSettings = new AdvancedPopupRequest {
                Text = "+4% crit chance!",
                Color = Color.White,
                DurationInFrames = 60 * 2,
                Velocity = Projectile.velocity,
            };  
            PopupText.NewText(popupSettings, Projectile.Center + new Vector2(0f, -50f));
        }

        public void ConstantLatchEffect()
        {

        }
        public override void AI()
        {
            myPlayer.heldProj = Projectile.whoAmI;
            myPlayer.itemTime = 2;
            myPlayer.itemAnimation = 2;

            Vector2 mountedCenter = myPlayer.MountedCenter;
            Vector2 toOwner = mountedCenter - Projectile.Center;
            myPlayer.ChangeDir(-Math.Sign(toOwner.X));
            float chainLength = toOwner.Length();

            if (IsStickingToTarget)
            {
                if (Chomped == false)
                {
                    if (++Projectile.frameCounter >= 6)
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
                        if (Projectile.frame == 3)
                        {
                            Chomped = true;
                            Projectile.damage = 0;
                            SoundEngine.PlaySound(snaptrapMetal, Projectile.Center);
                            for (int i = 0; i < 6; ++i)
                            {
                                Dust.NewDust(Projectile.Center, 6, 6, chompDust, 0f, 0f, 0, default(Color), 1);
                            }
                        }
                    }
                }
                if (!retracting)
                {
                    StickyAI(chainLength);
                }
                else
                {
                    Projectile.damage = 0;
                    Projectile.tileCollide = false;
                    IsStickingToTarget = false;
                }
            }
            else
            {
                NormalAI(mountedCenter, chainLength);
            }
            if (!SoundEngine.TryGetActiveSound(chainUnwindSlot, out var activeSound))
            {
                var tracker = new ProjectileAudioTracker(Projectile);
                chainUnwindSlot = SoundEngine.PlaySound(snaptrapChain, Projectile.position, soundInstance => BasicSoundUpdateCallback(tracker, soundInstance, 0));
            }
            Projectile.timeLeft = 2;
            Dust.NewDust(Projectile.Center, 6, 6, chompDust, 0f, 0f, 0, default, 1f);
            Dust.NewDust(Projectile.Center, 6, 6, DustID.MushroomTorch, 0f, 0f, 0, Color.Blue, 2f);
            if (hasDoneLatchEffect)
            {
                ConstantLatchEffect();
            }
        }
        private bool BasicSoundUpdateCallback(ProjectileAudioTracker tracker, ActiveSound soundInstance, int type)
        {
            // Update sound location according to projectile position
            if (type == 0)
            {
                soundInstance.Position = Projectile.position;
                if (!Chomped || retracting) 
                {
                    return tracker.IsActiveAndInGame();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                soundInstance.Position = myPlayer.position;
                if (shouldBeWarning)
                {
                    soundInstance.Pitch = 0f + ((float)warningTimer / (float)warningFrames);
                }
                else
                {  soundInstance.Pitch = 0f; }
                if (IsStickingToTarget)
                {
                    return tracker.IsActiveAndInGame();
                }
                else
                {
                    return false;
                }
            }
        }

        private void NormalAI(Vector2 mountedCenter, float chainLength)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                if (--framesUntilRetractable <= 0)
                {
                    bool stillInUse = myPlayer.channel && !myPlayer.noItems && !myPlayer.CCed;
                    if (!stillInUse)
                    {
                        Projectile.damage = 0;
                        Projectile.tileCollide = false;
                        retracting = true;
                    }
                }
            }

            if (chainLength >= shootRange)
            {
                Projectile.damage = 0;
                Projectile.tileCollide = false;
                retracting = true;
            }
            if (retracting)
            {
                Vector2 towardsOwner = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                retractAccel += 0.4f;
                Projectile.velocity = towardsOwner * retractAccel;
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                if (Projectile.Distance(mountedCenter) <= retractAccel)
                {
                    Projectile.Kill(); // Kill the projectile once it is close enough to the player
                }
                Projectile.frame = 0;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                if (Main.rand.NextBool() == true) {
                    Dust.NewDust(Projectile.Center, 4, 4, ModContent.DustType<DespoticDust>(), 0, 0, 0, default(Color), 1f);
                }
            }
        }

        private const int StickTime = 60 * 40; // 40 seconds,
        private void StickyAI(float chainLength)
        {
            Projectile.rotation = staticRotation;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            StickTimer += 1f;
            damageTimer += 1;

            int npcTarget = TargetWhoAmI;
            if (StickTimer >= StickTime || npcTarget < 0 || npcTarget >= 200)
            {
                retracting = true;
                IsStickingToTarget = false;
            }
            else if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage)
            {
                Projectile.Center = Main.npc[npcTarget].Center;
                Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
                if (!SoundEngine.TryGetActiveSound(snaptrapWarningSlot, out var activeSound))
                {
                    var tracker = new ProjectileAudioTracker(Projectile);
                    snaptrapWarningSlot = SoundEngine.PlaySound(snaptrapWarning, myPlayer.Center, soundInstance => BasicSoundUpdateCallback(tracker, soundInstance, 1));
                }
            }
            else
            {
                retracting = true;
                IsStickingToTarget = false;
            }
            if (damageTimer >= framesBetweenHits)
            {
                damageTimer = 0;
                if (currentDamageAmount < fullPowerDamageAmount)
                {
                    currentDamageAmount += 1;
                }
                else
                {
                    if (!hasDoneLatchEffect)
                    {
                        hasDoneLatchEffect = true;
                        OneTimeLatchEffect();
                    }
                    PerHitLatchEffect();
                }
                int dmg = ReMapDamage(currentDamageAmount, fullPowerDamageAmount, minDamage, maxDamage);
                Projectile.damage = dmg;
            }
            if (chainLength-extraFlexibility >= shootRange)
            {
                warningTimer += 1;
                if (warningTimer > warningFrames)
                {
                    SoundEngine.PlaySound(snaptrapForcedRetract, Projectile.Center);
                    retracting = true;
                    IsStickingToTarget = false;
                    warningTimer = warningFrames;
                }
                shouldBeWarning = true;
            }
            else
            {
                shouldBeWarning = false;
                warningTimer = 0;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                if (--framesUntilRetractable <= 0)
                {
                    bool stillInUse = myPlayer.channel && !myPlayer.noItems && !myPlayer.CCed;
                    if (!stillInUse)
                    {
                        retracting = true;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
            Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(ChainTexturePath);
            Asset<Texture2D> chainTextureExtra = ModContent.Request<Texture2D>(ChainTextureExtraPath);
            Asset<Texture2D> chainTextureExtra2 = ModContent.Request<Texture2D>(ChainTextureExtra2Path);
            Rectangle? chainSourceRectangle = null;
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
            while (chainLengthRemainingToDraw > 0f)
            {
                // This code gets the lighting at the current tile coordinates
                Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));
                var chainTextureToDraw = chainTexture;
                if (chainCount >= 12)
                {
                    // Use normal chainTexture and lighting, no changes
                }
                else if (chainCount >= 8)
                {
                    // Near to the ball, we draw a custom chain texture and slightly make it glow if unlit.
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
                    // Close to the ball, we draw a custom chain texture and draw it at full brightness glow.
                    chainTextureToDraw = chainTextureExtra2;
                    chainDrawColor = Color.White;
                }
                Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);
                
                // chainDrawPosition is advanced along the vector back to the player by the chainSegmentLength
                chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
                chainCount++;
                chainLengthRemainingToDraw -= chainSegmentLength;
            }
            return true;
        } 
    }
}