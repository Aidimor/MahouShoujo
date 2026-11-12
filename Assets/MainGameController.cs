using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    // Las 3 lunas oficiales del juego
    public enum MoonType { None, BloodMoon, GiantMoon, Halfmoon }

    [Header("--- DATOS GENERALES ---")]
    public List<NewCardData> Allcards = new List<NewCardData>();
    public List<MahouShoujoData> allAvailableMagicalGirls = new List<MahouShoujoData>();

    public MahouShoujoData playerCharacter;
    [HideInInspector] public List<MahouShoujoData> randomChoices = new List<MahouShoujoData>();

    [Header("--- CONFIGURACIÓN DE LUNAS ---")]
    [Tooltip("Cantidad máxima de lunas que pueden aparecer al azar en la partida (Ej: 0 o 1)")]
    public int maxRandomMoons = 1;

    [Header("--- LUNA ACTIVA EN ESTA PARTIDA ---")]
    public MoonType activeMoon = MoonType.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeGameSession();
    }

    public void InitializeGameSession()
    {
        // Seleccionamos las lunas disponibles (excluyendo 'None' para el pool aleatorio)
        List<MoonType> possibleMoons = new List<MoonType> { MoonType.BloodMoon, MoonType.GiantMoon, MoonType.Halfmoon };

        System.Random rand = new System.Random();

        // Determinamos cuántas lunas habrá al azar según el límite configurado (maxRandomMoons)
        int moonsToSpawn = Mathf.Clamp(maxRandomMoons, 0, 1); // Por ahora limitamos a 0 o 1 activa a la vez

        if (moonsToSpawn > 0 && possibleMoons.Count > 0)
        {
            activeMoon = possibleMoons[rand.Next(possibleMoons.Count)];
            Debug.Log($"[SISTEMA DE LUNAS] 🌙 ¡Luna activa en esta partida: {activeMoon} !");
        }
        else
        {
            activeMoon = MoonType.None;
            Debug.Log("[SISTEMA DE LUNAS] 🌑 Ninguna luna activa en esta partida.");
        }

        SelectionScript selector = FindObjectOfType<SelectionScript>();
        if (selector != null)
        {
            selector.SetupRandomCharacterChoices();
        }
    }

    public List<MahouShoujoData> GetRandomMahouShoujoChoices()
    {
        randomChoices.Clear();
        if (allAvailableMagicalGirls == null || allAvailableMagicalGirls.Count == 0) return randomChoices;

        System.Random rand = new System.Random();
        int countToTake = Mathf.Min(2, allAvailableMagicalGirls.Count);
        randomChoices = allAvailableMagicalGirls.OrderBy(m => rand.Next()).Take(countToTake).ToList();

        return randomChoices;
    }

    public void SelectPlayerCharacter(MahouShoujoData chosenCharacter)
    {
        playerCharacter = chosenCharacter;
        if (playerCharacter != null)
        {
            Debug.Log($"[SELECCIÓN] ¡Has elegido a: {playerCharacter.cardName}!");
        }
    }
}