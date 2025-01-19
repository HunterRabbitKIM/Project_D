using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 originScale;

    [SerializeField]
    private float hoverScale = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOScale(originScale * hoverScale, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOScale(originScale, 0.2f);
    }
}
