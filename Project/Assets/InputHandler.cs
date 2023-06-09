using System;
using System.IO;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

public class InputHandler : MonoBehaviourPun {
    //operator & cursors prefab
    private Operator ope;
    public GameObject cursor_prefab;
    private object vr_ref;

    //referenced setup, render & network
    private Setup setup;
    private Render render;
    private NetworkHandler network;

    //Some cursors attributes
    private static int cursor_HW = 16;
    private static int cursor_T = 1;
    private static int cursor_L = 1;

    //creator attributes
    private int uid_creator = 0;

    //Cursors's dictionnary
    public Dictionary<int, PCursor> p_cursors { get; set; }
    public Dictionary<int, MCursor> m_cursors { get; set; }
    public Dictionary<PCursor, GameObject> vr_cursors { get; set; } //specific to the VR users

    //Devices's dictionnary
    public Dictionary<object, MDevice> m_devices { get; set; }

    //must delete list (cursors)
    private List<int> to_delete_ids;

    //dixit list
    private List<GameObject> dixits_go;

    public bool initialized { get; set; } = false;
    public bool dixits_created { get; private set; } = false;
    public bool part_pc_init { get; private set; } = true;

    /******************************************************************************/
    /*                           MAIN BEHAVIOR METHODS                            */
    /******************************************************************************/
    private void Awake(){
        //initializing all containers vars
        m_devices = new Dictionary<object, MDevice>();
        p_cursors = new Dictionary<int, PCursor>();
        to_delete_ids = new List<int>();
        vr_cursors = new Dictionary<PCursor, GameObject>();
    }

    private void Update(){
        if(photonView.IsMine && initialized){
            Vector2 mouse_pos = Mouse.current.position.ReadValue();
            float mouse_x = mouse_pos.x/Screen.width;
            float mouse_y = (Screen.height - mouse_pos.y)/Screen.height;

            //handling operator's mouse
            if(Mouse.current.leftButton.wasPressedThisFrame){
                StartMoveMCursor(this, 0, mouse_x, mouse_y, true);
                setup.logger.Msg("Mouse Down -> sending a simple input via RPC", "S");
                photonView.RPC("InputRPC", RpcTarget.AllBuffered, "Down", mouse_x, mouse_y, 0);
            } else if(Mouse.current.leftButton.wasReleasedThisFrame){
                StopMoveMCursor(this, 0, mouse_x, mouse_y);
                photonView.RPC("InputRPC", RpcTarget.AllBuffered, "Up", mouse_x, mouse_y, 0);
            } else {
                MoveMCursor(this, 0, mouse_x, mouse_y);

                if(GetMCursor(this,0).drag){
                    photonView.RPC("InputRPC", RpcTarget.AllBuffered, "Move", mouse_x, mouse_y, 0);
                }
            }

            //handling cursors
            to_delete_ids.Clear();
            foreach(MDevice dev in m_devices.Values){
                foreach(MCursor mc in dev.cursors.Values){
                    //if not related PCursor then create it
                    if(mc.p_cursor==null){
                        mc.AddPCursor(new PCursor(mc.x, mc.y, mc.c));
                        setup.logger.Msg("creating a PCursor", "S");
                        byte[] c_data = ColorUtility.SerializeColor(mc.c);  // Serialize the Color object
                        string c_str = Convert.ToBase64String(c_data);
                        photonView.RPC("CreatePCursorRPC", RpcTarget.AllBuffered, uid_creator, mc.x, mc.y, c_str);
                        mc.uid = uid_creator;
                        uid_creator++;
                    }

                    //if MCursor to delete then delete it 
                    if(mc.to_delete){
                        to_delete_ids.Add(mc.id);
                        mc.RemovePCursor();
                        p_cursors.Remove(mc.uid);
                        if(!mc.hidden){
                            photonView.RPC("RemovePCursorRPC", RpcTarget.AllBuffered, mc.uid);
                        }
                    } else if(mc.x != mc.p_cursor.x || mc.y != mc.p_cursor.y){
                        photonView.RPC("MoveOrCreatePCursorRPC", RpcTarget.AllBuffered, mc.uid, mc.x, mc.y, mc.c.ToString());
                        mc.p_cursor.Move(mc.x, mc.y);
                    }

                    //drag predicates handling
                    if(mc.clicked){
                        mc.clicked = false;
                    }
                }

                foreach(int id in to_delete_ids){
                    dev.RemoveCursor(id);
                }
                to_delete_ids.Clear();
            }
        }
    }

    private void OnGUI(){
        if(initialized){
            foreach(PCursor pc in p_cursors.Values){
                Vector3 dst;
                if(PhotonNetwork.IsMasterClient){
                    dst = CoordOfMouseToOpe(pc.Coord());
                } else {
                    if(setup.is_vr){
                        dst = CoordOfMouseToVR(pc.Coord());
                        dst.y -= 2.5f;
                    } else {
                        dst = CoordOfMouseToWall(pc.Coord());
                    }
                }
                if(setup.is_vr){
                    //here we wanna move the related GO in the VR scene 
                    dst.z = vr_cursors[pc].transform.position.z;
                    vr_cursors[pc].transform.position = dst;
                } else {
                    if(setup.is_master){
                        GUI.DrawTexture(new Rect(dst.x - cursor_HW, dst.y - cursor_HW, setup.zoom_ratio*cursor_HW, setup.zoom_ratio*cursor_HW), pc.tex);
                    } else {
                        if(part_pc_init){
                            pc.tex = CursorsTex.SimpleCursor(pc.c, Color.black, cursor_HW, (int)setup.zoom_ratio, (int)setup.zoom_ratio);
                            part_pc_init = false;
                        }
                        GUI.DrawTexture(new Rect(dst.x - cursor_HW, dst.y - cursor_HW, setup.zoom_ratio*cursor_HW, setup.zoom_ratio*cursor_HW), pc.tex);
                    }
                }
            }
        }
    }

    /******************************************************************************/
    /*                 SYNCHRONIZATION CLASS AND METHODS                          */
    /******************************************************************************/

    public void InitializeFromOpe(){
        ope = gameObject.GetComponent<Operator>();
        setup = GameObject.Find("ScriptManager").GetComponent<Setup>();
        render = GameObject.Find("ScriptManager").GetComponent<Render>();
        network = GameObject.Find("ScriptManager").GetComponent<NetworkHandler>();
        vr_ref = gameObject.GetComponent<Operator>(); //might specify with abstract class ? not really needed I guess

        if(photonView.IsMine){
            //I am me -> operator so set cursor visible
            // + register my mouse as a device
            Cursor.visible = true; 
            RegisterDevice("Mouse", this);
            CreateMCursor(this, 0, 0.5f, 0.5f, Color.red);
            //now registering the device that will hold all VR cursors
            RegisterDevice("VR",vr_ref); //associating operator as an object
        } else {
            if(setup.is_vr){
                if(setup.dixits){
                    //must implement
                } else {
                    GameObject.Find("Circle0").GetComponent<Shape>().SetAsVR();
                }
            } else {
                //set the cursors invisible & scale em
                Cursor.visible = false;
                cursor_HW = 16*4;
            }
        }

        if(setup.dixits){
            //must implement
        } else {
            GameObject.Find("Circle0").GetComponent<Shape>().AddOwner(0);
        }
        setup.logger.Msg("Initialized from Ope", "V");
    }

    public void ParticipantIsReady(int n =-1){
        if(!photonView.IsMine){
            setup.logger.Msg("Received PartIsReady "+n, "C");
            if(setup.is_vr){
                if(n!=-1){
                    ope.GetComponent<PhotonView>().RPC("AddVRCursor", RpcTarget.AllBuffered, n);
                }
            }
        }
        initialized = true;
        setup.logger.Msg("InputHandler has initialized", "V");
        if(setup.is_master){
            GameObject.Find("ScriptManager").GetComponent<SmartiesManager>().StartFromOpe(setup, ope);
        }
        render.InitializeFromIH(ope);
    }

    [PunRPC]
    public void InputRPC(string str, float x_, float y_, int id_){
        //(x,y) are in (0,0)-(1,1)
        //we wanna send em to each render
        //setup.logger.Msg("FROM InputRPC -> CALLING render.Input", "S");
        render.Input(str, x_, y_, id_);
    }

    /******************************************************************************/
    /*                 MASTER CURSORS CLASS AND METHODS                           */
    /******************************************************************************/
    public class MCursor {
        //all Master Cursor's attributes
        public PCursor p_cursor { get; private set; }
        public int id { get; private set; }
        public int uid { get; set; }
        public Color c { get; set; }
        public float x { get; private set; }
        public float y { get; private set; }
        public int button { get; set; }
        public bool hidden { get; set; }
        public bool drag { get; set; }
        public bool clicked { get; set; }
        public bool to_delete { get; set; }

        //Constructor
        public MCursor(int id_, float x_, float y_, Color c_){
            id = id_;
            x = x_;
            y = y_;
            c = c_;
            p_cursor = null;
            drag = false;
            clicked = false;
        }

        //setters
        public void Move(float x_, float y_){
            x = x_;
            y = y_;
        }

        public void AddPCursor(PCursor pc){
            p_cursor = pc;
        }
        public void RemovePCursor(){
            p_cursor = null;
        }

        public void Pick(){
            drag = true;
        }
        public void Drop(){
            drag = false;
        }
    };

    //all related methods
    public MCursor GetMCursor(object obj, int id_){
        if(m_devices.ContainsKey(obj)){
            return m_devices[obj].GetCursor(id_);
        }
        return null;
    }

    public void CreateMCursor(object obj, int id_, float x_, float y_, Color c_, bool hid_=false){
        setup.logger.Msg("New MCursor "+id_+" attached to "+obj+" of color "+c_, "V");
        MDevice device = GetDevice(obj);
        if(device==null){
            return;
        }
        MCursor cursor = device.GetCursor(id_);
        if(cursor!=null){
            return;
        }

        cursor = device.CreateCursor(id_, x_, y_, c_);
        cursor.hidden = hid_;
    }

    public void RemoveMCursor(object obj, int id_){
        MDevice device = GetDevice(obj);
        if(device==null){
            return;
        }
        MCursor cursor = device.GetCursor(id_);
        if(cursor==null){
            return;
        }
        cursor.to_delete = true;
    }

    //moving methods
    public void StartMoveMCursor(object obj, int id_, float x_, float y_, bool d_){
        MCursor cursor = GetMCursor(obj, id_);
        if(cursor==null){
            return;
        }
        cursor.Pick();
    }
    public void MoveMCursor(object obj, int id_, float x_, float y_){
        MCursor cursor = GetMCursor(obj, id_);
        if(cursor==null){
            return;
        }
        cursor.Move(x_, y_);
    }
    public void StopMoveMCursor(object obj, int id_, float x_, float y_){
        MCursor cursor = GetMCursor(obj, id_);
        if(cursor==null){
            return;
        }
        cursor.Drop();
    }

    public void CursorClick(object obj, int id_, float x_, float y_, int btn=0){
        MCursor cursor = GetMCursor(obj, id_);
        if(cursor==null){
            return;
        }
        cursor.clicked = true;
        cursor.button = btn;
    }
    /******************************************************************************/
    /*            PARTICIPANT CURSORS CLASS AND METHODS                           */
    /******************************************************************************/
    public class PCursor{
        public float x { get; private set; }
        public float y { get; private set; }
        public Color c { get; set; }
        public Texture2D tex { get; set; }

        public PCursor(float x_, float y_, Color c_){
            x = x_;
            y = y_;
            c = c_;
            tex = CursorsTex.SimpleCursor(c, Color.black, cursor_HW, cursor_T, cursor_L);
        }

        public Vector3 Coord(){
            return (new Vector3(x,y,0f)); //returning coords as on mouse plan (no z-axis)
        }

        public void Move(float x_, float y_){
            x = x_;
            y = y_;
        }
    };

    //related methods
    public PCursor GetPCursor(object obj, int id_){
        if(GetMCursor(obj, id_)!=null){
            return GetMCursor(obj, id_).p_cursor;
        }
        return null;
    }

    //RPC to create a PCursor
    [PunRPC]
    public void CreatePCursorRPC(int uid, float x_, float y_, string c_str){
        byte[] c_data = Convert.FromBase64String(c_str);
        Color color = ColorUtility.DeserializeColor(c_data);
        setup.logger.Msg("color of the PCursor is "+color+" from "+c_str, "S");
        p_cursors.Add(uid, new PCursor(x_, y_, color));
        //now we wanna add a GO to the VR scene in order to keep a visual trace of the pcursor
        if(setup.is_vr){
            //first creating a new cursor from the prefab and put it on center of wall
            GameObject pc_go = Instantiate(cursor_prefab, Vector3.zero, Quaternion.identity);
            string pc_name = "cursor"+uid;
            pc_go.name = pc_name;
            //translate the coordinates to the wall ones.
            pc_go.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
            pc_go.transform.position = new Vector3(0f,2.5f,5f);
            //now positionning correctly the cursor
            vr_cursors.Add(p_cursors[uid], pc_go);
        }
    }

    //RPC to remove a PCursor
    [PunRPC]
    public void RemovePCursorRPC(int uid){
        vr_cursors.Remove(p_cursors[uid]);
        p_cursors.Remove(uid);
        Destroy(GameObject.Find("cursor"+uid)); //is the Find() absolute or relative ?
    }

    //RPC to move a PCursor
    [PunRPC]
    public void MoveOrCreatePCursorRPC(int uid, float x_, float y_, string c_str){
        if(!p_cursors.ContainsKey(uid)){
            byte[] c_data = Convert.FromBase64String(c_str);
            Color color = ColorUtility.DeserializeColor(c_data);
            p_cursors.Add(uid, new PCursor(x_, y_, color));
        } else {
            p_cursors[uid].Move(x_, y_);
        }
    }

    /******************************************************************************/
    /*                 MASTER DEVICES CLASS AND METHODS                           */
    /******************************************************************************/
    public class MDevice {
        public string name { get; private set; }
        public Dictionary<int, MCursor> cursors { get; set; }

        //Constructor
        public MDevice(string str){
            name = str;
            cursors = new Dictionary<int, MCursor>();
        }

        //specific cursor getter
        public MCursor GetCursor(int id_){
            if(cursors.ContainsKey(id_)){
                return cursors[id_];
            }
            return null;
        }

        //cursor creating method (adds & returns a new cursor);
        public MCursor CreateCursor(int id_, float x_, float y_, Color c_){
            MCursor cursor = new MCursor(id_, x_, y_, c_);
            cursors.Add(id_, cursor);
            return cursor;
        }

        //cursor removing method
        public void RemoveCursor(int id_){
            cursors.Remove(id_);
        }
    };

    public MDevice GetDeviceFromName(string name){
        foreach(MDevice device in m_devices.Values){
            if(device.name==name){
                return device;
            }
        }
        return null;
    }

    public MDevice GetDevice(object obj){
        if(m_devices.ContainsKey(obj)){
            return m_devices[obj];
        }
        return null;
    }

    public void RegisterDevice(string str, object obj){
        //if the object is already referrring a device -> nothing to register
        if(GetDevice(obj)!=null){
            setup.logger.Msg("The device '"+str+"' already exists", "E");
            return;
        }
        setup.logger.Msg("Registering the device '"+str+"'", "V");
        m_devices.Add(obj, new MDevice(str));
    }
    
    /******************************************************************************/
    /*                        SHAPES & VR HANDLING METHODS                        */
    /******************************************************************************/
    public void AddVRCursorFromOpe(int n = -1){
        setup.logger.Msg("Adding the VR Cursor "+n, "C");
        CreateMCursor(vr_ref, n, 0.5f, 0.5f, Color.green);
    }

    public void InputFromVR(string name, Vector3 input, int id){
        //here we wanna first get the associated cursor
        setup.logger.Msg("Input Handler is receiving the VR Input","V");
        Vector3 mouse_input = CoordOfVRToMouse(input);

        if(photonView.IsMine){ //only needed for operator
            MCursor mc = GetMCursor(vr_ref, id);
            if(mc==null){
                setup.logger.Msg("cursor is null for id "+id, "E");
                return;
            }
            if(name=="Move"){
                mc.Move(mouse_input.x, mouse_input.y);
            }
        }

        //mc is the cursor we wanna move onto the coord 'input'
        switch (name) {
            case "Move":
                //because visual pos will be adjusted later on
                render.Input("Move", mouse_input.x, mouse_input.y, id);
                break;
            case "Down":
                //in this case we do the same as for the mouse press but with the VR id
                setup.logger.Msg("The received input is "+name+" from "+id, "C");
                render.Input("Down", mouse_input.x, mouse_input.y, id);
                break;
            case "Up":
                //in this case we do the same as for the mouse release but with the VR id
                render.Input("Up", mouse_input.x, mouse_input.y, id);
                break;
                
            //must implement all other asap
            case "JoyDown":
                //dunno what to do in this case
                break;
            case "JoyUp":
                //dunno what to do in this case
                break;
            default:
                break;
        }
    }

    /******************************************************************************/
    /*                        TRANSLATING COORDS FUNCTIONS                        */
    /******************************************************************************/
    
    public Vector3 CoordOfMouseToOpe(Vector3 mouse){
        Vector3 ope_c = Vector3.zero;
        ope_c.x = mouse.x * Screen.width;
        ope_c.y = mouse.y * Screen.height;
        return ope_c;
    }

    public Vector3 CoordOfMouseToVR(Vector3 mouse){
        Vector3 vr_c = Vector3.zero;
        vr_c.x = 10f*mouse.x -5f;
        vr_c.y = 5f*(1f-mouse.y) + 2.5f;
        vr_c.z = 4.99f;
        return vr_c;
    }

    public Vector3 CoordOfMouseToWall(Vector3 mouse){
        Vector3 wall_c = Vector3.zero;
        wall_c.x = -setup.x_pos + mouse.x * setup.wall_width;
        wall_c.y = -setup.y_pos + mouse.y * setup.wall_height;
        return wall_c;
    }

    public Vector3 CoordOfVRToMouse(Vector3 vr){
        Vector3 mouse_c = Vector3.zero;
        /* mouse_c.x = (vr.x - 5f)/10f;
        mouse_c.y = -1f*(((vr.y-2.5f)/5f) -1f);
        mouse_c.z = 0f; */ //ALMOST WORKING
        mouse_c.x = (vr.x)/10f + 0.5f;
        mouse_c.y = (-1f/5f)*vr.y +1f;
        return mouse_c;
    }

    public Vector3 CoordOfVRToOpe(Vector3 vr){
        Vector3 ope_c = Vector3.zero;
        ope_c.x = (10f*vr.x)/5f;
        ope_c.y = (5f*(vr.y - 2.5f))/2.5f;
        ope_c.y *= -1f;
        return ope_c;
    }

    // COLOR HANDLING CLASS & METHODS
    public static class ColorUtility {
        public static byte[] SerializeColor(Color color){
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms)){
                writer.Write(color.r);
                writer.Write(color.g);
                writer.Write(color.b);
                writer.Write(color.a);
                return ms.ToArray();
            }
        }

        public static Color DeserializeColor(byte[] colorData){
            using (MemoryStream ms = new MemoryStream(colorData))
            using (BinaryReader reader = new BinaryReader(ms)){
                float r = reader.ReadSingle();
                float g = reader.ReadSingle();
                float b = reader.ReadSingle();
                float a = reader.ReadSingle();
                return (new Color(r, g, b, a));
            }
        }
    }
}
