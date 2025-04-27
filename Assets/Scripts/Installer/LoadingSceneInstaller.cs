namespace MyGame.Script.Installer
{
    using VContainer;
    using GameFoundation.DI;
    using GameFoundation.Scripts.UIModule.Utilities;
    using MyGame.Script;

    public class LoadingSceneScope : SceneScope
    {
        protected override void Configure(IContainerBuilder builder) { builder.InitScreenManually<LoadingScreenPresenter>(); }
    }
}