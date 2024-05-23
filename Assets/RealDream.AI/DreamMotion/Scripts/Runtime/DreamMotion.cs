using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RealDream;
using RealDream.Network;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace RealDream.AI
{
    public class DreamMotion : MonoBehaviour
    {
        private NetClient _client;

        public string ServerIp = "100.100.35.183";
        public int ServerPort = 26950;
        public string OutputDir = "Assets/RealDream_Output";
        public static DreamMotion Instance { get; private set; }

        [Header("Config")] 
        public string VideoPath;
        [HideInInspector] public string ModelPath;

        [Header("Runtime")] [HideInInspector] public bool _isWorking = false;
        [Range(0, 1.0f)] [HideInInspector] public float _progress = 0;
        private GameEventRegisterService _eventRegister;
        
        [Header("Debug")] 
        public BVHPreview Viewer;
        public int FrameRate = 30;
        public void UploadAsset(string path, bool isFastMode = false)
        {
            if (_isWorking)
            {
                UnityEngine.Debug.Log("Working, please wait for a while");
                return;
            }

            if (!File.Exists(path))
            {
                Debug.LogError("Can not find file: " + path);
                return;
            }

            // check cache
            DoClear();
            
            if (Application.isPlaying)
                StartCoroutine(StartTask(path));
            else
                EditorCoroutineUtil.StartCoroutine(StartTask(path));
        }

        IEnumerator StartTask(string path)
        {
            _isWorking = true;
            DoAwake();
            _curPath = path;
            while (_isWorking)
            {
                _client.Update();
                yield return null;
            }

            DoClear();
            _progress = 0;
        }

        private string _curPath;
        private void OnEvent_OnServerConnected(object param)
        {
            UnityEngine.Debug.Log("OnEvent_OnServerConnected");
            FileTransferUtil.UploadFile(_curPath);
        }

        private void OnEvent_OnServerProgress(object param)
        {
            var lst = (List<object>)param;
            var fileName = (string)lst[0];
            var progress = (float)lst[1];
            _progress = progress;
            UnityEngine.Debug.Log($"OnServerProgress {progress}");
        }

        private void DoAwake()
        {
            Instance = this;
            _eventRegister = new GameEventRegisterService();
            _eventRegister.RegisterEvent(this);
            _client = new NetClient();
            _client.Awake(ServerIp, ServerPort);
            _client.Start();
        }

        public void DoClear()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            _isWorking = false;
            _progress = 0;
        }


        void OnEvent_OnServerResult(object param)
        {
            PathUtil.CreateDir(OutputDir);
            var lst = (List<object>)param;
            var fileName = (string)lst[0];
            var bytes = (byte[])lst[1];
            var hash = (string)lst[2];
            var isVideo = fileName.EndsWith(".mp4");
            var postfix = isVideo ? ".bvh" : ".fbx";
            if (fileName.Contains("@@")) // remove hash prefix
            {
                fileName = fileName.Split("@@")[1];
            }
            var path = Path.Combine(OutputDir, $"{fileName}{postfix}");
            PathUtil.CreateDir(Path.GetDirectoryName(path));
            if (File.Exists(path))
                File.Delete(path);
            Debug.Log($"OnResult to {path}");
            File.WriteAllBytes(path, bytes);

            if (Viewer != null)
            {
                Viewer.FilePath = path;
                Viewer.Parse();
                Viewer.gameObject.SetActive(true);
                Viewer.FrameRate = FrameRate;
            }
            _isWorking = false;
        }

    }
}