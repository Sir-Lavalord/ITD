using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.GameContent;
using System;
using ReLogic.Content;

using ITD.Players;

namespace ITD.Systems
{
    /*public class DrawLayerData
    {
        public static Color DefaultColor() => new Color(255, 255, 255, 0) * 0.7f;

        public Asset<Texture2D> Texture { get; init; }

        public Func<Color> Color { get; init; } = DefaultColor;
    }*/

    public sealed class FrontGunLayer : PlayerDrawLayer
    {
        private static List<int> ItemLayerData { get; set; }

        public static void RegisterData(int type)
        {
            if (!ItemLayerData.Contains(type))
            {
                ItemLayerData.Add(type);
            }
        }

        public override void Load()
        {
            ItemLayerData = new List<int>();
        }

        public override void Unload()
        {
            ItemLayerData = null;
        }

        public override Position GetDefaultPosition()
        {
            return new AfterParent(PlayerDrawLayers.HeldItem);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            Item heldItem = drawInfo.heldItem;

            if (drawInfo.shadow != 0f || drawPlayer.JustDroppedAnItem || drawPlayer.frozen || drawPlayer.pulley || !drawPlayer.CanVisuallyHoldItem(heldItem) || heldItem.type <= 0 || drawPlayer.dead || drawPlayer.wet && heldItem.noWet)
            {
                return false;
            }

            return true;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
			ITDPlayer modPlayer = drawPlayer.GetModPlayer<ITDPlayer>();

            Item heldItem = drawInfo.heldItem;

            bool valid = false;
            foreach (var type in ItemLayerData)
            {
                if (type == heldItem.type)
                {
					valid = true;
					break;
                }
            }
            if (!valid)
            {
                return;
            }
			
			Texture2D texture = TextureAssets.Item[heldItem.type].Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			
			Vector2 position = new Vector2((float)((int)(drawInfo.Position.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(drawInfo.Position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)) - new Vector2(drawPlayer.direction*5f, 0f);//Player.MountedCenter - new Vector2(Player.direction*4f, Player.gravDir*2f);
			float rotation = (Vector2.Normalize(Main.MouseWorld - drawPlayer.MountedCenter)*drawPlayer.direction).ToRotation();

			drawPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2 * drawPlayer.direction);
			
			position = position - Main.screenPosition;
			
			Vector2 origin = new Vector2(0, texture.Height);
			SpriteEffects effects = SpriteEffects.None;
			if (drawPlayer.direction == -1)
			{
				origin = new Vector2(texture.Width, texture.Height);
				effects = SpriteEffects.FlipHorizontally;
			}
			
			//Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(225, 225, 225), rotation - recoilBack * Player.direction, origin, 1f, effects, 0f);
			DrawData drawData = new DrawData(texture, position, sourceRectangle, new Color(255, 255, 255), rotation - modPlayer.recoilFront * drawPlayer.direction, origin - new Vector2(0, 10f), 1f, effects, 0f);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
	
	public sealed class BackGunLayer : PlayerDrawLayer
    {
        private static List<int> ItemLayerData { get; set; }

        public static void RegisterData(int type)
        {
            if (!ItemLayerData.Contains(type))
            {
                ItemLayerData.Add(type);
            }
        }

        public override void Load()
        {
            ItemLayerData = new List<int>();
        }

        public override void Unload()
        {
            ItemLayerData = null;
        }

        public override Position GetDefaultPosition()
        {
            return new AfterParent(PlayerDrawLayers.OffhandAcc);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            Item heldItem = drawInfo.heldItem;

            if (drawInfo.shadow != 0f || drawPlayer.JustDroppedAnItem || drawPlayer.frozen || drawPlayer.pulley || !drawPlayer.CanVisuallyHoldItem(heldItem) || heldItem.type <= 0 || drawPlayer.dead || drawPlayer.wet && heldItem.noWet)
            {
                return false;
            }

            return true;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
			ITDPlayer modPlayer = drawPlayer.GetModPlayer<ITDPlayer>();
			
            Item heldItem = drawInfo.heldItem;

            bool valid = false;
            foreach (var type in ItemLayerData)
            {
                if (type == heldItem.type)
                {
					valid = true;
					break;
                }
            }
            if (!valid)
            {
                return;
            }
			
			Texture2D texture = TextureAssets.Item[heldItem.type].Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			
			Vector2 position = new Vector2((float)((int)(drawInfo.Position.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(drawInfo.Position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)) - new Vector2(drawPlayer.direction*6f, 0f);//Player.MountedCenter - new Vector2(Player.direction*4f, Player.gravDir*2f);
			float rotation = (Vector2.Normalize(Main.MouseWorld - drawPlayer.MountedCenter)*drawPlayer.direction).ToRotation();

			drawPlayer.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2 * drawPlayer.direction);
			
			position = position - Main.screenPosition;
			
			Vector2 origin = new Vector2(0, texture.Height);
			SpriteEffects effects = SpriteEffects.None;
			if (drawPlayer.direction == -1)
			{
				origin = new Vector2(texture.Width, texture.Height);
				effects = SpriteEffects.FlipHorizontally;
			}
			
			//Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(225, 225, 225), rotation - recoilBack * Player.direction, origin, 1f, effects, 0f);
			DrawData drawData = new DrawData(texture, position, sourceRectangle, new Color(225, 225, 225), rotation - modPlayer.recoilBack * drawPlayer.direction, origin - new Vector2(drawPlayer.direction*2f, 8f), 1f, effects, 0f);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}