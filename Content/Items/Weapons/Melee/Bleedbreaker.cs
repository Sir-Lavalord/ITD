using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using ITD.Content.Projectiles.Friendly.Melee;
using Terraria.DataStructures;
using ITD.Content.Projectiles.Friendly.Ranger;

namespace ITD.Content.Items.Weapons.Melee
{
    public class Bleedbreaker : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 74;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.damage = 50;
            Item.knockBack = 7;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.shootSpeed = 5f;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<BleedbreakerSwing>();
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
    }
}