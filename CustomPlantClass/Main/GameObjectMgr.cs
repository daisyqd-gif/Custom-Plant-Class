#nullable enable

namespace CustomPlantClass.Main
{
    public class GameObjectMgr : MonoBehaviour
    {
        private static readonly Dictionary<CustomItemType, GameObject> ObjectDictionary = new();

        public static CustomItemType Register<T>(GameObject gameObject, int id = -1) where T : Component
        {
            gameObject.AddComponent<T>();
            return Register(gameObject, id);
        }
        public static CustomItemType Register(GameObject gameObject, int id = -1)
        {
            // If user manually passed an ID, ensure it's not taken
            if (id != -1 && ObjectDictionary.ContainsKey((CustomItemType)id))
            {
                throw new ArgumentException(
                    $"Prefab ID {id} is already taken. Use -1 to auto-allocate."
                );
            }

            // Allocate or wrap the ID
            CustomItemType theItemType = id == -1
                ? AllocatePrefabID()
                : (CustomItemType)id;

            ObjectDictionary.Add(theItemType, gameObject);
            return theItemType;
        }
        public static GameObject Get(CustomItemType type)
        {
            return ObjectDictionary.GetValueSafe(type);
        }
        public static GameObject Instantiate(CustomItemType type, Vector3 position, Quaternion rotation)
        {
            return Instantiate(Get(type), position, rotation);
        }
        public static GameObject Instantiate(CustomItemType type, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Instantiate(Get(type), position, rotation, parent);
        }
        public static GameObject Instantiate(CustomItemType type)
        {
            return Instantiate(Get(type));
        }

        // ---------------------------------------------------------
        //  Allocate prefab ID
        // ---------------------------------------------------------

        public static int AllocatePrefabID()
        {
            PrefabIDAllocator.LoadFreezeTable();

            // 1. Resolve mod identity
            Assembly? asm = PrefabIDAllocator.ResolveModAssembly();

            string guidBase;

            if (asm != null)
            {
                var pluginAttr = asm.GetCustomAttribute<BepInPlugin>();
                guidBase = pluginAttr?.GUID ?? asm.FullName!;
            }
            else
            {
                var calling = Assembly.GetCallingAssembly();
                guidBase = calling?.FullName ?? "__PREFABIDFREEZE_UNKNOWN__";
            }

            // 2. Multi‑ID‑per‑mod
            if (!PrefabIDAllocator.GuidCallIndex.TryGetValue(guidBase, out int index))
                index = 0;

            string freezeKey = $"{guidBase}::{index}";
            PrefabIDAllocator.GuidCallIndex[guidBase] = index + 1;

            // 3. If frozen, return it
            if (PrefabIDAllocator.FreezeTable.TryGetValue(freezeKey, out int frozen))
                return frozen;

            // 4. Deterministic base ID
            int baseId = Math.Abs(freezeKey.GetHashCode()) % 50000 + 10000;
            int candidate = baseId;

            // 5. Avoid collisions with existing prefabs
            while (ObjectDictionary.ContainsKey(new CustomItemType(candidate)))
                candidate++;

            // 6. Freeze and return
            PrefabIDAllocator.FreezeTable[freezeKey] = candidate;
            PrefabIDAllocator.SaveFreezeTable();

            return candidate;
        }
        private static class PrefabIDAllocator
        {
            static readonly string FreezePath = Path.Combine(Paths.ConfigPath, "PrefabIDFreeze.json");
            public static Dictionary<string, int> FreezeTable = new();
            static bool FreezeLoaded = false;

            // Per‑mod call index (not saved)
            public static readonly Dictionary<string, int> GuidCallIndex = new();

            static readonly JsonSerializerOptions FreezeJsonOptions = new()
            {
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };

            // ---------------------------------------------------------
            //  Freeze table load/save
            // ---------------------------------------------------------

            public static void LoadFreezeTable()
            {
                if (FreezeLoaded) return;

                try
                {
                    if (File.Exists(FreezePath))
                    {
                        string json = File.ReadAllText(FreezePath);
                        FreezeTable = JsonSerializer.Deserialize<Dictionary<string, int>>(json, FreezeJsonOptions)
                                    ?? new Dictionary<string, int>();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PrefabIDFreeze] Failed to load freeze table: {e}");
                    FreezeTable = new Dictionary<string, int>();
                }

                FreezeLoaded = true;
            }

            public static void SaveFreezeTable()
            {
                try
                {
                    string? dir = Path.GetDirectoryName(FreezePath);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    string json = JsonSerializer.Serialize(FreezeTable, FreezeJsonOptions);
                    File.WriteAllText(FreezePath, json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PrefabIDFreeze] Failed to save freeze table: {e}");
                }
            }

            // ---------------------------------------------------------
            //  Resolve mod assembly (same logic as LevelIDAllocator)
            // ---------------------------------------------------------

            public static Assembly? ResolveModAssembly()
            {
                var calling = Assembly.GetCallingAssembly();
                if (calling != null &&
                    calling != typeof(PrefabIDAllocator).Assembly &&
                    calling.GetCustomAttribute<BepInPlugin>() != null)
                {
                    return calling;
                }

                var st = new StackTrace();
                var frames = st.GetFrames();
                if (frames != null)
                {
                    foreach (var frame in frames)
                    {
                        var type = frame.GetMethod()?.DeclaringType;
                        if (type == null) continue;

                        var asm = type.Assembly;
                        if (asm.GetCustomAttribute<BepInPlugin>() != null)
                            return asm;
                    }
                }

                return null;
            }
        }
    }
    public struct CustomItemType
    {
        private int id;
        public static implicit operator int(CustomItemType type)
        {
            return type.id;
        }
        public static implicit operator CustomItemType(int type)
        {
            return new CustomItemType(type);
        }
        public CustomItemType(int id) => this.id = id;
    }
}