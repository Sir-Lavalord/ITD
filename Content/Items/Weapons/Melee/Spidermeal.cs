using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Melee
{
    public class Spidermeal : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.crit = 4;
            Item.DamageType = DamageClass.Melee;
            Item.width = 84;
            Item.height = 84;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(silver: 70);
            Item.ResearchUnlockCount = 1;
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
        }
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Stinky, 60 * 5);
            target.AddBuff(BuffID.Poisoned, 60 * 5);
            SoundEngine.PlaySound(SoundID.Item16, target.position);
        }
        public override bool? CanHitNPC(Player player, NPC target)
        {
            if (target.isLikeATownNPC)
                return true;
            return null;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.isLikeATownNPC)
            {
                modifiers.ScalingArmorPenetration += 1f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddCustomShimmerResult(ItemID.GoldBroadsword)
                .AddCustomShimmerResult(ItemID.Stinkbug, 5)
                .AddCondition(new Condition("", () => false))
                .Register();
        }
    }
}