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
    //unity attributes
    private Setup setup;
    private NetworkHandler network_handler;
    private Operator ope;

    //geometry attibutes
    private float screen_ratio = 1f;
    private float pix_to_unit = 1f;
    private float sw;
    private float sh;
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

    public void Input(string name, Vector3 coord, int id){
        //first we wanna check which one of the shape we are tryna move
        foreach(GameObject obj in shapes.Values){
            //Debug.Log("for shape : "+obj.name);
            Shape obj_ctrl = obj.GetComponent<Shape>();
            if(obj_ctrl.IsOwnedBy(id)){ //later on we'll like to add more scripts + abstract class
                //Debug.Log("we have the owner : "+id);
                switch (name){
                    case "Down":
                        if(obj_ctrl.CoordsInside(coord)){
                            obj.GetComponent<PhotonView>().RPC("PickRPC", RpcTarget.AllBuffered);
                        }
                        break;
                    case "Move":
                        //already tested if dragging ? test it again ?
                        if(obj_ctrl.IsDragged()){
                            //then move shape depending on role
                            obj.GetComponent<PhotonView>().RPC("MoveRPC", RpcTarget.AllBuffered, coord, setup.zoom_ratio);
                        }
                        break;
                    case "Up":
                        obj.GetComponent<PhotonView>().RPC("DropRPC", RpcTarget.AllBuffered);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void InitializeFromIH(Operator O_){
        ope = O_;
        if(!shapes.ContainsKey("Circle(Clone)")){
            shapes.Add(GameObject.Find("Circle(Clone)").name,GameObject.Find("Circle(Clone)"));
        }

        if(PhotonNetwork.IsMasterClient){
            sw = Screen.width;
            sh = Screen.height;
            screen_ratio = sh/sw;
            pix_to_unit = Camera.main.orthographicSize /(sh/2.0f);
            abs = 0.1f;
            ih_scale = ih_scale*abs;
        } else {
            if(setup.is_vr){
                //simply replacing the shape on the wall ?
                shapes["Circle(Clone)"].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                shapes["Circle(Clone)"].transform.position = new Vector3(0f, 2.5f, 4.99f);
            } else {
                sw = setup.wall_width;
                sh = setup.wall_height;
                screen_ratio = sh/sw;
                ortho_size = (float)Camera.main.orthographicSize / (float)setup.wall.RowsAmount();
                pix_to_unit = (float)setup.wall.RowsAmount() * (float)Camera.main.orthographicSize / (sh/2.0f);
                abs = 1.0f;
                foreach(GameObject shape in shapes.Values){
                    //zoom value = amount of division ?
                    //shape.transform.localScale *= 2f;
                    shape.transform.localScale *= setup.zoom_ratio;
                }
            }
        }
    }

    public void NewShape(string name, Vector3 pos, int id, string cat){
        Debug.LogError("Render -> NewShape");
        //shape already created so just need to get it
        GameObject new_shape = GameObject.Find(name);
        if(new_shape!=null){
            Shape shape_ctrl = new_shape.GetComponent<Shape>();
            shape_ctrl.Categorize(cat);
            shape_ctrl.SetSize(new_shape.transform.localScale.x);
            shape_ctrl.PositionOn(pos);
            shape_ctrl.AddOwner(id);
            shapes.Add(name, new_shape);
            if(!PhotonNetwork.IsMasterClient){
                Debug.LogError("not MC, sizing shape up");
                new_shape.transform.localScale *= setup.zoom_ratio;
            }
        } else {
            Debug.LogError("can't get the shape bro");
        }
    }
}
