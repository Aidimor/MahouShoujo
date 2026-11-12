using UnityEngine;

public class ReferenceScript : MonoBehaviour
{
    [Header("Referencias")]
    public HandController _scriptHand;

 
    public void ActivateHandTrue()
    {
        if (_scriptHand != null && _scriptHand._handParent != null)
        {
            _scriptHand._handParent.SetActive(true);
            Debug.Log("[ANIMATION EVENT] Mano activada (True)");
        }
    }

    // Función para desactivar la mano
    public void ActivateHandFalse()
    {
        if (_scriptHand != null && _scriptHand._handParent != null)
        {
            _scriptHand._handParent.SetActive(false);
            Debug.Log("[ANIMATION EVENT] Mano desactivada (False)");
        }
    }
}