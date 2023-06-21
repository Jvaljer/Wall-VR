using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Operator : MonoBehaviourPun {
    //referenced setup & net manag
    private Setup setup;
    private NetworkHandler network;
    private InputHandler input_handler;

    //possible shapes prefab
    private GameObject circle_prefab;
    private GameObject square_prefab;

    [PunRPC]
    public void InitializeRPC(){
        setup = GameObject.Find("ScriptManager").GetComponent<Setup>();
        network = GameObject.Find("ScriptManager").GetComponent<NetworkHandler>();
        input_handler = gameObject.GetComponent<InputHandler>();

        if(photonView.IsMine){
            //setting the screen resolution only for myself
            Screen.fullScreen = setup.full_screen;
            if(Screen.fullScreen){
                setup.screen_width = Screen.width;
                setup.screen_height = Screen.height;
            } else {
                Screen.SetResolution( (int)setup.screen_width, (int)setup.screen_height, false );
            }
        }
        input_handler.InitializeFromOpe();
        if(setup.master_only){
            input_handler.ParticipantIsReady();
        } else {
            network.SendOpeToParticipants();
        }
    }

    [PunRPC]
    public void InstantiateShape(string category, Vector3 pos){
        switch (category){
            case "circle":
                break;
            case "square":
                square_prefab = PhotonNetwork.InstantiateRoomObject("Square", pos, Quaternion.identity);
                square_prefab.GetComponent<PhotonView>().RPC("MoveRPC", RpcTarget.AllBuffered, pos);
                break;
            default:
                break;
        }
    }

    [PunRPC]
    public void AddVRCursor(int n = -1){
        input_handler.AddVRCursorFromOpe(n);
    }

    [PunRPC]
    public void VRInputRPC(string str, Vector3 coord, int n){
        setup.logger.Msg("Receiving a VR input via RPC : "+str, "C");
        string name = "";
        switch (str){
            case "RayMove":
                name = "Move";
                break;
            case "TriggerDown":
                name = "Down";
                break;
            case "TriggerUp":
                name = "Up";
                break;
            default:
                break;
        }
        setup.logger.Msg("Sending the VR Input : "+name, "C");
        input_handler.InputFromVR(name,coord,n);
    }
}
