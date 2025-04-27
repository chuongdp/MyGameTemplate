namespace MyGame.Script.Services
{
    using GameFoundation.Scripts.Utilities.UserData;
    using LocalData;

    public class LocalSettingDataHandleService
    {
        private readonly SettingData             settingData;
        private readonly IHandleUserDataServices handleUserDataServices;

        public LocalSettingDataHandleService(SettingData settingData, IHandleUserDataServices handleUserDataServices)
        {
            this.settingData            = settingData;
            this.handleUserDataServices = handleUserDataServices;
        }

        public float       VolumeMusic => this.settingData.VolumeMusic;
        public float       VolumeSound => this.settingData.VolumeSound;
        public float       Sensitivity => this.settingData.Sensitivity;
        public int         Language    => this.settingData.Language;
        public EResolution Resolution  => this.settingData.Resolution;
        public EFps        FPS         => this.settingData.FPS;
        public bool        Vibration   => this.settingData.Vibration;
        public bool        Shadow      => this.settingData.Shadow;
        public bool        InvertY     => this.settingData.InvertY;

        public void SetMusicVolume(float value)
        {
            this.settingData.VolumeMusic = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetSoundVolume(float value)
        {
            this.settingData.VolumeSound = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetSensitivity(float value)
        {
            this.settingData.Sensitivity = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetLanguage(int value)
        {
            this.settingData.Language = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetResolution(EResolution value)
        {
            this.settingData.Resolution = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetFPS(EFps value)
        {
            this.settingData.FPS = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetVibration(bool value)
        {
            this.settingData.Vibration = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetShadow(bool value)
        {
            this.settingData.Shadow = value;
            this.handleUserDataServices.Save(this.settingData);
        }

        public void SetInvertY(bool value)
        {
            this.settingData.InvertY = value;
            this.handleUserDataServices.Save(this.settingData);
        }
    }
}