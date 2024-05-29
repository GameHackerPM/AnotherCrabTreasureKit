using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity;

using System.Reflection;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;


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
        string fadingLabelContent = "", velocityVal = "";
        private string ForDebuggingString = "";
        private bool isInvincible = false, airJump = false, isInvisible = false, invisiblityStatus = false, hasSetMat = false;
        private string calTrainerState = "Off";
        private Rect windowRect = new Rect(0, 0, 400, 400);
        private int tabIndex = 0;
        private Color backgroundColor = Color.grey;
        private bool showMenu = true;
        private bool showVelocity = true, showEnemies = true;
        private Vector2 mainScrollPos = Vector2.zero;
        private Vector2 scrollPos = Vector2.zero;
        private GUIStyle infoGUIStyle = null;
        private List<Enemy> enemyList = null;
        private Camera _mCamera;
        private IEnumerator EntityUpdate = null;

        Main()
        {
            infoGUIStyle = new GUIStyle();
            infoGUIStyle.normal.textColor = Color.red;
            infoGUIStyle.border.Add(new Rect(1f, 1f, 1f, 1f));
            _mCamera = Camera.main;
        }
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

            EntityUpdate = EntityUpdateFunc(0f);
            StartCoroutine(EntityUpdate);
            FadeLabel("Game Injected!");
        }
        private Vector3 W2S(Vector3 worldPosition)
        {
            return _mCamera.WorldToScreenPoint(worldPosition);
        }

        private float Distance(Vector3 worldPosition)
        {
            return Vector3.Distance(_mCamera.transform.position, worldPosition);
        }
        private bool IsVisible(GameObject target, Vector3 position, Vector3 origin, LayerMask mask)
        {
            Ray ray = new Ray(origin, (position - origin).normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, Single.PositiveInfinity, mask))
            {
                if (hit.collider.gameObject == target)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        private void Basic_ESP(Vector3 position, string text, float additionalY = 0f)
        {
            Vector3 pos = _mCamera.WorldToScreenPoint(position);
            if (pos.z > 0)
            {
                GUI.Label(new Rect(
                        pos.x,
                        Screen.height - pos.y + additionalY,
                        pos.x + (text.Length * GUI.skin.label.fontSize),
                        Screen.height - pos.y + GUI.skin.label.fontSize * 2),
                    text);
            }
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
                _mCamera = Camera.main;
                //if (Input.GetKeyDown(KeyCode.L))
                //    DebugSettings.settings.debugBuildSettings.debugMode = !DebugSettings.settings.debugBuildSettings.debugMode;

                if (showVelocity)
                    velocityVal = $"Curr Velocity: {(_player.velocity.x != 0.0f ? _player.velocity.magnitude : 0.0f):F}";

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
                }

                if (calTrainerState != "Off" && _player.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name == "an_Kril_AtkChargeThrust")
                {
                    var animState = _player.anim.GetCurrentAnimatorStateInfo(0);
                    if (animState.normalizedTime >= .525 && animState.normalizedTime < .575)
                    {
                        //CAL Jump code
                        SkinnedMeshRenderer mr = _player.transform.GetComponentInChildren<SkinnedMeshRenderer>(true);
                        Material matx = null;

                        if (mr != null && hasSetMat == false &&
                            (calTrainerState == "Visual" || calTrainerState == "Automatic"))
                        {
                            matx = mr.material;
                            hasSetMat = true;
                            mr.material.color = Color.blue;
                            mr.material = null;
                            _player.StartCoroutine(ColorFlashCoroutine(matx, mr));
                        }
                    }

                    //Auto Jump
                    if (calTrainerState == "Automatic" && animState.normalizedTime >= .55 && animState.normalizedTime < .65)
                    {
                        _player.CallProtectedMethod("Jump");
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
                    {
                        _player.CallProtectedMethod("Jump");
                    }
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

                if (showEnemies && EntityUpdate == null)
                {
                    EntityUpdate = EntityUpdateFunc(0f);
                    StartCoroutine(EntityUpdate);
                }
            }
            else
            {
                enemyList = null;
            }

            if (Input.GetKeyDown(KeyCode.Insert))
                showMenu = !showMenu;

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (_player != null)
                {
                    _player.CancelInvincibility();
                    _player.statusEffects.stealth = false;
                    if (gameManager.godMode)
                        gameManager.ToggleGodmode();
                    showEnemies = false;
                    EntityUpdate = null;
                }

                Loader.UnLoad();
            }
        }

        public IEnumerator ColorFlashCoroutine(Material mat, SkinnedMeshRenderer mr)
        {
            yield return new WaitForSeconds(.1f);
            mr.material = mat;
            hasSetMat = false;
        }

        private IEnumerator EntityUpdateFunc(float time)
        {
            yield return new WaitForSeconds(time);
            enemyList = FindObjectsOfType<Enemy>().ToList();
            if (showEnemies)
            {
                EntityUpdate = EntityUpdateFunc(3f);
                StartCoroutine(EntityUpdate);
            }
            else
                EntityUpdate = null;
        }
        public void FadeLabel(string message)
        {
            alphaAmount = 0f;
            fadingLabelContent = message;
            showFadingLabel = true;
        }
        public void OnGUI()
        {
            //if (UnityEngine.Event.current.type != EventType.Repaint)
            //    return;
            if (!string.IsNullOrEmpty(ForDebuggingString))
                GUI.Label(new Rect(Screen.width / 2, 60, 200f, 150f), ForDebuggingString);
            if (showFadingLabel && alphaAmount < 2f)
            {
                alphaAmount += 0.3f * Time.deltaTime;
                GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaAmount >= 1f ? 2f - alphaAmount : alphaAmount);
                GUI.Label(new Rect(Screen.width / 2, 40, 200f, 50f), fadingLabelContent);
            }
            else if (alphaAmount >= 2f)
            {
                alphaAmount = 0f;
                GUI.color = originalColor;
                showFadingLabel = false;
            }

            if (showMenu)
            {
                GUI.backgroundColor = backgroundColor;

                windowRect = GUI.Window(0, windowRect, MenuWindow, "Menu SDK <GH> V0.7 ('Insert' to Show/Hide Menu)");
            }

            if (showVelocity)
            {
                GUI.Label(new Rect(50f, Screen.height - 250f, 400f, 50f), velocityVal, infoGUIStyle);
            }

            if (showEnemies)
            {
                if (enemyList != null)
                {
                    foreach (var enemy in enemyList)
                    {
                        var pos = enemy.GetCenter();
                        float distance = Vector3.Distance(_mCamera.transform.position, pos);
                        if (distance > 700f)
                            continue;

                        //if (!IsVisible(enemy.gameObject, enemy.GetCenter(), _player.transform.position, GlobalSettings.settings.groundLayers))
                        //    continue;
                        //Render.DrawBox(new Vector2(pos.x, pos.y), new Vector2(20f, 20f), Color.red);
                        //GUI.DrawTexture(new Rect(
                        //    pos.x,
                        //    Screen.height - pos.y,
                        //    pos.x + 20f,
                        //    Screen.height - pos.y + 20f *2), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                        //string minifiedName = "";
                        //var splitNameParts = enemy.name.Split('_');
                        //if (splitNameParts.Length > 2)
                        //    minifiedName = splitNameParts[1] + splitNameParts[2];
                        //else
                        //    minifiedName = splitNameParts[1];

                        Basic_ESP(pos, "Name: " + enemy.name.Replace("Enemy_", ""), 30f);
                        Basic_ESP(pos, $"HP: {enemy.health:##.##}/{enemy.startingHealth:##.##}", 50f);
                    }
                }
            }
        }
        void MenuWindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(100));
            if (GUILayout.Toggle(tabIndex == 0, "Main", "Button", GUILayout.ExpandWidth(true)))
            {
                tabIndex = 0;
            }

            if (GUILayout.Toggle(tabIndex == 1, "Indicators", "Button", GUILayout.ExpandWidth(true)))
            {
                tabIndex = 1;
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
                case 1:
                    GUILayout.BeginVertical(GUI.skin.box);
                    mainScrollPos = GUILayout.BeginScrollView(mainScrollPos);
                    if (GUILayout.Button("CAL Trainer - " + calTrainerState))
                    {
                        switch (calTrainerState)
                        {
                            case "Visual": calTrainerState = "Automatic"; break;
                            case "Automatic": calTrainerState = "Off"; break;
                            default: calTrainerState = "Visual"; break;
                        }
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    showVelocity = GUILayout.Toggle(showVelocity, "Show Velocity.");
                    showEnemies = GUILayout.Toggle(showEnemies, "Show Enemy Infos.");
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
