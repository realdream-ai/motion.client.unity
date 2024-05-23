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
            GUILayout.BeginHorizontal();

            DrawButton(_owner.ModelPath, "BindModel");
            DrawButton(_owner.VideoPath, "ParseVideo");

            if (_owner._isWorking && GUILayout.Button("Stop"))
            {
                _owner.DoClear();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            
            if (_owner._isWorking&& _owner._progress > 0)
            {
                if(_owner._progress>0.5f) 
                   EditorGUILayout.LabelField("Next step would be slow, please wait for a while about 1-4 minutes");
                EditorGUILayout.LabelField($"Progress: {(_owner._progress * 100):0.00}%");
            }
            GUILayout.Space(10);
        }

        void DrawButton(string path, string msg)
        {
            if (!_owner._isWorking && File.Exists(path) && GUILayout.Button(msg))
            {
                _owner.UploadAsset(_owner.VideoPath);
            }
        }
    }
}