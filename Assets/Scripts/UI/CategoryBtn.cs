using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CategoryBtn : MonoBehaviour, IPointerDownHandler
{
    public string categoryName = "";
    public bool isSelected = true;

    public Vector3 selectedScale = Vector3.one;
    Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;

        if (isSelected)
        {
            transform.localScale = selectedScale;
            WordManager.instance.AddCategory(categoryName);
            gameObject.GetComponentInChildren<Image>().color = Color.green;
        }
        else
        {
            transform.localScale = originalScale;
            WordManager.instance.RemoveCategory(categoryName);
            gameObject.GetComponentInChildren<Image>().color = Color.red;

        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameObject.GetComponentInParent<CategoryPanelUI>().GetTheCountOfCategoriesSelected() > 1 || !isSelected)
        {
            isSelected = !isSelected;
        }

        if (isSelected)
        {
            transform.localScale = selectedScale;
            WordManager.instance.AddCategory(categoryName);
            gameObject.GetComponentInChildren<Image>().color = Color.green;
        }
        else
        {
            transform.localScale = originalScale;
            WordManager.instance.RemoveCategory(categoryName);
            gameObject.GetComponentInChildren<Image>().color = Color.red;

        }
 
    }
}
