namespace Game.Script.Blueprints
{
    using BlueprintFlow.BlueprintReader;

    [BlueprintReader("CharacterEmotion", true)]
    [CsvHeaderKey("Id")]
    public class CharacterEmotionBlueprint: GenericBlueprintReaderByRow<int, CharacterAnimationData>
    {
        
    }
}