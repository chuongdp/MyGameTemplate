namespace Game.Script.UIs.Screen.ChapterSelection
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameFoundation.DI;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.Presenter;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.UIModule.ScreenFlow.Signals;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Signals;
    using Game.Script.Blueprints;
    using Game.Script.Services;
    using Game.Script.UIs.Common;
    using TMPro;
    using UnityEngine;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Debug = UnityEngine.Debug;

    public class ChapterSelectionScreenView : BaseView
    {
        public HeaderItem     HeaderItem;
        public ChapterAdapter ChapterAdapter;
        public Button         BtnSelect;
        public GameObject     LoadingObj;
    }

    [ScreenInfo(nameof(ChapterSelectionScreenView))]
    public class ChapterSelectionPresenter : BaseScreenPresenter<ChapterSelectionScreenView>
    {
        private readonly IDependencyContainer container;
        private readonly ChapterDataBlueprint chapterDataBlueprint;
        private readonly IGameAssets          gameAssets;
        private readonly SceneDirector        sceneDirector;

        private HeaderItemPresenter    headerItemPresenter;
        private List<ChapterItemModel> chapterDataList   = new();
        private int                    selectedChapterId = 1;

        public ChapterSelectionPresenter(SignalBus            signalBus, ILogService logger,
                                         IDependencyContainer container,
                                         ChapterDataBlueprint chapterDataBlueprint,
                                         IGameAssets          gameAssets,
                                         SceneDirector        sceneDirector) : base(signalBus, logger)
        {
            this.container            = container;
            this.chapterDataBlueprint = chapterDataBlueprint;
            this.gameAssets           = gameAssets;
            this.sceneDirector        = sceneDirector;
        }

        protected override void OnViewReady()
        {
            base.OnViewReady();
            this.Init();
            this.OpenViewAsync().Forget();
        }

        private void Init()
        {
            this.View.LoadingObj.SetActive(false);
            this.View.BtnSelect.onClick.AddListener(this.SelectChapter);
            this.InitHeaderItem();
            this.InitChapterAdapter();
        }

        private void InitHeaderItem()
        {
            this.headerItemPresenter ??= this.container.Instantiate<HeaderItemPresenter>();
            this.headerItemPresenter.SetView(this.View.HeaderItem);
            this.headerItemPresenter.BindData(new HeaderItemModel());
        }

        public override UniTask BindData() { return UniTask.CompletedTask; }

        private async void InitChapterAdapter()
        {
            this.chapterDataList = this.chapterDataBlueprint.Select(chapterData => new ChapterItemModel
            {
                ChapterData    = chapterData.Value,
                OnClickChapter = this.OnSelectChapter,
                IsSelected     = chapterData.Value.Id == this.selectedChapterId
            }).ToList();

            await this.View.ChapterAdapter.InitItemAdapter(this.chapterDataList);
        }

        private void OnSelectChapter(ChapterItemPresenter chapter)
        {
            this.selectedChapterId = chapter.ChapterId;
            var list = this.View.ChapterAdapter.GetPresenters();
            foreach (var presenter in list) presenter.SetSelected(false);
            chapter.SetSelected(true);
        }

        private void SelectChapter()
        {
            var selectedChapter = this.chapterDataList.FirstOrDefault(chapter => chapter.ChapterData.Id == this.selectedChapterId);
            if (selectedChapter == null)
            {
                Debug.Log($"Not found chapter with id: {this.selectedChapterId}");

                return;
            }

            this.View.LoadingObj.SetActive(true);
            _ = this.LoadChapterScene(selectedChapter.ChapterData.ChapterScene);
        }

        protected virtual async UniTask LoadChapterScene(string sceneName)
        {
            await this.sceneDirector.LoadSingleSceneAsync(sceneName);
            Debug.Log($"Load scene {sceneName} done");
        }

        public override void Dispose()
        {
            base.Dispose();
            this.View.LoadingObj.SetActive(false);
            this.headerItemPresenter?.Dispose();
            this.headerItemPresenter = null;
            this.View.BtnSelect.onClick.RemoveListener(this.SelectChapter);
            this.View.ChapterAdapter.GetPresenters().ForEach(p => p.Dispose());
        }
    }
}