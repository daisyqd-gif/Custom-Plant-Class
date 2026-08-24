namespace CustomPlantClass.Runtime
{
    public class BoardBehaviour : MonoBehaviour
    {
        public Board board => GetComponent<Board>();
        public static List<Action<Board>> StartEvents = new();
        public static List<Action<Board>> UpdateEvents = new();
        public static List<Action<Board>> FixedUpdateEvents = new();
        public static List<Action<Board>> DestroyEvents = new();
        public static void AddStartEvent(Action<Board> action)
        {
            StartEvents.Add(action);
        }
        public static void AddUpdateEvent(Action<Board> action)
        {
            UpdateEvents.Add(action);
        }
        public static void AddFixedUpdateEvent(Action<Board> action)
        {
            FixedUpdateEvents.Add(action);
        }
        public static void AddDestroyEvent(Action<Board> action)
        {
            DestroyEvents.Add(action);
        }
        public void Start()
        {
            ResetScanner.EnsureScanned();
            foreach (var i in StartEvents)
            {
                try
                {
                    i(board);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        public void Update()
        {
            foreach (var i in UpdateEvents)
            {
                try
                {
                    i(board);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        public void FixedUpdate()
        {
            foreach (var i in FixedUpdateEvents)
            {
                try
                {
                    i(board);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
        public void OnDestroy()
        {
            ResetRegistry.ResetAll();
            foreach (var i in DestroyEvents)
            {
                try
                {
                    i(board);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ResetOnBoardDestroyAttribute : Attribute
    {
        public object DefaultValue { get; }

        public ResetOnBoardDestroyAttribute() { }

        public ResetOnBoardDestroyAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }
    public static class ResetRegistry
    {
        private static readonly List<(FieldInfo field, object defaultValue)> entries = new();

        public static void Register(FieldInfo field, object defaultValue)
        {
            entries.Add((field, defaultValue));
        }

        public static void ResetAll()
        {
            foreach (var (field, defaultValue) in entries)
            {
                Debug.Log($"Defaulted field {field.Name}.");
                field.SetValue(null, defaultValue); // static fields
            }
        }
    }
    public static class ResetScanner
    {
        private static bool initialized = false;

        public static void EnsureScanned()
        {
            if (initialized) return;
            initialized = true;

            ScanAllManagedAssemblies();
        }

        public static void ScanType(Type type)
        {
            var fields = type.GetFields(
                BindingFlags.Static | BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<ResetOnBoardDestroyAttribute>();
                if (attr == null) continue;

                object defaultValue = attr.DefaultValue;

                if (defaultValue == null)
                    defaultValue = field.FieldType.IsValueType
                        ? Activator.CreateInstance(field.FieldType)
                        : null;

                ResetRegistry.Register(field, defaultValue);
                Debug.Log($"Found field defaulter for field {field.Name}.");
            }
        }
        private static void ScanAllManagedAssemblies()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Skip Unity/IL2CPP engine assemblies
                if (asm.FullName.StartsWith("Unity") ||
                    asm.FullName.StartsWith("System") ||
                    asm.FullName.StartsWith("mscorlib"))
                    continue;

                ScanAssembly(asm);
            }
        }
        private static void ScanAssembly(Assembly asm)
        {
            try{
                foreach (var type in asm.GetTypes())
                    ScanType(type);
            }
            catch (Exception)
            {
                
            }
        }
    }
    
}
