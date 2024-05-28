using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;

using System.Reflection;
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
        private string calTrainerState = "Off";
        private Rect windowRect = new Rect(0, 0, 400, 400);
        private int tabIndex = 0;
        private Color backgroundColor = Color.grey;
        private bool showMenu = true;
        private Vector2 mainScrollPos = Vector2.zero;
        private Vector2 scrollPos = Vector2.zero;
        //private Hook damageHook;
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

            //damageHook = new Hook(
            //    typeof(Player).GetMethod("TakeShellDamage", BindingFlags.Instance | BindingFlags.Public),
            //    typeof(Main).GetMethod("TakeShellDamage"));
        }

        public static void TakeShellDamage(Action<Player, float, bool, bool> orig, Player self, float damage, bool crushing, bool trueDamage = false)
        {
            damage = 0f;
            crushing = false;
            orig(self, damage, crushing, trueDamage);
        }

        public void Update()
        {
            _player = Player.singlePlayer;
            var gameManager = GameManager.instance;
            if (_player != null)
            {
                //if (Input.GetKeyDown(KeyCode.L))
                //    DebugSettings.settings.debugBuildSettings.debugMode = !DebugSettings.settings.debugBuildSettings.debugMode;

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

                if (Input.GetKeyDown(KeyCode.Home) && false) // 'ontrigger' Discord user code for Adding shaders to PlayerBlocker layers and showing them.
                {
                    var triggersLayer = LayerMask.NameToLayer("PlayerBlocker");

                    var triggerObjects = GameObject.FindObjectsOfType<GameObject>()
                        .Where(obj =>
                        {
                            return obj.layer == triggersLayer
                            || (obj.GetComponent<Collider>() != null && obj.GetComponent<MeshRenderer>() == null);
                        })
                        .ToList();

                    /*var allRenderers = triggerObjects.SelectMany(obj => obj.GetComponents<Renderer>()).ToList();
                    var rendererWithTriggerMaterial = allRenderers.First(r => {
                        return r.material != null && r.material.name.StartsWith("Trigger");
                    });

                    var rendererWithBlockerMaterial = allRenderers.First(r => {
                        return r.material != null && r.material.name.StartsWith("Tools_NavBlock");
                    });

                    var triggerMaterial = GameObject.Instantiate(rendererWithTriggerMaterial.material);
                    var blockerMaterial = GameObject.Instantiate(rendererWithBlockerMaterial.material);*/
                    var colliders = triggerObjects
                        .SelectMany(obj => obj.GetComponents<Collider>())
                        .Where(box => box != null)
                        .ToList();

                    for (int i = 0; i < colliders.Count; i++)
                    {
                        var col = colliders[i];
                        var renderer = col.GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.gameObject.layer = 0;
                            continue;
                        }

                        if (col.gameObject.layer == triggersLayer)
                        {
                            col.gameObject.layer = 0;
                        }

                        if (col is BoxCollider)
                        {
                            var box = col as BoxCollider;
                            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                            cube.transform.position = box.transform.TransformPoint(box.center);
                            cube.transform.SetParent(box.transform, true);
                            cube.transform.localScale = box.size;

                            cube.GetComponent<Collider>().enabled = false;
                            Material mat = new Material(Shader.Find("AggroCrab/PropShader"));

                            cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
                            cube.transform.localEulerAngles = new Vector3();
                        }
                    }

                    //CAL Jump code
                    SkinnedMeshRenderer mr = _player.transform.GetComponentInChildren<SkinnedMeshRenderer>(true);
                    //MethodInfo method = AccessTools.Method(typeof(Player), "Jump");
                    Material mat = null;

                    //Blink near correct timing
                    if (_player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= .525 && _player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime < .575 && _player.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "an_Kril_AtkChargeThrust")
                    {

                        if (mr != null && hasSetMat == false && (calTrainerState == "Visual" || calTrainerState == "Automatic"))
                        {
                            mat = mr.material;
                            hasSetMat = true;
                            Debug.Log("found");
                            mr.SetMaterial(null);
                            mr.material.color = Color.blue;
                            _player.StartCoroutine(ColorFlashCoroutine(mat, mr));
                        }
                    }
                    //Auto Jump
                    if (_player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= .55 && _player.anim.GetCurrentAnimatorStateInfo(0).normalizedTime < .65 && _player.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "an_Kril_AtkChargeThrust")
                    {
                        if (calTrainerState == "Automatic")
                        {
                            _player.Jump()
                        }
                    }
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
            {
                if (_player != null)
                {
                    _player.CancelInvincibility();
                    _player.statusEffects.stealth = false;
                }

                //damageHook?.Undo();
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

            if (showMenu)
            {
                GUI.backgroundColor = backgroundColor;

                windowRect = GUI.Window(0, windowRect, MenuWindow, "Menu SDK <GH> V0.6 ('Insert' to Show/Hide Menu)");
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
                    DebugSettings.settings.debugBuildSettings.debugMode = GUILayout.Toggle(DebugSettings.settings.debugBuildSettings.debugMode, "Enable Debug Mode.");
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
                    if (GUILayout.Button("CAL Trainer - " + calTrainerState))
                    {
                        switch (calTrainerState)
                        {
                            case "Visual": calTrainerState = "Automaitc"; break;
                            case "Automatic": calTrainerState = "Off"; break;
                            default: calTrainerState = "Visual"; break;
                        }
                    }
                    GUILayout.Label("Key 'i' ==> Toggle GodMode (Speed, unkillable, ..etc).");
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
