using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Accessories.Defensive.Buffs
{
    public class SwimshieldWipers : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(26, 20);
        }
        // TODO: should other blindness mods buffs like calamity's be included?
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Blackout] = true;
        }
    }
}
