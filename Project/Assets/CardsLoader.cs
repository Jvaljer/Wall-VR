using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardsLoader : MonoBehaviour {

    //all cards' textures
    private List<object> textures;

    //size of cards in a (0->1) side scaled wall
    private static float card_width = 0.04495f;
    private static float card_height = 0.23109f;

    //disposition values
    private static float gap_x = 0.03112f;
    float fst_x = gap_x + card_width/4f; //x coordinates of the first card (up left)
    float x_dist = 0.06581f; //distance between each card on the x axis

    //card creation method
    [PunRPC]
    public void InitializeDixit(int card_pv, int index, bool vr, float swu, float shu){
        Debug.Log("Initializing dixit "+index);

        /*
        There are 100 dixits overall :
        --> full repartition (maybe too much)
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
            object[] tmp_res = Resources.LoadAll("dixit_cards_all/", typeof(Texture2D));
            textures = new List<object>();

            System.Random rand = new System.Random();
            List<int> indexs = new List<int>();
            int rn;
            for(int i=0; i<45; i++){
                rn = rand.Next(0,tmp_res.Length);
                while(indexs.Contains(rn)){
                    rn = rand.Next(0,tmp_res.Length);
                }
                indexs.Add(rn);
            }

            foreach(int n in indexs){
                textures.Add(tmp_res[n]);
            }


        }

        Render r = GameObject.Find("ScriptManager").GetComponent<Render>();
        float x_, y_;
        if(index<15){
            y_ = 0.25f;
        } else if(index<30){
            y_ = 0.5f;
        } else {
            y_ = 0.75f;
        }
        
        x_ = fst_x + (index%15)*x_dist; //shall be accurate ... (must test)

        float x_pos = x_*swu- (swu/2f);
        float y_pos = (shu/2f) - y_*shu;
        float z_pos = 1.5f;

        if(vr){
            float tmp = y_pos+2.5f;
            y_pos = tmp;
            z_pos = 4.99f;
        }

        Debug.LogError("Mouse coordinates for the dixit n°"+index+" are "+new Vector2(x_,y_)+" and screen coordinates are "+new Vector2(x_pos, y_pos));
        
        Vector3 pos = new Vector3((float)x_pos, (float)y_pos, (float)z_pos);
        Vector3 scale = new Vector3(1f,1f,1f);

        GameObject card_go = PhotonView.Find(card_pv).gameObject;

        card_go.transform.position = pos;
        card_go.transform.localScale = scale;
    }
}