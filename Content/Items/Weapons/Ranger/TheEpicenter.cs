using ITD.Content.Items.Materials;
using ITD.Content.Projectiles;
using ITD.Content.Projectiles.Friendly.Ranger;
using ITD.Systems;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;

namespace ITD.Content.Items.Weapons.Ranger;

public class TheEpicenter : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        FrontGunLayer.RegisterData(Item.type);
    }
    public override void SetDefaults()
    {
        Item.damage = 20;
        Item.DamageType = DamageClass.Ranged;
        Item.width = 68;
        Item.height = 24;
        Item.useTime = 12;
        Item.useAnimation = 12;
        Item.useStyle = -1;
        Item.noMelee = true;
        Item.knockBack = 6;
        Item.value = Item.sellPrice(gold: 5);
        Item.UseSound = SoundID.Item43;
        Item.rare = ItemRarityID.LightRed;
        Item.shoot = ModContent.ProjectileType<TheEpicenterSpark>();
        Item.shootSpeed = 6f;
        Item.useAmmo = AmmoID.Bullet;
        Item.autoReuse = true;
        Item.noUseGraphic = true;
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        /*
        var line = tooltips.First(x => x.Name == "ItemName");
        string[] parts = line.Text.Split(' ');
        if (parts.Length > 1)
        {
            string itemName = string.Join(" ", parts, 1, parts.Length - 1);
            string prefix = parts[0];
            line.Text = string.Format(itemName, prefix);
        }
        */
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
        if (player.altFunctionUse != 2)
        {
            modPlayer.recoilFront = 0.075f;
            modPlayer.recoilBack = 0.075f;
            int proj = Projectile.NewProjectile(source, position, velocity, type, player.ownedProjectileCounts[ModContent.ProjectileType<TheEpicenterBlackhole>()] <= 0 ? (int)(damage * 1f) : (int)(damage * 0.75f), knockback);
            Main.projectile[proj].GetGlobalProjectile<ITDInstancedGlobalProjectile>().ProjectileSource = ITDInstancedGlobalProjectile.ProjectileItemSource.TheEpicenter;
        }
        else
        {
            modPlayer.BetterScreenshake(20, 8, 8, true);
            modPlayer.recoilFront = 0.6f;
            modPlayer.recoilBack = 0.6f;
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<TheEpicenterBlackhole>(), damage, knockback);
        }
        return false;
    }
    public override bool AltFunctionUse(Player player)
    {
        return player.ownedProjectileCounts[ModContent.ProjectileType<TheEpicenterBlackhole>()] <= 0;
    }
    public override bool CanConsumeAmmo(Item ammo, Player player)
    {
        return Main.rand.NextFloat() >= 0.2f;
    }
    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {

        if (player.ownedProjectileCounts[ModContent.ProjectileType<TheEpicenterBlackhole>()] <= 0)
        {
            type = ModContent.ProjectileType<TheEpicenterSpark>();
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
            {
                PositionInWorld = position,
            }, player.whoAmI);
        }
        velocity = velocity.RotatedByRandom(MathHelper.ToRadians(4));
        Vector2 muzzleOffset = Vector2.Normalize(velocity) * 25f;

        if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
        {
            position += muzzleOffset;
        }
    }

    public static void Hold(Player player)
    {
        ITDPlayer modPlayer = player.ITD();
        Vector2 mouse = modPlayer.MousePosition;

        if (mouse.X < player.Center.X)
            player.direction = -1;
        else
            player.direction = 1;

        float rotation = (Vector2.Normalize(mouse - player.MountedCenter) * player.direction).ToRotation();
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilBack * player.direction - MathHelper.PiOver2 * player.direction);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        Hold(player);
    }
    public override void HoldStyle(Player player, Rectangle heldItemFrame)
    {
        Hold(player);
    }

    public override Vector2? HoldoutOffset()
    {
        return new Vector2(14f, -10f);
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
    public override void AddRecipes()
    {
        CreateRecipe(1)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddIngredient(ItemID.MeteoriteBar, 3)
            .AddIngredient(ModContent.ItemType<StarlitBar>(), 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
