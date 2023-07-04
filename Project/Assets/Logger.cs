using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Logger : MonoBehaviour {
    private bool logs_on = true;
    private bool on_console = false;

    /*
        Global idea of this class is to offer a proper debugging displayer, 
    that would be normed & easily usable, plus offer the 
    */

    //Constructor
    public Logger(){
        //here we simply wanna know if we're gonna be in standalone or in editor
#if UNITY_EDITOR
        on_console = true;
#endif
    }
    
    public void Msg(string message, string category = ""){
        if(logs_on){
            switch (category){
                case "E":
                    //display an error
                    DisplayErr(message);
                    break;
                case "C":
                    //display a casual message
                    DisplayCasu(message);
                    break;
                case "EDO":
                    //display a message on editor only
                    DisplayOnEdit(message, category);
                    break;
                case "SAO":
                    //display the message on standalone only
                    DisplayOnStand(message, category);
                    break;
                case "V":
                    //display a validation
                    DisplayValid(message);
                    break;
                case "S":
                    //display a special message (such as a statement)
                    DisplaySpe(message);
                    break;

                default:
                    //display a casual message
                    DisplayCasu(message);
                    break;
            }
        }
    }

    public void DisplayErr(string msg){
        if(on_console){
            Debug.Log("(ERROR)-> "+msg);
        } else {
            Debug.LogError("(ERR)-> "+msg);
        }
    }

    public void DisplayCasu(string msg){
        if(on_console){
            Debug.Log(msg);
        } else {
            Debug.LogError(msg);
        }
    }

    public void DisplayValid(string msg){
        if(on_console){
            Debug.Log("(VALIDATION)-> "+msg);
        } else {
            Debug.LogError("(VAL)-> "+msg);
        }
    }

    public void DisplaySpe(string msg){
        if(on_console){
            Debug.Log("|----------( "+msg+" )----------|");
        } else {
            Debug.LogError("|----- "+msg+" -----|");
        }
    }

    public void DisplayOnEdit(string msg, string cat){
        if(on_console){
            Debug.Log(msg);
        }
    }

    public void DisplayOnStand(string msg, string cat){
        if(!on_console){
            Debug.LogError(msg);
        }
    }
}