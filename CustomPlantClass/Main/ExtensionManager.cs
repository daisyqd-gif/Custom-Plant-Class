namespace CustomPlantClass.Main
{
    public static class ExtensionManager
    {
        public static GridItemType ToGridItemType(this ID self)
        {
            return (GridItemType)self.id;
        }
        public static ID ToID(this GridItemType self)
        {
            return (int)self;
        }
        public static int AddAndGetIndex<T>(this List<T> self, T item)
        {
            int counter = self.Count;
            self.Add(item);
            return counter;
        }
        public static int AddAndGetIndex<T>(this Il2CppSystem.Collections.Generic.List<T> self, T item)
        {
            int counter = self.Count;
            self.Add(item);
            return counter;
        }
        public static Component AddComponent(this MonoBehaviour self, Il2CppSystem.Type type)
        {
            return self.gameObject.AddComponent(type);
        }
        public static Component GetOrAddComponent(this MonoBehaviour self, Il2CppSystem.Type type)
        {
            if (self.TryGetComponent(type, out var component)) return component;
            return self.gameObject.AddComponent(type);
        }
    }
    public static class Il2CppExtensions
    {
        public static HashSet<T> ToSystemHashSet<T>(this Il2CppSystem.Collections.Generic.HashSet<T> self)
        {
            return [.. self];
        }
        public static Il2CppSystem.Collections.Generic.HashSet<T> ToIl2CppHashSet<T>(this IEnumerable<T> self)
        {
            var output = new Il2CppSystem.Collections.Generic.HashSet<T>();
            self.Where((T element) =>
            {
                output.Add(element);
                return true;
            });
            return output;
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.HashSet<T> self, Il2CppSystem.Collections.Generic.HashSet<T> value)
        {
            value.ToSystemHashSet().Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.HashSet<T> self, HashSet<T> value)
        {
            value.Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.HashSet<T> self, Il2CppSystem.Collections.Generic.List<T> value)
        {
            value.ToSystemList().Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.HashSet<T> self, List<T> value)
        {
            value.Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.List<T> self, Il2CppSystem.Collections.Generic.List<T> value)
        {
            value.ToSystemList().Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.List<T> self, List<T> value)
        {
            value.Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.List<T> self, Il2CppSystem.Collections.Generic.HashSet<T> value)
        {
            value.ToSystemHashSet().Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
        public static void Merge<T>(this Il2CppSystem.Collections.Generic.List<T> self, HashSet<T> value)
        {
            value.Where((T element) =>
            {
                self.Add(element);
                return true;
            });
        }
    }
}
