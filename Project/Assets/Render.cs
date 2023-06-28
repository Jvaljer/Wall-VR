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

    //internal class : Dixit (representing one of all the dixit cards)
    public class DixitCard : MonoBehaviour {
        public GameObject go { get; set; } //CardGO in scene
        public PhotonView pv { get; set; }
        private int id;

        public DixitCard(Texture2D tex, int id_, Transform wall){
            Vector3 card_pos;
            Quaternion card_rota;
            if(wall==null){
                //in that case we are in 2D
                card_pos = new Vector3(0f,0f,1.5f); //putting card on 1.5f depth to allow bringing em forward
                card_rota = Quaternion.identity;
            } else {
                //in that case we are in VR
                card_pos = wall.position;
                card_rota = wall.rotation;
            }
            go = PhotonNetwork.InstantiateRoomObject("Dixit Card", card_pos, card_rota);
            go.GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
            pv = go.GetPhotonView();
            id = id_;
        }
    }

    //unity attributes
    private Setup setup;
    private NetworkHandler network_handler;
    private Operator ope;

    //geometry attibutes
    private float screen_ratio = 1f;
    private float pix_to_unit = 1f;
    private float sw;
    private float sh;
    public float sw_unity { get; private set; }
    public float sh_unity { get; private set; }
    private float ortho_size = 5f;

    //private float pixel_in_mm = 0.264275256f; //abel's laptop

    //dixits attributes
    private object[] dixits_tex;
    private Dictionary<string, DixitCard> dixits;

    //shapes attributes
    private Dictionary<string, GameObject> shapes;

    public void Start(){
        setup = GameObject.Find("ScriptManager").GetComponent<Setup>();
        network_handler = GameObject.Find("ScriptManager").GetComponent<NetworkHandler>();
        if(setup.dixits){
            dixits = new Dictionary<string, DixitCard>();
        } else {
            shapes = new Dictionary<string, GameObject>();
        }
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
        if(setup.dixits){
            setup.logger.Msg("received an input for the dixits manipulation", "S");
            //must implement
        } else {
            foreach(GameObject obj in shapes.Values){
                Shape obj_ctrl = obj.GetComponent<Shape>();
                switch (name){
                    case "Down":
                        setup.logger.Msg("Input Down is received by Render from "+id+" on "+new Vector2(m_x, m_y), "C");
                        if(obj_ctrl.CoordsInside(new Vector2(px,py))){
                            setup.logger.Msg("The Down Input is inside the shape", "V");
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

        if(setup.dixits){
            if(dixits_tex==null || dixits_tex.Length==0){
                LoadDixits();
            }
            if(dixits.Count==0 || dixits==null){
                if(setup.is_master){
                    CreateAllDixits();
                } else {
                    FetchDixits();
                }
            }
        } else {
            if(!shapes.ContainsKey("Circle0")){
                shapes.Add(GameObject.Find("Circle0").name,GameObject.Find("Circle0"));
            }
        }

        if(PhotonNetwork.IsMasterClient){
            sw = Screen.width;
            sh = Screen.height;
            screen_ratio = sh/sw;
            pix_to_unit = Camera.main.orthographicSize /(sh/2.0f);
            sw_unity = sw*pix_to_unit;
            sh_unity = sh*pix_to_unit;
            setup.logger.Msg("Render (Operator) is : cam_orthoSize="+Camera.main.orthographicSize+" sw="+sw+" sh="+sh+" PtU="+pix_to_unit+" swu="+sw_unity+" shu="+sh_unity, "C");
        } else {
            if(setup.is_vr){
                //simply replacing the shape on the wall ?
                if(setup.dixits){
                    //must implement
                } else {
                    shapes["Circle0"].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    shapes["Circle0"].transform.position = new Vector3(0f, 2.5f, 4.99f);
                }

                GameObject wall_go = GameObject.Find("WallGO");
                sw = wall_go.transform.localScale.x;
                sh = wall_go.transform.localScale.y;
                //put WallGO size instead ??
                pix_to_unit = (sw/2.0f) / (sh/1.0f);
                sw_unity = sw*pix_to_unit;
                sh_unity = sh*pix_to_unit;
                setup.logger.Msg("Render (VR) is : sw="+sw+" sh="+sh+" PtU="+pix_to_unit+" swu="+sw_unity+" shu="+sh_unity, "C");
            } else {
                sw = setup.wall_width;
                sh = setup.wall_height;
                screen_ratio = sh/sw;
                ortho_size = (float)Camera.main.orthographicSize / (float)setup.wall.RowsAmount();
                pix_to_unit = (float)setup.wall.RowsAmount() * (float)Camera.main.orthographicSize / (sh/2.0f);
                sw_unity = sw*pix_to_unit;
                sh_unity = sh*pix_to_unit;
                setup.logger.Msg("Render (Wall) is : cam_orthoSize="+Camera.main.orthographicSize+" sw="+sw+" sh="+sh+" PtU="+pix_to_unit+" swu="+sw_unity+" shu="+sh_unity, "C");

                if(setup.dixits){
                    //must implement
                } else {
                    foreach(GameObject shape in shapes.Values){
                        //zoom value = amount of division ?
                        shape.transform.localScale *= setup.zoom_ratio;
                    }
                }
            }
            PlaceDixits();
        }
    }

    public void LoadDixits(){
        dixits_tex = Resources.LoadAll("dixit_cards_all/", typeof(Texture2D));
        if(dixits_tex!=null){
            setup.logger.Msg("Loaded all dixits from dixit_cards_all/ : "+dixits_tex.Length, "V");
        } else {
            setup.logger.Msg("Failed to load all dixits from dixit_cards_all/", "E");
        }
    }

    public void CreateAllDixits(){
        setup.logger.Msg("Render starts creating All Dixits", "C");
        LoadDixits();

        if(dixits_tex.Length>0){
            //setting up cards variables & containers
            Texture2D tex;
            GameObject wall = GameObject.Find("WallGO"); //will return null if in 2D
            for(int i=0; i<45; i++){ //only loading the 45 first dixits
                tex = (Texture2D)dixits_tex[i];
                //Card initialization
                DixitCard card;
                if(wall==null){
                    card = new DixitCard(tex, i, null);
                } else {
                    card = new DixitCard(tex, i, wall.transform);
                }
                //giving x_ & y_ in real coordinates -> translation has already been done
                float swu = Screen.width*(Camera.main.orthographicSize /(Screen.height/2.0f));
                float shu = Screen.height*(Camera.main.orthographicSize /(Screen.height/2.0f));
                card.pv.RPC("InitializeDixit", RpcTarget.AllBuffered, card.pv.ViewID, i, setup.is_vr, swu, shu);
                //setup.logger.Msg(card.go.name+" is placed on : "+card.go.transform.position, "C");
                card.go.GetComponent<Card>().SetName("Dixit "+i);
                card.go.GetComponent<Card>().SetDixitClass(card);
                dixits.Add("Dixit "+i, card);
                setup.logger.Msg("created and added dixit n°"+i, "V");
            }
        } else {
            setup.logger.Msg("Dixits aren't loaded ...", "E");
        }
    }

    public void FetchDixits(){
        //must implement
        return;
    }

    public void PlaceDixits(){
        //must implement
        return;
    }
}