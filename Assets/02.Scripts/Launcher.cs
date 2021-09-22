using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launcher : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    public bool inLobby = false;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        inLobby = true;
    }

    public void JoinRoom()
    {
        
        /*
        if(inLobby)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        */
        if(PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Photon.Realtime.RoomOptions roomOptions= new Photon.Realtime.RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(null,roomOptions);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameMap");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
