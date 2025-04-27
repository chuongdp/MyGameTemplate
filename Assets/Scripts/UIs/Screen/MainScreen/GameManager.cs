namespace UnityTemplateProjects.UIs.Screen.MainScreen
{
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using HyperGames.UnityTemplate.UnityTemplate.Interfaces;
    using HyperGames.UnityTemplate.UnityTemplate.Services.Vibration;
    using MiraiGame.Script.Signals;
    using UnityEngine;

    public class GameManager : MonoBehaviour
    {
        #region Inject

        [Inject] private IVibrationService vibrationService;
        [Inject] private SignalBus         signalBus;

        #endregion

        private void Start() { this.signalBus.Subscribe<AttackSignal>(this.OnAttackPressed); }

        private void OnAttackPressed(AttackSignal obj)
        {
            Debug.Log("Attack button pressed");
            // Implement attack logic here
            this.vibrationService.PlayPresetType(VibrationPresetType.SoftImpact);
        }

        private void OnDestroy() { this.signalBus.Unsubscribe<AttackSignal>(this.OnAttackPressed); }
    }
}