using PrivateMaidMode;

namespace COM3D2.DressCode;

internal class DressCodeControl : MonoBehaviour {
	private const int UiWidth = 1920;

	private static readonly Vector3 PanelMargin = new(15, 0);
	private static readonly Vector2 SceneListSize = new(184, 925);
	private static readonly Vector2 ScopeListSize = new(144, 925);
	private static readonly Vector2 MaidListSize = new(502, 925);

	private static SelectionState _selectionState;

	private CostumeProfile? _privateModeProfile;

	private SceneList _sceneList;
	private ScopeList _scopeList;
	private MaidSelectPanel _maidSelectPanel;
	private ProfilePanel _profilePanel;

	private DressCodeManager m_mgr;
	private GameObject m_goPanel;

	public CostumeScene SelectedScene {
		get => _sceneList.SelectedValue;
		set => _sceneList.SelectValue(value);
	}

	public ProfileScope SelectedScope {
		get => _scopeList.SelectedValue;
		set => _scopeList.SelectValue(value);
	}

	public Maid SelectedMaid {
		get => _maidSelectPanel.SelectedMaid;
		set => _maidSelectPanel.SelectMaid(value);
	}

	public void Init(DressCodeManager manager, CanvasComponent panel) {
		m_mgr = manager;
		m_goPanel = panel.GameObject;

		panel.SizeDelta = new(1920, 1080);
		panel.Position = new(0, 0, 500);

		panel.AddComponent<Canvas>();
		panel.AddComponent<uGUICanvas>();
		panel.AddComponent<GraphicRaycaster>();

		_sceneList = new SceneList(panel);
		_sceneList.Position = new(-(UiWidth / 2) + SceneListSize.x / 2 + 41, 0);
		_sceneList.Size = SceneListSize;
		_sceneList.ValueSelected += (o, e) => UpdateProfileSelection();

		_scopeList = new ScopeList(panel);
		_scopeList.Position = _sceneList.Position + PanelMargin + new Vector3(SceneListSize.x / 2 + ScopeListSize.x / 2, 0);
		_scopeList.Size = ScopeListSize;
		_scopeList.ValueSelected += (o, e) => UpdateScopeSelection();

		_maidSelectPanel = new MaidSelectPanel(panel);
		_maidSelectPanel.Position = _scopeList.Position + PanelMargin + new Vector3(ScopeListSize.x / 2 + MaidListSize.x / 2, 0);
		_maidSelectPanel.Size = MaidListSize;
		_maidSelectPanel.MaidSelected += (o, e) => UpdateMaidSelection();

		_maidSelectPanel.ScrollChild.Position = new(0, MaidListSize.y / 2 - 112.5f);

		_profilePanel = new ProfilePanel(panel, this);
		_profilePanel.ProfileSelected += OnProfileChanged;

		var button = new CircleButton(panel, "BackButton");
		button.Position = new(790, -458);
		button.Term = "BackButton";
		button.Click += () => {
			GameMain.Instance.MainCamera.FadeOut(0.5f, false, () => {
				panel.SetActive(false);

				var dailyManager = manager.GetManager<DailyMgr>();
				dailyManager.m_goPanel.SetActive(true);

				var privateMaid = PrivateModeMgr.Instance.PrivateMaid;
				if (privateMaid != null) {
					privateMaid.Visible = true;

					// update private maid costume if necessary
					var newProfile = DressCode.GetPreferredProfile(privateMaid, CostumeScene.PrivateMode);
					if (_privateModeProfile != newProfile) {
						DressCode.ResetCostume(privateMaid);
						privateMaid.AllProcPropSeqStart();
						if (DressCode.TryGetEffectiveCostume(privateMaid, CostumeScene.PrivateMode, out var costume)) {
							DressCode.SetCostume(privateMaid, CostumeScene.PrivateMode, true);
						}
					}
				}
				manager.ResetBackground();
				GameMain.Instance.MainCamera.FadeIn();
			});
		};
	}

	public void CreateSelector() {
		var privateMaid = PrivateModeMgr.Instance.PrivateMaid;
		_privateModeProfile = privateMaid != null ? DressCode.GetPreferredProfile(privateMaid, CostumeScene.PrivateMode) : null;
		var previousSelection = _selectionState;
		_maidSelectPanel.Initialize();
		if (previousSelection != null) {
			SelectedScene = previousSelection.Scene;
			SelectedScope = previousSelection.Scope;
			SelectedMaid = previousSelection.Maid;
		} else {
			SelectedScope = ProfileScope.Scene;
			_sceneList.SelectFirstAvailable();
		}
	}

	internal void ResetSelections() => _selectionState = null;

	private void SetSceneMode() {
		_maidSelectPanel.SetActive(false);
		_profilePanel.SetSceneMode();
	}

	private void SetMaidMode() {
		_maidSelectPanel.SetActive(true);
		_profilePanel.SetMaidMode();
	}

	private void UpdateScopeSelection() {
		switch (SelectedScope) {
			case ProfileScope.Scene:
				SetSceneMode();
				break;
			case ProfileScope.Maid:
				SetMaidMode();
				break;
		}
		UpdateMaidSelection();
	}

	private void UpdateMaidSelection() {
		_sceneList.SetNpcMode(SelectedScope == ProfileScope.Maid && SelectedMaid.status.heroineType is not (MaidStatus.HeroineType.Original or MaidStatus.HeroineType.Transfer));
		_sceneList.SelectFirstAvailable(SelectedScene);
		UpdateProfileSelection();
	}

	private void UpdateProfileSelection() {
		_selectionState = new() {
			Scene = SelectedScene,
			Scope = SelectedScope,
			Maid = SelectedMaid,
		};
		var profile = SelectedScope switch {
			ProfileScope.Scene => DressCode.GetPreferredProfile(SelectedScene),
			ProfileScope.Maid => DressCode.GetPreferredProfile(SelectedMaid, SelectedScene),
			_ => throw new NotImplementedException(),
		};
		_profilePanel.SelectProfile(profile);
	}

	private void OnProfileChanged(object sender, ProfilePanel.ProfileSelectedEventArgs e) {
		if (SelectedScope == ProfileScope.Scene) {
			ConfigurationManager.Configuration.SetPreferredProfile(SelectedScene, e.Profile);
		} else {
			ConfigurationManager.Configuration.SetPreferredProfile(SelectedMaid, SelectedScene, e.Profile);
		}
	}

	class SelectionState {
		public CostumeScene Scene { get; set; }
		public ProfileScope Scope { get; set; }
		public Maid Maid { get; set; }
	}
}
