using ITD.Content.Items;
using ITD.Content.Items.Accessories.Combat.Melee.Snaptraps;
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
using ITD.Systems;

namespace ITD.Content.Projectiles
{
    public abstract class ITDSnaptrap : ModProjectile
    {
        public string toSnaptrapMetal = "ITD/Content/Sounds/SnaptrapMetal";
        public SoundStyle snaptrapMetal;
        public string toSnaptrapForcedRetract = "ITD/Content/Sounds/SnaptrapForcedRetract";
        private SoundStyle snaptrapForcedRetract;
        SlotId chainUnwindSlot;
        public string toSnaptrapChain = "ITD/Content/Sounds/SnaptrapUnwind";
        private SoundStyle snaptrapChain;
        SlotId snaptrapWarningSlot;
        public string toSnaptrapWarning = "ITD/Content/Sounds/SnaptrapWarning";
        private SoundStyle snaptrapWarning;

        public static Player myPlayer;
        /// <summary>
        /// In pixels. Multiply by 16f to get the tile equivalent.
        /// </summary>
        public float ShootRange { get; set; } = 16f * 8f;
        /// <summary>
        /// Acceleration of the Snaptrap while retracting.
        /// </summary>
        public float RetractAccel { get; set; } = 1.5f;
        /// <summary>
        /// Timer. The Snaptrap cannot retract until this value is equal or less than 0. (This is done in the AI)
        /// </summary>
        public int FramesUntilRetractable { get; set; } = 10;
        /// <summary>
        /// The amount of tiles you can go outside ShootRange without forced retraction.
        /// </summary>
        public float ExtraFlexibility { get; set; } = 16f * 2f;
        /// <summary>
        /// Amount of frames between each hit the Snaptrap gives. Less is faster.
        /// </summary>
        public int FramesBetweenHits { get; set; } = 60;
        /// <summary>
        /// Damage at the start, before reaching full power.
        /// </summary>
        public int MinDamage { get; set; } = 1;
        /// <summary>
        /// Damage when reaching full power.
        /// </summary>
        public int MaxDamage { get; set; } = 25;
        /// <summary>
        /// Amount of hits it takes for the Snaptrap to reach full power.
        /// </summary>
        public int FullPowerHitsAmount { get; set; } = 10;
        /// <summary>
        /// Amount of frames the warning sound should play for before forcefully retracting.
        /// </summary>
        public int WarningFrames { get; set; } = 60;
        /// <summary>
        /// DustID of the dust that appears when the Snaptrap latches onto an enemy.
        /// </summary>
        public int ChompDust { get; set; } = DustID.Torch;
        /// <summary>
        /// Chain texture. Override GetChainTexture(), GetChainColor(), and ExtraChainEffects() for customization.
        /// </summary>
        public string ToChainTexture {  get; set; } = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/SnaptrapChain";

        private float staticRotation; //
        public bool retracting = false; //
        private int damageTimer = 0; //
        private int currentDamageAmount = 0; //
        private int warningTimer = 0; //
        private bool shouldBeWarning = false; //
        private bool hasDoneLatchEffect = false;

        private bool chainWeight = false;
        private float lengthIncrease = 0f;
        //Use floats in the form of percentages to increase this
        private float retractMultiplier = 0f;
        //Use floats in the form of decimals to increase this. Base is 0.4f.

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
            Projectile.usesIDStaticNPCImmunity = true;
            DrawOffsetX = -12;
            DrawOriginOffsetY = -16;
            //Projectile.hide = true;
            SetSnaptrapProperties();
            snaptrapMetal = new SoundStyle(toSnaptrapMetal);
            snaptrapForcedRetract = new SoundStyle(toSnaptrapForcedRetract);
            snaptrapChain = new SoundStyle(toSnaptrapChain)
            {
                IsLooped = true,
            };
            snaptrapWarning = new SoundStyle(toSnaptrapWarning)
            {
                IsLooped = true,
            };
        }

        public override Color? GetAlpha(Color lightColor)
        {
            var tileCoords = Projectile.Center.ToTileCoordinates();
            return Lighting.GetColor(tileCoords.X, tileCoords.Y);
        }

        private void SetSnaptrapPlayerFlags(SnaptrapPlayer snaptrapPlayer)
        {
            chainWeight = snaptrapPlayer.ChainWeightEquipped;
            lengthIncrease = snaptrapPlayer.LengthIncrease;
            retractMultiplier = snaptrapPlayer.RetractMultiplier;
        }

        public override void OnSpawn(IEntitySource source)
        {
            myPlayer = Main.player[Projectile.owner];
            Projectile.netUpdate = true;
            Projectile.idStaticNPCHitCooldown = FramesBetweenHits;
            if (myPlayer.TryGetModPlayer(out SnaptrapPlayer modPlayer))
            {
                SetSnaptrapPlayerFlags(modPlayer);
            }
            if (chainWeight)
            {
                MinDamage += MinDamage / 10;
                MaxDamage += MaxDamage / 10;
            }
            Projectile.damage = MinDamage;

            ShootRange = ShootRange + (ShootRange * lengthIncrease);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!retracting && !IsStickingToTarget)
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
            retracting = true;
            return false;
        }

        private static int ReMapDamage(int currentHitNum, int maxHitNum, int minDmg, int maxDmg)
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
            if (myPlayer.dead)
            {
                Projectile.Kill();
                return;
            }
            Vector2 mountedCenter = myPlayer.MountedCenter;
            Vector2 toOwner = mountedCenter - Projectile.Center;

            if (retracting)
            {
                IsStickingToTarget = false;
                Projectile.tileCollide = false;
                Projectile.damage = 0;
            }

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
                                Dust.NewDust(Projectile.Center, 6, 6, ChompDust, 0f, 0f, 0, default, 1);
                            }
                        }
                    }
                }
                if (!retracting)
                {
                    StickyAI(chainLength);
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
                    soundInstance.Volume = 0f + ((float)warningTimer / (float)WarningFrames);
                    soundInstance.Pitch = 0f + ((float)warningTimer / (float)WarningFrames);
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
            if (chainLength >= ShootRange)
            {
                retracting = true;
            }
            if (retracting)
            {
                Vector2 towardsOwner = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                RetractAccel += 0.4f + retractMultiplier;
                Projectile.velocity = towardsOwner*RetractAccel;
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                if (Projectile.Distance(mountedCenter) <= RetractAccel)
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
            }
            if (damageTimer >= FramesBetweenHits)
            {
                damageTimer = 0;
                if (currentDamageAmount < FullPowerHitsAmount)
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
                int dmg = ReMapDamage(currentDamageAmount, FullPowerHitsAmount, MinDamage, MaxDamage);
                Projectile.damage = dmg;
            }
            if (chainLength-ExtraFlexibility >= ShootRange)
            {
                warningTimer += 1;
                if (warningTimer > WarningFrames)
                {
                    SoundEngine.PlaySound(snaptrapForcedRetract, Projectile.Center);
                    retracting = true;
                    warningTimer = WarningFrames;
                }
                shouldBeWarning = true;
            }
            else
            {
                shouldBeWarning = false;
                warningTimer = 0;
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
            Player player = Main.player[Projectile.owner];
            Asset<Texture2D> chainTexture = ModContent.Request<Texture2D>(ToChainTexture);
            Rectangle? chainSourceRectangle = null;
            float chainHeightAdjustment = 0f; // Use this to adjust the chain overlap. 

            Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
            Vector2 chainDrawPosition = Projectile.Center;
            Vector2 vectorFromProjectileToPlayer = player.Center.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
            Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayer.SafeNormalize(Vector2.Zero);
            float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
            if (chainSegmentLength == 0)
            {
                chainSegmentLength = 10;
            }
            float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
            int chainCount = 0;
            float chainLengthRemainingToDraw = vectorFromProjectileToPlayer.Length() + chainSegmentLength / 2f;
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
