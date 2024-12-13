using UnityEngine;
using UnityEngine.SceneManagement;


public class ViewController : MonoBehaviour
{
    public static ViewController Instance;
    
    public GameObject RotateObject;
    
    private ScreenOrientation currentOrientation;
    
    public Animator anim;
    
    public bool IsLandscape;

#if !UNITY_EDITOR
        public bool IsMobileDevice { get => GamePlay.IsMobileDevice() == 1; }
       
#endif
    public bool OnetimeIsMobileDevice;
    private void Awake()
    {
#if !UNITY_EDITOR
            OnetimeIsMobileDevice = IsMobileDevice;
#endif
        Debug.Log("===> OnetimeIsMobileDevice" + OnetimeIsMobileDevice);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (OnetimeIsMobileDevice)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (screenHeight > screenWidth)
            {
                ShowHideRotateAnim(true);
            }
            else
            {
                ShowHideRotateAnim(false);
            }
        }
    }

    void ShowHideRotateAnim(bool isShow)
    {
        if (isShow && !RotateObject.activeSelf && OnetimeIsMobileDevice)
        {
            RotateObject.SetActive(true);
            anim.Play(IsLandscape ? "RoateDevice" : "RoateDevice2");
        }
        else if (!isShow && RotateObject.activeSelf)
        {
            Debug.Log("Pop Close Landspace");
            RotateObject.SetActive(false);
        }
    }
}
