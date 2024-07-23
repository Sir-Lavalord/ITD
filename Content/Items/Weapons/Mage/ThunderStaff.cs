using ITD.Content.Projectiles.Friendly;
using ITD.Utils;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Mage
{
    public class ThunderStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToMagicWeapon(ProjectileID.None, 30, 0f, true);
            Item.damage = 300;
            Item.staff[Type] = true;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item89;
        }
        public override bool? UseItem(Player player)
        {
            Vector2 startPos = new Vector2(Main.MouseWorld.X, Main.screenPosition.Y);
            Vector2 endPos = Helpers.QuickRaycast(startPos, Vector2.UnitY);
            MiscHelpers.CreateLightningEffects(startPos + new Vector2(Main.rand.NextFloat(-256f, 256f), 0f), endPos);
            Projectile.NewProjectile(Item.GetSource_FromThis(), endPos, Vector2.Zero, ModContent.ProjectileType<LightningStaffStrike>(), Item.damage, 0f);
            return true;
        }
    }
}
