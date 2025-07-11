using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.Items.Placeable.Biomes.Minibiomes.BlackMoldBm;

namespace ITD.Content.Items.Consumables
{
    public class BlackFlask : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;

            ItemID.Sets.DrinkParticleColors[Type] = [
                new Color(255, 255, 255),
            ];
        }

        public override void SetDefaults()
        {
            Item.UseSound = SoundID.Item3;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useTurn = true;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.width = 14;
            Item.height = 24;
            Item.buffType = ModContent.BuffType<WeaponImbueMelomycosis>();
            Item.buffTime = Item.flaskTime;
            Item.value = Item.sellPrice(0, 0, 5);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient<BlackMoldItem>(4)
                .AddTile(TileID.ImbuingStation)
                .Register();
        }
    }
}