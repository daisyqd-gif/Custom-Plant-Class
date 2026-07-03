#nullable enable

namespace CustomPlantClass
{
    public class CustomOnZombieBomb : CustomEffect
    {
        public Zombie zombie { get => transform.GetComponent<Zombie>(); }
        public override CustomEffectUsage Usage { get => CustomEffectUsage.Zombie; }
        public virtual GameObject obj { get => null!; }
        public int Damage { get; set; } = 0;
        public override bool CanCountDown { get; } = true;
        public override bool TimedEffect { get; } = true;
        public override float CycleCountDown { get; set; } = 1f;
        public override float RemoveCountDown { get; set; } = 5f;
        public virtual PlantType FromType { get; } = PlantType.Nothing;
        private GameObject obj_cache = null!;
        public Board board => Board.Instance;
        public override void OnAddEffect()
        {
            if (zombie == null)
            {
                Destroy(this);
                return;
            }
            var parentScale = transform.parent.localScale;
            var localScale = transform.localScale;

            transform.localScale = new Vector3(
                localScale.x * 0.3f / parentScale.x,
                localScale.y * 0.3f / parentScale.y,
                localScale.z * 0.3f / parentScale.z
            );
            if (obj != null)
            {
                obj_cache = Instantiate(obj, zombie.axis.transform.position + new Vector3(0f, 0.95f, 0f), Quaternion.identity, zombie.transform);
            }
        }
        public override void OnTimerZero()
        {
            if (board != null) board.boardAction.CreateCherryExplode(transform.position, PlantMgr.getRow(transform.position.y), CherryBombType.Bullet, Mathf.RoundToInt(Damage / 4));
        }
        public override void OnRemoveEffect()
        {
            if (board != null) board.boardAction.CreateCherryExplode(transform.position, PlantMgr.getRow(transform.position.y), CherryBombType.Normal, Damage);
            if (obj_cache != null && !obj_cache.IsDestroyed()) Destroy(obj_cache);
        }
    }
    [Obsolete("Use the custom effect version")]
    public class CustomOnZombieComponent : MonoBehaviour
    {
        public static GameObject? bomb { get; set; }
        public int Damage { get; set; } = 0;
        public float deathCD = 100f;
        public virtual float deathTime { get; set; } = 5f;
        public float effectCD = 100f;
        public virtual float effectTime { get; set; } = 1f;
        public virtual PlantType FromType { get; set; } = PlantType.Nothing;
        public Board? board;
        public void AddDamage(int damage) => Damage += damage;
        public void Start()
        {
            board = InstanceManager.Board;
            if (board == null)
            {
                Destroy(gameObject);
                return;
            }

            var parentScale = transform.parent.localScale;
            var localScale = transform.localScale;

            transform.localScale = new Vector3(
                localScale.x * 0.3f / parentScale.x,
                localScale.y * 0.3f / parentScale.y,
                localScale.z * 0.3f / parentScale.z
            );
            deathCD = deathTime;
            effectCD = effectTime;
        }
        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame)
            {
                deathCD -= Time.deltaTime;
                if (deathCD <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }
                effectCD -= Time.deltaTime;
                if (effectCD <= 0f)
                {
                    AttrEffect();
                    effectCD = effectTime;
                    return;
                }
            }
        }
        public void OnDestroy() => DestroyEffect();
        public virtual void AttrEffect()
        {
            if (board != null) board.boardAction.CreateCherryExplode(transform.position, PlantMgr.getRow(transform.position.y), CherryBombType.Bullet, Mathf.RoundToInt(Damage / 4));
        }
        public virtual void DestroyEffect()
        {
            if (board != null) board.boardAction.CreateCherryExplode(transform.position, PlantMgr.getRow(transform.position.y), CherryBombType.Normal, Damage);
        }
        public static CustomOnZombieComponent? TryAddBomb(Zombie zombie)
        {
            if (zombie.gameObject == null || zombie == null || zombie.IsDestroyed()) return null;
            var component = zombie.GetComponentInChildren<CustomOnZombieComponent>();
            if (component != null)
                return component;
            var result = Instantiate(bomb, zombie.axis.transform.position + new Vector3(0f, 0.95f, 0f), Quaternion.identity, zombie.transform);
            if (result == null) return null;
            return result.GetComponent<CustomOnZombieComponent>();
        }
    }
}
