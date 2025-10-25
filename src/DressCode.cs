using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using com.workman.cm3d2.scene.dailyEtc;
using COM3D2.I2PluginLocalization;
using HarmonyLib;
using Honeymoon;
using I2.Loc;
using MaidCafe;
using PrivateMaidMode;
using UnityEngine.SceneManagement;

namespace DressCode;

// TODO CharacterSelect thumbnail

[BepInPlugin("net.perdition.com3d2.dresscode", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("net.perdition.com3d2.editbodyloadfix", BepInDependency.DependencyFlags.SoftDependency)]
public partial class DressCode : BaseUnityPlugin {
	internal const string ScriptTag = "dresscode";
	internal const string ReopenPanelTag = "dresscodereopen";

	internal static readonly Version GameVersion = new(GameUty.GetBuildVersionText());

	private static ManualLogSource _logger;
	private static Configuration _config;

	private static readonly PluginLocalization _localization = new(PluginInfo.PLUGIN_NAME);
	private static readonly Dictionary<string, List<MaidProp>> _originalCostume = new();
	private static readonly Dictionary<string, Configuration.Costume> _temporaryCostume = new();
	private static string _currentSceneName = string.Empty;
	private static CostumeScene _activeCostumeScene;
	private static bool _isReloadingCostume = false;

	private static readonly CostumeScene[] TemporaryCostumeScenes = {
		CostumeScene.PrivateMode,
		CostumeScene.Honeymoon,
	};

	private static readonly CostumeScene[] PersistentCostumeScenes = {
		CostumeScene.PrivateMode,
		CostumeScene.Honeymoon,
		CostumeScene.NightPool,
	};

	internal static bool ReopenPanel { get; set; } = false;

	private void Awake() {
		SceneManager.sceneLoaded += OnSceneLoaded;

		_logger = Logger;

		ConfigurationManager.Load();
		_config = ConfigurationManager.Configuration;

		Harmony.CreateAndPatchAll(typeof(DressCode));
		Harmony.CreateAndPatchAll(typeof(CostumeEdit));
		Harmony.CreateAndPatchAll(typeof(CharacterSelectThumbnail));
		Harmony.CreateAndPatchAll(GameVersion.Major >= 3 ? typeof(CostumeEdit.PatchMethods30) : typeof(CostumeEdit.PatchMethods20));
	}

	internal static void LogDebug(object data) {
		_logger.LogDebug(data);
	}

	internal static string GetTermKey(string key) => _localization.GetTermKey(key);

	internal static Texture2D GetThumbnail(CostumeScene scene, Maid maid) {
		Texture2D result = null;
		var thumbnailPath = GetThumbnailPath(maid, scene);
		if (File.Exists(thumbnailPath)) {
			result = UTY.LoadTexture(thumbnailPath);
		}
		return result;
	}

	internal static string GetThumbnailPath(Maid maid, CostumeScene scene) {
		return Path.Combine(Maid.ThumbnailDictionary, GetThumbnailFileName(maid, scene));
	}

	internal static string GetThumbnailFileName(Maid maid, CostumeScene scene) {
		return $"dresscode_{maid?.status.guid ?? "scene"}_{scene.ToString().ToLower()}.png";
	}

	internal static bool TryGetMaidProfile(Maid maid, CostumeScene scene, out Configuration.SceneProfile profile) {
		return _config.TryGetMaidProfile(maid, scene, out profile);
	}

	internal static bool TryGetSceneProfile(CostumeScene scene, out Configuration.SceneProfile profile) {
		return _config.TryGetSceneProfile(scene, out profile);
	}

	internal static bool HasEffectiveCostume(Maid maid, CostumeScene scene) {
		return TryGetEffectiveCostume(maid, scene, out _);
	}

	internal static bool TryGetEffectiveCostume(Maid maid, CostumeScene scene, out Configuration.Costume costume) {
		var hasMaidProfile = _config.TryGetMaidProfile(maid, scene, out var maidProfile);
		var hasSceneProfile = _config.TryGetSceneProfile(scene, out var sceneProfile);

		if (hasMaidProfile) {
			if (maidProfile.PreferredProfile == CostumeProfile.Personal && maidProfile.HasCostume) {
				costume = maidProfile.Costume;
				return true;
			}
			if (maidProfile.PreferredProfile == CostumeProfile.Scene && hasSceneProfile && sceneProfile.HasCostume) {
				costume = sceneProfile.Costume;
				return true;
			}
		} else if (hasSceneProfile) {
			if (sceneProfile.PreferredProfile == CostumeProfile.Scene && sceneProfile.HasCostume) {
				costume = sceneProfile.Costume;
				return true;
			}
		}

		costume = null;
		return false;
	}

	internal static CostumeProfile GetPreferredProfile(Maid maid, CostumeScene scene) {
		if (TryGetMaidProfile(maid, scene, out var profile)) {
			return profile.PreferredProfile;
		} else {
			return GetPreferredProfile(scene);
		}
	}

	internal static CostumeProfile GetPreferredProfile(CostumeScene scene) {
		if (TryGetSceneProfile(scene, out var profile)) {
			return profile.PreferredProfile;
		} else {
			return CostumeProfile.Default;
		}
	}

	internal static void LoadCostume(Maid maid, Configuration.Costume costume, bool isEditMode = false, bool isTemporary = false) {
		LogDebug($"Loading {(isTemporary ? "temporary " : "")}costume for {maid.name}...");

		foreach (var mpn in CostumeEdit.CostumeMpn) {
			var hasItem = costume.TryGetItem(mpn, out var item) && item.FileName != string.Empty;
			//Log.LogDebug($"- {mpn,-10} {item.IsEnabled,-5} {item.FileName}");
			if (!isEditMode && hasItem && !item.IsEnabled) {
				maid.ResetProp(mpn);
			} else if (hasItem) {
				maid.SetProp(mpn, item.FileName, 0, isEditMode || isTemporary);
			} else {
				maid.DelProp(mpn, isEditMode || isTemporary);
			}
		}
	}

	internal static void SetCostume(Maid maid, CostumeScene scene, bool isTemporary = false) {
		if (!isTemporary && _originalCostume.ContainsKey(maid.status.guid)) {
			return;
		}

		if (!TryGetEffectiveCostume(maid, scene, out var costume)) {
			return;
		}

		GameMain.instance.StartCoroutine(WaitMaidPropBusy(maid, () => {
			if (isTemporary) {
				SetTemporaryCostume(maid, costume);
			} else {
				BackupCostume(maid);
			}
			LoadCostume(maid, costume, false, isTemporary);
			maid.AllProcPropSeqStart();
		}));
	}

	internal static void SetTemporaryCostume(Maid maid, Configuration.Costume costume) {
		_temporaryCostume.Add(maid.status.guid, costume);
	}

	/// <summary>
	/// Stores a maid's current persistent costume.
	/// </summary>
	private static void BackupCostume(Maid maid) {
		LogDebug($"Backing up costume for {maid.name}...");

		var costume = new List<MaidProp>();
		foreach (var maidProp in maid.m_aryMaidProp) {
			var originalMaidProp = new MaidProp {
				idx = maidProp.idx,
				strFileName = maidProp.strFileName,
				nFileNameRID = maidProp.nFileNameRID,
				m_dicTBodySkinPos = new(maidProp.m_dicTBodySkinPos),
				m_dicTBodyAttachPos = new(maidProp.m_dicTBodyAttachPos),
				m_dicMaterialProp = new(maidProp.m_dicMaterialProp),
				m_dicBoneLength = new(maidProp.m_dicBoneLength),
			};
			if (maidProp.listSubProp != null) {
				originalMaidProp.listSubProp = new(maidProp.listSubProp);
			}
			costume.Add(originalMaidProp);
		}
		_originalCostume.Add(maid.status.guid, costume);
	}

	internal static void ResetCostume(Maid maid) {
		if (_originalCostume.ContainsKey(maid.status.guid)) {
			RestoreCostume(maid);
		}
		if (_temporaryCostume.ContainsKey(maid.status.guid)) {
			RemoveTemporaryCostume(maid);
		}
	}

	/// <summary>
	/// Restores a maid's persistent costume previously backed up using <see cref="BackupCostume"/>.
	/// </summary>
	private static void RestoreCostume(Maid maid) {
		LogDebug($"Restoring costume for {maid.name}...");

		var costume = _originalCostume[maid.status.guid];
		foreach (var originalMaidProp in costume) {
			maid.SetProp((MPN)originalMaidProp.idx, originalMaidProp.strFileName, originalMaidProp.nFileNameRID);
			var maidProp = maid.GetProp((MPN)originalMaidProp.idx);
			maidProp.m_dicTBodySkinPos = originalMaidProp.m_dicTBodySkinPos;
			maidProp.m_dicTBodyAttachPos = originalMaidProp.m_dicTBodyAttachPos;
			maidProp.m_dicMaterialProp = originalMaidProp.m_dicMaterialProp;
			maidProp.m_dicBoneLength = originalMaidProp.m_dicBoneLength;
			if (originalMaidProp.listSubProp != null && originalMaidProp.listSubProp.Count > 0) {
				maid.DelProp((MPN)originalMaidProp.idx);
				for (int i = 0; i < originalMaidProp.listSubProp.Count; i++) {
					var originalSubProp = originalMaidProp.listSubProp[i];
					if (originalSubProp != null) {
						maid.SetSubProp((MPN)originalMaidProp.idx, i, originalSubProp.strFileName, originalSubProp.nFileNameRID);
						var subProp = maid.GetSubProp((MPN)originalMaidProp.idx, i);
						subProp.fTexMulAlpha = originalSubProp.fTexMulAlpha;
					}
				}
			}
		}
		_originalCostume.Remove(maid.status.guid);
	}

	private static void RemoveTemporaryCostume(Maid maid) {
		LogDebug($"Removing temporary costume for {maid.name}...");

		foreach (var mpn in CostumeEdit.CostumeMpn) {
			var prop = maid.GetProp(mpn);
			maid.ResetProp(prop);
		}
		_temporaryCostume.Remove(maid.status.guid);
	}

	internal static Maid GetHeadMaid() {
		Maid firstMaid = null;
		Maid firstStockMaid = null;
		foreach (var maid in GetMaids()) {
			firstMaid ??= maid;
			if (maid.status.leader) {
				return maid;
			}
		}
		foreach (var maid in GetStockMaids()) {
			firstStockMaid ??= maid;
			if (maid.status.leader) {
				return maid;
			}
		}
		return firstMaid ?? firstStockMaid;
	}

	internal static IEnumerable<Maid> GetMaids() {
		var characterMgr = GameMain.Instance.CharacterMgr;
		for (var i = 0; i < characterMgr.GetMaidCount(); i++) {
			var maid = characterMgr.GetMaid(i);
			if (maid != null) {
				yield return maid;
			}
		}
	}

	internal static IEnumerable<Maid> GetStockMaids() {
		var characterMgr = GameMain.Instance.CharacterMgr;
		for (var i = 0; i < characterMgr.GetStockMaidCount(); i++) {
			var maid = characterMgr.GetStockMaid(i);
			if (maid != null) {
				yield return maid;
			}
		}
	}

	internal static IEnumerator WaitMaidPropBusy(Maid maid, Action callback) {
		while (maid.IsBusy) {
			yield return null;
		}
		callback?.Invoke();
		yield break;
	}

	private static CostumeScene GetCostumeScene(string sceneName) {
		var costumeScene = CostumeScene.None;
		if (sceneName == SceneName.Yotogi) {
			costumeScene = CostumeScene.Yotogi;
		} else if (sceneName == SceneName.PrivateMode || sceneName == SceneName.PrivateModeEvent) {
			costumeScene = CostumeScene.PrivateMode;
		} else if (sceneName == SceneName.Honeymoon) {
			costumeScene = CostumeScene.Honeymoon;
		} else if (sceneName == SceneName.NightPool) {
			costumeScene = CostumeScene.NightPool;
		} else if (sceneName.StartsWith("SceneDance") && DanceMain.SelectDanceData != null) {
			if (DanceMain.SelectDanceData.RhythmGameCorrespond) {
				costumeScene = CostumeScene.Dance;
			} else {
				costumeScene = CostumeScene.PoleDance;
			}
		}
		return costumeScene;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (MaidCafeManager.isMaidCafeMode) return;

		var prevSceneName = _currentSceneName;
		var nextSceneName = scene.name;
		_currentSceneName = nextSceneName;
		var nextCostumeScene = GetCostumeScene(nextSceneName);

		// prevent resetting costumes when entering edit mode while in a dress code scene
		if (nextSceneName == SceneName.Edit && (CostumeEdit.KeepCostume || PrivateModeMgr.Instance.PrivateMaid)) return;
		CostumeEdit.KeepCostume = false;

		// reload private mode costume between private mode events
		if (_activeCostumeScene == CostumeScene.PrivateMode && nextSceneName == SceneName.PrivateModeEvent) {
			var maid = PrivateModeMgr.Instance.PrivateMaid;
			if (!maid.IsCrcBody && TryGetEffectiveCostume(maid, CostumeScene.PrivateMode, out var costume)) {
				LogDebug("Reloading private mode costume...");
				_isReloadingCostume = true;
				LoadCostume(maid, costume, false, true);
				_isReloadingCostume = false;
			}
			return;
		}

		// keep night pool costume for night pool yotogi
		if (_activeCostumeScene == CostumeScene.NightPool && nextCostumeScene == CostumeScene.Yotogi) {
			return;
		}

		if (_activeCostumeScene == CostumeScene.Honeymoon) {
			var maid = HoneymoonManager.Instance.targetMaid;

			if (maid.IsCrcBody) return;

			// reload honeymoon costume between honeymoon events
			if (nextSceneName == SceneName.Honeymoon) {
				if (TryGetEffectiveCostume(maid, CostumeScene.Honeymoon, out var costume)) {
					LogDebug("Reloading honeymoon costume...");
					if (_temporaryCostume.ContainsKey(maid.status.guid)) {
						_isReloadingCostume = true;
						LoadCostume(maid, costume, false, true);
						_isReloadingCostume = false;
					} else {
						maid.AllProcPropSeqStart();
						maid.Visible = true;
						SetCostume(maid, CostumeScene.Honeymoon, true);
					}
				}
				return;
			}

			// set permanent honeymoon costume for honeymoon yotogi
			if (nextCostumeScene == CostumeScene.Yotogi) {
				LogDebug("Honeymoon yotogi started.");
				if (HasEffectiveCostume(maid, CostumeScene.Honeymoon)) {
					RemoveTemporaryCostume(maid);
					SetCostume(maid, CostumeScene.Honeymoon);
				}
				return;
			}

			// restore permanent costume after honeymoon yotogi
			if (GetCostumeScene(prevSceneName) == CostumeScene.Yotogi) {
				LogDebug("Honeymoon yotogi ended.");
				if (_originalCostume.ContainsKey(maid.status.guid)) {
					RestoreCostume(maid);
				}
				return;
			}
		}

		if (PersistentCostumeScenes.Contains(_activeCostumeScene)) {
			// keep persistent costumes during intermediate scenes
			if (nextSceneName == SceneName.Adv || nextSceneName == SceneName.Eyecatch) {
				return;
			} else if (nextCostumeScene != _activeCostumeScene) {
				// end persistent scene if not loading the scene itself or an intermediate scene
				LogDebug($"{_activeCostumeScene} persistent scene ended.");
				_activeCostumeScene = CostumeScene.None;
			}
		}

		// reset costumes if no scene is active
		if (_activeCostumeScene == CostumeScene.None) {
			foreach (var maid in GetMaids().Where(e => e.body0 && !e.IsCrcBody)) {
				ResetCostume(maid);
			}
			_originalCostume.Clear();
			_temporaryCostume.Clear();
		}

		if (nextCostumeScene != _activeCostumeScene) {
			_activeCostumeScene = nextCostumeScene;
			if (PersistentCostumeScenes.Contains(_activeCostumeScene)) {
				LogDebug($"{_activeCostumeScene} persistent scene started.");
			}
		}

		if (nextCostumeScene != CostumeScene.None) {
			foreach (var maid in GetMaids().Where(e => e.body0 && !e.IsCrcBody)) {
				// allow honeymoon costume to load properly on startup
				if (nextCostumeScene == CostumeScene.Honeymoon) {
					maid.Visible = true;
				}
				SetCostume(maid, nextCostumeScene, TemporaryCostumeScenes.Contains(nextCostumeScene));
			}
		}
	}

	[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.Deactivate))]
	[HarmonyPrefix]
	private static void CharacterMgr_OnDeactivate(CharacterMgr __instance, int f_nActiveSlotNo, bool f_bMan) {
		if (f_nActiveSlotNo == -1 || f_bMan || __instance.m_objActiveMaid[f_nActiveSlotNo] == null) {
			return;
		}
		var maid = __instance.m_gcActiveMaid[f_nActiveSlotNo];
		if (maid != null && !maid.IsCrcBody) {
			ResetCostume(maid);
		}
	}

	// use character selection menu as trigger for certain scenes
	[HarmonyPatch(typeof(CharacterSelectMain), nameof(CharacterSelectMain.OnFinish))]
	[HarmonyPostfix]
	private static void CharacterSelectMain_OnFinish(CharacterSelectMain __instance) {
		if (__instance.select_maid_ == null || __instance.select_maid_.IsCrcBody) {
			return;
		}

		switch (__instance.scene_chara_select_.select_type) {
			case SceneCharacterSelect.SelectType.Yotogi:
				_activeCostumeScene = CostumeScene.YotogiTalk;
				LogDebug($"{_activeCostumeScene} scene started.");
				SetCostume(__instance.select_maid_, CostumeScene.YotogiTalk, true);
				break;
			case SceneCharacterSelect.SelectType.HoneymoonMode:
				if (_activeCostumeScene != CostumeScene.Honeymoon) {
					_activeCostumeScene = CostumeScene.Honeymoon;
					LogDebug($"{_activeCostumeScene} scene started.");
					foreach (var mpn in CostumeEdit.CostumeMpn) {
						var prop = __instance.select_maid_.GetProp(mpn);
						__instance.select_maid_.ResetProp(prop);
					}
					SetCostume(__instance.select_maid_, CostumeScene.Honeymoon, true);
				}
				break;
		}
	}

	[HarmonyPatch(typeof(YotogiSubCharacterSelectManager), nameof(YotogiSubCharacterSelectManager.OnFinish))]
	[HarmonyPostfix]
	private static void YotogiSubCharacterSelectManager_OnFinish(YotogiSubCharacterSelectManager __instance) {
		if (_activeCostumeScene != CostumeScene.Yotogi) {
			return;
		}

		// secondary yotogi maids are not yet loaded on sceneLoaded, so we fix them here
		foreach (var maid in __instance.loaded_check_maid_) {
			if (maid != null && !maid.IsCrcBody) {
				SetCostume(maid, CostumeScene.Yotogi);
			}
		}
	}

	[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.SetActiveMaid))]
	[HarmonyPostfix]
	private static void CharacterMgr_SetActiveMaid(Maid f_maid) {
		if (!f_maid.IsCrcBody && _isLoadingPrivateModeMaid && !_temporaryCostume.ContainsKey(f_maid.status.guid)) {
			foreach (var mpn in CostumeEdit.CostumeMpn) {
				var prop = f_maid.GetProp(mpn);
				f_maid.ResetProp(prop);
			}
			f_maid.Visible = true;
			SetCostume(f_maid, CostumeScene.PrivateMode, true);
		}
	}

	// do not reset items during or between private mode or honeymoon events
	[HarmonyPatch(typeof(Maid), nameof(Maid.ResetProp), typeof(MaidProp), typeof(bool))]
	[HarmonyPrefix]
	private static bool Maid_ResetProp(Maid __instance, MaidProp mp) {
		if (!__instance.IsCrcBody && (_activeCostumeScene == CostumeScene.PrivateMode || _activeCostumeScene == CostumeScene.Honeymoon) && !_isReloadingCostume && _temporaryCostume.TryGetValue(__instance.status.guid, out var costume)) {
			var mpn = (MPN)mp.idx;
			var hasItem = costume.TryGetItem(mpn, out var item) && item.FileName != string.Empty;
			if (hasItem) {
				if (!item.IsEnabled) {
					// allow resetting the item if the costume slot is not enabled
					return true;
				} else if (item.FileName != mp.strTempFileName) {
					// revert temporary script item to costume item
					__instance.SetProp(mpn, item.FileName, 0, true);
				}
			} else {
				// remove temporary script item
				__instance.DelProp(mpn, true);
			}
			return false;
		}
		return true;
	}

	// hack to prevent error after certain scenes (private mode event)
	[HarmonyPatch(typeof(TBodySkin), nameof(TBodySkin.CoReFixBlendValues))]
	[HarmonyPostfix]
	private static IEnumerator TBodySkin_CoReFixBlendValues(IEnumerator __result, TBodySkin __instance) {
		while (__result.MoveNext()) {
			yield return __result.Current;
			if (__instance.morph == null) yield break;
		}
	}

	[HarmonyPatch(typeof(SceneMgr), nameof(SceneMgr.Start))]
	[HarmonyPostfix]
	private static void SceneMgr_OnStart() {
		var manager = GameObject.Find("/UI Root/Manager");
		NGUITools.AddChild<DressCodeManager>(manager);
	}

	[HarmonyPatch(typeof(DailyCtrl), nameof(DailyCtrl.Init))]
	[HarmonyPostfix]
	private static void DailyCtrl_OnInit(DailyCtrl __instance) {
		// do not create button in CM3D2 management as return scripts do not work
		if (DailyMgr.IsLegacy) return;

		var panel = GameObject.Find("/UI Root/DailyPanel");
		var menu = panel.transform.Find("OfficeMenu")?.gameObject;
		if (menu == null) {
			menu = NGUITools.AddChild(panel);
			menu.name = "OfficeMenu";
			menu.transform.localPosition = new(933, 439);

			var table = menu.AddComponent<UITable>();
			table.pivot = UIWidget.Pivot.TopRight;
			table.padding = new(0, 2);
		}

		var button = NGUITools.AddChild(menu, GameObject.Find("/UI Root/DailyPanel/Horizon1/Credit"));
		button.name = "DressCode";
		button.GetComponent<Localize>().enabled = false;
		button.GetComponent<UIButton>().normalSprite = "main_buttom";
		var value = button.transform.Find("Value");
		value.GetComponent<Localize>().enabled = false;
		value.GetComponent<UILabel>().text = "DressCode";
		EventDelegate.Add(button.GetComponent<UIButton>().onClick, () => {
			GameMain.Instance.MainCamera.FadeOut(0.5f, false, () => {
				Open(__instance.m_mgr);
				GameMain.Instance.MainCamera.FadeIn();
			});
		});
	}

	[HarmonyPatch(typeof(BasePanelMgr), nameof(BasePanelMgr.BeforeFadeIn))]
	[HarmonyPrefix]
	private static void BasePanelMgr_BeforeFadeIn(BasePanelMgr __instance) {
		if (__instance is DailyMgr dailyMgr && ReopenPanel) {
			ReopenPanel = false;
			Open(dailyMgr, true);
		}
	}

	private static void Open(DailyMgr dailyMgr, bool retainSelections = false) {
		var maid = PrivateModeMgr.Instance.PrivateMaid;
		if (maid != null) {
			maid.Visible = false;
		}
		dailyMgr.m_ctrl.m_goPanel.gameObject.SetActive(false);
		var manager = dailyMgr.GetManager<DressCodeManager>();
		manager.SetBackground();
		if (!retainSelections) {
			manager.ResetSelections();
		}
		manager.OpenPanel();
	}
}
