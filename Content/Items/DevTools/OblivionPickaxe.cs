using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.DevTools
{
    public class OblivionPickaxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 1020;
            Item.DamageType = DamageClass.Melee;
            Item.width = 200;
            Item.height = 168;
            Item.useTime = 3;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = Item.buyPrice(platinum: 90); // Buy this item for one gold - change gold to any coin and change the value to any number <= 100
            Item.rare = ModContent.RarityType<DespoticRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.pick = 9999; // How strong the pickaxe is, see https://terraria.wiki.gg/wiki/Pickaxe_power for a list of common values
            Item.attackSpeedOnlyAffectsWeaponAnimation = true; // Melee speed affects how fast the tool swings for damage purposes, but not how fast it can dig
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
       
        }
    }
}