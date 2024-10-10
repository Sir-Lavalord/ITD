using log4net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using ITD.Utilities;
using ReLogic.Graphics;
using Terraria.GameContent;
using ITD.Systems.Recruitment;

namespace ITD.Content.UI
{
    public class RecruitmentButtonGui : ITDUIState
    {
        private RecruitmentButton recruitmentButton;
        public override bool Visible => RecruitmentButtonVisible();
        private static bool RecruitmentButtonVisible()
        {
            bool talking = Main.LocalPlayer.talkNPC > -1;
            Mod dialogueTweakMod = ITD.Instance.dialogueTweak;
            if (dialogueTweakMod is null)
                return talking;
            dialogueTweakMod.TryFind("Configuration", out ModConfig config);
            FieldInfo field = config.GetType().GetField("VanillaUI");
            bool isDialogueTweakVanillaUI = (bool)field.GetValue(config);
            return talking && isDialogueTweakVanillaUI;
        }
        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 2"));
        }
        public override void OnInitialize()
        {
            recruitmentButton = new RecruitmentButton();
            Append(recruitmentButton);
        }
        public override void Update(GameTime gameTime)
        {
            DynamicSpriteFont font = FontAssets.MouseText.Value;
            Vector2 size = font.MeasureString(Main.npcChatText);
            float wrapWidth = 460f;
            int numberOfLines = (int)(size.X / wrapWidth);
            float yOffset = numberOfLines * 30f; // hacky way to do this
            //Main.NewText(yOffset.ToString() + " " + size.X.ToString());
            recruitmentButton.UpdateProperties(32f, Main.screenWidth / 2 + 210, 150 + yOffset);
            Recalculate();
            base.Update(gameTime);
        }
    }
    public class RecruitmentButton : UIElement
    {
        public const string buttonTex = "ITD/Content/UI/RecruitmentButton";
        public const string highlight = buttonTex + "_Highlight";
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Texture2D swordTexture = ModContent.Request<Texture2D>(buttonTex).Value;
            Texture2D highlightTexture = ModContent.Request<Texture2D>(highlight).Value;
            spriteBatch.Draw(swordTexture, GetDimensions().ToRectangle(), Color.White);
            if (IsMouseHovering)
                spriteBatch.Draw(highlightTexture, GetDimensions().ToRectangle(), Color.White);
        }
        public void UpdateProperties(float dimension, float left, float top)
        {
            Width.Set(dimension, 0f);
            Height.Set(dimension, 0f);
            Left.Set(left, 0f);
            Top.Set(top, 0f);
        }
        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                UICommon.TooltipMouseText(Language.GetOrRegister(ITD.Instance.GetLocalizationKey($"UI.{nameof(RecruitmentButton)}.MouseHoverName")).Value);
            }
        }
        public override void LeftClick(UIMouseEvent evt)
        {
            Player player = Main.LocalPlayer;
            if (TownNPCRecruitmentLoader.TryRecruit(player.talkNPC, player))
            {
                player.SetTalkNPC(-1);
            }
        }
    }
}
