using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DVAH{
public class NoAdPopUp : MonoBehaviour
{
    [SerializeField] float _speed = 10;
    CanvasGroup _canvasGroup;

    private void Awake() {
        _canvasGroup = this.GetComponent<CanvasGroup>();
    }
    private void OnEnable() {
        this.transform.localPosition = Vector2.zero;
        this.transform.localScale = Vector2.one;
        _canvasGroup.alpha = 1;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(Vector2.up * _speed * Time.deltaTime);
        _canvasGroup.alpha -= 0.7f * Time.deltaTime;
        if(_canvasGroup.alpha <= 0)
            this.gameObject.SetActive(false);
    }
}
}
