using UnityEngine;
using TMPro;

public class GrimoryController : MonoBehaviour
{
    public Animator _grimorAnimator;
    [System.Serializable]
    public class GrimoryPageInfo
    {
        public int _id;
        
        public TextMeshProUGUI _descriptionText;
        
    }   
    public GrimoryPageInfo[] _grimoryPageInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnPage(bool right)
    {
        _grimorAnimator.SetBool("Right", right);   
        _grimorAnimator.SetTrigger("Turn");
    }
}
