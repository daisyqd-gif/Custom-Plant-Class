#nullable enable
namespace CustomPlantClass.Main
{
    /// <summary>
    /// Core metadata for defining a custom plant.
    /// </summary>
    public struct BaseCustomPlantData
    {
        public ID PlantId;
        public GameObject Prefab;
        public GameObject Preview;

        public List<(ID, ID)> Fusions;
        public float AttackInterval;
        public float ProduceInterval;
        public int AttackDamage;
        public int MaxHealth;
        public float Cd;
        public int Sun;
        public BulletType DefaultBullet;
        public bool CanPF;
        public bool CanStarUp;
        public CardLevel CardColor;
        public bool IsRainbowCard;
        public bool IsUltimatePlant;
        public int CardRepeatAmt;
        public string Name;
        public string AlmanacEntry;

        public static BaseCustomPlantData Create(
            ID id,
            GameObject prefab,
            GameObject preview
        ) => new BaseCustomPlantData
        {
            PlantId = id,
            Prefab = prefab,
            Preview = preview,

            // defaults
            Fusions = [],
            AttackInterval = 0f,
            ProduceInterval = 0f,
            AttackDamage = 0,
            MaxHealth = 300,
            Cd = 0f,
            Sun = 0,
            DefaultBullet = BulletType.Bullet_pea,
            CanPF = false,
            CanStarUp = false,
            CardColor = CardLevel.White,
            IsRainbowCard = false,
            IsUltimatePlant = false,
            CardRepeatAmt = 1,
            Name = "",
            AlmanacEntry = ""
        };
    }

    /// <summary>
    /// Metadata for defining a custom plant skin.
    /// </summary>
    public struct BasePlantSkinData
    {
        public BaseCustomPlantData data;
        public GameObject SkinPrefab;
        public GameObject SkinPreview;
        public List<(BulletType, List<GameObject?>)> BulletSkinList;
    }

    /// <summary>
    /// Metadata for defining a bullet.
    /// </summary>
    public struct BaseCustomBulletData
    {
        public ID BulletId;
        public GameObject Prefab;

    }
    public struct BaseCustomZombieData
    {
        public ID theZombieType;
        public GameObject Prefab;
        public Sprite Preview;
        public int theAtackDamage;
        public int maxHealth;
        public int theFirstArmorHealth;
        public FirstArmorType theFirstArmorType;
        public string theFirstArmorPath;
        public int theSecondArmorHealth;
        public SecondArmorType theSecondArmorType;
        public string theSecondArmorPath;
        public int SpawnLevel;
        public int SpawnWeight;
    }
    public struct BaseCustomGridItemData
    {
        public ID type;
        public GameObject Prefab;
    }
    public struct CustomBossHealthSliderData
    {
        public ZombieType theZombieType = ZombieType.Nothing;
        public Sprite? Icon;
        public Sprite? FillIcon;
        public Color FillColor = Color.magenta;

        public CustomBossHealthSliderData(Sprite icon)
        {
        }
    }
    public struct BoardPosition
    {
        public int Row { get; }
        public int Column { get; }

        public BoardPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        // BoardPosition → world position
        public static implicit operator Vector2(BoardPosition pos)
        {
            float x = pos.Column * 1.35f - 4.8f;

            Board board = Instance;
            bool roof = board.boardTag.isRoof;
            int rows = board.rowNum;

            float y;

            if (!roof)
            {
                if (rows == 6)
                    y = 2.3f - pos.Row * 1.45f;
                else
                    y = 2.3f - pos.Row * 1.67f;
            }
            else
            {
                // Roof math
                if (x <= 1.5f)
                {
                    float f = (pos.Row * 1.4f);
                    y = 1.6f - f + x * 0.22f + 0.5f;
                }
                else
                {
                    y = 4.0f - pos.Row * 1.45f;
                }
            }

            return new Vector2(x, y);
        }

        // world position → BoardPosition
        public static implicit operator BoardPosition(Vector2 world)
        {
            float x = world.x;
            float y = world.y;

            Board board = Instance;
            bool roof = board.boardTag.isRoof;
            int rows = board.rowNum;

            // Column
            int col = Mathf.FloorToInt((x + 5.6f) / 1.35f);
            col = Mathf.Clamp(col, 0, board.columnNum - 1);

            // Row
            int row;

            if (!roof)
            {
                if (rows == 6)
                    row = Mathf.FloorToInt((3.7f - y) / 1.45f);
                else
                    row = Mathf.FloorToInt((3.7f - y) / 1.67f);
            }
            else
            {
                if (x <= 1.5f)
                {
                    float f = (y - x * 0.22f) - 0.5f;
                    row = Mathf.FloorToInt((1.6f - f) / 1.4f) + 1;
                }
                else
                {
                    row = Mathf.FloorToInt((4.0f - y) / 1.45f);
                }
            }

            row = Mathf.Clamp(row, 0, rows - 1);

            return new BoardPosition(row, col);
        }

        public override string ToString() => $"({Row}, {Column})";
    }
    public struct Struct1_Plant
    {
        public Type BaseType;
        public Type CustomType;
        public BaseCustomPlantData data;
    }
    public struct BaseCustomLevelData
    {
        public BaseCustomLevelData()
        {
        }

        public LevelType LevelType { readonly get; set; } = LevelType.Nothing;
        public int LevelID { readonly get; set; } = -114514;
        public string LevelName { readonly get; set; } = "";
        public string LevelNameEn { readonly get; set; } = "";
        public Sprite? LevelSprite { readonly get; set; } = default;
        public SceneType SceneType { readonly get; set; } = SceneType.Day_6;
        public GameObject? ScenePrefab { readonly get; set; } = default;
        public Sprite? SceneBackground { readonly get; set; } = default;
        public MusicType MusicType { readonly get; set; } = (MusicType)(-1);
        public AudioClip? MusicAudio { readonly get; set; } = default;
        public int MaxWave { readonly get; set; } = 100;
        public List<ZombieType> ZombieTypes { readonly get; set; } = new() { ZombieType.RandomZombie, ZombieType.RandomPlusZombie, ZombieType.DiamondRandomZombie };
        public BoxType_Short[,] MapRoadTypes { readonly get; set; } = new BoxType_Short[,] { };
        public CustomLevelSelection selection { readonly get; set; } = default;
        public PlantType[] SelectTypes { readonly get; set; } = new PlantType[] { };
        public Action EnterAction { readonly get; set; } = () => { };
        public Action<Board> EnterGameAction { readonly get; set; } = (Board b) => { };
        public int SunCounter { readonly get; set; } = default;
        public List<AdvBuff> AdvBuffs { readonly get; set; } = new();
        public List<UltiBuff> UltiBuffs { readonly get; set; } = new();
        public List<TravelUnlocks> TravelUnlocks { readonly get; set; } = new();
        public List<TravelDebuff> TravelDebuffs { readonly get; set; } = new();
        public BoardTag BoardTag { readonly get; set; } = default;
    }
    public enum CustomLevelSelection
    {
        Normal = 0,
        Convey = 1,
        PreSelected = 2
    }
    public enum BossSliderType
    {
        UltimateSword = 0,
        ObsidianGarcantuar = 1,
        UltimateDrown = 2,
        UltimateFootball = 3,
        UltimateHorse = 4,
        UltimateImp = 5,
        UltimateJackbox = 6,
        UltimateJackson = 7,
        UltimateKirov = 8,
        UltimateLegion = 9,
        UltimateMachineNut = 10,
        UltimatePaper = 11,
        UltimateSnow = 12
    }
    public enum PlantLevelData
    {
        Basic = 0,
        Secondary = 1,
        Super = 2,
        WeakUltimate = 3,
        StrongUltimate = 4,
        FinalUltimate = 5,
        TreasurePlant = 6
    }
    public enum BoxType_Short
    {
        G = 0,         // 草地
        W = 1,         // 水域
        D = 2,         // 泥土
        R = 3,         // 屋顶
        S = 4,         // 石头
        River = 5,     // 河流
        Dirt_water = 6 // 泥水域
    }
}