using ITD.Kinematics;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Other.GrabbOMatic20000
{
    public class GrabbOMatic20000 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            player.GetModPlayer<GrabbOMaticPlayer>().Active = true;
        }
    }
    public class GrabbOMaticPlayer : ModPlayer
    {
        private const string toTex = "ITD/Content/Items/Other/GrabbOMatic20000/";
        private static readonly Asset<Texture2D> armTex = ModContent.Request<Texture2D>(toTex + "SegBase");
        private static readonly Asset<Texture2D> armMidTex = ModContent.Request<Texture2D>(toTex + "SegMid");
        private static readonly Asset<Texture2D> handTex = ModContent.Request<Texture2D>(toTex + "SegHand");
        public bool Active;
        public GrabbOMaticArm arm = null;
        public override void ResetEffects()
        {
            Active = false;
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (!Active)
                return;
            arm ??= new GrabbOMaticArm(Player.Center.X, Player.Center.Y);
            arm.ChainBase = Player.Center;
            for (int i = 0; i < 2; i++)
                arm.Update(Player.GetITDPlayer().MousePosition);
            arm.Draw(Main.spriteBatch, Main.screenPosition, Color.White, Player.direction == 1, armMidTex.Value, armTex.Value, handTex.Value);
        }
    }
    public class GrabbOMaticArm(float x, float y) : KineChain(x, y)
    {
        private KineSegment[] segments =
        [
            new KineSegment(x, y, 42f),
            new KineSegment(x, y, 42f),
            new KineSegment(x, y, 42f),
        ];
        public override KineSegment[] Segments => segments;
    }
}
