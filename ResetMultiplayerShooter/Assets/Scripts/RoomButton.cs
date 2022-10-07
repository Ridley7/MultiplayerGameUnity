using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text buttonText;

    private RoomInfo info;

    public void SetButtonDetails(RoomInfo inputInFo)
    {
        info = inputInFo;

        buttonText.text = info.Name;
    }
}
