using Il2CppInterop.Runtime;

namespace CustomPlantClass.Level
{
    /*
    internal class SelectWorldPlants : MonoBehaviour
    {
        public static SelectWorldPlants Instance = null!;
        public static GameObject WorldButton = null!;
        public static GameObject WorldPage = null!;

        public static int PageCardMax => 6 * 9;
        public static Board board => Board.Instance;
        public static List<PlantType> GetPlants() => [.. GameAPP.resourcesManager.allPlants.ToArray().Where(t => !Enum.IsDefined(t))];

        public bool init = false;

        public static void InitButton()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                if (board == null) return;
                GameObject worldButton = null!;
                if (!board.boardTag.isIZ)
                {
                    worldButton = Instantiate(InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/ShowCardLayout/ColorCards"),
                        InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/ShowCardLayout")).gameObject;
                }
                else
                {
                    worldButton = Instantiate(IZBottomMenu.Instance.plantLibrary.transform.FindChild("Buttons/NextPage"),
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Buttons")).gameObject;
                }
                worldButton.name = "SelectWorldPlants";
                worldButton.SetActive(true);
                worldButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "二创冒险植物";
                Destroy(worldButton.GetComponent<UIButton>());
                Instance = worldButton.AddComponent<SelectWorldPlants>();
                WorldButton = worldButton;
            }
            catch (Exception) { }
        }

        /*public void OpenPlantsCard()
        {
            try
            {
                if (board == null) return;

                // reuse existing page
                if (init && WorldPage != null)
                {
                    if (!board.boardTag.isIZ)
                    {
                        var cont = InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer");
                        for (int i = 0; i < cont.childCount; i++)
                            cont.GetChild(i).gameObject.SetActive(false);
                    }
                    else
                    {
                        var cont = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid");
                        for (int i = 0; i < cont.childCount; i++)
                            cont.GetChild(i).gameObject.SetActive(false);
                    }

                    WorldPage.SetActive(true);
                    return;
                }

                // ============================================================
                // NON-IZ MODE: WORLD LAYOUT
                // ============================================================
                if (!board.boardTag.isIZ)
                {
                    var cont = InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer");
                    for (int i = 0; i < cont.childCount; i++)
                        cont.GetChild(i).gameObject.SetActive(false);

                    var page = Instantiate(
                        cont.Find("ColorCards"),
                        cont
                    ).gameObject;

                    WorldPage = page;
                    page.name = "WorldCards";
                    page.SetActive(true);

                    // remove template pages/cards
                    for (int i = page.transform.childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(i).gameObject);
                    for (int i = page.transform.GetChild(0).childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(0).GetChild(i).gameObject);

                    var startCard = page.transform.GetChild(0).GetChild(0).gameObject;

                    // ------------------------------------------------------------
                    // Build world → plant list with filtering
                    // ------------------------------------------------------------
                    var worlds = new List<(string worldName, List<PlantType> plants)>();

                    foreach (var kv in BranchAdventureManager.CustomLevelPlantTypes)
                    {
                        string worldName = kv.Key;
                        var (unlocks, bg, filter) = kv.Value;

                        var worldPlants = unlocks.Values.Where(pt => filter(pt)).ToList();

                        if (worldPlants.Count > 0)
                            worlds.Add((worldName, worldPlants));
                    }

                    // ------------------------------------------------------------
                    // Page layout: fit as many worlds as possible per page
                    // ------------------------------------------------------------
                    const int CardsPerRow = 9;
                    int currentPageCardCount = 0;
                    List<List<(string worldName, List<PlantType> plants)>> pages = new();
                    List<(string worldName, List<PlantType> plants)> currentPage = new();

                    foreach (var w in worlds)
                    {
                        int worldCardCount = w.plants.Count;
                        int worldRows = Mathf.CeilToInt(worldCardCount / (float)CardsPerRow);
                        int worldHeight = worldRows * CardsPerRow;

                        if (currentPageCardCount + worldHeight > PageCardMax)
                        {
                            pages.Add(currentPage);
                            currentPage = new();
                            currentPageCardCount = 0;
                        }

                        currentPage.Add(w);
                        currentPageCardCount += worldHeight;
                    }

                    pages.Add(currentPage);

                    // ------------------------------------------------------------
                    // Instantiate pages
                    // ------------------------------------------------------------
                    page.transform.GetChild(0).gameObject.name = "WorldPage_1";

                    for (int i = 1; i < pages.Count; i++)
                    {
                        var tmp = Instantiate(page.transform.GetChild(0).gameObject, page.transform);
                        tmp.name = $"WorldPage_{i + 1}";
                    }

                    // ------------------------------------------------------------
                    // Fill pages with worlds
                    // ------------------------------------------------------------
                    for (int pi = 0; pi < pages.Count; pi++)
                    {
                        var parentPage = page.transform.GetChild(pi);

                        foreach (var (worldName, plants) in pages[pi])
                        {
                            // WORLD HEADER
                            var headerObj = new GameObject("WorldHeader", typeof(RectTransform).ToIl2CppType(), typeof(TextMeshProUGUI).ToIl2CppType());
                            headerObj.transform.SetParent(parentPage, false);
                            var headerText = headerObj.GetComponent<TextMeshProUGUI>();
                            headerText.text = worldName;
                            headerText.fontSize = 32;
                            headerText.color = Color.white;

                            // WORLD GRID
                            var worldGrid = new GameObject("WorldGrid", typeof(RectTransform).ToIl2CppType());
                            worldGrid.transform.SetParent(parentPage, false);

                            foreach (var pt in plants)
                            {
                                var cardObj = Instantiate(startCard, worldGrid.transform);
                                var card = cardObj.transform.GetChild(1).GetComponent<CardUI>();
                                var packet = cardObj.transform.GetChild(0);

                                cardObj.SetActive(true);

                                Mouse.Instance.ChangeCardSprite(pt, card);

                                Image image = card.transform.GetChild(0).GetComponent<Image>();
                                image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                                packet.GetChild(0).GetComponent<Image>().sprite = image.sprite;
                                packet.GetChild(0).GetComponent<RectTransform>().sizeDelta =
                                    card.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;

                                card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    PlantDataManager.PlantData_Default[pt].cost.ToString();
                                packet.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                    PlantDataManager.PlantData_Default[pt].cost.ToString();

                                card.GetComponent<BoxCollider2D>().enabled = true;

                                card.thePlantType = pt;
                                card.theSeedType = (int)pt;
                                card.theSeedCost = PlantDataManager.PlantData_Default[pt].cost;
                                card.fullCD = PlantDataManager.PlantData_Default[pt].cd;

                                cardObj.name = pt.ToString();
                            }
                        }
                    }

                    Destroy(startCard);
                }

                // ============================================================
                // IZ MODE: tight-packed, auto-unlock all custom plants
                // ============================================================
                else
                {
                    var cont = IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid");
                    for (int i = 0; i < cont.childCount; i++)
                        cont.GetChild(i).gameObject.SetActive(false);

                    var page = Instantiate(
                        cont.Find("全部植物"),
                        cont
                    ).gameObject;

                    WorldPage = page;
                    page.name = "二创冒险植物";
                    page.SetActive(true);

                    var list = GameAPP.resourcesManager.allPlants.ToArray()
                        .Where(pt => !Enum.IsDefined(pt))
                        .ToList();

                    int count = list.Count;
                    int pageNum = count / PageCardMax + (count % PageCardMax > 0 ? 1 : 0);

                    for (int i = page.transform.childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(i).gameObject);
                    for (int i = page.transform.GetChild(0).childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(0).GetChild(i).gameObject);

                    var startCard = page.transform.GetChild(0).GetChild(0).gameObject;
                    int remain = count;

                    page.transform.GetChild(0).gameObject.name = "PlantCardPage_1";
                    for (int i = 1; i < pageNum; i++)
                    {
                        var tmp = Instantiate(page.transform.GetChild(0).gameObject, page.transform);
                        tmp.name = $"PlantCardPage_{i + 1}";
                    }

                    int listIndex = 0;

                    for (int i = 0; i < pageNum; i++)
                    {
                        var parent = page.transform.GetChild(i);

                        for (int j = 0; j < PageCardMax; j++)
                        {
                            if (remain == 0) break;

                            var pt = list[listIndex];

                            var cardObj = Instantiate(startCard, parent);
                            var card = cardObj.transform.GetChild(0).GetComponent<CardUI>();
                            cardObj.SetActive(true);

                            Image image = card.transform.GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();

                            card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                PlantDataManager.PlantData_Default[pt].cost.ToString();

                            card.gameObject.SetActive(true);
                            Mouse.Instance.ChangeCardSprite(pt, card);

                            card.GetComponent<BoxCollider2D>().enabled = true;

                            card.thePlantType = pt;
                            card.theSeedType = (int)pt;
                            card.theSeedCost = 0;
                            card.fullCD = 0f;
                            cardObj.name = pt.ToString();

                            listIndex++;
                            remain--;
                        }
                    }

                    Destroy(startCard);
                }

                init = true;
            }
            catch (Il2CppException) { }
        }
        public void OpenPlantsCard()
        {
            try
            {
                if (board == null) return;
                if (init && WorldPage != null)
                {
                    if (!board.boardTag.isIZ)
                        for (int i = 0; i < InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").childCount; i++)
                            InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(i).gameObject.SetActive(false);
                    else
                        for (int i = 0; i < IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").childCount; i++)
                            IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").GetChild(i).gameObject.SetActive(false);
                    WorldPage.SetActive(true);
                    return;
                }
                if (!board.boardTag.isIZ)
                {
                    for (int i = 0; i < InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").childCount; i++)
                        InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer").GetChild(i).gameObject.SetActive(false);
                    var page = Instantiate(InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer/ColorCards"),
                        InGameUI.Instance.transform.FindChild("Bottom/SeedLibrary/Grid/CardPagesContainer")).gameObject;
                    WorldPage = page;
                    page.name = "CustomCards";
                    page.SetActive(true);
                    var list = new List<PlantType>();
                    BranchAdventureManager.CustomLevelPlantTypes.Where((
                        KeyValuePair<string, (Dictionary<int, PlantType>, Sprite, Func<PlantType, bool>)> a)=>{
                            list.Union(a.Value.Item1.Values);
                            return false;
                    });
                    int count = list.Count;
                    int pageNum = count / PageCardMax + (count % PageCardMax > 0 ? 1 : 0); // 计算需要的页数
                    for (int i = page.transform.childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(i).gameObject); // 销毁除第一页以外的所有页
                    for (int i = page.transform.GetChild(0).childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(0).GetChild(i).gameObject); // 销毁除第一张卡以外的所有卡
                    var startCard = page.transform.GetChild(0).GetChild(0).gameObject;
                    int remain = count;

                    page.transform.GetChild(0).gameObject.name = "SampleGrid_1";
                    for (int i = 1; i < pageNum; i++) // 已经有第一个了，可以少实例化一个
                    {
                        var tmp = Instantiate(page.transform.GetChild(0).gameObject, page.transform); // 实例化页面
                        tmp.name = $"SampleGrid_{i + 1}";
                    }

                    int listIndex = 0; // 循环变量，位于list的哪一个索引
                    for (int i = 0; i < pageNum; i++) // 实例化卡，要从第一个开始实例化
                    {
                        var parent = page.transform.GetChild(i);
                        for (int j = 0; j < PageCardMax; j++)
                        {
                            var pt = list[listIndex];
                            var cardObj = Instantiate(startCard, parent);
                            var card = cardObj.transform.GetChild(1).GetComponent<CardUI>();
                            var packet = cardObj.transform.GetChild(0);
                            packet.localPosition = card.transform.localPosition;
                            packet.localRotation = card.transform.localRotation;
                            packet.localScale = card.transform.localScale;
                            cardObj.SetActive(true);

                            //修改图片
                            Mouse.Instance.ChangeCardSprite(pt, card);

                            //背景图片
                            Image image = card.transform.GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                            packet.GetChild(0).GetComponent<Image>().sprite = image.sprite;
                            // image.SetNativeSize();
                            packet.GetChild(0).GetComponent<RectTransform>().sizeDelta = card.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
                            // packet.GetChild(0).GetComponent<Image>().SetNativeSize();

                            //设置价格
                            card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataManager.PlantData_Default[pt].cost.ToString();
                            packet.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataManager.PlantData_Default[pt].cost.ToString();

                            cardObj.gameObject.SetActive(true);

                            card.GetComponent<BoxCollider2D>().enabled = true;

                            //设置数据
                            card.thePlantType = pt;
                            card.theSeedType = (int)pt;
                            card.theSeedCost = PlantDataManager.PlantData_Default[pt].cost;
                            card.fullCD = PlantDataManager.PlantData_Default[pt].cd;
                            cardObj.name = pt.ToString();

                            listIndex++;
                            // 如果没了就结束循环
                            remain--;
                            if (remain == 0) break;
                        }
                    }
                    Destroy(startCard);
                }
                else
                {
                    for (int i = 0; i < IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").childCount; i++)
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid").GetChild(i).gameObject.SetActive(false);
                    var page = Instantiate(IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid/全部植物"),
                        IZBottomMenu.Instance.plantLibrary.transform.FindChild("Grid")).gameObject;
                    WorldPage = page;
                    page.name = "二创冒险植物";
                    page.SetActive(true);
                    var list = new List<PlantType>();
                    BranchAdventureManager.CustomLevelPlantTypes.Where((
                        KeyValuePair<string, (Dictionary<int, PlantType>, Sprite, Func<PlantType, bool>)> a)=>{
                            list.Union(a.Value.Item1.Values);
                            return false;
                    });
                    int count = list.Count;
                    int pageNum = count / PageCardMax + (count % PageCardMax > 0 ? 1 : 0); // 计算需要的页数
                    for (int i = page.transform.childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(i).gameObject); // 销毁除第一页以外的所有页
                    for (int i = page.transform.GetChild(0).childCount - 1; i >= 1; i--)
                        Destroy(page.transform.GetChild(0).GetChild(i).gameObject); // 销毁除第一张卡以外的所有卡
                    var startCard = page.transform.GetChild(0).GetChild(0).gameObject;
                    int remain = count;

                    page.transform.GetChild(0).gameObject.name = "PlantCardPage_1";
                    for (int i = 1; i < pageNum; i++) // 已经有第一个了，可以少实例化一个
                    {
                        var tmp = Instantiate(page.transform.GetChild(0).gameObject, page.transform); // 实例化页面
                        tmp.name = $"PlantCardPage_{i + 1}";
                    }

                    int listIndex = 0; // 循环变量，位于list的哪一个索引
                    for (int i = 0; i < pageNum; i++) // 实例化卡，要从第一个开始实例化
                    {
                        var parent = page.transform.GetChild(i);
                        for (int j = 0; j < PageCardMax; j++)
                        {
                            var pt = list[listIndex];
                            var cardObj = Instantiate(startCard, parent);
                            var card = cardObj.transform.GetChild(0).GetComponent<CardUI>();
                            cardObj.SetActive(true);

                            //背景图片
                            Image image = card.transform.GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[pt].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();

                            //设置价格
                            card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                                PlantDataManager.PlantData_Default[pt].cost.ToString();

                            card.gameObject.SetActive(true);

                            //修改图片
                            Mouse.Instance.ChangeCardSprite(pt, card);

                            card.GetComponent<BoxCollider2D>().enabled = true;

                            //设置数据
                            card.thePlantType = pt;
                            card.theSeedType = (int)pt;
                            card.theSeedCost = 0;
                            card.fullCD = 0f;
                            cardObj.name = pt.ToString();

                            listIndex++;
                            // 如果没了就结束循环
                            remain--;
                            if (remain == 0) break;
                        }
                    }
                    Destroy(startCard);
                }
                init = true;
            }
            catch (Il2CppException) { }
        }

        public void Update()
        {
            //判断鼠标按下
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (Input.GetMouseButtonDown(0) && WorldButton != null)
            {
                //击中二创冒险植物Button
                if (hit.collider != null && hit.collider.gameObject == WorldButton)
                    OpenPlantsCard();
            }

            //设置鼠标特效
            if (WorldButton != null && hit.collider != null && hit.collider.gameObject == WorldButton)
                CursorChange.SetClickCursor();
        }
        internal class CustomCardComponent : MonoBehaviour
        {
            CardUI self => GetComponent<CardUI>();
            int cd=2;
            public Sprite newBg { get; private set; }
            public void Init(Sprite bg)
            {
                newBg = bg;
            }
            public void Update()
            {
                cd-=1;
                if (cd < 0) Event();
            }
            public void Event()
            {
                Destroy(this);
            }
        }
    }
    */
}