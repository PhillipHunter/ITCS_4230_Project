﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Gun : MonoBehaviour {
    //Don't touch this unless you're sure
    private const float SOUND_FILE_LENGTH = 10.0f;

    private const int NUM_OF_BULLETS = 30;
    private const float BULLET_SPEED = 20;
    private const float FIRE_DELAY = 0.08f;
    private const float RELOAD_DELAY = 2.0f;
    private const int RELOAD_AMOUNT = 1;
    private const float SOUND_PLAY_LENGTH = 0.08f;

    private float currentFireDelay = 0;
    private float currentReloadDelay = 0;
    private Stack<GameObject> clip;
    private Stack<GameObject> unloaded;
    public int id;
    private GameObject gui;

    public void Load (GameObject bullet)
    {
        bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
        bullet.transform.position = new Vector3(0, -1000, 0);
        unloaded.Push(bullet); //I know it doesn't make since, its a recent change
    }

    void UpdateUI()
    {
        gui.GetComponent<Text>().text = "Ammo: " + clip.Count;
    }


    public GameObject CreateBullet()
    {
        Vector3 rotation = this.gameObject.transform.forward;
        Vector3 position = this.gameObject.transform.position;

        GameObject o = Resources.Load<GameObject>("Bullet");

        GameObject o2 = (GameObject)Instantiate(o);
        //GameObject o2 = PhotonNetwork.Instantiate("Bullet", new Vector3(0, -10, 0), Quaternion.identity, 0);
        Color c = ColorAlgorithm.GetColor(id);

        o2.GetComponent<Renderer>().material.color = c;
        o2.GetComponentInChildren<Light>().color = c;
        o2.GetComponent<Bullet>().gun = this;
        o2.layer = 22; //The bullet layer
        o2.transform.position = new Vector3(0, -1000, 0);



        //o2.transform.position = position + rotation * 0.7f;
        //bullet.transform.rotation.SetLookRotation(rotation * 3);
        o2.GetComponent<Rigidbody>().velocity = rotation * BULLET_SPEED;
        //o2.GetComponent<Bullet>().network = true;
        //o2.GetComponent<Bullet>().Fire();
        return o2;
    }

    public void NetworkShoot()
    {
        if (clip.Count == 0) clip.Push(CreateBullet());
            this.GetComponent<AudioSource>().time = SOUND_FILE_LENGTH - SOUND_PLAY_LENGTH;
            this.GetComponent<AudioSource>().Play();
            currentFireDelay = FIRE_DELAY;
            currentReloadDelay = RELOAD_DELAY;
            Vector3 rotation = this.gameObject.transform.forward;
            Vector3 position = this.gameObject.transform.position;

            GameObject bullet = (GameObject)clip.Pop();
            bullet.transform.position = position + rotation * 0.7f;
            //bullet.transform.rotation.SetLookRotation(rotation * 3);
            bullet.GetComponent<Rigidbody>().velocity = rotation * BULLET_SPEED;
            bullet.GetComponent<Bullet>().Fire();
            NetworkPlayer.Shoot(this.id);
            this.UpdateUI();
    }

    public void Shoot()
    {
        //if (this.gameObject.GetComponentInParent<NetworkPlayer>().id != NetworkPlayer.mainID) return;
        try
        {
            if (clip != null && clip.Count > 0)
            {
                this.GetComponent<AudioSource>().time = SOUND_FILE_LENGTH - SOUND_PLAY_LENGTH;
                this.GetComponent<AudioSource>().Play();
                currentFireDelay = FIRE_DELAY;
                currentReloadDelay = RELOAD_DELAY;
                Vector3 rotation = this.gameObject.transform.forward;
                Vector3 position = this.gameObject.transform.position;

                GameObject bullet = (GameObject)clip.Pop();
                bullet.transform.position = position + rotation * 0.7f;
                //bullet.transform.rotation.SetLookRotation(rotation * 3);
                bullet.GetComponent<Rigidbody>().velocity = rotation * BULLET_SPEED;
                bullet.GetComponent<Bullet>().Fire();
                NetworkPlayer.Shoot(this.id);
                this.UpdateUI();
            }
        }
        catch (InvalidOperationException e) { }
    }

	// Use this for initialization
	void Start () {

    }

    public void SetTeam (int id)
    {
        this.id = id;
        Color c = ColorAlgorithm.GetColor(id);
        this.SetColor(c);
        for(int i = 0; i < NUM_OF_BULLETS; i++)
        {
            GameObject o = Resources.Load<GameObject>("Bullet");

            GameObject o2 = (GameObject)Instantiate(o);
            //GameObject o2 = PhotonNetwork.Instantiate("Bullet", new Vector3(0, -10, 0), Quaternion.identity, 0);

            o2.GetComponent<Renderer>().material.color = c;
            o2.GetComponentInChildren<Light>().color = c;
            o2.GetComponent<Bullet>().gun = this;
            o2.layer = 22; //The bullet layer
            o2.transform.position = new Vector3(0, -1000, 0);

            if (clip == null)
            {
                clip = new Stack<GameObject>();
                
            }
            clip.Push(o2);
        }
        this.gui = GameObject.Find("AmmoCounter");
        UpdateUI();
    }

    void SetColor(Color c)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer curr in renderers)
        {            
            if (!curr.material.name.Contains("GunMetal")) //Lol
            {
                curr.material.color = c;
            }            
        }
    }

    // Update is called once per frame
    void Update () {
        if(currentReloadDelay > 0) currentReloadDelay -= Time.deltaTime;
        if(unloaded == null)
        {
            unloaded = new Stack<GameObject>();
            return;
        }
        if (clip.Count < NUM_OF_BULLETS)
        {
            if (currentReloadDelay <= 0)
            {
                if (clip.Count == NUM_OF_BULLETS)
                {
                    return;
                }
                if (unloaded.Count == 0) unloaded.Push(CreateBullet());
                clip.Push(unloaded.Pop());
                this.UpdateUI();
            }
        }
        if(currentFireDelay > 0)
        {
            currentFireDelay -= Time.deltaTime;
            return;
        }
        if (this.id == NetworkPlayer.mainID && Input.GetMouseButton(0))
        {
            this.Shoot();
        }
	}
}
