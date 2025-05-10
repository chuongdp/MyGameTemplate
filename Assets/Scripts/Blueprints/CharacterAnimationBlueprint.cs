namespace Game.Script.Blueprints
{
    using BlueprintFlow.BlueprintReader;

    [BlueprintReader("CharacterAnimation", true)]
    [CsvHeaderKey("Id")]
    public class CharacterAnimationBlueprint : GenericBlueprintReaderByRow<int, CharacterAnimationData>
    {
    }

    public class CharacterAnimationData
    {
        public int    Id              { get; set; }
        public string AnimationName   { get; set; }
        public string CharacterStatus { get; set; }
        public string TriggerName     { get; set; }
    }
}