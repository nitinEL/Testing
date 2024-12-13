using Defective.JSON;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RowColIDs
{
    public double amount = 0;
    public int columnNumber = 0;
    public int rowNumber = 0;
}

[Serializable]
public class positions
{
    public int remainingSpin = 0;
    public List<RowColIDs> _positions = new List<RowColIDs>();
}

[Serializable]
public class spin
{
    public double betAmount = 0;
    public double payout = 0;
    public List<positions> grid = new List<positions>();
}

public class GetBonusSpinData : MonoBehaviour
{
    public static GetBonusSpinData instance;

    //internal string jsonString = "[{\r\n  \"betAmount\": 30,\r\n  \"payout\": 3.65,\r\n  \"spinCount\": 3,\r\n  \"spin\": [\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            2\r\n          ],\r\n          \"amount\": 3.65\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 0,\r\n      \"grid\": []\r\n    }\r\n  ]\r\n},\r\n{\r\n  \"betAmount\": 30,\r\n  \"payout\": 22.72,\r\n  \"spinCount\": 19,\r\n  \"spin\": [\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            3\r\n          ],\r\n          \"amount\": 0.12\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            3\r\n          ],\r\n          \"amount\": 0.12\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            2\r\n          ],\r\n          \"amount\": 0.3\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            2\r\n          ],\r\n          \"amount\": 1.87\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            0\r\n          ],\r\n          \"amount\": 4.41\r\n        },\r\n        {\r\n          \"position\": [\r\n            3,\r\n            2\r\n          ],\r\n          \"amount\": 1.13\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            1\r\n          ],\r\n          \"amount\": 2.74\r\n        },\r\n        {\r\n          \"position\": [\r\n            0,\r\n            2\r\n          ],\r\n          \"amount\": 2.4\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            0\r\n          ],\r\n          \"amount\": 0.49\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            0\r\n          ],\r\n          \"amount\": 2.65\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            2\r\n          ],\r\n          \"amount\": 0.7\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            1\r\n          ],\r\n          \"amount\": 0.22\r\n        },\r\n        {\r\n          \"position\": [\r\n            4,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            1\r\n          ],\r\n          \"amount\": 1.95\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            1\r\n          ],\r\n          \"amount\": 1.19\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            0\r\n          ],\r\n          \"amount\": 2.33\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 0,\r\n      \"grid\": []\r\n    }\r\n  ]\r\n},\r\n{\r\n  \"betAmount\": 30,\r\n  \"payout\": 28.66,\r\n  \"spinCount\": 14,\r\n  \"spin\": [\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            3\r\n          ],\r\n          \"amount\": 0.13\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            0\r\n          ],\r\n          \"amount\": 1.68\r\n        },\r\n        {\r\n          \"position\": [\r\n            3,\r\n            1\r\n          ],\r\n          \"amount\": 0.69\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            0\r\n          ],\r\n          \"amount\": 0.66\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            0\r\n          ],\r\n          \"amount\": 3.81\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            1\r\n          ],\r\n          \"amount\": 2.83\r\n        },\r\n        {\r\n          \"position\": [\r\n            3,\r\n            2\r\n          ],\r\n          \"amount\": 4.99\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            2\r\n          ],\r\n          \"amount\": 0.12\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            3\r\n          ],\r\n          \"amount\": 0.3\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            2\r\n          ],\r\n          \"amount\": 0.64\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            0\r\n          ],\r\n          \"amount\": 2.03\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            1\r\n          ],\r\n          \"amount\": 3.13\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            2\r\n          ],\r\n          \"amount\": 3.51\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            1\r\n          ],\r\n          \"amount\": 3.9\r\n        },\r\n        {\r\n          \"position\": [\r\n            3,\r\n            3\r\n          ],\r\n          \"amount\": 0.14\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 0,\r\n      \"grid\": []\r\n    }\r\n  ]\r\n},\r\n{\r\n  \"betAmount\": 30,\r\n  \"payout\": 30.15,\r\n  \"spinCount\": 21,\r\n  \"spin\": [\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            0\r\n          ],\r\n          \"amount\": 0.38\r\n        },\r\n        {\r\n          \"position\": [\r\n            4,\r\n            0\r\n          ],\r\n          \"amount\": 3.35\r\n        },\r\n        {\r\n          \"position\": [\r\n            1,\r\n            3\r\n          ],\r\n          \"amount\": 0.15\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            1\r\n          ],\r\n          \"amount\": 4.86\r\n        },\r\n        {\r\n          \"position\": [\r\n            4,\r\n            1\r\n          ],\r\n          \"amount\": 5.62\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            3\r\n          ],\r\n          \"amount\": 0.11\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            0\r\n          ],\r\n          \"amount\": 0.73\r\n        },\r\n        {\r\n          \"position\": [\r\n            3,\r\n            2\r\n          ],\r\n          \"amount\": 0.33\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            1\r\n          ],\r\n          \"amount\": 1.31\r\n        },\r\n        {\r\n          \"position\": [\r\n            1,\r\n            2\r\n          ],\r\n          \"amount\": 4.67\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            1\r\n          ],\r\n          \"amount\": 2.99\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            0\r\n          ],\r\n          \"amount\": 1.72\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            1\r\n          ],\r\n          \"amount\": 2.85\r\n        },\r\n        {\r\n          \"position\": [\r\n            4,\r\n            2\r\n          ],\r\n          \"amount\": 0.2\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            3\r\n          ],\r\n          \"amount\": 0.11\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            2,\r\n            2\r\n          ],\r\n          \"amount\": 0.49\r\n        },\r\n        {\r\n          \"position\": [\r\n            0,\r\n            3\r\n          ],\r\n          \"amount\": 0.18\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 0,\r\n      \"grid\": []\r\n    }\r\n  ]\r\n},\r\n{\r\n  \"betAmount\": 30,\r\n  \"payout\": 35.79,\r\n  \"spinCount\": 12,\r\n  \"spin\": [\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            0\r\n          ],\r\n          \"amount\": 2.52\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            0\r\n          ],\r\n          \"amount\": 3.61\r\n        },\r\n        {\r\n          \"position\": [\r\n            0,\r\n            1\r\n          ],\r\n          \"amount\": 0.83\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            4,\r\n            1\r\n          ],\r\n          \"amount\": 6.32\r\n        },\r\n        {\r\n          \"position\": [\r\n            2,\r\n            2\r\n          ],\r\n          \"amount\": 1.4\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            0\r\n          ],\r\n          \"amount\": 6.25\r\n        },\r\n        {\r\n          \"position\": [\r\n            4,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            0,\r\n            0\r\n          ],\r\n          \"amount\": 6.01\r\n        },\r\n        {\r\n          \"position\": [\r\n            1,\r\n            3\r\n          ],\r\n          \"amount\": 0.1\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            1,\r\n            1\r\n          ],\r\n          \"amount\": 3.34\r\n        },\r\n        {\r\n          \"position\": [\r\n            4,\r\n            2\r\n          ],\r\n          \"amount\": 0.24\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            2\r\n          ],\r\n          \"amount\": 0.15\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 3,\r\n      \"grid\": [\r\n        {\r\n          \"position\": [\r\n            3,\r\n            1\r\n          ],\r\n          \"amount\": 4.42\r\n        },\r\n        {\r\n          \"position\": [\r\n            0,\r\n            2\r\n          ],\r\n          \"amount\": 0.3\r\n        }\r\n      ]\r\n    },\r\n    {\r\n      \"remainingSpin\": 2,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 1,\r\n      \"grid\": []\r\n    },\r\n    {\r\n      \"remainingSpin\": 0,\r\n      \"grid\": []\r\n    }\r\n  ]\r\n}]";
    internal string jsonString = "";

    public List<spin> spins = new List<spin>();

    [SerializeField] int _gridIndex = 0;
    public static int gridIndex { get => instance._gridIndex; set => instance._gridIndex = value; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        //GetBonusSpinData_1();
    }

    internal void GetBonusSpinData_1(string _response)
    {
        jsonString = _response;

        spins = new List<spin>();
        gridIndex = 0;
        JSONObject _jsonStringTumble = new JSONObject(jsonString);

        spin _spin = new spin();

        for (int j = 0; j < _jsonStringTumble[1].count; j++)
        {
            JSONObject _spinData = _jsonStringTumble[1][j].GetField("results");

            if (_spinData == null)
            {
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

            spins.Add(_spin);
        }
    }
}
