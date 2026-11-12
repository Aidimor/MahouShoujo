using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CardShopController : MonoBehaviour
{
    public int _gold;
    public Transform _packParent;
    public GameObject _cardPrefab;
    public int _totalPacks;
    public TextMeshProUGUI _totalPacksText;
    public GameObject[] _cardsPackObject;
    public int _station;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {
        // Verificamos si el mouse existe y si se presionó el botón izquierdo en este frame
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            RemoveCards();
        }
        UpdatePacksText();
    }

    public void RemoveCards()
    {
        // Validamos que queden sobres disponibles
        if (_totalPacks > 0)
        {
            // Validamos que el array exista y que la estación actual sea válida
            if (_cardsPackObject != null && _station < _cardsPackObject.Length && _cardsPackObject[_station] != null)
            {
                // Desactivamos el sobre actual
                _cardsPackObject[_station].SetActive(false);

                // IMPORTANTE: Restamos un sobre del total disponible
              
               

                // Comprobamos si hemos llegado al final del array de sobres visuales
                // Usamos Length - 1 para que sea dinámico sin importar cuántos objetos tenga el array
                if (_station >= _cardsPackObject.Length - 1)
                {
                    _station = 0; // Reiniciamos la estación al principio
                    _totalPacks--;
                    // Reactivamos todos los sobres para un nuevo ciclo
                    for (int i = 0; i < _cardsPackObject.Length; i++)
                    {
                        if (_cardsPackObject[i] != null && _totalPacks > 0)
                        {
                            _cardsPackObject[i].SetActive(true);
                        }
                    }

                }
                else
                {
                    _station++; // Avanzamos a la siguiente estación
                }
            }
        }
        else
        {
            Debug.Log("¡No te quedan sobres por abrir! Compra más en la tienda.");
        }
    }

    private void UpdatePacksText()
    {
        if (_totalPacksText != null)
        {
            _totalPacksText.text = _totalPacks.ToString();
        }
    }
}