using UnityEngine;
using UnityEngine.UI;

public class IconManager : MonoBehaviour
{
    [Header("Icon Properties")]
    [SerializeField] private RawImage[] icons;
    [SerializeField] private Color activeEmissionColor = Color.white;
    [SerializeField] private Color inactiveEmissionColor = Color.black;

    [Header("Animation Properties")]
    [SerializeField] private Vector3 hiddenPositionOffset = new Vector3(-100, 0, 0);
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float displayDuration = 2f;

    private int currentIconIndex = -1;
    private bool iconsAreVisible = false;
    private float iconDisplayTimer = 0f;

    private void Start()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            SetIconEmission(i, inactiveEmissionColor);
            icons[i].rectTransform.position += hiddenPositionOffset;
        }

        SetIconActive(0);
    }

    private void Update()
    {
        if (iconsAreVisible)
        {
            iconDisplayTimer += Time.deltaTime;
            if (iconDisplayTimer >= displayDuration)
            {
                SlideIconsOut();
            }
        }
    }

    public void SetIconActive(int index)
    {
        if (!iconsAreVisible)
        {
            SlideIconsIn();
        }

        if (currentIconIndex >= 0 && currentIconIndex < icons.Length)
        {
            SetIconEmission(currentIconIndex, inactiveEmissionColor);
        }

        SetIconEmission(index, activeEmissionColor);
        currentIconIndex = index;
        iconDisplayTimer = 0f;
    }

    private void SlideIconsIn()
    {
        iconsAreVisible = true;
        StartCoroutine(SlideIconsToPosition(0f));
    }

    private void SlideIconsOut()
    {
        iconsAreVisible = false;
        StartCoroutine(SlideIconsToPosition(hiddenPositionOffset.x));
    }

    private System.Collections.IEnumerator SlideIconsToPosition(float targetXAnchoredPosition)
    {
        float elapsedTime = 0f;
        Vector2[] startAnchoredPositions = new Vector2[icons.Length];

        for (int i = 0; i < icons.Length; i++)
        {
            startAnchoredPositions[i] = icons[i].rectTransform.anchoredPosition;
        }

        while (elapsedTime < slideDuration)
        {
            elapsedTime += Time.deltaTime;
            for (int i = 0; i < icons.Length; i++)
            {
                float newX = Mathf.Lerp(startAnchoredPositions[i].x, targetXAnchoredPosition, elapsedTime / slideDuration);
                icons[i].rectTransform.anchoredPosition = new Vector2(newX, startAnchoredPositions[i].y);
            }
            yield return null;
        }
    }


    private void SetIconEmission(int index, Color emissionColor)
    {
        if (index >= 0 && index < icons.Length)
        {
            Material iconMaterial = icons[index].materialForRendering;
            iconMaterial.SetColor("_EmissionColor", emissionColor);
            icons[index].material = iconMaterial;
        }
    }
}
