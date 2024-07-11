using Microsoft.Build.Construction;
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
            Item.width = 20;
            Item.height = 20;
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
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
        private static readonly int[] ignoredProjectiles =
            [
            ProjectileID.SkeletonBone,
            ProjectileID.FlamingArrow,
            ];
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
                            return ignoredProjectiles.Contains(projectile.type);
                        }
                    }
                }
            }
            return false;
        }
    }
}