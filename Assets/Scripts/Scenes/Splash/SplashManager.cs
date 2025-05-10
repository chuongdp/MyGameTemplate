namespace UnityTemplateProjects.Scenes.Splash
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SplashManager : MonoBehaviour
    {
        private const string LoadingSceneName = "0.LoadingScene";

        private void Start() { this.InitSDK(); }

        private void InitSDK()
        {
            this.LoadLoadingScene();
        }

        private async void LoadLoadingScene()
        {
            await UniTask.Delay(1000);
            Debug.Log("LoadLoadingScene");
            SceneManager.LoadSceneAsync(LoadingSceneName);
        }
    }
}