namespace CustomPlantClass.Main
{
    public class GameAPPInitBehaviour : MonoBehaviour
    {
        public static List<Struct1_Plant> data_plant = new();
        public static ResourcesManager resourcesManager => GameAPP.resourcesManager;
        public static void RegisterAllPlants()
        {
            foreach (var i in data_plant)
            {
                List<GameObject> list = new();
                var data = i.data;
                var id = data.PlantId;
                GameObject plantPrefab = data.Prefab;
                var comp = plantPrefab.AddComponent(i.BaseType.ToIl2CppType()).GetComponent<Plant>();
                comp.thePlantType = id;
                if (i.CustomType != null) plantPrefab.AddComponent(i.CustomType.ToIl2CppType());
                list.Add(plantPrefab);
                resourcesManager.plantPrefabs.Add(id, plantPrefab);
                resourcesManager._plantPrefabs.Add(id, list.ToIl2CppList());
                resourcesManager.plantPreviews.Add(id, data.Preview);
                resourcesManager.allPlants.Add(id);
                PlantDataManager.PlantData defData = new()
                {
                    thePlantType = id,
                    maxHealth = data.MaxHealth,
                    cost = data.Sun,
                    attackInterval = data.AttackInterval,
                    produceInterval = data.ProduceInterval,
                    cd = data.Cd,
                    attackDamage = data.AttackDamage
                };
                PlantDataManager.PlantData_Default.Add(id, defData);
                foreach (var j in data.Fusions)
                {
                    MixData.AddOrderedRecipe(j.Item1, j.Item2, id);
                }
            }
        }
    }
}