using UnityEngine;



[CreateAssetMenu(fileName = "NewPagesData", menuName = "New Pages Data/New Page")]
public class NewPagesData : ScriptableObject
{
    [System.Serializable]
    public enum PageType
    {
        Attack,
        AOEattack,
        DefenseUp,
        Weapon,
        Recollect,
        CDReduction,
        AddMana
    }

    [System.Serializable]
    public struct PageData
    {
        public PageType pageType;
        public int value;
     
    }

    [Header("Visual & UI")]
    public Sprite artwork;
    [TextArea] public string levelDescription;

    [Header("Economy & Combat Data")]
    public int _manaCost;

    [Header("Page Mechanics")]
    public PageData[] _pageData;
}