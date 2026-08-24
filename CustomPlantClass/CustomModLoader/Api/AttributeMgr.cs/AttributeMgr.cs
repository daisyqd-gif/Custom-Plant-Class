using CustomPlantClass;

namespace FrameWorkLoader.API
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModAttribute : Attribute
    {
        public ModInfo Info { get; }

        public ModAttribute(ModInfo info)
        {
            Info = info;
        }
    }
    [Serializable]
    public struct ModInfo
    {
        public string Name;
        public string Version;
        public string TargetGameVersion;
        public int Priority;
        public int Category;
        public int Danger;
        public bool AutoLoadAttributes;
        public string[] Dependencies = Array.Empty<string>();
        public ModInfo(string Name, int Priority = 0, ModCategory Category = ModCategory.SecondaryCreation, ModDanger Danger = ModDanger.Normal, bool AutoLoadAttributes = false)
        {
            var Version = MyPluginInfo.TargetVersion;
            this.Name = Name;
            this.Version = Version;
            this.Priority = Priority;
            this.Danger = (int)Danger;
            TargetGameVersion = Version;
            this.Category = (int)Category;
            this.AutoLoadAttributes = AutoLoadAttributes;
        }
        public ModInfo(string Name, int Priority = 0, ModCategory Category = ModCategory.SecondaryCreation, ModDanger Danger = ModDanger.Normal, string Version = "1.0.0", bool AutoLoadAttributes = false)
        {
            TargetGameVersion = MyPluginInfo.TargetVersion;
            this.Name = Name;
            this.Version = Version;
            this.Priority = Priority;
            this.Danger = (int)Danger;
            this.Category = (int)Category;
            this.AutoLoadAttributes = AutoLoadAttributes;
        }
        public void DependsOn(string[] Dependencies)
        {
            this.Dependencies = Dependencies;
        }
    }
    /// <summary>
    /// Categorizes mods based on their purpose and scale.
    /// </summary>
    public enum ModCategory
    {
        /// <summary>
        /// Mods that only fix bugs or improve the base game without adding new content.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Mods that add custom plants and/or custom zombies.
        /// Typically single-content or small-scale creations.
        /// </summary>
        SecondaryCreation = 1,

        /// <summary>
        /// Mods that add multiple custom plants and/or zombies.
        /// Essentially content packs or mod bundles.
        /// </summary>
        ModGroup = 2,

        /// <summary>
        /// Large-scale mods that extend the game's systems and add a significant amount of content.
        /// Comparable to DLC-level expansions.
        /// </summary>
        DLC = 3,

        /// <summary>
        /// Mods that provide systems, APIs, or frameworks that other mods depend on.
        /// </summary>
        Framework = 4
    }
    /// <summary>
    /// Indicates how dangerous a mod is to the runtime based on its patching behavior.
    /// </summary>
    public enum ModDanger
    {
        /// <summary>
        /// Mods that do not patch anything.
        /// Pure data/content mods; extremely safe.
        /// </summary>
        VeryLow = 0,

        /// <summary>
        /// Mods that patch small, isolated methods with low risk of conflict.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Mods that patch frequently overridden virtual methods.
        /// These can conflict with other mods and affect core gameplay behavior.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Mods that use IL2CPP subclassing or other unsafe techniques.
        /// Highest risk; may cause instability or memory corruption.
        /// </summary>
        High = 3
    }
}
