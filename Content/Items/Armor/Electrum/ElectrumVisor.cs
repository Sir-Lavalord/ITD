using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;

using ITD.Utilities;
using ITD.Content.Items.Materials;
using ITD.Content.NPCs;

namespace ITD.Content.Items.Armor.Electrum
{
    [AutoloadEquip(EquipType.Head)]
    public class ElectrumVisor : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
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
            player.moveSpeed += 0.08f;
        }
		
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ElectrumChestplate>() && legs.type == ModContent.ItemType<ElectrumLeggings>();
        }
        public override void UpdateArmorSet(Player player)
        {
			SetElectrumPlayer modPlayer = player.GetModPlayer<SetElectrumPlayer>();
            modPlayer.setBonus = true;
            player.setBonus = SetBonusText.Value;
        }
		
		public override void AddRecipes()
		{
			CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<ElectrumBar>(), 20)
                .AddTile(TileID.Anvils)
                .Register();
		}
    }
	
	public class SetElectrumPlayer : ModPlayer
    {
        public bool setBonus = false;

        public override void ResetEffects()
        {
            setBonus = false;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (setBonus)
			{
				target.GetGlobalNPC<ITDGlobalNPC>().zapped = true;
				int crit = (int)(Player.GetCritChance(DamageClass.Generic))+(int)(Player.GetCritChance(item.DamageType))+item.crit;
				float damage = Player.GetDamage(DamageClass.Generic).ApplyTo(Player.GetDamage(item.DamageType).ApplyTo(item.damage));
				MiscHelpers.Zap(target.Center, Player, (int)(damage * 0.75f), crit, 1);
				
				SoundEngine.PlaySound(SoundID.Item94, target.position);
				for (int i = 0; i < 3; i++)
				{
					int dust = Dust.NewDust(target.Center, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 2f;
				}
			}
		}
		
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (setBonus)
			{
				target.GetGlobalNPC<ITDGlobalNPC>().zapped = true;
				MiscHelpers.Zap(target.Center, Player, (int)(proj.damage * 0.75f), proj.CritChance, 1);
				
				SoundEngine.PlaySound(SoundID.Item94, target.position);
				for (int i = 0; i < 3; i++)
				{
					int dust = Dust.NewDust(target.Center, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 2f;
				}
			}
		}
    }
}
