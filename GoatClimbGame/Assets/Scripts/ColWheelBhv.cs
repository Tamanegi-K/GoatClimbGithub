using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColWheelBhv : MonoBehaviour
{
    public List<Image> colDetectors = new List<Image>();
    public List<Image> hrmDetectors = new List<Image>();

    [System.Serializable]
    public class OneColDect
    {
        public Sprite off, on;
    }

    [SerializeField]
    public OneColDect[] colourDectUI;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
