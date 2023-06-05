using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gateway : MonoBehaviour {
    public static string[] arguments;
    //public StreamWriter writer;

    public void Awake(){
        //string path = "WallVR/Single_WallVR/Assets/Resources/PersonalLogs/my_log.txt";
        //writer = new StreamWriter(path, true);

#if UNITY_EDITOR
  #if UNITY_EDITOR_WIN
        Debug.Log("Windows Editor -> VR Part by default");
        //must initialize all args
        arguments = new string[10];
        arguments[0] = "-vr";
        arguments[1] = "1";
        arguments[2] = "-r";
        arguments[3] = "p";
        arguments[4] = "-sw"; 
        arguments[5] = "1980"; 
        arguments[6] = "-sh"; 
        arguments[7] = "1080"; 
        arguments[8] = "-wall";
        arguments[9] = "DESKTOP";
        string args = "";
        for(int i=0; i<10; i++){
            args += arguments[i];
            args += " ";
        }
        Debug.Log("arguments : "+args);
        SceneManager.LoadScene("VR");

  #elif UNITY_EDITOR_LINUX
        Debug.LogError("Linux Editor -> Operator by default");
        //must initialize all args
        arguments = new string[10];
        arguments[0] = "-vr";
        arguments[1] = "0";
        arguments[2] = "-r";
        arguments[3] = "m";
        arguments[4] = "-sw"; 
        arguments[5] = "1024"; 
        arguments[6] = "-sh"; 
        arguments[7] = "512"; 
        arguments[8] = "-wall";
        arguments[9] = "DESKTOP";
        Debug.Log("arguments are : "+arguments);
        SceneManager.LoadScene("Wall");
  #else 
        Debug.LogError("Editor is not Windows nor Linux");
  #endif
#elif UNITY_STANDALONE
        arguments = System.Environment.GetCommandLineArgs();
  #if UNITY_STANDALONE_WIN 
        Debug.LogError("Windows Standalone -> must parse arguments, VR authorized");
        bool vr_scene = false;
        //WriteLog("Windows Standalone -> must parse arguments, VR authorized");
        for(int i=0; i<arguments.Length; i++){
            if(arguments[i]=="-vr"){
                if(int.Parse(arguments[i+1])==1){
                    vr_scene = true;
                } else {
                    vr_scene = false;
                }
            }
        }
        if(vr_scene){
            if(arguments.Count == 0){
                arguments = new string[10];
                arguments[0] = "-vr";
                arguments[1] = "1";
                arguments[2] = "-r";
                arguments[3] = "p";
                arguments[4] = "-sw"; 
                arguments[5] = "1980"; 
                arguments[6] = "-sh"; 
                arguments[7] = "1080"; 
                arguments[8] = "-wall";
                arguments[9] = "DESKTOP";
                string args = "";
                for(int i=0; i<10; i++){
                    args += arguments[i];
                    args += " ";
                }
                Debug.Log("arguments : "+args);
            }
            Debug.LogError("Loading VR Scene");
            //WriteLog("Loading VR Scene");
            SceneManager.LoadScene("VR");
        } else {
            Debug.LogError("Loading Wall Scene");
            //WriteLog("Loading Wall Scene");
            SceneManager.LoadScene("Wall");
        }
  #elif UNITY_STANDALONE_LINUX
        Debug.LogError("Linux Standalone -> must parse argument, VR prohibited");
        SceneManager.LoadScene("Wall");
  #else 
        Debug.LogError("Standalone is not Windows nor linux");
  #endif
#else
        Debug.Log("ELSE -=> NOTHING");
        Debug.LogError("Couldn't Identify the executive source");
#endif
    }

    /*public void WriteLog(string str){
        //writing some text
        writer.WriteLine(str);
        writer.Flush();
    }*/
}
