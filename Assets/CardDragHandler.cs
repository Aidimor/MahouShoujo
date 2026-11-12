using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Transform originalParent;

    private Coroutine returnCoroutine;
    private StageController stageController;

    private bool isDragging = false;
    private Vector2 targetAnchoredPosition;
    private bool isCardLocked = false;

    [HideInInspector] public HandController.PlayerDeckEntry cardEntryReference;

    public float dragFollowSpeed = 20f;
    public float rotationSmoothSpeed = 15f;
    public float dropRadius = 150f; // Radio de distancia para detectar los slots de la mesa
    public float targetScaleMultiplier = 1.25f;
    [Range(0f, 1f)] public float targetAlpha = 0.5f;
    public float smoothSpeed = 10f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        stageController = Object.FindObjectOfType<StageController>();
        isCardLocked = false;
    }

    private void Update()
    {
        if (isDragging)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetAnchoredPosition, Time.deltaTime * dragFollowSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * rotationSmoothSpeed);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCardLocked) return;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
        originalScale = rectTransform.localScale;
        originalParent = transform.parent;

        Transform canvasTransform = canvas != null ? canvas.transform : originalParent;
        transform.SetParent(canvasTransform, true);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1f;

        isDragging = true;
        targetAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isCardLocked) return;

        if (canvas != null)
        {
            targetAnchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        else
        {
            targetAnchoredPosition += eventData.delta;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isCardLocked) return;

        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Comprobamos si se soltó en alguno de los slots de la mesa (_cardsPlayedPos)
        int nearestSlotIndex = GetNearestTableSlotIndex();

        if (nearestSlotIndex != -1)
        {
            isCardLocked = true;

            if (cardEntryReference != null && cardEntryReference.cardData != null)
            {
                NewCardData data = cardEntryReference.cardData;
                string levelDesc = data._levelData != null ? data._levelData.levelDescription : "Sin descripción";

                if (stageController != null)
                {
                    stageController.RegisterUsedCard(cardEntryReference.cardId, data.cardName, cardEntryReference.currenttypeId, levelDesc, Color.white, cardEntryReference.currentvalor);
                    stageController.PlaceCardOnTable(this, nearestSlotIndex);
                }
            }
        }
        else
        {
            Debug.Log("[CardDragHandler] Carta soltada fuera de la mesa. Regresando a la mano...");
            returnCoroutine = StartCoroutine(SmoothReturnToOriginal());
        }
    }

    /// <summary>
    /// Permite que el StageController devuelva la carta a la mano si su slot de mesa es reemplazado.
    /// </summary>
    public void ReturnToHand()
    {
        isCardLocked = false;
        gameObject.SetActive(true);
        if (returnCoroutine != null) StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(SmoothReturnToOriginal());
    }

    private int GetNearestTableSlotIndex()
    {
        if (stageController == null || stageController._cardsPlayedPos == null) return -1;

        float closestDistance = float.MaxValue;
        int selectedSlotIndex = -1;

        Camera cam = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera) ? canvas.worldCamera : null;
        Vector2 cardScreenPos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);

        for (int i = 0; i < stageController._cardsPlayedPos.Length; i++)
        {
            GameObject slotObj = stageController._cardsPlayedPos[i];
            if (slotObj != null)
            {
                RectTransform slotRect = slotObj.GetComponent<RectTransform>();
                if (slotRect != null)
                {
                    Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(cam, slotRect.position);
                    float distance = Vector2.Distance(cardScreenPos, slotScreenPos);

                    if (distance < closestDistance && distance <= dropRadius)
                    {
                        closestDistance = distance;
                        selectedSlotIndex = i;
                    }
                }
            }
        }

        return selectedSlotIndex;
    }

    private IEnumerator SmoothReturnToOriginal()
    {
        transform.SetParent(originalParent, true);

        Vector2 startPos = rectTransform.anchoredPosition;
        Quaternion startRot = rectTransform.localRotation;
        Vector3 startScale = rectTransform.localScale;
        float startAlpha = canvasGroup.alpha;

        float elapsedTime = 0f;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, t);
            rectTransform.localRotation = Quaternion.Slerp(startRot, originalRotation, t);
            rectTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
        returnCoroutine = null;
    }
}