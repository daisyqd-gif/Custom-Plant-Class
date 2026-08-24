#nullable enable

namespace CustomPlantClass.Level
{
    /// <summary>
    /// Central utility manager for all custom level registration
    /// </summary>
    public class CustomLevelMgr : MonoBehaviour
    {
        public static Dictionary<int, Func<bool>> CanUnlockLevel = new();
        public static void OnLoad()
        {

            DataMgr.GameStartActions.Add(() =>
            {
                foreach (var level in DataMgr.LoadedCustomLevels)
                {
                    if (level.ScenePrefab != null)
                    {
                        var scenePrefab = level.ScenePrefab;
                        scenePrefab.transform.FindChild("bg")?.AddComponent<GiveFertilize>();
                        scenePrefab.transform.FindChild("checklose")?.AddComponent<GameLose>();
                        // Add FloorMgr to all "floor" children
                        foreach (Transform child in scenePrefab.transform)
                        {
                            if (Regex.IsMatch(child.name, "floor", RegexOptions.IgnoreCase))
                                child.gameObject.AddComponent<FloorMgr>();
                        }
                        if (GameAPP.resourcesManager.backgroundPrefabs.TryAdd(level.SceneType, scenePrefab))
                        {
                            ModLogger.LogInfo($"Registered bg type {(int)level.SceneType}.");
                        }
                        else
                        {
                            ModLogger.LogWarn($"BgType {level.SceneType} already exists! Using original.");
                        }
                    }
                    if (level.MusicAudio != null)
                    {
                        if (GameAPP.soundManager.musics.TryAdd(level.MusicType, level.MusicAudio))
                        {
                            ModLogger.LogInfo($"Registered new music type {(int)level.MusicType}.");
                        }
                        else
                        {
                            ModLogger.LogWarn($"MusicType {level.MusicType} already exists! Using original.");
                        }
                    }
                    ModLogger.LogInfo($"Loaded custom level {level.LevelNameEn}.");
                }
            });
        }

        /// <summary>
        /// Registers a custom level with a custom component.
        /// </summary>
        public static int RegisterCustomLevel<T>(BaseCustomLevelData data) where T : MonoBehaviour
        {
            DataMgr.EnsureGameNotStarted();
            int theLevelID = data.LevelID;
            if (theLevelID < 0)
            {
                throw new ArgumentException("Invalid level ID : ", nameof(data.LevelID));
            }
            if (data.LevelName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Invalid level Name : ", nameof(data.LevelName));
            }
            if (DataMgr.UsedLevelIDs.Contains(theLevelID))
            {
                throw new InvalidOperationException($"Level ID {theLevelID} already exists.");
            }
            if (data.MaxWave <= 0 || data.MaxWave > 100)
            {
                throw new ArgumentException("Invalid wave count : ", nameof(data.MaxWave));
            }
            if (data.MapRoadTypes == null)
                throw new InvalidOperationException($"Level {theLevelID} must have a layout.");
            DataMgr.UsedLevelIDs.Add(theLevelID);
            DataMgr.CustomLevelComponents.Add(theLevelID, typeof(T));
            DataMgr.LoadedCustomLevels.Add(data);
            return theLevelID;
        }

        /// <summary>
        /// Registers a custom level.
        /// </summary>
        public static int RegisterCustomLevel(BaseCustomLevelData data)
        {
            DataMgr.EnsureGameNotStarted();
            int theLevelID = data.LevelID;
            if (theLevelID < 0)
            {
                throw new ArgumentException("Invalid level ID : ", nameof(data.LevelID));
            }
            if (data.LevelName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Invalid level Name : ", nameof(data.LevelName));
            }
            if (DataMgr.UsedLevelIDs.Contains(theLevelID))
            {
                throw new InvalidOperationException($"Level ID {theLevelID} already exists.");
            }
            if (data.MaxWave <= 0 || data.MaxWave > 100)
            {
                throw new ArgumentException("Invalid wave count : ", nameof(data.MaxWave));
            }
            if (data.MapRoadTypes == null)
                throw new InvalidOperationException($"Level {theLevelID} must have a layout.");
            else if (data.MapRoadTypes.Length == 0)
                throw new InvalidOperationException($"Level {theLevelID} must have a layout.");
            DataMgr.UsedLevelIDs.Add(data.LevelID);
            DataMgr.LoadedCustomLevels.Add(data);
            return data.LevelID;
        }
        public static int RegisterCustomLevel<T>(BaseCustomLevelData data, Func<bool> canUnlock) where T : MonoBehaviour
        {
            DataMgr.EnsureGameNotStarted();
            int theLevelID = data.LevelID;
            if (theLevelID < 0)
            {
                throw new ArgumentException("Invalid level ID : ", nameof(data.LevelID));
            }
            if (data.LevelName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Invalid level Name : ", nameof(data.LevelName));
            }
            if (DataMgr.UsedLevelIDs.Contains(theLevelID))
            {
                throw new InvalidOperationException($"Level ID {theLevelID} already exists.");
            }
            if (data.MaxWave <= 0 || data.MaxWave > 100)
            {
                throw new ArgumentException("Invalid wave count : ", nameof(data.MaxWave));
            }
            if (data.MapRoadTypes == null)
                throw new InvalidOperationException($"Level {theLevelID} must have a layout.");
            DataMgr.UsedLevelIDs.Add(theLevelID);
            DataMgr.CustomLevelComponents.Add(theLevelID, typeof(T));
            DataMgr.LoadedCustomLevels.Add(data);
            CanUnlockLevel.Add(data.LevelID, canUnlock);
            return theLevelID;
        }
        public static int RegisterCustomLevel(BaseCustomLevelData data, Func<bool> canUnlock)
        {
            DataMgr.EnsureGameNotStarted();
            int theLevelID = data.LevelID;
            if (theLevelID < 0)
            {
                throw new ArgumentException("Invalid level ID : ", nameof(data.LevelID));
            }
            if (data.LevelName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Invalid level Name : ", nameof(data.LevelName));
            }
            if (DataMgr.UsedLevelIDs.Contains(theLevelID))
            {
                throw new InvalidOperationException($"Level ID {theLevelID} already exists.");
            }
            if (data.MaxWave <= 0 || data.MaxWave > 100)
            {
                throw new ArgumentException("Invalid wave count : ", nameof(data.MaxWave));
            }
            if (data.MapRoadTypes == null)
                throw new InvalidOperationException($"Level {theLevelID} must have a layout.");
            else if (data.MapRoadTypes.Length == 0)
                throw new InvalidOperationException($"Level {theLevelID} must have a layout.");
            DataMgr.UsedLevelIDs.Add(data.LevelID);
            DataMgr.LoadedCustomLevels.Add(data);
            CanUnlockLevel.Add(data.LevelID, canUnlock);
            return data.LevelID;
        }
        public static int AllocateLevelID()
        {
            LevelIDAllocator.LoadFreezeTable();

            // 1. Resolve mod identity
            Assembly? asm = LevelIDAllocator.ResolveModAssembly();

            string guidBase;

            if (asm != null)
            {
                var pluginAttr = asm.GetCustomAttribute<BepInPlugin>();
                guidBase = pluginAttr?.GUID ?? asm.FullName!;
            }
            else
            {
                var calling = Assembly.GetCallingAssembly();
                guidBase = calling?.FullName ?? "__LEVELIDFREEZE_UNKNOWN__";
            }

            // 2. Multi‑ID‑per‑mod: assign per‑mod call index
            if (!LevelIDAllocator.GuidCallIndex.TryGetValue(guidBase, out int index))
                index = 0;

            string freezeKey = $"{guidBase}::{index}";
            LevelIDAllocator.GuidCallIndex[guidBase] = index + 1;

            // 3. If frozen, return it
            if (LevelIDAllocator.FreezeTable.TryGetValue(freezeKey, out int frozen))
                return frozen;

            // 4. Build used set (custom levels only)
            HashSet<int> used = new();

            foreach (var lvl in DataMgr.LoadedCustomLevels)
                used.Add(lvl.LevelID);

            foreach (var id in DataMgr.UsedLevelIDs)
                used.Add(id);

            // 5. Deterministic base ID from freezeKey
            int baseId = Math.Abs(freezeKey.GetHashCode()) % 50000 + 10000;
            int candidate = baseId;

            while (used.Contains(candidate))
                candidate++;

            // 6. Freeze and return
            LevelIDAllocator.FreezeTable[freezeKey] = candidate;
            LevelIDAllocator.SaveFreezeTable();

            //UsedLevelIDs.Add(candidate);
            return candidate;
        }
        public static int AllocateLevelID(string name)
        {
            LevelIDAllocator.LoadFreezeTable();

            string guidBase = name;

            // 2. Multi‑ID‑per‑mod: assign per‑mod call index
            if (!LevelIDAllocator.GuidCallIndex.TryGetValue(guidBase, out int index))
                index = 0;

            string freezeKey = $"{guidBase}::{index}";
            LevelIDAllocator.GuidCallIndex[guidBase] = index + 1;

            // 3. If frozen, return it
            if (LevelIDAllocator.FreezeTable.TryGetValue(freezeKey, out int frozen))
                return frozen;

            // 4. Build used set (custom levels only)
            HashSet<int> used = new();

            foreach (var lvl in DataMgr.LoadedCustomLevels)
                used.Add(lvl.LevelID);

            foreach (var id in DataMgr.UsedLevelIDs)
                used.Add(id);

            // 5. Deterministic base ID from freezeKey
            int baseId = Math.Abs(freezeKey.GetHashCode()) % 50000 + 10000;
            int candidate = baseId;

            while (used.Contains(candidate))
                candidate++;

            // 6. Freeze and return
            LevelIDAllocator.FreezeTable[freezeKey] = candidate;
            LevelIDAllocator.SaveFreezeTable();

            //UsedLevelIDs.Add(candidate);
            return candidate;
        }
        internal static class LevelIDAllocator
        {
            // === Freeze table backing ===
            static readonly string FreezePath = Path.Combine(Paths.ConfigPath, "LevelIDFreeze.json");
            public static Dictionary<string, int> FreezeTable = new();
            static bool FreezeLoaded = false;

            // Per‑mod call index (not saved; deterministic)
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
                    Debug.LogError($"[LevelIDFreeze] Failed to load freeze table: {e}");
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
                    ModLogger.LogError("[LevelIDFreeze]", $"Failed to save freeze table: {e}");
                }
            }

            // ---------------------------------------------------------
            //  Resolve mod assembly (exact same logic)
            // ---------------------------------------------------------

            public static Assembly? ResolveModAssembly()
            {
                // 1. Fast path
                var calling = Assembly.GetCallingAssembly();
                if (calling != null &&
                    calling != typeof(LevelIDAllocator).Assembly &&
                    calling.GetCustomAttribute<BepInPlugin>() != null)
                {
                    return calling;
                }

                // 2. Stack walk
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

                // 3. Unknown
                return null;
            }


        }
    }
}