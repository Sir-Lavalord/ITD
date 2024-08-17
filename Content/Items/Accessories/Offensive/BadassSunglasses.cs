using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Localization;
using System;
using ReLogic.Content;

namespace ITD.Content.Items.Accessories.Offensive
{
	[AutoloadEquip(EquipType.Face)]
    public class BadassSunglasses : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = Item.buyPrice(10);
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
			BadassPlayer modPlayer = player.GetModPlayer<BadassPlayer>();
            modPlayer.sunglassesOn = true;
			if (modPlayer.sunglassesCharge < 200)
				modPlayer.sunglassesCharge++;
			player.GetCritChance(DamageClass.Generic) += Math.Max(modPlayer.sunglassesCharge-100, 0);
        }
    }
	
	public class BadassPlayer : ModPlayer
    {
        public bool sunglassesOn;
		public int sunglassesCharge = 0;
		
        public override void ResetEffects()
        {
			if (!sunglassesOn)
				sunglassesCharge = 0;
            sunglassesOn = false;
        }
		public override void UpdateDead()
        {
			sunglassesCharge = 0;
		}
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			if (target.life > 0)
			{
				if (sunglassesCharge > 100)
				{
					SoundEngine.PlaySound(SoundID.Item101, Player.Center);
					for (int i = 0; i < 15; ++i)
					{
						int dustId = Dust.NewDust(Player.Center, 0, 0, 228, 0.0f, 0.0f, 100, new Color(), 2f);
						Main.dust[dustId].noGravity = true;
						Main.dust[dustId].velocity *= 5f;
					}
				}
				sunglassesCharge = 0;
			}
			else
			{
				if (sunglassesCharge > 100)
				{
					SoundEngine.PlaySound(SoundID.Item94, Player.Center);
					string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(BadassSunglasses)}.KillMessages."+Main.rand.Next(3))).Value;
					CombatText.NewText(new Microsoft.Xna.Framework.Rectangle((int) target.position.X, (int) target.position.Y, target.width, target.height), Color.Yellow, Text);
				}
			}
        }
		
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
			Vector2 position = Player.Center - Main.screenPosition - new Vector2(0f, 8f);
			Asset<Texture2D> texture = ModContent.Request<Texture2D>("ITD/Content/Items/Accessories/Offensive/BadassSunglasses_Aura");
			Rectangle sourceRectangle = texture.Frame(1, 1);
			Vector2 origin = sourceRectangle.Size() / 2f;
			Color color = Color.White;
			float opacity = Math.Max(sunglassesCharge-100, 0)*0.005f;
			color.A = (byte)(color.A*opacity);
			
			Main.EntitySpriteDraw(texture.Value, position, sourceRectangle, color*opacity, 0, origin, 0.5f+Main.essScale*0.5f, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture.Value, position, sourceRectangle, color*opacity, 0, origin, 1.2f-Main.essScale*0.5f, SpriteEffects.None, 0f);
        }
    }
}