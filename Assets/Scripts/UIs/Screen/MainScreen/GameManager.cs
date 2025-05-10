namespace UnityTemplateProjects.UIs.Screen.MainScreen
{
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using HyperGames.UnityTemplate.UnityTemplate.Interfaces;
    using HyperGames.UnityTemplate.UnityTemplate.Services.Vibration;
    using MiraiGame.Script.Signals;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private CharacterItemView CharacterItemView;

        #region Inject

        [Inject] private IVibrationService    vibrationService;
        [Inject] private SignalBus            signalBus;
        [Inject] private IDependencyContainer container;

        #endregion

        private CharacterItemPresenter characterItemPresenter;

        private void Start()
        {
            this.signalBus.Subscribe<AttackSignal>(this.OnAttackPressed);
            this.BindCharacter();
        }

        private void BindCharacter()
        {
            this.characterItemPresenter ??= this.container.Instantiate<CharacterItemPresenter>();
            this.characterItemPresenter.SetView(this.CharacterItemView);
            this.characterItemPresenter.BindData(new CharacterItemModel());
        }

        private void OnAttackPressed(AttackSignal obj)
        {
            Debug.Log("Attack button pressed");
            // Implement attack logic here
            this.vibrationService.PlayPresetType(VibrationPresetType.SoftImpact);
        }

        private void OnDestroy()
        {
            this.signalBus.Unsubscribe<AttackSignal>(this.OnAttackPressed);
        }
    }
}