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
        base.OnConnectedToMaster();
        current_in_room = 0;
        //creating the room
        room_opt = new RoomOptions{MaxPlayers=max_in_room, IsVisible=true, IsOpen=true};
        PhotonNetwork.JoinOrCreateRoom("Room", room_opt, TypedLobby.Default);
    }

    public override void OnJoinedRoom(){
        //triggered for the just joining entity only
        base.OnJoinedRoom();
        setup = GameObject.Find("ScriptManager").GetComponent<Setup>();
        if(PhotonNetwork.IsMasterClient){
            setup.logger.Msg("Joining the Photon Room as Operator ("+PhotonNetwork.LocalPlayer.ActorNumber+")", "S");
            ope_prefab = PhotonNetwork.Instantiate("Operator", transform.position, transform.rotation);
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);

            if(setup.dixits){
                //first trying to create only one dixit
                NetworkCreateSingleDixit();
            } else {
                shape1_prefab = PhotonNetwork.InstantiateRoomObject("Circle", new Vector3(0f,0f,1f), Quaternion.identity);
                Shape sh1_ctrl = shape1_prefab.GetComponent<Shape>();
                sh1_ctrl.SetName("Circle0");
                sh1_ctrl.Categorize("circle");
                sh1_ctrl.SetSize(shape1_prefab.transform.localScale.x);
                sh1_ctrl.PositionOn(Vector3.zero);
            }

            if(setup.master_only){
                ope_prefab.GetComponent<PhotonView>().RPC("InitializeRPC", RpcTarget.AllBuffered);
                setup.logger.Msg("Program is on Master Only -> ParticipantIsReady from Ope", "S");
            }

        } else {
            if(setup.is_vr){
                setup.logger.Msg("Joining the Photon Room as VR Part ("+PhotonNetwork.LocalPlayer.ActorNumber+")", "S");
                vr_prefab = PhotonNetwork.Instantiate("VR Participant", transform.position, transform.rotation);
                vr_prefab.GetComponent<Participant>().InitializeFromNetwork(setup);
                cur_participant = vr_prefab;
            } else {
                setup.logger.Msg("Joining the Photon Room as Wall Part ("+PhotonNetwork.LocalPlayer.ActorNumber+")", "S");
                part_prefab = PhotonNetwork.Instantiate("Wall Participant", transform.position, transform.rotation);
                part_prefab.GetComponent<Participant>().InitializeFromNetwork(setup);
                cur_participant = part_prefab;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        base.OnPlayerEnteredRoom(newPlayer);
        if(PhotonNetwork.IsMasterClient){
            setup.logger.Msg("Player entered the room -> "+PhotonNetwork.CurrentRoom.PlayerCount+"/"+(setup.part_cnt +1), "C");
            //if i'm master then test some stuff
            if(PhotonNetwork.CurrentRoom.PlayerCount==(setup.part_cnt +1)){ //all parts + master
                setup.logger.Msg("Everyone joined -> now initializing", "V");
                shape1_prefab.GetComponent<PhotonView>().RPC("SetNameRPC", RpcTarget.AllBuffered, "Circle0");
                ope_prefab.GetComponent<PhotonView>().RPC("InitializeRPC", RpcTarget.AllBuffered);
            }
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
        PhotonNetwork.NickName = System.DateTime.Now.Ticks.ToString();
        PhotonNetwork.ConnectUsingSettings();
    }

    public void NetworkCreateSingleDixit(){
        Render render = GameObject.Find("ScriptManager").GetComponent<Render>();
        render.InitializeSingleDixits(0);
    }
}

