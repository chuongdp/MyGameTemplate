namespace MyGame.Script
{
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.Utilities;
    using MyGame.Script.UIs.Screen.ChapterSelection;
    using VContainer;

    public class ChapterSelectionInstaller : SceneScope
    {
        protected override void Configure(IContainerBuilder builder) { builder.InitScreenManually<ChapterSelectionPresenter>(); }
    }
}