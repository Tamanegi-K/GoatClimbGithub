using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColWheelBhv : MonoBehaviour
{
    public CanvasGroup fader;
    public List<Image> colDetectors = new List<Image>();
    public Image hrmContr, hrmTriad;
    public List<Image> hrmAnal = new List<Image>();
    private float redAngle = 0f, orangeAngle = 300f, yellowAngle = 240f;

    [System.Serializable]
    public class OneColDect
    {
        public Sprite off, on;
    }

    [SerializeField]
    public OneColDect[] colourDectUI;

    public Dictionary<PlantSpawning.PlantColour, int> cccNoC = new Dictionary<PlantSpawning.PlantColour, int>(); // short for currentColourCountNoCentre



    // Start is called before the first frame update
    void Start()
    {
        CountColoursNow(new string[7] { "", "", "", "", "", "", "" });
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // If nothing was put in, do not fade in anyhing
        if (cccNoC[PlantSpawning.PlantColour.RED] + cccNoC[PlantSpawning.PlantColour.ORANGE] + cccNoC[PlantSpawning.PlantColour.YELLOW] + cccNoC[PlantSpawning.PlantColour.GREEN] + cccNoC[PlantSpawning.PlantColour.BLUE] + cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0)
        {
            fader.alpha = 0f;
            return;
        }

        fader.alpha = Mathf.Abs(Mathf.Cos(Time.time * 5f / Mathf.PI));
    }

    public void CountColoursNow(string[] inputs)
    {
        //Debug.LogWarning("started");
        cccNoC.Clear();
        cccNoC.Add(PlantSpawning.PlantColour.RED, 0);
        cccNoC.Add(PlantSpawning.PlantColour.ORANGE, 0);
        cccNoC.Add(PlantSpawning.PlantColour.YELLOW, 0);
        cccNoC.Add(PlantSpawning.PlantColour.GREEN, 0);
        cccNoC.Add(PlantSpawning.PlantColour.BLUE, 0);
        cccNoC.Add(PlantSpawning.PlantColour.PURPLE, 0);
        
        // Count the colours using the names of plants
        for (int i = 1; i < 7; i += 1)
        {
            foreach (PlantSpawning.OnePlantInfo opi in GameMainframe.GetInstance().GetComponent<PlantSpawning>().plantMasterlist)
            {
                // Loop through name of flower with the plantMasterlist and only execute if it matches
                if (opi.plantName == inputs[i])
                {
                    cccNoC[opi.plantCol] += 1;
                    //Debug.Log(cccNoC[opi.plantCol] + " ++");
                }
            }
        }

        int totalCounts = cccNoC[PlantSpawning.PlantColour.RED] + cccNoC[PlantSpawning.PlantColour.ORANGE] + cccNoC[PlantSpawning.PlantColour.YELLOW] + cccNoC[PlantSpawning.PlantColour.GREEN] + cccNoC[PlantSpawning.PlantColour.BLUE] + cccNoC[PlantSpawning.PlantColour.PURPLE];

        // Display the things

        // first, turn everything off
        for (int i = 0; i < colDetectors.Count; i += 1)
        {
            colDetectors[i].enabled = false;
            colDetectors[i].sprite = colourDectUI[i].off;
        }
        hrmContr.enabled = false; hrmTriad.enabled = false;
        for (int i = 0; i < hrmAnal.Count; i += 1)
        {
            hrmAnal[i].enabled = false;
        }

        // --- HARMONIES ---
        // CONTRASTING
        if ( // R/G
            cccNoC[PlantSpawning.PlantColour.RED] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0
            )
		{
            hrmContr.enabled = true;
            hrmContr.transform.localEulerAngles = new Vector3(0f, 0f, redAngle);

            colDetectors[0].enabled = true; colDetectors[0].sprite = colourDectUI[0].off;
            colDetectors[3].enabled = true; colDetectors[3].sprite = colourDectUI[3].off;
        }
        else if ( // O/B
            cccNoC[PlantSpawning.PlantColour.RED] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0
            )
        {
            hrmContr.enabled = true;
            hrmContr.transform.localEulerAngles = new Vector3(0f, 0f, orangeAngle);

            colDetectors[1].enabled = true; colDetectors[1].sprite = colourDectUI[1].off;
            colDetectors[4].enabled = true; colDetectors[4].sprite = colourDectUI[4].off;
        }
        else if ( // Y/P
            cccNoC[PlantSpawning.PlantColour.RED] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] >= 0
            )
        {
            hrmContr.enabled = true;
            hrmContr.transform.localEulerAngles = new Vector3(0f, 0f, yellowAngle);

            colDetectors[2].enabled = true; colDetectors[2].sprite = colourDectUI[2].off;
            colDetectors[5].enabled = true; colDetectors[5].sprite = colourDectUI[5].off;
        }

        // ANALOGOUS
        if ( // P/R/O
            cccNoC[PlantSpawning.PlantColour.RED] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] == 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] == 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] == 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] >= 0
            )
        {
            hrmAnal[0].enabled = true;

            colDetectors[5].enabled = true; colDetectors[5].sprite = colourDectUI[5].off;
            colDetectors[0].enabled = true; colDetectors[0].sprite = colourDectUI[0].off;
            colDetectors[1].enabled = true; colDetectors[1].sprite = colourDectUI[1].off;
        }
        if ( // R/O/Y
            cccNoC[PlantSpawning.PlantColour.RED] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0
            )
        {
            hrmAnal[1].enabled = true;

            colDetectors[0].enabled = true; colDetectors[0].sprite = colourDectUI[0].off;
            colDetectors[1].enabled = true; colDetectors[1].sprite = colourDectUI[1].off;
            colDetectors[2].enabled = true; colDetectors[2].sprite = colourDectUI[2].off;
        }
        if ( // O/Y/G
            cccNoC[PlantSpawning.PlantColour.RED] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0
            )
        {
            hrmAnal[2].enabled = true;

            colDetectors[1].enabled = true; colDetectors[1].sprite = colourDectUI[1].off;
            colDetectors[2].enabled = true; colDetectors[2].sprite = colourDectUI[2].off;
            colDetectors[3].enabled = true; colDetectors[3].sprite = colourDectUI[3].off;
        }
        if ( // Y/G/B
            cccNoC[PlantSpawning.PlantColour.RED] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0
            )
        {
            hrmAnal[3].enabled = true;

            colDetectors[2].enabled = true; colDetectors[2].sprite = colourDectUI[2].off;
            colDetectors[3].enabled = true; colDetectors[3].sprite = colourDectUI[3].off;
            colDetectors[4].enabled = true; colDetectors[4].sprite = colourDectUI[4].off;
        }
        if ( // G/B/P
            cccNoC[PlantSpawning.PlantColour.RED] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] >= 0
            )
        {
            hrmAnal[4].enabled = true;

            colDetectors[3].enabled = true; colDetectors[3].sprite = colourDectUI[3].off;
            colDetectors[4].enabled = true; colDetectors[4].sprite = colourDectUI[4].off;
            colDetectors[5].enabled = true; colDetectors[5].sprite = colourDectUI[5].off;
        }
        if ( // B/P/R
            cccNoC[PlantSpawning.PlantColour.RED] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] >= 0
            )
        {
            hrmAnal[5].enabled = true;

            colDetectors[4].enabled = true; colDetectors[4].sprite = colourDectUI[4].off;
            colDetectors[5].enabled = true; colDetectors[5].sprite = colourDectUI[5].off;
            colDetectors[0].enabled = true; colDetectors[0].sprite = colourDectUI[0].off;
        }

        // --- TRIADIC ---
        if ( // R/Y/B
            cccNoC[PlantSpawning.PlantColour.RED] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] <= 0
            )
        {
            hrmTriad.enabled = true;
            hrmTriad.transform.localEulerAngles = new Vector3(0f, 0f, redAngle);

            colDetectors[0].enabled = true; colDetectors[0].sprite = colourDectUI[0].off;
            colDetectors[2].enabled = true; colDetectors[2].sprite = colourDectUI[2].off;
            colDetectors[4].enabled = true; colDetectors[4].sprite = colourDectUI[4].off;
        }
        else if ( // O/G/P
            cccNoC[PlantSpawning.PlantColour.RED] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.ORANGE] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.YELLOW] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.GREEN] >= 0 &&
            cccNoC[PlantSpawning.PlantColour.BLUE] <= 0 &&
            cccNoC[PlantSpawning.PlantColour.PURPLE] >= 0
            )
        {
            hrmTriad.enabled = true;
            hrmTriad.transform.localEulerAngles = new Vector3(0f, 0f, orangeAngle);

            colDetectors[1].enabled = true; colDetectors[1].sprite = colourDectUI[1].off;
            colDetectors[3].enabled = true; colDetectors[3].sprite = colourDectUI[3].off;
            colDetectors[5].enabled = true; colDetectors[5].sprite = colourDectUI[5].off;
        }


        // --- COLOURS ---
        // RED
        if (cccNoC[PlantSpawning.PlantColour.RED] > 0)
        {
            colDetectors[0].enabled = true;
            colDetectors[0].sprite = colourDectUI[0].on;
        }

        // ORANGE
        if (cccNoC[PlantSpawning.PlantColour.ORANGE] > 0)
        {
            colDetectors[1].enabled = true;
            colDetectors[1].sprite = colourDectUI[1].on;
        }

        // YELLOW
        if (cccNoC[PlantSpawning.PlantColour.YELLOW] > 0)
        {
            colDetectors[2].enabled = true;
            colDetectors[2].sprite = colourDectUI[2].on;
        }

        // GREEN
        if (cccNoC[PlantSpawning.PlantColour.GREEN] > 0)
        {
            colDetectors[3].enabled = true;
            colDetectors[3].sprite = colourDectUI[3].on;
        }

        // BLUE
        if (cccNoC[PlantSpawning.PlantColour.BLUE] > 0)
        {
            colDetectors[4].enabled = true;
            colDetectors[4].sprite = colourDectUI[4].on;
        }

        // PURPLE
        if (cccNoC[PlantSpawning.PlantColour.PURPLE] > 0)
        {
            colDetectors[5].enabled = true;
            colDetectors[5].sprite = colourDectUI[5].on;
        }

        //Debug.Log(cccNoC[PlantSpawning.PlantColour.RED] + " | " + cccNoC[PlantSpawning.PlantColour.ORANGE] + " | " + cccNoC[PlantSpawning.PlantColour.YELLOW] + " | " + cccNoC[PlantSpawning.PlantColour.GREEN] + " | " + cccNoC[PlantSpawning.PlantColour.BLUE] + " | " + cccNoC[PlantSpawning.PlantColour.PURPLE]);
    }
}
