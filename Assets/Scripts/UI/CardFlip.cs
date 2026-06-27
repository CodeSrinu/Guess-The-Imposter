using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardFlip : MonoBehaviour
{

    [SerializeField] private GameObject frontPart;
    [SerializeField] private GameObject backPart;

    private bool _isFlipped = false;
    [SerializeField] private float flipDuration = 0.5f;


    public void Flip()
    {
        float targetY = _isFlipped ? 0f : 180f;
        _isFlipped = !_isFlipped;

        transform.DORotate(targetY * Vector3.up, flipDuration).OnUpdate(() =>
        {
            float currentY = transform.localEulerAngles.y;

            if (currentY > 180f) currentY = Mathf.Abs(currentY - 360f);

            if (currentY > 90f)
            {
                if (frontPart.activeSelf) frontPart.SetActive(false);
                if (!backPart.activeSelf) backPart.SetActive(true);
            }
            else
            {
                if (!frontPart.activeSelf) frontPart.SetActive(true);
                if (backPart.activeSelf) backPart.SetActive(false);
            }
        });
    }

    public void SetWordText(string word)
    {
        TextMeshProUGUI txtComp = backPart.GetComponentInChildren<TextMeshProUGUI>();
        txtComp.text = word;
    }

    public void SetNameText(string name)
    {
        string txt = name != "" ? "Pass device to " + name : "";

        TextMeshProUGUI txtComp = frontPart.transform.Find("Player name").GetComponent<TextMeshProUGUI>();

        txtComp.gameObject.SetActive(true);
        txtComp.text = txt;
    }

    public void SetPlayerType(bool isImposter)
    {
        TextMeshProUGUI txtComp = backPart.transform.Find("Player Type").GetComponent<TextMeshProUGUI>();
        txtComp.text = isImposter ? "You are Imposter" : "You are Civilian"; 
    }

    public void ResetCard()
    {
        if (_isFlipped)
        {
            Flip();
        }

        TextMeshProUGUI txtComp = frontPart.transform.Find("Player name").GetComponent<TextMeshProUGUI>(); ;
        txtComp.gameObject.SetActive(false);
    }

}
