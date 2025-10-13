using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.NPCs.Friendly;
using Terraria.Audio;
using Terraria.Localization;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps;

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
        snaptrapPull = new SoundStyle(toSnaptrapPull)
        {
            IsLooped = true,
        };
    }
    public Player Player => Main.player[Projectile.owner];
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
            if (dur <= 120)
                dur++;
            //NEED BETTER MATH HERE
            distance = (Player.Distance(target.Center) - dur - 30) / (16f * 25);
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
        if (Player.Distance(target.Center) >= 30)
        {
            // special check for a demoted strawman dummy for testing below (feel free to remove)
            bool shouldTargetBePulledTowardsPlayer =

            target is NPC ?
            (possibleNPC.ModNPC is StrawmanDummy d && possibleNPC.ai[0] == 6) || !possibleNPC.boss && possibleNPC.BossBar == null && possibleNPC.knockBackResist > 0 :

            target is Player && !possiblePlayer.noKnockback;

            bool shouldPlayerInsteadBePulledTowardsTarget =

            target is not NPC || possibleNPC.boss || possibleNPC.knockBackResist <= 0;
            if (shouldTargetBePulledTowardsPlayer)
            {
                Vector2 direction = Player.Center - target.Center;
                direction.Normalize();
                direction *= float.Lerp(0.05f, 20, 0.5f);

                bool collidesWithTiles = target is not NPC || !possibleNPC.noTileCollide;

                if (collidesWithTiles)
                {
                    if (!Collision.SolidCollision(target.BottomLeft, target.width, 16))
                    {
                        if (target.velocity.Y >= 0 && (tile.TileType == TileID.Platforms))
                        {
                            target.position.Y += 2;
                        }
                        if (target.velocity.Y == 0 && (tile.TileType == TileID.Platforms))
                        {

                            target.position.Y += 6;
                        }
                    }
                }
                target.velocity = direction;
            }

            else if (shouldPlayerInsteadBePulledTowardsTarget)
            {
                Vector2 direction = (target.Center - Player.Center).SafeNormalize(Vector2.Zero);
                direction *= float.Lerp(0.05f, 20, 0.5f);
                if (!Collision.SolidCollision(Player.BottomLeft, Player.width, 16))
                {
                    if (Player.velocity.Y >= 0 && (tile.TileType == TileID.Platforms))
                    {

                        Player.position.Y += 2;
                    }
                }
                Player.armorEffectDrawOutlines = true;
                Player.velocity = direction;
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
            if (power > 0 || dur >= 30)
            {
                if (Player.HasBuff(ModContent.BuffType<WhiplatchBuff>()))
                    Player.ClearBuff(ModContent.BuffType<WhiplatchBuff>());
                Player.AddBuff(ModContent.BuffType<WhiplatchBuff>(), (int)(dur * 30));
                Player.GetModPlayer<WhiplatchBuffPlayer>().power = power;
            }
        }
        base.OnKill(timeLeft);
    }
}