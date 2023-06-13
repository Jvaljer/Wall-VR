using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Setup : MonoBehaviourPun {
    //Gateway collected arguments
    public string[] args;
    //user's condition 
    public bool is_master { get; private set; } = false;
    public bool is_vr { get; private set; } = false;
    public string role_str { get; private set; }

    //master specific attributes
    public int part_cnt { get; private set; } = 1;
    public bool master_only { get; private set; } = false;
    
    //used wall & attributes
    public float wall_width { get; private set; } 
    public float wall_height { get; private set; }
    private string wall_str;
    public Wall wall { get; private set; }

    //wall part location 
    public float wall_pos_x { get; private set; }
    public float wall_pos_y { get; private set; }
    public float x_pos { get; private set; }
    public float y_pos { get; private set; }

    //other screen related attributes
    public int screen_width { get; set; } = 1024;
    public int screen_height { get; set; } = 512;
    public bool full_screen { get; private set; } = false;
    public float zoom_ratio { get; private set; } = 1f;

    public void Start(){
        Debug.LogError("Setup Starting on scene : "+SceneManager.GetActiveScene().name);
        args = Gateway.arguments;
        Debug.Log("got args : "+args);
        for(int i=0; i<args.Length; i++){
            Debug.Log(args[i]);
            switch (args[i]){
                case "-wall":
                    wall_str = args[i+1];
                    break;
                case "-sh":
                    screen_height = int.Parse(args[i+1]);
                    break;
                case "-sw":
                    screen_width = int.Parse(args[i+1]);
                    break;
                case "-fs":
                    full_screen = ((int.Parse(args[i+1]))!=0);
                    break;

                case "-r":
                    is_master = (args[i+1]=="m");
                    Debug.Log("Is Master : "+is_master);
                    break;

                case "-vr":
                    is_vr = ((int.Parse(args[i+1]))!=0);
                    break;

                case "-x":
                    x_pos = float.Parse(args[i+1]);
                    break;
                case "-y":
                    y_pos = float.Parse(args[i+1]);
                    break;

                case "-pa":
                    part_cnt = int.Parse(args[i+1]);
                    break;
                case "-mo":
                    if((int.Parse(args[i+1]))==1){
                        Debug.LogError("master only");
                        //yes
                        part_cnt = 1;
                        master_only = true;
                    }
                    break;

                default:
                    break;
            }
        }
        Debug.Log("ended parsing");
        switch (wall_str){
            case "WILDER":
                wall = new Wilder();
                break;
            case "DESKTOP":
                wall = new Desktop(2,2);
                break;
            default:
                break;
        }
        wall_height = wall.Height();
        wall_width = wall.Width();
        
        Debug.Log("Setup Connects to the Server");
        //now connecting to the server
        ConnectToServer();
    }

    public void ConnectToServer(){
        GameObject.Find("ScriptManager").GetComponent<NetworkHandler>().Connect();
    }
}
