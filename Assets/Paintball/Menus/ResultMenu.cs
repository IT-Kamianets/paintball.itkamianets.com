using System;
using ITKamianets.Paintball.Core;
using ITKamianets.Paintball.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace ITKamianets.Paintball.Menus
{
    public class ResultMenu : MonoBehaviour
    {
        Text _title;

        public event Action Leave;

        public static ResultMenu Create()
        {
            var canvas = UiKit.Canvas("Result Menu", 40);
            var menu = canvas.gameObject.AddComponent<ResultMenu>();
            menu.Build(canvas);
            menu.gameObject.SetActive(false);
            return menu;
        }

        public void Show(TeamId winner)
        {
            _title.text = TeamColors.Name(winner) + " перемогли";
            _title.color = TeamColors.Of(winner);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void Build(RectTransform canvas)
        {
            UiKit.Panel(canvas, new Color(0.03f, 0.04f, 0.06f, 0.62f));
            _title = UiKit.Label(canvas, "Title", "Перемога", 58, Color.white, new Vector2(0.5f, 0.5f), new Vector2(900f, 80f), new Vector2(0f, 80f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Label(canvas, "Score", "Рахунок до " + MatchRules.ScoreLimit, 24, new Color(0.85f, 0.88f, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(500f, 36f), new Vector2(0f, 20f));
            UiKit.Button(canvas, "До меню", new Color(0.25f, 0.32f, 0.42f), new Vector2(0f, -90f), new Vector2(360f, 68f), () => Leave?.Invoke());
        }
    }
}
