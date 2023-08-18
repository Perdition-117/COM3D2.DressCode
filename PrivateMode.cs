using System.Collections;
using HarmonyLib;
using PrivateMaidMode;

namespace COM3D2.DressCode;

public partial class DressCode {
	private static bool _isLoadingPrivateModeMaid = false;

	// loading private maid in office scene
	[HarmonyPatch(typeof(PrivateModeMgr), nameof(PrivateModeMgr.LoadPrivateMaid))]
	[HarmonyPrefix]
	private static void PrivateModeMgr_PreLoadPrivateMaid() {
		_isLoadingPrivateModeMaid = true;
	}

	[HarmonyPatch(typeof(PrivateModeMgr), nameof(PrivateModeMgr.LoadPrivateMaid))]
	[HarmonyPostfix]
	private static void PrivateModeMgr_PostLoadPrivateMaid() {
		_isLoadingPrivateModeMaid = false;
	}

	// selecting private maid
	[HarmonyPatch(typeof(PrivateCharaSelectMain), nameof(PrivateCharaSelectMain.SelectCharacter))]
	[HarmonyPrefix]
	private static void PrivateCharaSelectMain_PreSelectCharacter() {
		_isLoadingPrivateModeMaid = true;
	}

	[HarmonyPatch(typeof(PrivateCharaSelectMain), nameof(PrivateCharaSelectMain.SelectCharacter))]
	[HarmonyPostfix]
	private static void PrivateCharaSelectMain_PostSelectCharacter() {
		_isLoadingPrivateModeMaid = false;
	}

	// loading private mode event
	[HarmonyPatch(typeof(PrivateEventManager), nameof(PrivateEventManager.SetupPrivateMode))]
	[HarmonyPrefix]
	private static void PrivateEventManager_PreSetupPrivateMode() {
		_isLoadingPrivateModeMaid = true;
	}

	[HarmonyPatch(typeof(PrivateEventManager), nameof(PrivateEventManager.SetupPrivateMode))]
	[HarmonyPostfix]
	private static IEnumerator PrivateEventManager_PostSetupPrivateMode(IEnumerator __result) {
		while (__result.MoveNext()) yield return __result.Current;
		_isLoadingPrivateModeMaid = false;
	}
}
