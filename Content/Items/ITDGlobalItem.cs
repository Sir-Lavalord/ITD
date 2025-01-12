using ITD.Utilities;
using Terraria;
using Terraria.ModLoader;
using ITD.Players;
using ITD.Content.Projectiles;
using ITD.Content.Items.Weapons.Melee.Snaptraps;
using ITD.Common.Prefixes;

namespace ITD.Content.Items
{
    public class ITDGlobalItem : GlobalItem
    {
        public override void HoldItem(Item item, Player player)
        {
            ModPrefix prefix = PrefixLoader.GetPrefix(item.prefix);

            if (prefix is ComplexPrefix complexPrefix)
                complexPrefix.UpdateHeldPrefix(item, player);
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            ModPrefix prefix = PrefixLoader.GetPrefix(item.prefix);

            if (prefix is ComplexPrefix complexPrefix)
                complexPrefix.UpdateEquippedPrefix(item, player);
        }
        public override bool? UseItem(Item item, Player player)
        {
            ITDPlayer modPlayer = player.GetITDPlayer();
            if ((item.buffTime > 0))
            {
                if (modPlayer.portableLab)
                {
                    player.AddBuff(item.buffType, (int)(item.buffTime * 1.1f), true);
                }
            }
            return base.UseItem(item, player);
        }
    }
}
