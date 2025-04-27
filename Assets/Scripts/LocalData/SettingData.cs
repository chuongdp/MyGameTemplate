namespace LocalData
{
    using GameFoundation.Scripts.Interfaces;

    public class SettingData : ILocalData
    {
        public float       VolumeMusic { get; set; }
        public float       VolumeSound { get; set; }
        public float       Sensitivity { get; set; }
        public int         Language    { get; set; }
        public EResolution Resolution  { get; set; }
        public EFps        FPS         { get; set; }
        public bool        Vibration   { get; set; }
        public bool        Shadow      { get; set; }
        public bool        InvertY     { get; set; }

        public void Init()
        {
            this.VolumeMusic = 1f;
            this.VolumeSound = 1f;
            this.Sensitivity = 0.5f;
            this.Language    = 0;
            this.Resolution  = EResolution.Medium;
            this.FPS         = EFps.FPS60;
            this.Vibration   = true;
            this.Shadow      = true;
            this.InvertY     = false;
        }
    }

    public enum EResolution
    {
        Low    = 0,
        Medium = 1,
        High   = 2
    }

    public enum EFps
    {
        FPS30,
        FPS60,
        Unlimited
    }
}