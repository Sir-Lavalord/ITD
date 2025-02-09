using ITD.Content.Items.Weapons.Melee.Snaptraps;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Buffs.PetBuffs;
using ITD.Physics;
using Microsoft.Xna.Framework;
using rail;
using Terraria.DataStructures;
using System;
using ITD.Content.Buffs.GeneralBuffs;

namespace ITD.Content.Projectiles.Friendly.Melee.Snaptraps
{
    public class WhiplatchProjectile : ITDSnaptrap
    {
        private readonly Asset<Texture2D> chainSprite = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Melee/Snaptraps/WhiplatchChain");
        VerletChain Wire;
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
        public override void ConstantLatchEffect()
        {
            NPC npc = Main.npc[TargetWhoAmI];
            Tile tile = Framing.GetTileSafely(player.Bottom);
            if (!manualRetract)
            {
                dur++;
                //NEED BETTER MATH HERE
                distance = (player.Distance(npc.Center) - dur - 30) / (16f * 25);
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
            if (player.Distance(npc.Center) >= 30)
            {
                if (!npc.boss && npc.BossBar == null && npc.knockBackResist > 0)
                {
                        Vector2 direction = player.Center - npc.Center;
                        direction.Normalize();
                        direction *= float.Lerp(0.05f, 20, 0.5f);
                        if (!npc.noTileCollide)
                        {
                            if (!Collision.SolidCollision(npc.BottomLeft, npc.width, 16))
                            {
                                if (npc.velocity.Y >= 0 && (tile.TileType == TileID.Platforms))
                                {

                                    npc.position.Y += 2;
                                }
                                if (npc.velocity.Y == 0 && (tile.TileType == TileID.Platforms))
                                {

                                    npc.position.Y += 6;
                                }
                            }
                        }
                        npc.velocity = direction;
                    }
                
                else if (npc.boss || npc.knockBackResist <= 0)
                {
                    Vector2 direction = npc.Center - player.Center;
                    direction.Normalize();
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