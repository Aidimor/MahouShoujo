using UnityEngine;
using TMPro;

public class CardHand : MonoBehaviour
{
    [Header("Referencias de Texto (Opcional: Arrastra aquí tus TextMeshPro)")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescText;

    [Header("Tipo de Carta (Mapeado)")]
    public int _type;
    public int _manaRequired;
    public GameObject _manaParent;
    public TextMeshProUGUI _manaText;

    /// <summary>
    /// Método llamado desde HandController para inyectar los datos visuales de la carta incluyendo el maná.
    /// </summary>
    public void SetupCardData(HandController.PlayerDeckEntry entry, string cardName, string levelDesc, Color cardColor, int manaCost)
    {
        _type = entry.currenttypeId;
        _manaRequired = manaCost;

        // 1. Si no arrastraste las referencias en el inspector, las buscamos automáticamente en los hijos
        if (cardNameText == null || cardDescText == null)
        {
            TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var txt in allTexts)
            {
                string objName = txt.gameObject.name.ToLower();
                if ((objName.Contains("name") || objName.Contains("title") || objName.Contains("nombre")) && cardNameText == null)
                {
                    cardNameText = txt;
                }
                else if ((objName.Contains("desc") || objName.Contains("text") || objName.Contains("descripcion")) && cardDescText == null)
                {
                    cardDescText = txt;
                }
            }

            // Fallback por orden si no tienen nombres específicos en los GameObjects
            if (cardNameText == null && allTexts.Length > 0) cardNameText = allTexts[0];
            if (cardDescText == null && allTexts.Length > 1) cardDescText = allTexts[1];
        }

        // 2. Asignamos los textos de forma segura
        if (cardNameText != null)
        {
            cardNameText.text = cardName;
        }

        if (cardDescText != null)
        {
            cardDescText.text = levelDesc;
        }

        // 3. Asignamos el costo de maná al texto correspondiente y activamos su contenedor si existe
        if (_manaText != null)
        {
            _manaText.text = manaCost.ToString();
        }

        if (_manaParent != null)
        {
            _manaParent.SetActive(true);
        }

        Debug.Log($"[CardHand] Carta configurada en UI -> Nombre: {cardName} | Maná: {manaCost}");
    }
}