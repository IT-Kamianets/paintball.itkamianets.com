using ITKamianets.Engine.Core;
using ITKamianets.Paintball.Core;
using ITKamianets.Paintball.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace ITKamianets.Paintball.Hud
{
    /// <summary>Score, crosshair, ammo, elimination and spawn protection.</summary>
    public class MatchHud : MonoBehaviour
    {
        Text _red;
        Text _blue;
        Text _you;
        Text _ammo;
        Text _hint;
        Text _status;
        Text _hit;
        Image _dot;
        GameObject _death;
        float _hitUntil;

        public static MatchHud Create()
        {
            var canvas = UiKit.Canvas("HUD", 10);
            var hud = canvas.gameObject.AddComponent<MatchHud>();
            hud.Build(canvas);
            hud.gameObject.SetActive(false);
            return hud;
        }

        void OnEnable()
        {
            EngineEvents.Subscribe<PaintballElimination>(OnElimination);
        }

        void OnDisable()
        {
            EngineEvents.Unsubscribe<PaintballElimination>(OnElimination);
        }

        public void Tick(MatchDirector match)
        {
            if (match == null)
            {
                return;
            }

            _red.text = match.Score.Red.ToString();
            _blue.text = match.Score.Blue.ToString();
            var player = match.Player;
            if (player == null)
            {
                return;
            }

            _you.rectTransform.anchoredPosition = new Vector2(player.Team == TeamId.Red ? -150f : 150f, -28f);
            _dot.color = TeamColors.Of(player.Team);
            var marker = player.GetComponent<IMarker>();
            if (marker != null)
            {
                _ammo.text = marker.IsReloading ? "Перезарядка" : "Кулі  " + marker.Balls;
            }

            var showHint = match.IsRunning && Time.time - match.StartedAt < 8f;
            _hint.gameObject.SetActive(showHint);

            var dead = match.IsRunning && !player.IsAlive;
            _death.SetActive(dead && !match.Score.Winner.HasValue);
            if (dead)
            {
                var left = Mathf.Max(0f, player.RespawnAt - Time.time);
                _status.text = "Ви вибули\nВідродження за " + Mathf.CeilToInt(left);
            }
            else if (player.IsInvulnerable && match.IsRunning)
            {
                _status.text = "Захист";
            }
            else
            {
                _status.text = "";
            }

            _hit.gameObject.SetActive(Time.unscaledTime < _hitUntil);
        }

        void OnElimination(PaintballElimination elimination)
        {
            if (!elimination.KillerIsPlayer)
            {
                return;
            }

            _hitUntil = Time.unscaledTime + 1.1f;
            _hit.color = TeamColors.Of(elimination.KillerTeam);
        }

        void Build(RectTransform canvas)
        {
            var bar = UiKit.Rect(canvas, "Score", new Vector2(0.5f, 1f), new Vector2(520f, 64f), new Vector2(0f, -48f));
            var background = bar.gameObject.AddComponent<Image>();
            background.color = new Color(0.04f, 0.05f, 0.07f, 0.55f);
            UiKit.Label(bar, "Red name", "ЧЕРВОНІ", 18, TeamColors.Red, new Vector2(0.5f, 0.5f), new Vector2(140f, 24f), new Vector2(-190f, 14f));
            _red = UiKit.Label(bar, "Red score", "0", 36, TeamColors.Red, new Vector2(0.5f, 0.5f), new Vector2(80f, 48f), new Vector2(-150f, -6f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Label(bar, "Colon", ":", 36, Color.white, new Vector2(0.5f, 0.5f), new Vector2(40f, 48f), Vector2.zero, TextAnchor.MiddleCenter, FontStyle.Bold);
            _blue = UiKit.Label(bar, "Blue score", "0", 36, TeamColors.Blue, new Vector2(0.5f, 0.5f), new Vector2(80f, 48f), new Vector2(150f, -6f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Label(bar, "Blue name", "СИНІ", 18, TeamColors.Blue, new Vector2(0.5f, 0.5f), new Vector2(140f, 24f), new Vector2(190f, 14f));
            _you = UiKit.Label(bar, "You", "ви", 14, new Color(1f, 0.92f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(60f, 20f), new Vector2(-150f, -28f));

            Crosshair(canvas);
            _ammo = UiKit.Label(canvas, "Ammo", "Кулі  150", 26, Color.white, new Vector2(1f, 0f), new Vector2(280f, 40f), new Vector2(-170f, 48f), TextAnchor.MiddleRight, FontStyle.Bold);
            _hint = UiKit.Label(canvas, "Hint", "ЛКМ — постріл    V — вид    Esc — пауза", 20, new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0f), new Vector2(800f, 32f), new Vector2(0f, 36f));
            _status = UiKit.Label(canvas, "Status", "", 32, Color.white, new Vector2(0.5f, 0.5f), new Vector2(700f, 120f), new Vector2(0f, -40f), TextAnchor.MiddleCenter, FontStyle.Bold);
            _hit = UiKit.Label(canvas, "Hit", "Влучання", 28, TeamColors.Red, new Vector2(0.5f, 0.5f), new Vector2(400f, 40f), new Vector2(0f, 70f), TextAnchor.MiddleCenter, FontStyle.Bold);
            _hit.gameObject.SetActive(false);

            var death = UiKit.Stretch(canvas, "Death");
            var veil = death.gameObject.AddComponent<Image>();
            veil.color = new Color(0.25f, 0.02f, 0.03f, 0.28f);
            veil.raycastTarget = false;
            _death = death.gameObject;
            _death.SetActive(false);
        }

        void Crosshair(Transform parent)
        {
            var root = UiKit.Rect(parent, "Crosshair", new Vector2(0.5f, 0.5f), new Vector2(48f, 48f), Vector2.zero);
            Hair(root, new Vector2(2f, 10f), new Vector2(0f, 10f));
            Hair(root, new Vector2(2f, 10f), new Vector2(0f, -10f));
            Hair(root, new Vector2(10f, 2f), new Vector2(10f, 0f));
            Hair(root, new Vector2(10f, 2f), new Vector2(-10f, 0f));
            var dot = UiKit.Rect(root, "Dot", new Vector2(0.5f, 0.5f), new Vector2(4f, 4f), Vector2.zero);
            _dot = dot.gameObject.AddComponent<Image>();
            _dot.color = Color.white;
            _dot.raycastTarget = false;
        }

        static void Hair(Transform parent, Vector2 size, Vector2 position)
        {
            var rect = UiKit.Rect(parent, "Hair", new Vector2(0.5f, 0.5f), size, position);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.9f);
            image.raycastTarget = false;
        }
    }
}
