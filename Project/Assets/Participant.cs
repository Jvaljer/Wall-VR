using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using Photon.Pun;

public class Participant : MonoBehaviourPun {
    //referenced setup & operator
    public Setup setup { get; set; }
    public GameObject ope { get; set; }

    //VR Components
    private GameObject right_hand;
    private GameObject left_hand;
    private GameObject headset;

    //ray attributes
    private GameObject ray_go;
    private RaycastHit hit;

    //devices & shape manipulation ids
    private int id;
    private bool started = false;

    //update method is used only for VR participant, as they're the only one (yet) to have possible interactions
    private void Update(){
        if(photonView.IsMine && started){
            Debug.Log("I am a VR participant");
            Ray ray = new Ray(right_hand.transform.position, right_hand.transform.forward);
            if(Physics.Raycast(ray, out hit)){
                //Debug.Log("ray is hitting something : "+hit.transform.gameObject.name);
                if(hit.transform.tag == "Wall" ||hit.transform.tag == "Shape"){
                    //we wanna move the cursor to the hit position
                    Debug.Log("sending move input to Ope on wall point "+hit.point);
                    ope.GetComponent<PhotonView>().RPC("VRInputRPC", RpcTarget.AllBuffered, "Move", hit.point, PhotonNetwork.LocalPlayer.ActorNumber);
                }
            }
        }
    }

    public void InitializeFromNetwork(Setup S_){
        Debug.Log("InitializeFromNetwork with Setup : "+(S_==null)+" with potential id : "+PhotonNetwork.LocalPlayer.ActorNumber);
        setup = S_;
        if(photonView.IsMine){
            InitializeMyself();
        }
    }

    public void InitializeMyself(){
        id = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("assigned id : "+id);
        if(setup.is_vr){
            Debug.Log("InitializeMyself :: VR");
            //wanna fetch all of my VR components
            headset = GameObject.Find("XR Origin");
            right_hand = headset.transform.GetChild(0).GetChild(1).gameObject;
            ray_go = right_hand.transform.GetChild(0).gameObject;
            left_hand = headset.transform.GetChild(0).GetChild(2).gameObject;
        } else {
            Debug.Log("InitializeMyself :: Wall");
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
        Debug.Log("GetRightCtrl with Setup : "+(setup!=null));
        if(setup.is_vr){
            return right_hand;
        } else {
            Debug.Log("I no do VR boss -> null");
            return null;
        }
    }

    [PunRPC]
    public void FetchForOperatorRPC(){
        if(photonView.IsMine || PhotonNetwork.IsMasterClient){
            //might have to fetch the one of the Wall Scene ??
            ope = GameObject.Find("Operator(Clone)");
            Debug.Log("FetchForOperatorRPC -> ParticipantIsReady");
            ope.GetComponent<InputHandler>().ParticipantIsReady(PhotonNetwork.LocalPlayer.ActorNumber);
            started = true;
        }
    }

    [PunRPC]
    public void SomeoneLeft(int pv){
        if(pv==1){ //in this program logic, 1 is the master client (operator)
            Application.Quit();
        }
    }
}
