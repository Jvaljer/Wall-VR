using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkHandler : MonoBehaviourPunCallbacks {
    //referenced setup
    public Setup setup;

    //photon room
    private RoomOptions room_opt;
    private byte max_in_room = 12; //10 wall part + 1 wall ope + 1 VR part
    public int current_in_room { get; private set; }
    public bool ope_joined { get; private set; } = false;

    //prefabs
    public GameObject ope_prefab { get; private set; }
    public GameObject part_prefab { get; private set; }
    public GameObject vr_prefab { get; private set; }

    //local participant prefab
    public GameObject cur_participant;

    //overall based shapes prefab
    public GameObject shape1_prefab;

    public override void OnConnectedToMaster(){
        Debug.LogError("OnConnectedToMaster : "+PhotonNetwork.LocalPlayer.ActorNumber);
        base.OnConnectedToMaster();
        current_in_room = 0;
        //creating the room
        room_opt = new RoomOptions{MaxPlayers=max_in_room, IsVisible=true, IsOpen=true};
        PhotonNetwork.JoinOrCreateRoom("Room", room_opt, TypedLobby.Default);
    }

    public override void OnJoinedRoom(){
        //triggered for the just joining entity only
        base.OnJoinedRoom();
        Debug.Log("OnJoinedRoom"+PhotonNetwork.LocalPlayer.ActorNumber);
        setup = GameObject.Find("ScriptManager").GetComponent<Setup>();
        if(setup.is_master){
            Debug.Log("OnJoinedRoom as Operator : "+PhotonNetwork.LocalPlayer.ActorNumber);
            ope_prefab = PhotonNetwork.Instantiate("Operator", transform.position, transform.rotation);
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);

            Debug.LogError("instantiating shape");
            shape1_prefab = PhotonNetwork.InstantiateRoomObject("Circle", Vector3.zero, Quaternion.identity);
            Shape sh1_ctrl = shape1_prefab.GetComponent<Shape>();
            Debug.LogError("setting shape up : "+shape1_prefab.transform.localScale+" , "+shape1_prefab.transform.position);
            sh1_ctrl.Categorize("circle");
            sh1_ctrl.SetSize(shape1_prefab.transform.localScale.x);
            sh1_ctrl.PositionOn(Vector3.zero);

            if(setup.master_only){
                ope_prefab.GetComponent<PhotonView>().RPC("InitializeRPC", RpcTarget.AllBuffered);
                ope_prefab.GetComponent<InputHandler>().ParticipantIsReady();
            }

        } else {
            if(setup.is_vr){
                Debug.Log("OnJoinedRoom as VR Participant : "+PhotonNetwork.LocalPlayer.ActorNumber);
                vr_prefab = PhotonNetwork.Instantiate("VR Participant", transform.position, transform.rotation);
                vr_prefab.GetComponent<Participant>().InitializeFromNetwork(setup);
                cur_participant = vr_prefab;
                GameObject.Find("Operator(Clone)").GetComponent<InputHandler>().RegisterDevice("Right", cur_participant.GetComponent<Participant>().GetRightCtrl());
                GameObject.Find("Operator(Clone)").GetComponent<PhotonView>().RPC("RegisterDeviceRPC", RpcTarget.AllBuffered, "Right", cur_participant.GetComponent<Participant>().GetRightCtrl());
            } else {
                Debug.Log("OnJoinedRoom as Wall Participant : "+PhotonNetwork.LocalPlayer.ActorNumber);
                part_prefab = PhotonNetwork.Instantiate("Wall Participant", transform.position, transform.rotation);
                part_prefab.GetComponent<Participant>().InitializeFromNetwork(setup);
                cur_participant = part_prefab;
            }
 
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        base.OnPlayerEnteredRoom(newPlayer);
        if(PhotonNetwork.IsMasterClient){
            Debug.Log("OnPlayerEnteredRoom as operator : "+PhotonNetwork.LocalPlayer.ActorNumber);
            //if i'm master then test some stuff
            if(PhotonNetwork.CurrentRoom.PlayerCount==(setup.part_cnt +1)){ //all parts + master
                ope_prefab.GetComponent<PhotonView>().RPC("InitializeRPC", RpcTarget.AllBuffered);
            }
        } else {
            Debug.Log("OnPlayerEnteredRoom as participant : "+PhotonNetwork.LocalPlayer.ActorNumber);
        }
        //else I don't give a fuck
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        base.OnPlayerLeftRoom(otherPlayer);
        if(!PhotonNetwork.IsMasterClient){
            cur_participant.GetComponent<PhotonView>().RPC("SomeoneLeft", RpcTarget.AllBuffered, otherPlayer.ActorNumber);
        }
    }

    public void SendOpeToParticipants(){
        if(!PhotonNetwork.IsMasterClient){
            cur_participant.GetComponent<PhotonView>().RPC("FetchForOperatorRPC", RpcTarget.AllBuffered);
        }
    }

    public void Connect(){
        Debug.Log("NetworkHandler -> Connect "+PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.NickName = System.DateTime.Now.Ticks.ToString();
        PhotonNetwork.ConnectUsingSettings();
    }
}

