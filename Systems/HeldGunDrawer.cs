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
using ITD.Utilities;

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

            if (drawInfo.shadow != 0f || drawPlayer.JustDroppedAnItem || drawPlayer.frozen || drawPlayer.pulley || !drawPlayer.CanVisuallyHoldItem(heldItem) || heldItem.type <= ItemID.None || drawPlayer.dead || drawPlayer.wet && heldItem.noWet)
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
			
			Color drawColor = Lighting.GetColor((int)((double)drawInfo.Position.X + (double)drawPlayer.width * 0.5) / 16, (int)(((double)drawInfo.Position.Y + (double)drawPlayer.height * 0.5) / 16.0));
			
			Vector2 position = new Vector2((float)((int)(drawInfo.Position.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(drawInfo.Position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)) - new Vector2(drawPlayer.direction*5f, 0f);//Player.MountedCenter - new Vector2(Player.direction*4f, Player.gravDir*2f);
            position -= Main.screenPosition;
			
			Vector2 value = Main.OffsetsPlayerHeadgear[drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height];
			value.Y -= 2f;
			position += value * drawPlayer.gravDir;
			
			if (drawInfo.compFrontArmFrame.X / drawInfo.compFrontArmFrame.Width >= 7)
			{
				position += new Vector2(drawPlayer.direction, drawPlayer.gravDir);
			}
			
			float rotation = (Vector2.Normalize(modPlayer.MousePosition - drawPlayer.MountedCenter)*drawPlayer.direction).ToRotation();
			
			Vector2 origin = new Vector2(0f, texture.Height);
			SpriteEffects effects = SpriteEffects.None;
			if (drawPlayer.direction == -1)
			{
				origin += new Vector2(texture.Width, 0f);
				effects = SpriteEffects.FlipHorizontally;
			}
			if (drawPlayer.gravDir == -1)
			{
				origin -= new Vector2(0f, texture.Height);
				effects |= SpriteEffects.FlipVertically;
			}
			
			float adjustedItemScale = drawPlayer.GetAdjustedItemScale(heldItem);
			Vector2 offset = heldItem.ModItem.HoldoutOffset().HasValue ? heldItem.ModItem.HoldoutOffset().Value : new Vector2();
			
			DrawData drawData = new DrawData(texture, position, sourceRectangle, drawColor, rotation - modPlayer.recoilFront * drawPlayer.direction * drawPlayer.gravDir, origin + new Vector2((offset.X-4f) * drawPlayer.direction, (offset.Y-4f) * drawPlayer.gravDir), adjustedItemScale, effects, 0f);
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
            return new AfterParent(PlayerDrawLayers.Skin);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            Item heldItem = drawInfo.heldItem;

            if (drawInfo.shadow != 0f || drawPlayer.JustDroppedAnItem || drawPlayer.frozen || drawPlayer.pulley || !drawPlayer.CanVisuallyHoldItem(heldItem) || heldItem.type <= ItemID.None || drawPlayer.dead || drawPlayer.wet && heldItem.noWet)
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
			
			Color drawColor = Lighting.GetColor((int)((double)drawInfo.Position.X + (double)drawPlayer.width * 0.5) / 16, (int)(((double)drawInfo.Position.Y + (double)drawPlayer.height * 0.5) / 16.0));
			
			Vector2 position = new Vector2((float)((int)(drawInfo.Position.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2))), (float)((int)(drawInfo.Position.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f))) + drawPlayer.bodyPosition + new Vector2((float)(drawPlayer.bodyFrame.Width / 2), (float)(drawPlayer.bodyFrame.Height / 2)) + new Vector2(drawPlayer.direction*6f, drawPlayer.gravDir*2f);//Player.MountedCenter - new Vector2(Player.direction*4f, Player.gravDir*2f);
			position -= Main.screenPosition;
			
			Vector2 value = Main.OffsetsPlayerHeadgear[drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height];
			value.Y -= 2f;
			position += value * drawPlayer.gravDir;
			
			if (drawInfo.compFrontArmFrame.X / drawInfo.compFrontArmFrame.Width >= 7)
			{
				position += new Vector2(drawPlayer.direction, drawPlayer.gravDir);
			}
			
			float rotation = (Vector2.Normalize(modPlayer.MousePosition - drawPlayer.MountedCenter)*drawPlayer.direction).ToRotation();
			
			Vector2 origin = new Vector2(0f, texture.Height);
			SpriteEffects effects = SpriteEffects.None;
			if (drawPlayer.direction == -1)
			{
				origin += new Vector2(texture.Width, 0f);
				effects = SpriteEffects.FlipHorizontally;
			}
			if (drawPlayer.gravDir == -1)
			{
				origin -= new Vector2(0f, texture.Height);
				effects |= SpriteEffects.FlipVertically;
			}
			
			float adjustedItemScale = drawPlayer.GetAdjustedItemScale(heldItem);
			Vector2 offset = heldItem.ModItem.HoldoutOffset().HasValue ? heldItem.ModItem.HoldoutOffset().Value : new Vector2();
			
			DrawData drawData = new DrawData(texture, position, sourceRectangle, drawColor, rotation - modPlayer.recoilBack * drawPlayer.direction * drawPlayer.gravDir, origin + new Vector2(offset.X * drawPlayer.direction, offset.Y * drawPlayer.gravDir), adjustedItemScale, effects, 0f);
            drawInfo.DrawDataCache.Add(drawData);
        }
    }
}