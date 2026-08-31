# PVZ Fusion Custom Plant Class Framework
A modular, extensible gameplay framework for Plants vs. Zombies Fusion that allows modders to create custom plants, zombies, effects, projectiles, and gameplay systems using clean C# APIs.
## Overview
This framework provides a unified API for extending PVZ Fusion without modifying the game’s internal code. It is designed for modders who want to build:
- new plants
- new zombies
- new projectiles
- new effects
- new mechanics
- new levels
- etc...
  
All through a stable C# interface.
It is built on top of:
- CustomizeLib (Apache‑2.0)
- BepInEx (LGPL‑2.1)
- PVZ Fusion’s internal gameplay architecture
- [![Roslyn](https://img.shields.io/nuget/v/Microsoft.CodeAnalysis.CSharp?label=Roslyn&color=blue)](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/)
## Features
1. Base classes for plants, zombies, levels, etc...
2. A central framework for registration
3. Convinence helpers for easier coding
4. A custom mod loader ( Incomplete )
5. Registries and an event manager for cross mod communication
6. Custom structs and data types for easier data storage
### Future
1. A way to run mods in Lua
2. Runtime compilation of mods
3. A visual plant creator that allows modders or designers to create plants with minimal coding
## Installation
1. Install this custom fork of BepInEx 6.0.0 and CustomizeLib from https://pan.quark.cn/s/6461fdaccff5#/list/share -> 融合版/融合Mod/鲑鱼MOD整理/BepinEX版本/BepInEx前置框架 and download all files in the folder and place them into the game's directory (where PlantsVsZombiesRH.exe is located) and also locate customizelib in any of salmon's mods and put it in. TEMPORARY WARNING: Do not use the August 22 version
2. Locate the release for PVZ Fusion Custom Plant Class Framework in the releases tab in this repo and download it into: gamedirectory/BepInEx/plugins
3. Run the game. You should see a black window open up and its name should be: BepInEx 6.0.0 dev - PlantsVsZombiesRH.exe). When the game is fully booted up and it is in the main menu, wait 5 seconds and close the game.
4. You should see a folder called interop in the BepInEx folder
5. Done!
## Creating a mod

### Prerequisites:
- Medium to advanced understanding in the C# coding language
- Medium understanding in making AssetBundles in Unity
- A 64 bit computer running Windows 10 or later
- Basic drawing and animating capibilities in Unity

### Guide:
1. Create your modding folder.
2. Install tools like dnspy(https://github.com/dnSpy/dnSpy/releases/tag/v6.1.8), Il2CppDumper(https://github.com/Perfare/Il2CppDumper/releases), Tuanjie editor 2022.3, Microsoft Visual Studio Code(https://apps.microsoft.com/detail/xp9khm4bk9fz7q), and idealy a dissasembler.
3. Put your tools into the buid folder in their own folders.
4. Create a folder and name it combinemod. This will be your build folder.
5. Download the template folder from here https://github.com/daisyqd-gif/PVZRH-modding-tools/releases and extract the sourcecode.zip into your combinemod folder
6. Create a folder called "lib" in the combinemod folder
7. Run Il2CppDumper on the game and copy the generated dummydll folder into the lib folder.
8. Locate the BepInEx folder and copy it into the lib folder.
9. Copy the template folder and name it your mod and follow the instructions there.
### An example of a full mod
```csharp
global using BepInEx;
global using CustomizeLib.BepInEx;
global using HarmonyLib;
global using System.Reflection;
global using UnityEngine;
global using CustomPlantClass;
global using CustomPlantClass.Main;

namespace FreezeGatlingPea
{
    [BepInPlugin(MyPluginInfo.PluginGuid, MyPluginInfo.PluginName, MyPluginInfo.PluginVersion)]
    public class Core : ModPlugin
    {
        private AssetBundle assetBundle;
        private ID plantType;
        public override void InitializeMod()
        {
            // Load the AssetBundle containing your plant prefab(s)
            // Replace "abname" with your actual bundle name
            assetBundle = CustomCore.GetAssetBundle(
                Assembly.GetExecutingAssembly(),
                "freezegatlingpea"
            );
        }
        public override void OnGameInit()
        {
            TypeData.SnowPlants.Add(plantType);
        }
        public override void InitializePlants()
        {
            // Fill out the plant metadata
            BaseCustomPlantData Data = new BaseCustomPlantData()
            {
                PlantId = DataMgr.AllocateID(), // Automatically assigns a unique ID

                Prefab = assetBundle.GetAsset<GameObject>("FreezeGatlingPeaPrefab"),   // Main plant prefab
                Preview = assetBundle.GetAsset<GameObject>("FreezeGatlingPeaPreview"), // Card preview prefab

                Fusions = DataMgr.MirrorTuple((PlantType.GatlingPea,PlantType.WaterAloes)), // Optional fusion recipes

                AttackInterval = 1.5f,   // Time between attacks (shooters only)
                ProduceInterval = 0f,  // Time between sun/production cycles
                AttackDamage = 20,      // Damage per attack
                MaxHealth = 300,       // Plant HP
                Cd = 1.5f,               // Card cooldown
                Sun = 475,               // Sun cost

                DefaultBullet = BulletType.Bullet_pea, // Shooter bullet type, this is never used for now so just leave it as is.

                CanPF = false,     // Enable PF ability if the plant has one
                CanStarUp = false, // Enable Star-Up ability if the plant has one

                CardColor = CardLevel.Green, // Determines card rarity and UI color
                /*
                    White  = Normal plants
                    Green  = Fusion plants
                    Blue   = Super plants
                    Purple = Weak ultimate plants
                    Gold   = Strong ultimate plants
                    Red    = Special/Treasure mode plants
                */

                IsRainbowCard = false,  // Appears in the Rainbow Card menu
                IsUltimatePlant = false, // Travel-locked ultimate plant
                CardRepeatAmt = 1,       // How many copies appear in Rainbow Card menu

                Name = "极冰机枪射手",           // Plant name (shown in UI)
                AlmanacEntry = "机枪射手一次可以发射四颗豌豆。当充能超过4时，可以发射四颗极冰豆。\n\n"+    // Almanac description (CN + EN recommended)
                "<color=#3D1400>融合配方：</color><color=red>机枪射手 + 水滴芦荟</color>\n"+
                "<color=#3D1400>伤害：</color><color=red>20×4/1.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①免疫冻结和冰封，受到雪球/雪叉/寒冰菇/旗帜波暴风雪效果时获得1/1/15/60层充能。可消耗1层充能投出极冰豆。</color>\n"
            };

            // Register the plant and retrieve its ID
            plantType = DataMgr.RegisterCustomPlant<GatlingPea, FreezeGatlingPea>(Data);

            Log.LogInfo($"{MyPluginInfo.PluginName} {MyPluginInfo.PluginVersion} loaded.");
        }
    }

    // Your custom plant class. Put this into its own file if it gets too big
    // You can leave it empty or override BaseCustomPlant methods for custom behavior.
    public class FreezeGatlingPea : BaseCustomPlant
    {
        public override Transform FindShoot() => _plant.transform.FindChild("GatlingPea_head/Shoot");
        public override Bullet Shoot_Custom()
        {
            ReplaceSprite();
            Vector2 pos=_plant.shoot.position;
            if (_plant.attributeCount >= 1)
            {
                Bullet b1=CreateBullet.Instance.SetBullet(pos.x,pos.y,_plant.thePlantRow,BulletType.Bullet_extremeSnowPea,BulletMoveWay.MoveRight);
                b1.Damage=_plant.attackDamage*2;
                b1.fromType=_plant.thePlantType;
                Bullet b4 = CreateBullet.Instance.SetBullet(
                    pos.x, pos.y, _plant.thePlantRow,
                    BulletType.Bullet_extremeSnowPea,
                    BulletMoveWay.Free
                );
                b4.Damage = _plant.attackDamage*2;
                b4.fromType = _plant.thePlantType;
                b4.transform.Rotate(0, 0, 45);
                Bullet b5 = CreateBullet.Instance.SetBullet(
                    pos.x, pos.y, _plant.thePlantRow,
                    BulletType.Bullet_extremeSnowPea,
                    BulletMoveWay.Free
                );
                b5.Damage = _plant.attackDamage*2;
                b5.fromType = _plant.thePlantType;
                b5.transform.Rotate(0, 0, 30);
                Bullet b2 = CreateBullet.Instance.SetBullet(
                    pos.x, pos.y, _plant.thePlantRow,
                    BulletType.Bullet_extremeSnowPea,
                    BulletMoveWay.Free
                );
                b2.Damage = _plant.attackDamage*2;
                b2.fromType = _plant.thePlantType;
                b2.transform.Rotate(0, 0, -30);
                Bullet b3 = CreateBullet.Instance.SetBullet(
                    pos.x, pos.y, _plant.thePlantRow,
                    BulletType.Bullet_extremeSnowPea,
                    BulletMoveWay.Free
                );
                b3.Damage = _plant.attackDamage*2;
                b3.fromType = _plant.thePlantType;
                b3.transform.Rotate(0, 0, -45);
                _plant.attributeCount=-1;
                return b1;
            }
            Bullet b=CreateBullet.Instance.SetBullet(pos.x,pos.y,_plant.thePlantRow,BulletType.Bullet_pea,BulletMoveWay.MoveRight);
            b.Damage=_plant.attackDamage;
            b.fromType=_plant.thePlantType;
            return b;
        }
        public override string GetTextString() => "充能: "+_plant.attributeCount;
        public void ReplaceSprite()
        {
            var head1=transform.FindChild("GatlingPea_head");
            var head2=transform.FindChild("SnowGatling_head");
            if (_plant.attributeCount >= 1)
            {
                head1.gameObject.SetActive(false);
                head2.gameObject.SetActive(true);
            }
            else
            {
                head1.gameObject.SetActive(true);
                head2.gameObject.SetActive(false);
            }
        }
    }

    public class MyPluginInfo
    {
        public const string PluginGuid = "FreezeGatlingPea.Bepinex";
        public const string PluginName = "FreezeGatlingPea";
        public const string PluginVersion = "3.7";
    }

    [HarmonyPatch(typeof(Plant))]
    public static class Plant_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Plant.UpdateAttackCountDown))]
        public static void UpdateAttackCountDown_Postfix(Plant __instance)
        {
            if (__instance.TryGetComponent<FreezeGatlingPea>(out _) &&
                __instance.attributeCount >= 1)
            {
                __instance.thePlantAttackCountDown -= Time.deltaTime;
            }
        }
    }
}
```
### Updating your mod
1. Install the newest game version and repeat step 1, 2, 3, and 4  in the installation guide.
2. Repeat steps 5, 7, 8, 9 in the creating a mod guide.
3. Reopen all of your mods and rebuild all of them and fix all errors that resulted from the update.
