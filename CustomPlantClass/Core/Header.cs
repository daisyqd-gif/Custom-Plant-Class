// ===== System Collections =====
global using System.Collections;
global using System.Collections.Generic;
// ===== System Text =====
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;
// ===== System Misc =====
global using System;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Reflection;
global using System.Diagnostics;
global using System.Threading.Tasks;
global using System.Security.Cryptography;
// ===== BepInEx =====
global using BepInEx;
global using BepInEx.Logging;
global using BepInEx.Unity.IL2CPP;
// ===== Unity =====
global using UnityEngine;
global using UnityEngine.UI;
global using UnityEngine.Rendering;
global using Unity.VisualScripting;
// ===== Other =====
global using Il2CppInterop.Runtime.Injection;
global using GameLevel.RogueShooting;
global using CustomizeLib.BepInEx;
global using HarmonyLib;
global using TMPro;
global using Core;
global using UI;
// ===== CustomPlantClass =====
global using CustomPlantClass.RogueShootingMgr;
global using CustomPlantClass.Main;
global using CustomPlantClass.Level;
// ===== ModLoader =====
global using FrameWorkLoader.API;
global using FrameWorkLoader.Loader;
// ===== Static Aliases =====
global using static Zombie;
global using static Plant;
global using static Board;
// ===== Aliases =====
global using Random = UnityEngine.Random;
global using Object = UnityEngine.Object;
global using Debug = UnityEngine.Debug;
global using File = System.IO.File;
[assembly: CustomMod(CustomPlantClass.MyPluginInfo.PluginName)]
namespace CustomPlantClass;