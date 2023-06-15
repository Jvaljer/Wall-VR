using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Shape : MonoBehaviourPun {
    public string title { get; private set; }
    public Setup setup { get; set; } 
    public string category { get; set; }
    public float size { get; set; }
    public Vector3 position { get; set; }
    private bool dragged = false;
    private bool vr = false;
    public List<int> owners { get; private set; } = new List<int>();

    //setters
    public void SetName(string str){
        gameObject.name = str;
        title = str;
    }
    [PunRPC]
    public void SetNameRPC(string str){
        SetName(str);
    }
    public void Categorize(string cat){
        category  = cat;
    }
    public void SetSize(float n){
        Debug.Log("setting size of shape");
        size = n;
    }
    public void PositionOn(Vector3 pos){
        gameObject.transform.position = pos;
        position = pos;
    }
    public void Pick(){
        dragged = true;
    }
    public void Drop(){
        dragged = false;
    }
    public void SetAsVR(){
        Debug.Log("setting the shape as VR");
        vr = true;
    }
    
    //getters
    public bool IsDragged(){
        return dragged;
    }

    public bool IsOwnedBy(int id){
        return owners.Contains(id);
    }

    //setters
    public void AddOwner(int id){
        owners.Add(id);
    }
    public void RemoveOwner(int id){
        owners.Remove(id);
    }
    public void ClearOwners(){
        owners.Clear();
    }

    //Some predicates
    public bool CoordsInside(Vector2 coord){
        bool cond = false;
        switch (category){
            case "circle":
                float dist = (float)Math.Sqrt(Math.Pow(coord.x - position.x, 2) + Math.Pow(coord.y - position.y, 2));
                float rad = size/2;
                cond = (dist <= rad);
                break;
            case "square":
                float half = size/2;
                //borders
                float up = position.y + half;
                float down = position.y - half;
                float left = position.x - half;
                float right = position.x + half;
                cond = (coord.x<=right) && (coord.x>=left) && (coord.y<=up) && (coord.y>=down);
                break;
            default:
                break;
        }
        return cond;
    }

    [PunRPC]
    public void PickRPC(){
        dragged = true;
    }

    [PunRPC]
    public void DropRPC(){
        dragged = false;
    }

    public void Move(float x_, float y_, float zoom){
        //received the correct coords so simply moving the shape
        float z_;
        if(vr){
            z_ = 4.99f;
        } else {
            z_ = 1f;
        }
        Vector3 new_pos = new Vector3(x_,y_,z_);
        gameObject.transform.position = new_pos;
        position = new_pos;
    }
}

