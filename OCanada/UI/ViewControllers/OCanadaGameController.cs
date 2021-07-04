using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using OCanada.Configuration;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    internal class OCanadaGameController : MonoBehaviour, IInitializable, IDisposable, INotifyPropertyChanged
    {
        private GameplaySetupViewController gameplaySetupViewController;
        private OCanadaPauseMenuController oCanadaPauseMenuController;

        private bool parsed;
        private Mode selectedMode;

        private int prevUpdate;
        private int currentTime;
        private int currentTimeSeconds;

        private System.Random random;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action GameExit;
        private string _userName;
        private IPlatformUserModel platformUserModel;

        private int _score;
        private int _timer;
        private int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScoreFormatted)));
            }
        }

        private int Timer
        {
            get
            {
                return _timer;
            }
            set
            {
                _timer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerFormatted)));
            }
        }

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIValue("clickable-images")]
        private List<object> clickableImages = Enumerable.Range(1, 24).Select(i =>
        {
            return new ClickableFlag() as object;
        }).ToList();


        [UIValue("username")]
        private string Username
        {
            get => _userName;
            set
            {
                _userName = value.Substring(0, 3).ToUpper();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
            }
        }

        [UIValue("timer")]
        private string TimerFormatted => $"{Timer}s";

        [UIValue("score")]
        private string ScoreFormatted
        {
            get => $"Score: {Score}";
        }

        [UIValue("high-score")]
        private string HighScoreFormatted
        {
            get
            {
                if (selectedMode == Mode.Standard)
                {
                    return $"High Score: {PluginConfig.Instance.HighScoreStandard}";
                }
                else if (selectedMode == Mode.Endless)
                {
                    return $"High Score: {PluginConfig.Instance.HighScoreEndless}";
                }
                else
                {
                    return "uh oh";
                }
            }
        }
        
        [Inject]
        public void Construct(GameplaySetupViewController gameplaySetupViewController, OCanadaPauseMenuController oCanadaPauseMenuController, IPlatformUserModel platformUserModel)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.oCanadaPauseMenuController = oCanadaPauseMenuController;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            parsed = false;
            gameObject.SetActive(false);
            selectedMode = Mode.None;

            GetUsername();
            
            Score = 0;
            prevUpdate = 1;
            currentTime = 0;
            currentTimeSeconds = 0;
            Timer = 0;
            random = new System.Random();

            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
            oCanadaPauseMenuController.ResumeClicked += ResumeGame;
            oCanadaPauseMenuController.ExitClicked += ExitGame;
        }

        public void Dispose()
        {
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
            oCanadaPauseMenuController.ResumeClicked -= ResumeGame;
            oCanadaPauseMenuController.ExitClicked -= ExitGame;
        }

        private async void GetUsername()
        {
            UserInfo user = await platformUserModel.GetUserInfo();
            Username = user.userName;
        }

        private void Parse(RectTransform siblingTranform)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaGame.bsml"), siblingTranform.parent.gameObject, this);
                parsed = true;
            }
            rootTransform.SetParent(siblingTranform.parent);
            transform.SetParent(rootTransform);
        }

        internal void StartGame(RectTransform siblingTransform, Mode selectedMode)
        {
            Parse(siblingTransform);
            rootTransform.gameObject.SetActive(true);
            this.selectedMode = selectedMode;

            Score = 0;
            prevUpdate = Mathf.FloorToInt(Time.time * 1000);
            currentTime = 0;
            currentTimeSeconds = 0;

            if (selectedMode == Mode.Standard)
            {
                Timer = 30;
            }
            else if (selectedMode == Mode.Endless)
            {
                Timer = 10;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerFormatted)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighScoreFormatted)));

            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                clickableFlag.FlagClickedEvent += ClickableFlag_FlagClickedEvent;
            }

            SpawnFlags();
            gameObject.SetActive(true);
        }

        public void Update()
        {
            if (Time.time * 1000 >= prevUpdate)
            {
                var currentUpdate = Mathf.FloorToInt(Time.time * 1000);
                var difference = currentUpdate - prevUpdate; //Calculate difference in time
                prevUpdate = currentUpdate;
                UpdateTime(difference);
            }
        }

        private void UpdateTime(int difference)
        {
            currentTime += difference;
            if (currentTime > (currentTimeSeconds + 1) * 1000)
            {
                currentTimeSeconds++;
                Timer--;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimerFormatted)));
                if (Timer <= 0)
                {
                    ExitGame();
                }
            }
        }

        private void SpawnFlags()
        {
            for (int i = 0; i < 7; i++)
            {
                SpawnFlag();
            }
        }

        private void SpawnFlag()
        {
            List<ClickableFlag> imageList = clickableImages.OfType<ClickableFlag>().ToList();
            int canadianFlag = random.Next(1, 6);
            int imageIndex = random.Next(imageList.Count);
            int flagIndex = 0;
            if (canadianFlag != 5)
            {
                flagIndex = random.Next(Flags.FlagList.Count);
            }
            imageList[imageIndex].SetImage(Flags.FlagList[flagIndex]);
        }

        private void ClearFlags()
        {
            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                clickableFlag.SetImage(null);
            }
        }

        private void ClickableFlag_FlagClickedEvent(int pointValue)
        {
            Score += pointValue;
            bool respawn = true;

            if (selectedMode == Mode.Endless)
            {
                Timer += Mathf.CeilToInt(pointValue / 2);
            }

            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                if (clickableFlag.PointValue > 0)
                {
                    respawn = false;
                }
            }

            if (respawn)
            {
                ClearFlags();
                SpawnFlags();
            }
        }

        [UIAction("pause-clicked")]
        private void PauseGame()
        {
            oCanadaPauseMenuController.ShowModal(rootTransform);
            gameObject.SetActive(false);
        }

        private void ResumeGame()
        {
            prevUpdate = Mathf.FloorToInt(Time.time * 1000);
            gameObject.SetActive(true);
        }

        private void ExitGame()
        {
            rootTransform.gameObject.SetActive(false);
            gameObject.SetActive(false);

            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                clickableFlag.FlagClickedEvent -= ClickableFlag_FlagClickedEvent;
            }
            if (selectedMode == Mode.Standard)
            {
                PluginConfig.Instance.HighScoreStandard = Score > PluginConfig.Instance.HighScoreStandard ? Score : PluginConfig.Instance.HighScoreStandard;
            }
            else if (selectedMode == Mode.Endless)
            {
                PluginConfig.Instance.HighScoreEndless = Score > PluginConfig.Instance.HighScoreEndless ? Score : PluginConfig.Instance.HighScoreEndless;
            }

            GameExit?.Invoke();
        }

        private void GameplaySetupViewController_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            ExitGame();
        }
    }

    internal class ClickableFlag
    {
        [UIComponent("clickable-image")]
        private ClickableImage clickableImage;

        private FlagImage flagImage;
        internal event Action<int> FlagClickedEvent;
        internal int PointValue => flagImage != null ? flagImage.PointValue : 0;

        internal void SetImage(FlagImage flagImage)
        {
            if (this.flagImage != null)
            {
                this.flagImage.SpriteLoaded -= FlagImage_SpriteLoaded;
            }

            if (flagImage != null)
            {
                if (flagImage.SpriteWasLoaded)
                {
                    clickableImage.sprite = flagImage.Sprite;
                    this.flagImage = flagImage;
                }
                else
                {
                    this.flagImage = null;
                    flagImage.SpriteLoaded += FlagImage_SpriteLoaded;
                    _ = flagImage.Sprite;
                }

                if (flagImage.PointValue >= 5)
                {
                    clickableImage.HighlightColor = Color.yellow;
                }
                else
                {
                    clickableImage.HighlightColor = Color.red;
                }
            }
            else
            {
                clickableImage.sprite = Utilities.ImageResources.BlankSprite;
                this.flagImage = null;
            }
        }

        private void FlagImage_SpriteLoaded(object sender, System.EventArgs e)
        {
            if (sender is FlagImage flagImage)
            {
                this.flagImage = flagImage;
                clickableImage.sprite = flagImage.Sprite;
                flagImage.SpriteLoaded -= FlagImage_SpriteLoaded;
            }
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            FieldAccessor<ImageView, float>.Set(clickableImage, "_skew", 0);
        }

        [UIAction("flag-clicked")]
        private void FlagClicked()
        {
            if (flagImage != null)
            {
                int pointValue = PointValue;
                flagImage = null;
                clickableImage.sprite = Utilities.ImageResources.BlankSprite;
                FlagClickedEvent?.Invoke(pointValue);
            }
        }
    }
}
