using ITD.Players;
using ITD.Systems;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Localization;

namespace ITD.Utilities
{
    public static class PlayerHelpers
    {
        public static void KillMeCustom(this Player p, string key, double dmg = 10.0, int hitDirection = 0, bool pvp = false)
        {
            p.KillMe(p.DeathByLocalization(key), dmg, hitDirection, pvp);
        }
        public static PlayerDeathReason DeathByLocalization(this Player p, string key)
        {
            NetworkText death = Language.GetText($"Mods.ITD.DeathMessage.{key}").WithFormatArgs(p.name).ToNetworkText();
            return PlayerDeathReason.ByCustomReason(death);
        }
        public static Player FromGuid(Guid guid) => Main.player.FirstOrDefault(p => p.active && p.GetITDPlayer().guid == guid);
        public static ITDPlayer GetITDPlayer(this Player player) => player.GetModPlayer<ITDPlayer>();
        public static SnaptrapPlayer GetSnaptrapPlayer(this Player player) => player.GetModPlayer<SnaptrapPlayer>();
        public static bool IsLocalPlayer(this Player player) => player.whoAmI == Main.myPlayer;
        public static bool Exists(this Player player) => player != null && player.active && !player.dead;
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
