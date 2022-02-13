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
using OCanada.GameplaySetupScene;
using OCanada.Flag;
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
        public event Action<Mode, int> GameExit;
        private string _userName;
        private IPlatformUserModel platformUserModel;

        private AudioPlayer audioPlayer;
        private string[] oCanadaNotes;
        private int currentNote;

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
                if (value < 0)
                {
                    ExitGame();
                }
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
                _userName = value.Length < 3 ? value.ToUpper() : value.Substring(0, 3).ToUpper();
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

                if (selectedMode == Mode.Endless)
                {
                    return $"High Score: {PluginConfig.Instance.HighScoreEndless}";
                }

                return "uh oh";
            }
        }
        
        [Inject]
        public void Construct(GameplaySetupViewController gameplaySetupViewController, OCanadaPauseMenuController oCanadaPauseMenuController, IPlatformUserModel platformUserModel, AudioPlayer audioPlayer)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            this.oCanadaPauseMenuController = oCanadaPauseMenuController;
            this.platformUserModel = platformUserModel;
            this.audioPlayer = audioPlayer;
            oCanadaNotes = Notes.OCanadaNotes;
            currentNote = 0;
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
            int imageIndex = random.Next(imageList.Count);
            
            int flagIndex = 0; // Def to 0 because this is where Canadian flag is
            int canadianFlag = random.Next(1, 6);
            if (canadianFlag != 5) // 1/5 chance of Canadian Flag spawn
            {
                flagIndex = random.Next(Flags.Points.Count);
            }

            var (name, pointValue) = Flags.Points.ElementAt(flagIndex);
            var flagSprite = FlagLoader.GetSprite(name);

            imageList[imageIndex].SetImage(flagSprite, pointValue);
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
            if (Score < 0)
            {
                Score = 0;
                ExitGame();
            }

            if (pointValue > 0)
            {
                audioPlayer.PlayNote(oCanadaNotes[currentNote % oCanadaNotes.Length]);
                currentNote++;
            }

            if (selectedMode == Mode.Endless)
            {
                Timer += Mathf.CeilToInt(pointValue / 2);
                if (Timer > 20)
                {
                    Timer = 20;
                }
            }

            bool respawn = true;
            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                if (clickableFlag.PointValue > 0)
                {
                    respawn = false;
                    break;
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
            if (rootTransform != null && gameObject != null)
            {
                rootTransform.gameObject.SetActive(false);
                gameObject.SetActive(false);

                foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
                {
                    if (clickableFlag != null)
                    {
                        clickableFlag.FlagClickedEvent -= ClickableFlag_FlagClickedEvent;
                    }
                }

                if (selectedMode == Mode.Standard)
                {
                    PluginConfig.Instance.HighScoreStandard = Score > PluginConfig.Instance.HighScoreStandard ? Score : PluginConfig.Instance.HighScoreStandard;
                }
                else if (selectedMode == Mode.Endless)
                {
                    PluginConfig.Instance.HighScoreEndless = Score > PluginConfig.Instance.HighScoreEndless ? Score : PluginConfig.Instance.HighScoreEndless;
                }

                currentNote = 0;
                GameExit?.Invoke(selectedMode, Score);
            }
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
        internal event Action<int> FlagClickedEvent;

        internal int PointValue;

        internal void SetImage(Sprite flagImage, int pointValue = 0)
        {
            PointValue = pointValue;
            if (flagImage == null)
            {
                clickableImage.sprite = Utilities.ImageResources.BlankSprite;
                return;
            }
            
            clickableImage.sprite = flagImage;
            clickableImage.HighlightColor = PointValue >= 5
                ? Color.yellow
                : Color.red;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            FieldAccessor<ImageView, float>.Set(clickableImage, "_skew", 0);
        }

        [UIAction("flag-clicked")]
        private void FlagClicked()
        {
            int pointValue = PointValue;
            PointValue = 0;
            clickableImage.sprite = Utilities.ImageResources.BlankSprite;
            FlagClickedEvent?.Invoke(pointValue);
        }
    }
}
