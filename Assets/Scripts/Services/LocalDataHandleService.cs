namespace Game.Script.Services
{
    using GameFoundation.Scripts.Utilities.UserData;
    using LocalData;

    public class LocalDataHandleService
    {
        private readonly UserLocalData           userLocalData;
        private readonly IHandleUserDataServices handleUserDataServices;

        public LocalDataHandleService(UserLocalData userLocalData, IHandleUserDataServices handleUserDataServices)
        {
            this.userLocalData          = userLocalData;
            this.handleUserDataServices = handleUserDataServices;
        }

        public string NamePlayer => this.userLocalData.NamePlayer;

        public int Level => this.userLocalData.Level;

        public bool IsFirstTime => this.userLocalData.IsFirstTime;

        public void SetNamePlayer(string namePlayer)
        {
            this.userLocalData.NamePlayer  = namePlayer;
            this.userLocalData.IsFirstTime = false;
            this.handleUserDataServices.Save(this.userLocalData);
        }

        public void SetLevel(int level)
        {
            this.userLocalData.Level = level;
            this.handleUserDataServices.Save(this.userLocalData);
        }

        public void SetIsFirstTime(bool isFirstTime)
        {
            this.userLocalData.IsFirstTime = isFirstTime;
            this.handleUserDataServices.Save(this.userLocalData);
        }
    }
}