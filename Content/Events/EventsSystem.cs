using ITD.Networking;
using ITD.Networking.Packets;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.Events
{
    public class EventsSystem : ModSystem
    {
        public static EventsSystem Instance => ModContent.GetInstance<EventsSystem>();
        public static readonly Dictionary<sbyte, ITDEvent> EventsByID = [];
        public static readonly Dictionary<Type, ITDEvent> EventsByType = [];
        public static readonly Dictionary<Type, sbyte> IDsByType = [];
        public static sbyte ActiveEvent = -1;
        public static T GetEvent<T>() where T : ITDEvent => EventsByType[typeof(T)] as T;
        /// <summary>
        /// Starts an event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void BeginEvent(ITDEvent e)
        {
            ActiveEvent = e.Type;
            e.IsActive = true;

            NetSystem.SendPacket(new SyncEventStatePacket(true));
        }
        /// <inheritdoc cref="BeginEvent(ITDEvent)"/>
        public static void BeginEvent<T>() where T : ITDEvent => BeginEvent(EventsByType[typeof(T)]);
        /// <inheritdoc cref="BeginEvent(ITDEvent)"/>
        public static void BeginEvent(sbyte type) => BeginEvent(EventsByID[type]);
        /// <summary>
        /// Stops an event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void StopEvent(ITDEvent e)
        {
            // has to be done here otherwise it's gonna try to access the events dict with -1 which will throw
            NetSystem.SendPacket(new SyncEventStatePacket(false));

            ITDEvent.BarScaleVisualProgress = 0f;
            ActiveEvent = -1;
            e.IsActive = false;
        }
        /// <inheritdoc cref="StopEvent(ITDEvent)"/>
        public static void StopEvent<T>() where T : ITDEvent => StopEvent(EventsByType[typeof(T)]);
        /// <inheritdoc cref="StopEvent(ITDEvent)"/>
        public static void StopEvent(sbyte type) => StopEvent(EventsByID[type]);
        public static sbyte RegisterEvent(ITDEvent instance)
        {
            sbyte newID = (sbyte)EventsByID.Count;
            Type t = instance.GetType();
            EventsByID[newID] = instance;
            EventsByType[t] = instance;
            IDsByType[t] = newID;
            return newID;
        }
        public override void PostUpdateWorld()
        {
            bool hasActiveEvent = ActiveEvent > -1;
            if (hasActiveEvent)
            {
                ITDEvent activeEvent = EventsByID[ActiveEvent];
                activeEvent.WorldUpdate();
                if (activeEvent.ShouldStop())
                    StopEvent(activeEvent);
                return;
            }
            for (sbyte i = 0; i < EventsByID.Count; i++)
            {
                ITDEvent @event = EventsByID[i];
                if (@event.NaturalSpawn())
                {
                    BeginEvent(@event);
                    break;
                }
            }
        }
        public static void OnKill(NPC npc)
        {
            if (ActiveEvent < 0)
                return;
            ITDEvent activeEvent = EventsByID[ActiveEvent];
            activeEvent.OnKill(npc);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int invasionIndex = layers.FindIndex(l => l.Name == "Vanilla: Invasion Progress Bars");
            if (invasionIndex != 1)
            {
                layers.Insert(invasionIndex, new LegacyGameInterfaceLayer("ITD: Event Progress Bars", delegate
                {
                    DrawEventProgressBars();
                    return true;
                }, InterfaceScaleType.UI));
            }
        }
        private static void DrawEventProgressBars()
        {
            if (ActiveEvent < 0)
                return;
            ITDEvent activeEvent = EventsByID[ActiveEvent];
            if (activeEvent.PreDrawProgressBar(Main.spriteBatch))
                activeEvent.DrawProgressBar(Main.spriteBatch);
            activeEvent.PostDrawProgressBar(Main.spriteBatch);
        }
    }
    public class EventVisualsHandler : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
        private static ITDEvent ActiveEvent => EventsSystem.ActiveEvent < 0 ? null : EventsSystem.EventsByID[EventsSystem.ActiveEvent];
        public override ModWaterStyle WaterStyle => ActiveEvent?.WaterStyle;
        public override int Music => ActiveEvent == null ? -1 : ActiveEvent.Music;
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (!isActive)
                return;
            ActiveEvent?.VisualsUpdate(player);
        }
        public override bool IsSceneEffectActive(Player player) => EventsSystem.ActiveEvent > -1;
    }
    /// <summary>
    /// Singleton for custom events. Wrapper for <see cref="GlobalNPC"/>, <see cref="ModSystem"/> and <see cref="ModSceneEffect"/> with only the useful stuff for events.
    /// </summary>
    public abstract class ITDEvent : ModType, ILocalizedModType
    {
        public static Color TerrariaBlurple => new(63, 65, 151, 255);
        public static float BarScaleVisualProgress = 0;
        public sbyte Type { get; private set; }
        public string LocalizationCategory => "Events";
        /// <summary>
        /// The path to the icon texture for this event. Defaults to the event's namespace and name (like other ModTypes)
        /// </summary>
        public virtual string IconTexture => $"{GetType().Namespace.Replace('.', '/')}/{Name}";
        /// <summary>
        /// The color that is used for the event title's frame or "background".
        /// </summary>
        public virtual Color EventTitleBackgroundColor => TerrariaBlurple;
        /// <summary>
        /// The color that is used to draw the progress bar.
        /// </summary>
        public virtual Color BarColor => Main.OurFavoriteColor;
        /// <summary>
        /// The <see cref="ModWaterStyle"/> used while this event is active.
        /// </summary>
        public virtual ModWaterStyle WaterStyle => null;
        /// <summary>
        /// The music track used while this event is active.
        /// </summary>
        public virtual int Music => -1;
        private bool _isActive = false;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (value && !_isActive)
                    OnActivate();
                else if (!value && _isActive)
                    OnDeactivate();
                _isActive = value;
            }
        }
        /// <summary>
        /// <para>Whether this event will disable vanilla spawning while it's active.</para>
        /// It is recommended to set this to true for fine control over spawns, vanilla or otherwise.
        /// </summary>
        public virtual bool OverrideVanillaSpawns => true;
        /// <summary>
        /// The visual progress (what will be shown in the progress bar) of this event.
        /// </summary>
        /// <returns></returns>
        public virtual float GetVisualProgress() => 0f;
        protected sealed override void Register()
        {
            Type = EventsSystem.RegisterEvent(this);
            ModTypeLookup<ITDEvent>.Register(this);
        }
        public sealed override void SetupContent()
        {
            SetStaticDefaults();
        }
        public virtual void NetSend(BinaryWriter writer)
        {

        }
        public virtual void NetReceive(BinaryReader reader)
        {

        }
        /// <summary>
        /// Runs once when this event is activated.
        /// </summary>
        public virtual void OnActivate()
        {

        }
        /// <summary>
        /// Runs for every <see cref="ITDEvent"/> every frame, as long as there isn't currently an active event. Return true to start this event.
        /// </summary>
        /// <returns></returns>
        public virtual bool NaturalSpawn()
        {
            return false;
        }
        /// <summary>
        /// Runs every frame while this event is active. Use this to set conditions for this event stopping.
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldStop()
        {
            return false;
        }
        /// <summary>
        /// Runs every frame while this event is active, but does not run on multiplayer clients.
        /// For visual effects, override <see cref="VisualsUpdate(Player)"/> instead.
        /// </summary>
        public virtual void WorldUpdate()
        {

        }
        /// <summary>
        /// Runs every frame while this event is active, but does not run on the server.
        /// For actions that must happen on the server, override <see cref="WorldUpdate"/> instead.
        /// </summary>
        /// <param name="player"></param>
        public virtual void VisualsUpdate(Player player)
        {

        }
        /// <summary>
        /// Called whenever any NPC in the world is killed. Use this to check for valid event NPC kills and do stuff accordingly.
        /// </summary>
        /// <param name="npc"></param>
        public virtual void OnKill(NPC npc)
        {

        }
        /// <summary>
        /// Return false to stop the progress bar from drawing normally.
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <returns></returns>
        public virtual bool PreDrawProgressBar(SpriteBatch spriteBatch)
        {
            return true;
        }
        public void DrawProgressBar(SpriteBatch spriteBatch)
        {
            if (BarScaleVisualProgress < 1f)
                BarScaleVisualProgress += 0.05f;
            BarScaleVisualProgress = Math.Clamp(BarScaleVisualProgress, 0, 1f);

            Asset<Texture2D> icon = ModContent.Request<Texture2D>(IconTexture);
            LocalizedText eventTitle = this.GetLocalization("EventProgressTitle");
            Color backgroundColor = EventTitleBackgroundColor;
            Color barColor = BarColor;

            DrawProgressPercentage(BarScaleVisualProgress, eventTitle.Value, icon.Value, backgroundColor, barColor);
        }
        public virtual void PostDrawProgressBar(SpriteBatch spriteBatch)
        {

        }
        /// <summary>
        /// Override this to set a custom bar subtitle based on the current progressAlpha (0f - 1f), which is the same as the value returned in <see cref="GetVisualProgress()"/>;
        /// </summary>
        /// <param name="alphaFactor"></param>
        /// <returns></returns>
        public virtual string BarSubtitle(float progressAlpha)
        {
            return $"{(progressAlpha * 100f):0.00}%";
        }
        private void DrawProgressPercentage(float alphaFactor, string invasionTitle, Texture2D icon, Color backgroundColor, Color barColor)
        {
            // deobfuscated with chyattCBT, kinda like the thing i did for the DesertDescription detour
            // edited to use my variables instead of the vanilla invasion ones

            // Handle drawing of invasion percentage
            int width = (int)(200f * alphaFactor);
            int height = (int)(45f * alphaFactor);
            Vector2 position = new(Main.screenWidth - 120, Main.screenHeight - 40);
            Utils.DrawInvBG(Main.spriteBatch, new Rectangle((int)position.X - width / 2, (int)position.Y - height / 2, width, height), TerrariaBlurple * 0.785f);

            float progressAlpha = GetVisualProgress();
            string percentageText = BarSubtitle(progressAlpha);

            // Drawing the progress bar
            Texture2D progressBar = TextureAssets.ColorBar.Value;
            float barWidth = 169f * alphaFactor;
            float barHeight = 8f * alphaFactor;

            Vector2 textPosition = position + Vector2.UnitY * barHeight + Vector2.UnitX * 1f;
            Utils.DrawBorderString(Main.spriteBatch, percentageText, textPosition, Color.White * alphaFactor, alphaFactor, 0.5f, 1f);

            Vector2 barPosition = textPosition + Vector2.UnitX * (progressAlpha - 0.5f) * barWidth;

            Main.spriteBatch.Draw(progressBar, position, null, Color.White * alphaFactor, 0f, new Vector2(progressBar.Width / 2, 0f), alphaFactor, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, barPosition, new Rectangle(0, 0, 1, 1), barColor * alphaFactor, 0f, new Vector2(1f, 0.5f), new Vector2(barWidth * progressAlpha, barHeight), SpriteEffects.None, 0f);

            // Draw invasion title and icon
            Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(invasionTitle);
            float additionalWidth = Math.Max(0f, titleSize.X - 200f);
            float boxWidth = 120f + additionalWidth;

            Rectangle boxRect = Utils.CenteredRectangle(new Vector2(Main.screenWidth - boxWidth, Main.screenHeight - 80),
                (titleSize + new Vector2(icon.Width + 12, 6f)) * alphaFactor);

            Utils.DrawInvBG(Main.spriteBatch, boxRect, backgroundColor);

            // Draw icon
            Main.spriteBatch.Draw(icon, boxRect.Left() + Vector2.UnitX * alphaFactor * 8f, null, Color.White * alphaFactor, 0f, new Vector2(0f, icon.Height / 2), alphaFactor * 0.8f, SpriteEffects.None, 0f);

            // Draw title
            Utils.DrawBorderString(Main.spriteBatch, invasionTitle, boxRect.Right() + Vector2.UnitX * alphaFactor * -22f, Color.White * alphaFactor, alphaFactor * 0.9f, 1f, 0.4f);
        }
        /// <summary>
        /// Override to tell the game which NPCs to spawn and when. To change spawn rate, override <see cref="ModifySpawnRate(Player, ref int, ref int)"/>.
        /// </summary>
        /// <param name="spawnInfo"></param>
        /// <returns></returns>
        public virtual IEnumerable<(int, float)> GetPool(NPCSpawnInfo spawnInfo)
        {
            yield return (NPCID.None, 0f);
        }
        public virtual void ModifySpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {

        }
        /// <summary>
        /// Runs once when deactivated.
        /// </summary>
        public virtual void OnDeactivate()
        {

        }
    }
}
