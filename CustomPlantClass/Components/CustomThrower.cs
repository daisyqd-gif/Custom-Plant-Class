namespace CustomPlantClass
{
    public class CustomThrower : CustomShooter
    {
        protected Vector2 firstPostion;
        protected float firstTime;
        protected virtual float flightTime { get; set; } = 1.5f;
        protected virtual bool CheckRange(Zombie zombie) => _plant.axis.position.x < zombie.axis.position.x;
        protected virtual Zombie ThrowerSearchZombie()
        {
            var board = _plant.board;
            if (board == null || board.zombieArray == null)
                return null;

            Zombie best = null;
            float bestDist = float.MaxValue;

            foreach (var z in board.zombieArray)
            {
                if (z == null) continue;
                if (z.theZombieRow != _plant.thePlantRow) continue;
                if (z.axis == null) continue;

                var zPos = z.axis.position;
                float vision = _plant.vision;

                // must be within vision range
                if (!(zPos.x < vision)) continue;

                // your virtual filter
                if (!CheckRange(z)) continue;

                // extra static filter (umbrella / special cases)
                if (!Thrower.ThrowSearchZombie(z)) continue;

                // pick closest in front
                var plantPos = _plant.axis.position;
                float dist = Mathf.Abs(zPos.x - plantPos.x);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = z;
                }
            }

            _plant.targetZombie = best;

            // fallback: boss
            if (best == null)
            {
                var bossObj = _plant.SearchBoss(); // virtual on plant
                if (bossObj != null)
                {
                    best = bossObj.GetComponent<Zombie>();
                    _plant.targetZombie = best;
                }
            }

            return best;
        }
        public override void PlantShootUpdate()
        {
            _plant.UpdateAttackCountDown();
            if (_plant.thePlantAttackCountDown > 0) return;
            _plant.thePlantAttackCountDown = _plant.thePlantAttackInterval * Random.Range(0.95f, 1.05f);
            Zombie zombie = ThrowerSearchZombie();
            if (zombie == null || zombie.col == null)
                return;
            Vector2 targetPos = GetZombiePosition(zombie);
            firstPostion = targetPos;
            _plant.anim.SetTriggerString("shoot");
            firstTime = Time.time;
        }
        protected virtual void ShootZombie(Zombie zombie)
        {
            // must have a shoot point
            if (_plant.shoot == null || zombie == null)
                return;

            // where the projectile starts
            Vector2 projectilePosition = _plant.shoot.position;

            // target motion + position
            Vector2 targetVelocity = zombie.Velocity;
            Vector2 targetCurrentPosition = zombie.ColliderPosition;

            // Lawnf helper: compute arc parameters for a given flight time
            // returns something like: [t, vx, vy, gravityOrDeltaVy]
            float[] arc = MathHelper.CalculateProjectileWithSpeed(
                projectilePosition,
                targetVelocity,
                targetCurrentPosition,
                flightTime
            );

            if (arc == null || arc.Length < 4)
                return;

            // spawn bullet at shoot point
            Bullet b = PlantMgr.SetBullet(
                _plant,
                GetBulletType(),
                BulletMoveWay.Throw,
                _plant.attackDamage * 2
            );

            if (b == null)
                return;

            b.ThrowTo(zombie);

            if (_plant.melonSputter)
                b.melonSputter = true;

            // hook for subclasses (Winter, Lava, Gold, etc.)
            UniqueEffect(b);
        }
        // v = projectile start position (shoot point)
        // returns (arcParams, umbrellaPlant)
        protected virtual (float[] arc, Plant umbrella) FindUmbrella(Vector2 v)
        {
            var board = _plant.board;
            if (board == null)
                return (null, null);

            var gridSystem = board.gridSystem;
            if (gridSystem == null)
                return (null, null);

            int col = _plant.thePlantColumn;
            int row = _plant.thePlantRow;

            float[] bestArc = null;
            Plant bestUmbrella = null;

            // scan columns from this plant to the right
            while (col < board.columnNum)
            {
                var grid = gridSystem.GetGrid(col, row);
                if (grid == null || grid.plants == null)
                    break;

                foreach (var p in grid.plants)
                {
                    if (p == null)
                        continue;

                    // decompiler mangled types; this is really "is umbrella plant?"
                    if (!IsUmbrella(p))
                        continue;

                    var pos = p.axis.position;

                    // must be to the left of firstPostion.x (where we first targeted)
                    if (pos.x >= firstPostion.x)
                        continue;

                    // compute where umbrella will be at catch time
                    // vanilla adds +0.75f on Y
                    var umbrellaPos = new Vector2(pos.x, pos.y + 0.75f);

                    float t1 = firstTime;          // when we first locked target
                    float t2 = Time.time;          // now

                    // Lawnf helper: solve arc that goes from v → umbrellaPos over flightTime,
                    // given timing info (t1, t2)
                    float[] arc = MathHelper.CalculateProjectileParameters(
                        v,
                        t1,
                        umbrellaPos,   // "firstPlace"
                        t2,
                        umbrellaPos,   // "secondPlace" (same here)
                        flightTime
                    );

                    if (arc != null)
                    {
                        bestArc = arc;
                        bestUmbrella = p;
                        break;
                    }
                }

                if (bestArc != null)
                    break;

                col++;
            }

            return (bestArc, bestUmbrella);
        }
        // Called by animation event
        public override Bullet Shoot_Custom()
        {
            if (_plant.shoot == null)
                return null;

            Vector2 startPos = _plant.shoot.position;

            var creator = CreateBullet.Instance;
            if (creator == null)
                return null;

            int row = _plant.thePlantRow;
            var bulletType = GetBulletType();

            Bullet b = creator.SetBullet(
                startPos.x,
                startPos.y,
                row,
                bulletType,
                BulletMoveWay.Throw,
                false
            );

            if (b == null)
                return null;

            // 1) Umbrella branch
            (var _, Plant umbrella) = FindUmbrella(startPos);
            if (umbrella != null)
            {
                b.ThrowTo(umbrella,new(startPos),new(flightTime));
                goto ApplyCommon;
            }

            // 2) Zombie branch
            Zombie z = _plant.targetZombie;
            if (z != null && z.col != null)
            {
                b.ThrowTo(z,new(startPos),new(flightTime));
                goto ApplyCommon;
            }

            // 3) Fallback: grid-based speed
            Vector2 targetVelocity = Vector2.zero;
            Board board = _plant.board;
            if (board != null && board.gridSystem != null)
            {
                var grid = board.gridSystem.GetGrid(board.columnNum - 1, _plant.thePlantRow);
                if (grid != null)
                {
                    Vector2 targetPos = grid.Position;
                    b.SetSpeed(startPos, targetVelocity, targetPos, flightTime);
                    goto ApplyCommon;
                }
            }

            return null;

        ApplyCommon:
            b.Damage=_plant.attackDamage;
            b.fromType = _plant.thePlantType;
            b.melonSputter = _plant.melonSputter;

            UniqueEffect(b);

            _plant.targetZombie = null;

            int soundId = Random.RandomRangeInt(3, 5);
            GameAPP.PlaySound(soundId, 0.5f, 1f);

            return b;
        }
        // used in MelonShoot: .OrderBy(z => MelonOrderKey(z))
        protected virtual float MelonOrderKey(Zombie z)
        {
            if (z == null || z.axis == null || _plant.axis == null)
                return float.PositiveInfinity;

            Vector3 zPos = z.axis.position;
            Vector3 plantPos = _plant.axis.position;

            float dx = zPos.x - plantPos.x;
            float dy = zPos.y - plantPos.y;

            // Euclidean distance
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
        protected virtual bool IsUmbrella(Plant p)
        {
            // this is where you map the weird IL2CPP checks
            // to something sane like:
            return p.thePlantType == PlantType.CabbageUmbrella
                || p.thePlantType == PlantType.EmeraldUmbrella;
        }
        // Called by a second animation event (e.g. double‑lob, splash, etc.)
        public override Bullet Shoot2_Custom()
        {
            if (_plant.shoot == null)
                return null;

            Vector2 startPos = _plant.shoot.position;

            var creator = CreateBullet.Instance;
            if (creator == null)
                return null;

            int row = _plant.thePlantRow;
            var bulletType = GetBulletType();

            Bullet b = creator.SetBullet(
                startPos.x,
                startPos.y,
                row,
                bulletType,
                BulletMoveWay.Throw,
                false
            );

            if (b == null)
                return null;

            // 1) Umbrella branch
            (var _, Plant umbrella) = FindUmbrella(startPos);
            if (umbrella != null)
            {
                var arcStart = new Il2CppSystem.Nullable<Vector2>(startPos);

                b.ThrowTo(umbrella,arcStart);
                goto ApplyCommon;
            }

            // 2) Zombie branch
            Zombie z = _plant.targetZombie;
            if (z != null && z.col != null)
            {
                var arcStart = new Il2CppSystem.Nullable<Vector2>(startPos);

                b.ThrowTo(z, arcStart);
                goto ApplyCommon;
            }

            // 3) Fallback: grid-based speed
            Vector2 targetVelocity = Vector2.zero;
            Board board = _plant.board;
            if (board != null && board.gridSystem != null)
            {
                var grid = board.gridSystem.GetGrid(board.columnNum - 1, _plant.thePlantRow);
                if (grid != null)
                {
                    Vector2 targetPos = grid.Position;
                    b.SetSpeed(startPos, targetVelocity, targetPos, 2);
                    goto ApplyCommon;
                }
            }

            return null;

        ApplyCommon:
            b.Damage=_plant.attackDamage;
            b.fromType = _plant.thePlantType;
            b.melonSputter = _plant.melonSputter;

            UniqueEffect(b);

            _plant.targetZombie = null;

            int soundId = Random.RandomRangeInt(3, 5);
            GameAPP.PlaySound(soundId, 0.5f, 1f);

            return b;
        }
        protected virtual void UniqueEffect(Bullet b) { }
        public override BulletType GetBulletType() => BulletType.Bullet_cabbage;
        public override BulletType GetBulletType2() => GetBulletType();
        protected virtual Vector2 GetZombiePosition(Zombie zombie)
        {
            if (zombie == null || zombie.col == null)
                return default;

            var bounds = zombie.col.bounds;

            // NORMAL ZOMBIES
            if (zombie.theZombieType != ZombieType.ZombieBoss && zombie.theZombieType != ZombieType.ZombieBoss2) //the 2 zombie types
            {
                // aim at the TOP of the collider
                return new Vector2(
                    bounds.center.x,
                    bounds.center.y + bounds.extents.y
                );
            }

            // SPECIAL ZOMBIES (0x2C, 0x2E) — use land height instead
            var mouse = Mouse.Instance;
            if (mouse != null)
            {
                float landY = mouse.GetLandY(8.5f, _plant.thePlantRow);
                return new Vector2(8.5f, landY + 0.3f);
            }

            return default;
        }
    }
}