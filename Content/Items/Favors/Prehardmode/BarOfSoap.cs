using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;
using ITD.Content.Rarities;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class BarOfSoap : Favor
    {
        public override int FavorFatigueTime => 60;

        public static LocalizedText HotkeyText { get; private set; }
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(7, 13));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            HotkeyText = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(BarOfSoap)}.HotkeyText"));
            Item.rare = ModContent.RarityType<Content.Rarities.FavorRarity>();
        }
        public override string GetBarStyle()
        {
            return "SoapBarStyle";
        }
        public override bool UseFavor(Player player)
        {
            for (int i = 0; i < 8; i++)
            {
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, new Vector2((float)Math.Cos(MathHelper.PiOver4 * i) * 2f, (float)Math.Sin(MathHelper.PiOver4 * i) * 2f), ProjectileID.PureSpray, 0, 0.1f, player.whoAmI);
            }
            return true;
        }
        public override float ChargeAmount(ChargeData chargeData)
        {
            if (chargeData.Type == ChargeType.DamageGiven)
            {
                return 0.01f;
            }
            return 0f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.First(x => x.Name == "Tooltip3");
            string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
            line.Text = hotkeyText;
        }
    }
}