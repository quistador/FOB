using UnityEngine;
using System.Collections;

public class TextSlot : MonoBehaviour {

    // Use this for initialization
    void Start () 
    {

    }

    void Update () 
    {

    }

    private Squad _squadForSlot;
    public Squad squadForSlot 
    {
        get
        {
            return _squadForSlot;
        }
        set
        {
            _squadForSlot = value;
            TextMesh textMesh = this.gameObject.GetComponent<TextMesh>() as TextMesh;
            textMesh.text = this.squadForSlot.squadTypeDisplayName;
        }
    }
}
