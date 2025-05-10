using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    using System.Collections.Generic;

    [AddComponentMenu("Malbers/Utilities/Tools/Unity [Tools] Utilities")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/ui/unity-utils")]
    public class UnityUtils : MonoBehaviour
    {
        /// <summary>Instantiate a GameObject chosen randomly from a GameObjectList in the position of this gameObject</summary>
        public void InstantiateRandom(List<GameObject> value)
        {
            var randomObject = value[Random.Range(0, value.Count)];
            Instantiate(randomObject, this.transform.position, this.transform.rotation);
        }

        /// <summary>Instantiate a GameObject chosen randomly from a GameObjectList in the position of this gameObject and it also  parent it </summary>
        public void InstantiateRandomAndParent(List<GameObject> value)
        {
            var randomObject = value[Random.Range(0, value.Count)];
            Instantiate(randomObject, this.transform.position, this.transform.rotation, this.transform);
        }

        public virtual void PauseEditor()
        {
            Debug.Log("Pause Editor", this);
            Debug.Break();
        }

        /// <summary>Multiply the Local Scale by a value</summary>
        public virtual void Scale_By_Float(float scale) { this.transform.localScale = Vector3.one * scale; }

        private AudioSource[] audios;

        /// <summary>Ugly way to stop all audiosources on the scenes</summary>
        public virtual void PauseAllAudio(bool pause)
        {
            if (!this.enabled) return;

            this.audios ??= FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

            if (pause)
            {
                foreach (var audio in this.audios)
                    if (audio.isPlaying)
                        audio.Pause();
            }
            else
            {
                foreach (var audio in this.audios)
                    if (audio != null)
                        audio.UnPause();
            }
        }

        public void AddRigiBody()
        {
            var rb = this.gameObject.AddComponent<Rigidbody>();

            // Lock all constraints on the Rigidbody
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;
        }

        public void DestroyRigidbody()
        {
            if (this.gameObject.TryGetComponent<Rigidbody>(out var rb))
                Destroy(rb);
        }

        public void Forward_Direction(Transform target) { this.transform.forward = (target.position - this.transform.position).normalized; }

        public void Forward_Direction(TransformVar target) { this.Forward_Direction(target.Value); }

        public void Forward_Direction_NoY(Transform target)
        {
            var forward = target.position - this.transform.position;
            forward.y              = 0;
            this.transform.forward = forward.normalized;
        }

        public void Forward_Direction_NoY(TransformVar target) { this.Forward_Direction_NoY(target.Value); }

        public virtual void Toggle_Enable(Behaviour component) { component.enabled = !component.enabled; }

        public virtual void Time_Freeze(bool value) { this.Time_Scale(value ? 0 : 1); }

        public virtual void Time_Scale(float value) { Time.timeScale = value; }

        public virtual void Freeze_Time(bool value) { this.Time_Freeze(value); }

        /// <summary>Destroy this GameObject by a time </summary>
        public void DestroyMe(float time) { Destroy(this.gameObject, time); }

        /// <summary>Destroy this GameObject</summary>
        public void DestroyMe() { Destroy(this.gameObject); }

        /// <summary>Destroy this GameObject on the Next Frame</summary>
        public void DestroyMeNextFrame() { this.StartCoroutine(this.DestroyNextFrame()); }

        /// <summary>Destroy a GameObject</summary>
        public void DestroyGameObject(GameObject go) { Destroy(go); }

        /// <summary>Destroy a Component</summary>
        public void DestroyComponent(Component component) { Destroy(component); }

        /// <summary>Disable a gameObject and enable it the next frame</summary>
        public void Reset_GameObject(GameObject go)
        {
            go.SetActive(false);
            this.Delay_Action(() => go.SetActive(true));
        }

        /// <summary>Disable a Monobehaviour and enable it the next frame</summary>
        public void Reset_Monobehaviour(MonoBehaviour go)
        {
            go.SetEnable(false);
            this.Delay_Action(() => go.SetEnable(true));
        }

        /// <summary>Hide this GameObject after X Time</summary>
        public void GameObjectHide(float time) { this.Invoke(nameof(this.DisableGo), time); }

        /// <summary>Random Rotate around X</summary>
        public void RandomRotateAroundX() { this.transform.Rotate(new Vector3(Random.Range(0, 360), 0, 0), Space.Self); }

        /// <summary>Random Rotate around X</summary>
        public void RandomRotateAroundY() { this.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0), Space.Self); }

        /// <summary>Random Rotate around X</summary>
        public void RandomRotateAroundZ() { this.transform.Rotate(new Vector3(0, 0, Random.Range(0, 360)), Space.Self); }

        //public void Move_Local(Vector3Var vector) => transform.Translate(vector, Space.Self);
        //public void Move_World(Vector3Var vector) => transform.Translate(vector, Space.World);

        public void DebugLog(string value) { Debug.Log($"[{this.name}]-[{value}]", this); }

        public void DebugLog(object value) { Debug.Log($"[{this.name}]-[{value}]", this); }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Reset the Local Rotation of this gameObject</summary>
        public void Rotation_Reset() { this.transform.localRotation = Quaternion.identity; }

        /// <summary>Reset the Local Position of this gameObject</summary>
        public void Position_Reset() { this.transform.localPosition = Vector3.zero; }

        /// <summary>Reset the Local Rotation of a gameObject</summary>
        public void Rotation_Reset(GameObject go) { go.transform.localRotation = Quaternion.identity; }

        /// <summary>Reset the Local Position of a gameObject</summary>
        public void Position_Reset(GameObject go) { go.transform.localPosition = Vector3.zero; }

        /// <summary>Reset the Local Rotation of a transform</summary>
        public void Rotation_Reset(Transform go) { go.localRotation = Quaternion.identity; }

        /// <summary>Reset the Local Position of a transform</summary>
        public void Position_Reset(Transform go) { go.localPosition = Vector3.zero; }

        /// <summary>Parent this Game Object to a new Transform, retains its World Position</summary>
        public void Parent(Transform value) { this.transform.parent = value; }

        public void Parent(GameObject value) { this.Parent(value.transform); }

        public void Parent(Component value) { this.Parent(value.transform); }

        /// <summary>Remove the Parent of a transform</summary>
        public void Unparent(Transform value) { value.parent = null; }

        /// <summary>Remove the Parent of a transform</summary>
        public void Unparent(GameObject value) { this.Unparent(value.transform); }

        /// <summary>Remove the Parent of a transform</summary>
        public void Unparent(Component value) { this.Unparent(value.transform); }

        /// <summary>Disable a behaviour on a gameobject using its index of all the behaviours attached to the gameobject.
        /// Useful when they're duplicated components on a same gameobject </summary>
        public void Behaviour_Disable(int index)
        {
            var components                                                        = this.GetComponents<Behaviour>();
            if (components != null) components[index % components.Length].enabled = false;
        }

        /// <summary>Enable a behaviour on a gameobject using its index of all the behaviours attached to the gameobject.
        /// Useful when they're duplicated components on a same gameobject </summary>
        public void Behaviour_Enable(int index)
        {
            var components                                                        = this.GetComponents<Behaviour>();
            if (components != null) components[index % components.Length].enabled = true;
        }

        public void Behaviour_EnableNextFrame(Behaviour behaviour)
        {
            behaviour.enabled = false;
            this.Delay_Action(() => behaviour.enabled = true);
        }

        /// <summary>Add an gameobject to Don't destroy on load logic</summary>
        public void Dont_Destroy_On_Load(GameObject value) { DontDestroyOnLoad(value); }

        /// <summary>Loads additive a new scene</summary>
        public void Load_Scene_Additive(string value) { SceneManager.LoadScene(value, LoadSceneMode.Additive); }

        /// <summary>Loads a new scene</summary>
        public void Load_Scene(string value)
        {
            if (!string.IsNullOrEmpty(value))
                SceneManager.LoadScene(value, LoadSceneMode.Single);
        }

        /// <summary>Parent this GameObject to a new Transform</summary>
        public void Parent_Local(Transform value)
        {
            this.transform.parent = value;
            this.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            this.transform.localScale = Vector3.one;
        }

        /// <summary>Parent a GameObject to a new Transform</summary>
        public void Parent_Local(GameObject value) { this.Parent_Local(value.transform); }

        public void Parent_Local(Component value) { this.Parent_Local(value.transform); }

        /// <summary>Instantiate a GameObject in the position of this gameObject</summary>
        public void Instantiate(GameObject value) { Instantiate(value, this.transform.position, this.transform.rotation); }

        /// <summary>Instantiate a GameObject in the position of this gameObject and it also  parent it </summary>
        public void InstantiateAndParent(GameObject value) { Instantiate(value, this.transform.position, this.transform.rotation, this.transform); }

        /// <summary>Show/Hide the Cursor</summary>
        public static void ShowCursor(bool value)
        {
            Cursor.lockState = !value ? CursorLockMode.Locked : CursorLockMode.None; // Lock or unlock the cursor.
            Cursor.visible   = value;
        }

        public static void ShowCursorInvert(bool value) { ShowCursor(!value); }

        private void DisableGo() { this.gameObject.SetActive(false); }

        private IEnumerator C_Reset_GameObject(GameObject go)
        {
            if (go.activeInHierarchy)
            {
                go.SetActive(false);

                yield return null;
                go.SetActive(true);
            }

            yield return null;
        }

        private IEnumerator C_Reset_Mono(MonoBehaviour go)
        {
            if (go.gameObject.activeInHierarchy)
            {
                go.enabled = false;

                yield return null;
                go.enabled = true;
            }

            yield return null;
        }

        private IEnumerator DestroyNextFrame()
        {
            yield return null;
            Destroy(this.gameObject);
        }

        public void RectTransform_Width(float width)
        {
            var rt = this.GetComponent<RectTransform>();

            if (rt) rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
        }

        public void RectTransform_Height(float height)
        {
            var rt = this.GetComponent<RectTransform>();

            if (rt) rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        public void RectTransform_Width(int width) { this.RectTransform_Width((float)width); }

        public void RectTransform_Height(int height) { this.RectTransform_Height((float)height); }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UnityUtils))]
    public class UnityUtilsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("Use this component to execute simple unity logics, " +
                                          "such as Parent, Instantiate, Destroy, Disable..\nUse it via Unity Events");
        }
    }
#endif
}