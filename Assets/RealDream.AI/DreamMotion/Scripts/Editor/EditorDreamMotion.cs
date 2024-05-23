using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RealDream.AI
{
    [CustomEditor(typeof(DreamMotion))]
    public class EditorDreamMotion : Editor
    {
        private DreamMotion _owner;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _owner = target as DreamMotion;
            GUILayout.Space(20);
            switch (_owner._curState)
            {
                case DreamMotion.State.Idle:
                    GUILayout.BeginHorizontal();
                    DrawButton(_owner.ModelPath, "BindModel");
                    DrawButton(_owner.VideoPath, "ParseVideo");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    break;
                case DreamMotion.State.Done:
                    break;
                case DreamMotion.State.WaitingResult:
                    if(_owner._progress>0.5f) 
                        EditorGUILayout.LabelField("Next step would be slow, please wait for a while about 1-4 minutes");
                    if (_owner._progress > 0)
                        EditorGUILayout.LabelField($"Progress: {(_owner._progress * 100):0.00}%");
                    GUILayout.Space(20);
                    if (_owner._progress > 0)
                    {
                        if (GUILayout.Button("Stop"))
                        {
                            _owner.StopTask();
                            EditorApplication.isPlaying = false;
                        }
                    }
                    break;
            }

            GUILayout.Space(10);
        }

        void DrawButton(string path, string msg)
        {
            if ( File.Exists(path) && GUILayout.Button(msg))
            {
                if (!Application.isPlaying)
                {
                    _owner._awakeTaskPath = _owner.VideoPath;
                    EditorApplication.isPlaying = true;
                }
                else
                {
                    _owner._awakeTaskPath = "";
                    _owner.StartTask(_owner.VideoPath);
                }
            }
        }
    }
}