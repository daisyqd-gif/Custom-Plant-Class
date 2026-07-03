


namespace CustomPlantClass.RogueShootingMgr
{
    public class CustomRogueShootingManager : MonoBehaviour
    {
        #region Runtime
        public static CustomRogueShootingManager Instance;
        public ShootingManager ShootingManager => GetComponent<ShootingManager>();
        public void Awake()
        {
            if (Instance == null) Instance = this;
            Debug.Log("[CRS] Manager attached to ShootingManager.");
        }
        public static IEnumerable<CustomRogueBuff> GetCustomUpgrades()
        {
            foreach (var kv in _configs)
            {
                foreach (var buff in kv.Value.Buffs)
                    yield return buff;
            }
        }
        public static IEnumerable<CustomRogueBuff> GetCustomBuffsForPlant(PlantType type)
        {
            foreach (var kv in _configs)
            {
                foreach (var buff in kv.Value.Buffs)
                {
                    if (buff.ShowType == type)
                        yield return buff;
                }
            }
        }


        public void OnDestroy()
        {
            Instance = null;
        }
        #endregion

        #region Static
        private static readonly Dictionary<PlantType, ICustomRogueConfig> _configs = new();

        // Called by mods BEFORE EnterGame
        public static void RegisterCustomRogueShootingPlant<TClass>()
            where TClass : ICustomRogueConfig, new()
        {
            var cfg = new TClass();
            var type = cfg.PlantType;

            if (_configs.ContainsKey(type))
            {
                Debug.LogWarning($"[CRS] Config for {type} already registered.");
                return;
            }

            _configs[type] = cfg;
            Debug.Log($"[CRS] Registered config for {type}.");
        }

        public static class CustomRogueOtherBuffRegistry
        {
            private static readonly List<ICustomOtherBuff> _buffs = new();

            public static void Register(ICustomOtherBuff buff)
            {
                _buffs.Add(buff);
            }

            public static IEnumerable<ICustomOtherBuff> GetForRun(ShootingManager sm)
            {
                foreach (var buff in _buffs)
                    if (buff.CanAppear(sm))
                        yield return buff;
            }
        }

        // Called DURING EnterGame, after plants are created
        public static void ApplyReinforcement(Plant plant)
        {
            if (Instance == null)
            {
                Debug.LogError("[CRS] Manager not initialized.");
                return;
            }

            if (plant == null)
                return;

            if (_configs.TryGetValue(plant.thePlantType, out var cfg))
            {
                cfg.ApplyReinforcement(plant);
                Debug.Log($"[CRS] Reinforcement applied to {plant.thePlantType}.");
            }
        }
        #endregion
    }
    public interface ICustomRogueConfig
    {
        PlantType PlantType { get; }
        IReadOnlyList<CustomRogueBuff> Buffs { get; }
        IReadOnlyList<GeneralBuff> GeneralBuffs { get; }

        public void ApplyReinforcement(Plant plant);
    }
    public interface ICustomOtherBuff
    {
        string Title { get; }
        string Description { get; }
        PlantType ShowType { get; }
        ZombieType ZombieType { get; }
        Quality Rarity { get; }
        bool Interactable { get; }

        bool CanAppear(ShootingManager sm);
        void OnGet(ShootingManager sm);
    }
}