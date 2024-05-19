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
        private List<ShellCollectable> ShellCollectableList = null;
        float alphaAmount = 0f;
        bool showFadingLabel = false;
        Color originalColor;
        string fadingLabelContent = "";
        private bool isInvincible = false, airJump = false, isInvisible = false, invisiblityStatus = false;
        private Rect windowRect = new Rect(0, 0, 400, 400);
        private int tabIndex = 0;
        private Color backgroundColor = Color.grey;
        private bool showMenu = true;
        private Vector2 mainScrollPos = Vector2.zero;
        private Vector2 scrollPos = Vector2.zero;
        public void Start()
        {
            ShellCollectableList = new List<ShellCollectable>();
            foreach (var collectableItemData in InventoryMasterList.staticList)
            {
                if (collectableItemData is ShellCollectable shellCollectableItem)
                {
                    ShellCollectableList.Add(shellCollectableItem);
                }
            }
            FadeLabel("Game Injected!");
        }
        public void Update()
        {
            _player = Player.singlePlayer;
            var gameManager = GameManager.instance;
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
                if (gameManager != null)
                {
                    if (Input.GetKeyDown(KeyCode.I))
                    {
                        gameManager.ToggleGodmode();
                        DebugSettings_GodMode.godmode.AddDefaultGodMode();
                        DebugSettings_GodMode.godmode.CycleGodMode();
                    }
                    if (Input.GetKeyDown(KeyCode.U))
                    {
                        isInvincible = !isInvincible;
                        if (!isInvincible)
                            _player.CancelInvincibility();

                        // Physics.gravity = Vector3.down * 1f; // Gravity Modifier
                    }
                }

                if (isInvincible)
                    _player.MakeInvincible(float.MaxValue);

                if (airJump)
                {
                    if (_player.input.controller.GetButtonDown("Jump"))
                        _player.EjectJump();
                }

                if (isInvisible)
                {
                    invisiblityStatus = true;
                    _player.statusEffects.stealth = true;
                }
                else if (invisiblityStatus)
                {
                    _player.statusEffects.stealth = false;
                    invisiblityStatus = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Insert))
                showMenu = !showMenu;

            if (Input.GetKeyDown(KeyCode.Delete))
                Loader.UnLoad();
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

            if (showMenu)
            {
                GUI.backgroundColor = backgroundColor;

                windowRect = GUI.Window(0, windowRect, MenuWindow, "Menu SDK <GH> V0.5 ('Insert' to Show/Hide Menu)");
            }
            //GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 150f, 50f), "Game Injected!");
        }
        void MenuWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(100));
            if (GUILayout.Toggle(tabIndex == 0, "Main", "Button", GUILayout.ExpandWidth(true)))
            {
                tabIndex = 0;
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            switch (tabIndex)
            {
                case 0:
                    GUILayout.BeginVertical(GUI.skin.box);
                    mainScrollPos = GUILayout.BeginScrollView(mainScrollPos);
                    GUILayout.Label("Equip Shells:");
                    scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.MaxHeight(120));
                    foreach (var shellCollectable in ShellCollectableList)
                    {
                        if (GUILayout.Button(shellCollectable.name.Replace("Inv_Shell_", ""), GUILayout.MaxWidth(240)))
                        {
                            if (_player != null)
                                shellCollectable.Equip();
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    airJump = GUILayout.Toggle(airJump, "Air Jump");
                    isInvincible = GUILayout.Toggle(isInvincible, "Invincible (U)");
                    isInvisible = GUILayout.Toggle(isInvisible, "Invisible");
                    GUILayout.Label("Key 'P' ==> Save Current Location.");
                    GUILayout.Label("Key 'O' ==> Load Last Saved Location.");
                    GUILayout.Label("Key 'Delete' ==> Unload the tool.");
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    break;
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUI.DragWindow(); // Allow the user to drag the window around
        }
    }
}
