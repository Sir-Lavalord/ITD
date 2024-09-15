using ITD.Players;
using ITD.Systems;
using Microsoft.Xna.Framework;
using Terraria;

namespace ITD.Utilities
{
    public static class PlayerHelpers
    {
        public static ITDPlayer GetITDPlayer(this Player player) => player.GetModPlayer<ITDPlayer>();
        public static SnaptrapPlayer GetSnaptrapPlayer(this Player player) => player.GetModPlayer<SnaptrapPlayer>();
        public static bool IsLocalPlayer(this Player player) => player.whoAmI == Main.myPlayer;
        public static Vector2 LookDirection(this Player player) => (player.GetITDPlayer().MousePosition - player.Center).SafeNormalize(Vector2.Zero);


        public static void CleanHoldStyle(Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
        {
            if (rotationOriginFromCenter == null)
                rotationOriginFromCenter = Vector2.Zero;
            Vector2 origin = rotationOriginFromCenter.Value;
            origin.X *= player.direction;
            origin.Y *= player.gravDir;
            player.itemRotation = desiredRotation;
            if (flipAngle)
                player.itemRotation *= player.direction;
            else if (player.direction < 0)
                player.itemRotation += MathHelper.Pi;
            Vector2 MainCenter = player.itemRotation.ToRotationVector2() * (spriteSize.X / -2f - 10f) * player.direction;

            Vector2 Center = MainCenter - origin.RotatedBy(player.itemRotation);
            Vector2 offsetAgain = spriteSize * -0.5f;

            Vector2 finalPosition = desiredPosition + offsetAgain + Center;
            if (stepDisplace)
            {
                int frame = player.bodyFrame.Y / player.bodyFrame.Height;
                if ((frame > 6 && frame < 10) || (frame > 13 && frame < 17))
                {
                    finalPosition -= Vector2.UnitY * 2f;
                }
            }
            player.itemLocation = finalPosition + new Vector2(spriteSize.X * 0.5f, 0);
        }
    }
}
