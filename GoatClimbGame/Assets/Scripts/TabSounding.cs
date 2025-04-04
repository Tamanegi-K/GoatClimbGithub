using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabSounding : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData ped)
    {
        AudioManager.GetInstance().PlaySFXUI("shift");
    }
}
