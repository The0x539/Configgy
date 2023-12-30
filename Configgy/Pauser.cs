using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Configgy
{
    public static class Pauser
    {
        public static bool Paused => GameStateManager.Instance.IsStateActive(pauseKey);

        private static GameState configState = new GameState("configgyPause");

        private static List<GameObject> pauserObjects = new List<GameObject>();

        const string pauseKey = "ConfiggyPauser";

        private static BehaviourRelay watcher;
        

        public static void Pause(GameObject origin)
        {
            pauserObjects = pauserObjects.Where(x => x != null && x.activeInHierarchy).Distinct().ToList();
            
            if(!pauserObjects.Contains(origin))
                pauserObjects.Add(origin);
            
            CheckWatcher();
            SetPaused(true);
        }

        public static void Pause(params GameObject[] origins)
        {
            pauserObjects = pauserObjects.Where(x => x != null && x.activeInHierarchy).Distinct().ToList();

            for (int i=0;i<origins.Length;i++)
            {
                if (!pauserObjects.Contains(origins[i]))
                    pauserObjects.Add(origins[i]);
            }

            CheckWatcher();
            SetPaused(true);
        }

        private static void CheckWatcher()
        {
            if(watcher == null)
            {
                watcher = new GameObject("ConfiggyPauserWatcher").AddComponent<BehaviourRelay>();
                GameObject.DontDestroyOnLoad(watcher.gameObject);
                watcher.StartCoroutine(PauseWatcher());
            }
        }

        private static void SetPaused(bool paused)
        {
            if (paused == Paused)
                return;

            configState = new GameState(pauseKey);

            configState.cursorLock = LockMode.Unlock;
            configState.playerInputLock = LockMode.Lock;
            configState.cameraInputLock = LockMode.Lock;
            configState.priority = 20;

            Time.timeScale = paused ? 0f : 1f;
            OptionsManager.Instance.paused = paused;
            NewMovement.Instance.enabled = !paused;
            CameraController.Instance.enabled = !paused;
            GunControl.Instance.activated = !paused;

            if(paused)
                GameStateManager.Instance.RegisterState(configState);
            else
                GameStateManager.Instance.PopState(pauseKey);
        }

        private static IEnumerator PauseWatcher()
        {
            while (true)
            {
                if (Paused)
                {
                    if (!RemainPaused())
                    {
                        SetPaused(false);
                    }
                }

                yield return null;
            }
        }

        private static bool RemainPaused()
        {
            for (int i = 0; i < pauserObjects.Count; i++)
            {
                if (pauserObjects[i] == null)
                    continue;

                if (pauserObjects[i].activeInHierarchy)
                    return true;
            }

            return false;
        }

        

        public static void Unpause(GameObject origin = null)
        {
            if(origin != null)
                if(pauserObjects.Contains(origin))
                    pauserObjects.Remove(origin);

            pauserObjects = pauserObjects.Where(x => x != null && x.activeInHierarchy).ToList();

            if (pauserObjects.Count > 0)
                return;

            SetPaused(false);
        }
    }
}
