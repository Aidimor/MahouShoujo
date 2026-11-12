using UnityEngine;

[CreateAssetMenu(fileName = "NewMahouShoujo", menuName = "Mahou Shoujo/Mahou Shoujo Data")]
public class MahouShoujoData : ScriptableObject
{
    [Header("Información Básica")]
    public string cardName;

    public Sprite artwork;
 
 

   

    [System.Serializable]
    public class Skill
    {
        [System.Serializable]
        public enum SkillType
        {
            Add,
            Remove,
            Steal,
            Change,


        }
        public SkillType _skillType;


        [System.Serializable]
        public enum Type
        {
           EnemyOrbs,
           EnemyHp,
           MapOrbs,
           EnemyCard,
           Moon,
           Shield,
           PlayerHP,

        }
        public Type _type;
        public int _ammount;
    }
    public Skill[] _skill;

 
  
}