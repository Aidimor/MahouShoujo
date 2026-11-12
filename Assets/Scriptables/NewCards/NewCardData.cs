using UnityEngine;

public enum NewCardType { PowerUp, DefenseUp, Weapon, Recollect }
public enum NewCardColor { Plata, Sangre, Cristal, Normal }

[CreateAssetMenu(fileName = "NewCardData", menuName = "New Card Game/New Card Data")]
public class NewCardData : ScriptableObject
{
    [Header("Información Básica")]
    public string cardName;
    public Sprite artwork;
    public NewCardType cardType;


    [Header("Coste de Orbes Lunares [Plata, Sangre, Cristal]")]
    [Tooltip("Índice 0: Orbes de Plata | Índice 1: Orbes de Sangre | Índice 2: Orbes de Cristal")]
    public int _manaCost;

    [Header("Progresión (3 Niveles)")]
    public NewCardLevelData _levelData;
}

[System.Serializable]
public class NewCardLevelData
{

    [Header("Información de Nivel")]
    [TextArea] public string levelDescription;
    public int _scaledValue;

  
}
