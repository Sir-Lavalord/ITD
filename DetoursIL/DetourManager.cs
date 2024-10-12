using ITD.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.DetoursIL
{
    public class DetourManager : ModSystem
    {
        public static List<DetourGroup> detours = [];
        public static T GetInstance<T>() where T : DetourGroup
        {
            return detours.FirstOrDefault(n => n is T) as T;
        }
        public override void Load()
        {
            foreach (Type t in ITD.Instance.Code.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(DetourGroup))))
            {
                DetourGroup instance = (DetourGroup)Activator.CreateInstance(t);
                detours.Add(instance);
                instance.Load();
            }
        }
        public override void Unload()
        {
            foreach(DetourGroup instance in detours)
                instance.Unload();
            detours?.Clear();
        }
    }
}
