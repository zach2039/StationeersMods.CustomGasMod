using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts;
using Assets.Scripts.Atmospherics;
using Assets.Scripts.Util;
using Assets.Scripts.UI;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Assets.Scripts.Networking;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using zach2039.CustomGas.Assets.Scripts.Atmospherics;

namespace Assets.Scripts.Networking
{
	[HarmonyPatch(typeof(ServerAdmin))]
	public class ServerAdmin_Patch
	{
		private static bool parameterRequireCheck(ServerAdmin instance, List<string> commands, int Required)
		{
			if (commands.Count >= Required)
			{
				return true;
			}
			instance.LogMessege(string.Format("Invalid command :{0} command requires at least {1} parameters", commands[0], Required));
			return false;
		}

		private static bool maxParameterCheck(ServerAdmin instance, List<string> commands, int Limit)
		{
			if (commands.Count > Limit)
			{
				instance.LogMessege(string.Format("Invalid command :{0} command can not have more than {1} entries/parameters", commands[0], Limit));
				return false;
			}
			return true;
		}

		[HarmonyPatch("CommandAddTankGas", new Type[] { typeof(List<string>), typeof(ChatMessage) })]
		[HarmonyPrefix]
		public static bool CommandAddTankGas_Patch(ServerAdmin __instance, List<string> commands, ChatMessage msg)
		{
			ModGasMixture gasMixture = default(ModGasMixture);
			if (!maxParameterCheck(__instance, commands, 4))
			{
				__instance.LogMessege("Invalid parameter. Only 4 parameters are allowed.");
				return false; // skip original method
			}
			if (!ServerAdmin_Patch.parameterRequireCheck(__instance, commands, 4))
			{
				__instance.LogMessege("Must have at least 4 parameters Command,RefID,GasName,moles.");
				return false; // skip original method
			}
			long refID;
			long.TryParse(commands[1], out refID);
			Thing thing = __instance.ServerGetThing(refID);
			if (thing.InternalAtmosphere != null || thing is Pipe)
			{
				string gasName = commands[2];
				float moles;
				float.TryParse(commands[3], out moles);
				gasMixture = new ModGasMixture();
				gasMixture.GetGasByName(gasName).Quantity = moles;
				__instance.LogMessege(string.Format("Added {0} moles of {1} to {2}", moles, gasName, thing.PrefabName));
				// TODO: Allow burnmix to be spawned as well, not just single gases
				//case 7:
				//	{
				//		float volatiles = 0.66f * moles;
				//		float oxygen = 0.34f * moles;
				//		gasMixture = new GasMixture(oxygen, 0f, 0f, volatiles, 0f, 0f, 0f);
				//		this.LogMessege(string.Format("Added {0} moles of Fuel to {1}", moles, thing.PrefabName));
				//		break;
				//	}
				//case 8:
				//	{
				//		float volatiles = 0.66f * moles;
				//		float oxygen = 0.33f * moles;
				//		gasMixture = new ModGasMixture(oxygen, 0f, 0f, volatiles, 0f, 0f, 0f);
				//		__instance.LogMessege(string.Format("Added {0} moles of Rocket Fuel to {1}", moles, thing.PrefabName));
				//		break;
				//	}
				//default:
				gasMixture.TotalEnergy = gasMixture.HeatCapacity * Chemistry.Temperature.TwentyDegrees;
				Pipe pipe = thing as Pipe;
				if (pipe != null && pipe.PipeNetwork != null)
				{
					AtmosphereExtension.Add(pipe.PipeNetwork.Atmosphere, gasMixture);
				}
				else
				{
					AtmosphereExtension.Add(thing.InternalAtmosphere, gasMixture);
				}
			}
			else
			{
				__instance.LogMessege("No internal Atmosphere exsits");
			}
			AddGasCommand addGasCommand = new AddGasCommand();
			addGasCommand.target = refID;
			addGasCommand.oxygen = -1f;
			addGasCommand.carbonDioxide = -1f;
			addGasCommand.nitrogen = -1f;
			addGasCommand.nitrousOxide = -1f;
			addGasCommand.pollutant = -1f;
			addGasCommand.water = -1f;
			addGasCommand.volatiles = -1f;
			NetworkServer.SendToClients<ChatMessage>(msg, NetworkChannel.GeneralTraffic, -1);
			return false; // skip original method
		}
	}
}