namespace CustomPlantClass
{
    public static class EffectsMgr
    {
        private static void Abort(CustomEffect effect)
        {
            if (effect != null && !effect.IsDestroyed())
            {
                Object.DestroyImmediate(effect);
            }
        }
        public static CustomEffect AddEffect<T>(this MonoBehaviour self, float cycleTimer = -1f, float removeTimer = -1f) where T : CustomEffect
        {
            if (self == null || self.IsDestroyed()) return null;
            if (PlantMgr.IsNotNullMonoBehaviour(self.GetOrAddComponent<T>(), out var comp))
            {
                switch (comp.Usage)
                {
                    case CustomEffectUsage.Plant:
                        {
                            if (!self.TryGetComponent<Plant>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.Zombie:
                        {
                            if (!self.TryGetComponent<Zombie>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.Bullet:
                        {
                            if (!self.TryGetComponent<Bullet>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.GridItem:
                        {
                            if (!self.TryGetComponent<GridItem>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                }
                if (cycleTimer > 0f)
                {
                    comp.CycleCountDown += cycleTimer;
                }
                if (removeTimer > 0f)
                {
                    comp.RemoveCountDown += removeTimer;
                }
                return comp;
            }
            return null;
        }
        public static bool TryGetCustomEffect<T>(this MonoBehaviour self, out T effect) where T : CustomEffect => self.TryGetComponent(out effect);
        public static void RemoveEffect<T>(this MonoBehaviour self) where T : CustomEffect
        {
            if (!self.TryGetComponent<T>(out var component)) return;
            component.OnRemoveEffect();
            Object.Destroy(component);
        }
        public static CustomEffect AddEffect<T>(this Transform self, float cycleTimer = -1f, float removeTimer = -1f) where T : CustomEffect
        {
            if (self == null || self.IsDestroyed()) return null;
            if (PlantMgr.IsNotNullMonoBehaviour(self.GetOrAddComponent<T>(), out var comp))
            {
                switch (comp.Usage)
                {
                    case CustomEffectUsage.Plant:
                        {
                            if (!self.TryGetComponent<Plant>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.Zombie:
                        {
                            if (!self.TryGetComponent<Zombie>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.Bullet:
                        {
                            if (!self.TryGetComponent<Bullet>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.GridItem:
                        {
                            if (!self.TryGetComponent<GridItem>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                }
                if (cycleTimer > 0f)
                {
                    comp.CycleCountDown += cycleTimer;
                }
                if (removeTimer > 0f)
                {
                    comp.RemoveCountDown += removeTimer;
                }
                return comp;
            }
            return null;
        }
        public static bool TryGetCustomEffect<T>(this Transform self, out T effect) where T : CustomEffect => self.TryGetComponent(out effect);
        public static void RemoveEffect<T>(this Transform self) where T : CustomEffect
        {
            if (!self.TryGetComponent<T>(out var component)) return;
            component.OnRemoveEffect();
            Object.Destroy(component);
        }
        public static CustomEffect AddEffect<T>(this GameObject self, float cycleTimer = -1f, float removeTimer = -1f) where T : CustomEffect
        {
            if (self == null || self.IsDestroyed()) return null;
            if (PlantMgr.IsNotNullMonoBehaviour(self.GetOrAddComponent<T>(), out var comp))
            {
                switch (comp.Usage)
                {
                    case CustomEffectUsage.Plant:
                        {
                            if (!self.TryGetComponent<Plant>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.Zombie:
                        {
                            if (!self.TryGetComponent<Zombie>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.Bullet:
                        {
                            if (!self.TryGetComponent<Bullet>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                    case CustomEffectUsage.GridItem:
                        {
                            if (!self.TryGetComponent<GridItem>(out var _))
                            {
                                Abort(comp);
                                return null;
                            }
                            break;
                        }
                }
                if (cycleTimer > 0f)
                {
                    comp.CycleCountDown += cycleTimer;
                }
                if (removeTimer > 0f)
                {
                    comp.RemoveCountDown += removeTimer;
                }
                return comp;
            }
            return null;
        }
        public static bool TryGetCustomEffect<T>(this GameObject self, out T effect) where T : CustomEffect => self.TryGetComponent(out effect);
        public static void RemoveEffect<T>(this GameObject self) where T : CustomEffect
        {
            if (!self.TryGetComponent<T>(out var component)) return;
            component.OnRemoveEffect();
            Object.Destroy(component);
        }
    }
    public class CustomEffect : MonoBehaviour
    {
        public virtual bool CanCountDown { get; } = false;
        public virtual bool TimedEffect { get; } = false;
        public virtual float CycleCountDown { get; set; } = 0f;
        public virtual float RemoveCountDown { get; set; } = 0f;
        public virtual CustomEffectUsage Usage { get => CustomEffectUsage.Other; }
        private float AttrCountDown = 0f;
        private float RemoveCd = 0f;
        public void Start()
        {
            AttrCountDown = CycleCountDown;
            RemoveCd = RemoveCountDown;
            OnAddEffect();
        }
        public void FixedUpdate()
        {
            if (CanCountDown)
            {
                AttrCountDown -= Time.deltaTime;
                if (AttrCountDown <= 0)
                {
                    OnTimerZero();
                    AttrCountDown = CycleCountDown;
                }
            }
            if (TimedEffect)
            {
                RemoveCd -= Time.deltaTime;
                if (RemoveCd <= 0)
                {
                    OnRemoveEffect();
                    Destroy(this);
                }
            }
        }
        public virtual void OnAddEffect() { }
        public virtual void OnTimerZero() { }
        public virtual void OnRemoveEffect() { }
    }
    public enum CustomEffectUsage
    {
        Plant = 0,
        Zombie = 1,
        Bullet = 2,
        GridItem = 3,
        Other = 4
    }
}
