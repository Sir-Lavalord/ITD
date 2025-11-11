using ITD.Content.Buffs.Debuffs;
using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Summoner;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Summoner;

public class RiteOfImmolation : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 15;
        Item.DamageType = DamageClass.SummonMeleeSpeed;
        Item.width = 30;
        Item.height = 34;
        Item.useTime = 10;
        Item.useAnimation = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5;
        Item.value = 10000;
        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item20;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<RiteOfImmolationProj>();
        Item.shootSpeed = 128f;
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        MiscHelpers.GetPointOnSwungItemPath(player, 30f, 30f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out Vector2 position, out Vector2 spinningpoint);
        Vector2 value = spinningpoint.RotatedBy((double)(1.57079637f * player.direction * player.gravDir), default);
        Dust.NewDustPerfect(position, DustID.Torch, new Vector2?(value * 4f), 0, default, 1.5f).noGravity = true;
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff<RiteOfImmolationTagDebuff>(300);
        player.MinionAttackTargetNPC = target.whoAmI;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        float adjustedItemScale = player.GetAdjustedItemScale(player.inventory[player.selectedItem]);
        Projectile proj = Main.projectile[Projectile.NewProjectile(source, position + velocity * adjustedItemScale, new Vector2(), type, damage, knockback, player.whoAmI, adjustedItemScale)];
        proj.rotation = Main.rand.NextFloat(MathHelper.Pi);
        proj.direction = player.direction;
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<EmberlionMandible>(), 8)
            .AddIngredient(ModContent.ItemType<EmberlionSclerite>(), 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}