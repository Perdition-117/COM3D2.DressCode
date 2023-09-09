using PrivateMaidMode;

namespace COM3D2.DressCode;

internal class DressCodeControl : MonoBehaviour {
	private ScopeSelectTabBar _scopeSelectTabBar;
	private MaidSelectPanel _maidSelectPanel;
	private SceneList _sceneList;
	private ProfilePanel _profilePanel;

	private DressCodeManager m_mgr;
	private GameObject m_goPanel;

	public ProfileScope SelectedScope {
		get => _scopeSelectTabBar.SelectedScope;
		set => _scopeSelectTabBar.SelectScope(value);
	}

	public CostumeScene SelectedScene {
		get => _sceneList.SelectedScene;
		set => _sceneList.SelectScene(value);
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

		_scopeSelectTabBar = new ScopeSelectTabBar(panel);
		_scopeSelectTabBar.ScopeSelected += (o, e) => UpdateScopeSelection();

		_maidSelectPanel = new MaidSelectPanel(panel);
		_maidSelectPanel.MaidSelected += (o, e) => UpdateMaidSelection();

		_sceneList = new SceneList(panel);
		_sceneList.SceneSelected += (o, e) => UpdateProfileSelection();

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

				var maid = PrivateModeMgr.Instance.PrivateMaid;
				if (maid != null) {
					maid.Visible = true;
				}
				manager.ResetBackground();
				GameMain.Instance.MainCamera.FadeIn();
			});
		};
	}

	public void CreateSelector() {
		SelectedScope = ProfileScope.Scene;
		_maidSelectPanel.Initialize();
	}

	private void SetSceneMode() {
		_maidSelectPanel.SetActive(false);
		_sceneList.Position = new(-832, 0);
		_profilePanel.SetSceneMode();
	}

	private void SetMaidMode() {
		_maidSelectPanel.SetActive(true);
		_sceneList.Position = new(-337, 0);
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
		_sceneList.SelectFirstAvailable();
		UpdateProfileSelection();
	}

	private void UpdateProfileSelection() {
		var profile = SelectedScope switch {
			ProfileScope.Scene => DressCode.GetPreferredProfile(SelectedScene),
			ProfileScope.Maid => DressCode.GetPreferredProfile(SelectedMaid, SelectedScene),
			_ => throw new NotImplementedException(),
		};
		_profilePanel.SelectProfile(profile);
	}

	private void OnProfileChanged(object sender, ProfilePanel.ProfileSelectedEventArgs e) {
		var privateMaid = PrivateModeMgr.Instance.PrivateMaid;
		CostumeProfile? currentProfile = privateMaid != null ? DressCode.GetPreferredProfile(privateMaid, SelectedScene) : null;
		if (SelectedScope == ProfileScope.Scene) {
			ConfigurationManager.Configuration.SetPreferredProfile(SelectedScene, e.Profile);
		} else {
			ConfigurationManager.Configuration.SetPreferredProfile(SelectedMaid, SelectedScene, e.Profile);
		}
		// update private maid costume immediately if necessary
		if (SelectedScene == CostumeScene.PrivateMode && privateMaid != null) {
			var newProfile = DressCode.GetPreferredProfile(privateMaid, SelectedScene);
			if (currentProfile != newProfile) {
				if (DressCode.TryGetEffectiveCostume(privateMaid, CostumeScene.PrivateMode, out var costume)) {
					DressCode.LoadCostume(privateMaid, costume, false, true);
					DressCode.SetTemporaryCostume(privateMaid, costume);
				} else {
					DressCode.ResetCostume(privateMaid);
				}
			}
		}
	}
}
