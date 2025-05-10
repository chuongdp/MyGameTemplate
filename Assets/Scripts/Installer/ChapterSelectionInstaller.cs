namespace Game.Script
{
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.Utilities;
    using Game.Script.UIs.Screen.ChapterSelection;
    using VContainer;

    public class ChapterSelectionInstaller : SceneScope
    {
        protected override void Configure(IContainerBuilder builder) { builder.InitScreenManually<ChapterSelectionPresenter>(); }
    }
}