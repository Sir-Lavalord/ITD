using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Tools.Cyanite
{
    public class CyaniteHammer : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 9;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.hammer = 90; 
            Item.attackSpeedOnlyAffectsWeaponAnimation = true; 
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
       
        }
    }
}