namespace Game.Script.Editor.Csv
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    [FilePath("Editor/CsvConfig.foo", FilePathAttribute.Location.ProjectFolder)]
    public class CsvDataConfig : ScriptableSingleton<CsvDataConfig>
    {
        public List<CsvConfigData> data;

        [Serializable]
        public class CsvConfigData
        {
            public string SheetName;
            public string SheetUrl;
            public string NameCsv;
        }

        public void SaveSO() { this.Save(true); }
    }
}