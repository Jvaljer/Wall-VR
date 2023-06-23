using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardsLoader : MonoBehaviour {

    //all cards' textures
    object[] textures;

    //size of cards
    private static float card_width = 0.033f;
    private static float card_height = 0.239f;

    //card creation method
    [PunRPC]
    public void InitializeDixit(int card_pv, int i, bool master, bool vr){
        if(textures==null){
            textures = Resources.LoadAll("dixit_cards_all/", typeof(Texture2D));
        }

        //we have 2 possibilities
        //first of all we are in 2D -> dixit are simply created onto the scene
        //second case we are in VR -> dixit are attached to the wallGO
        if(master){
            //must implement
        } else {
            if(vr){
                //must implement
            } else {
                //must implement
            }
        }

        return;
    }
}