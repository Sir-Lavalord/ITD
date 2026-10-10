using ITD.Content.Buffs.MinionBuffs;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Summoner
{
    public class NeptuneNet : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Summon;
            Item.width = 48;
            Item.height = 66;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = false;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item44;
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<NeptuneJellyfishBuff>();
            Item.shoot = ModContent.ProjectileType<NeptuneJellyfishMinion>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;
            return false;
        }
        public ParticleEmitter emitter;

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (player.itemAnimation == player.itemAnimationMax)
            {
                emitter = ParticleSystem.NewEmitter<BeanMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
                emitter.tag = Item;
                emitter.keptAlive = true;
            }

            if (emitter != null)
            {
                emitter.keptAlive = true;
                if (Main.rand.NextBool(1))
                {
                    MiscHelpers.GetPointOnSwungItemPath(player, 90f, 90f, 0.5f + 0.6f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out Vector2 position, out Vector2 spinningpoint);
                    Vector2 velocity = spinningpoint.RotatedBy((double)(1.57079637f * player.direction * player.gravDir), default);
                    emitter?.Emit(position, velocity * 4f, 0f, 20);
                }
            }
        }

    }
}