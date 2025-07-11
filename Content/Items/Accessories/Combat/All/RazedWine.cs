using System.Linq;
using ITD.Utilities;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Players;

namespace ITD.Content.Items.Accessories.Combat.All
{
    public class RazedWine : ModItem
    {
        public static readonly int cooldownMax = 120;
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(28, 38);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ITDPlayer itdPlayer = player.GetITDPlayer();
            player.GetDamage(DamageClass.Generic) += 0.1f;
            itdPlayer.razedWine = true;
            if (itdPlayer.razedCooldown > 0)
            {
                return;
            }
            NPC[] npcs = itdPlayer.GetNearbyNPCs(10f * 16f);
            if (npcs.Length > 0)
            {
                NPC target = npcs.OrderByDescending(npc => npc.lifeMax).FirstOrDefault();
                Projectile.NewProjectile(player.GetSource_Accessory(Item), player.Center + player.velocity, Vector2.Zero, ModContent.ProjectileType<RazedWineBottle>(), 20, 0.1f, player.whoAmI, target.whoAmI);
                itdPlayer.razedCooldown = cooldownMax;
            }
        }
    }
}
