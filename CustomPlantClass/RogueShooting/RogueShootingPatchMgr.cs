#nullable enable

namespace CustomPlantClass
{
    /*[HarmonyPatch(typeof(ShootingManager))]
    public static class ShootingManager_Patch
    {
        [HarmonyPatch(nameof(ShootingManager.Start))]
        [HarmonyPostfix]
        public static void PostStart(ShootingManager __instance)
        {
            if (__instance != null)
            {
                CustomRogueShootingManager.Instance=__instance.GetOrAddComponent<CustomRogueShootingManager>();
            }
        }
        [HarmonyPatch(nameof(ShootingManager.RegisterCoreBuff))]
        [HarmonyPostfix]
        public static void RegisterCoreBuff_Postfix(ShootingManager __instance, MultipleChoiceMenu menu)
        {
            if (menu == null)
                return;

            foreach (var plantType in Lawnf.GetAllPlantTypes())
            {
                foreach (var custom in CustomRogueShootingManager.GetCustomBuffsForPlant(plantType))
                {
                    if (!custom.CanAppear)
                        continue;

                    int chosenCount = __instance.GetBuffChoiceCount(plantType, custom.Title);
                    if (chosenCount >= custom.MaxCount)
                        continue;

                    float weight = custom.AppearWeight;
                    if (weight < 1f)
                    {
                        float roll = UnityEngine.Random.value;
                        float luckyFactor = ((__instance._lucky * 0.3f) + 1f) * weight;
                        if (luckyFactor < roll)
                            continue;
                    }

                    string displayTitle = custom.Title;
                    if (chosenCount > 0)
                        displayTitle += $"\n已选了{chosenCount}次";
                    var a = () => custom.OnGet();
                    UnityAction onClick = a;

                    menu.RegisterOption(
                        displayTitle,
                        custom.Description,
                        onClick,
                        custom.ShowType,
                        ZombieType.Nothing,
                        custom.Rarity,
                        true
                    );
                }
            }
        }

        [HarmonyPatch(nameof(ShootingManager.UpgradeBuff))]
        [HarmonyPrefix]
        public static bool UpgradeBuff_Prefix(
            ShootingManager __instance,
            MultipleChoiceMenu menu,
            TheButton button)
        {
            // 1. no chances left
            if (__instance.upgradeBuffChance < 1)
            {
                InGameText.Instance?.ShowText("升级机会不足", 5f, false);
                return false;
            }

            var all = new List<IUpgradeBuff>();

            // 2. vanilla upgrades
            var vanillaDict = __instance.GetCanUpgrades(menu);
            if (vanillaDict != null)
            {
                foreach (var kv in vanillaDict)
                    all.Add(new VanillaUpgradeWrapper(kv.Value));
            }

            // 3. custom upgrades
            foreach (var buff in CustomRogueShootingManager.GetCustomUpgrades())
                all.Add(new CustomUpgradeWrapper(buff));

            // 4. nothing to upgrade
            if (all.Count == 0)
            {
                InGameText.Instance?.ShowText("没有可以升级的词条", 5f, false);
                return false;
            }

            // 5. pick one
            var chosen = all[Random.Range(0, all.Count)];
            chosen.Apply();

            // 6. show message (matches decoded literals)
            ShowUpgradeMessage(chosen);

            // 7. update button + chances
            UpdateUpgradeButton(__instance, button);

            return false; // skip original
        }

        private static void ShowUpgradeMessage(IUpgradeBuff buff)
        {
            string plantName = Lawnf.GetName(buff.Plant);
            string qualityName = Helper.qualityNames[buff.Rarity];

            string msg =
                "诸神注视着你，并选中了一个他们喜欢的词条\n" +
                $"植物<color=green>【{plantName}】</color>的词条<color=yellow>【{buff.Title}】</color>获得品质升级\n" +
                $"当前品质：{qualityName}";

            InGameText.Instance?.ShowText(msg, 10f, false);
        }

        private static void UpdateUpgradeButton(ShootingManager mgr, TheButton button)
        {
            mgr.upgradeBuffChance--;

            if (button == null)
                return;

            var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = $"升级品质({mgr.upgradeBuffChance})";

                if (mgr.upgradeBuffChance < 1)
                {
                    button.Interactable = false;
                    CursorChange.SetDefaultCursor();
                }
            }
        }
    }
    /*
    [HarmonyPatch(typeof(ShootingManager), nameof(ShootingManager.RegisterOtherBuff))]
    public static class ShootingManager_RegisterOtherBuff_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ShootingManager __instance, MultipleChoiceMenu menu)
        {
            if (menu == null)
                return false;

            float lucky = __instance._lucky;
            float LuckyFactor(float baseChance) => (lucky * 0.3f + 1f) * baseChance;

            // --------------------------------------------------------------------
            // 1) 品质升级 (StringLiteral_12004 / 6038)
            // 获得{0}次升级词条品质的机会，只能用于速度和力量的通用词条，且随机升级
            // plantType: 0xfe → EndoFlame
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.15f))
            {
                Quality q = __instance.GetRandomQuality();

                string text = string.Format(
                    "获得{0}次升级词条品质的机会，只能用于速度和力量的通用词条，且随机升级",
                    (int)q
                );

                UnityAction onClick = () =>
                {
                    // ShootingManager.<>b__55_0 (async builder)
                    // TODO: re‑implement the actual async upgrade logic if needed.
                };

                menu.RegisterOption(
                    "品质升级",
                    text,
                    onClick,
                    PlantType.EndoFlame,
                    ZombieType.Nothing,
                    q,
                    true
                );
            }

            // --------------------------------------------------------------------
            // 2) 幸运 (StringLiteral_13694 / 13714)
            // 幸运提高{0:F0}，幸运可以提高好词条出现概率
            // 当前幸运值：{1:F0}/250
            // plantType: 0xe5 → LuckyBlover
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.15f) && lucky < 2.5f)
            {
                Quality q = __instance.GetRandomQuality();
                float delta;

                if (!__instance.superUpgrade || q != Quality.diamond)
                {
                    delta = q switch
                    {
                        Quality.Default    => 0.25f,
                        Quality.silver      => 0.5f,
                        Quality.gold      => 0.75f,
                        Quality.diamond => 1.25f,
                        _                 => 0f
                    };
                }
                else
                {
                    delta = 6.25f;
                }

                string text = string.Format(
                    "幸运提高{0:F0}，幸运可以提高好词条出现概率\n当前幸运值：{1:F0}/250",
                    delta,
                    lucky
                );

                UnityAction onClick = () =>
                {
                    // 原逻辑：对所有植物应用某个 Action + 设置 uncrashable = true
                    // 这里只保留“幸运提升”的语义，避免乱动 uncrashable。
                    __instance._lucky += delta;
                };

                menu.RegisterOption(
                    "幸运",
                    text,
                    onClick,
                    PlantType.LuckyBlover,
                    ZombieType.Nothing,
                    q,
                    true
                );
            }

            // --------------------------------------------------------------------
            // 3) 伤害增幅 (StringLiteral_9596 / 10416)
            // 全体植物获得{0:F0}%伤害增幅
            // 当前增幅：{1:F0}%
            // plantType: 0xfe → EndoFlame
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.15f))
            {
                Quality q = __instance.GetRandomQuality();
                float add;

                if (!__instance.superUpgrade || q != Quality.diamond)
                {
                    add = q switch
                    {
                        Quality.Default    => 0.10f,
                        Quality.silver      => 0.20f,
                        Quality.gold      => 0.30f,
                        Quality.diamond => 0.50f,
                        _                 => 0f
                    };
                }
                else
                {
                    add = q switch
                    {
                        Quality.Default    => 0.10f,
                        Quality.silver      => 0.20f,
                        Quality.gold      => 0.30f,
                        Quality.diamond => 0.50f,
                        _                 => 0f
                    };
                }

                var travel = TravelMgr.Instance;
                if (travel != null)
                {
                    float current = travel.damageAmplification;

                    string text = string.Format(
                        "全体植物获得{0:F0}%伤害增幅\n当前增幅：{1:F0}%",
                        add * 100f,
                        current * 100f
                    );

                    UnityAction onClick = () =>
                    {
                        // 原 IL2CPP 这里用的是一个 DisplayClass + lambda，
                        // 但真正的伤害逻辑在 TravelMgr.damageAmplification 上。
                        travel.damageAmplification += add;
                    };

                    menu.RegisterOption(
                        "伤害增幅",
                        text,
                        onClick,
                        PlantType.EndoFlame,
                        ZombieType.Nothing,
                        q,
                        true
                    );
                }
            }

            // --------------------------------------------------------------------
            // 4) 高级刷新 (StringLiteral_8714 / 8720)
            // 高级刷新，可以刷新词条品质
            // plantType: 0xfe → EndoFlame
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.10f))
            {
                UnityAction onClick = () =>
                {
                    // 原代码这里只是一个 UnityAction 占位，实际逻辑在别处。
                    // 这里保持语义：触发一次“高级刷新”。
                    // TODO: hook into your actual reroll system.
                };

                menu.RegisterOption(
                    "高级刷新",
                    "高级刷新，可以刷新词条品质",
                    onClick,
                    PlantType.EndoFlame,
                    ZombieType.Nothing,
                    Quality.silver,
                    true
                );
            }

            // --------------------------------------------------------------------
            // 5) 保护 (StringLiteral_10001 / 10421)
            // 全体植物获得{0}护盾，复活后依然生效
            // plantType: 0xfe → EndoFlame
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.15f))
            {
                Quality q = __instance.GetRandomQuality();
                float baseValue = q switch
                {
                    Quality.Default    => 1f,
                    Quality.silver      => 2f,
                    Quality.gold      => 3f,
                    Quality.diamond => 5f,
                    _                 => 0f
                };

                float value = baseValue;
                if (ShootingManager.randomType == RandomZombieType.Pea || ShootingManager.randomType == RandomZombieType.Random)
                    value *= 10f;

                string text = string.Format(
                    "全体植物获得{0}护盾，复活后依然生效",
                    (int)value
                );

                UnityAction onClick = () =>
                {
                    // TODO: apply shield to all plants in your framework.
                };

                menu.RegisterOption(
                    "保护",
                    text,
                    onClick,
                    PlantType.EndoFlame,
                    ZombieType.Nothing,
                    q,
                    true
                );
            }

            // --------------------------------------------------------------------
            // 6) 碾压保护 (StringLiteral_4743 / 10426)
            // 全体植物获得抵御碾压的能力，复活后依然生效
            // plantType: 0xfe → EndoFlame
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.05f) && !__instance.uncrashable)
            {
                UnityAction onClick = () =>
                {
                    // 原 b__55_1：Foreach 所有植物 + __this.uncrashable = true
                    __instance.uncrashable = true;
                    // TODO: if you want per‑plant flags, apply them here via Lawnf.GetAllPlants().
                };

                menu.RegisterOption(
                    "碾压保护",
                    "全体植物获得抵御碾压的能力，复活后依然生效",
                    onClick,
                    PlantType.EndoFlame,
                    ZombieType.Nothing,
                    Quality.diamond,
                    true
                );
            }

            // --------------------------------------------------------------------
            // 7) 复活 (StringLiteral_12484 + 12489/12494/12499/12504/12509)
            // plantType: 0xfe → EndoFlame
            // --------------------------------------------------------------------
            if (Random.value < LuckyFactor(0.15f))
            {
                if (!__instance.superUpgrade)
                {
                    Quality q = __instance.GetRandomQuality();
                    float seconds = __instance.reviveTimer / 1000f;
                    string text;
                    UnityAction onClick;

                    switch (q)
                    {
                        case Quality.Default:
                            text = string.Format(
                                "复活时间降低10%\n当前复活时长：{0:F1}秒",
                                seconds
                            );
                            var tmp = () =>
                            {
                                __instance.reviveTimer *= 0.9f;
                            };
                            onClick = tmp; // b__55_9
                            break;

                        case Quality.silver:
                            text = string.Format(
                                "复活时间降低20%\n当前复活时长：{0:F1}秒",
                                seconds
                            );
                            var tmp2 = () =>
                            {
                                __instance.reviveTimer *= 0.8f;
                            };
                            onClick = tmp2; // b__55_10
                            break;

                        case Quality.gold:
                            text = string.Format(
                                "复活时间降低30%\n当前复活时长：{0:F1}秒",
                                seconds
                            );
                            var tmp3 = () =>
                            {
                                __instance.reviveTimer *= 0.7f;
                            };
                            onClick = tmp3; // b__55_11
                            break;

                        case Quality.diamond:
                            text = string.Format(
                                "复活时间降低50%\n当前复活时长：{0:F1}秒",
                                seconds
                            );
                            var tmp4 = () =>
                            {
                                __instance.reviveTimer *= 0.5f;
                            };
                            onClick = tmp4; // b__55_12
                            break;

                        default:
                            goto SkipRevive;
                    }

                    menu.RegisterOption(
                        "复活",
                        text,
                        onClick,
                        PlantType.EndoFlame,
                        ZombieType.Nothing,
                        q,
                        true
                    );
                }
                else
                {
                    // 超级复活：复活时间降低99%，当前复活时长：0秒
                    var a = () =>
                    {
                        // b__55_14
                        __instance.superUpgrade = true;
                        __instance.reviveTimer = 0f;
                    };
                    UnityAction onClick = a;

                    menu.RegisterOption(
                        "复活",
                        "复活时间降低99%\n当前复活时长：0秒",
                        onClick,
                        PlantType.EndoFlame,
                        ZombieType.Nothing,
                        Quality.diamond,
                        true
                    );
                }
            }
        SkipRevive: ;

            // --------------------------------------------------------------------
            // 8) 超质变事件 (7243/6118, 7238/645, 7233/6088+8073+6033)
            // choice 0/1: plantType 0xfe → EndoFlame
            // choice 2: plantType 0x3c9 → UltimateJalaNut
            // --------------------------------------------------------------------
            float roll = Random.value;
            if (roll < 0.0001f && !__instance.appearSuperQualitative && !__instance.endless)
            {
                __instance.appearSuperQualitative = true;

                int choice = Random.Range(0, 3);
                string title;
                string text;
                UnityAction onClick;
                PlantType plantType;

                if (choice == 0)
                {
                    title = "超质变：腐化";
                    text  = "获得词条：腐化";
                    plantType = PlantType.EndoFlame;

                    onClick = () =>
                    {
                        // TODO: implement腐化效果
                    };
                }
                else if (choice == 1)
                {
                    title = "超质变：步步高升";
                    text  = "所有词条一定是最高品质，且钻石词条的加成x5\n注意：部分植物攻速过快时会丢失动画导致无法攻击或攻速降低";
                    plantType = PlantType.EndoFlame;

                    onClick = () =>
                    {
                        // TODO: implement步步高升效果
                    };
                }
                else
                {
                    // choice == 2
                    plantType = PlantType.UltimateJalaNut; // 0x3c9

                    text =
                        "获得词条：力量会给予希望\n" + // 6088
                        "获得植物：" +                  // 6088/8475
                        Lawnf.GetName(PlantType.UltimateJalaNut) + "、" +
                        Lawnf.GetName(PlantType.UltimateGatling) + "、" +
                        "获得600%攻击力加成";          // 6033

                    title = "超质变：步步高升"; // 7233

                    onClick = () =>
                    {
                        // TODO: implement“力量会给予希望”超质变效果
                    };
                }

                menu.RegisterOption(
                    title,
                    text,
                    onClick,
                    plantType,
                    ZombieType.Nothing,
                    Quality.diamond,
                    true
                );
            }

            // --------------------------------------------------------------------
            // 9) 自定义“其他词条”挂钩
            // --------------------------------------------------------------------
            foreach (var custom in CustomRogueOtherBuffRegistry.GetForRun(__instance))
            {
                if (!custom.CanAppear(__instance))
                    continue;

                var tmp = () =>
                {
                    custom.OnGet(__instance);
                };
                UnityAction onClick = tmp;

                menu.RegisterOption(
                    custom.Title,
                    custom.Description,
                    onClick,
                    custom.ShowType,
                    custom.ZombieType,
                    custom.Rarity,
                    custom.Interactable
                );
            }

            return false;
        }
    }
    */
    public interface IUpgradeBuff
    {
        PlantType Plant { get; }
        string Title { get; }
        Quality Rarity { get; }
        void Apply();
    }

    public sealed class VanillaUpgradeWrapper : IUpgradeBuff
    {
        private readonly GeneralBuff _buff;

        public VanillaUpgradeWrapper(GeneralBuff buff) => _buff = buff;

        public PlantType Plant => _buff.plantType;
        public string Title => _buff.Title;
        public Quality Rarity => _buff.Rarity;
        public void Apply() => _buff.OnGet();
    }

    public sealed class CustomUpgradeWrapper : IUpgradeBuff
    {
        private readonly CustomRogueBuff _buff;

        public CustomUpgradeWrapper(CustomRogueBuff buff) => _buff = buff;

        public PlantType Plant => _buff.ShowType;
        public string Title => _buff.Title;
        public Quality Rarity => _buff.Rarity;
        public void Apply() => _buff.OnGet();
    }//*/
}