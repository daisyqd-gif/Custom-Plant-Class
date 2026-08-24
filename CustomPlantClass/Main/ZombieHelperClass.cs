

namespace CustomPlantClass.Main
{
    public static class ZombieMgr
    {
        private static Vector3 cornerpos = new();
        public static void LosePaper(this PaperZombie self)
        {
            self.theSecondArmorHealth = 0;
            self.TakeDamage(1, self, DamageType.Normal); //It wants an Idamagemaker so I put itself in
        }
        public static void LosePaper(this GatlingPaperZombie_a self)
        {
            self.theSecondArmorHealth = 0;
            self.TakeDamage(1, self, DamageType.Normal); //It wants an Idamagemaker so I put itself in
        }
        public static void FlyAway(this Zombie self)
        {
            if (self == null) return;

            GameObject preview = CreateZombie.CreateZombiePreview(
                self.theZombieType,
                Color.white,
                self.board.transform,
                self.axis.position
            );

            self.Die(2);
            Vector3 target;
            try{
                // get bg SpriteRenderer
                SpriteRenderer sr = self.board.background.transform.Find("bg").Find("bg").GetComponent<SpriteRenderer>();

                // compute top-right corner
                Vector2 size = sr.sprite.bounds.size;
                Vector3 topRightLocal = new Vector3(size.x / 2f, size.y / 2f, 0f);
                Vector3 topRightWorld = sr.transform.TransformPoint(topRightLocal);
                target = topRightWorld + new Vector3(5f, 5f, 0f);
                cornerpos = target;
            }
            catch (NullReferenceException)
            {
                target = cornerpos;
            }
            // start coroutine
            preview.AddComponent<CustomParticle>().StartCoroutine(FlyAwayRoutine(preview, target));
            static IEnumerator FlyAwayRoutine(GameObject obj, Vector3 target)
            {
                float duration = 1.5f;       // total travel time
                float elapsed = 0f;
                float maxSpeed = 20f;        // speed at the end
                float spinSpeed = 1440f;      // degrees per second

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);

                    // slow → fast acceleration curve
                    float speed = maxSpeed * t;

                    // movement
                    obj.transform.position = Vector3.MoveTowards(
                        obj.transform.position,
                        target,
                        speed * Time.deltaTime
                    );

                    // spin (constant or accelerating)
                    obj.transform.RotateAround(obj.transform.TransformPoint(obj.GetCenterLocalSprite()), new Vector3(0f, 0f, 1f), spinSpeed * Time.deltaTime);

                    yield return null;
                }

                obj.transform.position = target;
                Object.Destroy(obj);
            }
        }
        public static void Crashed(this Zombie self)
        {
            if (self == null) return;

            GameObject preview = CreateZombie.CreateZombiePreview(
                self.theZombieType,
                Color.white,
                self.board.transform,
                self.axis.position
            );

            self.Die(2);

            preview.transform.localScale = Vector3.Scale(
                preview.transform.localScale,
                new Vector3(1f, 0.1f, 1f)
            );
            preview.transform.localPosition -= new Vector3(0f, 0.5f, 0f);
            preview.AddComponent<CustomParticle>().StartCoroutine(DieRoutine(preview));
            static IEnumerator DieRoutine(GameObject obj)
            {
                float duration = 1.5f;       // total travel time
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                Object.Destroy(obj);
            }
        }
        internal static void TrySetDTierZombies(Board board, int theWave, int theRound, List<ZombieType> zombieTypes)
        {
            if (theRound >= 13 && board.boardTag.isRogue && board.boardTag.isTravel || theWave > 80 && board.boardTag.rogueShooting)
            {
                foreach (var i in DataMgr.Level4Zombies)
                {
                    if (zombieTypes.Contains(i.Key))
                    {
                        InitZombieList.AddZombieToList(i.Value, theWave);
                    }
                }
            }
        }
    }
}