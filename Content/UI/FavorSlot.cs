using ITD.Content.Items.Favors;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using ITD.Systems;
using System.Collections.Generic;
using ReLogic.Graphics;
using System.Linq;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using ITD.Content.Items.Other;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ITD.Content.UI
{
    public class FavorSlotGui : ITDUIState
    {
        private FavorSlot favor;
        public override bool Visible => Main.LocalPlayer.TryGetModPlayer(out FavorPlayer favorPlayer) && favorPlayer.FavorSlotVisible;
        public override int InsertionIndex(List<GameInterfaceLayer> layers)
        {
            return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
        }
        public override void OnInitialize()
        {
            favor = new FavorSlot();
            Append(favor);
        }
        public override void Update(GameTime gameTime)
        {
            if (Main.playerInventory)
            {
                favor.UpdateProperties(52f, Main.screenWidth - 500, 30);
            }
            else
            {
                favor.UpdateProperties(52f, Main.screenWidth - 370, 30);
            }
            Recalculate();
            base.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
    public class FavorSlot : ITDItemSlot
    {
        public override ref Item item => ref Main.LocalPlayer.GetModPlayer<FavorPlayer>().FavorItem;
        public override Func<Item, bool> isValid => (item) => item.ModItem is Favor;
        public override string Texture => "ITD/Content/UI/FavorSlot";
        public override void Update(GameTime gameTime)
        {
            if (IsMouseOver)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (NoItem)
                    UICommon.TooltipMouseText(Language.GetOrRegister(ITD.Instance.GetLocalizationKey($"UI.{nameof(FavorSlot)}.MouseHoverName")).Value);
            }
        }
        public override void PostClickItemOut(ref Item mouseItem)
        {
            if (mouseItem.ModItem is Favor favorItem)
            {
                favorItem.Charge = 0f;
                favorItem.OnUnequip();
            }
        }
        public override void PostClickItemIn(ref Item slotItem)
        {
            if (slotItem.ModItem is Favor favorItem)
            {
                favorItem.Charge = 0f;
                favorItem.OnUnequip();
            }
        }
    }
}
