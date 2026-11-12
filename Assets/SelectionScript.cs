using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SelectionScript : MonoBehaviour
{
    private GameController gameController;

    public GameObject _selectionPanel;

    [Header("--- REFERENCIAS DE IMÁGENES EN UI (LOS CUADROS DE LUNAS) ---")]
    [Tooltip("Arrastra aquí los objetos Image que representan las lunas en la UI")]
    public List<Image> moonDisplayImages = new List<Image>();

    [Header("--- CONFIGURACIÓN DE COLORES PARA LAS LUNAS ---")]
    public Color bloodMoonColor = Color.red;      // Para Blood Moon
    public Color giantMoonColor = Color.yellow;   // Para Giant Moon
    public Color halfmoonColor = Color.magenta;   // Para Halfmoon (Estrella partida)

    [Header("--- OPCIÓN 1 DE PERSONAJE ---")]
    public Button characterButton1;
    public TextMeshProUGUI characterText1;

    [Header("--- OPCIÓN 2 DE PERSONAJE ---")]
    public Button characterButton2;
    public TextMeshProUGUI characterText2;

    [Header("--- REGISTRO DE IDs ---")]
    public List<int> _mahouShoujoId = new List<int>();

    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();
    }

    private void Start()
    {
        UpdateMoonDisplay();
        SetupRandomCharacterChoices();
    }

    public void UpdateMoonDisplay()
    {
        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null) return;
        }

        GameController.MoonType activeMoon = gameController.activeMoon;

        if (activeMoon == GameController.MoonType.None)
        {
            Debug.Log("[SELECCIÓN] No hay lunas activas en esta partida.");
            foreach (var img in moonDisplayImages)
            {
                if (img != null) img.gameObject.SetActive(false);
            }
            return;
        }

        // Mostramos la luna activa en el primer cuadro disponible de la UI
        for (int i = 0; i < moonDisplayImages.Count; i++)
        {
            if (moonDisplayImages[i] != null)
            {
                if (i == 0)
                {
                    moonDisplayImages[i].gameObject.SetActive(true);
                    moonDisplayImages[i].color = GetMoonVisualColor(activeMoon);
                }
                else
                {
                    moonDisplayImages[i].gameObject.SetActive(false); // Ocultamos los demás si solo hay una activa
                }
            }
        }

        Debug.Log($"[SELECCIÓN] Luna activa mostrada en UI: {activeMoon}");
    }

    private Color GetMoonVisualColor(GameController.MoonType moon)
    {
        switch (moon)
        {
            case GameController.MoonType.BloodMoon: return bloodMoonColor;
            case GameController.MoonType.GiantMoon: return giantMoonColor;
            case GameController.MoonType.Halfmoon: return halfmoonColor;
            default: return Color.white;
        }
    }

    public void SetupRandomCharacterChoices()
    {
        if (gameController == null) return;

        List<MahouShoujoData> choices = gameController.GetRandomMahouShoujoChoices();
        if (choices == null || choices.Count == 0) return;

        _mahouShoujoId.Clear();

        if (choices.Count > 0 && choices[0] != null)
        {
            MahouShoujoData char1 = choices[0];
            int index1 = gameController.allAvailableMagicalGirls.IndexOf(char1);
            _mahouShoujoId.Add(index1);

            if (characterButton1 != null)
            {
                characterButton1.gameObject.SetActive(true);
                Image btnImage1 = characterButton1.GetComponent<Image>();
                if (btnImage1 != null && char1.artwork != null) btnImage1.sprite = char1.artwork;
                if (characterText1 != null) characterText1.text = char1.cardName;

                characterButton1.onClick.RemoveAllListeners();
                characterButton1.onClick.AddListener(() => MahouShoujoSelected(index1));
            }
        }
        else if (characterButton1 != null) characterButton1.gameObject.SetActive(false);

        if (choices.Count > 1 && choices[1] != null)
        {
            MahouShoujoData char2 = choices[1];
            int index2 = gameController.allAvailableMagicalGirls.IndexOf(char2);
            _mahouShoujoId.Add(index2);

            if (characterButton2 != null)
            {
                characterButton2.gameObject.SetActive(true);
                Image btnImage2 = characterButton2.GetComponent<Image>();
                if (btnImage2 != null && char2.artwork != null) btnImage2.sprite = char2.artwork;
                if (characterText2 != null) characterText2.text = char2.cardName;

                characterButton2.onClick.RemoveAllListeners();
                characterButton2.onClick.AddListener(() => MahouShoujoSelected(index2));
            }
        }
        else if (characterButton2 != null) characterButton2.gameObject.SetActive(false);
    }

    public void MahouShoujoSelected(int id)
    {
        if (gameController != null)
        {
            if (id >= 0 && id < gameController.allAvailableMagicalGirls.Count)
            {
                MahouShoujoData selectedCharacter = gameController.allAvailableMagicalGirls[id];
                gameController.SelectPlayerCharacter(selectedCharacter);

                var stageCtrl = GetComponent<StageController>();
                if (stageCtrl != null)
                {
                    if (stageCtrl._mahouShoujoPlayer != null)
                        stageCtrl._mahouShoujoPlayer.GetComponent<Image>().sprite = selectedCharacter.artwork;

                    if (stageCtrl._mainEnemy != null && stageCtrl._enemy != null)
                    {
                        stageCtrl._mainEnemy.GetComponent<Image>().sprite = stageCtrl._enemy.artwork;
                        stageCtrl._mainEnemy.GetComponent<Image>().color = Color.white;
                    }
                }

                if (_selectionPanel != null) _selectionPanel.SetActive(false);

                var handCtrl = GetComponent<HandController>();
                if (handCtrl != null && handCtrl._stageAnimator != null)
                {
                    handCtrl._stageAnimator.SetBool("GameStarts", true);
                }
            }
        }
    }
}