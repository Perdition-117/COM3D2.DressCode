using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using com.workman.cm3d2.scene.dailyEtc;
using COM3D2.I2PluginLocalization;
using HarmonyLib;
using I2.Loc;
using UnityEngine.SceneManagement;

namespace COM3D2.DressCode;

/*
 * TODO
 * 
 * Hair (?)
 * CharacterSelect thumbnail
 * 
 */

[BepInPlugin("net.perdition.com3d2.dresscode", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("net.perdition.com3d2.editbodyloadfix", BepInDependency.DependencyFlags.SoftDependency)]
public class DressCode : BaseUnityPlugin {
	internal const string ScriptTag = "dresscode";

	private static ManualLogSource _logger;
	private static Configuration _config;

	private static readonly PluginLocalization _localization = new(PluginInfo.PLUGIN_NAME);
	private static readonly Dictionary<string, List<MaidProp>> _originalCostume = new();
	private static CostumeScene _currentScene;

	private void Awake() {
		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;

		_logger = Logger;

		ConfigurationManager.Load();
		_config = ConfigurationManager.Configuration;

		Harmony.CreateAndPatchAll(typeof(DressCode));
		Harmony.CreateAndPatchAll(typeof(CostumeEdit));
	}

	internal static void LogDebug(object data) {
		_logger.LogDebug(data);
	}

	internal static string GetTermKey(string key) => _localization.GetTermKey(key);

	internal static Texture2D GetThumbnail(CostumeScene scene, Maid maid) {
		Texture2D result = null;
		var thumbnailPath = Path.Combine(Maid.ThumbnailDictionary, DressCode.GetThumbnailFileName(maid, scene));
		if (File.Exists(thumbnailPath)) {
			result = UTY.LoadTexture(thumbnailPath);
		}
		return result;
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

	private static bool TryGetEffectiveCostume(Maid maid, CostumeScene scene, out Configuration.Costume costume) {
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

	internal static void LoadCostume(Maid maid, Configuration.Costume costume, bool isEditMode = false) {
		LogDebug("Loading costume...");

		foreach (var mpn in CostumeEdit.CostumeMpn) {
			var hasItem = costume.TryGetItem(mpn, out var item) && item.FileName != string.Empty;
			//Log.LogDebug($"- {mpn,-10} {item.IsEnabled,-5} {item.FileName}");
			if (!isEditMode && hasItem && !item.IsEnabled) {
				maid.ResetProp(mpn);
			} else if (hasItem) {
				maid.SetProp(mpn, item.FileName, 0, isEditMode);
			} else {
				maid.DelProp(mpn, isEditMode);
			}
		}
	}

	private static void SetCostume(Maid maid, CostumeScene scene) {
		if (!TryGetEffectiveCostume(maid, scene, out var costume)) {
			return;
		}

		LogDebug($"Setting costume for {maid.name}");

		GameMain.instance.StartCoroutine(WaitMaidPropBusy(maid, () => {
			BackupCostume(maid);
			LoadCostume(maid, costume);
			maid.AllProcPropSeqStart();
		}));
	}

	private static void BackupCostume(Maid maid) {
		LogDebug($"Backing up costume for {maid.name}");

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

	private static void RestoreCostume(Maid maid) {
		LogDebug($"Restoring costume for {maid.name}");

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

	internal static Maid GetHeadMaid() {
		var characterMgr = GameMain.Instance.CharacterMgr;
		Maid firstMaid = null;
		Maid firstStockMaid = null;
		for (var i = 0; i < characterMgr.GetMaidCount(); i++) {
			var maid = characterMgr.GetMaid(i);
			if (maid != null) {
				firstMaid ??= maid;
				if (maid.status.leader) {
					return maid;
				}
			}
		}
		for (var i = 0; i < characterMgr.GetStockMaidCount(); i++) {
			var maid = characterMgr.GetStockMaid(i);
			if (maid != null) {
				firstStockMaid ??= maid;
				if (maid.status.leader) {
					return maid;
				}
			}
		}
		return firstMaid ?? firstStockMaid;
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
		if (sceneName == "SceneYotogi") {
			costumeScene = CostumeScene.Yotogi;
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
		var costumeScene = GetCostumeScene(scene.name);

		_currentScene = costumeScene;

		if (costumeScene == CostumeScene.None) return;

		var numMaids = GameMain.Instance.CharacterMgr.GetMaidCount();
		for (var i = 0; i < numMaids; i++) {
			var maid = GameMain.Instance.CharacterMgr.GetMaid(i);
			if (maid != null && maid.body0 && !_originalCostume.ContainsKey(maid.status.guid)) {
				SetCostume(maid, costumeScene);
			}
		}
	}

	private static void OnSceneUnloaded(Scene scene) {
		// do not load costumes when entering or leaving edit mode while in a dress code scene
		if (GameMain.Instance.GetNowSceneName() == "SceneEdit" && CostumeEdit.KeepCostume) return;
		if (scene.name == "SceneEdit" && CostumeEdit.KeepCostume) {
			CostumeEdit.KeepCostume = false;
			return;
		}

		var numMaids = GameMain.Instance.CharacterMgr.GetMaidCount();
		for (var i = 0; i < numMaids; i++) {
			var maid = GameMain.Instance.CharacterMgr.GetMaid(i);
			if (maid != null && maid.body0 && _originalCostume.ContainsKey(maid.status.guid)) {
				RestoreCostume(maid);
			}
		}
		_originalCostume.Clear();
	}

	[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.Deactivate))]
	[HarmonyPrefix]
	private static void CharacterMgr_OnDeactivate(CharacterMgr __instance, int f_nActiveSlotNo, bool f_bMan) {
		if (f_nActiveSlotNo == -1 || f_bMan || __instance.m_objActiveMaid[f_nActiveSlotNo] == null) {
			return;
		}
		var maid = __instance.m_gcActiveMaid[f_nActiveSlotNo];
		if (maid != null && _originalCostume.ContainsKey(maid.status.guid)) {
			RestoreCostume(maid);
		}
	}

	[HarmonyPatch(typeof(YotogiSubCharacterSelectManager), nameof(YotogiSubCharacterSelectManager.OnFinish))]
	[HarmonyPostfix]
	private static void YotogiSubCharacterSelectManager_OnFinish(YotogiSubCharacterSelectManager __instance) {
		if (_currentScene != CostumeScene.Yotogi) {
			return;
		}

		// secondary yotogi maids are not yet loaded on sceneLoaded, so we fix them here
		foreach (var maid in __instance.loaded_check_maid_) {
			if (maid != null && !_originalCostume.ContainsKey(maid.status.guid)) {
				SetCostume(maid, CostumeScene.Yotogi);
			}
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
				__instance.m_goPanel.gameObject.SetActive(false);
				var manager = __instance.m_mgr.GetManager<DressCodeManager>();
				manager.SetBackground();
				manager.OpenPanel();
				GameMain.Instance.MainCamera.FadeIn();
			});
		});
	}
}
