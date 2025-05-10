using Exoa.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Exoa.TutorialEngine
{
    using Cysharp.Threading.Tasks;
    using MalbersAnimations;
    using UnityEngine.Serialization;

    public class TutorialController : MonoBehaviour
    {
        private enum State
        {
            Inactive,
            Loading,
            Playing,
            FadIngOut
        };

        private static State tutorialState;
        private static bool  isSkippable;

        private TutorialSession                    session;
        private List<TutorialSession.TutorialStep> steps;
        public  TutorialPopup                      popup;

        public  Button        hiddenBtn;
        public  RectTransform hiddenBtnRt;
        public  RectTransform mask;
        private int           currentStep = -1;
        public  float         maskScale   = 1.2f;
        private Color         initBGColor;
        public  Image         bg;

        public static TutorialController instance;
        private       bool               retried;

        private float      size1Duration = 0.2f;
        public  Color      tutoBGColor;
        private GameObject currentTarget;
        private Vector2    currentEndPositionValue;
        private float      currentEndSizeValue;
        private float      currentEndPositionChangeTime;
        private Color      currentBgColor;

        private GameObject lastFocusedObject;
        private Transform  lastFocusedObjectParent;
        private int        lastFocusedObjectSibling;
        public  bool       debug;

        [Header("ANIMATION")] public Springs       popupMoveSettings;
        private                      Vector2Spring popupMoveSpring;
        public                       Springs       maskMoveSettings;
        private                      Vector2Spring maskMoveSpring;
        public                       Springs       maskSize1Settings;
        public                       Springs       maskSize2Settings;
        private                      Vector2Spring maskSizeSpring;
        public                       Springs       bgColorSettings;
        private                      Vector4Spring bgColorSpring;

        [Header("Joystick")] public MobileJoystick hiddenJoystickMove;
        public                      MobileJoystick hiddenJoystickRotate;

        public static bool IsSkippable      { get => isSkippable; set => isSkippable = value; }
        public static bool IsTutorialActive => tutorialState == State.Playing;

        private void OnDestroy() { TutorialEvents.OnTutorialLoaded -= this.OnTutorialLoeaded; }

        private void Awake()
        {
            if (instance != null) throw new Exception("TutorialController alraedy creaeted");

            instance = this;
            this.popup.gameObject.SetActive(false);
            this.popup.closeBtn.onClick.AddListener(this.OnClosePopup);
        }

        private void OnClosePopup() { this.HideTutorial(); }

        private void Start()
        {
            this.initBGColor       = this.bg.color;
            this.bg.material.color = this.initBGColor;

            if (TutorialLoader.instance.tutorialLoaded) this.OnTutorialLoeaded();

            TutorialEvents.OnTutorialLoaded += this.OnTutorialLoeaded;
        }

        private void OnTutorialLoeaded()
        {
            this.steps = new List<TutorialSession.TutorialStep>();
            if (TutorialLoader.instance != null && TutorialLoader.instance.currentTutorial.tutorial_steps != null) this.steps.AddRange(TutorialLoader.instance.currentTutorial.tutorial_steps);

            tutorialState    = State.Playing;
            this.currentStep = -1;

            if (this.steps.Count > 0)
            {
                this.popup.OnClickNext.RemoveAllListeners();
                this.popup.OnClickNext.AddListener(this.Next);
                this.popup.closeBtn.gameObject.SetActive(IsSkippable);
                this.popup.gameObject.SetActive(true);
                this.popup.createBackground = false;
                this.popup.Init();
                this.popup.Center();
                this.popup.Open();

                this.bg.gameObject.SetActive(true);

                this.mask.gameObject.SetActive(true);
                this.mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
                this.mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   200);
                this.mask.anchoredPosition = new Vector2(0, 1000);
                this.hiddenBtn.gameObject.SetActive(false);

                this.Next();
            }
            else
            {
                this.HideTutorial();
            }
        }

        private void Update()
        {
            if (tutorialState == State.Inactive) return;

            if (tutorialState == State.FadIngOut)
            {
                var targetOutSize = new Vector2(7000, 7000);
                this.mask.sizeDelta                 = this.maskSize1Settings.UpdateSpring(ref this.maskSizeSpring, targetOutSize);
                this.popup.PopupRt.anchoredPosition = this.popupMoveSettings.UpdateSpring(ref this.popupMoveSpring, targetOutSize);
                if (Vector2.Distance(targetOutSize, this.mask.sizeDelta) < 100)
                {
                    tutorialState = State.Inactive;
                    this.bg.gameObject.SetActive(false);
                    this.mask.gameObject.SetActive(false);
                    this.popup.Hide();
                    TutorialEvents.OnTutorialComplete?.Invoke();

                    return;
                }
            }

            if (tutorialState == State.Playing)
            {
                if (this.currentTarget != null)
                {
                    var targetRect2D = this.GetObjectCanvasRect(this.currentTarget);
                    var targetSize   = Mathf.Max(targetRect2D.width, targetRect2D.height);

                    if (this.currentEndSizeValue != targetSize) this.currentEndSizeValue = targetSize;

                    if (this.currentEndPositionValue != targetRect2D.position) this.currentEndPositionValue = targetRect2D.position;

                    this.mask.anchoredPosition = this.maskMoveSettings.UpdateSpring(ref this.maskMoveSpring, this.currentEndPositionValue);
                    var targetMaskSize = Vector2.one * this.currentEndSizeValue * this.maskScale;

                    if (this.currentEndPositionChangeTime > Time.time - this.size1Duration)
                    {
                        targetMaskSize      = new Vector2(.3f, 2f) * this.currentEndSizeValue * this.maskScale;
                        this.mask.sizeDelta = this.maskSize1Settings.UpdateSpring(ref this.maskSizeSpring, targetMaskSize);
                    }
                    else
                    {
                        this.mask.sizeDelta = this.maskSize2Settings.UpdateSpring(ref this.maskSizeSpring, targetMaskSize);
                    }

                    var popupPositon = this.popup.CalculatePopupPosition(targetRect2D);
                    this.popup.PopupRt.anchoredPosition = this.popupMoveSettings.UpdateSpring(ref this.popupMoveSpring, popupPositon);
                    this.hiddenBtnRt.anchoredPosition   = this.currentEndPositionValue;
                    this.hiddenBtnRt.sizeDelta          = targetMaskSize;
                }
                else
                {
                    this.popup.PopupRt.anchoredPosition = this.popupMoveSettings.UpdateSpring(ref this.popupMoveSpring, Vector2.zero);
                }

                var newColorV = this.bgColorSettings.UpdateSpring(ref this.bgColorSpring, this.currentBgColor.ToVector4());
                this.bg.color = newColorV.ToColor();
            }
        }

        private async void Next()
        {
            if (this.debug) print("Next Tutorial Step");
            this.currentStep++;
            if (this.steps == null || this.currentStep >= this.steps.Count)
            {
                this.HideTutorial();

                return;
            }

            var s = this.steps[this.currentStep];
            this.popup.SetStep(s);
            this.currentTarget = null;

            if (s.target_obj != "") this.currentTarget = GameObject.Find(s.target_obj);
            //if (lastFocusedObject != null)
            //{
            //lastFocusedObject.transform.SetParent(lastFocusedObjectParent);
            //lastFocusedObject.transform.SetSiblingIndex(lastFocusedObjectSibling);
            //}
            var differentObject = this.currentTarget != this.lastFocusedObject;

            if (this.currentTarget != null)
            {
                this.lastFocusedObject        = this.currentTarget;
                this.lastFocusedObjectParent  = this.currentTarget.transform.parent;
                this.lastFocusedObjectSibling = this.currentTarget.transform.GetSiblingIndex();

                var rect2D = this.GetObjectCanvasRect(this.currentTarget);

                var size = Mathf.Max(rect2D.width, rect2D.height);
                var dir  = new Vector3(rect2D.position.x, rect2D.position.y, 0) - this.mask.anchoredPosition3D;

                Debug.DrawRay(this.mask.position, dir, Color.yellow, 10);
                this.mask.rotation = Quaternion.LookRotation(this.mask.forward, dir);

                this.currentEndPositionValue      = rect2D.position;
                this.currentEndPositionChangeTime = differentObject ? Time.time : Time.time - this.size1Duration;

                this.currentEndSizeValue = size;
                var rt  = this.currentTarget.GetComponent<RectTransform>();
                var btn = this.currentTarget.GetComponent<Button>();
                this.hiddenBtn.onClick.RemoveAllListeners();

                if (s.isClickable && rt != null && btn != null)
                {
                    this.hiddenBtn.gameObject.SetActive(true);
                    this.hiddenBtn.onClick.AddListener(btn.onClick.Invoke);
                }
                else
                {
                    this.hiddenBtn.gameObject.SetActive(false);
                }

                if (s is { isClickable: true, isJoyStickMove: true } && this.hiddenJoystickMove != null)
                {
                    this.hiddenBtn.gameObject.SetActive(false);
                    this.hiddenJoystickMove.gameObject.SetActive(true);
                    if (s.isReplacingNextButton)
                    {
                        this.hiddenJoystickMove.OnJoystickPressed.AddListener(async (pressed) =>
                        {
                            if (!pressed) return;
                            
                            await UniTask.Delay(1000);
                            this.popup.nextBtn.onClick.Invoke();
                            this.hiddenJoystickMove.gameObject.SetActive(false);
                        });
                    }
                }
                else
                {
                    this.hiddenJoystickMove.gameObject.SetActive(false);
                }

                if (s is { isClickable: true, isJoyStickRotate: true } && this.hiddenJoystickRotate != null)
                {
                    this.hiddenBtn.gameObject.SetActive(false);
                    this.hiddenJoystickRotate.gameObject.SetActive(true);
                    if (s.isReplacingNextButton)
                    {
                        this.hiddenJoystickRotate.OnJoystickPressed.AddListener(async (pressed) =>
                        {
                            if (!pressed) return;

                            await UniTask.Delay(500);
                            this.popup.nextBtn.onClick.Invoke();
                            this.hiddenJoystickRotate.gameObject.SetActive(false);
                        });
                    }
                }
                else
                {
                    this.hiddenJoystickRotate.gameObject.SetActive(false);
                }

                if (s.isReplacingNextButton && rt != null && btn != null)
                {
                    this.popup.nextBtn.gameObject.SetActive(false);

                    this.hiddenBtn.onClick.AddListener(this.popup.nextBtn.onClick.Invoke);
                }
                else
                {
                    this.popup.nextBtn.gameObject.SetActive(true);
                }

                TutorialEvents.OnTutorialFocus?.Invoke(s.target_obj, rect2D.center);
                TutorialEvents.OnTutorialProgress?.Invoke(this.currentStep, this.steps.Count);
            }
            else if (!this.retried && !string.IsNullOrEmpty(s.target_obj))
            {
                this.retried = true;
                if (this.debug) Debug.Log("RETRYING CANNOT FIND " + s.target_obj);
                // Retry in .2f seconds
                this.currentStep--;
                this.Invoke("Next", .2f);
            }
            else
            {
                if (!string.IsNullOrEmpty(s.target_obj) && this.debug)
                    Debug.Log("CANNOT FIND " + s.target_obj);

                this.currentEndPositionValue      = Vector2.zero;
                this.currentEndPositionChangeTime = Time.time;
                this.popup.nextBtn.gameObject.SetActive(true);
                this.hiddenBtn.gameObject.SetActive(false);
                this.mask.sizeDelta        = Vector2.zero;
                this.mask.anchoredPosition = Vector2.zero;
                TutorialEvents.OnTutorialProgress?.Invoke(this.currentStep, this.steps.Count);
            }

            this.currentBgColor = this.currentStep == 0 ? this.initBGColor : this.tutoBGColor;
        }

        private void HideTutorial()
        {
            this.popup.OnClickNext.RemoveAllListeners();

            tutorialState = State.FadIngOut;
        }

        /**
        * Get Object's bounds in the Canvas space
        **/
        private Rect GetObjectCanvasRect(GameObject obj)
        {
            var objRect     = obj.GetComponent<RectTransform>();
            var objRenderer = obj.GetComponent<Renderer>();
            var objCollider = obj.GetComponentInChildren<Collider>();

            var newRect = new Rect();
            if (objRect != null)
            {
                newRect = objRect.GetRectFromOtherParent(this.mask.parent as RectTransform);
            }

            else if (objRenderer != null)
            {
                newRect = objRenderer.GetScreenRect(Camera.main);
                newRect = (this.mask.parent as RectTransform).ScreenRectToRectTransform(newRect);
            }
            else if (objCollider != null)
            {
                newRect = objCollider.GetScreenRect(Camera.main);
                newRect = (this.mask.parent as RectTransform).ScreenRectToRectTransform(newRect);
            }

            return newRect;
        }
    }
}