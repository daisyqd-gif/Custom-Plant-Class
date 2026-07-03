#nullable enable
namespace CustomPlantClass.Main
{
    public static class ModRegistryManager
    {
        // name -> typed registry (boxed as object)
        private static readonly Dictionary<string, object> registries = new();

        // ---------- PUBLIC API ----------

        /// <summary>
        /// Creates a registry with a specific item type T.
        /// Only the owning mod should call this.
        /// </summary>
        public static void CreateRegistry<T>(string name)
        {
            if (registries.TryGetValue(name, out var existing))
            {
                // If registry exists but type mismatches, warn
                if (existing is not Registry<T>)
                {
                    ModLogger.LogWarn(
                        $"Registry '{name}' already exists with a different type. " +
                        $"Expected: {existing.GetType().FullName}, got: {typeof(Registry<T>).FullName}"
                    );
                }
                return;
            }

            registries[name] = new Registry<T>();
            ModLogger.LogInfo($"Created registry {name} of type {typeof(T).FullName}.");
        }

        /// <summary>
        /// Adds an item to a registry of type T.
        /// Other mods call this to add items.
        /// </summary>
        public static void AddToRegistry<T>(string name, T item)
        {
            DataMgr.AddGameStartAction(() =>
            {
                if (!registries.TryGetValue(name, out var obj))
                {
                    ModLogger.LogWarn(
                        $"Registry '{name}' not found. " +
                        $"Did the owning mod forget to call CreateRegistry<T>()?"
                    );
                    return;
                }

                if (obj is Registry<T> reg)
                {
                    reg.Items.Add(item);
                }
                else
                {
                    ModLogger.LogWarn(
                        $"Registry '{name}' exists but has a different type than {typeof(T).FullName}."
                    );
                }
            });
        }

        /// <summary>
        /// Gets a registry of type T.
        /// </summary>
        public static List<T>? GetRegistry<T>(string name)
        {
            if (registries.TryGetValue(name, out var obj) && obj is Registry<T> reg)
                return reg.Items;

            return null;
        }

        /// <summary>
        /// Try-get pattern for safer access.
        /// </summary>
        public static bool TryGetRegistry<T>(string name, out List<T> registry)
        {
            if (registries.TryGetValue(name, out var obj) && obj is Registry<T> reg)
            {
                registry = reg.Items;
                return true;
            }

            registry = null!;
            return false;
        }

        // ---------- INTERNAL TYPES ----------

        private class Registry<T>
        {
            public List<T> Items { get; } = new();
        }
    }
}
