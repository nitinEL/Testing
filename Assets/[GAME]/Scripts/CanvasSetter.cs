using UnityEngine;
using UnityEngine.UI;

    public class CanvasSetter : MonoBehaviour
    {
        private CanvasScaler canvasScaler;

        private void Awake()
        {
#if UNITY_WEBGL
            canvasScaler = GetComponent<CanvasScaler>();
            SetMatchRatio();
#endif
        }

        public void SetMatchRatio()
        {
            //Debug.Log("Screen.width::" + Screen.width);
            //Debug.Log("Screen.height::" + Screen.height);
            float screenWidth = Screen.width > Screen.height ? Screen.width : Screen.height;
            float screenHeight = Screen.height < Screen.width ? Screen.height : Screen.width;
           // Debug.Log("Final Screen.width::" + screenWidth);
            //Debug.Log("Final Screen.height::" + screenHeight);
            float scaleFactor = screenWidth / screenHeight;
            Debug.Log("scaleFactor::" + scaleFactor);

            if (scaleFactor > 2.2f && scaleFactor <= 3f)
            {
                canvasScaler.matchWidthOrHeight = 1f;
            }
            else if (scaleFactor >= 1.3f && scaleFactor < 2)
            {
                canvasScaler.matchWidthOrHeight = 0f;
            }
            else if (scaleFactor >= 2.2f)
            {
                canvasScaler.matchWidthOrHeight = 0f;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 1f;
            }
        }
    }