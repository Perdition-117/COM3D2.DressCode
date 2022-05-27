namespace COM3D2.DressCode;

internal class DressCodeControl : MonoBehaviour {
	private ScopeSelectTabBar _scopeSelectTabBar;
	private MaidSelectPanel _maidSelectPanel;
	private SceneList _sceneList;
	private ProfilePanel _profilePanel;

	private DressCodeManager m_mgr;
	private GameObject m_goPanel;

	private ProfileScope _selectedScope = ProfileScope.Scene;
	private CostumeScene _selectedScene;
	private Maid _selectedMaid;

	public void Init(DressCodeManager manager, CanvasComponent panel) {
		m_mgr = manager;
		m_goPanel = panel.GameObject;

		panel.SizeDelta = new(1920, 1080);
		panel.Position = new(0, 0, 500);

		panel.AddComponent<Canvas>();
		panel.AddComponent<uGUICanvas>();
		panel.AddComponent<GraphicRaycaster>();

		_scopeSelectTabBar = new ScopeSelectTabBar(panel);
		_scopeSelectTabBar.ScopeSelected += OnScopeSelected;

		_maidSelectPanel = new MaidSelectPanel(panel);
		_maidSelectPanel.MaidSelected += OnMaidSelected;

		_sceneList = new SceneList(panel);
		_sceneList.SceneSelected += OnSceneSelected;

		_profilePanel = new ProfilePanel(panel);
		_profilePanel.ProfileSelected += OnProfileChanged;

		var button = new CircleButton(panel, "BackButton");
		button.Position = new(790, -458);
		button.Term = "BackButton";
		button.Click += () => {
			GameMain.Instance.MainCamera.FadeOut(0.5f, false, () => {
				panel.SetActive(false);

				var dailyManager = manager.GetManager<DailyMgr>();
				dailyManager.m_goPanel.SetActive(true);

				manager.ResetBackground();
				GameMain.Instance.MainCamera.FadeIn();
			});
		};
	}

	public void CreateSelector() {
		SetSceneMode();
		_scopeSelectTabBar.SelectScope(ProfileScope.Scene);
		_maidSelectPanel.Initialize(CharacterSelectManager.Type.Select);
		//_sceneList.SelectFirstAvailable();
	}

	private void SetSceneMode() {
		_maidSelectPanel.SetActive(false);
		_sceneList.Position = new(-832, 0);
		_profilePanel.SetSceneMode();
		SelectMaid(DressCode.GetHeadMaid());
	}

	private void SetMaidMode() {
		_maidSelectPanel.SetActive(true);
		_sceneList.Position = new(-337, 0);
		_profilePanel.SetMaidMode();
		SelectMaid(_maidSelectPanel.SelectedMaid);
	}

	private void SelectMaid(Maid maid) {
		_selectedMaid = maid;
		_profilePanel.SelectedMaid = maid;
		_sceneList.SetNpcMode(_selectedScope == ProfileScope.Maid && maid.status.heroineType != MaidStatus.HeroineType.Original);
		_sceneList.SelectFirstAvailable();
		UpdateProfile();
	}

	private void UpdateProfile() {
		var profile = _selectedScope switch {
			ProfileScope.Scene => DressCode.GetPreferredProfile(_selectedScene),
			ProfileScope.Maid => DressCode.GetPreferredProfile(_selectedMaid, _selectedScene),
			_ => throw new NotImplementedException(),
		};
		_profilePanel.SelectProfile(profile);
	}

	private void OnScopeSelected(object sender, ScopeSelectTabBar.ScopeSelectedEventArgs e) {
		_selectedScope = e.Scope;
		switch (e.Scope) {
			case ProfileScope.Scene:
				SetSceneMode();
				break;
			case ProfileScope.Maid:
				SetMaidMode();
				break;
		}
	}

	private void OnSceneSelected(object sender, SceneList.SceneSelectedEventArgs e) {
		_selectedScene = e.Scene;
		_profilePanel.SelectedScene = e.Scene;
		UpdateProfile();
	}

	private void OnMaidSelected(object sender, MaidSelectPanel.MaidSelectedEventArgs e) {
		SelectMaid(e.Maid);
	}

	private void OnProfileChanged(object sender, ProfilePanel.ProfileSelectedEventArgs e) {
		if (_selectedScope == ProfileScope.Scene) {
			ConfigurationManager.Configuration.SetPreferredProfile(_selectedScene, e.Profile);
		} else {
			ConfigurationManager.Configuration.SetPreferredProfile(_selectedMaid, _selectedScene, e.Profile);
		}
	}
}
