namespace CustomPlantClass
{
    public static class Compatibility
    {
        public class CustomCore_Old
        {
            /// <summary>
            /// 注册自定义词条
            /// </summary>
            /// <param name="text">词条描述</param>
            /// <param name="buffType">词条类型</param>
            /// <param name="canUnlock">解锁条件</param>
            /// <param name="cost">价格</param>
            /// <param name="plantType">显示的植物类型</param>
            /// <param name="level">最大等级</param>
            /// <param name="bg">背景</param>
            /// <returns>词条ID</returns>
            public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost,
                PlantType plantType = PlantType.Nothing, int level = 1, BuffBgType bg = default) =>
                RegisterCustomBuff(text, buffType, canUnlock, cost, PlantType.Nothing, false, plantType, level, bg);

            /// <summary>
            /// 注册自定义词条
            /// </summary>
            /// <param name="text">词条描述</param>
            /// <param name="buffType">词条类型</param>
            /// <param name="canUnlock">解锁条件</param>
            /// <param name="cost">价格</param>
            /// <param name="plantType">显示的植物类型</param>
            /// <param name="level">最大等级</param>
            /// <param name="bg">背景</param>
            /// <param name="infoType">判定类型</param>
            /// <param name="addProbability">增加植物在场时词条抽取概率</param>
            /// <returns>词条ID</returns>
            public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost, PlantType infoType, bool addProbability,
                PlantType plantType = PlantType.Nothing, int level = 1, BuffBgType bg = default) =>
                RegisterCustomBuff(text, buffType, canUnlock, cost, plantType, level, bg, plantType: infoType, addProbability: addProbability);

            /// <summary>
            /// 注册自定义僵尸词条
            /// </summary>
            /// <param name="text">词条描述</param>
            /// <param name="zombieType">显示的僵尸类型</param>
            /// <param name="level">等级</param>
            /// <param name="bg">背景</param>
            /// <returns></returns>
            public static int RegisterCustomDebuff(string text, Func<bool> unlock = null, ZombieType zombieType = ZombieType.NormalZombie, int level = 1, BuffBgType bg = default) =>
                RegisterCustomBuff(text, BuffType.Debuff, unlock, 0, PlantType.Nothing, level: level, bgType: bg, zombieType);
            public static int RegisterCustomBuff(string text, BuffType buffType, Func<bool> canUnlock, int cost,
                PlantType icon, int level,
                BuffBgType bgType = default, ZombieType zombieType = ZombieType.NormalZombie,
                int buffID = -1, PlantType plantType = PlantType.Nothing, bool addProbability = false)
            {
                if (buffType == BuffType.UnlockPlant)
                    buffID = -5422; //<-as long as it is not -1, it allocates an ID
                return CustomCore.RegisterCustomBuff(new BuffConfig()
                {
                    desc = text,
                    type = buffType,
                    unlock = canUnlock,
                    cost = cost,
                    iconPlant = icon,
                    maxLevel = level,
                    backGround = bgType,
                    iconZombie = zombieType,
                    ID = buffID == -1 ? null : buffID,
                    lockPlantType = plantType,
                    probably = addProbability
                });
            }
        }
    }
}