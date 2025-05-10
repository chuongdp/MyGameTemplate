namespace Game.Script.Blueprints
{
    using BlueprintFlow.BlueprintReader;

    [BlueprintReader("ChapterData", true)]
    [CsvHeaderKey("Id")]
    public class ChapterDataBlueprint : GenericBlueprintReaderByRow<int, ChapterData>
    {
    }

    public class ChapterData
    {
        public int    Id           { get; set; }
        public string ChapterName  { get; set; }
        public string ChapterScene { get; set; }
    }
}