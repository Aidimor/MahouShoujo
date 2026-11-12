using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StageController : MonoBehaviour
{
    [Header("--- REFERENCIAS EN ESCENA ---")]
    public GameObject _mahouShoujoPlayer;
    public GameObject _mainEnemy;

    [Header("--- GESTIÓN DE MANÁ ---")]
    public int _manaPool = 0; // Tu variable de maná
    public TextMeshProUGUI _manaPoolText;

    [Header("--- POSICIONES DE CARTAS EN LA MESA ---")]
    [Tooltip("Array de exactamente 2 GameObjects que representan los slots donde se colocan las cartas jugadas")]
    public GameObject[] _cardsPlayedPos = new GameObject[2];

    private CardDragHandler[] activeTableSlots = new CardDragHandler[2];

    [System.Serializable]
    public class UsedCardDataEntry
    {
        public int cardId;
        public string cardName;
        public int typeId;
        public string levelDescription;
        public Color cardColor;
        public int cardValue;
    }

    [Header("--- HISTORIAL GENERAL DE CARTAS ---")]
    public List<UsedCardDataEntry> usedCardsHistory = new List<UsedCardDataEntry>();

    [Header("--- 🃏 LISTA NUEVA: CARTAS ACTIVAS EN LA MESA (Exactamente 2) ---")]
    public List<UsedCardDataEntry> activeTableCards = new List<UsedCardDataEntry>();

    [Header("--- DATOS DE ENEMIGOS (ScriptableObject) ---")]
    public EnemiesData _enemy;

    [System.Serializable]
    public class PlayerInfoOnGame
    {
        public string _name;
        public int _currentHp;
        public int _maxHp;
    }

    [Header("--- EQUIPO DE JUGADORES ---")]
    public List<PlayerInfoOnGame> _playerInfoOnGameList = new List<PlayerInfoOnGame>();

    [System.Serializable]
    public class EnemyInfoOnGame
    {
        public string _name;
        public int _currentHp;
        public int _maxHp;
    }

    [Header("--- OLEADA DE ENEMIGOS ---")]
    public List<EnemyInfoOnGame> _enemyInfoOnGameList = new List<EnemyInfoOnGame>();

    [Header("--- UI Y EFECTOS ---")]
    public TextMeshProUGUI _playerHPtext;
    public TextMeshProUGUI _enemyHPtext;
    public ParticleSystem[] _playerParticles;
    public ParticleSystem[] _enemyParticles;

    private void Start()
    {
        InitializeBattleEntities();

        // Al iniciar el primer turno, calculamos y aumentamos el maná inicial según las lunas
        IncreaseTurnMana();
    }

    private void Update()
    {
        if (_enemyInfoOnGameList != null && _enemyInfoOnGameList.Count > 0 && _enemyHPtext != null)
        {
            _enemyHPtext.text = _enemyInfoOnGameList[0]._currentHp.ToString() + "/" + _enemyInfoOnGameList[0]._maxHp.ToString();
        }

        if (_playerInfoOnGameList != null && _playerInfoOnGameList.Count > 0 && _playerHPtext != null)
        {
            _playerHPtext.text = _playerInfoOnGameList[0]._currentHp.ToString() + "/" + _playerInfoOnGameList[0]._maxHp.ToString();
        }

        _manaPoolText.text = _manaPool.ToString();
    }

    /// <summary>
    /// Calcula el incremento de maná en base a las lunas activas y la Giant Moon.
    /// </summary>
    public void IncreaseTurnMana()
    {
        int moonCount = 0;

        if (GameController.Instance != null && GameController.Instance.activeMoon != GameController.MoonType.None)
        {
            // Si hay alguna luna activa, por defecto cuenta como 1 luna base
            moonCount = 1;

            // Si la luna activa es la GiantMoon (la que da maná), vale por dos
            if (GameController.Instance.activeMoon == GameController.MoonType.GiantMoon)
            {
                moonCount *= 2;
                Debug.Log("[MANÁ] 🌕 ¡Giant Moon activa! El valor de incremento de maná se duplica.");
            }
        }

        // Aumentamos el _manaPool por x (siendo x el número de lunas en juego o su valor modificado)
        _manaPool += moonCount;
        Debug.Log($"[MANÁ] 🔮 Turno iniciado. _manaPool incrementado en +{moonCount}. Total actual: {_manaPool}");
    }

    public void InitializeBattleEntities()
    {
        _playerInfoOnGameList.Clear();

        if (_mahouShoujoPlayer != null)
        {
            string playerName = "Mahou Shoujo";
            int playerHp = 100;

            if (GameController.Instance != null && GameController.Instance.playerCharacter != null)
            {
                playerName = GameController.Instance.playerCharacter.name;
                //playerHp = GameController.Instance.playerCharacter._hp;
            }

            PlayerInfoOnGame newPlayerInfo = new PlayerInfoOnGame
            {
                _name = playerName,
                _maxHp = playerHp,
                _currentHp = playerHp
            };

            _playerInfoOnGameList.Add(newPlayerInfo);
        }

        _enemyInfoOnGameList.Clear();

        if (_enemy != null)
        {
            EnemyInfoOnGame newEnemyInfo = new EnemyInfoOnGame
            {
                _name = _enemy.cardName,
                _maxHp = _enemy._hp,
                _currentHp = _enemy._hp
            };

            _enemyInfoOnGameList.Add(newEnemyInfo);
        }
        else if (_mainEnemy != null)
        {
            EnemyInfoOnGame newEnemyInfo = new EnemyInfoOnGame
            {
                _name = _mainEnemy.name,
                _maxHp = 50,
                _currentHp = 50
            };

            _enemyInfoOnGameList.Add(newEnemyInfo);
        }
    }

    public bool PlaceCardOnTable(CardDragHandler droppedCard, int targetSlotIndex)
    {
        if (_cardsPlayedPos == null || targetSlotIndex < 0 || targetSlotIndex >= _cardsPlayedPos.Length) return false;

        if (activeTableSlots == null || activeTableSlots.Length != _cardsPlayedPos.Length)
        {
            activeTableSlots = new CardDragHandler[_cardsPlayedPos.Length];
        }

        if (activeTableSlots[targetSlotIndex] != null)
        {
            CardDragHandler oldCard = activeTableSlots[targetSlotIndex];
            if (oldCard != null)
            {
                Debug.Log($"[StageController] El slot {targetSlotIndex} fue reemplazado. Devolviendo carta anterior a la mano.");
                oldCard.ReturnToHand();
            }
        }

        activeTableSlots[targetSlotIndex] = droppedCard;

        if (_cardsPlayedPos[targetSlotIndex] != null)
        {
            Transform targetTransform = _cardsPlayedPos[targetSlotIndex].transform;
            droppedCard.transform.SetParent(targetTransform, true);

            RectTransform cardRect = droppedCard.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = Vector2.zero;
                cardRect.localRotation = Quaternion.identity;
            }
        }

        RebuildActiveTableCardsList();

        return true;
    }

    private void RebuildActiveTableCardsList()
    {
        activeTableCards.Clear();

        foreach (var slotCard in activeTableSlots)
        {
            if (slotCard != null && slotCard.cardEntryReference != null && slotCard.gameObject.activeInHierarchy)
            {
                var entry = slotCard.cardEntryReference;
                NewCardData data = entry.cardData;

                UsedCardDataEntry tableEntry = new UsedCardDataEntry
                {
                    cardId = entry.cardId,
                    cardName = data != null ? data.cardName : "Desconocida",
                    typeId = entry.currenttypeId,
                    levelDescription = (data != null && data._levelData != null) ? data._levelData.levelDescription : "Sin descripción",
                    cardColor = Color.white,
                    cardValue = entry.currentvalor
                };

                activeTableCards.Add(tableEntry);
            }
        }

        if (activeTableCards.Count >= 2)
        {
            Debug.Log($"⚠️ [STAGE CONTROLLER] ¡YA HAY 2 CARTAS PUESTAS EN LA MESA!\n" +
                      $"1. [{activeTableCards[0].cardName}] (Valor: {activeTableCards[0].cardValue})\n" +
                      $"2. [{activeTableCards[1].cardName}] (Valor: {activeTableCards[1].cardValue})");

            int attack = 0;

            if (activeTableCards != null && activeTableCards.Count >= 2)
            {
                foreach (var card in activeTableCards)
                {
                    if (card.typeId == 0) // PowerUp / Ataque
                    {
                        int cardValueToAdd = card.cardValue;

                        // --- EFECTO DE LA BLOOD MOON: Potencia los Power-Ups ---
                        if (GameController.Instance != null && GameController.Instance.activeMoon == GameController.MoonType.BloodMoon)
                        {
                            cardValueToAdd = Mathf.RoundToInt(cardValueToAdd * 1.5f);
                            Debug.Log($"[BLOOD MOON] 🩸 ¡Potenciado ataque {card.cardName} a {cardValueToAdd}!");
                        }

                        attack += cardValueToAdd;
                    }
                }

                // --- EFECTO DE LA HALFMOON: Eco Temporal (Duplica la última carta) ---
                if (GameController.Instance != null && GameController.Instance.activeMoon == GameController.MoonType.Halfmoon && activeTableCards.Count >= 2)
                {
                    int lastCardValue = activeTableCards[1].typeId == 0 ? activeTableCards[1].cardValue : 0;
                    attack += lastCardValue;
                    Debug.Log($"[HALFMOON] 🌗 ¡Eco Temporal! Suma +{lastCardValue} extra.");
                }
            }

            HandController handCtrl = FindObjectOfType<HandController>();
            StartCoroutine(AttackNumerator(attack));
            if (handCtrl != null)
            {
                handCtrl.NotifyCardPlayed();
            }
        }
    }

    public void RegisterUsedCard(int cardId, string cardName, int typeId, string levelDescription, Color cardColor, int cardValue = 0)
    {
        UsedCardDataEntry newEntry = new UsedCardDataEntry
        {
            cardId = cardId,
            cardName = cardName,
            typeId = typeId,
            levelDescription = levelDescription,
            cardColor = cardColor,
            cardValue = cardValue
        };

        usedCardsHistory.Add(newEntry);
    }

    public IEnumerator AttackNumerator(int baseDamage)
    {
        int finalDamage = baseDamage;
        Debug.Log("Attack = " + finalDamage.ToString());
        if (finalDamage > 0)
        {
            if (_playerParticles != null && _playerParticles.Length > 0 && _playerParticles[0] != null)
            {
                _playerParticles[0].Play();
            }
            yield return new WaitForSeconds(0.5f);

            if (_enemyInfoOnGameList != null && _enemyInfoOnGameList.Count > 0)
            {
                _enemyInfoOnGameList[0]._currentHp -= finalDamage;
                if (_enemyInfoOnGameList[0]._currentHp < 0)
                {
                    _enemyInfoOnGameList[0]._currentHp = 0;
                }

                Debug.Log($"[StageController] ¡Ataque ejecutado! Daño: {finalDamage}. HP Enemigo: {_enemyInfoOnGameList[0]._currentHp}");
            }
        }

        yield return StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        int enemyDamage = (_enemy != null) ? _enemy._attack : 10;

        if (_enemyParticles != null && _enemyParticles.Length > 0 && _enemyParticles[0] != null)
        {
            _enemyParticles[0].Play();
        }

        yield return new WaitForSeconds(0.4f);

        if (_playerInfoOnGameList != null && _playerInfoOnGameList.Count > 0)
        {
            _playerInfoOnGameList[0]._currentHp -= enemyDamage;
            if (_playerInfoOnGameList[0]._currentHp < 0)
            {
                _playerInfoOnGameList[0]._currentHp = 0;
            }

            Debug.Log($"👾 [TURNO ENEMIGO] ¡El enemigo atacó a la Mahou Shoujo! Daño: {enemyDamage}. HP Restante Jugador: {_playerInfoOnGameList[0]._currentHp}");
        }

        yield return new WaitForSeconds(0.5f);

        // --- RESETEO DE LA MESA ---
        for (int i = 0; i < activeTableSlots.Length; i++)
        {
            if (activeTableSlots[i] != null)
            {
                Destroy(activeTableSlots[i].gameObject);
                activeTableSlots[i] = null;
            }
        }

        activeTableCards.Clear();

        // --- ORDENAMOS AL HAND CONTROLLER QUE RECIQUE Y CREE LA NUEVA MANO ---
        HandController handCtrl = FindObjectOfType<HandController>();
        if (handCtrl != null)
        {
            handCtrl.PrepareNextTurnHand();
        }

        // --- AUMENTAMOS EL MANÁ AL INICIAR EL NUEVO TURNO ---
        IncreaseTurnMana();

        Debug.Log("🔄 [SISTEMA] Turno finalizado. Mesa reseteada, nueva mano creada y maná actualizado.");
    }
}