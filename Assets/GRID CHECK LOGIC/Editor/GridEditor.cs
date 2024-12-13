
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(20);
        GridManager gridManager = (GridManager)target;

        EditorGUILayout.LabelField("TotalItem = 1, 2, 3, 4, 5, 6", EditorStyles.boldLabel);
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Grids Display", EditorStyles.boldLabel);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Third Grid", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (gridManager.ThirdGrid != null)
        {
            for (int i = -1; i < gridManager.ThirdGrid.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));

                if (i == -1)
                    for (int j = 0; j < 5; j++)
                    {
                        EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(50));
                    }
                else
                    //for (int j = 0; j < gridManager.secondItemGrids[i].Count; j++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (i > gridManager.ThirdGrid[i].Count - 1)
                        {
                            continue;
                        }

                        if (gridManager.ThirdGrid[i][j] != null)
                        {
                            string textComponent = gridManager.ThirdGrid[i][j];
                            if (textComponent != null)
                            {
                                EditorGUILayout.LabelField(textComponent, GUILayout.Width(50));
                            }
                        }
                    }

                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Second Grid", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (gridManager.SecondGrid != null)
        {
            for (int i = -1; i < gridManager.SecondGrid.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));

                if (i == -1)
                    for (int j = 0; j < 5; j++)
                    {
                        EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(50));
                    }
                else
                    //for (int j = 0; j < gridManager.secondItemGrids[i].Count; j++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (i > gridManager.SecondGrid[i].Count - 1)
                        {
                            continue;
                        }

                        if (gridManager.SecondGrid[i][j] != null)
                        {
                            string textComponent = gridManager.SecondGrid[i][j];
                            if (textComponent != null)
                            {
                                EditorGUILayout.LabelField(textComponent, GUILayout.Width(50));
                            }
                        }
                    }

                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("First Grid", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (gridManager.FirstGrid != null)
        {
            for (int i = -1; i < gridManager.FirstGrid.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));

                if (i == -1)
                    for (int j = 0; j < 5; j++)
                    {
                        EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(50));
                    }
                else
                    for (int j = 0; j < 5; j++)
                    {
                        if (i > gridManager.FirstGrid[i].Count - 1)
                        {
                            continue;
                        }

                        if (gridManager.FirstGrid[i][j] != null)
                        {
                            string textComponent = gridManager.FirstGrid[i][j];
                            if (textComponent != null)
                            {
                                EditorGUILayout.LabelField(textComponent, GUILayout.Width(50));
                            }
                        }
                    }

                EditorGUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Final Main Grid", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (gridManager.FinalMainGrid != null)
        {
            for (int i = -1; i < gridManager.FinalMainGrid.Count; i++)
            {
                if (i >= 0 && i == gridManager.FinalMainGrid.Count - 4)
                    EditorGUILayout.LabelField("-----------------Top Grid Part-----------------");
                EditorGUILayout.BeginHorizontal();

                if(i >= 0)
                    EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));
                else
                    EditorGUILayout.LabelField("", GUILayout.Width(50));

                if (i == -1)
                    for (int j = 0; j < 5; j++)
                    {
                        EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(50));
                    }
                else
                    for (int j = 0; j < gridManager.FinalMainGrid[i].Count; j++)
                    {
                        if (gridManager.FinalMainGrid[i][j] != null)
                        {
                            string textComponent = gridManager.FinalMainGrid[i][j];
                            if (textComponent != null)
                            {
                                //if (textComponent == " ")
                                //{
                                //    EditorGUILayout.LabelField(" ", GUILayout.Width(50));
                                //}
                                //else
                                //{
                                    EditorGUILayout.LabelField(textComponent, GUILayout.Width(50));
                                //}
                            }
                        }
                    }

                EditorGUILayout.EndHorizontal();
            }
        }

        //GUILayout.Space(10);
        //EditorGUILayout.LabelField("First Tumble Grids", EditorStyles.boldLabel);
        //GUILayout.Space(10);
        //if (gridManager.firstTumbleItemGrids != null)
        //{
        //    for (int i = -1; i < gridManager.firstTumbleItemGrids.Count; i++)
        //    {
        //        EditorGUILayout.BeginHorizontal();

        //        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));

        //        if (i == -1)
        //            for (int j = 0; j < 5; j++)
        //            {
        //                EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(50));
        //            }
        //        else
        //            //for (int j = 0; j < gridManager.firstTumbleItemGrids[i].Count; j++)
        //            for (int j = 0; j < 5; j++)
        //            {
        //                if (i > gridManager.firstTumbleItemGrids[i].Count - 1)
        //                {
        //                    continue;
        //                }

        //                if (gridManager.firstTumbleItemGrids[j][i] != null)
        //                {
        //                    string textComponent = gridManager.firstTumbleItemGrids[j][i];
        //                    EditorGUILayout.LabelField(textComponent, GUILayout.Width(50));
        //                }
        //            }

        //        EditorGUILayout.EndHorizontal();
        //    }
        //}

        //GUILayout.Space(10);
        //EditorGUILayout.LabelField("Combine Main And First Tumble Grid", EditorStyles.boldLabel);
        //GUILayout.Space(10);
        //if (gridManager.secondItemGrids != null)
        //{
        //    for (int i = -1; i < gridManager.secondItemGrids.Count; i++)
        //    {
        //        EditorGUILayout.BeginHorizontal();

        //        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));

        //        if (i == -1)
        //            for (int j = 0; j < 5; j++)
        //            {
        //                EditorGUILayout.LabelField(j.ToString(), GUILayout.Width(50));
        //            }
        //        else
        //            //for (int j = 0; j < gridManager.secondItemGrids[i].Count; j++)
        //            for (int j = 0; j < 5; j++)
        //            {
        //                if (i > gridManager.secondItemGrids[i].Count - 1)
        //                {
        //                    continue;
        //                }

        //                if (gridManager.secondItemGrids[j][i] != null)
        //                {
        //                    string textComponent = gridManager.secondItemGrids[j][i];
        //                    if (textComponent != null)
        //                    {
        //                        EditorGUILayout.LabelField(textComponent, GUILayout.Width(50));
        //                    }
        //                }
        //            }

        //        EditorGUILayout.EndHorizontal();
        //    }
        //}
    }
}
#endif