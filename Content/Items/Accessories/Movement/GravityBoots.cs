using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using System;
using ITD.Utilities;

namespace ITD.Content.Items.Accessories.Movement
{
	[AutoloadEquip(EquipType.Shoes)]
    public class GravityBoots : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Expert;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
			player.GetModPlayer<GravityBootsPlayer>().gravityBoots = true;
        }
    }
	
	public class GravityBootsPlayer : ModPlayer
    {
        public bool gravityBoots = false;
		public int gravityBootsCharge = 0;
		public int gravityBootsSound = 0;
		
		public override void ResetEffects()
        {
			if (!gravityBoots && gravityBootsCharge > 0)
				gravityBootsCharge--;
			gravityBoots = false;
        }
		
		public override void UpdateEquips()
        {
			if (gravityBoots)
			{
				if (!Player.IsOnStandableGround() || Player.velocity.Y != 0f)
				{
					Player.runAcceleration *= 2f;
					Player.runSlowdown *= 2f;
					Player.maxRunSpeed += 1f;
					if (Player.controlJump)
					{
						if (gravityBootsCharge < 10)
							gravityBootsCharge++;
						Player.gravity *= 0.25f;
						if (Player.velocity.Y * Player.gravDir > 0)
						{
							Player.velocity.Y *= 0.5f;
						}
						Player.fallStart = (int)(Player.position.Y / 16f);
						
						if (gravityBootsCharge == 10)
						{
							for (int i = 0; i < 2; i++)
							{
								int direction = (i == 0) ? 1 : -1;
								
								Vector2 position = Player.Center + new Vector2(direction*12f, (Player.height + 2f) * Player.gravDir * 0.5f);

								Dust dust = Dust.NewDustDirect(position - new Vector2(4f, 4f), 0, 0, 255, 0f, 0f, 0, default, 1f);
								//dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cShoe, Player);
								dust.noGravity = true;
								dust.velocity = Player.velocity + new Vector2(direction*2f, Player.gravDir*2f);
							}
						}
						
						gravityBootsSound = ++gravityBootsSound % 10;
						if (gravityBootsSound == 0)
							SoundEngine.PlaySound(SoundID.Item24, Player.Center);
					}
					else
					{
						if (gravityBootsCharge > 0)
							gravityBootsCharge--;
					}
				}
				else
				{
					if (gravityBootsCharge > 0)
						gravityBootsCharge--;
				}
			}
        }
		
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
			if (gravityBootsCharge > 0)
			{
				Texture2D texture = ModContent.Request<Texture2D>("ITD/Content/Items/Accessories/Movement/GravityBoots_Propulsion").Value;
				Rectangle sourceRectangle = texture.Frame(1, 1);
				Vector2 origin = sourceRectangle.Size() / 2f;

				for (int i = 0; i < 2; i++)
				{
					int direction = (i == 0) ? 1 : -1;
					Vector2 position = Player.Center - Main.screenPosition + new Vector2(direction*12f, (Player.height + 2f) * Player.gravDir * 0.5f);
					float sine = (float)(Math.Sin(Main.GlobalTimeWrappedHourly*10));
										
					Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(110, 38, 94, 50)*0.1f*gravityBootsCharge, Main.GlobalTimeWrappedHourly * -direction, origin, 1.25f + sine * 0.05f, SpriteEffects.None, 0f);
					Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(192, 59, 166, 100)*0.1f*gravityBootsCharge, Main.GlobalTimeWrappedHourly * -direction * 1.5f, origin, 0.75f + sine * 0.25f, SpriteEffects.None, 0f);
					Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(240, 135, 112, 100)*0.1f*gravityBootsCharge, Main.GlobalTimeWrappedHourly * -direction * 2f, origin, 0.75f - sine * 0.25f, SpriteEffects.None, 0f);
				}
			}
        }
    }
}