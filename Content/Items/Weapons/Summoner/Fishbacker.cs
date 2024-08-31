using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.Projectiles.Hostile;
using Microsoft.Build.Evaluation;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Summoner
{
    //inthejellyofthefish
    public class Fishbacker : ModItem
    {

        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<FishbackerProj>(), 20, 1, 6, 30);
            Item.width = 36;
            Item.height = 38;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.noMelee = true;
        }
        public override bool MeleePrefix()
        {
            return true;
        }
        public override void AddRecipes()
        {
        }
    }
}