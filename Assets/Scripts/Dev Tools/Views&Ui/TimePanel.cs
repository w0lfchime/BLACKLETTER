using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameLogic; 

namespace BLDebug
{
    public sealed class TimePanel : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] TMP_Text tickHzText;
        [SerializeField] TMP_Text timeScaleText;
        [SerializeField] TMP_Text elapsedText;
        [SerializeField] TMP_Text tickText;

        [SerializeField] Slider timeScaleSlider;

        [SerializeField] Button playButton;
        [SerializeField] Button pauseButton;
        [SerializeField] Button stepButton;

        [Header("Display")]
        [Tooltip("How often (seconds) to refresh the UI text + button interactables.")]
        [Min(0.02f)]
        public float displayUpdateRate = 0.10f;

        [Header("Time Scale Slider")]
        [Min(0.1f)] public float sliderMin = 0.1f;
        [Min(0.1f)] public float sliderMax = 10f;

        float _nextUiUpdateTime;
        bool _suppressSliderCallback;

        void Awake()
        {
            if (timeScaleSlider)
            {
                timeScaleSlider.minValue = sliderMin;
                timeScaleSlider.maxValue = sliderMax;
            }
        }

        void OnEnable()
        {
            if (timeScaleSlider) timeScaleSlider.onValueChanged.AddListener(OnTimeScaleSliderChanged);
            if (playButton) playButton.onClick.AddListener(OnPlayClicked);
            if (pauseButton) pauseButton.onClick.AddListener(OnPauseClicked);
            if (stepButton) stepButton.onClick.AddListener(OnStepClicked);

            ForceUIRefresh();
        }

        void OnDisable()
        {
            if (timeScaleSlider) timeScaleSlider.onValueChanged.RemoveListener(OnTimeScaleSliderChanged);
            if (playButton) playButton.onClick.RemoveListener(OnPlayClicked);
            if (pauseButton) pauseButton.onClick.RemoveListener(OnPauseClicked);
            if (stepButton) stepButton.onClick.RemoveListener(OnStepClicked);
        }

        void Update()
        {
            if (displayUpdateRate <= 0f) displayUpdateRate = 0.02f;

            if (Time.unscaledTime >= _nextUiUpdateTime)
            {
                _nextUiUpdateTime = Time.unscaledTime + displayUpdateRate;
                RefreshUI();
            }
        }

        void ForceUIRefresh()
        {
            _nextUiUpdateTime = 0f;
            RefreshUI();
        }

        void RefreshUI()
        {
            var c = Clock.I;

            if (!c)
            {
                SetTextSafe(tickHzText, "tick hz: (no clock)");
                SetTextSafe(timeScaleText, "t scale: -");
                SetTextSafe(elapsedText, "Elapsed: -");
                SetTextSafe(tickText, "Tick: -");

                SetButtonSafe(playButton, false, false);
                SetButtonSafe(pauseButton, false, false);
                SetButtonSafe(stepButton, false, false);

                if (timeScaleSlider) timeScaleSlider.interactable = false;
                return;
            }

            SetTextSafe(tickHzText, $"tick hz: {c.tickRate}");
            SetTextSafe(timeScaleText, $"t scale: {c.timeScale:0.###}");
            SetTextSafe(elapsedText, $"Elapsed: {c.time:0.###}");
            SetTextSafe(tickText, $"Tick: {c.tick}");

            bool isPaused = c.paused;

            // Button rules:
            // - If paused: Pause disabled, Play enabled, Step enabled
            // - If playing: Play disabled, Pause enabled, Step disabled
            SetButtonSafe(playButton, true, isPaused);
            SetButtonSafe(pauseButton, true, !isPaused);
            SetButtonSafe(stepButton, true, isPaused);

            // Slider controls timeScale always (even paused), but reflect current value without feedback loop.
            if (timeScaleSlider)
            {
                timeScaleSlider.interactable = true;
                if (!Mathf.Approximately(timeScaleSlider.value, c.timeScale))
                {
                    _suppressSliderCallback = true;
                    timeScaleSlider.value = Mathf.Clamp(c.timeScale, timeScaleSlider.minValue, timeScaleSlider.maxValue);
                    _suppressSliderCallback = false;
                }
            }
        }

        void OnTimeScaleSliderChanged(float value)
        {
            if (_suppressSliderCallback) return;
            var c = Clock.I;
            if (!c) return;

            c.timeScale = Mathf.Clamp(value, 0.1f, 10f);
            // Optional immediate refresh so the text updates instantly even with a slower update rate.
            RefreshUI();
        }

        void OnPlayClicked()
        {
            var c = Clock.I;
            if (!c) return;
            c.paused = false;
            RefreshUI();
        }

        void OnPauseClicked()
        {
            var c = Clock.I;
            if (!c) return;
            c.paused = true;
            RefreshUI();
        }

        void OnStepClicked()
        {
            var c = Clock.I;
            if (!c) return;

            // Only step when paused, as requested.
            c.paused = true;

            // Requires Clock.Step() to be callable. Easiest: add a public wrapper on Clock:
            // public static void StepOnce() { if (I) I.Step(); }
            Clock.StepOnce();

            RefreshUI();
        }

        static void SetTextSafe(TMP_Text t, string s)
        {
            if (t) t.text = s;
        }

        static void SetButtonSafe(Button b, bool enabled, bool interactable)
        {
            if (!b) return;
            b.gameObject.SetActive(enabled);
            b.interactable = interactable;
        }
    }
}