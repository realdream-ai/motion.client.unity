using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if REALDREAM_MOTION
using RealDream.Network;

namespace RealDream.AI
{
    public class DreamMotion : MonoBehaviour
    {
        public enum State
        {　
            Idle,
            WaitingResult,
            Done
        }

        [Header("Input")] //
        public string VideoPath;

        [HideInInspector] public string ModelPath;

        [Header("Output")] //
        public string OutputDir = "Output/DreamMotion";

        [Header("Debug")] //
        public BVHPreview Viewer;

        [Header("Runtime")] //
        [HideInInspector]
        public State _curState = State.Idle;


        [Range(0, 1.0f)] [HideInInspector] public float _progress = 0;
        private GameEventRegisterService _eventRegister;
        private int _curFrameRate = 60;
        private NetClient _client;
        private string _curPath;

        [Header("Runtime")] //
        [HideInInspector]
        public string _awakeTaskPath;

        private string _curFilePath;
        private void Start()
        {
            if (!string.IsNullOrEmpty(_awakeTaskPath))
            {
                _curFilePath = _awakeTaskPath;
                StartTask(_awakeTaskPath);
                _awakeTaskPath = null;
            }
        }

        public void StartTask(string path)
        {
            if (_curState == State.WaitingResult)
            {
                UnityEngine.Debug.Log("Working, please wait for a while");
                return;
            }

            if (!File.Exists(path))
            {
                Debug.LogError("Can not find file: " + path);
                return;
            }

            _curState = State.Idle;
            // check cache
            var hashTag = HashUtil.CalcHash(path) + CacheUtil.CacheSplitTag;
            var cachePath = PathUtil.GetAllPath(OutputDir, "*.*")
                    .Where(a => !a.EndsWith(".meta"))
                    .FirstOrDefault(a => Path.GetFileNameWithoutExtension(a).StartsWith(hashTag))
                ;
            if (!string.IsNullOrEmpty(cachePath))
            {
                Debug.Log("Load from cache " + cachePath);
                ShowResult(cachePath);
                return;
            }

            DoClear();
            if (Application.isPlaying)
                StartCoroutine(DoTask(path));
            else
                EditorCoroutineUtil.StartCoroutine(DoTask(path));
        }


        public void StopTask()
        {
            DoClear();
        }


        private IEnumerator DoTask(string path)
        {
            _curState = State.WaitingResult;
            DoAwake();
            _curPath = path;
            while (_curState == State.WaitingResult)
            {
                _client.Update();
                yield return null;
            }

            DoClear();
            _progress = 0;
        }


        private void DoAwake()
        {
            _eventRegister = new GameEventRegisterService();
            _eventRegister.RegisterEvent(this);
            _client = new NetClient();
            _client.Awake(GlobalConfig.Instance.DefaultServerIp, GlobalConfig.Instance.DefaultServerPort);
            _client.Start();
        }

        private void DoClear()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            _progress = 0;
            _curState = State.Idle;
        }

        private void OnEvent_OnServerProgress(object param)
        {
            var lst = (List<object>)param;
            var fileName = (string)lst[0];
            var progress = (float)lst[1];
            _progress = progress;
            UnityEngine.Debug.Log($"OnServerProgress {progress}");
        }
        private void OnEvent_OnServerResService(object param)
        {
            SaveAndQuit(param);
        }
        private void OnEvent_OnServerConnected(object param)
        {
            UnityEngine.Debug.Log("OnEvent_OnServerConnected");
            _RequestService(EServiceType.Mocap,(int)EDrawingServiceType.All, _curPath);
        }

        private void _RequestService(EServiceType serviceType,int msgType, string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Can not find file: " + path);
                return;
            }

            Debug.Log("RequestService " + serviceType + " " + path);
            ClientSend.ReqService(serviceType,(int)msgType, path);
        }
        
        private void SaveAndQuit(object param)
        {
            var lst = (List<object>)param;
            var file = (string)lst[0];
            var bytes = (byte[])lst[1];
            PathUtil.CreateDir(OutputDir);
            var hashTag = HashUtil.CalcHash(_curFilePath) + CacheUtil.CacheSplitTag;
            var fileName = Path.GetFileName(_curFilePath);
            var path = Path.Combine(OutputDir, hashTag +fileName+ ".bvh");
            Debug.Log($"OnResult to {path}");
            File.WriteAllBytes(path, bytes);
            _client.Disconnect();
            ShowResult(path);
        }
        
        private void ShowResult(string path)
        {
            if (Viewer != null)
            {
                Viewer.FilePath = path;
                Viewer.Parse();
                Viewer.gameObject.SetActive(true);
                Viewer.FrameRate = _curFrameRate;
            }

            _curState = State.Done;
        }
    }
}
#endif