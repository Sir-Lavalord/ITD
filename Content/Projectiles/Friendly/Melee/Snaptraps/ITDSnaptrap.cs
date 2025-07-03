using Terraria.Audio;
using ReLogic.Utilities;
using Terraria.DataStructures;
using ITD.Content.Items.Weapons.Melee.Snaptraps;
using ITD.Systems;
using ITD.Utilities;
using System.IO;
using Terraria.ModLoader.IO;
using System;
using Terraria.WorldBuilding;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    /// <summary>
    /// <para>General change overview:</para>
    /// <para>- Instanced fields for <see cref="SoundStyle"/>s is correct. However, set these values in SetStaticDefaults, not SetDefaults. SEE BELOW</para>
    /// <para>- Update: This doesn't work. SoundStyles need to be instanced per entity so setting them in SetStaticDefaults only sets them for the template projectile (citation needed), todo: find a way around this?.</para>
    /// <para>- Static field to keep the owner player could be messy. Replace this with an instanced lambda property.</para>
    /// <para>- Instanced properties for all necessary Snaptrap properties is correct. Keep this as is.</para>
    /// <para>- Private fields for internal variables is correct. Keep this as is (mostly. make sure to check for possible property replacements)</para>
    /// <para>- Fields for SnaptrapPlayer flags is incorrect. Do not do this. Simply modify the variables in OnSpawn. See below.</para>
    /// <para>- Do not introduce unnecessary complexity here, as it may be hard to keep up with when more Snaptraps are introduced.</para>
    /// <para>- Getter and setter properties for IsStickingToTarget and TargetWhoAmI are correct. Look into how to use the third slot.</para>
    /// <para>- StickTimer is currently unnecessary as it is incredibly long by itself. Though, consider usage of the third ai[] slot here.</para>
    /// <para>- Here's the big mistake you've done with projectile damage. Snaptraps damaging by setting the Projectile.damage value is horrible. Look below for fix.</para>
    /// <para>- Use <see cref="ModProjectile.ModifyHitNPC(NPC, ref NPC.HitModifiers)"/>, <see cref="Projectile.usesLocalNPCImmunity"/> and <see cref="Projectile.localNPCHitCooldown"/> instead. (might need to be synced?)</para>
    /// <para>Extra notes:</para>
    /// <para>- Don't be dumb when animating. Make sure animation is friendly for all frame amounts.</para>
    /// <para>- Most of the current AI is fine.</para>
    /// <para>- Test MP a lot.</para>
    /// <para>- Add a new hook for modifying chain position offset. Could be cool. (or just add a ref <see cref="Vector2"/> param to ExtraChainEffects.)</para>
    /// </summary>
    public abstract class ITDSnaptrap : ITDProjectile
    {
        // Post-Implementation overview:
        // Achieved most of the things above.
        // New way of damaging stuff should be a lot cleaner, less scuffed and more solid.

        public string toSnaptrapChomp = "ITD/Content/Sounds/SnaptrapMetal";
        public SoundStyle snaptrapChomp;
        public string toSnaptrapForcedRetract = "ITD/Content/Sounds/SnaptrapForcedRetract";
        private SoundStyle snaptrapForcedRetract;
        private SlotId chainUnwindSlot;
        public string toSnaptrapChain = "ITD/Content/Sounds/SnaptrapUnwind";
        private SoundStyle snaptrapChain;
        private SlotId snaptrapWarningSlot;
        public string toSnaptrapWarning = "ITD/Content/Sounds/SnaptrapWarning";
        private SoundStyle snaptrapWarning;
        public Player Owner => Main.player[Projectile.owner];
        /// <summary>
        /// In pixels. To work with tile coordinates, simply multiply the number of tiles of reach by 16f.
        /// </summary>
        public float ShootRange { get; set; } = 16f * 8f;
        /// <summary>
        /// Acceleration of the Snaptrap while retracting.
        /// </summary>
        public float RetractAccel { get; set; } = 1.5f;
        /// <summary>
        /// The amount of pixels you can go outside ShootRange without forced retraction.
        /// </summary>
        public float ExtraFlexibility { get; set; } = 16f * 2f;

        /* special note: store the spawn source from onspawn and use the item's useTime for this instead. (might need syncing)
        public int FramesBetweenHits { get; set; } = 60;
        */

        /// <summary>
        /// Base damage at the start, before reaching full power.
        /// </summary>
        public int MinDamage { get; set; } = 1;
        /// <summary>
        /// Base damage when reaching full power.
        /// </summary>
        private int MaxDamage { get; set; } = 22;
        /// <summary>
        /// Amount of hits it takes for the Snaptrap to reach full power.
        /// </summary>
        public byte FullPowerHitsAmount { get; set; } = 10;
        /// <summary>
        /// Amount of frames the player should be warned for before forcefully retracting.
        /// </summary>
        public ushort WarningFrames { get; set; } = 60;
        /// <summary>
        /// DustID of the dust that appears when the Snaptrap latches onto an enemy.
        /// </summary>
        public short ChompDust { get; set; } = DustID.Torch;
        /// <summary>
        /// Chain texture. Override GetChainTexture(), GetChainColor(), and ExtraChainEffects() for customization.
        /// </summary>
        public string ToChainTexture { get; set; } = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/SnaptrapChain";

        private float staticRotation; //
        public bool retracting = false; //
        public bool manualRetract = false; //
        private bool shouldBeWarning = false; //
        protected bool hasDoneLatchEffect = false;
        private float gravity = 0f;
        private byte currentHitsAmount = 0;
        private float baseAccel = 0f;

        private EntitySource_ItemUse_WithAmmo source;
        private ITDSnaptrapItem SourceItem => source.Item.ModItem as ITDSnaptrapItem;

        // everything after the chainWeight bool is ooglyboogly. check notes.

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
        public int WarningTimer
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public bool IsStickingToPlayerTarget
        {
            get => Projectile.localAI[0] == 1f;
            set => Projectile.localAI[0] = value ? 1f : 0f;
        }
        public bool DoHitPlayer
        {
            get => Projectile.localAI[2] == 1f;
            set => Projectile.localAI[2] = value ? 1f : 0f;
        }
        public Entity Target
        {
            get
            {
                return IsStickingToPlayerTarget ? Main.player[TargetWhoAmI] : IsStickingToTarget ? Main.npc[TargetWhoAmI] : null;
            }
        }
        public virtual void SetSnaptrapDefaults()
        {

        }
        /// <summary>
        /// Override to set static default values, such as ProjectileID sets.
        /// </summary>
        public virtual void SetSnaptrapStaticDefaults()
        {

        }
        public sealed override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;

            SetSnaptrapStaticDefaults();
        }
        public sealed override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            // do the other part in OnSpawn
            Projectile.usesLocalNPCImmunity = true;
            DrawOffsetX = -12;
            DrawOriginOffsetY = -16;
            //Projectile.hide = true;
            SetSnaptrapDefaults();
            snaptrapChomp = new SoundStyle(toSnaptrapChomp);
            snaptrapForcedRetract = new SoundStyle(toSnaptrapForcedRetract);
            snaptrapChain = new SoundStyle(toSnaptrapChain)
            {
                IsLooped = true,
            };
            snaptrapWarning = new SoundStyle(toSnaptrapWarning)
            {
                IsLooped = true,
            };
            baseAccel = RetractAccel;
        }
        /// <summary>
        /// Called after storing the EntitySource and accessing SnaptrapPlayer variables (Which means using the <see cref="SourceItem"/> property is safe here.)
        /// </summary>
        public virtual void OnSnaptrapSpawn()
        {


        }
        private void SetSnaptrapPlayerFlags(SnaptrapPlayer player)
        {
            if (player.ChainWeightEquipped)
                gravity += 2f;
            ShootRange = player.LengthModifier.ApplyTo(ShootRange);
            RetractAccel = player.RetractVelocityModifier.ApplyTo(RetractAccel);
            FullPowerHitsAmount = (byte)player.FullPowerHitsModifier.ApplyTo(FullPowerHitsAmount);
            WarningFrames = (ushort)player.WarningModifier.ApplyTo(WarningFrames);
        }
        public sealed override void OnSpawn(IEntitySource source)
        {
            this.source = source as EntitySource_ItemUse_WithAmmo;
            Projectile.localNPCHitCooldown = SourceItem.Item.useTime;
            MaxDamage = SourceItem.Item.damage;
            SetSnaptrapPlayerFlags(this.source.Player.GetSnaptrapPlayer());
            OnSnaptrapSpawn();
        }
        public sealed override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(snaptrapChomp, Projectile.position);
            retracting = true;
            Projectile.netUpdate = true;
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
            {
                return true;
            }
            float num1 = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.Center + Projectile.velocity * 20,
                Projectile.width * Projectile.scale, ref num1))
            {
                if (!IsStickingToTarget && !retracting)
                {
/*                    Main.NewText("clank clank clank, get trapped!");
*/                    Projectile.frame = 1;
                }
            }
            return false;
        }
        public sealed override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteFlags(retracting, hasDoneLatchEffect, IsStickingToPlayerTarget, DoHitPlayer);
            writer.Write((byte)TargetWhoAmI);
            SendExtraSnaptrapAI(writer);
        }
        public virtual void SendExtraSnaptrapAI(BinaryWriter writer)
        {

        }
        public sealed override void ReceiveExtraAI(BinaryReader reader)
        {
            reader.ReadFlags(out retracting, out hasDoneLatchEffect, out var isStickingToPlayerTarget, out var doHitPlayer);
            IsStickingToPlayerTarget = isStickingToPlayerTarget;
            DoHitPlayer = doHitPlayer;
            TargetWhoAmI = reader.ReadByte();
            ReceiveExtraSnaptrapAI(reader);
        }
        public virtual void ReceiveExtraSnaptrapAI(BinaryReader reader)
        {

        }
        public sealed override bool? CanHitNPC(NPC target)
        {
            if (retracting)
                return false;
            return null;
        }
        private void ModifyHit(Entity target, ref Player.HurtModifiers playerMods, ref NPC.HitModifiers npcMods)
        {
            int maxDamage = MaxDamage;
            ModifyMaxDamage(ref maxDamage);
            float currentBaseDamage = Helpers.Remap(currentHitsAmount, 0, FullPowerHitsAmount, MinDamage, maxDamage);
            float currentDamageAfterMeleeScaling = Owner.GetTotalDamage(DamageClass.Melee).ApplyTo(currentBaseDamage);
            bool crit = Main.rand.NextFloat(100f + float.Epsilon) < Projectile.CritChance;
            float currentDamageAfterDefense =

                target is NPC npc ? npcMods.GetDamage(currentDamageAfterMeleeScaling, crit, true, Owner.luck) :
                target is Player player ? playerMods.GetDamage(currentDamageAfterMeleeScaling, (int)player.statDefense, player.DefenseEffectiveness.Value) :
                currentDamageAfterMeleeScaling;

            if (target is Player)
            {
                playerMods.ModifyHurtInfo += (ref Player.HurtInfo hit) =>
                {
                    hit.Damage = (int)currentDamageAfterDefense;
                };
            }
            else
            {
                npcMods.ModifyHitInfo += (ref NPC.HitInfo hit) =>
                {
                    hit.Crit = crit;
                    hit.Damage = (int)currentDamageAfterDefense;
                };
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            NPC.HitModifiers throwaway = new();
            ModifyHit(target, ref modifiers, ref throwaway);
        }
        public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player.HurtModifiers throwaway = new();
            ModifyHit(target, ref throwaway, ref modifiers);
        }
        private void OnHitTarget(Entity target)
        {
            if (!IsStickingToTarget)
            {
                staticRotation = Projectile.rotation;
                IsStickingToTarget = true;
                IsStickingToPlayerTarget = target is Player;
                TargetWhoAmI = target.whoAmI;
                Projectile.Center = target.Center;
                Projectile.velocity = target.velocity;
                Projectile.netUpdate = true;
            }
            if (!hasDoneLatchEffect && ++currentHitsAmount >= FullPowerHitsAmount)
            {
                PerHitLatchEffect();
                if (OneTimeLatchEffect())
                {
                    SoundEngine.PlaySound(snaptrapChomp, Projectile.Center);
                    for (int i = 0; i < 6; ++i)
                    {
                        Dust.NewDust(Projectile.Center, 6, 6, ChompDust, 0f, 0f, 0, default, 1);
                    }
                }
                hasDoneLatchEffect = true;
            }
            else if (hasDoneLatchEffect)
            {
                PerHitLatchEffect();
            }
        }
        public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => OnHitTarget(target);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => OnHitTarget(target);
        public virtual void ModifyMaxDamage(ref int maxDamage)
        {

        }
        public virtual void PerHitLatchEffect()
        {

        }
        /// <summary>
        /// Return false to stop the chomp sound from playing and chomp dust from being spawned.
        /// </summary>
        /// <returns></returns>
        public virtual bool OneTimeLatchEffect()
        {
            return true;
        }
        public virtual void ConstantLatchEffect()
        {

        }
        public override void AI()
        {
            if (Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Vector2 mountedCenter = Owner.MountedCenter;
            Vector2 toOwner = mountedCenter - Projectile.Center;

            if (retracting)
            {
                IsStickingToTarget = false;
                Projectile.tileCollide = false;
                Projectile.damage = 0;
            }

            float chainLength = toOwner.Length();

            // apply gravity (i. e. chainWeight)
            if (!IsStickingToTarget)
            {
                Projectile.velocity.Y += gravity / (Projectile.extraUpdates + 1);
            }

            if (IsStickingToTarget)
            {
                if (++Projectile.frameCounter >= 3 * (Projectile.extraUpdates + 1) && Projectile.frame < Main.projFrames[Type] - 1)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame == Main.projFrames[Type] - 2)
                    {
                        if (OnChomp())
                        {
                            SoundEngine.PlaySound(snaptrapChomp, Projectile.Center);
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
        /// <summary>
        /// Return false to prevent sound and dust.
        /// </summary>
        /// <returns></returns>
        public virtual bool OnChomp()
        {
            return true;
        }
        private bool BasicSoundUpdateCallback(ProjectileAudioTracker tracker, ActiveSound soundInstance, int type)
        {
            // Update sound location according to projectile position
            // (type 0 is for the chain sound callback. type 1 is for the warning sound callback)
            if (type == 0)
            {
                soundInstance.Position = Projectile.Center;
                if (!IsStickingToTarget || retracting)
                    return tracker.IsActiveAndInGame();
                return false;
            }
            else
            {
                soundInstance.Position = Owner.Center;
                if (shouldBeWarning)
                {
                    soundInstance.Volume = 0f + (WarningTimer / (float)WarningFrames);
                    soundInstance.Pitch = 0f + (WarningTimer / (float)WarningFrames);
                }
                else
                {
                    soundInstance.Volume = 0f;
                    soundInstance.Pitch = 0f;
                }
                if (IsStickingToTarget)
                    return tracker.IsActiveAndInGame();
                return false;
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
                RetractAccel += baseAccel;
                Projectile.velocity = towardsOwner * RetractAccel;
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                if (Projectile.DistanceSQ(mountedCenter) <= RetractAccel * RetractAccel)
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
        public virtual void StickyAI(float chainLength)
        {
            Projectile.rotation = staticRotation;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Entity target = Target;
            Player player = target as Player;
            NPC npc = target as NPC;
            float gfxOffY = target is Player ? player.gfxOffY : target is NPC ? npc.gfxOffY : 0;
            bool targetCantBeDamaged = target is Player ? false : target is NPC ? npc.dontTakeDamage : false;
            if (Target.active && !targetCantBeDamaged)
            {
                Projectile.Center = target.Center;
                Projectile.gfxOffY = gfxOffY;
                if (!SoundEngine.TryGetActiveSound(snaptrapWarningSlot, out var activeSound))
                {
                    var tracker = new ProjectileAudioTracker(Projectile);
                    snaptrapWarningSlot = SoundEngine.PlaySound(snaptrapWarning, Owner.Center, soundInstance => BasicSoundUpdateCallback(tracker, soundInstance, 1));
                }
            }
            else
            {
                retracting = true;
            }
            if (chainLength - ExtraFlexibility >= ShootRange)
            {
                WarningTimer += 1;
                if (WarningTimer > WarningFrames)
                {
                    SoundEngine.PlaySound(snaptrapForcedRetract, Projectile.Center);
                    retracting = true;
                    WarningTimer = WarningFrames;
                }
                shouldBeWarning = true;
            }
            else
            {
                shouldBeWarning = false;
                WarningTimer = 0;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            var tileCoords = Projectile.Center.ToTileCoordinates();
            return Lighting.GetColor(tileCoords.X, tileCoords.Y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chainDrawPosition">Position this chain is being drawn at.</param>
        /// <param name="chainCount">Position of this chain segment in the chain, starting from the Snaptrap and ending at the player's arms.</param>
        /// <returns></returns>
        public virtual Color GetChainColor(Vector2 chainDrawPosition, int chainCount)
        {
            return Lighting.GetColor(chainDrawPosition.ToTileCoordinates());
        }

        public virtual Asset<Texture2D> GetChainTexture(Asset<Texture2D> defaultTexture, Vector2 chainDrawPosition, int chainCount)
        {
            return defaultTexture;
        }

        public virtual void ExtraChainEffects(ref Vector2 chainDrawPosition, int chaincount)
        {
            if (retracting)
                return;
            float factor = WarningTimer / (float)WarningFrames * 2.25f;
            chainDrawPosition += Main.rand.NextVector2Circular(factor, factor);
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
                ExtraChainEffects(ref chainDrawPosition, chainCount);
                Color chainDrawColor = GetChainColor(chainDrawPosition, chainCount);
                var chainTextureToDraw = GetChainTexture(chainTexture, chainDrawPosition, chainCount);
                // draw using the shader. important for ITDProjectiles with ProjectileShader overridden or that could be potentially overridden
                Main.EntitySpriteDraw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None);

                chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
                chainCount++;
                chainLengthRemainingToDraw -= chainSegmentLength;
            }
            return true;
        }
    }
}
