using System.Collections;
using com.workman.cm3d2.scene.dailyEtc;
using HarmonyLib;

namespace COM3D2.DressCode;

internal class CostumeEdit {
	private static CostumeScene _currentScene;
	private static CostumeProfile _currentProfile;
	private static string _currentScript;
	private static DailyMgr.Daily _currentDayTime;

	private static readonly Dictionary<MPN, bool> _enabledMpn = new();

	private static bool _isDressCode = false;

	internal static readonly HashSet<MPN> AvailableMpn = new() {
		//MPN.hairf,
		//MPN.hairr,
		//MPN.hairt,
		//MPN.hairs,
		//MPN.hairaho,

		MPN.wear,
		MPN.skirt,
		MPN.mizugi,
		MPN.bra,
		MPN.panz,
		MPN.stkg,
		MPN.shoes,
		MPN.onepiece,

		MPN.headset,
		MPN.glove,
		MPN.acchead,
		MPN.acchana,
		MPN.acckamisub,
		MPN.acckami,
		MPN.accmimi,
		MPN.accnip,
		MPN.acckubi,
		MPN.acckubiwa,
		MPN.accheso,
		MPN.accude,
		MPN.accashi,
		MPN.accsenaka,
		MPN.accshippo,
		MPN.accanl,
		MPN.accvag,
		MPN.megane,
		MPN.accxxx,
		MPN.handitem,
		MPN.acchat,
	};

	private static readonly Dictionary<DailyMgr.Daily, string> DayTimeLabels = new() {
		[DailyMgr.Daily.Daytime] = "*昼メニュー",
		[DailyMgr.Daily.Night] = "*夜メニュー",
	};

	public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, IConvertible {
		var retValue = value != null && Enum.IsDefined(typeof(TEnum), value);
		result = retValue ? (TEnum)Enum.Parse(typeof(TEnum), value) : default;
		return retValue;
	}

	internal static void StartCostumeEdit(Maid maid, CostumeScene scene) {
		StartCostumeEdit(maid, scene, CostumeProfile.Personal);
	}

	internal static void StartCostumeEdit(CostumeScene scene) {
		StartCostumeEdit(DressCode.GetHeadMaid(), scene, CostumeProfile.Shared);
	}

	private static void StartCostumeEdit(Maid maid, CostumeScene scene, CostumeProfile profile) {
		GameMain.Instance.MainCamera.FadeOut(0.5f, false, () => {
			GameMain.Instance.CharacterMgr.SetActiveMaid(maid, 0);
			maid.Visible = true;
			maid.AllProcPropSeqStart();
			GameMain.instance.StartCoroutine(DressCode.WaitMaidPropBusy(maid, () => {
				_currentScene = scene;
				DressCode.LogDebug($"Starting scene edit for {scene} ({profile})...");
				var costume = CreateCostume(maid, scene, profile);
				DressCode.LoadCostume(maid, costume, true);
				maid.AllProcPropSeqStart();
				GameMain.instance.StartCoroutine(DressCode.WaitMaidPropBusy(maid, () => {
					var scriptMgr = GameMain.Instance.ScriptMgr;
					scriptMgr.adv_kag.kag.LoadScenarioString($"@SceneCall name=SceneEdit {DressCode.ScriptTag}={profile} label={DayTimeLabels[_currentDayTime]}");
					scriptMgr.adv_kag.kag.Exec();
				}));
			}));
		});
	}

	private static Configuration.Costume CreateCostume(Maid maid, CostumeScene scene, CostumeProfile requestedProfile) {
		Configuration.Costume newCostume;
		if (requestedProfile == CostumeProfile.Personal && DressCode.TryGetMaidProfile(maid, scene, out var maidProfile) && maidProfile.HasCostume) {
			DressCode.LogDebug("Creating costume from maid profile...");
			newCostume = DressCode.CloneCostume(maidProfile.Costume);
		} else if (DressCode.TryGetSceneProfile(scene, out var sceneProfile) && sceneProfile.HasCostume) {
			DressCode.LogDebug("Creating costume from scene profile...");
			newCostume = DressCode.CloneCostume(sceneProfile.Costume);
		} else {
			DressCode.LogDebug("Creating costume from maid props...");
			newCostume = DressCode.CloneCostume(maid);
		}
		_enabledMpn.Clear();
		foreach (var item in newCostume.Items) {
			_enabledMpn[item.Slot] = item.IsEnabled;
		}
		return newCostume;
	}

	private static void SaveCostume(Maid maid, CostumeScene scene) {
		var newCostume = new Configuration.Costume();

		DressCode.LogDebug("Saving costume...");
		foreach (var mpn in AvailableMpn) {
			var prop = maid.GetProp(mpn);
			var isEnabled = !_enabledMpn.TryGetValue(mpn, out var isForcedExplicit) || isForcedExplicit;
			if (prop.strTempFileName == string.Empty || (IsDeleteItem(mpn, prop.strTempFileName) && isEnabled)) {
				continue;
			}
			DressCode.LogDebug($"- {mpn,-10} {isEnabled,-5} {prop.strTempFileName}");
			newCostume.AddItem(mpn, prop.strTempFileName, isEnabled);
		}

		ConfigurationManager.Configuration.SaveCostume(maid, scene, newCostume, _currentProfile);

		foreach (var mpn in AvailableMpn) {
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
		__instance.enabledMpns = AvailableMpn;

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

		var enabledSetMpns = new MPN[] {
			MPN.set_maidwear,
			MPN.set_mywear,
			MPN.set_underwear,
		};
		var w = __instance.CategoryList.Find(c => c.m_eCategory == SceneEditInfo.EMenuCategory.セット).m_listPartsType;
		foreach (var mpn in enabledSetMpns) {
			w.Find(c => c.m_mpn == mpn).m_isEnabled = true;
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
	[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset))]
	[HarmonyPrefix]
	private static bool CharacterMgr_PresetSet(ref List<MaidProp> __state, CharacterMgr __instance, Maid f_maid, CharacterMgr.Preset f_prest) {
		if (!_isDressCode) return true;

		__state = f_prest.listMprop;
		f_prest.listMprop = f_prest.listMprop.Where(e => AvailableMpn.Contains((MPN)e.idx)).ToList();

		foreach (var maidProp in f_prest.listMprop) {
			var mpn = (MPN)maidProp.idx;
			var fileName = maidProp.strFileName;
			if (string.IsNullOrEmpty(fileName) && CM3.dicDelItem.TryGetValue(mpn, out var deleteItem)) {
				fileName = deleteItem;
			}
			if (__instance.IsEnableMenu(fileName)) {
				if (mpn != MPN.body) {
					f_maid.SetProp(mpn, fileName, maidProp.nFileNameRID, true);
				}
			} else {
				f_maid.DelProp(mpn, true);
			}
		}

		if (Product.isPublic) {
			f_maid.SetProp(MPN.bra, "bra030_i_.menu", 0, true);
			f_maid.SetProp(MPN.panz, "Pants030_i_.menu", 0, true);
		}

		f_maid.AllProcPropSeqStart();

		return false;
	}

	[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet), typeof(Maid), typeof(CharacterMgr.Preset))]
	[HarmonyPostfix]
	private static void CharacterMgr_PostPresetSet(List<MaidProp> __state, CharacterMgr.Preset f_prest) {
		if (!_isDressCode) return;
		f_prest.listMprop = __state;
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
			if (button.m_mpn == MPN.hairt) {
				var component = UTY.GetChildObject(button.m_btnButton.gameObject, "Item2", false).GetComponent<UITexture>();
				SetButtonTexture(__instance, maid, SceneEditInfo.EMenuCategory.アクセサリ, MPN.acckamisub, component);
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
		if (itemData.m_mpn == MPN.hairt) {
			__instance.testForced[MPN.acckamisub] = active;
		}
		SetCostumeItemEnabled(MPN.acckamisub, active);
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
}
