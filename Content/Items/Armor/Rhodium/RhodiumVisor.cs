using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;

using ITD.Content.Items.Materials;
using ITD.Utilities;
using ITD.Content.Buffs.EquipmentBuffs;

namespace ITD.Content.Items.Armor.Rhodium
{
    [AutoloadEquip(EquipType.Head)]
    public class RhodiumVisor : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
        public override void SetStaticDefaults()
        {
            SetBonusText = this.GetLocalization("SetBonus");
        }
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 18;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Green;
            Item.defense = 2;
        }
		
		public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 4f;
        }
		
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<RhodiumChestplate>() && legs.type == ModContent.ItemType<RhodiumLeggings>();
        }
        public override void UpdateArmorSet(Player player)
        {
            SetRhodiumPlayer modPlayer = player.GetModPlayer<SetRhodiumPlayer>();
            modPlayer.setBonus = true;
            player.setBonus = SetBonusText.Value;
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<RhodiumBar>(), 20)
                .AddTile(TileID.Anvils)
                .Register();
		}
    }
	
	public class SetRhodiumPlayer : ModPlayer
    {
        public bool setBonus = false;

        public override void ResetEffects()
        {
            setBonus = false;
        }

		public override void OnHurt(Player.HurtInfo info)
		{
			if (setBonus)
			{
				SoundEngine.PlaySound(SoundID.NPCHit21, Player.Center);
				Player.AddBuff(ModContent.BuffType<RhodiumRageBuff>(), 600);
			}
		}
    }
}
