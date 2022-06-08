using System;
using System.Collections.Generic;
using UnityEngine;
using StationeersMods.Interface;
using Assets.Scripts.Objects;
using System.Reflection;
using Assets.Scripts;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Appliances;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Objects.SpaceShuttle;
using HarmonyLib;
using Reagents;

// https://crc32.online/
// for prefab string hash
// note that prefab name crc must not overflow int32

namespace zach2039.CustomGas.Assets.Scripts
{
    public class CustomGas : ModBehaviour
    {
		// Mod reference for logs and whatnot.
		public static readonly string MODID = "CustomGasMod";

		// Reference our prefabs, scenes, and etc here
		ContentHandler modContent;

		/// <summary>
        /// Attaches prefabs once game has finished loading all resources, including the asset list in the WorldManager.
        /// </summary>
        /// <param name="contentHandler"></param>
		public override void OnLoaded(ContentHandler contentHandler)
		{
			Debug.Log("[" + CustomGas.MODID + "] Loaded with " + contentHandler.prefabs.Count.ToString());
			this.modContent = contentHandler;

			var harmony = new Harmony("org.bepinex.plugins.stationeersmods.zach2039.assets.scripts.customgas");
			harmony.PatchAll();
			Debug.Log("[" + CustomGas.MODID + "] Patched with Harmony");

			// Add required load event
			//Prefab.OnPrefabsLoaded += this.AttachPrefabs;
		}

		//public void AttachPrefabs()
		//{
		//	Debug.Log("[" + CustomGas.MODID + "] Running after prefabs");
		//
		//	Type typeFromHandle = typeof(Prefab);
		//
		//	// Locate asset lists from Prefab class
		//	Debug.Log("[" + CustomGas.MODID + "] Getting _allPrefabs from Prefab");
		//	Dictionary<int, Thing> dictionary = typeFromHandle.GetField("_allPrefabs", BindingFlags.Static | BindingFlags.Public).GetValue(null) as Dictionary<int, Thing>;
		//
		//	Debug.Log("[" + CustomGas.MODID + "] Getting PrefabsgameObject from Prefab");
		//	//GameObject gameObject = typeFromHandle.GetField("PrefabsGameObject", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as GameObject;
		//	GameObject gameObject = Prefab.PrefabsGameObject;
		//
		//	Debug.Log("[" + CustomGas.MODID + "] Injecting custom GameObjects");
		//	foreach (GameObject gameObject2 in this.modContent.prefabs)
		//	{
		//		Debug.Log(
		//			string.Concat(new string[] {"[", CustomGas.MODID, "] Found Prefab: ", gameObject2.name, " with tag: ", gameObject2.tag, " and Layer: ", gameObject2.layer.ToString(), " = ", LayerMask.LayerToName(gameObject2.layer)})
		//			);
		//		if (gameObject2.GetComponent<Thing>() != null && !gameObject2.CompareTag("NotSpawnable"))
		//		{
		//			GameObject gameObject3 = GameObject.Instantiate<GameObject>(gameObject2, gameObject.transform);
		//			Debug.Log("[" + CustomGas.MODID + "] Instanced: " + gameObject3.name);
		//			Thing thing = gameObject3.GetComponent<Thing>();
		//			if (thing != null)
		//			{
		//				Debug.Log("[" + CustomGas.MODID + "] Found Thing: " + thing.name);
		//				if (WorldManager.Instance != null)
		//				{
		//					Debug.Log("[" + CustomGas.MODID + "] Adding to WorldManager");
		//					Thing component2 = gameObject2.GetComponent<Thing>();
		//					component2.gameObject.SetActive(false);
		//					WorldManager.Instance.SourcePrefabs.Add(component2);
		//					Debug.Log("[" + CustomGas.MODID + "] Added to WorldManager");
		//				}
		//				thing.name = gameObject2.name;
		//				thing.ThingTransform.position = Vector3.zero;
		//				thing.ThingTransform.rotation = Quaternion.identity;
		//				dictionary.Add(thing.PrefabHash, thing);
		//				Debug.Log("[" + CustomGas.MODID + "] Added Thing");
		//				thing.CacheStates(false);
		//				Prefab.AllPrefabs.Add(thing);
		//				Thing.AllThings.Add(thing);
		//				thing.Lights.Clear();
		//				Light[] componentsInChildren = thing.GetComponentsInChildren<Light>(true);
		//				for (int i = 0; i < componentsInChildren.Length; i++)
		//				{
		//					Debug.Log("[" + CustomGas.MODID + "] Adding ThingLight");
		//					ThingLight thingLight = new ThingLight(componentsInChildren[i], thing);
		//					thing.Lights.Add(thingLight);
		//					thingLight.LayerMask = LightManager.Instance.WallLightsMask;
		//					if (thingLight.Light)
		//					{
		//						thingLight.Light.cullingMask = thingLight.LayerMask;
		//					}
		//				}
		//				DynamicThing dynamicThing = thing as DynamicThing;
		//				if (dynamicThing != null)
		//				{
		//					DynamicThing.DynamicThingPrefabs.Add(dynamicThing);
		//					Item item = dynamicThing as Item;
		//					if (item != null)
		//					{
		//						Ore ore = thing as Ore;
		//						if (ore)
		//						{
		//							ElectronicReader.AddToLookup(ore);
		//							Ore.AllOrePrefabs.Add(ore);
		//							Debug.Log("[" + CustomGas.MODID + "] Added as Ore");
		//							Ice ice = ore as Ice;
		//							if (ice != null)
		//							{
		//								Ice.AllIcePrefabs.Add(ice);
		//							}
		//						}
		//						Ingot ingot = thing as Ingot;
		//						if (ingot)
		//						{
		//							Ingot.AllIngotPrefabs.Add(ingot);
		//						}
		//						Cartridge cartridge = item as Cartridge;
		//						if (cartridge != null)
		//						{
		//							Cartridge.AllCartidges.Add(cartridge);
		//						}
		//
		//					}
		//				}
		//				Structure structure = thing as Structure;
		//				if (structure != null)
		//				{
		//					Structure.AllStructures.Add(structure);
		//					Device device = thing as Device;
		//					if (device)
		//					{
		//						Device.AllDevicePrefabs.Add(device);
		//						if (thing as LogicUnitBase)
		//						{
		//							LogicUnitBase.AllLogicPrefabs.Add(device);
		//						}
		//					}
		//					ModularRocket modularRocket = thing as ModularRocket;
		//					if (modularRocket != null)
		//					{
		//						ModularRocket.AllModularRocketPrefabs.Add(modularRocket);
		//					}
		//					FabricatorBase fabricatorBase = thing as FabricatorBase;
		//					if (fabricatorBase != null)
		//					{
		//						FabricatorBase.AllFabricatorPrefabs.Add(fabricatorBase);
		//					}
		//					if (thing is IAirlockDevice)
		//					{
		//						AirlockControlBase.AllAirlockEnabledPrefabs.Add(device);
		//					}
		//				}
		//				IConstructionKit constructionKit = thing as IConstructionKit;
		//				if (constructionKit != null)
		//				{
		//					ElectronicReader.AddToLookup(constructionKit);
		//				}
		//				IMicrowaveIngredient microwaveIngredient = thing as IMicrowaveIngredient;
		//				if (microwaveIngredient != null)
		//				{
		//					Microwave.AllIngredients.Add(microwaveIngredient);
		//				}
		//				IPackageableIngredient packageableIngredient = thing as IPackageableIngredient;
		//				if (packageableIngredient != null)
		//				{
		//					BasicPackagingMachine.AllIngredients.Add(packageableIngredient);
		//				}
		//				IChemistryIngredient chemistryIngredient = thing as IChemistryIngredient;
		//				if (chemistryIngredient != null)
		//				{
		//					ChemistryStation.AllIngredients.Add(chemistryIngredient);
		//				}
		//				IPaintMixerIngredient paintMixerIngredient = thing as IPaintMixerIngredient;
		//				if (paintMixerIngredient != null)
		//				{
		//					PaintMixer.AllIngredients.Add(paintMixerIngredient);
		//				}
		//			}
		//		}
		//	}
		//}

		// Start is called before the first frame update
		void Start() { }

		// Update is called once per frame
		void Update() { }
    }
}
