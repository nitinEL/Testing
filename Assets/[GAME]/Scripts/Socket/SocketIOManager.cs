using BestHTTP.SocketIO;
using Defective.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ServerMode
{
    Live,
    Local
}

public enum CurrentModePlay
{
    Live,
    Demo
}

public enum IsRemainingSpin
{
    None,
    RegularSpin,
    BonusSpin,
    ExpandedWildCard
}

public enum SocketState
{
    None,
    Connecting,
    Connect,
    Disconnect,
    FixDisconnect
}

[Serializable]
public class BonusSpinData
{
    public string totalFreeSpin;
    public string spin_id;
    public string totalWinAmount;
    public int lastIndexSpin;
    public float lastAmount;
    public float purchaseAmount;
    public float type = 0;
    public List<spin> spins = new List<spin>();
}

[Serializable]
public class RemainingWinCombinations
{
    public string _id;
    public string itemName;
    public string multiplier;
    public string winAmount;
    public string totalMatch;
    public string index;
}

[Serializable]
public class RegularSpinClass
{
    public float PurchaseAmount;
    public string status;
    public List<RemainingWinCombinations> regularSpinData = new List<RemainingWinCombinations>();
}

public class SocketIOManager : MonoBehaviour
{
    public static SocketIOManager instance;

    public ServerMode serverMode;
    public CurrentModePlay currentMode;
    public SocketState socketConnectState;
    public IsRemainingSpin isRemainingSpin;
    
    public ErrorPanel errorPanel;
    public GameObject loadingPanel;

    private float PING_INTERVAL = 5f;

    #region Private Variables Socket IO

    private string address;
    private SocketManager socket;
    [HideInInspector] public bool isCompleteRemainingSpinData = false;
    [HideInInspector] public bool isCompleteUserInfoSpinData = false;
    private bool firstTimeUserInfo = false;

    #endregion

    [Header("REMAINING SPINS")]
    public RegularSpinClass regularSpinClass;
    public BonusSpinData bonusRespinData;
    public static string expandedWildResponse;

    [Header("SOCKET EVENT NAMES")]
    public static string win = "Win";
    public static string loss = "Loss";
    public static string regular_spin_wheel = "regular_spin_wheel";
    public static string remaining_spins = "remaining_spins";
    public static string bonus_spin = "bonus_spin";
    public static string expanded_wild_card = "expanded_wild_card";
    public static string sessionId = "sessionId";
    public static string user_info = "user_info";
    public static string account_balance = "account_balance";
    public static string finished_round = "finished_round";

    public static Action balanceGet;

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

        if (serverMode == ServerMode.Live)
        {
            address = "https://gamezone.elaunchinfotech.in/socket.io/";
            //address = "https://gamezone.elaunchinfotech.in/";
        }
        else if (serverMode == ServerMode.Local)
        {
            address = "http://localhost:7002/socket.io/";
            //address = "http://192.168.1.93:7002/socket.io/";
        }

        Application.runInBackground = true;

        Debug.Log($"Server Add { address }");
    }

    private void Start()
    {
        CheckPossibilities(3);
    }

    public void CheckPossibilities(int _count)
    {
        // Original list
        int[] list = { 0, 1, 2 };

        // Store possibilities
        List<int[]> possibilities = new List<int[]>();

        // Perform swap of one index with all others
        for (int i = 0; i < list.Length; i++)
        {
            for (int j = 0; j < list.Length; j++)
            {
                if (i != j) // Ensure we are not swapping the index with itself
                {
                    int[] temp = (int[])list.Clone(); // Create a copy of the list
                    // Swap the elements
                    int swap = temp[i];
                    temp[i] = temp[j];
                    temp[j] = swap;
                    possibilities.Add(temp);
                }
            }
        }

        // Remove duplicates
        List<string> uniquePossibilities = new List<string>();
        foreach (var possibility in possibilities)
        {
            string str = string.Join(",", possibility);
            if (!uniquePossibilities.Contains(str))
            {
                uniquePossibilities.Add(str);
            }
        }

        // Print results
        Debug.Log("All unique possibilities:");
        foreach (var possibility in uniquePossibilities)
        {
            Debug.Log("[" + possibility + "]");
        }
    }

    public void ConnectToSocket()
    {
        socket = new SocketManager(new Uri(address));
        socket.Options.ServerVersion = SupportedSocketIOVersions.v3;
        socket.Options.AutoConnect = true;
        socket.Options.ConnectWith = BestHTTP.SocketIO.Transports.TransportTypes.WebSocket;
        socket.Options.AdditionalQueryParams = new PlatformSupport.Collections.ObjectModel.ObservableDictionary<string, string>();
        socket.Options.AdditionalQueryParams.Add("game_code", "fearFortune");
        socketConnectState = SocketState.Connecting;

        Debug.Log("Socket  Connecting....");

        socket.Socket.On(SocketIOEventTypes.Connect, (s, p, a) =>
        {
            Debug.Log("Connected!");
            //loadingPanel.SetActive(false);
            socketConnectState = SocketState.Connect;
            StartCoroutine(CheckPingTimeOut());
            GetPlayerBalance();
            CheckFreeSpinRemaining();
        });

        socket.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) =>
        {
            Debug.LogError($"Socket Error: {packet}, Args: {string.Join(", ", args)}");
        });

        socket.Socket.On(SocketIOEventTypes.Disconnect, (socket, packet, args) =>
        {
            loadingPanel.SetActive(true);

            if (socketConnectState != SocketState.FixDisconnect)
            {
                socketConnectState = SocketState.Disconnect;
                SceneManager.LoadScene("Splash");
                Destroy(gameObject);
            }
            Debug.Log("Disconnected!");
        });

        socket.Socket.On("open", (s, p, a) =>
        {
            Debug.Log("Open event received!");

        });

        socket.Socket.On(regular_spin_wheel, (s, p, a) =>
        {
            OnSocketIOResponse(p.Payload);
        });

        socket.Socket.On(bonus_spin, (s, p, a) =>
        {
            OnSocketIOResponse(p.Payload);
        });

        socket.Socket.On(expanded_wild_card, (s, p, a) =>
        {
            OnSocketIOResponse(p.Payload);
        });

        socket.Socket.On(remaining_spins, (s, p, a) =>
        {
            OnSocketIOResponse(p.Payload);
            if (!isCompleteRemainingSpinData)
            {
                isCompleteRemainingSpinData = true;
            }
        });

        socket.Socket.On(user_info, (s, p, a) =>
        {
            Debug.Log(user_info);
            OnSocketIOResponse(p.Payload);
        });

        socket.Socket.On("pong", (s, p, a) =>
        {
            //Debug.Log("PONGGG" + p.Payload);
            OnSocketIOResponse(p.Payload);
        });

        socket.Socket.On(finished_round, (s, p, a) =>
        {
            Debug.Log($"<color=green>finished_round {p.Payload} </color>");
        });
        socket.Open();
    }

    public void OnSocketIOResponse(string _response)
    {
        Debug.Log($"_response {_response}");
        JSONObject _data = new JSONObject(_response);

        string actionName = _data[0].ToString().Trim('"');

        if (_data[1].GetField("status") != null)//Error message
        {
            errorPanel.gameObject.SetActive(true);
            errorPanel.setText(_data[1].GetField("message").ToString().Trim('"'), "Oops! Something went wrong. Please refresh the page or try again in a moment.");
            socketConnectState = SocketState.FixDisconnect;
            socket.Socket.Disconnect();

            Debug.Log($"_response {_response}");
            return;
        }

        if (actionName == bonus_spin)
        {
            Debug.Log($"<color=green> Bonus Spin _response { _response } </color>");
            GameManager.instance.GetDataToServer(_response);
        }
        else if (actionName == expanded_wild_card) 
        {
            Debug.Log($"<color=green> expanded_wild_card {_response} </color>");
            GameManager.instance.GetDataToServer(_response);
        }
        else if (actionName == regular_spin_wheel)
        {
            RegularSpinConvertJson(_response);
        }
        else if (actionName == remaining_spins)
        {
            var (data, success, _isRemainingSpin) = GetFreeSpinRemainingSpin(_response);
            expandedWildResponse = _isRemainingSpin == IsRemainingSpin.ExpandedWildCard ? _response : string.Empty;
            bonusRespinData = data;
            isRemainingSpin = _isRemainingSpin;
        }
        else if (actionName == user_info)
        {
            Debug.Log($"<color=green> user_info _response {_response}, firstTimeUserInfo {firstTimeUserInfo} </color>");
            JSONObject _userInfo = new JSONObject(_response);
            if (firstTimeUserInfo == true)
            {
                Debug.Log($"===> Reset player balance");
                GameManager.PlayerChips = double.Parse(_userInfo[1].GetField(account_balance).ToString().Trim('"'));
                balanceGet?.Invoke();

                //Debug.Log("===> Compare to User balance");
                //if (GameManager.PlayerChips == double.Parse(_userInfo[1].GetField(account_balance).ToString().Trim('"')))
                //{
                //    Debug.Log("Both Balance is Right Get");
                //}
                //else 
                //{
                //    Debug.LogError($"Both balances are not same {GameManager.PlayerChips}, Main server balance {double.Parse(_userInfo[1].GetField(account_balance).ToString().Trim('"'))}");
                //}
            }
            else
            {
                Debug.Log("firstTimeUserInfo is FALSE");

                firstTimeUserInfo = true;
                string _symbol = _userInfo[1].GetField("symbol").ToString().Trim('"');
                GameManager.currencySymbol = string.IsNullOrWhiteSpace(_symbol) ? _userInfo[1].GetField("currency").ToString().Trim('"') + " " : _symbol + " ";
                currentMode = _symbol == "FUN" ? CurrentModePlay.Demo : CurrentModePlay.Live;
                GameManager.conversionRate = float.Parse(_userInfo[1].GetField("conversionRate").ToString().Trim('"'));
                GameManager.roundOff = int.Parse(_userInfo[1].GetField("roundOff").ToString().Trim('"'));
                GameManager.PlayerChips = double.Parse(_userInfo[1].GetField(account_balance).ToString().Trim('"'));
                if (!isCompleteUserInfoSpinData)
                {
                    isCompleteUserInfoSpinData = true;
                }
            }
        }
        else if (actionName == "pong")
        {
            //Debug.Log("==> " + actionName);
        }
    }

    public void GetPlayerBalance()
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField(sessionId, SessionIdExtractor.instance.sessionId);
        socket.Socket.Emit(user_info, jsonData.Print());
        Debug.Log($"<color=cyan> ==> Get userInfo {jsonData} </color>");
    }

    public void CheckFreeSpinRemaining()
    {
        JSONObject jsonData = new JSONObject();
        jsonData.AddField(sessionId, SessionIdExtractor.instance.sessionId);
        socket.Socket.Emit(remaining_spins, jsonData.Print());
        Debug.Log($"<color=cyan> ==> Check Free Spin Remaining {jsonData} </color>");
    }

    public void SendData(string actionName, double betAmount = 0)   
    {
        string _betAmount = betAmount.ToString("f2");                   
        JSONObject jsonData = new JSONObject();

        if (actionName == bonus_spin)
        {
            jsonData.AddField("purchaseAmount", float.Parse(_betAmount));
        }
        else if (actionName == expanded_wild_card) 
        { 
            jsonData.AddField("purchaseAmount", float.Parse(_betAmount));
        }
        else if (actionName == regular_spin_wheel)
        {
            jsonData.AddField("bet", float.Parse(_betAmount));
        }

        jsonData.AddField(sessionId, SessionIdExtractor.instance.sessionId);
        socket.Socket.Emit(actionName, jsonData.Print());
        Debug.Log($"<color=cyan>==> {actionName} Data Send {jsonData} </color> ");   
    }

    public void RegularSpinConvertJson(string _response)
    {
        JSONObject _data = new JSONObject(_response);

        JSONObject winCombinations = _data[1].GetField("winCombinations");
        JSONObject _isWin = _data[1].GetField("isWin");

        if (_isWin.ToString() == "true")
        {
            //Debug.Log("Game Win");

            Debug.Log($"<color=green> ==> Regular Data : { _data } </color>");
            regularSpinClass.regularSpinData.Clear();

            for (int i = 0; i < winCombinations.count; i++)
            {
                for (int k = 0; k < winCombinations[i].count; k++)
                {
                    RemainingWinCombinations _matchData = new RemainingWinCombinations();

                    WinCombination winCombination = new WinCombination();
                    //_matchData._id = winCombinations[i][k].GetField("_id").ToString().Trim('"');
                    _matchData.itemName = winCombinations[i][k].GetField("itemName").ToString().Trim('"');
                    //_matchData.multiplier = winCombinations[i][k].GetField("multiplier").ToString().Trim('"');
                    _matchData.winAmount = winCombinations[i][k].GetField("winAmount").ToString().Trim('"');
                    _matchData.totalMatch = winCombinations[i][k].GetField("totalMatch").ToString().Trim('"');

                    regularSpinClass.regularSpinData.Add(_matchData);
                }
            }

            //regularSpinClass.regularSpinData.Sort((a, b) => int.Parse(a.index).CompareTo(int.Parse(b.index)));
            GameManager.instance.GetDataToServer(win, _response);
            regularSpinClass.status = win;
        }
        else
        {
            regularSpinClass.regularSpinData.Clear();
            regularSpinClass.status = loss;
            GameManager.instance.GetDataToServer(loss);
            //Debug.Log("Game Loss");
        }
    }

    public void SendFinishedBonusSpinIndex(int _index)
    {
        if (currentMode == CurrentModePlay.Live)
        {
            JSONObject jsonData = new JSONObject();
            jsonData.AddField("index", _index);
            Debug.Log($"<color=green> Finish Round ===============> { jsonData } </color>");
            socket.Socket.Emit("finished_round", jsonData.Print());
        }
    }

    public void SendFinishedexpandedwildcardend()
    {
        if (currentMode == CurrentModePlay.Live)
        {
            socket.Socket.Emit("expanded_wild_card_end");
            Debug.Log($"<color=green> expanded_wild_card_end </color>");
        }
    }

    public (BonusSpinData, bool, IsRemainingSpin) GetFreeSpinRemainingSpin(string _response)
    {
        bool someBool = false;

        Debug.Log($"_response { _response }");

        BonusSpinData retuneValue = new BonusSpinData();
        JSONObject _jdata = new JSONObject(_response);

        if (_jdata[1][1].GetField("results") != null)
        {
            
        }
        else
        {
            isRemainingSpin = IsRemainingSpin.None;
            Debug.Log("No More Spin Remeaning");
            return (retuneValue, someBool, IsRemainingSpin.None);
        }

        //retuneValue.spin_id = _jdata[1][0].GetField("id").ToString().Trim('"');
        retuneValue.totalFreeSpin = _jdata[1][0].GetField("spin").ToString();
        retuneValue.totalWinAmount = _jdata[1][0].GetField("total").ToString();
        retuneValue.type = float.Parse(_jdata[1][0].GetField("type").ToString());

        retuneValue.lastAmount = float.Parse(_jdata[1][0].GetField("lastAmount").ToString());
        retuneValue.purchaseAmount = float.Parse(_jdata[1][0].GetField("purchaseAmount").ToString());

        JSONObject _jsonStringTumble = new JSONObject(_response);

        if (retuneValue.type == 4)
        {
            for (int j = 0; j < _jsonStringTumble[1].count; j++)
            {
                JSONObject _spinData = _jsonStringTumble[1][j].GetField("results");

                if (_spinData == null)
                {
                    continue;
                }
            }

            return (retuneValue, someBool, IsRemainingSpin.ExpandedWildCard);
        }

        if (_jdata[1][0].GetField("lastIndex").ToString() == "null")
        {
            retuneValue.lastIndexSpin = -1;
            //retuneValue.lastIndexSpin = 0;
        }
        else
        {
            Debug.Log($"<color=red> lastIndex Found </color>");
            retuneValue.lastIndexSpin = int.Parse(_jdata[1][0].GetField("lastIndex").ToString());
        }

        #region OLD Code

        //for (int j = 0; j < _jdata[1][1].GetField("results").count; j++)
        //{
        //    FreeSpinList _freeSpinList = new FreeSpinList();
        //    _freeSpinList.spinNumber = j + 1;

        //    if (_jdata[1][1].GetField("results")[j].ToString().Trim('"') == "lose")
        //    {
        //        _freeSpinList.status = loss;
        //    }
        //    else
        //    {
        //        _freeSpinList.status = win;

        //        MatchData _matchData = new MatchData();

        //        for (int k = 0; k < _jdata[1][1].GetField("results")[j].GetField("winCombinations").count; k++)
        //        {
        //            RemainingWinCombinations winCombinations = new RemainingWinCombinations();
        //            winCombinations._id = _jdata[1][1].GetField("results")[j].GetField("winCombinations")[k].GetField("_id").ToString().Trim('"');
        //            winCombinations.itemName = _jdata[1][1].GetField("results")[j].GetField("winCombinations")[k].GetField("itemName").ToString().Trim('"');
        //            winCombinations.multiplier = _jdata[1][1].GetField("results")[j].GetField("winCombinations")[k].GetField("multiplier").ToString().Trim('"');
        //            winCombinations.winAmount = _jdata[1][1].GetField("results")[j].GetField("winCombinations")[k].GetField("winAmount").ToString().Trim('"');
        //            winCombinations.totalMatch = _jdata[1][1].GetField("results")[j].GetField("winCombinations")[k].GetField("totalMatch").ToString();
        //            winCombinations.index = _jdata[1][1].GetField("results")[j].GetField("winCombinations")[k].GetField("index").ToString();

        //            _matchData.winCombinations.Add(winCombinations);
        //        }

        //        if (_jdata[1][1].GetField("results")[j].GetField("grid") != null)
        //        {
        //            for (int gridCount = 0; gridCount < _jdata[1][1].GetField("results")[j].GetField("grid").count; gridCount++)
        //            {
        //                List<string> row = new List<string>();
        //                for (int gridColum = 0; gridColum < _jdata[1][1].GetField("results")[j].GetField("grid")[gridCount].count; gridColum++)
        //                {
        //                    row.Add(_jdata[1][1].GetField("results")[j].GetField("grid")[gridCount][gridColum].ToString().Trim('"'));

        //                }
        //                _matchData.grids.Add(row);
        //            }
        //        }

        //        for (int gridCount = 0; gridCount < _jdata[1][1].GetField("results")[j].GetField("multiplier").count; gridCount++)
        //        {
        //            _freeSpinList.multiplierList.Add(_jdata[1][1].GetField("results")[j].GetField("multiplier")[gridCount].ToString());
        //        }

        //        /////---------->
        //        for (int gridCount = 0; gridCount < _jdata[1][1].GetField("results")[j].GetField("multiplier_grid").count; gridCount++)
        //        {
        //            List<int> row = new List<int>();
        //            for (int gridColum = 0; gridColum < _jdata[1][1].GetField("results")[j].GetField("multiplier_grid")[gridCount].count; gridColum++)
        //            {
        //                row.Add(int.Parse(_jdata[1][1].GetField("results")[j].GetField("multiplier_grid")[gridCount][gridColum].ToString().Trim('"')));

        //            }
        //            _matchData.multiplier_grid.Add(row);
        //        }

        //        _freeSpinList.matchData = _matchData;
        //    }
        //    retuneValue.freeSpinLists.Add(_freeSpinList);
        //}

        #endregion

        retuneValue.spins = new List<spin>();

        spin _spin = new spin();

        for (int j = 0; j < _jsonStringTumble[1].count; j++)
        {
            JSONObject _spinData = _jsonStringTumble[1][j].GetField("results");

            if (_spinData == null)
            {
                //_spin.betAmount = double.Parse(_jsonStringTumble[1][j].GetField("betAmount").ToString());
                _spin.payout = double.Parse(_jsonStringTumble[1][j].GetField("total").ToString());
                continue;
            }

            _spin.grid = new List<positions>();

            for (int i = 0; i < _spinData.count; i++)
            {
                JSONObject _spinGridData = _spinData[i].GetField("grid");

                positions positions = new positions();
                positions.remainingSpin = int.Parse(_spinData[i].GetField("remainingSpin").ToString());

                for (int g = 0; g < _spinGridData.count; g++)
                {
                    JSONObject _spinGridDataPositions = _spinGridData[g].GetField("position");
                    RowColIDs rowColIDs = new RowColIDs();

                    rowColIDs.amount = double.Parse(_spinGridData[g].GetField("amount").ToString());
                    rowColIDs.columnNumber = int.Parse(_spinGridDataPositions[0].ToString());
                    rowColIDs.rowNumber = int.Parse(_spinGridDataPositions[1].ToString());

                    positions._positions.Add(rowColIDs);
                }
                _spin.grid.Add(positions);
            }
            retuneValue.spins.Add(_spin);
        }
        return (retuneValue, someBool, IsRemainingSpin.BonusSpin);
    }

    IEnumerator CheckPingTimeOut()
    {
        SB:
        if (socketConnectState == SocketState.Connect)
        {
            if (currentMode == CurrentModePlay.Live)
            {
                JSONObject jsonData = new JSONObject();
                jsonData.AddField(sessionId, SessionIdExtractor.instance.sessionId);
                //Debug.Log("Ping Pong Send ====>");
                socket.Socket.Emit("ping", jsonData.Print());
            }

            yield return new WaitForSeconds(PING_INTERVAL);//1
            goto SB;
        }
        else
        {
            yield return new WaitForSeconds(1);
            goto SB;
        }
    }

    public InputField inputField;

    public void ApplyBtnClick() 
    {
        if (inputField.text != null) 
        { 
            Time.timeScale = float.Parse(inputField.text);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (inputField.text != null)
            {
                Time.timeScale = float.Parse(inputField.text);
            }
        }
    }
}

