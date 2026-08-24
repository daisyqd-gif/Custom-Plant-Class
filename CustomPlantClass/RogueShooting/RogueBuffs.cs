

namespace CustomPlantClass.RogueShootingMgr
{
    public abstract class CustomRogueBuff
    {
        public abstract PlantType ShowType { get; }
        public abstract string Title { get; }
        public abstract string Description { get; }
        public virtual int MaxCount { get; }
        public virtual bool CanAppear { get; }
        public virtual bool Passive { get; }
        public virtual bool General { get; }
        public virtual float AppearWeight { get; }
        protected string PlantName { get => Lawnf.GetName(ShowType); }
        protected Plant Plant
        {
            get
            {
                if (ShootingManager.Instance.TryGetPlant(ShowType, out var plant))
                {
                    return plant;
                }
                return null;
            }
        }
        public abstract Quality Rarity { get; }
        public abstract void OnGet();
    }
    public abstract class GeneralCustomBuff : CustomRogueBuff
    {
        protected PlantType plantType; // 0x10
        protected Quality randomQuality; // 0x14

        public override PlantType ShowType { get => plantType; }
        public override int MaxCount { get => 1000; }
        public override bool General { get => true; }
        public override Quality Rarity { get => randomQuality; }
    }
    public class CustomDamageBuff : GeneralCustomBuff // TypeDefIndex: 2881
    {
        // Fields
        private const float BaseDamage = 0.3f;
        private float FinalAmount
        {
            get
            {
                var q = randomQuality;   // 0 = white, 1 = green, 2 = blue, 3 = purple
                var sm = ShootingManager.Instance;

                // If super upgrade is active AND quality is purple
                if (sm.superUpgrade && q == Quality.diamond)
                    return 7.5f;

                // Normal quality scaling
                switch (q)
                {
                    case Quality.Default: return 0.3f;
                    case Quality.silver: return 0.6f;
                    case Quality.gold: return 0.9f;
                    case Quality.diamond: return 1.5f;
                }

                return 0f;
            }
        }
        public override string Title { get => "强化：力量"; }
        public override string Description { get => $"{PlantName}获得{FinalAmount * 100f:F0}%独立伤害增幅\n当前增幅：{TravelMgr.Instance.data.GetDamageMultiplier(plantType) * 100f:F0}%"; }
        public override void OnGet()
        {
            TravelMgr.Instance.data.AddDamage(plantType, FinalAmount);
        }
        public CustomDamageBuff(PlantType type)
        {
            plantType = type;
            randomQuality = ShootingManager.Instance.GetRandomQuality();
        }
    }
    public class CustomSpeedBuff : GeneralCustomBuff // TypeDefIndex: 2881
    {
        // Fields
        private const float BaseSpeed = 0.2f;
        private float FinalAmount
        {
            get
            {
                var q = randomQuality;   // 0 = white, 1 = green, 2 = blue, 3 = purple
                var sm = ShootingManager.Instance;

                // If super upgrade is active AND quality is purple
                if (sm.superUpgrade && q == Quality.diamond)
                    return 5.0f;

                // Normal quality scaling
                switch (q)
                {
                    case Quality.Default: return 0.2f;
                    case Quality.silver: return 0.4f;
                    case Quality.gold: return 0.6f;
                    case Quality.diamond: return 1f;
                }

                return 0f;
            }
        }
        public override string Title { get => "强化：速度"; }
        public override string Description { get => $"{PlantName}获得{FinalAmount * 100f:F0}%速度增幅\n当前增幅：{TravelMgr.Instance.data.GetSpeed(plantType) * 100f:F0}%"; }
        public override void OnGet()
        {
            TravelMgr.Instance.data.AddSpeed(plantType, FinalAmount);
        }
        public CustomSpeedBuff(PlantType type)
        {
            plantType = type;
            randomQuality = ShootingManager.Instance.GetRandomQuality();
        }
    }
    public class CustomUpgradeBuff : CustomRogueBuff // TypeDefIndex: 2881
    {
        // Fields
        private readonly PlantType sourceType; // 0x10
        private readonly PlantType targetType; // 0x14
        public override PlantType ShowType { get => sourceType; }
        public override bool General { get => true; }
        public override Quality Rarity { get => Quality.silver; }
        public override string Title { get => "升级"; }
        public override string Description { get => $"{PlantName}升级到{targetType}"; }
        public override void OnGet()
        {
            ShootingManager.Instance.UpgradePlant(sourceType, targetType);
        }
        public CustomUpgradeBuff(PlantType from, PlantType to)
        {
            sourceType = from; // 0x10
            targetType = to; // 0x14
        }
    }
}