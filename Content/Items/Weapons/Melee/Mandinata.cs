using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Systems;
using ITD.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee;

public class Mandinata : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
        ItemID.Sets.Spears[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.damage = 21;
        Item.DamageType = DamageClass.Melee;
        Item.width = 40;
        Item.height = 40;
        Item.useTime = 26;
        Item.useAnimation = 26;
        Item.UseSound = SoundID.Item20;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 7;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<MandinataProjectile>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        ITDPlayer modPlayer = player.GetITDPlayer();
        float ai = Main.rand.NextFloat(0.5f, 1f) * Item.shootSpeed * 0.75f * player.direction;
        Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<MandinataProjectile>(), damage, knockback, player.whoAmI, ai, modPlayer.itemVar[0]);
        if (modPlayer.itemVar[0] == 1f)
            modPlayer.itemVar[0] = 0f;
        return false;
    }

    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] < 1;
    }

    public override bool? UseItem(Player player)
    {
        if (!Main.dedServ && Item.UseSound.HasValue)
        {
            SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
        }

        return null;
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<EmberlionMandible>(), 1)
            .AddIngredient(ModContent.ItemType<EmberlionSclerite>(), 4)
            .AddIngredient(ItemID.IronBar, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
