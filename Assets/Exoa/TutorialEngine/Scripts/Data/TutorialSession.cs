using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class TutorialSession
{
    [System.Serializable]
    public struct TutorialStep
    {
        public string target_obj;
        public string text;
        public bool   isClickable;
        public bool   isJoyStickMove;
        public bool   isJoyStickRotate;
        public bool   isReplacingNextButton;
    }

    public TutorialStep[] steps;
}