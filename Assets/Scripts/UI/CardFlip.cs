using DG.Tweening;
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


}
