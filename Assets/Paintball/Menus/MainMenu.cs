using System;
using ITKamianets.Paintball.Core;
using ITKamianets.Paintball.Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ITKamianets.Paintball.Menus
{
    public class MainMenu : MonoBehaviour
    {
        Button _red;

        public event Action<TeamId> TeamChosen;

        public static MainMenu Create()
        {
            var canvas = UiKit.Canvas("Main Menu", 20);
            var menu = canvas.gameObject.AddComponent<MainMenu>();
            menu.Build(canvas);
            return menu;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (EventSystem.current != null && _red != null)
            {
                EventSystem.current.SetSelectedGameObject(_red.gameObject);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void Build(RectTransform canvas)
        {
            UiKit.Panel(canvas, new Color(0.04f, 0.05f, 0.07f, 0.72f));
            UiKit.Label(canvas, "Title", "KP Paintball Arena", 64, Color.white, new Vector2(0.5f, 0.5f), new Vector2(1200f, 80f), new Vector2(0f, 250f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Label(canvas, "Subtitle", "Пейнтбол у Кам’янці-Подільському", 28, new Color(0.95f, 0.86f, 0.62f), new Vector2(0.5f, 0.5f), new Vector2(900f, 40f), new Vector2(0f, 185f));
            UiKit.Label(
                canvas,
                "Body",
                "Обери команду. Ти і два напарники проти трьох ботів.\nОдне влучання фарбою — суперник вибуває і відроджується.\nПерша команда до " + MatchRules.ScoreLimit + " очок перемагає.",
                24,
                new Color(0.9f, 0.92f, 0.94f),
                new Vector2(0.5f, 0.5f),
                new Vector2(980f, 140f),
                new Vector2(0f, 70f));
            _red = UiKit.Button(canvas, "Грати за червоних", TeamColors.Red, new Vector2(-230f, -90f), new Vector2(380f, 72f), () => TeamChosen?.Invoke(TeamId.Red));
            UiKit.Button(canvas, "Грати за синіх", TeamColors.Blue, new Vector2(230f, -90f), new Vector2(380f, 72f), () => TeamChosen?.Invoke(TeamId.Blue));
            UiKit.Label(canvas, "Controls", "WASD — рух    миша — огляд    ЛКМ — постріл    Shift — біг    Ctrl — присід    Пробіл — стрибок    V — камера", 20, new Color(0.75f, 0.78f, 0.82f), new Vector2(0.5f, 0.5f), new Vector2(1400f, 60f), new Vector2(0f, -230f));
        }
    }
}
