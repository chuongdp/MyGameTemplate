using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exoa.TutorialEngine
{
    public class TutorialLoader : MonoBehaviour
    {
        public static TutorialLoader instance;

        private TutorialController tc;

        [HideInInspector] public bool tutorialLoaded;

        [HideInInspector] public Tutorial currentTutorial;

        [HideInInspector] public string loadedTutorialName;

        public virtual void Awake()
        {
            instance            = this;
            this.tc             = this.GetComponent<TutorialController>();
            this.tutorialLoaded = false;
        }

        public void Start() { }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (Input.GetKeyDown(KeyCode.E) && Event.current != null && Event.current.control && Event.current.type == EventType.KeyDown)
                SceneManager.LoadScene("TutorialEditor");
        }
#endif

        public virtual void Load(string name, bool sendLoadedEvent = true)
        {
            if (this.tc != null && this.tc.debug) Debug.Log("Load " + name);

            this.currentTutorial = LoadOffline(name);

            this.loadedTutorialName = name;
            this.ProcessTutorial(sendLoadedEvent);
        }

        protected virtual void ProcessTutorial(bool sendLoadedEvent)
        {
            if (this.currentTutorial == null) return;

            this.tutorialLoaded = true;

            if (sendLoadedEvent)
                TutorialEvents.OnTutorialLoaded?.Invoke();
            TutorialEvents.OnTutorialReady?.Invoke();
        }

        public static Tutorial LoadOffline(string name)
        {
            var json = Resources.Load<TextAsset>("Tutorials/" + name);

            return JsonUtility.FromJson<Tutorial>(json.text);
        }
    }
}