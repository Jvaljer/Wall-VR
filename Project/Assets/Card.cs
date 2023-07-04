using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Card : MonoBehaviourPun {
    public Render.DixitCard dixit_class { get; set; }
    public string go_name { get; private set; }
    public bool dragged { get; private set; } = false;
    private bool vr = false;
    public int current_owner { get; set; } //id of the player moving the card (-1 by default)
    public Vector3 go_pos { get; private set; }
    private float go_width;
    private float go_height;

    //setters
    public void SetDixitClass(Render.DixitCard dix_class){
        dixit_class = dix_class;
    }

    public void SetAttributes(float w_, float h_){
        go_width = w_;
        go_height = h_;
    }

    [PunRPC]
    public void SetNameRPC(string str){
        gameObject.name = str;
        go_name = str;
    }

    public void PositionOn(Vector3 pos){
        gameObject.transform.position = pos;
        go_pos = pos;
    }

    public void SetAsVR(){
        vr = true;
    }

    //manipulation method & RPC
    public bool ClickIsInside(Vector2 click){
        bool cond = false;

        float half_w = go_width/2f;
        float half_h = go_height/2f;

        float up = go_pos.y + half_h;
        float down = go_pos.y - half_h;
        float left = go_pos.x - half_w;
        float right = go_pos.x + half_w;
        //Debug.Log("bounds are : up->"+up+" down->"+down+" left->"+left+" right->"+right+"  with hw & hh -> "+half_w+" & "+half_h);
        cond = (click.x<=right) && (click.x>=left) && (click.y<=up) && (click.y>=down);

        return cond;
    }

    [PunRPC]
    public void PickRPC(int id_){
        dragged = true;
        current_owner = id_;
    }

    [PunRPC]
    public void DropRPC(){
        dragged = false;
        current_owner = -1; //resetting the current owner
    }

    public void Move(float x_, float y_){
        //coords are already translated -> simply have to move the card
        float z_;

        if(vr){
            z_ = 4.99f;
        } else {
            z_ = 2f;
        }

        Vector3 new_pos = new Vector3(x_, y_, z_);
        gameObject.transform.position = new_pos;
        go_pos = new_pos;
    }
}