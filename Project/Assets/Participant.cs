using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.InputSystem;

public class Participant : MonoBehaviourPun {
    //referenced setup & operator
    public Setup setup { get; set; }
    public GameObject ope { get; set; }

    //VR Components
    private GameObject right_hand;
    private GameObject left_hand;
    private GameObject headset;

    //XR components
    private XRController right_ctrl;
    UnityEngine.XR.InputDevice r_ctrl_device;

    //ray attributes
    private GameObject ray_go;
    private RaycastHit hit;

    //devices & shape manipulation ids
    private int id;

    //useful predicates
    private bool started = false;
    private bool vr_fetched = false;

    //update method is used only for VR participant, as they're the only one (yet) to have possible interactions
    private void Update(){
        if(photonView.IsMine && started){
            if(setup.is_vr){
                if(photonView.IsMine){
                    setup.logger.Msg("I am VR","EDO");
                    Ray ray = new Ray(right_hand.transform.position, right_hand.transform.forward);
                    if(Physics.Raycast(ray, out hit)){
                        if(hit.transform.tag == "Wall" || hit.transform.tag == "Shape"){
                            //we wanna move the cursor to the hit position
                            ope.GetComponent<PhotonView>().RPC("VRInputRPC", RpcTarget.AllBuffered, "Move", hit.point, PhotonNetwork.LocalPlayer.ActorNumber);
                        }
                    }

                    if(!vr_fetched){
                        FetchForVRComponent();
                    }

                    //we wanna fetch for the VR inputs
                    bool trigger;
                    if(r_ctrl_device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out trigger) && trigger){
                        setup.logger.Msg("Trigger's been triggered !", "V");
                        ope.GetComponent<PhotonView>().RPC("VRInputRPC", RpcTarget.AllBuffered, "TriggerDown", hit.point, PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                }
            }
        }
    }
    public void InitializeFromNetwork(Setup S_){
        Debug.Log("InitializeFromNetwork with Setup : "+(S_==null)+" with potential id : "+PhotonNetwork.LocalPlayer.ActorNumber);
        setup = S_;
        setup.logger.Msg("Just got the setup, with id n°"+PhotonNetwork.LocalPlayer.ActorNumber, "V");
        if(photonView.IsMine){
            InitializeMyself();
        }
    }

    public void InitializeMyself(){
        id = PhotonNetwork.LocalPlayer.ActorNumber;
        if(setup.is_vr){
            setup.logger.Msg("Starting initialization as VR", "C");
            //wanna fetch all of my VR components
            headset = GameObject.Find("XR Origin");
            right_hand = headset.transform.GetChild(0).GetChild(1).gameObject;
            ray_go = right_hand.transform.GetChild(0).gameObject;
            left_hand = headset.transform.GetChild(0).GetChild(2).gameObject;
            //then fetch the xr controller (inputs)
            right_ctrl = right_hand.GetComponent<XRController>();
        } else {
            setup.logger.Msg("Starting initialization as Wall", "C");
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

        setup.logger.Msg("Finished Initializing", "V");
    }

    public GameObject GetRightCtrl(){
        setup.logger.Msg("Getting Right Controler", "C");
        if(setup.is_vr){
            return right_hand;
        } else {
            setup.logger.Msg("Me VR don do -> null", "E");
            return null;
        }
    }

    public void FetchForVRComponent(){
        var input_devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(input_devices);
        setup.logger.Msg("GOT "+input_devices.Count+" XR devices : ", "S");
        foreach(var device in input_devices){
            if(device.role.ToString()=="RightHanded"){
                setup.logger.Msg("found the right hand !!!", "V");
                r_ctrl_device = device;
                vr_fetched = true;
            }
        }
        setup.logger.Msg("vr_fetched is "+vr_fetched, "S");
    }

    [PunRPC]
    public void FetchForOperatorRPC(){
        if(photonView.IsMine || PhotonNetwork.IsMasterClient){
            //might have to fetch the one of the Wall Scene ??
            ope = GameObject.Find("Operator(Clone)");
            if(setup!=null){ //setup null for operator
                setup.logger.Msg("Participant fetched operator -> PartIsReady", "V");
            }
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
