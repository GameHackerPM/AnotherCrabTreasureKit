using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;
using UnityEngine;

namespace GHAnotherCrabKit
{
    class Main : MonoBehaviour
    {
        private Player _player;
        private Vector3 LastSavedPosition = Vector3.zero;
        float alphaAmount = 0f;
        bool showFadingLabel = false;
        Color originalColor;
        string fadingLabelContent = "";
        public void Start()
        {
            //_player = FindObjectOfType<Player>();
            FadeLabel("Game Injected!");
        }
        public void Update()
        {
            _player = FindObjectOfType<Player>();
            if (_player != null)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    LastSavedPosition = _player.transform.position;
                    FadeLabel($"{LastSavedPosition.x},{LastSavedPosition.y},{LastSavedPosition.z}");
                    GUIManager.instance.HUD.itemNotification.Play("Location Saved.", true, null, "", 1);
                }

                if (Input.GetKeyDown(KeyCode.O) && LastSavedPosition != Vector3.zero)
                {
                    _player.transform.position = LastSavedPosition;
                    FadeLabel($"{LastSavedPosition.x},{LastSavedPosition.y},{LastSavedPosition.z}");
                    GUIManager.instance.HUD.itemNotification.Play("Location Loaded.", true, null, "", 1);
                }
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Loader.UnLoad();
            }
        }

        public void FadeLabel(string message)
        {
            alphaAmount = 0f;
            fadingLabelContent = message;
            showFadingLabel = true;
        }

        public void OnGUI()
        {
            if (showFadingLabel && alphaAmount < 1f)
            {
                alphaAmount += 0.3f * Time.deltaTime;
                GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaAmount);
                GUI.Label(new Rect(Screen.width / 2, 40, 200f, 50f), fadingLabelContent);
            }
            else if (alphaAmount >= 1f)
            {
                alphaAmount = 0f;
                GUI.color = originalColor;
                showFadingLabel = false;
            }
            //GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 150f, 50f), "Game Injected!");
        }
    }
}
