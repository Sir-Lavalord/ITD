using ITD.Content.Items;
using ITD.Content.Items.Accessories.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles
{
    public abstract class ITDSnaptrap : ModProjectile
    {
        public SoundStyle snaptrapMetal = new SoundStyle("ITD/Content/Sounds/SnaptrapMetal", SoundType.Sound);
        public SoundStyle snaptrapForcedRetract = new SoundStyle("ITD/Content/Sounds/SnaptrapForcedRetract", SoundType.Sound);
        SlotId chainUnwindSlot;
        public SoundStyle snaptrapChain = new SoundStyle("ITD/Content/Sounds/SnaptrapUnwind", SoundType.Sound)
        {
            IsLooped = true,
        };
        SlotId snaptrapWarningSlot;
        public SoundStyle snaptrapWarning = new SoundStyle("ITD/Content/Sounds/SnaptrapWarning", SoundType.Sound)
        {
            IsLooped = true,
        };

        public static Player myPlayer;
        /// <summary>
        /// In pixels. Multiply by 16f to get the tile equivalent.
        /// </summary>
        public float shootRange { get; set; } = 16f * 8f;
        /// <summary>
        /// Acceleration of the Snaptrap while retracting.
        /// </summary>
        public float retractAccel { get; set; } = 1.5f;
        /// <summary>
        /// Timer. The Snaptrap cannot retract until this value is equal or less than 0. (This is done in the AI)
        /// </summary>
        public int framesUntilRetractable { get; set; } = 10;
        /// <summary>
        /// The amount of tiles you can go outside shootRange without forced retraction.
        /// </summary>
        public float extraFlexibility { get; set; } = 16f * 2f;
        /// <summary>
        /// Amount of frames between each hit the Snaptrap gives. Less is faster.
        /// </summary>
        public int framesBetweenHits { get; set; } = 60;
        /// <summary>
        /// Damage at the start, before reaching full power.
        /// </summary>
        public int minDamage { get; set; } = 1;
        /// <summary>
        /// Damage when reaching full power.
        /// </summary>
        public int maxDamage { get; set; } = 25;
        /// <summary>
        /// Amount of hits it takes for the Snaptrap to reach full power.
        /// </summary>
        public int fullPowerHitsAmount { get; set; } = 10;
        /// <summary>
        /// Amount of frames the warning sound should play for before forcefully retracting.
        /// </summary>
        public int warningFrames { get; set; } = 60;
        /// <summary>
        /// DustID of the dust that appears when the Snaptrap latches onto an enemy.
        /// </summary>
        public int chompDust { get; set; } = DustID.Torch;
        /// <summary>
        /// Chain texture. Override GetChainTexture(), GetChainColor(), and ExtraChainEffects() for customization.
        /// </summary>
        public string toChainTexture {  get; set; } = "ITD/Content/Projectiles/Friendly/Snaptraps/SnaptrapChain";

        float staticRotation; //
        public bool retracting = false; //
        int damageTimer = 0; //
        int currentDamageAmount = 0; //
        int warningTimer = 0; //
        bool shouldBeWarning = false; //
        bool hasDoneLatchEffect = false;

        bool chainWeight = false;

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

        public virtual void SetSnaptrapProperties()
        {

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
            DrawOffsetX = -12;
            DrawOriginOffsetY = -16;
            Projectile.hide = true;
            SetSnaptrapProperties();
        }

        public override Color? GetAlpha(Color lightColor)
        {
            var tileCoords = Projectile.Center.ToTileCoordinates();
            return new Color(Color.White.ToVector4() * Lighting.GetColor(tileCoords.X, tileCoords.Y).ToVector4());
        }

        private void SetSnaptrapPlayerFlags(SnaptrapPlayer snaptrapPlayer)
        {
            chainWeight = snaptrapPlayer.ChainWeightEquipped;
        }

        public override void OnSpawn(IEntitySource source)
        {
            myPlayer = Main.player[Projectile.owner];
            myPlayer.TryGetModPlayer<SnaptrapPlayer>(out SnaptrapPlayer modPlayer);
            if (modPlayer != null)
            {
                SetSnaptrapPlayerFlags(modPlayer);
            }
            if (chainWeight)
            {
                minDamage += minDamage / 10;
                maxDamage += maxDamage / 10;
            }
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
        public virtual void PerHitLatchEffect()
        {

        }

        public virtual void OneTimeLatchEffect()
        {

        }

        public virtual void ConstantLatchEffect()
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

            if (chainWeight && !IsStickingToTarget)
            {
                Projectile.velocity.Y += 0.4f/(Projectile.extraUpdates+1);
            }

            if (IsStickingToTarget)
            {
                if (Chomped == false)
                {
                    if (++Projectile.frameCounter >= 3 * (Projectile.extraUpdates + 1))
                    {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
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
            if (hasDoneLatchEffect && !retracting)
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
                    soundInstance.Volume = 0f + ((float)warningTimer / (float)warningFrames);
                    soundInstance.Pitch = 0f + ((float)warningTimer / (float)warningFrames);
                }
                else
                {
                    soundInstance.Volume = 0f;
                    soundInstance.Pitch = 0f;
                }
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
                Projectile.velocity = towardsOwner*retractAccel;
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
                if (currentDamageAmount < fullPowerHitsAmount)
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
                int dmg = ReMapDamage(currentDamageAmount, fullPowerHitsAmount, minDamage, maxDamage);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chainDrawPosition">Position this chain is being drawn at. Divide by 16 to get tile coordinates.</param>
        /// <param name="chainCount">Position of this chain segment in the chain, starting from the Snaptrap and ending at the player's arms.</param>
        /// <returns></returns>
        public virtual Color GetChainColor(Vector2 chainDrawPosition, int chainCount)
        {
            return Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));
        }

        public virtual Asset<Texture2D> GetChainTexture(Asset<Texture2D> defaultTexture, Vector2 chainDrawPosition, int chainCount)
        {
            return defaultTexture;
        }

        public virtual void ExtraChainEffects(Vector2 chainDrawPosition, int chaincount)
        {

        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
            Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(toChainTexture);
            Rectangle? chainSourceRectangle = null;
            float chainHeightAdjustment = 0f; // Use this to adjust the chain overlap. 

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
                ExtraChainEffects(chainDrawPosition, chainCount);
                Color chainDrawColor = GetChainColor(chainDrawPosition, chainCount);
                var chainTextureToDraw = GetChainTexture(chainTexture, chainDrawPosition, chainCount);
                Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);
                
                chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
                chainCount++;
                chainLengthRemainingToDraw -= chainSegmentLength;
            }
            return true;
        } 
    }
}
