#nullable enable

using CustomPlantClass.Examples;

namespace CustomPlantClass
{
    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlant_Patch
    {
        // -----------------------------
        // MIX REPLACEMENT
        // -----------------------------
        [HarmonyPatch(nameof(CreatePlant.CheckMix))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void CheckMix_Postfix(CreatePlant __instance, ref Plant __result)
        {
            if (__result == null || !Utils.InGame()) return;

            if (!DataMgr.replaceList.TryGetValue(__result.thePlantType, out var entry))
                return;

            var (newType, condition) = entry;

            if (condition(__result) && GameAPP.theGameStatus == GameStatus.InGame)
            {
                int row = __result.thePlantRow;
                int col = __result.thePlantColumn;

                __result.Die(DieReason.ByMix);
                __result = __instance.SetPlant(col, row, newType, null, default, true);
            }
        }

        // -----------------------------
        // TRAVEL LIMITS FOR CUSTOM ULTIMATES
        // -----------------------------
        [HarmonyPatch(nameof(CreatePlant.LimTravel))]
        [HarmonyPostfix]
        public static void LimTravel_Postfix(CreatePlant __instance, PlantType theSeedType, ref bool __result)
        {
            // If already blocked by base game, don't override
            if (__result) return;

            Board board = __instance.board;
            if (board == null)
            {
                __result = false;
                return;
            }

            if (board.boardTag.enableAllTravelPlant || board.boardTag.isSuperRandom) return;
            if (!board.boardTag.enableTravelPlant)
            {
                __result = true;
                InGameText.Instance.ShowText("该配方仅旅行模式或深渊可用", 3f, false);
                return;
            }
            bool isStrong = DataMgr.CustomStrongUltiPlants.ContainsKey((int)theSeedType);
            if (isStrong)
            {
                if (/*TravelMgr.Instance == null || */!(TravelDictionary.PlantToUnlock.TryGetValue(theSeedType, out var val) && DataMgr.CustomTravelUnlocks.Contains(val) && Lawnf.TravelUnlock(val)))
                //!TravelMgr.Instance.data.unlockedPlants.ToSystemList()
                //    .Any(n => DataMgr.CustomStrongUltiPlants.Values
                //        .Any(n2 => n2.unlock == n)))
                {
                    __result = true;
                    InGameText.Instance.ShowText("该配方需要抽取", 3f, false);
                    return;
                }
            }

            // All checks passed → allow planting
            __result = false;
        }//*/
    }

    [HarmonyPatch(typeof(CreateZombie))]
    [HarmonyPriority(Priority.Last)]
    public static class CreateZombie_Patch
    {
        [HarmonyPatch(nameof(CreateZombie.SetZombie))]
        [HarmonyPostfix]
        public static void SetZombie_Postfix(CreateZombie __instance, ref Zombie __result, ref bool isIdle)
        {
            if (__result == null || !Utils.InGame()) return;
            if (!DataMgr.onZombieTypeSpawnActionList.ContainsKey(__result.theZombieType)) return;
            var a = DataMgr.onZombieTypeSpawnActionList[__result.theZombieType];
            if (a.Item2.Invoke(__result) && GameAPP.theGameStatus == GameStatus.InGame)
            {
                var row = __result.theZombieRow;
                var x = __result.transform.position.x;
                __result = __instance.SetZombie(row, a.Item1, x, isIdle);//;)
            }
            else if (__instance.board.TryGetComponent<CustomLevelComponent>(out var customLevelComponent))
            {
                customLevelComponent.OnZombieCreate(__result);
            }
        }
    }

    [HarmonyPatch(typeof(TreasureData))]
    public static class TreasureDataPatch
    {
        [HarmonyPatch(nameof(TreasureData.GetCardLevel))]
        [HarmonyPrefix]
        public static bool GetCardLevel(TreasureData __instance, ref PlantType thePlantType, ref CardLevel __result)
        {
            if (DataMgr.CustomCardLevel.ContainsKey((int)thePlantType))
            {
                __result = DataMgr.CustomCardLevel[(int)thePlantType];
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(HealthSliderManager), nameof(HealthSliderManager.Awake))]
    public static class HealthSliderManagerAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(HealthSliderManager __instance)
        {
            Transform layout = __instance.transform
                .Find("Scroll View/Viewport/Content/Layout");

            if (layout == null)
                return;

            foreach (ZombieType i in DataMgr.bossHealthSliders.Keys)
            {
                GameObject barObj = Object.Instantiate(DataMgr.bossHealthSliders[i], layout, false);

                var controller = barObj.AddComponent<BoardHealthSlider>();
                controller.zombieType = i;

                controller.slider = barObj.GetComponent<Slider>();
                if (controller.slider == null) controller.slider = barObj.GetComponentInChildren<Slider>();
                controller.backImage = barObj.transform
                    .Find("Fill Area/Back")
                    .GetComponent<Image>();

                controller.backImage.type = Image.Type.Filled;
                controller.backImage.fillMethod = Image.FillMethod.Horizontal;
                controller.backImage.fillOrigin = 0;

                __instance.sliders.Add(controller);
            }
        }
    }
    [HarmonyPatch(typeof(TravelHelper))]
    public class TravelHelperGetAllUltimatePlantTypesPatch
    {
        [HarmonyPatch(nameof(TravelHelper.GetAllUltimatePlantTypes))]
        [HarmonyPostfix]
        public static void Postfix(
            bool isStrongUltimate,
            bool withSub,
            Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            // -----------------------------------------
            // STRONG ULTIMATES
            // -----------------------------------------
            if (isStrongUltimate)
            {
                // DataMgr strong ultimates
                foreach (var kv in DataMgr.CustomStrongUltiPlants)
                {
                    PlantType plant = (PlantType)kv.Key;
                    __result.Add(plant);

                    // Add variant (subType)
                    if (withSub &&
                        DataMgr.CustomStrongUltimateInfo.TryGetValue(plant, out var info) &&
                        info.Item1.HasValue)
                    {
                        __result.Add(info.Item1.Value);
                    }
                }

                // CustomCore strong ultimates
                foreach (var plant in CustomCore.CustomStrongUltimatePlants.Keys)
                {
                    __result.Add(plant);

                    // Add variant (subType) if present
                    if (withSub &&
                        DataMgr.CustomStrongUltimateInfo.TryGetValue(plant, out var info) &&
                        info.Item1.HasValue)
                    {
                        __result.Add(info.Item1.Value);
                    }
                }
            }

            // -----------------------------------------
            // WEAK ULTIMATES
            // -----------------------------------------
            else
            {
                foreach (var kv in DataMgr.CustomWeakUltiPlants)
                {
                    PlantType plant = (PlantType)kv.Key;
                    bool isVariant = kv.Value;

                    __result.Add(plant);

                    // Weak ultimate variant (boolean only)
                    if (withSub && isVariant)
                        __result.Add(plant);
                }
            }

            // -----------------------------------------
            // DEDUPLICATE
            // -----------------------------------------
            var unique = new HashSet<PlantType>(__result.ToSystemList());
            __result.Clear();
            foreach (var p in unique)
                __result.Add(p);
        }
    }
    [HarmonyPatch(typeof(TravelLookMenu))]
    public static class TravelLookMenuPatch
    {
        [HarmonyPatch(nameof(TravelLookMenu.GetUnlocks))]
        [HarmonyPostfix]
        public static void PostGetUnlocks(TravelLookMenu __instance, ref Il2CppSystem.Collections.Generic.List<TravelUnlocks> __result)
        {
            if (DataMgr.CustomTravelUnlocks.Count <= 0)
                return;
            foreach (var id in DataMgr.CustomTravelUnlocks)
                if (__instance.showAll)
                    __result.Add(id);
        }
    }
    [HarmonyPatch(typeof(AlmanacPlantMenu))]
    public class AlmanacPlantAwakeMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacPlantMenu.Awake))]
        [HarmonyPostfix]
        public static void Awake_Postfix(AlmanacPlantMenu __instance)
        {
            Transform obj = __instance.transform.Find("Scroll View/Viewport/Content").transform;
            if (PlantMgr.IsNotNull((obj != null) ? obj.Find("LookUlti_1").GetComponent<UIButton>() : null, out var button))
            {
                var a = () =>
                {
                    __instance.ShowPlants(TravelHelper.GetAllUltimatePlantTypes(false, true));
                };
                if (button?.clickEvent != null)
                    button.clickEvent.AddListener(a);
            }
            var btn = obj?.Find("LookUlti_2")?.GetComponent<UIButton>();
            if (btn == null) return;
            var action = () =>
            {
                var list = TravelHelper.GetAllUltimatePlantTypes(true, true);
                __instance.ShowPlants(list);
            };
            btn.clickEvent.AddListener(action);
        }
    }
    [HarmonyPatch(typeof(GameAPP))]
    public class GameAPP_Patch
    {
        [HarmonyPatch(nameof(GameAPP.Awake))]
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void Awake_Postfix(GameAPP __instance)
        {
            ModLoader.OnGameStart();
            foreach (Action a in DataMgr.GameStartActions)
            {
                try
                {
                    a.Invoke();
                }
                catch (Exception e)
                {
                    ModLogger.LogError("Startup exception!\n" + e.ToString());
                }
            }
            DataMgr.IsGameStarted = true;
            GameAPPInitBehaviour.RegisterAllPlants();
            //__instance.AddComponent<GameAppInitBehaviour>();
        }
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void Start_Postfix(GameAPP __instance)
        {
            foreach (KeyValuePair<ZombieType, (int, int)> i in DataMgr.CustomZombieSpawns)
            {
                if (ZombieDataManager.zombieDataDic.ContainsKey(i.Key))
                {
                    ZombieDataManager.zombieDataDic[i.Key].summonLevel = i.Value.Item1;
                    ZombieDataManager.zombieDataDic[i.Key].summonWeight = i.Value.Item2;
                }
            }
            //no null check because if it is null, the game does not exist
            Plugin.Logger.LogInfo("================================");
            Plugin.Logger.LogInfo($"     Registered {DataMgr.CustomPlantCount} custom plants, {DataMgr.CustomSkinCount} custom plant skins, {DataMgr.CustomBigStarCount} custom big stars, and {DataMgr.CustomBulletCount} custom bullets.");
            Plugin.Logger.LogInfo($"     In total, there are now {GameAPP.resourcesManager.allPlants.Count} plants");

            foreach (var msg in DataMgr.StartUpMessages)
                Plugin.Logger.LogInfo("     " + msg);

            foreach (var wrn in DataMgr.StartUpWarnings)
                Plugin.Logger.LogWarning("     " + wrn);

            foreach (var err in DataMgr.StartUpErrors)
                Plugin.Logger.LogError("     " + err);

            Plugin.Logger.LogInfo("================================");
        }
        [HarmonyPatch(nameof(GameAPP.Start))]
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void Start_Postfix_2(GameAPP __instance)
        {
            foreach (Action a in DataMgr.GameAppInitActions)
            {
                try
                {
                    a.Invoke();
                }
                catch (Exception e)
                {
                    ModLogger.LogError("GameAPP init exception!\n" + e.ToString());
                }
            }
        }
    }
    [HarmonyPatch(typeof(Plant))]
    public static class Plant_Patches
    {
        // -------------------------
        //  Die()
        // -------------------------
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Plant.Die))]
        public static bool Die_Prefix(Plant __instance, DieReason reason)
        {
            if (__instance == null || !Utils.InGame()) return true;
            if (__instance.TryGetComponent<BaseCustomPlant>(out var p))
            {
                if (p.isPF &&
                (reason == DieReason.CrashInWater ||
                reason == DieReason.Default ||
                reason == DieReason.ByFreeze))
                {
                    __instance.thePlantHealth = __instance.thePlantMaxHealth;
                    __instance.UpdateText();
                    return false;
                }
                else if (p.isPF && reason == DieReason.BySteal)
                {
                    Plant newPlant = CreatePlant.Instance.SetPlant(
                        __instance.thePlantColumn,
                        __instance.thePlantRow,
                        __instance.thePlantType,
                        null,
                        default,
                        true,
                        false,
                        null
                    );
                    if (newPlant.TryGetComponent<BaseCustomPlant>(out var baseCustomPlant))
                    {
                        IEnumerator enumerator(BaseCustomPlant baseCustomPlant)
                        {
                            yield return new WaitForFixedUpdate();
                            baseCustomPlant.StartPF();
                        }
                        GameAPP.Instance.StartCoroutine(enumerator(baseCustomPlant));
                    }
                }
                else if (!p.CanBeFrozen && reason == DieReason.ByFreeze)
                {
                    return false;
                }
                else if (!p.CanDie)
                {
                    return false;
                }
                else
                {
                    p.OnDie(reason);
                }
            }
            return true;
        }

        // -------------------------
        //  Crashed()
        // -------------------------
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Plant.Crashed))]
        public static bool Crashed_Prefix(Plant __instance)
        {
            if (__instance.TryGetComponent<BaseCustomPlant>(out var p))
            {
                if (p.isPF)
                {
                    __instance.thePlantHealth = __instance.thePlantMaxHealth;
                    return false;
                }
                else if (!p.CanBeCrashed)
                {
                    return false;
                }
                else
                {
                    p.OnDie(DieReason.Crash);
                }
            }
            return true;
        }

        // -------------------------
        //  Crashed()
        // -------------------------
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Plant.TakeDamage))]
        public static bool TakeDamage_Prefix(Plant __instance, ref int damage, IDamageMaker damageFrom, DamageType damageType = DamageType.Normal)
        {
            if (__instance.TryGetComponent<BaseCustomPlant>(out var p))
            {
                if (p.isPF)
                {
                    __instance.thePlantHealth = __instance.thePlantMaxHealth;
                    return false;
                }
                else if (p is ICherryImmune)
                {
                    if (damageType == DamageType.CherryExplode) return false;
                }
                else
                {
                    damage = p.OnTakeDamage(damage, damageFrom, damageType);
                }
            }
            else if (__instance.TryGetComponent<PlantSkinComponent>(out var comp))
            {
                if (p is ICherryImmune)
                {
                    if (damageType == DamageType.CherryExplode) return false;
                }
            }
            return true;
        }

        // -------------------------
        //  StarUp()
        // -------------------------
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Plant.StarUp))]
        public static bool StarUp_Prefix(Plant __instance)
        {
            if (DataMgr.CustomStarUps.Contains((int)__instance.thePlantType))
            {
                __instance.starUp = true;
                __instance.UpdateStarIcon();
                return false;
            }
            return true;
        }

        // -------------------------
        //  InitText()
        // -------------------------
        [HarmonyPatch(nameof(Plant.InitText))]
        [HarmonyPostfix]
        public static void InitText_Postfix(Plant __instance)
        {
            if (__instance.TryGetComponent<BaseCustomPlant>(out var BaseCustomPlant))
            {
                BaseCustomPlant.InitText();
            }
        }
    }
    [HarmonyPatch(typeof(FreezedPlant))]
    public static class FreezedPlant_Patch
    {
        [HarmonyPatch(nameof(FreezedPlant.InitFreezedPlant))]
        [HarmonyPrefix]
        public static bool InitFreezedPlant_Prefix(FreezedPlant __instance, PlantType thePlantType)
        {
            if (__instance == null) return true;
            if (PlantMgr.IsTypeIn1x1(__instance.theItemColumn, __instance.theItemRow, (Plant p) =>
            {
                if (p.TryGetComponent<BaseCustomPlant>(out var c) && (c.isPF && c._plant.thePlantType == thePlantType || !c.CanBeFrozen && c._plant.thePlantType == thePlantType))
                {
                    return true;
                }
                return false;
            }))
            {
                __instance.Die();
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(UltimateSwordZombie.__c))]
    public static class UltimateSwordZombie___c_Patch
    {
        [HarmonyPatch(nameof(UltimateSwordZombie.__c._UseShovel_b__11_0))]
        [HarmonyPrefix]
        public static bool AttackEffect_Prefix(Plant p, ref bool __result)
        {
            if (p == null) return true;
            if (p.TryGetComponent<BaseCustomPlant>(out var component))
            {
                if (component.isPF)
                {
                    p.thePlantHealth = p.thePlantMaxHealth;
                    p.UpdateText();
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Zombie))]
    public static class Zombie_Patch
    {
        [HarmonyPatch(nameof(Zombie.AttackEffect))]
        [HarmonyPrefix]
        public static bool AttackEffect_Prefix(Zombie __instance, Plant plant)
        {
            if (__instance == null || plant == null) return true;
            if (__instance.TryGetComponent<BaseCustomZombie>(out var z))
            {
                return z.AttackEffect(plant);
            }
            return true;
        }
        [HarmonyPatch(nameof(Zombie.Die))]
        [HarmonyPrefix]
        public static void Die_Prefix(Zombie __instance, ref int reason)
        {
            if (__instance == null || !Utils.InGame()) return;
            if (__instance.theStatus == ZombieStatus.Dying)
            {
                if (reason == __instance.dieReason)
                {
                    return;
                }
            }
            if (__instance.TryGetComponent<CustomLevelComponent>(out var customLevelComponent))
            {
                customLevelComponent.OnZombieDie(__instance, reason);
            }
        }
        [HarmonyPatch(nameof(Zombie.Awake))]
        [HarmonyPostfix]
        public static void Awake_Postfix(Zombie __instance)
        {
            if (__instance == null || !Utils.InGame())
                return;

            if (!DataMgr.zombieDatas.TryGetValue(__instance.theZombieType, out var data))
                return;

            // Attack damage
            __instance.theAttackDamage = data.theAtackDamage;

            // FIRST ARMOR
            if (!string.IsNullOrEmpty(data.theFirstArmorPath))
            {
                var firstObj = __instance.transform.FindChild(data.theFirstArmorPath);
                if (firstObj != null)
                {
                    __instance.theFirstArmor = firstObj.gameObject;
                    __instance.theFirstArmorType = data.theFirstArmorType;
                    __instance.theFirstArmorMaxHealth = data.theFirstArmorHealth;
                }
            }

            // SECOND ARMOR
            if (!string.IsNullOrEmpty(data.theSecondArmorPath))
            {
                var secondObj = __instance.transform.FindChild(data.theSecondArmorPath);
                if (secondObj != null)
                {
                    __instance.theSecondArmor = secondObj.gameObject;
                    __instance.theSecondArmorType = data.theSecondArmorType;
                    __instance.theSecondArmorMaxHealth = data.theSecondArmorHealth;
                }
            }

            __instance.UpdateHealthText();
        }
    }

    [HarmonyPatch(typeof(Shooter))]
    public class Shooter_Patch
    {
        [HarmonyPatch(nameof(Shooter.AnimShoot))]
        [HarmonyPrefix]
        public static bool AnimShoot_Prefix(Shooter __instance, ref Bullet __result)
        {
            if (__instance.TryGetComponent<BaseCustomPlant>(out var BaseCustomPlant))
            {
                __result = BaseCustomPlant.AnimShoot_Custom();
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(Shooter.AnimShoot2))]
        [HarmonyPrefix]
        public static bool AnimShoot2_Prefix(Shooter __instance)
        {
            if (__instance.TryGetComponent<BaseCustomPlant>(out var baseCustomPlant))
            {
                baseCustomPlant.AnimShoot2_Custom();
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Bullet_pea))]
    public static class Bullet_pea_Patch
    {
        [HarmonyPatch(nameof(Bullet_pea.HitZombie))]
        [HarmonyPrefix]
        public static bool HitZombie_Prefix(Bullet_pea __instance, Zombie zombie)
        {
            if (__instance.TryGetComponent<BaseCustomBullet>(out var a))
            {
                return a.HitZombie(zombie);
            }
            return true;
        }
        [HarmonyPatch(nameof(Bullet_pea.HitPlant))]
        [HarmonyPrefix]
        public static bool HitPlant_Prefix(Bullet_pea __instance, Plant plant)
        {
            if (__instance.TryGetComponent<BaseCustomBullet>(out var a))
            {
                return a.HitPlant(plant);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Bullet))]
    public static class Bullet_Patch
    {
        [HarmonyPatch(nameof(Bullet.CheckZombie))]
        [HarmonyPrefix]
        public static bool CheckZombie_Prefix(Bullet __instance, Zombie zombie)
        {
            if (__instance.TryGetComponent<BaseCustomBullet>(out var a))
            {
                return a.HitZombieCondition(zombie);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Bullet_cabbage))]
    public static class Bullet_cabbage_Patch
    {
        [HarmonyPatch(nameof(Bullet_cabbage.HitZombie))]
        [HarmonyPrefix]
        public static bool HitZombie_Prefix(Bullet_cabbage __instance, Zombie zombie)
        {
            if (__instance.TryGetComponent<BaseCustomBullet>(out var a))
            {
                return a.HitZombie(zombie);
            }
            return true;
        }
        [HarmonyPatch(nameof(Bullet_cabbage.HitLand))]
        [HarmonyPrefix]
        public static bool HitPlant_Prefix(Bullet_cabbage __instance)
        {
            if (__instance.TryGetComponent<BaseCustomBullet>(out var a))
            {
                return a.HitLand();
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlantDataMenu))]
    public static class PlantDataMenuPatch
    {
        [HarmonyPatch(nameof(PlantDataMenu.Start))]
        [HarmonyPostfix]
        public static void Start_Postfix(PlantDataMenu __instance)
        {
            if (__instance == null || __instance.IsDestroyed()) return;
            if (__instance.plant == null || __instance.plant.IsDestroyed()) return;

            var basePlant = __instance.plant.GetComponent<BaseCustomPlant>();
            if (basePlant == null) return;

            var info = basePlant.GetLiveInfo();
            if (info.Count == 0) return;

            var sb = new StringBuilder();
            foreach (var pair in info)
                sb.AppendLine($"{pair.Key}：{pair.Value}");

            foreach (var text in __instance.infoText)
                text.text += sb.ToString();
        }
    }
    [HarmonyPatch(typeof(UltimateFootballZombie))]
    public static class UltimateFootballZombie_Patch
    {
        [HarmonyPatch(nameof(UltimateFootballZombie.AttackEffect))]
        [HarmonyPrefix]
        public static bool AttackEffect_Prefix(Plant plant)
        {
            if (plant.TryGetComponent<BaseCustomPlant>(out var p) && p.isPF)
            {
                plant.thePlantHealth = plant.thePlantMaxHealth;
                plant.UpdateText();
                return false;
            }
            return true;
        }
    }
    // =========================
    // UIMgr
    // =========================

    [HarmonyPatch(typeof(UIMgr))]
    public static class UIMgr_Patch
    {
        [HarmonyPatch(nameof(UIMgr.GetSceneType))]
        [HarmonyPrefix]
        public static bool GetSceneType_Prefix(
            LevelType theLevelType,
            int theLevelNumber,
            ref SceneType __result)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == theLevelType && x.LevelID == theLevelNumber);

            if (!isCustom)
                return true;

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == theLevelType && x.LevelID == theLevelNumber);

            __result = entity.SceneType;
            return false;
        }

        [HarmonyPatch(nameof(UIMgr.EnterGame))]
        [HarmonyPostfix]
        public static void EnterGame_Postfix(
            LevelType levelType,
            int levelNumber)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == levelType && x.LevelID == levelNumber);

            if (!isCustom)
            {
                GlobalTracker.IsCustomLevel = false;
                GlobalTracker.CustomLevelID = -1;
                return;
            }

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == levelType && x.LevelID == levelNumber);

            GlobalTracker.IsCustomLevel = true;
            GlobalTracker.CustomLevelID = entity.LevelID;

            var map = entity.MapRoadTypes;
            if (map == null)
                return;

            int rows = map.GetLength(0);
            int cols = map.GetLength(1);

            var board = InstanceManager.Board;
            board.rowNum = rows;

            var gs = board.gridSystem;

            for (int row = 0; row < rows; row++)
            {
                var grid = gs.GetGrid(0, row);
                board.roadType[row] = grid.boxType;
            }

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    var grid = gs.GetGrid(col, row);
                    grid.boxType = (BoxType)map[row, col];
                }
            }

            if (DataMgr.CustomLevelComponents.TryGetValue(entity.LevelID, out var type))
                board.gameObject.AddComponent(type.ToIl2CppType());

            if (entity.SceneBackground != null)
            {
                board.background.transform
                    .FindChild("bg")
                    .FindChild("bg")
                    .GetComponent<SpriteRenderer>()
                    .sprite = entity.SceneBackground;
            }

            board.boardTag = entity.BoardTag;
            board.theSun = entity.SunCounter;

            if (TravelMgr.Instance != null)
            {
                TravelMgr.Instance.data.advBuffs = (entity.AdvBuffs ?? new()).ToIl2CppList();
                TravelMgr.Instance.data.ultiBuffs = (entity.UltiBuffs ?? new()).ToIl2CppList();
                TravelMgr.Instance.data.travelDebuffs = (entity.TravelDebuffs ?? new()).ToIl2CppList();
                TravelMgr.Instance.data.unlockedPlants = (entity.TravelUnlocks ?? new()).ToIl2CppList();
            }

            entity.EnterAction?.Invoke();
        }

        [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
        [HarmonyPostfix]
        public static void EnterChallengeMenu_Postfix()
        {
            if (DataMgr.LoadedCustomLevels.Count == 0)
                return;

            Debug.Log("Initializing custom level button.");
            GameAPP.Instance.StartCoroutine(init());

            static IEnumerator init()
            {
                yield return null;
                yield return null;

                var levels = GameAPP.canvas.GetChild(0).FindChild("Levels");
                var firstBtns = levels.FindChild("FirstBtns");

                if (firstBtns.FindChild("LoadedCustomLevels") != null)
                    yield break;

                GameObject custom = Object.Instantiate(firstBtns.GetChild(0).gameObject, firstBtns);
                custom.name = "LoadedCustomLevels";
                custom.transform.localPosition =
                    MathHelper.GetLevelButtonPosition((firstBtns.childCount - 1) % 6,
                                                    (firstBtns.childCount - 1) / 6);

                var window = custom.transform.FindChild("Window");
                window.FindChild("Name").GetComponent<TextMeshProUGUI>().text = "更多二创关卡";

                var adv = levels.FindChild("PageAdvantureLevel");
                var customLevels = Object.Instantiate(adv.gameObject, levels);
                customLevels.SetActive(false);
                customLevels.name = "PageMoreLevels";

                var pages = customLevels.transform.FindChild("Pages");
                var levelSample = Object.Instantiate(
                    pages.FindChild("Page1").FindChild("Lv1").gameObject);

                foreach (var t in pages.FindChild("Page1").GetComponentsInChildren<Transform>(true))
                    if (t != pages.FindChild("Page1"))
                        Object.Destroy(t.gameObject);

                var pageSample = Object.Instantiate(pages.FindChild("Page1").gameObject);
                var rt = pageSample.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.localPosition = Vector3.zero;
                rt.localScale = Vector3.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Object.Destroy(pages.FindChild("Page1").gameObject);
                Object.Destroy(pages.FindChild("Page2").gameObject);
                Object.Destroy(pages.FindChild("Page3").gameObject);

                int levelIndex = 0;

                yield return null;

                foreach (var level in DataMgr.LoadedCustomLevels)
                {
                    if (levelIndex % 18 == 0)
                    {
                        var newPage = Object.Instantiate(pageSample, pages);
                        newPage.name = $"Pages{(levelIndex / 18) + 1}";
                        var rt2 = newPage.GetComponent<RectTransform>();
                        rt2.anchorMin = new Vector2(0.5f, 0.5f);
                        rt2.anchorMax = new Vector2(0.5f, 0.5f);
                        rt2.pivot = new Vector2(0.5f, 0.5f);
                        rt2.anchoredPosition = Vector2.zero;
                        rt2.localPosition = Vector3.zero;
                        rt2.localScale = Vector3.one;
                        rt2.offsetMin = Vector2.zero;
                        rt2.offsetMax = Vector2.zero;

                    }

                    int col = levelIndex % 6;
                    int row = (levelIndex / 6) % 3;
                    int page = levelIndex / 18;

                    var pageObj = pages.FindChild($"Pages{page + 1}");

                    var item = Object.Instantiate(levelSample, pageObj);
                    item.name = $"Lv{level.LevelID}";
                    item.transform.localPosition =
                        MathHelper.GetLevelButtonPosition(col, row);

                    if (level.LevelSprite != null)
                        item.GetComponent<Image>().sprite = level.LevelSprite;

                    var win = item.transform.Find("Window");
                    win.Find("Name").GetComponent<TextMeshProUGUI>().text = level.LevelName;

                    var victory = item.transform.Find("Window/Trophy").gameObject;
                    victory.SetActive(LevelProgressionManager.IsCompleted(level.LevelID));

                    var btn = win.GetComponent<Advanture_Btn>();
                    btn.levelType = level.LevelType;
                    btn.buttonNumber = level.LevelID;

                    levelIndex++;
                }

                window.GetComponent<FirstBtns>().pageToOpen = customLevels;
                window.GetComponent<FirstBtns>().originPosition = custom.transform.localPosition;

                Object.Destroy(pageSample);
                Object.Destroy(levelSample);
            }
        }
    }
    // =========================
    // CustomMenu
    // =========================

    /*[HarmonyPatch(typeof(CustomMenu))]
    public static class CustomMenuPatch
    {
        [HarmonyPatch(nameof(CustomMenu.Awake))]
        [HarmonyPostfix]
        public static void PostAwake()
        {
            var child = GameAPP.canvas.GetChild(0)
                .FindChild("Levels")
                .FindChild("FirstBtns")
                .FindChild("LoadedCustomLevels");

            if (child != null)
                child.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        }

        [HarmonyPatch(nameof(CustomMenu.OnDestroy))]
        [HarmonyPostfix]
        public static void PostOnDestroy()
        {
            var child = GameAPP.canvas.GetChild(0)
                .FindChild("Levels")
                .FindChild("FirstBtns")
                .FindChild("LoadedCustomLevels");

            if (child != null)
                child.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
        }
    }*/

    // =========================
    // PrizeMgr
    // =========================

    [HarmonyPatch(typeof(PrizeMgr))]
    public static class Patch_PrizeMgr_EnterNextMenu
    {
        [HarmonyPatch(nameof(PrizeMgr.EnterNextMenu))]
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (!GlobalTracker.IsCustomLevel)
                return;

            LevelProgressionManager.MarkCompleted(GlobalTracker.CustomLevelID);

            GlobalTracker.IsCustomLevel = false;
            GlobalTracker.CustomLevelID = -1;
        }
    }

    // =========================
    // WaveManager
    // =========================

    [HarmonyPatch(typeof(WaveManager))]
    public static class WaveManager_Patch
    {
        [HarmonyPatch(nameof(WaveManager.GetMaxWave))]
        [HarmonyPrefix]
        public static bool Prefix(LevelType levelType, int level, ref int __result)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == levelType && x.LevelID == level);

            if (!isCustom)
                return true;

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == levelType && x.LevelID == level);

            __result = entity.MaxWave;
            return false;
        }
    }
    public class PatchData
    {
        public static bool TryGetCustomLevel(LevelType theLevelType, int theLevelNumber, out BaseCustomLevelData data)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == theLevelType && x.LevelID == theLevelNumber);

            if (!isCustom)
            {
                data = default;
                return false;
            }

            data = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == theLevelType && x.LevelID == theLevelNumber);

            return true;
        }
    }

    // =========================
    // InitZombieList
    // =========================

    /*[HarmonyPatch(typeof(InitZombieList))]
    public static class InitZombieList_Patch
    {
        [HarmonyPatch(nameof(InitZombieList.SetAllowZombieTypeSpawn))]
        [HarmonyPrefix]
        public static bool Prefix(LevelType theLevelType, int theLevelNumber)
        {
            /*bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == theLevelType && x.LevelID == theLevelNumber);

            if (!isCustom)
                return true;

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == theLevelType && x.LevelID == theLevelNumber);
            
            if(!PatchData.TryGetCustomLevel(theLevelType, theLevelNumber, out var entity)) return true;

            if (entity.ZombieTypes == null || entity.ZombieTypes.Count == 0)
                return true;
            InitZombieList.AllowZombies(entity.ZombieTypes.ToIl2CppList());

            return false;
        }
    }//*/
    [HarmonyPatch(typeof(InitZombieList))]
    public static class InitZombieList_Patch
    {
        [HarmonyPatch(nameof(InitZombieList.PickZombie))]
        [HarmonyPrefix]
        public static void PickZombie_Prefix()
        {
            if (PatchData.TryGetCustomLevel(GameAPP.theBoardType, GameAPP.theBoardLevel, out var levelData))
            {
                foreach (var z in levelData.ZombieTypes)
                    InitZombieList.zombieToSpawns.Add(z);
            }
            var key = (GameAPP.theBoardType, GameAPP.theBoardLevel);
            if (DataMgr.AddedZombiesInLevel.TryGetValue(key, out var zombieTypes))
            {
                foreach (var z in zombieTypes)
                    InitZombieList.zombieToSpawns.Add(z);
            }
        }
    }
    [HarmonyPatch(typeof(InitBoard))]
    public static class InitBoardPatch
    {
        [HarmonyPatch(nameof(InitBoard.PreSelectCard))]
        [HarmonyPostfix]
        public static void PreSelectCard_Postfix(InitBoard __instance)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == GameAPP.theBoardType &&
                        x.LevelID == GameAPP.theBoardLevel);

            if (!isCustom)
                return;

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == GameAPP.theBoardType &&
                            x.LevelID == GameAPP.theBoardLevel);
            if (entity.selection != CustomLevelSelection.PreSelected) return;
            foreach (var c in entity.SelectTypes)
            {
                __instance.PreSelect(c);
            }
        }
        /*
        [HarmonyPatch(nameof(InitBoard.MoveOverEvent))]
        [HarmonyPrefix]
        public static bool MoveOverEvent_Prefix(InitBoard __instance, ref string direction)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == GameAPP.theBoardType &&
                        x.LevelID == GameAPP.theBoardLevel);

            if (!isCustom)
                return true;

            var levelData = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == GameAPP.theBoardType &&
                            x.LevelID == GameAPP.theBoardLevel);
            if (direction == "right")
            {
                if (__instance.board != null)
                {
                    if (!__instance.board.boardTag.disableSelectCard)
                    {
                        // 设置游戏状态
                        GameAPP.theGameStatus = GameStatus.Selecting;

                        // UI控制
                        InGameUI.Instance.ConveyorBelt.SetActive(false);
                        InGameUI.Instance.Bottom.SetActive(true);

                        // 启动协程移动UI元素
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.SeedBank, 79f, 0));
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.Bottom, 525f, 1));
                    }
                    else
                    {
                        // 延迟执行方法
                        __instance.Invoke("LeftMoveCamera", 1.5f);
                        InGameUI.Instance.Bottom.SetActive(false);
                    }
                }
            }
            else if (direction == "left")
            {
                if (__instance.board == null) return false;

                if (__instance.board.boardTag.disableSelectCard)
                {
                    if (__instance.board.cardBank)
                    {
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.SeedBank, 79f, 0));
                        __instance.AddCard();
                    }
                    else
                    {
                        InGameUI.Instance.SeedBank.SetActive(false);
                    }
                    InGameUI.Instance.Bottom.SetActive(false);
                }

                // 音量渐变协程
                __instance.StartCoroutine(__instance.DecreaseVolume());

                // 降低UI位置
                InGameUI.Instance.LowerUI();

                // 初始化割草机（特定模式下）
                if (!__instance.board.boardTag.disableMower)
                {
                    __instance.InitMower();
                }

                // 雾效果移动
                if (__instance.board.fog != null)
                {
                    Vector3 fogPosition = __instance.board.fog.transform.position;
                    Vector3 boardPosition = __instance.board.background.transform.position;

                    FogMgr.Instance.MoveObject(
                        new(fogPosition.x,
                        fogPosition.y,
                        boardPosition.z),
                        10f  // 移动速度
                    );
                }
                float invokeDelay = 0.5f;
                __instance.Invoke("ReadySetPlant", invokeDelay);
            }
            return false;
        }
        */
    }

    // =========================
    // Lawnf (Music)
    // =========================

    [HarmonyPatch(typeof(Lawnf))]
    public static class Lawnf_Patch
    {
        [HarmonyPatch(nameof(Lawnf.SetMusic))]
        [HarmonyPostfix]
        public static void SetMusic_Postfix(ref Board board)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == GameAPP.theBoardType &&
                        x.LevelID == GameAPP.theBoardLevel);

            if (!isCustom)
                return;

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == GameAPP.theBoardType &&
                            x.LevelID == GameAPP.theBoardLevel);

            if (entity.MusicType != (MusicType)(-1))
                GameAPP.Instance.PlayMusic(entity.MusicType);
            entity.EnterGameAction(board);
        }
        [HarmonyPatch(nameof(Lawnf.TravelDebuff))]
        [HarmonyPostfix]
        public static void TravelDebuff_Postfix(TravelDebuff buff, ref bool __result)
        {
            if (buff == TravelDebuff.EnumValue1018 || buff == TravelDebuff.EnumValue1019)
            {
                __result = false;
            }
        }
    }

    // =========================
    // InGameUI (Level Name)
    // =========================

    [HarmonyPatch(typeof(InGameUI))]
    public static class InGameUI_Patch
    {
        [HarmonyPatch(nameof(InGameUI.SetUniqueText))]
        [HarmonyPostfix]
        public static void Postfix(InGameUI __instance)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == GameAPP.theBoardType &&
                        x.LevelID == GameAPP.theBoardLevel);

            if (!isCustom)
                return;

            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == GameAPP.theBoardType &&
                            x.LevelID == GameAPP.theBoardLevel);

            __instance.SetLevelName(entity.LevelName);
        }
    }
    [HarmonyPatch(typeof(ConveyManager))]
    public static class ConveyManagerPatch
    {
        [HarmonyPatch(nameof(ConveyManager.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(ConveyManager __instance)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == GameAPP.theBoardType &&
                        x.LevelID == GameAPP.theBoardLevel);

            if (!isCustom)
                return;

            // Fetch the custom level entity
            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == GameAPP.theBoardType &&
                            x.LevelID == GameAPP.theBoardLevel);
            if (entity.selection != CustomLevelSelection.Convey) return;
            __instance.plants = entity.SelectTypes.ToList().ToIl2CppList();
        }

        [HarmonyPatch(nameof(ConveyManager.GetCardPool))]
        [HarmonyPostfix]
        public static void PostGetCardPool(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            bool isCustom = DataMgr.LoadedCustomLevels
                .Any(x => x.LevelType == GameAPP.theBoardType &&
                        x.LevelID == GameAPP.theBoardLevel);

            if (!isCustom)
                return;

            // Fetch the custom level entity
            var entity = DataMgr.LoadedCustomLevels
                .First(x => x.LevelType == GameAPP.theBoardType &&
                            x.LevelID == GameAPP.theBoardLevel);
            if (entity.selection != CustomLevelSelection.Convey) return;
            __result = entity.SelectTypes.ToList().ToIl2CppList();
        }
    }
    [HarmonyPatch(typeof(Board))]
    public class Board_Patch
    {
        [HarmonyPatch(nameof(Board.OnPlantCreate))]
        [HarmonyPrefix]
        public static void OnPlantCreate_Prefix(Board __instance, Plant plant)
        {
            if (__instance == null) return;
            if (__instance.TryGetComponent<CustomLevelComponent>(out var customLevelComponent))
            {
                customLevelComponent.OnPlantCreate(plant);
            }
        }
        [HarmonyPatch(nameof(Board.OnPlantDie))]
        [HarmonyPrefix]
        public static void OnPlantDie_Prefix(Board __instance, Plant plant, ref DieReason plantDieReason)
        {
            if (__instance == null) return;
            if (__instance.TryGetComponent<CustomLevelComponent>(out var customLevelComponent))
            {
                customLevelComponent.OnPlantDie(plant, plantDieReason);
            }
        }
    }
    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgr_Patch
    {
        [HarmonyPatch(nameof(TravelMgr.GetUnlocksPool))]
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void PostGetUnlocksPool(TravelMgr __instance, ref Il2CppSystem.Collections.Generic.List<TravelUnlocks> __result)
        {
            foreach (var id in DataMgr.CustomTravelUnlocks)
            {
                var unlock = id;
                if (!__result.Contains(unlock))
                    __result.Add(unlock);
            }
        }
        [HarmonyPatch(nameof(TravelMgr.GetUltiBuffPool))]
        [HarmonyPriority(Priority.First)]
        [HarmonyPostfix]
        public static void GetUltiBuffPool_PostFix(TravelMgr __instance, ref Il2CppSystem.Collections.Generic.List<UltiBuff> __result)
        {
            foreach (var id in DataMgr.CustomStrongUltimateInfo)
            {
                if (TravelDictionary.PlantToUnlock.ContainsKey(id.Key) && Lawnf.TravelUnlock(TravelDictionary.PlantToUnlock[id.Key]))
                {
                    if (!__result.Contains(id.Value.Item2))
                        __result.Add(id.Value.Item2);

                    if (!__result.Contains(id.Value.Item3))
                        __result.Add(id.Value.Item3);
                }
            }
        }
    }/*
    
    [HarmonyPatch(typeof(TravelBuffOptionButton))]
    public static class TravelBuffOptionButtonPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetPlant", new Type[] { })]
        public static bool PreSetPlant(TravelBuffOptionButton __instance)
        {
            string text = __instance.buff.ToString();
            Match m = Regex.Match(text, @"EnumValue(\d+)");
            if (!m.Success)
                return true;

            if (!int.TryParse(m.Groups[1].Value, out int enumValue))
                return true;

            if (!DataMgr.UnlockValueToPlantType.TryGetValue(enumValue, out int plantType))
                return true;

            if (!DataMgr.CustomStrongUltiPlants.ContainsKey(plantType))
                return true;

            __instance.SetPlant((ID)plantType);
            return false;
        }
    }//*/
    [HarmonyPatch(typeof(ZombieDataManager), nameof(ZombieDataManager.GetZombieData))]
    public static class ZombieDataManager_GetZombieData_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ZombieType zombieType, ref ZombieDataManager.ZombieData __result)
        {
            if (DataMgr.CustomZombieSpawns.TryGetValue(zombieType, out var val))
            {
                __result.summonLevel = val.Item1;
                __result.summonWeight = val.Item2;
            }
        }
    }
}