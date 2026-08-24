using CustomPlantClass.Examples;

namespace CustomPlantClass
{
    [BepInPlugin(MyPluginInfo.PluginGuid, MyPluginInfo.PluginName, MyPluginInfo.PluginVersion)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource Logger;
        public static AssetBundle assetBundle;
        internal static Plugin plugin;
        public static Plugin Instance { get => plugin; }
        public static bool Loaded = false;
        public override void Load()
        {
            Loaded = true;
            plugin = this;
            OnLoad();
            DataMgr.OnLoad();
            CustomLevelMgr.OnLoad();
            PluginBehaviour.OnLoad();
            StaticExamples.OnLoad();
            Log.LogInfo($"{MyPluginInfo.PluginName} {MyPluginInfo.PluginVersion} loaded.");
        }
        public void OnLoad()
        {
            Logger = Log;
            Tools.InitMod(Assembly.GetExecutingAssembly());
            assetBundle = AssetMgr.LoadBundleFromResource(Assembly.GetExecutingAssembly(), "datamgr", false);
            CustomCore.RegisterCustomCardToColorfulCards(PlantType.ElectricOnion, 1);
        }
    }
    public static class PluginBehaviour
    {
        public static Queue<Action> queued = new();
        public static void QueueOrExecute(Action a)
        {
            if (Plugin.Loaded) a();
            else queued.Enqueue(a);
        }
        public static void OnLoad()
        {
            while (queued.Count > 0)
            {
                try
                {
                    queued.Dequeue()();
                }
                catch (Exception e)
                {
                    ModLogger.LogError(e.ToString());
                }
            }
        }
    }
    public static class MyPluginInfo
    {
        public const string PluginGuid = "CustomPlantClass.Bepinex";
        public const string PluginName = "CustomPlantClass";
        public const string PluginVersion = "1.0.0";
        public const string TargetVersion = "3.8.1";
    }
}