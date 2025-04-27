namespace MyGame.Script
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using GameFoundation.Signals;
    using LocalData;
    using MyGame.Script.Services;
    using MyGame.Script.UIs.Popups;
    using MyGame.Script.UIs.Popups.SettingComponents;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIPopupSetting : BaseView
    {
        [Header("Music")] public Slider SldMusic;

        [Header("Sound")] public Slider SldSound;

        [Header("Sensitivity")] public Slider SldSensitivity;

        [Header("Vibration")] public SettingToggleItemView VibrationToggle;

        [Header("Selection")] public SettingSelectionItemView FpsSelection;
        public                       SettingSelectionItemView ResolutionSelection;
        public                       SettingSelectionItemView LanguageSelection;

        [Header("Shadow")] public SettingToggleItemView ShadowToggle;

        [Header("InvertY")] public SettingToggleItemView InvertYToggle;

        [Header("Close")] public Button BtnClose;
    }

    [PopupInfo(nameof(UIPopupSetting))]
    public class UIPopupSettingPresenter : BasePopupPresenter<UIPopupSetting>
    {
        private readonly SignalBus                     signalBus;
        private readonly ILogService                   logService;
        private readonly IDependencyContainer          container;
        private readonly LocalSettingDataHandleService localSettingDataHandle;
        private readonly LocalizationService           localizationService;
        private readonly IScreenManager                screenManager;

        private SettingSelectionItemPresenter fpsSelectionItemPresenter;
        private SettingSelectionItemPresenter resolutionSelectionItemPresenter;
        private SettingSelectionItemPresenter languageSelectionItemPresenter;
        private SettingToggleItemPresenter    vibrationItemPresenter;
        private SettingToggleItemPresenter    shadowItemPresenter;
        private SettingToggleItemPresenter    invertYItemPresenter;

        private bool needQuitApp = false;

        public UIPopupSettingPresenter(SignalBus                     signalBus,
                                       ILogService                   logger,
                                       IDependencyContainer          container,
                                       LocalSettingDataHandleService localSettingDataHandle,
                                       LocalizationService           localizationService,
                                       IScreenManager                screenManager) : base(signalBus, logger)
        {
            this.signalBus              = signalBus;
            this.logService             = logger;
            this.container              = container;
            this.localSettingDataHandle = localSettingDataHandle;
            this.localizationService    = localizationService;
            this.screenManager          = screenManager;
        }

        public override UniTask BindData() { return UniTask.CompletedTask; }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            this.InitUI();
            this.InitListener();
            this.BindFps();
            this.BindResolution();
            this.BindLanguage();
        }

        private void InitUI()
        {
            this.View.SldMusic.value       = this.localSettingDataHandle.VolumeMusic;
            this.View.SldSound.value       = this.localSettingDataHandle.VolumeSound;
            this.View.SldSensitivity.value = this.localSettingDataHandle.Sensitivity;
        }

        private void InitListener()
        {
            this.View.BtnClose.onClick.AddListener(this.CloseView);
            this.View.SldMusic.onValueChanged.AddListener(this.UpdateMusicVolumeValue);
            this.View.SldSound.onValueChanged.AddListener(this.UpdateSoundVolumeValue);
            this.View.SldSensitivity.onValueChanged.AddListener(this.UpdateSensitivityValue);

            this.InitVibrationToggle();
            this.InitShadowToggle();
            this.InitInvertYToggle();
        }

        private void BindFps()
        {
            this.fpsSelectionItemPresenter ??= this.container.Instantiate<SettingSelectionItemPresenter>();
            this.fpsSelectionItemPresenter.SetView(this.View.FpsSelection);
            this.fpsSelectionItemPresenter.OnViewReady();
            this.fpsSelectionItemPresenter.BindData(new SettingSelectionItemModel
            {
                Options = new Dictionary<int, string>()
                {
                    { 0, nameof(EFps.FPS30).Remove(0, 3) },
                    { 1, nameof(EFps.FPS60).Remove(0, 3) },
                    { 2, nameof(EFps.Unlimited) }
                },
                PreviousCallback = OnChangeFps,
                NextCallback     = OnChangeFps,
                CurrentOption    = 0
            });

            return;

            void OnChangeFps(int index)
            {
                var fps = (EFps)index;
                this.localSettingDataHandle.SetFPS(fps);
            }
        }

        private void BindResolution()
        {
            this.resolutionSelectionItemPresenter ??= this.container.Instantiate<SettingSelectionItemPresenter>();
            this.resolutionSelectionItemPresenter.SetView(this.View.ResolutionSelection);
            this.resolutionSelectionItemPresenter.OnViewReady();
            this.resolutionSelectionItemPresenter.BindData(new SettingSelectionItemModel
            {
                Options = new Dictionary<int, string>()
                {
                    { 0, nameof(EResolution.Low) },
                    { 1, nameof(EResolution.Medium) },
                    { 2, nameof(EResolution.High) }
                },
                PreviousCallback = OnChangeResolution,
                NextCallback     = OnChangeResolution,
                CurrentOption    = 0
            });

            return;

            void OnChangeResolution(int index)
            {
                this.needQuitApp = true;
                var resolution = (EResolution)index;
                this.localSettingDataHandle.SetResolution(resolution);
            }
        }

        private void BindLanguage()
        {
            var listLan = this.localizationService.ListLanguages();
            var dictLan = new Dictionary<int, string>();
            for (var i = 0; i < listLan.Count; i++)
            {
                dictLan.Add(i, listLan[i]);
            }

            this.languageSelectionItemPresenter ??= this.container.Instantiate<SettingSelectionItemPresenter>();
            this.languageSelectionItemPresenter.SetView(this.View.LanguageSelection);
            this.languageSelectionItemPresenter.OnViewReady();
            this.languageSelectionItemPresenter.BindData(new SettingSelectionItemModel
            {
                Options          = dictLan,
                PreviousCallback = OnChangeLanguage,
                NextCallback     = OnChangeLanguage,
                CurrentOption    = 0
            });

            return;

            void OnChangeLanguage(int index)
            {
                var lang = dictLan[index];
                this.localizationService.ChangeLanguage(lang);
            }
        }

        private void UpdateMusicVolumeValue(float value) { this.localSettingDataHandle.SetMusicVolume(value); }

        private void UpdateSoundVolumeValue(float value) { this.localSettingDataHandle.SetSoundVolume(value); }

        private void UpdateSensitivityValue(float value) { this.localSettingDataHandle.SetSensitivity(value); }

        private void InitVibrationToggle()
        {
            this.vibrationItemPresenter ??= this.container.Instantiate<SettingToggleItemPresenter>();
            this.vibrationItemPresenter.SetView(this.View.VibrationToggle);
            this.vibrationItemPresenter.OnViewReady();
            this.vibrationItemPresenter.BindData(new SettingToggleItemModel
            {
                IsOn     = this.localSettingDataHandle.Vibration,
                Callback = isOn => { this.localSettingDataHandle.SetVibration(isOn); }
            });
        }

        private void InitShadowToggle()
        {
            this.shadowItemPresenter ??= this.container.Instantiate<SettingToggleItemPresenter>();
            this.shadowItemPresenter.SetView(this.View.ShadowToggle);
            this.shadowItemPresenter.OnViewReady();
            this.shadowItemPresenter.BindData(new SettingToggleItemModel
            {
                IsOn = this.localSettingDataHandle.Shadow,
                Callback = isOn =>
                {
                    this.needQuitApp = true;
                    this.localSettingDataHandle.SetShadow(isOn);
                }
            });
        }

        private void InitInvertYToggle()
        {
            this.invertYItemPresenter ??= this.container.Instantiate<SettingToggleItemPresenter>();
            this.invertYItemPresenter.SetView(this.View.InvertYToggle);
            this.invertYItemPresenter.OnViewReady();
            this.invertYItemPresenter.BindData(new SettingToggleItemModel
            {
                IsOn     = this.localSettingDataHandle.InvertY,
                Callback = isOn => { this.localSettingDataHandle.SetInvertY(isOn); }
            });
        }
        
        private void OpenRestartAppConfirm()
        {
            this.screenManager.OpenScreen<UIPopupCommonPresenter, UIPopupCommonModel>(new UIPopupCommonModel
            {
                PopupType  = ECommonPopupType.Ok,
                Title      = "confirm_title",
                Content    = "quit_app_content",
                OkCallback = Application.Quit
            });
        }

        public override void CloseView()
        {
            if (this.needQuitApp)
            {
                this.OpenRestartAppConfirm();
            }
            base.CloseView();
        }
    }
}