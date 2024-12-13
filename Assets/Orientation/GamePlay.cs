using System.Runtime.InteropServices;


public static class GamePlay
{

    [DllImport("__Internal")]
    public static extern void SetGamePlayListener(
            string Name,
            string OnGamePause,
            string OnGameResume);

    [DllImport("__Internal")]
    public static extern int IsMobileDevice();

    [DllImport("__Internal")]
    public static extern bool IsInternetConnectionAvailable();

}
