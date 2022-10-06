using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject loadingScreen;
    public TMP_Text loadingText;

    public GameObject menuButtons;

    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        PhotonNetwork.ConnectUsingSettings();
    }

    private void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        

        PhotonNetwork.JoinLobby();

        loadingText.text = "Joining Lobby...";
    }

    public override void OnJoinedLobby()
    {
        //base.OnJoinedLobby();
        CloseMenus();
        menuButtons.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
