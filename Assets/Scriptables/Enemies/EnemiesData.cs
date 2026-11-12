using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemiy/Enemy Data")]
public class EnemiesData : ScriptableObject
{
    [Header("Información Básica")]
    public string cardName;
    public Sprite artwork;

    public int _hp;
    public int _attack;


    [System.Serializable]
    public class SpecialSkill
    {
        public int _value;

        [System.Serializable]
        public enum EnemyAbilityType
        {
            Attack,
            Defense,
            Special
        }
        public EnemyAbilityType _enemySkillType;
        public int _silverMana;
        public int _bloodMana;
        public int _cristalMana;     


    }
    public SpecialSkill[] _specialSkill;

}

/// <summary>
/// Tipos de habilidades especiales basadas en los slimes diseñados.
/// </summary>

