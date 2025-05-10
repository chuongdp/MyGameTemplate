namespace LocalData
{
    using GameFoundation.Scripts.Interfaces;

    public class UserLocalData : ILocalData
    {
        public int    Level;
        public string NamePlayer;
        public bool   IsFirstTime;

        public void Init()
        {
            this.NamePlayer  = "TestPlayer";
            this.IsFirstTime = true;
            this.Level       = 1;
        }
    }
}