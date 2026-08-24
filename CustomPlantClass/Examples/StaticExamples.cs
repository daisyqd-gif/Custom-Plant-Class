namespace CustomPlantClass.Examples
{
    public class StaticExamples : MonoBehaviour
    {
        public static void OnLoad()
        {
            Plugin.plugin.AddComponent<StaticExamples>();
        }
        public static void UltimateGatling_StarShoot(BaseCustomPlant plant, BulletType theBulletType)
        {
            plant.StartCoroutine(StarShoot_UltimateGatling_Internal(plant, theBulletType));
        }
        private static IEnumerator StarShoot_UltimateGatling_Internal(BaseCustomPlant plant, BulletType theBulletType)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    PlantMgr.SetBullet(plant._plant, theBulletType, BulletMoveWay.SuperGatling, new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)), Random.Range(-15f, 15f)).normalSpeed = Random.Range(12f, 14f);
                }
                yield return new WaitForFixedUpdate();
            }
        }
        public static void UltimatePlantern_Shrink
        (
            Board board,
            GameObject glow,
            Vector2 position,
            float growTargetScale = 40f,
            float growStep = 0.4f,
            int frameDelayMs = 8,
            float previewShrinkFactor = 0.95f,
            float previewLerpFactor = 0.024f,
            float previewSpinRadians = 0.0001396263f,
            float fadeFactor = 0.95f,
            float minAlpha = 0.01f,
            int doomChancePerTile = 10
        )
        {
            board.StartCoroutine(
                Shrink_Internal(
                    board, glow, position,
                    growTargetScale, growStep,
                    frameDelayMs,
                    previewShrinkFactor, previewLerpFactor, previewSpinRadians,
                    fadeFactor, minAlpha, doomChancePerTile
                )
            );
        }

        private static IEnumerator Shrink_Internal
        (
            Board board,
            GameObject glow,
            Vector2 position,
            float growTargetScale,
            float growStep,
            int frameDelayMs,
            float previewShrinkFactor,
            float previewLerpFactor,
            float previewSpinRadians,
            float fadeFactor,
            float minAlpha,
            int doomChancePerTile
        )
        {
            // ---------------------------------------------------------
            // PHASE 0 — Setup
            // ---------------------------------------------------------
            var sprite = glow.GetComponent<SpriteRenderer>();
            var sorting = glow.GetComponent<SortingGroup>();

            if (sorting != null)
                sorting.enabled = true;

            glow.transform.position = position;
            glow.transform.localScale = Vector3.one;

            if (sprite != null)
                sprite.color = Color.white;

            float delay = frameDelayMs / 1000f;
            WaitForSeconds wait = new WaitForSeconds(delay);

            // ---------------------------------------------------------
            // PHASE 1 — Spawn previews + kill zombies
            // ---------------------------------------------------------
            List<GameObject> previews = new List<GameObject>();
            var zombies = Lawnf.GetAllZombies(false);

            foreach (var z in zombies)
            {
                if (z == null || z.beforeDying)
                    continue;

                var preview = CreateZombie.CreateZombiePreview(
                    z.theZombieType,
                    Color.white,
                    board.transform,
                    z.transform.position
                );

                z.Die();

                if (preview != null)
                    previews.Add(preview);
            }

            // ---------------------------------------------------------
            // PHASE 2 — Animate previews toward center
            // ---------------------------------------------------------
            while (previews.Count > 0)
            {
                for (int i = previews.Count - 1; i >= 0; i--)
                {
                    var p = previews[i];
                    if (p == null)
                    {
                        previews.RemoveAt(i);
                        continue;
                    }

                    var t = p.transform;

                    // Move toward center (faster than Lerp)
                    t.position += (Vector3)(position - (Vector2)t.position) * previewLerpFactor;

                    // Shrink
                    t.localScale *= previewShrinkFactor;

                    // Spin
                    t.Rotate(0f, 0f, previewSpinRadians);

                    // Cull tiny previews
                    if (t.localScale.x < 0.05f)
                    {
                        Object.Destroy(p);
                        previews.RemoveAt(i);
                    }
                }

                yield return wait;
            }

            // ---------------------------------------------------------
            // PHASE 3 — Doom tiles
            // ---------------------------------------------------------
            for (int col = 0; col < board.columnNum; col++)
            {
                for (int row = 0; row < board.rowNum; row++)
                {
                    if (UnityEngine.Random.Range(0, doomChancePerTile) == 0)
                    {
                        Doom.SetDoom(
                            board,
                            new BoardPosition(row, col),
                            DoomType.IceDoom_big,
                            null,
                            false
                        );
                    }
                }
            }

            // ---------------------------------------------------------
            // PHASE 4 — Fade glow
            // ---------------------------------------------------------
            if (sprite != null)
            {
                var c = sprite.color;

                while (c.a > minAlpha)
                {
                    c.a *= fadeFactor;
                    sprite.color = c;
                    yield return wait;
                }
            }

            // ---------------------------------------------------------
            // PHASE 5 — Cleanup
            // ---------------------------------------------------------
            Object.Destroy(glow);
        }
    }
    public static class UltimateTorchBehaviour
    {
        public static Dictionary<BulletType,BulletType> FireTypes = new();
        public static void AddBulletToPool(BulletType fromType, BulletType toType) => FireTypes[fromType]=toType;
    }
    public static class SuperTorchBehaviour
    {
        public static Dictionary<BulletType,(BulletType,int)> FireTypes = new();
        public static void AddBulletToPool(BulletType fromType, BulletType toType, int DmgMultiplier) => FireTypes[fromType]=(toType,DmgMultiplier);
    }
}