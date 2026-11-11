using System;
using System.Collections;
using Game.Presentation.Ui;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Presentation.App
{
    /// <summary>Replaces scenes asynchronously behind the persistent loading overlay.</summary>
    [DisallowMultipleComponent]
    public sealed class SceneNavigator : MonoBehaviour, ISceneNavigator
    {
        private string _mainMenu;
        private string _match;
        private LoadingScreenView _loading;
        private AsyncOperation _operation;

        public bool IsLoading { get; private set; }

        public void Configure(string mainMenu, string match, LoadingScreenView loading)
        {
            _mainMenu = mainMenu;
            _match = match;
            _loading = loading;
        }

        public void ToMainMenu() => Load(_mainMenu);

        public void ToMatch() => Load(_match);

        private void Load(string scene)
        {
            if (IsLoading) return;
            IsLoading = true;
            var showOverlay = _loading.ShowOnTransitions;
            if (showOverlay) _loading.Show();
            StartCoroutine(Transition(scene, showOverlay));
        }

        private IEnumerator Transition(string scene, bool showOverlay)
        {
            if (showOverlay) yield return null;
            _operation = BeginLoad(scene);
            if (_operation == null)
            {
                _loading.HideImmediately();
                IsLoading = false;
                yield break;
            }
            _operation.allowSceneActivation = !showOverlay;
            while (!_operation.isDone)
            {
                if (showOverlay)
                {
                    _loading.SetProgress(_operation.progress);
                    if (_operation.progress >= .9f) _operation.allowSceneActivation = true;
                }
                yield return null;
            }
            if (showOverlay) _loading.SetProgress(1);
            // Scene roots receive services in sceneLoaded, then build their UI in Start.
            yield return null;
            if (showOverlay) yield return _loading.FadeOut();
            _operation = null;
            IsLoading = false;
        }

        private static AsyncOperation BeginLoad(string scene)
        {
            try { return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single); }
            catch (Exception exception) { Debug.LogException(exception); return null; }
        }

        private void OnDestroy()
        {
            if (_operation != null && !_operation.isDone) _operation.allowSceneActivation = true;
        }
    }
}
