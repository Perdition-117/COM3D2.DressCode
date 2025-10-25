using HarmonyLib;
using static SceneCharacterSelect;

namespace DressCode;

internal class CharacterSelectThumbnail {
	private static CostumeScene _scene;

	private static bool TryGetThumbnailPath(Maid maid, out string path) {
		path = string.Empty;
		if (DressCode.TryGetMaidProfile(maid, _scene, out var profile) && profile is { PreferredProfile: CostumeProfile.Personal, HasCostume: true }) {
			path = DressCode.GetThumbnailPath(maid, _scene);
			return true;
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CharacterSelectManager), nameof(CharacterSelectManager.Awake))]
	private static void CharacterSelectManager_Awake() {
		_scene = CostumeScene.None;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(CharacterSelectMain), nameof(CharacterSelectMain.OnCall))]
	private static void CharacterSelectMain_OnCall(CharacterSelectMain __instance) {
		_scene = __instance.scene_chara_select_.select_type switch {
			SelectType.Yotogi => CostumeScene.YotogiTalk,
			SelectType.NewYotogi or SelectType.NewYotogiAdditional => CostumeScene.Yotogi,
			SelectType.HoneymoonMode => CostumeScene.Honeymoon,
			_ => CostumeScene.None,
		};

		if (_scene != CostumeScene.None && TryGetThumbnailPath(__instance.select_maid_, out var thumbnailPath)) {
			__instance.chara_select_mgr_.big_thumbnail.SetFile(thumbnailPath);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DanceSelect), nameof(DanceSelect.Awake))]
	private static void DanceSelect_Awake() {
		_scene = CostumeScene.Dance;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(YotogiSubCharacterSelectManager), nameof(YotogiSubCharacterSelectManager.OnCall))]
	private static void YotogiSubCharacterSelectManager_OnCall() {
		_scene = CostumeScene.Yotogi;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(BigThumbnail), nameof(BigThumbnail.SetMaid))]
	private static bool BigThumbnail_SetMaid(BigThumbnail __instance, Maid maid) {
		if (_scene == CostumeScene.None) {
			return true;
		}

		if (_scene is CostumeScene.Dance or CostumeScene.PoleDance) {
			if (DanceSelect.m_SelectedDance.First() is { RhythmGameCorrespond: true }) {
				_scene = CostumeScene.Dance;
			} else {
				_scene = CostumeScene.PoleDance;
			}
		}

		if (_scene != CostumeScene.None && TryGetThumbnailPath(maid, out var thumbnailPath)) {
			__instance.SetFile(thumbnailPath);
			return false;
		}

		return true;
	}
}
