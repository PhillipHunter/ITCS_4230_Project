﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public static bool atSchool = true;
    public static GameController _GameController;
    public List<NetworkPlayer> players;
    public Transform xy_wall, yz_wall;   

    private void Start()
    {
        if (_GameController != null)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        _GameController = this;

        PhotonNetwork.isMessageQueueRunning = true;

        if (!PhotonNetwork.connected && !atSchool)
        {
            SceneManager.LoadScene("MainMenu");

            if (!Application.isEditor)
            {
                PersistentController.AddStatus("Attempted to start game with no connection!", true);
            }
            return;
        }
        GenerateLevel();
        CreatePlayer();
    }

    public void CreatePlayer()
    {
        Debug.Log("Creating player.");

        if (atSchool)
        {
            GameObject o = Resources.Load<GameObject>("SinglePlayerFPSController");

            GameObject i = (GameObject)Instantiate(o, new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10)), Quaternion.identity);


            i.layer = LayerMask.NameToLayer("Player " + (playerID + 2));



            GameObject gun = Resources.Load<GameObject>("Gun");

            GameObject guno = (GameObject)Instantiate(gun);

            Vector3 pos = i.transform.position;

            guno.transform.position = pos + i.transform.forward * 2;

            guno.transform.parent = i.transform;
            guno.GetComponent<Gun>().SetTeam(playerID);

            
            Camera c = i.GetComponentInChildren<Camera>();
            teamCamera(c, playerID+1);
            return;
        }

        PhotonNetwork.Instantiate("NetworkedFPSController", new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10)), Quaternion.identity, 0);
    }

    private const int teamoffset = 8;
    private const int numberOfTeams = 7;
    private static void teamCamera(Camera c, int team)
    {
        int currentTeamsWalls = team;
        for (int i = 0; i < numberOfTeams; i++)
        {
            if (i != currentTeamsWalls) hideLayer(c, "Walls " + (i + 1));
        }
    }

    private static void hideLayer(Camera c, int layer)
    {
        Debug.Log(layer);
        c.cullingMask |= layer;
    }

    private static void showLayer(Camera c, int layer)
    {
        c.cullingMask &= ~layer;
    }

    private static void hideLayer(Camera c, string layer)
    {
        int l = LayerMask.NameToLayer(layer);
        hideLayer(c, 1 << l);
    }

    static int playerID;
    static Color color;

    public void GenerateLevel()//curent level plane is 50x50 centered on origin
    {        
        Debug.Log("Generating Level.");

        playerID = ColorAlgorithm.getPlayerID();
        color = ColorAlgorithm.GetColor(playerID);

        /* Puts Walls around Every node */
        Object[,,] walls = new Object[11, 11, 2]; //Tot#Walls for nxm grid = n*(m+1)+ m*(n+1) = 2mn+n+m
        {
            float x, z;
            for (int i = 0; i < 10; i++)
            {
                x = i * 5 - 25;
                for (int j = 0; j < 10; j++)
                {
                    z = j * 5 - 25;
                    walls[i, j, 0] = makeWall(x, z, true);
                    walls[i, j, 1] = makeWall(x, z, false);

                    //((GameObject)walls[i, j, 0]).GetComponent<SpriteRenderer>().color = color;
                    //((GameObject)walls[i, j, 1]).GetComponent<SpriteRenderer>().color = color;
                }
                walls[i, 10, 0] = makeWall(x, 25, true);
                walls[i, 10, 1] = makeWall(25, x, false);

                //((GameObject)walls[i, 10, 0]).GetComponent<SpriteRenderer>().color = color;
                //((GameObject)walls[i, 10, 1]).GetComponent<SpriteRenderer>().color = color;
            };
        }
        

        System.Random rng = new System.Random();
        Vector2[] dirs = { //Adjacent Node Directions
            new Vector2(0, -1),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(-1, 0)
        };
        HashSet<Vector2> visited = new HashSet<Vector2>(); //Holds Visited Nodes
        Stack<Vector2> stack = new Stack<Vector2>(); //Holds Backtrack Worthy Nodes
        Vector2 current = new Vector2(0, 0); //Start node
        visited.Add(current);

        List<Vector2> options = new List<Vector2>();//Holds Viable Directions
        Vector2 adj; //Temp for Node in Direction
        Vector2 choice; //Selected Directions
        while (true)//Still options left
        {
            /* Loads in Viable Directions */
            options.Clear();
            for (int i=0;i<dirs.Length;i++)
            {
                adj = current + dirs[i];
                if(!visited.Contains(adj) && adj.x >= 0 && adj.x < 10 && adj.y >=0 && adj.y < 10)
                {
                    options.Add(dirs[i]);
                }
            }

            if (options.Count <= 0) //Dead End
            {
                if (stack.Count <= 0) { break; }//No Backtrack Options = Quit
                else { current = stack.Pop(); }//Backtrack = keep trying
            }
            else 
            {
                choice = options[0];
                if (options.Count > 1) //Multiple options, add to stack
                {
                    stack.Push(current);
                    choice = options[rng.Next(options.Count)];//pick random adj
                }
                if (choice.x + choice.y < 0)
                {
                    Destroy(walls[(int)current.x, (int)current.y, (int)Mathf.Abs(choice.x)]);
                    current += choice;
                }
                else
                {
                    current += choice;
                    Destroy(walls[(int)current.x, (int)current.y, (int)choice.x]);
                }
                visited.Add(current);
            }
            
        }
    }
    public Object makeWall(float x, float z, bool xy)
    {
        GameObject o;
        if (xy)
        {
            Vector3 sz = xy_wall.gameObject.GetComponent<Renderer>().bounds.size;
            o = ((Transform)Instantiate(xy_wall, new Vector3(x + sz.x / 2, sz.y / 2, z), Quaternion.identity)).gameObject;
        }
        else
        {
            Vector3 sz = yz_wall.gameObject.GetComponent<Renderer>().bounds.size;
            o = ((Transform)Instantiate(yz_wall, new Vector3(x, sz.y / 2, z+sz.z/2), Quaternion.identity)).gameObject;
        }

        Renderer r = o.GetComponent<Renderer>();
        r.material.color = color;
        //Collider c = o.GetComponent<Collider>();
        o.layer = LayerMask.NameToLayer("Walls " + (playerID + 1));
        return o;
    }
}
