using ITD.Content.Projectiles;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Accessories.Misc
{
    public class Skullmet : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(30, 20);
            Item.defense = 2;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.npcTypeNoAggro[NPCID.Skeleton] = true;
            player.npcTypeNoAggro[NPCID.ArmoredSkeleton] = true;
            player.npcTypeNoAggro[NPCID.SkeletonArcher] = true;
            player.npcTypeNoAggro[NPCID.ArmoredViking] = true;
            player.npcTypeNoAggro[NPCID.UndeadViking] = true;
            player.GetModPlayer<SkullmetPlayer>().Active = true;
        }
    }
    public class SkullmetPlayer : ModPlayer
    {
        public bool Active;
        public override void ResetEffects()
        {
            Active = false;
        }
        public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
        {
            if (Player == Main.LocalPlayer)
            {
                if (Active)
                {
                    if (damageSource.TryGetCausingEntity(out Entity source))
                    {
                        if (source is Projectile projectile)
                        {
                            bool check = projectile.TryGetGlobalProjectile(out SkullmetGlobalProjectile globalProjectile)
                                         && globalProjectile.shouldNotDamagePlayer;
                            return check;
                        }
                    }
                }
            }
            return false;
        }
    }
    public class SkullmetGlobalProjectile : GlobalProjectile
    {
        public bool shouldNotDamagePlayer = false;
        private static readonly int[] ignoredProjectiles =
            [
            ProjectileID.SkeletonBone,
            ProjectileID.FlamingArrow,
            ];
        private static readonly int[] ignoredNPCs =
            [
            NPCID.SkeletonArcher,
            ];
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return ignoredProjectiles.Contains(entity.type);
        }
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_Parent parent)
            {
                if (parent.Entity is NPC npc)
                {
                    shouldNotDamagePlayer = ignoredNPCs.Contains(npc.type);
                }
            }
        }
    }
}