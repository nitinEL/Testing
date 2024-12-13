using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GetAndAddSessionID : MonoBehaviour
{
    public static GetAndAddSessionID instance;

    public InputField inputField;
    public Button playButton;

    public string sessionID = string.Empty;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { 
            DestroyImmediate(gameObject);
        }
    }

    public void onPlayBtnClick() 
    {
        sessionID = inputField.text;


        if (sessionID == null || sessionID == string.Empty)
        {
            Debug.Log("Please Enter iD");
        }
        else 
        {
            Debug.Log(" sessionID " + sessionID);
            playButton.interactable = false;
            SceneManager.LoadScene(1);
        }
    }

}
