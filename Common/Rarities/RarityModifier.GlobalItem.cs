using MonoMod.Cil;
using ReLogic.Graphics;
using System.Collections.ObjectModel;
using System.Reflection;
using Terraria.GameContent;
using Terraria.GameContent.UI;

namespace ITD.Common.Rarities;

public class RarityModifierGlobalItem : GlobalItem // from better expert rarity mod
{
    // [private static properties and fields]

    private static int rarityInfo;
    private static readonly int[] hotbarRarityInfo;

    // [static constructors]

    static RarityModifierGlobalItem()
    {
        hotbarRarityInfo = new int[10];
    }

    // [public methods]

    public override void Load()
    {
        IL_Main.MouseTextInner += (il) =>
        {
            var c = new ILCursor(il);

            var xIndex = -1;
            var yIndex = -1;

            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld(typeof(Main).GetField("spriteBatch")),
                i => i.MatchLdsfld(typeof(FontAssets).GetField("MouseText")),
                i => i.MatchCallvirt(typeof(Asset<DynamicSpriteFont>).GetMethod("get_Value")),
                i => i.MatchLdloc(0),
                i => i.MatchLdloc(out xIndex),
                i => i.MatchConvR4(),
                i => i.MatchLdloc(out yIndex),
                i => i.MatchConvR4(),
                i => i.MatchNewobj(out _),
                i => i.MatchLdloc(out _))) return;

            if (!c.TryGotoPrev(MoveType.After,
                i => i.MatchLdsfld(typeof(Main).GetField("mouseTextColor")),
                i => i.MatchConvR4(),
                i => i.MatchLdcR4(255),
                i => i.MatchDiv(),
                i => i.MatchStloc(out _))) return;

            var label = c.DefineLabel();

            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, 0);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, 1);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, 2);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, xIndex);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, yIndex);

            c.EmitDelegate((string cursorText, int rarity, int diff, int x, int y) =>
            {
                if (diff > 0)
                    return true;

                if (!RarityModifierSystem.TryGetModifier(rarity, out RarityModifier modifier))
                    return true;

                modifier.Draw(new RarityModifier.DrawData
                {
                    Text = cursorText,
                    Position = new Vector2(x, y),
                    Color = ItemRarity.GetColor(rarity),
                    Rotation = 0f,
                    Origin = Vector2.Zero,
                    Scale = Vector2.One,
                    MaxWidth = -1f,
                    ShadowSpread = 2f
                });

                return false;
            });

            c.Emit(Mono.Cecil.Cil.OpCodes.Brtrue, label);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ret);
            c.MarkLabel(label);
        };

        On_PopupText.NewText_PopupTextContext_Item_int_bool_bool += (orig, context, newItem, stack, noStack, longText) =>
        {
            var index = orig(context, newItem, stack, noStack, longText);

            if (index < 0)
                return index;

            if (Main.popupText[index].expert)
                Main.popupText[index].rarity = ItemRarityID.Expert;

            if (Main.popupText[index].master)
                Main.popupText[index].rarity = ItemRarityID.Master;

            return index;
        };

        IL_Main.DrawItemTextPopups += (il) =>
        {
            var c = new ILCursor(il);

            ILLabel label = null;

            if (!c.TryGotoNext(MoveType.Before,
                i => i.MatchLdloc(1),
                i => i.MatchLdfld(typeof(PopupText).GetField("active", BindingFlags.Public | BindingFlags.Instance)),
                i => i.MatchBrfalse(out label))) return;

            var num7Index = -1;

            if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdloc(out _),
                i => i.MatchConvR4(),
                i => i.MatchLdcR4(255),
                i => i.MatchDiv(),
                i => i.MatchStloc(out num7Index))) return;

            c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_0);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, 1);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, 2);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, 4);
            c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, num7Index);
            c.EmitDelegate((float scaleTarget, PopupText popupText, string text, Vector2 vector2_2, float num7) =>
            {
                if (popupText.context is PopupTextContext.ItemPickupToVoidContainer or PopupTextContext.SonarAlert)
                    return true;

                if (!RarityModifierSystem.TryGetModifier(popupText.rarity, out RarityModifier modifier))
                    return true;

                var num10 = popupText.position.Y - Main.screenPosition.Y;

                if (Main.LocalPlayer.gravDir == -1.0)
                    num10 = Main.screenHeight - num10;

                modifier.Draw(new RarityModifier.DrawData
                {
                    Text = text,
                    Position = vector2_2 + new Vector2(popupText.position.X - Main.screenPosition.X, num10),
                    Color = new Color(popupText.color.R, popupText.color.G, popupText.color.B, popupText.color.A * popupText.alpha * (popupText.scale / scaleTarget)),
                    Rotation = popupText.rotation,
                    Origin = vector2_2,
                    Scale = Vector2.One * popupText.scale,
                    MaxWidth = -1f,
                    ShadowSpread = scaleTarget * 2f
                });

                return false;
            });

            c.Emit(Mono.Cecil.Cil.OpCodes.Brfalse, label);
        };

        On_Main.GUIHotbarDrawInner += (orig, self) =>
        {
            for (int i = 0; i < hotbarRarityInfo.Length; i++)
            {
                ref var item = ref Main.LocalPlayer.inventory[i];

                hotbarRarityInfo[i] = item.rare;

                if (item.expert)
                    item.rare = ItemRarityID.Expert;

                if (item.master)
                    item.rare = ItemRarityID.Master;
            }

            orig(self);

            for (int i = 0; i < hotbarRarityInfo.Length; i++)
            {
                ref var item = ref Main.LocalPlayer.inventory[i];

                item.rare = hotbarRarityInfo[i];
            }
        };

        MonoModHooks.Add(typeof(ItemLoader).GetMethod("PreDrawTooltip", BindingFlags.Public | BindingFlags.Static), ModifiedPreDrawTooltip);
        MonoModHooks.Add(typeof(ItemLoader).GetMethod("PostDrawTooltip", BindingFlags.Public | BindingFlags.Static), ModifiedPostDrawTooltip);
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod != "Terraria" || line.Name != "ItemName") return true;

        if (!RarityModifierSystem.TryGetModifier(item.rare, out RarityModifier modifier))
            return true;

        modifier.Draw(new RarityModifier.DrawData
        {
            Text = line.Text,
            Position = new Vector2(line.X, line.Y),
            Color = line.Color,
            Rotation = line.Rotation,
            Origin = line.Origin,
            Scale = line.BaseScale,
            MaxWidth = line.MaxWidth,
            ShadowSpread = line.Spread
        });

        return false;
    }

    // [private methods]

    private bool ModifiedPreDrawTooltip(OrigPreDrawTooltip orig, Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        rarityInfo = item.rare;

        if (item.expert)
            item.rare = ItemRarityID.Expert;

        if (item.master)
            item.rare = ItemRarityID.Master;

        return orig(item, lines, ref x, ref y);
    }

    private void ModifiedPostDrawTooltip(OrigPostDrawTooltip orig, Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
    {
        orig(item, lines);

        item.rare = rarityInfo;
    }

    // [...]

    private delegate bool OrigPreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y);
    private delegate void OrigPostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines);
}
