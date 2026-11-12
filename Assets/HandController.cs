using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HandController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerDeckEntry
    {
        public int cardId;
        public NewCardData cardData;
        public int currentLevel = 1;
        public int currenttypeId;
        public int currentvalor;
    }

    [System.Serializable]
    public class HandCardObjectPair
    {
        public PlayerDeckEntry deckEntry;
        public GameObject cardGameObject;

        public HandCardObjectPair(PlayerDeckEntry entry, GameObject obj)
        {
            deckEntry = entry;
            cardGameObject = obj;
        }
    }

    [Header("--- CONFIGURACIÓN DE PREFAB Y CONTENEDOR ---")]
    public GameObject cardPrefab;
    public Transform handContainer;

    [Header("--- GESTIÓN DE MAZO, MANO Y DESCARTES ---")]
    public List<PlayerDeckEntry> playerDeck = new List<PlayerDeckEntry>();
    public List<PlayerDeckEntry> currentHandData = new List<PlayerDeckEntry>();
    public List<HandCardObjectPair> activeHandObjects = new List<HandCardObjectPair>();
    public List<PlayerDeckEntry> discardPile = new List<PlayerDeckEntry>();

    [Header("--- CONFIGURACIÓN DE PARTIDA ---")]
    public int deckSizeToGenerate = 15;
    public int cardsPerHand = 4;       // Mano de exactamente 4 cartas

    [Header("--- CONTROL DE TURNO ---")]
    public int cardsRequiredToPlay = 2;
    private int cardsPlayedThisTurn = 0;

    [Header("--- REGISTRO Y CONTENEDORES ---")]
    public GameObject _handParent;
    public Animator _stageAnimator;

    [Header("--- CONFIGURACIÓN DE DISEÑO EN ARCO (MANO) ---")]
    public float spacingX = 140f;
    public float arcHeight = 35f;
    public float maxRotationAngle = 12f;

    private void Start()
    {
        TryLoadDeck();
    }

    private void Update()
    {
        if (playerDeck == null || (playerDeck.Count == 0 && currentHandData.Count == 0 && discardPile.Count == 0))
        {
            TryLoadDeck();
        }
    }

    [ContextMenu("Forzar Carga de Mazo Aleatorio")]
    public void TryLoadDeck()
    {
        GameController gameController = GameController.Instance != null ? GameController.Instance : FindObjectOfType<GameController>();

        if (gameController != null && gameController.Allcards != null && gameController.Allcards.Count > 0)
        {
            List<NewCardData> poolCards = gameController.Allcards;
            playerDeck.Clear();
            currentHandData.Clear();
            ClearActiveHandObjects();
            discardPile.Clear();
            int idCounter = 1;

            for (int i = 0; i < deckSizeToGenerate; i++)
            {
                NewCardData randomCard = poolCards[Random.Range(0, poolCards.Count)];

                if (randomCard != null)
                {
                    int initialVal = 0;
                    if (randomCard._levelData != null)
                    {
                        initialVal = randomCard._levelData._scaledValue;
                    }

                    int mappedTypeId = 0;
                    switch (randomCard.cardType)
                    {
                        case NewCardType.PowerUp: mappedTypeId = 0; break;
                        case NewCardType.DefenseUp: mappedTypeId = 1; break;
                        case NewCardType.Weapon: mappedTypeId = 2; break;
                        //case NewCardType.Effect: mappedTypeId = 3; break;
                        //case NewCardType.MagicalGirl: mappedTypeId = 4; break;
                    }

                    PlayerDeckEntry newEntry = new PlayerDeckEntry
                    {
                        cardId = idCounter,
                        cardData = randomCard,
                        currentLevel = 1,
                        currenttypeId = mappedTypeId,
                        currentvalor = initialVal
                    };

                    playerDeck.Add(newEntry);
                    idCounter++;
                }
            }

            ShuffleDeck();
            cardsPlayedThisTurn = 0;
            DealNewHand();
        }
        else
        {
            Debug.LogWarning("[HandController] No se encontró GameController o la lista Allcards está vacía.");
        }
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < playerDeck.Count; i++)
        {
            PlayerDeckEntry temp = playerDeck[i];
            int randomIndex = Random.Range(i, playerDeck.Count);
            playerDeck[i] = playerDeck[randomIndex];
            playerDeck[randomIndex] = temp;
        }
        Debug.Log("🔀 [MAZO] Mazo barajado.");
    }

    private void ClearActiveHandObjects()
    {
        foreach (var pair in activeHandObjects)
        {
            if (pair != null && pair.cardGameObject != null)
            {
                Destroy(pair.cardGameObject);
            }
        }
        activeHandObjects.Clear();
        currentHandData.Clear();

        if (handContainer != null)
        {
            foreach (Transform child in handContainer)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void DealNewHand()
    {
        ClearActiveHandObjects();

        for (int i = 0; i < cardsPerHand; i++)
        {
            if (playerDeck.Count == 0)
            {
                Debug.Log("⚠️ [MAZO] Mazo vacío al robar. Reciclando descartes...");
                foreach (var card in discardPile)
                {
                    playerDeck.Add(card);
                }
                discardPile.Clear();
                ShuffleDeck();
            }

            if (playerDeck.Count > 0)
            {
                PlayerDeckEntry drawnCard = playerDeck[0];
                currentHandData.Add(drawnCard);
                playerDeck.RemoveAt(0);
            }
        }

        cardsPlayedThisTurn = 0;
        UpdateHandUI();
    }

    private void UpdateHandUI()
    {
        if (cardPrefab == null || handContainer == null)
        {
            Debug.LogError("[HandController] ¡Falta asignar el cardPrefab o el handContainer!");
            return;
        }

        int totalCards = currentHandData.Count;
        if (totalCards == 0) return;

        for (int i = 0; i < totalCards; i++)
        {
            PlayerDeckEntry entry = currentHandData[i];
            if (entry != null && entry.cardData != null)
            {
                GameObject cardInstance = Instantiate(cardPrefab, handContainer);
                activeHandObjects.Add(new HandCardObjectPair(entry, cardInstance));

                NewCardData cardData = entry.cardData;

                Image cardImage = cardInstance.GetComponent<Image>();
                if (cardImage != null && cardData.artwork != null)
                {
                    cardImage.sprite = cardData.artwork;
                }

                string exactCardName = cardData.cardName;
                string exactLevelDesc = (cardData._levelData != null)
                    ? cardData._levelData.levelDescription + " " + cardData._levelData._scaledValue.ToString()
                    : "Sin descripción";

                // Obtenemos el costo de maná del ScriptableObject (asegúrate de que en NewCardData se llame _manaCost o ajústalo si tiene otro nombre)
                int manaCost = cardData._manaCost;

                CardHand cardHandScript = cardInstance.GetComponent<CardHand>();
                if (cardHandScript != null)
                {
                    cardHandScript._type = entry.currenttypeId;
                    // Pasamos el maná directamente al método de configuración
                    cardHandScript.SetupCardData(entry, exactCardName, exactLevelDesc, Color.white, manaCost);
                }

                CardDragHandler dragHandler = cardInstance.GetComponent<CardDragHandler>();
                if (dragHandler != null)
                {
                    dragHandler.cardEntryReference = entry;
                }

                RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    float normalizedX = (totalCards > 1) ? ((float)i / (totalCards - 1)) * 2f - 1f : 0f;
                    float posX = (i - (totalCards - 1) / 2f) * spacingX;
                    float posY = -Mathf.Pow(normalizedX, 2) * arcHeight + arcHeight;
                    float rotZ = -normalizedX * maxRotationAngle;

                    cardRect.anchoredPosition = new Vector2(posX, posY);
                    cardRect.localRotation = Quaternion.Euler(0f, 0f, rotZ);
                    cardRect.SetAsLastSibling();
                }
            }
        }

        Debug.Log($"[MANO] UI redibujada con éxito. Nuevas cartas en mano: {activeHandObjects.Count}");
    }

    public void NotifyCardPlayed()
    {
        cardsPlayedThisTurn++;
        Debug.Log($"[TURNO] Carta utilizada. Progreso: {cardsPlayedThisTurn} / {cardsRequiredToPlay}");
    }

    public void PrepareNextTurnHand()
    {
        cardsPlayedThisTurn = 0;

        foreach (var pair in activeHandObjects)
        {
            if (pair != null && pair.deckEntry != null)
            {
                discardPile.Add(pair.deckEntry);
            }
        }
        currentHandData.Clear();

        StageController stageCtrl = FindObjectOfType<StageController>();
        if (stageCtrl != null && stageCtrl.activeTableCards != null)
        {
            foreach (var tableEntry in stageCtrl.activeTableCards)
            {
                if (tableEntry != null)
                {
                    PlayerDeckEntry discCard = new PlayerDeckEntry
                    {
                        cardId = tableEntry.cardId,
                        currenttypeId = tableEntry.typeId,
                        currentvalor = tableEntry.cardValue
                    };
                    discardPile.Add(discCard);
                }
            }
        }

        Debug.Log("🧹 [TURNO] Limpiando mano anterior y mesa. Creando 4 cartas nuevas...");
        DealNewHand();
    }
}