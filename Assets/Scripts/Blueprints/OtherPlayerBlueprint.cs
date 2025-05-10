namespace Game.Script.Blueprints
{
    using BlueprintFlow.BlueprintReader;

    [BlueprintReader("OtherPlayerData", true)]
    [CsvHeaderKey("Id")]
    public class OtherPlayerBlueprint : GenericBlueprintReaderByRow<int, OtherPlayerData>
    {
        public OtherPlayerBlueprint() { }
    }

    public class OtherPlayerData
    {
        public int    Id      { get; set; }
        public string Name    { get; set; }
        public string Address { get; set; }
    }
}