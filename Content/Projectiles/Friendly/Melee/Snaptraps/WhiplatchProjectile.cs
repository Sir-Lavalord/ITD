using Terraria.Localization;
using Terraria.Audio;
using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.NPCs.Friendly;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class WhiplatchProjectile : ITDSnaptrap
    {
        private readonly Asset<Texture2D> chainSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/Snaptraps/WhiplatchChain");
        public static LocalizedText OneTimeLatchMessage { get; private set; }
        public override void SetSnaptrapDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            OneTimeLatchMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(WhiplatchProjectile)}.OneTimeLatchMessage"));
            ShootRange = 18f * 40f;
            RetractAccel = 1.5f;
            ExtraFlexibility = 16f * 6f;
            MinDamage = 20;
            FullPowerHitsAmount = 1;
            WarningFrames = 60;
            ChompDust = DustID.Titanium;
            ToChainTexture = "ITD/Content/Projectiles/Friendly/Melee/Snaptraps/WhiplatchChain";
            toSnaptrapChomp = "ITD/Content/Sounds/UltraHookHit";
            toSnaptrapForcedRetract = "ITD/Content/Sounds/UltraHookPull";
            toSnaptrapChain = "ITD/Content/Sounds/UltraHookThrowLoop";
            snaptrapPull= new SoundStyle(toSnaptrapPull)
            {
                IsLooped = true,
            };
        }
        public Player player => Main.player[Projectile.owner];
        public override bool OneTimeLatchEffect()
        {
            return true;
        }
        public string toSnaptrapPull = "ITD/Content/Sounds/UltraHookPull";
        private SoundStyle snaptrapPull;
        public override void OnSnaptrapSpawn()
        {
            DoHitPlayer = true;
            Projectile.netUpdate = true;
        }
        public override void AI()
        {
            DoHitPlayer = true;
            base.AI();
        }
        public override void ConstantLatchEffect()
        {
            Entity target = Target;
            NPC possibleNPC = target as NPC;
            Player possiblePlayer = target as Player;
            Tile tile = Framing.GetTileSafely(target.Bottom);
            if (!manualRetract)
            {
                dur++;
                //NEED BETTER MATH HERE
                distance = (player.Distance(target.Center) - dur - 30) / (16f * 25);
                if (distance <= 0)
                {
                    distance = 0;
                }
                if (distance >= 1)
                {
                    distance = 1;
                }
                power = 1f - distance;
            }
            if (player.Distance(target.Center) >= 30)
            {
                // special check for a demoted strawman dummy for testing below (feel free to remove)
                bool shouldTargetBePulledTowardsPlayer =

                target is NPC ?
                (possibleNPC.ModNPC is StrawmanDummy d && possibleNPC.ai[0] == 6) || !possibleNPC.boss && possibleNPC.BossBar == null && possibleNPC.knockBackResist > 0 :

                target is Player ?
                !possiblePlayer.noKnockback :

                false;

                bool shouldPlayerInsteadBePulledTowardsTarget =

                target is NPC ?
                possibleNPC.boss || possibleNPC.knockBackResist <= 0 : true;
                if (shouldTargetBePulledTowardsPlayer)
                {
                    Vector2 direction = player.Center - target.Center;
                    direction.Normalize();
                    direction *= float.Lerp(0.05f, 20, 0.5f);

                    bool collidesWithTiles = target is NPC ? !possibleNPC.noTileCollide : true;

                    if (collidesWithTiles)
                    {
                        if (!Collision.SolidCollision(target.BottomLeft, target.width, 16))
                        {
                            if (target.velocity.Y >= 0 && Main.tileSolidTop[tile.TileType])
                            {
                                target.position.Y += 2;
                            }
                            if (target.velocity.Y == 0 && Main.tileSolidTop[tile.TileType])
                            {

                                target.position.Y += 6;
                            }
                        }
                    }
                    target.velocity = direction;
                }
                
                else if (shouldPlayerInsteadBePulledTowardsTarget)
                {
                    Vector2 direction = (target.Center - player.Center).SafeNormalize(Vector2.Zero);
                    direction *= float.Lerp(0.05f, 20, 0.5f);
                    if (!Collision.SolidCollision(player.BottomLeft, player.width, 16))
                    {
                        if (player.velocity.Y >= 0 && (tile.TileType == TileID.Platforms))
                        {

                            player.position.Y += 2;
                        }
                    }
                    player.armorEffectDrawOutlines = true;
                    player.velocity = direction;
                }          
            }
            else
            {
                retracting = true;
            }
        }

        float distance;
        float power;
        float dur;
        public override void OnKill(int timeLeft)
        {
            if (manualRetract)
            {
                if (power >0 || dur >= 30)
                {
                    player.AddBuff(ModContent.BuffType<WhiplatchBuff>(), (int)(dur * 30));
                    player.GetModPlayer<WhiplatchBuffPlayer>().power = power;
                }
            }
            base.OnKill(timeLeft);
        }
    }
}