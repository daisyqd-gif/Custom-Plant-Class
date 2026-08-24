namespace CustomPlantClass.Main
{
    public static class SynergyManager
    {
        // Mods that approved synergy
        private static readonly HashSet<string> approvedMods = new();

        static SynergyManager()
        {
            // Register synergy listener
            InterModCommunications.RegisterListener(new SynergyListener());
        }

        // ---------- PUBLIC API ----------

        // Mod A calls this to ask Mod B
        public static void AskForSynergy(string requesterModId, string targetModId)
        {
            InterModCommunications.Send(requesterModId, "SYNERGY_PING", targetModId);
        }

        // Mod A checks if Mod B approved synergy
        public static bool ShouldLoadSynergy(string targetModId)
        {
            return approvedMods.Contains(targetModId);
        }

        // ---------- INTERNAL LISTENER ----------
        private class SynergyListener : IModEventListener
        {
            public void OnModMessage(string sender, string eventName, object data)
            {
                switch (eventName)
                {
                    // Mod A asks Mod B
                    case "SYNERGY_PING":
                        if (data is string targetModId && sender == targetModId)
                        {
                            // Mod B auto-approves synergy
                            InterModCommunications.Send(targetModId, "SYNERGY_PONG", targetModId);
                        }
                        break;

                    // Mod B responds
                    case "SYNERGY_PONG":
                        if (data is string modId)
                        {
                            approvedMods.Add(modId);
                            ModLogger.LogInfo($"[Synergy] {modId} approved synergy load.");
                        }
                        break;
                }
            }
        }
    }
}
