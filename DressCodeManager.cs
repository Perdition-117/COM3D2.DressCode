using com.workman.cm3d2.scene.dailyEtc;

namespace COM3D2.DressCode;

public class DressCodeManager : BaseCreatePanel {
	private CanvasComponent _maidPanel;

	private DressCodeControl m_Ctrl;

	public void Awake() {
		var uiRoot = GameObject.Find("/UI Root");

		var panel = new CanvasComponent(uiRoot, "DressCodePanel");
		panel.AddComponent<DressCodeControl>();
		_maidPanel = panel;
	}

	public override void Init() {
		m_goPanel = GetPanel("DressCodePanel");
		m_goPanel.SetActive(false);
		m_Ctrl = GetCtrl<DressCodeControl>();
		m_Ctrl.Init(this, _maidPanel);
	}

	public override void OpenPanel() {
		m_goPanel.SetActive(true);
		m_Ctrl.CreateSelector();
	}

	internal void SetBackground() {
		GameMain.Instance.MainCamera.Reset(CameraMain.CameraType.Target, true);
		GameMain.Instance.MainCamera.SetTargetPos(new(-0.05539433f, 0.95894f, 0.1269088f));
		GameMain.Instance.MainCamera.SetDistance(3f);
		GameMain.Instance.MainCamera.SetAroundAngle(new(-180f, 11.5528f));
		if (GameMain.Instance.VRMode) {
			GameMain.Instance.MainCamera.SetPos(new(0f, 1.4871f, 1f));
			GameMain.Instance.MainCamera.SetRotation(new(0f, -180f, 0f));
		}
		GameMain.Instance.BgMgr.ChangeBg("Theater");
	}

	internal void ResetBackground() {
		if (GameMain.Instance.VRMode) {
			GameMain.Instance.MainCamera.SetTargetPos(new(0f, 1.327261f, -0.1473188f));
			GameMain.Instance.MainCamera.SetDistance(3.6f);
			GameMain.Instance.MainCamera.SetAroundAngle(new(719.8212f, 2.235997f));
		} else {
			GameMain.Instance.MainCamera.SetTargetPos(new(0.5609447f, 1.380762f, -1.382336f));
			GameMain.Instance.MainCamera.SetDistance(1.6f);
			GameMain.Instance.MainCamera.SetAroundAngle(new(245.5691f, 6.273283f));
		}
		//GameMain.Instance.MainCamera.SetTargetOffset(Vector3.zero, false);
		GameMain.Instance.BgMgr.ChangeBg(GameMain.Instance.CharacterMgr.status.isDaytime ? DailyAPI.dayBg : DailyAPI.nightBg);
	}
}
