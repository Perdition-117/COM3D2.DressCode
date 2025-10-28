using System.Collections;
using System.Reflection.Emit;
using com.workman.cm3d2.scene.dailyEtc;
using HarmonyLib;
using Honeymoon;

namespace DressCode;

internal class CostumeEdit {
	private static CostumeScene _currentScene;
	private static CostumeProfile _currentProfile;
	private static string _currentScript;
	private static DailyMgr.Daily _currentDayTime;

	private static readonly Dictionary<MPN, bool> _enabledMpn = new();

	private static bool _isDressCode = false;

	// MPNs to be saved and loaded for costumes
	internal static readonly HashSet<MPN> CostumeMpn = new() {
		//Mpn.hairf,
		//Mpn.hairr,
		//Mpn.hairt,
		//Mpn.hairs,
		//Mpn.hairaho,

		Mpn.wear,
		Mpn.skirt,
		Mpn.mizugi,
		Mpn.bra,
		Mpn.panz,
		Mpn.stkg,
		Mpn.shoes,
		Mpn.onepiece,

		Mpn.headset,
		Mpn.glove,
		Mpn.acchead,
		Mpn.acchana,
		Mpn.acckamisub,
		Mpn.acckami,
		Mpn.accmimi,
		Mpn.accnip,
		Mpn.acckubi,
		Mpn.acckubiwa,
		Mpn.accheso,
		Mpn.accude,
		Mpn.accashi,
		Mpn.accsenaka,
		Mpn.accshippo,
		Mpn.accanl,
		Mpn.accvag,
		Mpn.megane,
		Mpn.accxxx,
		Mpn.handitem,
		Mpn.acchat,
	};

	// additional MPNs to be enabled in edit mode
	internal static readonly HashSet<MPN> EnabledMpn = new(CostumeMpn.Concat(new[] {
		Mpn.set_maidwear,
		Mpn.set_mywear,
		Mpn.set_underwear,
	}));

	private static readonly Dictionary<DailyMgr.Daily, string> DayTimeLabels = new() {
		[DailyMgr.Daily.Daytime] = "*昼メニュー",
		[DailyMgr.Daily.Night] = "*夜メニュー",
	};

	internal static bool KeepCostume { get; set; } = false;

	public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible {
		var retValue = value != null && Enum.IsDefined(typeof(TEnum), value);
		result = retValue ? (TEnum)Enum.Parse(typeof(TEnum), value) : default;
		return retValue;
	}

	internal static void StartCostumeEdit(Maid maid, CostumeScene scene) {
		StartCostumeEdit(maid, scene, CostumeProfile.Personal);
	}

	internal static void StartCostumeEdit(CostumeScene scene) {
		StartCostumeEdit(DressCode.GetHeadMaid(), scene, CostumeProfile.Scene);
	}

	private static void StartCostumeEdit(Maid maid, CostumeScene scene, CostumeProfile profile) {
		GameMain.Instance.MainCamera.FadeOut(0.5f, false, () => {
			GameMain.Instance.CharacterMgr.SetActiveMaid(maid, 0);
			maid.Visible = true;
			maid.AllProcPropSeqStart();
			GameMain.instance.StartCoroutine(DressCode.WaitMaidPropBusy(maid, () => {
				StartCostumeEdit(maid, scene, profile, DayTimeLabels[_currentDayTime], true);
			}));
		});
	}

	private static bool StartCostumeEdit(Maid maid, CostumeScene scene, string scriptFile, string label) {
		var profile = DressCode.GetPreferredProfile(maid, scene);
		if (profile == CostumeProfile.Default) {
			return true;
		}
		_currentScript = scriptFile;
		GameMain.Instance.MainCamera.FadeOut(0.5f, false, () => {
			KeepCostume = true;
			StartCostumeEdit(maid, scene, profile, label);
		});

		return false;
	}

	private static void StartCostumeEdit(Maid maid, CostumeScene scene, CostumeProfile profile, string label, bool reopenPanel = false) {
		_currentScene = scene;
		DressCode.LogDebug($"Starting scene edit for {scene} ({profile})...");
		var costume = CreateCostume(maid, scene, profile);
		DressCode.LoadCostume(maid, costume, true);
		maid.AllProcPropSeqStart();
		GameMain.instance.StartCoroutine(DressCode.WaitMaidPropBusy(maid, () => {
			var scriptMgr = GameMain.Instance.ScriptMgr;
			scriptMgr.adv_kag.kag.LoadScenarioString($"@SceneCall name=SceneEdit label={label} {DressCode.ScriptTag}={profile} facility_costume {(reopenPanel ? DressCode.ReopenPanelTag : string.Empty)}");
			scriptMgr.adv_kag.kag.Exec();
		}));
	}

	private static Configuration.Costume CreateCostume(Maid maid, CostumeScene scene, CostumeProfile requestedProfile) {
		Configuration.Costume newCostume;
		if (requestedProfile == CostumeProfile.Personal && DressCode.TryGetMaidProfile(maid, scene, out var maidProfile) && maidProfile.HasCostume) {
			DressCode.LogDebug("Creating costume from maid profile...");
			newCostume = CloneCostume(maidProfile.Costume);
		} else if (requestedProfile == CostumeProfile.Scene && DressCode.TryGetSceneProfile(scene, out var sceneProfile) && sceneProfile.HasCostume) {
			DressCode.LogDebug("Creating costume from scene profile...");
			newCostume = CloneCostume(sceneProfile.Costume);
		} else {
			DressCode.LogDebug("Creating costume from maid props...");
			newCostume = CloneCostume(maid);
		}
		_enabledMpn.Clear();
		foreach (var item in newCostume.Items) {
			_enabledMpn[item.Slot] = item.IsEnabled;
		}
		return newCostume;
	}

	private static Configuration.Costume CloneCostume(Configuration.Costume costume) {
		DressCode.LogDebug($"CloneCostume");
		var newCostume = new Configuration.Costume();
		foreach (var item in costume.Items) {
			newCostume.AddItem(item.Slot, item.FileName, item.IsEnabled);
		}
		return newCostume;
	}

	private static Configuration.Costume CloneCostume(Maid maid) {
		var newCostume = new Configuration.Costume();
		foreach (var mpn in CostumeMpn) {
			var prop = maid.GetProp(mpn);
			newCostume.AddItem((MPN)prop.idx, prop.strFileName);
		}
		return newCostume;
	}

	private static void SaveCostume(Maid maid, CostumeScene scene) {
		if (scene == CostumeScene.None) {
			throw new("Invalid CostumeScene");
		}

		var newCostume = new Configuration.Costume();

		DressCode.LogDebug($"Saving {scene} costume...");
		foreach (var mpn in CostumeMpn) {
			var prop = maid.GetProp(mpn);
			var isEnabled = !_enabledMpn.TryGetValue(mpn, out var isForcedExplicit) || isForcedExplicit;
			if (prop.strTempFileName == string.Empty || (IsDeleteItem(mpn, prop.strTempFileName) && isEnabled)) {
				continue;
			}
			DressCode.LogDebug($"- {mpn,-10} {isEnabled,-5} {prop.strTempFileName}");
			newCostume.AddItem(mpn, prop.strTempFileName, isEnabled);
		}

		ConfigurationManager.Configuration.SaveCostume(maid, scene, newCostume, _currentProfile);

		foreach (var mpn in CostumeMpn) {
			maid.ResetProp(mpn, true);
		}
		maid.AllProcPropSeqStart();
		maid.Visible = false;
	}

	private static bool IsDeleteItem(MPN mpn, string fileName) {
		return CM3.dicDelItem.TryGetValue(mpn, out var deleteItem) && deleteItem.ToLower() == fileName.ToLower();
	}

	[HarmonyPatch(typeof(SceneMgr), nameof(SceneMgr.Start))]
	[HarmonyPostfix]
	private static void SceneMgr_OnStart(SceneMgr __instance) {
		var tagBackup = GameMain.Instance.ScriptMgr.adv_kag.tag_backup;
		if (tagBackup.TryGetValue("name", out var sceneName) && sceneName == "SceneDaily") {
			if (tagBackup.TryGetValue("type", out var openType) && TryParse(openType, out _currentDayTime)) {
				// store current script in order to return from edit mode, since LoadScenario overrides script name
				_currentScript = GameMain.Instance.ScriptMgr.adv_kag.kag.GetCurrentFileName();
			}
		}
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.Awake))]
	[HarmonyPrefix]
	private static bool SceneEdit_OnAwake(SceneEdit __instance) {
		var tagBackup = GameMain.Instance.ScriptMgr.adv_kag.tag_backup;

		_isDressCode = tagBackup.TryGetValue(DressCode.ScriptTag, out var costumeProfile);

		if (!_isDressCode) return true;

		TryParse(costumeProfile, out _currentProfile);

		SceneEdit.Instance = __instance;
		// set CostumeEdit mode to make prop changes temporary
		__instance.modeType = SceneEdit.ModeType.CostumeEdit;
		__instance.m_cameraMoveSupport = __instance.gameObject.AddComponent<WfCameraMoveSupport>();
		__instance.editItemTextureCache = __instance.gameObject.AddComponent<EditItemTextureCache>();
		__instance.enabledMpns = EnabledMpn;

		if (tagBackup != null && tagBackup.TryGetValue("name", out var sceneName) && sceneName == "SceneEdit") {
			if (tagBackup.TryGetValue("label", out var strScriptArg)) {
				__instance.m_strScriptArg = strScriptArg;
			}
			if (tagBackup.ContainsKey("vr_mode")) {
				__instance.m_bVRComMode = true;
			}
			__instance.m_currentScriptFile = _currentScript;
		}

		__instance.EditItemGroupSwitch.defaultColor = (!__instance.m_bUseGroup) ? new(0.4f, 0.4f, 0.4f, 0.85f) : Color.white;
		EventDelegate.Add(__instance.EditItemGroupSwitch.onClick, __instance.OnClickEditItemGroup);

		__instance.EditTouchJumpSwitch.gameObject.SetActive(false);
		__instance.EditTouchJumpSwitch.defaultColor = (!__instance.m_bUseTouchJump) ? new(0.4f, 0.4f, 0.4f, 0.85f) : Color.white;
		EventDelegate.Add(__instance.EditTouchJumpSwitch.onClick, __instance.OnClickEditTouchJump);

		return false;
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.OnEndScene))]
	[HarmonyPrefix]
	private static void PreEndScene(SceneEdit __instance) {
		DressCode.ReopenPanel = GameMain.Instance.ScriptMgr.adv_kag.tag_backup.ContainsKey(DressCode.ReopenPanelTag);
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.OnEndScene))]
	[HarmonyPostfix]
	private static void PostEndScene(SceneEdit __instance) {
		if (!_isDressCode) return;

		_isDressCode = false;

		SaveCostume(__instance.maid, _currentScene);
	}

	//[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.OnEndScene))]
	//[HarmonyPrefix]
	private static bool OnEndScene(SceneEdit __instance) {
		if (!_isDressCode) return true;

		_isDressCode = false;

		if (GameMain.Instance.VRMode) {
			GameMain.Instance.CharacterMgr.ResetCharaPosAll();
		}
		UICamera.InputEnable = true;

		GameMain.Instance.ScriptMgr.adv_kag.kag.LoadScenarioString($"@jump file={__instance.m_currentScriptFile} label={__instance.m_strScriptArg}");
		GameMain.Instance.ScriptMgr.adv_kag.kag.Exec();

		return false;
	}

	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.OnEndDlgOk))]
	[HarmonyPrefix]
	private static bool OnEndDlgOk(SceneEdit __instance) {
		if (!_isDressCode) return true;

		GameMain.Instance.SysDlg.Close();
		UICamera.InputEnable = false;
		__instance.m_maid.ThumShotCamMove();
		__instance.m_maid.body0.trsLookTarget = GameMain.Instance.ThumCamera.transform;
		__instance.m_maid.boMabataki = false;

		__instance.StartCoroutine(WaitSetClothes(__instance));
		return false;
	}

	private static IEnumerator WaitSetClothes(SceneEdit sceneEdit) {
		while (GameMain.Instance.CharacterMgr.IsBusy()) {
			yield return null;
		}
		for (var i = 0; i < 90; i++) {
			yield return null;
		}

		GameMain.Instance.SoundMgr.PlaySe("SE022.ogg", false);
		sceneEdit.maid.ThumShotCustom(DressCode.GetThumbnailFileName(_currentProfile == CostumeProfile.Personal ? sceneEdit.maid : null, _currentScene), true);
		sceneEdit.maid.boMabataki = true;
		sceneEdit.maid.body0.trsLookTarget = null;
		GameMain.Instance.MainCamera.FadeOut(1f, false, sceneEdit.CreateKasizukiThumShot);
		yield break;
	}

	// load yotogi costume when launching edit mode from scheduled yotogi
	[HarmonyPatch(typeof(YotogiSkillSelectManager), nameof(YotogiSkillSelectManager.OnClickEdit))]
	[HarmonyPrefix]
	private static bool YotogiSkillSelectManager_OnClickEdit(YotogiSkillSelectManager __instance) {
		return StartCostumeEdit(__instance.maid_, CostumeScene.Yotogi, "YotogiMain.ks", "*edeit_end");
	}

	// load honeymoon costume when launching edit mode from honeymoon
	[HarmonyPatch(typeof(SceneHoneymoonModeMain), nameof(SceneHoneymoonModeMain.OnClickMaidEdit))]
	[HarmonyPrefix]
	private static bool SceneHoneymoonModeMain_OnClickMaidEdit(SceneHoneymoonModeMain __instance) {
		return StartCostumeEdit(__instance.manager.targetMaid, CostumeScene.Honeymoon, "HoneymoonModeMain.ks", "*メイン画面");
	}

	// enable preset save buttons
	[HarmonyPatch(typeof(PresetButtonCtrl), nameof(PresetButtonCtrl.Init))]
	[HarmonyPostfix]
	private static void PresetButtonCtrl_OnInit(GameObject goPresetButtonPanel) {
		UTY.GetChildObject(goPresetButtonPanel, "WindowPresetSave/Button", false).GetComponent<UIButton>().isEnabled = true;
	}

	// enable Set and Preset menus
	[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_Category))]
	[HarmonyPrefix]
	private static void SceneEdit_OnUpdatePanel_Category(SceneEdit __instance) {
		if (!_isDressCode) return;

		var enabledCategories = new SceneEditInfo.EMenuCategory[] {
			SceneEditInfo.EMenuCategory.セット,
			SceneEditInfo.EMenuCategory.プリセット,
		};
		foreach (var category in enabledCategories) {
			__instance.CategoryList.Find(c => c.m_eCategory == category).m_isEnabled = true;
		}
	}

	//[HarmonyPatch(typeof(Maid), "SetHairLengthSaveToMP")]
	//[HarmonyPrefix]
	private static bool Maid_OnSetHairLengthSaveToMP(Maid __instance) {
		Debug.Log("=== Maid_OnSetHairLengthSaveToMP");
		//return !_isDressCode;
		return true;
	}

	// force hide hair length window
	//[HarmonyPatch(typeof(HairLongWindow), "Update")]
	//[HarmonyPrefix]
	private static bool HairLongWindow_Update(HairLongWindow __instance) {
		if (!_isDressCode) return true;

		__instance.visible = false;
		return false;
	}

	// load only relevant slots from presets and as temporary props
	private static bool PresetSet(ref PresetBackup __state, CharacterMgr __instance, Maid f_maid, CharacterMgr.Preset f_prest) {
		if (!_isDressCode) return true;

		__state = new() {
			OriginalFileName = f_prest.strFileName,
			OriginalPropList = f_prest.listMprop,
		};
		// emptying the file name prevents loading ExternalPreset data
		f_prest.strFileName = "";
		f_prest.listMprop = f_prest.listMprop.Where(e => CostumeMpn.Contains((MPN)e.idx)).ToList();
		__state.ModifiedPropList = f_prest.listMprop;

		foreach (var maidProp in f_prest.listMprop) {
			var mpn = (MPN)maidProp.idx;
			var fileName = maidProp.strFileName;
			if (string.IsNullOrEmpty(fileName) && CM3.dicDelItem.TryGetValue(mpn, out var deleteItem)) {
				fileName = deleteItem;
			}
			if (__instance.IsEnableMenu(fileName)) {
				if (mpn != Mpn.body) {
					f_maid.SetProp(mpn, fileName, maidProp.nFileNameRID, true);
				}
			} else {
				f_maid.DelProp(mpn, true);
			}
		}

		if (Product.isPublic) {
			f_maid.SetProp(Mpn.bra, "bra030_i_.menu", 0, true);
			f_maid.SetProp(Mpn.panz, "Pants030_i_.menu", 0, true);
		}

		f_maid.AllProcPropSeqStart();

		return false;
	}

	private static void PostPresetSet(PresetBackup __state, CharacterMgr.Preset f_prest) {
		if (!_isDressCode) return;

		if (!string.IsNullOrEmpty(__state.OriginalFileName)) {
			f_prest.strFileName = __state.OriginalFileName;
		}

		// if the list is not the one we set in the prefix, an earlier patcher has probably restored it
		if (f_prest.listMprop == __state.ModifiedPropList) {
			f_prest.listMprop = __state.OriginalPropList;
		}
	}

	internal class PatchMethods30 {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset), typeof(bool))]
		private static bool CharacterMgr_PresetSet(ref PresetBackup __state, CharacterMgr __instance, Maid f_maid, CharacterMgr.Preset f_prest) => PresetSet(ref __state, __instance, f_maid, f_prest);

		[HarmonyPostfix]
		[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset), typeof(bool))]
		private static void CharacterMgr_PostPresetSet(PresetBackup __state, CharacterMgr.Preset f_prest) => PostPresetSet(__state, f_prest);

		private static void Category_SetEnabled(SceneEdit.SCategory category) {
			if (!_isDressCode) {
				category.m_isEnabled = true;
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.UpdatePanel_Category))]
		private static IEnumerable<CodeInstruction> UpdatePanel_Category(IEnumerable<CodeInstruction> instructions) {
			var codeMatcher = new CodeMatcher(instructions);

			codeMatcher
				.MatchStartForward(
					new CodeMatch(OpCodes.Ldc_I4_1),
					new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(SceneEdit.SCategory), nameof(SceneEdit.SCategory.m_isEnabled)))
				)
				.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(PatchMethods30), nameof(Category_SetEnabled)))
				.SetOpcodeAndAdvance(OpCodes.Nop);

			return codeMatcher.InstructionEnumeration();
		}
	}

	internal class PatchMethods20 {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset))]
		private static bool CharacterMgr_PresetSet(ref PresetBackup __state, CharacterMgr __instance, Maid f_maid, CharacterMgr.Preset f_prest) => PresetSet(ref __state, __instance, f_maid, f_prest);

		[HarmonyPostfix]
		[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset))]
		private static void CharacterMgr_PostPresetSet(PresetBackup __state, CharacterMgr.Preset f_prest) => PostPresetSet(__state, f_prest);
	}

	// save the temporary instead of the permanent items in the preset
	[HarmonyPatch(typeof(MaidProp), nameof(MaidProp.Serialize))]
	[HarmonyPrefix]
	private static void MaidProp_PreSerialize(ref MaidProp __state, MaidProp __instance) {
		if (!_isDressCode || !CostumeMpn.Contains((MPN)__instance.idx)) return;
		__state = new() {
			strFileName = __instance.strFileName,
			nFileNameRID = __instance.nFileNameRID,
		};
		__instance.strFileName = __instance.strTempFileName;
		__instance.nFileNameRID = __instance.nTempFileNameRID;
	}

	[HarmonyPatch(typeof(MaidProp), nameof(MaidProp.Serialize))]
	[HarmonyPostfix]
	private static void MaidProp_PostSerialize(MaidProp __state, MaidProp __instance) {
		if (!_isDressCode || !CostumeMpn.Contains((MPN)__instance.idx)) return;
		__instance.strFileName = __state.strFileName;
		__instance.nFileNameRID = __state.nFileNameRID;
	}

	[HarmonyPatch(typeof(CostumePartsEnabledCtrl), nameof(CostumePartsEnabledCtrl.LoadMaidPropData))]
	[HarmonyPrefix]
	private static bool LoadMaidPropData(CostumePartsEnabledCtrl __instance) {
		if (!_isDressCode) return true;

		var maid = GameMain.Instance.CharacterMgr.GetMaid(0);

		foreach (var button in __instance.m_dicItemBtn.Values) {
			SetButtonTexture(__instance, maid, button.m_menuCategory, button.m_mpn, button.m_txtItem);
			var costumeItemIsEnabled = !_enabledMpn.TryGetValue(button.m_mpn, out var isEnabled) || isEnabled;
			__instance.SetButtonActive(button, costumeItemIsEnabled);
			if (button.m_mpn == Mpn.hairt) {
				var component = UTY.GetChildObject(button.m_btnButton.gameObject, "Item2", false).GetComponent<UITexture>();
				SetButtonTexture(__instance, maid, SceneEditInfo.EMenuCategory.アクセサリ, Mpn.acckamisub, component);
			}
		}

		return false;
	}

	private static void SetButtonTexture(CostumePartsEnabledCtrl __instance, Maid maid, SceneEditInfo.EMenuCategory category, MPN mpn, UITexture tex) {
		var rId = maid.GetProp(mpn).nTempFileNameRID;
		foreach (var subPropMpnData in __instance.m_sceneEdit.subPropDatas) {
			if (mpn == subPropMpnData.mpn) {
				var items = subPropMpnData.manager.sloatItems;
				if (items != null && items[0].menuItem != null) {
					rId = items[0].menuItem.m_nMenuFileRID;
				}
				break;
			}
		}
		tex.mainTexture = rId == 0 ? __instance.m_notSettingIcon : __instance.GetTextureByRid(category, mpn, rId);
	}

	[HarmonyPatch(typeof(CostumePartsEnabledCtrl), nameof(CostumePartsEnabledCtrl.SetButtonActive))]
	[HarmonyPrefix]
	private static bool SetButtonActive(CostumePartsEnabledCtrl __instance, RandomPresetButton itemData, bool active) {
		if (!_isDressCode) return true;

		SetCostumeItemEnabled(itemData.m_mpn, active);
		if (itemData.m_mpn == Mpn.hairt) {
			__instance.testForced[Mpn.acckamisub] = active;
		}
		SetCostumeItemEnabled(Mpn.acckamisub, active);
		itemData.m_bBtnActive = active;
		if (itemData.m_bBtnActive) {
			itemData.m_btnButton.defaultColor = __instance.activeColor;
		} else {
			itemData.m_btnButton.defaultColor = __instance.inActiveColor;
		}

		return false;
	}

	private static void SetCostumeItemEnabled(MPN mpn, bool isEnabled) {
		_enabledMpn[mpn] = isEnabled;
	}

	private class PresetBackup {
		public string OriginalFileName { get; internal set; }
		public List<MaidProp> OriginalPropList { get; internal set; }
		public List<MaidProp> ModifiedPropList { get; internal set; }
	}
}
