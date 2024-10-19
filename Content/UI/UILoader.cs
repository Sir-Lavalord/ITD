using ITD.Networking;
using Microsoft.Xna.Framework;
using NVorbis.Contracts;
/*using StructureHelper.Core.Loaders.UILoading;
*/using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.UI
{
    public class UILoader : ModSystem
    {
		public static List<UserInterface> UserInterfaces = [];

        public static List<ITDUIState> UIStates = [];
        public override void Load() // same principle as the packet loader in NetSystem
        {
            foreach (Type t in Mod.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ITDUIState))))
            {
                var state = (ITDUIState)Activator.CreateInstance(t);
                var UI = new UserInterface();
                UI.SetState(state);
                state.UserInterface = UI;
                UIStates?.Add(state);
                UserInterfaces?.Add(UI);
            }
        }
        public override void Unload()
        {
            foreach (ITDUIState state in UIStates)
            {
                state.Unload();
            }
            UserInterfaces = null;
            UIStates = null;
        }
        public static void AddLayer(List<GameInterfaceLayer> layers, UIState state, int index, bool visible, InterfaceScaleType scale)
        {
            string name = state == null ? "Unknown" : state.ToString();
            layers.Insert(index, new LegacyGameInterfaceLayer("IntoTheDepths: " + name,
                delegate
                {
                    if (visible)
                        state.Draw(Main.spriteBatch);

                    return true;
                }, scale));
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < UIStates.Count; k++)
            {
                ITDUIState state = UIStates[k];
                AddLayer(layers, state, state.InsertionIndex(layers), state.Visible, state.Scale);
            }
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.ingameOptionsWindow || Main.InGameUI.IsVisible)
                return;
            foreach (UserInterface eachState in UserInterfaces)
            {
                if (eachState?.CurrentState != null && ((ITDUIState)eachState.CurrentState).Visible)
                    eachState.Update(gameTime);
            }
        }
    }
}
