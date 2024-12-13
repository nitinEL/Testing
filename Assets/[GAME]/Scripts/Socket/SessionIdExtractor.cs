using System;
using UnityEngine;

public class SessionIdExtractor : MonoBehaviour
{
    public static SessionIdExtractor instance;
    public string sessionId;
         
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (SocketIOManager.instance.socketConnectState == SocketState.None)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string url = Application.absoluteURL;

                if (!string.IsNullOrEmpty(url))
                {
                    // Parse the URL
                    Uri uri = new Uri(url);
                    string query = uri.Query;

                    // Extract the sessionId parameter
                    var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                    sessionId = queryParams.Get(SocketIOManager.sessionId);
                    // Output the sessionId
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        Debug.Log("Session ID: " + sessionId);
                    }
                    else
                    {
                        Debug.Log("Session ID not found in the URL.");
                    }
                }
                else
                {
                    Debug.Log("URL is empty.");
                }
                SocketIOManager.instance.ConnectToSocket();
            }
            else
            {
                if (GetAndAddSessionID.instance != null)
                {
                    sessionId = GetAndAddSessionID.instance.sessionID;
                }

                SocketIOManager.instance.ConnectToSocket();
            }
        }
    }
}
