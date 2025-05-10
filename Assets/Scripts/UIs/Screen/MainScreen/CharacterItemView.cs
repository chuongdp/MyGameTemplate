namespace UnityTemplateProjects.UIs.Screen.MainScreen
{
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.AssetLibrary;
    using GameFoundation.Scripts.UIModule.MVP;
    using MiraiGame.Script.Blueprints;
    using MiraiGame.Script.Signals;
    using UnityEngine;
    using GameFoundation.Signals;
    using Random = UnityEngine.Random;

    public class CharacterItemView : TViewMono
    {
        public  Animator Anim;
        public  float    DelayWeight = 0.3f;
        public  bool     IsKeepFace;
        private float    current;

        private void Update()
        {
            if (Input.GetMouseButton(0))
                this.current                        = 1;
            else if (!this.IsKeepFace) this.current = Mathf.Lerp(this.current, 0, this.DelayWeight);

            this.Anim.SetLayerWeight(1, this.current);
        }
    }

    public class CharacterItemModel
    {
    }

    public class CharacterItemPresenter : BaseUIItemPresenter<CharacterItemView, CharacterItemModel>
    {
        private readonly IGameAssets                 gameAssets;
        private readonly CharacterEmotionBlueprint   characterEmotionBlueprint;
        private readonly CharacterAnimationBlueprint characterAnimationBlueprint;
        private readonly SignalBus                   signalBus;

        private CharacterItemModel model;
        private float              current;

        public CharacterItemPresenter(IGameAssets                 gameAssets,
                                      CharacterEmotionBlueprint   characterEmotionBlueprint,
                                      CharacterAnimationBlueprint characterAnimationBlueprint,
                                      SignalBus                   signalBus) : base(gameAssets)
        {
            this.gameAssets                  = gameAssets;
            this.characterEmotionBlueprint   = characterEmotionBlueprint;
            this.characterAnimationBlueprint = characterAnimationBlueprint;
            this.signalBus                   = signalBus;
        }

        public override void BindData(CharacterItemModel param)
        {
            this.model = param;

            this.signalBus.Subscribe<PlayAnimationSignal>(this.OnPlayAnimationSignal);
            this.signalBus.Subscribe<PlayEmotionSignal>(this.OnPlayEmotionSignal);
        }

        private void OnPlayAnimationSignal(PlayAnimationSignal signal)
        {
            Debug.Log("Received PlayAnimationSignal");
            this.PlayRandomAnimation();
        }

        private void OnPlayEmotionSignal(PlayEmotionSignal signal)
        {
            Debug.Log("Received PlayEmotionSignal");
            this.PlayRandomEmotion();
        }

        private async void PlayRandomAnimation()
        {
            var animIndex = Random.Range(1, 40);
            this.View.Anim.SetInteger("animation", animIndex);
            await UniTask.Delay(5000);
            this.View.Anim.SetInteger("animation", 40);
        }

        private void PlayRandomEmotion()
        {
            var listEmotions = this.characterEmotionBlueprint.Values.ToList();
            this.PlayAnim(listEmotions);
        }

        private void PlayAnim(List<CharacterAnimationData> listAnimationData)
        {
            if (listAnimationData.Count == 0)
            {
                Debug.LogWarning("No animation clips available to play");

                return;
            }

            var randomIndex = Random.Range(0, listAnimationData.Count);
            var animData    = listAnimationData[randomIndex];

            Debug.Log($"Playing animation: {animData.TriggerName}");
            this.View.Anim.SetTrigger(animData.TriggerName);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.signalBus.Unsubscribe<PlayAnimationSignal>(this.OnPlayAnimationSignal);
        }
    }
}