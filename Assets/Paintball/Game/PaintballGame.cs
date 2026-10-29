using ITKamianets.Engine.CharacterView;
using ITKamianets.Engine.Core;
using ITKamianets.Engine.Input;
using ITKamianets.Paintball.Core;
using ITKamianets.Paintball.Hud;
using ITKamianets.Paintball.Maps;
using ITKamianets.Paintball.Menus;
using ITKamianets.Paintball.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace ITKamianets.Paintball.Game
{
    /// <summary>Wires the arena, match, menus and the Character View player. Press Play in Main.</summary>
    public class PaintballGame : MonoBehaviour
    {
        FortressArena _arena;
        MatchDirector _match;
        MatchHud _hud;
        MainMenu _menu;
        PauseMenu _pause;
        ResultMenu _result;
        CharacterViewCamera _view;
        UnityEngine.Camera _camera;
        GunView _gun;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (SceneManager.GetActiveScene().name != "Main")
            {
                return;
            }

            if (Object.FindAnyObjectByType<PaintballGame>() != null)
            {
                return;
            }

            new GameObject("KP Paintball").AddComponent<PaintballGame>();
        }

        void Awake()
        {
            PaintSplat.Clear();
            PaintProjectile.Clear();
            _arena = FortressArena.ExistingOrBuild();
            SetupLight();
            SetupCamera();
            SetupUiInput();
            _match = gameObject.AddComponent<MatchDirector>();
            _hud = MatchHud.Create();
            _menu = MainMenu.Create();
            _pause = PauseMenu.Create();
            _result = ResultMenu.Create();
            _gun = GunView.Create(_camera.transform);
            _menu.TeamChosen += StartMatch;
            _pause.Resume += Resume;
            _pause.Leave += LeaveToMenu;
            _result.Leave += LeaveToMenu;
            _match.Respawned += OnRespawned;
            Overview();
            _menu.Show();
        }

        void OnEnable()
        {
            EngineEvents.Subscribe<PaintballMatchEnded>(OnMatchEnded);
        }

        void OnDisable()
        {
            EngineEvents.Unsubscribe<PaintballMatchEnded>(OnMatchEnded);
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            var input = EngineInput.Default;
            if (input.UI.Pause.WasPressedThisFrame())
            {
                if (_result.gameObject.activeSelf)
                {
                    LeaveToMenu();
                }
                else if (_pause.gameObject.activeSelf)
                {
                    Resume();
                }
                else if (_match.IsRunning)
                {
                    Pause();
                }
            }

            if (_hud.gameObject.activeSelf)
            {
                _hud.Tick(_match);
            }
        }

        void StartMatch(TeamId team)
        {
            Time.timeScale = 1f;
            _menu.Hide();
            _pause.Hide();
            _result.Hide();
            PaintSplat.Clear();
            PaintProjectile.Clear();
            _match.Begin(team, _arena.Spawns, _arena.Posts, FighterFactory.Create);
            _view.Target = _match.Player.Motor;
            _view.SetLook(_match.Player.Motor.Yaw, 5f);
            _gun.SetTeam(team);
            _gun.gameObject.SetActive(true);
            _hud.gameObject.SetActive(true);
            EngineInput.Default.PlayerEnabled = true;
        }

        void Pause()
        {
            Time.timeScale = 0f;
            EngineInput.Default.PlayerEnabled = false;
            _pause.Show();
        }

        void Resume()
        {
            _pause.Hide();
            Time.timeScale = 1f;
            if (_match.IsRunning)
            {
                EngineInput.Default.PlayerEnabled = true;
            }
        }

        void LeaveToMenu()
        {
            Time.timeScale = 1f;
            EngineInput.Default.PlayerEnabled = false;
            _match.Clear();
            PaintProjectile.Clear();
            _view.Target = null;
            _gun.gameObject.SetActive(false);
            _hud.gameObject.SetActive(false);
            _pause.Hide();
            _result.Hide();
            Overview();
            _menu.Show();
        }

        void OnMatchEnded(PaintballMatchEnded ended)
        {
            EngineInput.Default.PlayerEnabled = false;
            PaintProjectile.Clear();
            _pause.Hide();
            _gun.gameObject.SetActive(false);
            _result.Show(ended.Winner);
        }

        void OnRespawned(Fighter fighter)
        {
            if (fighter.IsPlayer && _view != null)
            {
                _view.SetLook(fighter.Motor.Yaw, 5f);
            }
        }

        void SetupCamera()
        {
            _camera = UnityEngine.Camera.main;
            _camera.nearClipPlane = 0.08f;
            _view = _camera.GetComponent<CharacterViewCamera>();
            if (_view == null)
            {
                _view = _camera.gameObject.AddComponent<CharacterViewCamera>();
            }

            _view.Settings.FieldOfView = 80f;
            _view.Settings.StartView = CameraView.FirstPerson;
            _view.Settings.AllowToggle = true;
            _view.Target = null;
        }

        void SetupUiInput()
        {
            var system = Object.FindAnyObjectByType<EventSystem>();
            if (system == null)
            {
                system = new GameObject("EventSystem").AddComponent<EventSystem>();
            }

            var module = system.GetComponent<InputSystemUIInputModule>();
            if (module == null)
            {
                module = system.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            var input = EngineInput.Default;
            var leftover = module.actionsAsset;
            module.actionsAsset = input.Asset;
            module.point = InputActionReference.Create(input.UI.Point);
            module.leftClick = InputActionReference.Create(input.UI.Click);
            module.move = InputActionReference.Create(input.UI.Navigate);
            module.submit = InputActionReference.Create(input.UI.Submit);
            module.cancel = InputActionReference.Create(input.UI.Cancel);
            module.scrollWheel = InputActionReference.Create(input.UI.ScrollWheel);
            if (leftover != null && leftover != input.Asset)
            {
                leftover.Disable();
            }

            input.PlayerEnabled = false;
        }

        void SetupLight()
        {
            var light = Object.FindAnyObjectByType<Light>();
            if (light != null)
            {
                light.color = new Color(1f, 0.95f, 0.88f);
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                light.transform.rotation = Quaternion.Euler(46f, -30f, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.68f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.46f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.14f, 0.16f, 0.12f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.64f, 0.76f, 0.84f);
            RenderSettings.fogStartDistance = 48f;
            RenderSettings.fogEndDistance = 110f;
        }

        void Overview()
        {
            var position = new Vector3(-34f, 18f, -28f);
            _camera.fieldOfView = 58f;
            _camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(new Vector3(0f, 2f, 2f) - position));
        }
    }
}
