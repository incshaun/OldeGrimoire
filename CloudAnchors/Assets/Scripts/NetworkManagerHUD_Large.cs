﻿using System;
using System.ComponentModel;

#if ENABLE_UNET

namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/NetworkManagerHUD_Large")]
    [RequireComponent(typeof(NetworkManager))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NetworkManagerHUD_Large : MonoBehaviour
    {
        public NetworkManager manager;
        [SerializeField] public bool showGUI = true;
        [SerializeField] public int offsetX = 0;
        [SerializeField] public int offsetY = 0;
        [SerializeField] public float scale = 1;
        [SerializeField] public bool hideAfterStart = false;

        // Runtime variable
        bool m_ShowServer;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        void Update()
        {
            if (!showGUI)
                return;

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        manager.StartServer();
                    }
                    if (Input.GetKeyDown(KeyCode.H))
                    {
                        manager.StartHost();
                    }
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    manager.StartClient();
                }
            }
            if (NetworkServer.active && manager.IsClientConnected())
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    manager.StopHost();
                }
            }
        }

        void OnGUI()
        {
            if (!showGUI)
                return;

            int xpos = 10 + offsetX;
            int ypos = 40 + offsetY;
            int spacing = (int) (24 * scale);

            bool noConnection = (manager.client == null || manager.client.connection == null ||
                                 manager.client.connection.connectionId == -1);

            if (!manager.IsClientConnected() && !NetworkServer.active && manager.matchMaker == null)
            {
                if (noConnection)
                {
                    if (UnityEngine.Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "LAN Host(H)"))
                        {
                            manager.StartHost();
                        }
                        ypos += spacing;
                    }

                    if (GUI.Button(new Rect(xpos, ypos, (int) (105 * scale), (int) (20 * scale)), "LAN Client(C)"))
                    {
                        manager.StartClient();
                    }

//                    manager.networkAddress = GUI.TextField(new Rect(xpos + (int) (100 * scale), ypos, (int) (95 * scale), (int) (20 * scale)), manager.networkAddress);
                    manager.networkAddress = GUI.TextField(new Rect(xpos + (int) (100 * scale), ypos, (int) (95 * scale), (int) (20 * scale)), "192.168.1.109");
                    ypos += spacing;

                    if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // cant be a server in webgl build
                        GUI.Box(new Rect(xpos, ypos, (int) (200 * scale), (int) (25 * scale)), "(  WebGL cannot be server  )");
                        ypos += spacing;
                    }
                    else
                    {
                        if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "LAN Server Only(S)"))
                        {
                            manager.StartServer();
                        }
                        ypos += spacing;
                    }
                }
                else
                {
                    GUI.Label(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Connecting to " + manager.networkAddress + ":" + manager.networkPort + "..");
                    ypos += spacing;


                    if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Cancel Connection Attempt"))
                    {
                        manager.StopClient();
                    }
                }
            }
            else 
            {
            if (!hideAfterStart)
            {
                if (NetworkServer.active)
                {
                    string serverMsg = "Server: port=" + manager.networkPort;
                    if (manager.useWebSockets)
                    {
                        serverMsg += " (Using WebSockets)";
                    }
                    GUI.Label(new Rect(xpos, ypos, (int) (300 * scale), (int) (20 * scale)), serverMsg);
                    ypos += spacing;
                }
                if (manager.IsClientConnected())
                {
                    GUI.Label(new Rect(xpos, ypos, (int) (300 * scale), (int) (20 * scale)), "Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                    ypos += spacing;
                }
            }

            if (manager.IsClientConnected() && !ClientScene.ready && !hideAfterStart)
            {
                if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Client Ready"))
                {
                    ClientScene.Ready(manager.client.connection);

                    if (ClientScene.localPlayers.Count == 0)
                    {
                        ClientScene.AddPlayer(0);
                    }
                }
                ypos += spacing;
            }

            if ((NetworkServer.active || manager.IsClientConnected()) && !hideAfterStart)
            {
                if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Stop (X)"))
                {
                    manager.StopHost();
                }
                ypos += spacing;
            }

            if (!NetworkServer.active && !manager.IsClientConnected() && noConnection && !hideAfterStart)
            {
                ypos += 10;

                if (UnityEngine.Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    GUI.Box(new Rect(xpos - 5, ypos, (int) (220 * scale), (int) (25 * scale)), "(WebGL cannot use Match Maker)");
                    return;
                }

                if (manager.matchMaker == null)
                {
                    if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Enable Match Maker (M)"))
                    {
                        manager.StartMatchMaker();
                    }
                    ypos += spacing;
                }
                else
                {
                    if (manager.matchInfo == null)
                    {
                        if (manager.matches == null)
                        {
                            if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Create Internet Match"))
                            {
                                manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
                            }
                            ypos += spacing;

                            GUI.Label(new Rect(xpos, ypos, (int) (100 * scale), (int) (20 * scale)), "Room Name:");
                            manager.matchName = GUI.TextField(new Rect(xpos + (int) (100 * scale), ypos, (int) (100 * scale), (int) (20 * scale)), manager.matchName);
                            ypos += spacing;

                            ypos += 10;

                            if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Find Internet Match"))
                            {
                                manager.matchMaker.ListMatches(0, 20, "", false, 0, 0, manager.OnMatchList);
                            }
                            ypos += spacing;
                        }
                        else
                        {
                            for (int i = 0; i < manager.matches.Count; i++)
                            {
                                var match = manager.matches[i];
                                if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Join Match:" + match.name))
                                {
                                    manager.matchName = match.name;
                                    manager.matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, manager.OnMatchJoined);
                                }
                                ypos += spacing;
                            }

                            if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Back to Match Menu"))
                            {
                                manager.matches = null;
                            }
                            ypos += spacing;
                        }
                    }

                    if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Change MM server"))
                    {
                        m_ShowServer = !m_ShowServer;
                    }
                    if (m_ShowServer)
                    {
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, (int) (100 * scale), (int) (20 * scale)), "Local"))
                        {
                            manager.SetMatchHost("localhost", 1337, false);
                            m_ShowServer = false;
                        }
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, (int) (100 * scale), (int) (20 * scale)), "Internet"))
                        {
                            manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
                            m_ShowServer = false;
                        }
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, (int) (100 * scale), (int) (20 * scale)), "Staging"))
                        {
                            manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
                            m_ShowServer = false;
                        }
                    }

                    ypos += spacing;

                    GUI.Label(new Rect(xpos, ypos, (int) (300 * scale), (int) (20 * scale)), "MM Uri: " + manager.matchMaker.baseUri);
                    ypos += spacing;

                    if (GUI.Button(new Rect(xpos, ypos, (int) (200 * scale), (int) (20 * scale)), "Disable Match Maker"))
                    {
                        manager.StopMatchMaker();
                    }
                    ypos += spacing;
                }
            }
            }
        }
    }
}
#endif //ENABLE_UNET
