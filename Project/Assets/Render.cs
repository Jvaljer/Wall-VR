using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
public class Render : MonoBehaviourPun {

    //internal class : Dixit 
    /* //SHALL ADAPT ALL THE SCRIPTS BELOW AND SUPPRESS SHAPES !!!
    //must have the internal class : 
public class MyCard : MonoBehaviour {
    // Creation of the card 
    public GameObject goCard = null;
    public string pos_tag = "";
    public PhotonView pv;
    public Transform parent;
    public int id_on_wall;
    public MyCard(Texture2D tex, Transform mur , int i) {
        GameObject goCard = PhotonNetwork.InstantiateRoomObject("Card", mur.position, mur.rotation, 0, null);
        goCard.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
        parent = mur;
        pv = goCard.GetPhotonView();
        //Debug.Log("MyCard created on Mur : " + parent);
        id_on_wall = i;
        pos_tag = "onWall";
    }
}

public class MyCard : MonoBehaviour {
    //specific card attributes
    private GameObject go_card = null;
    private string pos_tag = "";
    public PhotonView pv;
    private Transform parent;
    public MyCard(Texture2D tex, Transform wall){
        GameObject go_card = PhotonNetwork.InstantiateRoomObject("Card", wall.position, wall.rotation, 0, null);
        go_card.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
        parent = wall;
        pv = go_card.GetPhotonView();
        pos_tag = "onWall";
    }
}

    //must have a LoadingCard class like this: 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardsLoading : MonoBehaviour {
    //all cards' textures
    object[] textures;

    //size of cards
    private static float width = 0.033f;
    private static float height = 0.239f;

    //cards dispositions values
    private static int card_per_line = Mathf.RoundToInt(Rendering.card_per_wall/2);
    private float card_spacing;
    private float min_card_x = -0.35f - width;

    //card loading method
    [PunRPC]
    private void LoadCards(int card_pv_id, int wall_pv_id, int pos, int card_index){
        //loading textures
        if(textures==null){
            textures = Resources.LoadAll("dixit_all/", typeof(Texture2D));
        }
        //defining spacing attribute
        card_spacing = 0.7f / (card_per_line - ((int)(Rendering.card_per_wall%2)));

        //getting the concerned wall & card
        Transform wall = PhotonView.Find(wall_pv_id).transform;
        GameObject card = PhotonView.Find(card_pv_id).gameObject;

        //setting the texture
        Texture2D tex = (Texture2D)textures[card_index];
        card.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);

        //setting the card's attributes 
        card.transform.parent = wall;
        card.transform.rotation = wall.rotation;
        card.name = "Card " + card_index;
        card.transform.localScale = new Vector3(width, height, 1.0f);

        //setting card's position
        if(pos < card_per_line){
            card.transform.localPosition = new Vector3(min_card_x + width + card_spacing * pos, -height, -0.01f);
        } else {
            pos = pos - card_per_line;
            card.transform.localPosition = new Vector3(min_card_x + width + card_spacing * pos, height, -0.01f);
        }
    }
}
    */
    //unity attributes
    private Setup setup;
    private NetworkHandler network_handler;
    private Operator ope;

    //geometry attibutes
    private float screen_ratio = 1f;
    private float pix_to_unit = 1f;
    private float sw;
    private float sh;
    private float sw_unity;
    private float sh_unity;
    private float abs = 1.0f;
    private float ih_scale = 1.5f;
    private float ortho_size = 5f;

    //private float pixel_in_mm = 0.264275256f; //abel's laptop

    //shapes attributes
    //all shapes
    private Dictionary<string, GameObject> shapes;

    public void Start(){
        setup = GameObject.Find("ScriptManager").GetComponent<Setup>();
        network_handler = GameObject.Find("ScriptManager").GetComponent<NetworkHandler>();
        shapes = new Dictionary<string, GameObject>();
    }

    public void Input(string name, float m_x, float m_y, int id){
        //receiving coords in (0,0)-(1,1)
        //wanna calculate (with swU & shU)
            //px = mouse_x*sw - (sw/2f);
            //py = (sh/2f) - mouse_y*sh;
        float px = m_x*sw_unity - (sw_unity/2f);
        float py = (sh_unity/2f) - m_y*sh_unity;
        if(setup.is_vr){
            //we simply need to adjust the y value because center of wall is (0,2.5)
            float tmp = py + 2.5f;
            py = tmp;
        }

        //and now does input on these coords
        foreach(GameObject obj in shapes.Values){
            Shape obj_ctrl = obj.GetComponent<Shape>();
            if(obj_ctrl.IsOwnedBy(id)){
                switch (name){
                    case "Down":
                        if(obj_ctrl.CoordsInside(new Vector2(px,py))){
                            obj.GetComponent<PhotonView>().RPC("PickRPC", RpcTarget.AllBuffered);
                        }
                        break;
                    case "Move":
                        if(obj_ctrl.IsDragged()){
                            obj_ctrl.Move(px, py, setup.zoom_ratio);
                        }
                        break;
                    case "Up":
                        obj.GetComponent<PhotonView>().RPC("DropRPC", RpcTarget.AllBuffered);
                        break;
                }
            }
        }
    }

    public void InitializeFromIH(Operator O_){
        ope = O_;
        if(!shapes.ContainsKey("Circle0")){
            shapes.Add(GameObject.Find("Circle0").name,GameObject.Find("Circle0"));
        }

        if(PhotonNetwork.IsMasterClient){
            sw = Screen.width;
            sh = Screen.height;
            screen_ratio = sh/sw;
            pix_to_unit = Camera.main.orthographicSize /(sh/2.0f);
            sw_unity = sw*pix_to_unit;
            sh_unity = sh*pix_to_unit;
            Debug.LogError("Operator's render has -> cam_orthoSize="+Camera.main.orthographicSize+" sw="+sw+" sh="+sh+" PtU="+pix_to_unit+" swu="+sw_unity+" shu="+sh_unity);
            abs = 0.1f;
            ih_scale = ih_scale*abs;
        } else {
            if(setup.is_vr){
                //simply replacing the shape on the wall ?
                shapes["Circle0"].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                shapes["Circle0"].transform.position = new Vector3(0f, 2.5f, 4.99f);

                GameObject wall_go = GameObject.Find("WallGO");
                sw = wall_go.transform.localScale.x;
                sh = wall_go.transform.localScale.y;
                //put WallGO size instead ??
                pix_to_unit = (sw/2.0f) / (sh/1.0f);
                sw_unity = sw*pix_to_unit;
                sh_unity = sh*pix_to_unit;
                Debug.LogError("VR's render has -> sw="+sw+" sh="+sh+" PtU="+pix_to_unit+" swu="+sw_unity+" shu="+sh_unity);
            } else {
                sw = setup.wall_width;
                sh = setup.wall_height;
                screen_ratio = sh/sw;
                ortho_size = (float)Camera.main.orthographicSize / (float)setup.wall.RowsAmount();
                pix_to_unit = (float)setup.wall.RowsAmount() * (float)Camera.main.orthographicSize / (sh/2.0f);
                sw_unity = sw*pix_to_unit;
                sh_unity = sh*pix_to_unit;
                Debug.LogError("Participant's render has -> cam_orthoSize="+Camera.main.orthographicSize+" sw="+sw+" sh="+sh+" PtU="+pix_to_unit+" swu="+sw_unity+" shu="+sh_unity);
                abs = 1.0f;
                foreach(GameObject shape in shapes.Values){
                    //zoom value = amount of division ?
                    shape.transform.localScale *= setup.zoom_ratio;
                }
            }
        }
    }
}
