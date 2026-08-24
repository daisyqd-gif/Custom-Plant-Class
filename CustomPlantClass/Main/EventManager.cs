namespace CustomPlantClass.Main
{
    public static class InterModCommunications
    {
        private static readonly HashSet<string> presentMods = new();
        private static readonly List<IModEventListener> listeners = new();

        // ---------- MOD PRESENCE ----------
        public static void RegisterMod(string modId)
        {
            presentMods.Add(modId);
            ModLogger.LogInfo($"[IMC] Registered mod: {modId}");
        }

        public static bool IsModPresent(string modId)
        {
            return presentMods.Contains(modId);
        }

        // ---------- LISTENER REGISTRATION ----------
        public static void RegisterListener(IModEventListener listener)
        {
            listeners.Add(listener);
            ModLogger.LogInfo($"[IMC] Registered listener: {listener.GetType().Name}");
        }

        // ---------- SINGLE CHANNEL SEND ----------
        public static void Send(string sender, string eventName, object data)
        {
            foreach (var listener in listeners)
            {
                try
                {
                    DispatchToListener(listener, sender, eventName, data);
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"[IMC] Listener error: {ex}");
                }
            }
        }

        public static void PingAll()
        {
            Send("IMC", "PING", "None");
        }

        // ---------- DISPATCH ----------
        private static void DispatchToListener(IModEventListener listener, string sender, string eventName, object data)
        {
            // AUTO-PONG FIRST (no mod code needed)
            Send("IMC", "PONG", sender);

            // THEN mod logic
            listener.OnModMessage(sender, eventName, data);
        }
    }

    // ---------- LISTENER INTERFACE ----------
    public interface IModEventListener
    {
        void OnModMessage(string sender, string eventName, object data);
    }
}
