using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardsLoader : MonoBehaviour {

    //all cards' textures
    object[] textures;

    //size of cards in a (0->1) side scaled wall
    private static float card_width = 0.04495f;
    private static float card_height = 0.23109f;

    //disposition values
    private static float gap_x = 0.057399f;
    float fst_x = gap_x + card_width/2f; //x coordinates of the first card (up left)
    float x_dist = 0.04841f;

    //card creation method
    [PunRPC]
    public void InitializeDixit(int card_pv, int i, bool vr, float swu, float shu){
        Debug.Log("Initializing dixit "+i);
        //dixits repartition on the wall (20 dixits)
        /*
           1 # # # # # # # # # 
           # # # # # # # # # 20
        */

        /*
        Miss Calculated -> 100 dixits not only 20...
        --> new repartition 
            1 # # # # # # # # # # # # # # # # # # # 20
            # # # # # # # # # # # # # # # # # # # # 40 
            # # # # # # # # # # # # # # # # # # # # 60
            # # # # # # # # # # # # # # # # # # # # 80
            # # # # # # # # # # # # # # # # # # # # 100
        --> how much do we really want 
          Guess that 45 would be enough ? 
          1 # # # # # # # # # # # # # # 
          # # # # # # # # # # # # # # # 
          # # # # # # # # # # # # # # 45
        */
        if(textures==null){
            Debug.Log("loading all textures");
            textures = Resources.LoadAll("dixit_cards_all/", typeof(Texture2D));
        }

        Debug.Log("fetching for render");
        Render r = GameObject.Find("ScriptManager").GetComponent<Render>();
        float x_, y_;

        Debug.Log("setting y_");
        if(i<10){
            y_ = 0.33333f;
        } else {
            y_ = 0.66666f;
        }

        Debug.Log("y_ is "+y_+" -> now setting x_");
        x_ = fst_x + (i%10)*(card_width + x_dist); //shall be accurate ... (must test)

        float x_pos = x_*swu- (swu/2f);
        float y_pos = (shu/2f) - y_*shu;
        float z_pos = 1.5f;

        if(vr){
            float tmp = y_pos+2.5f;
            y_pos = tmp;
            z_pos = 4.99f;
        }

        Vector3 pos = new Vector3((float)x_pos, (float)y_pos, (float)z_pos);
        Vector3 scale = new Vector3(1f,1f,1f);

        GameObject card_go = PhotonView.Find(card_pv).gameObject;

        Debug.Log("Setting its position to "+pos);
        card_go.transform.position = pos;
        card_go.transform.localScale = scale;
    }
}