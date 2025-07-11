using Terraria.Audio;
using Terraria.DataStructures;

using ITD.Content.Buffs.EquipmentBuffs;
using ITD.Particles.Misc;
using ITD.Particles;

namespace ITD.Content.Items.Accessories.Master
{
    public class Prophylaxis : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
            Item.accessory = true;
        }
		
		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ProphylaxisPlayer>().hasProphylaxis = true;
        }
    }
	
	internal class ProphylaxisPlayer : ModPlayer
    {
		public ParticleEmitter emitter;
		
        public bool hasProphylaxis;
		public bool bloodletting;
		public bool justHealed;
		public int bloodShield;

        public override void ResetEffects()
        {
            hasProphylaxis = false;
			bloodletting = false;
			justHealed = false;
        }
		
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
			if (bloodletting)
				modifiers.FinalDamage.Base -= bloodShield;
		}
		
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
			if (bloodletting)
			{
				if (emitter != null)
				{
					emitter.keptAlive = true;
					emitter.Emit(Player.Center + Main.rand.NextVector2Circular(16f, 16f), Player.velocity * 0.5f, 0f, 20);
				}
				
				g *= 0.25f;
				b *= 0.25f;
			}
		}
    }
	
	internal class ProphylaxisHealItem : GlobalItem
    {
		public override bool? UseItem(Item item, Player player)
        {
			ProphylaxisPlayer modPlayer = player.GetModPlayer<ProphylaxisPlayer>();
            if (item.healLife > 0 && modPlayer.hasProphylaxis && !modPlayer.justHealed)
            {
				player.statLife -= player.GetHealLife(item);
				modPlayer.bloodShield = player.GetHealLife(item);
				modPlayer.justHealed = true; // hack fix for potions being weird
				
				player.AddBuff(ModContent.BuffType<BloodlettingBuff>(), 480, false);
				
				SoundEngine.PlaySound(SoundID.NPCDeath23, player.Center);
				modPlayer.emitter = ParticleSystem.NewEmitter<BloodParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
				modPlayer.emitter.tag = player;
				modPlayer.emitter.keptAlive = true;
				for (int i = 0; i < 15; ++i)
					modPlayer.emitter.Emit(player.Center, Main.rand.NextVector2Circular(8f, 8f), 0f, 20);
            }
			return null;
        }
	}
}
