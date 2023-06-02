using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Participant : MonoBehaviourPun {
    //referenced setup
    private Setup setup;

    //VR Components
    private GameObject right_hand;
    private GameObject ray;
    private GameObject left_hand;
    private GameObject headset;

    public void InitializeFromNetwork(Setup S_){
        setup = S_;
        if(photonView.IsMine){
            InitializeMyself();
        }
    }

    public void InitializeMyself(){
        if(setup.is_vr){
            //wanna fetch all of my VR components
            headset = GameObject.Find("XR Origin");
            right_hand = headset.transform.GetChild(0).GetChild(1).gameObject;
            ray = right_hand.transform.GetChild(0).gameObject;
            left_hand = headset.transform.GetChild(0).GetChild(2).gameObject;
        } else {
            Screen.fullScreen = setup.full_screen;
            if(Screen.fullScreen){
                setup.screen_width = Screen.width;
                setup.screen_height = Screen.height;
            } else {
                Screen.SetResolution( (int)setup.screen_width, (int)setup.screen_height, false );
            }
            //setting the camera
            Vector3 old_pos = Camera.main.transform.position;
            Vector3 scale = Camera.main.transform.localScale;
            //translating & scaling camera
            float center_x = setup.x_pos + (setup.screen_width/2) - (setup.wall.Width()/2) + (setup.screen_width/2);
            float center_y = (setup.wall.Height()/2) - setup.y_pos + (setup.screen_height/2) - (setup.screen_height/2);
            Vector3 screen_pos = Camera.main.WorldToScreenPoint(Camera.main.transform.position);
            Vector3 world_pos = Camera.main.ScreenToWorldPoint(new Vector3(center_x, center_y, screen_pos.z));
            Camera.main.transform.position = world_pos;
        }
    }

    public GameObject GetRightCtrl(){
        if(setup.is_vr){
            return right_hand;
        } else {
            Debug.Log("I no do VR boss -> null");
            return null;
        }
    }

    [PunRPC]
    public void FetchForOperator(){
        if(photonView.IsMine || PhotonNetwork.IsMasterClient){
            //might have to fetch the one of the Wall Scene ??
            GameObject.Find("Operator(Clone)").GetComponent<InputHandler>().ParticipantIsReady();
        }
    }

    [PunRPC]
    public void SomeoneLeft(int pv){
        if(pv==1){ //in this program logic, 1 is the master client (operator)
            Application.Quit();
        }
    }
}
