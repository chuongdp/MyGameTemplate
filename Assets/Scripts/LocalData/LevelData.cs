namespace LocalData
{
    using GameFoundation.Scripts.Interfaces;

    public class LevelData : ILocalData
    {
        public int Level;

        public void Init() { this.Level = 1; }
    }
}