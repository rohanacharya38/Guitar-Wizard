
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class drag_and_drop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    RectTransform circleTransform;
    public RectTransform dustbinTransform;
    public audioCompare comparerScript;
    public CanvasGroup comparer;
    public Canvas canvas;
    Vector2 initialPosition;
    public Image fillImage;
    Color initialColor;
    Slider thisSlider;
    // Start is called before the first frame update
    void Start()
    {
        circleTransform = GetComponent<RectTransform>();
        initialPosition = circleTransform.anchoredPosition;
        initialColor = fillImage.color;
        thisSlider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().interactable = false;
        comparer.interactable = false;
        comparer.blocksRaycasts = false;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        circleTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        if (RectTransformUtility.RectangleContainsScreenPoint(dustbinTransform, circleTransform.transform.position))
        {
            thisSlider.value = 1;
            fillImage.color = Color.red;
        }
        else
        {
            thisSlider.value = 0;
            fillImage.color = initialColor;
        }

    }
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (fillImage.color == Color.red)
        {

            if (name == "Button-1")
            {
                comparerScript.resetOne();

            }
            else
            {

                comparerScript.resetTwo();
            }
        }
        thisSlider.value = 0;
        fillImage.color = initialColor;

        circleTransform.anchoredPosition = initialPosition;
        comparer.interactable = true;
        comparer.blocksRaycasts = true;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        GetComponent<CanvasGroup>().interactable = true;
        
    }
}
