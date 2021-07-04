using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using OCanada.Configuration;
using OCanada.UI.ViewControllers;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    internal class OCanadaGameController : MonoBehaviour, IInitializable, IDisposable, INotifyPropertyChanged
    {
        private OCanadaPauseMenuController oCanadaPauseMenuController;

        private bool parsed;
        private Mode selectedMode;

        private int prevUpdate;
        private float currentTime;
        private readonly float SPAWN_TIME = 5000;

        private System.Random random;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action GameExit;
        private string _userName;
        private IPlatformUserModel platformUserModel;

        private int _score;
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
        private string FormattedTimer => "0.001s";

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
        public void Construct(OCanadaPauseMenuController oCanadaPauseMenuController, IPlatformUserModel platformUserModel)
        {
            this.oCanadaPauseMenuController = oCanadaPauseMenuController;
            this.platformUserModel = platformUserModel;
        }

        public void Initialize()
        {
            parsed = false;
            gameObject.SetActive(false);
            selectedMode = Mode.None;

            GetUsername();
            
            score = 0;
            prevUpdate = 1;
            currentTime = SPAWN_TIME;
            random = new System.Random();
            
            oCanadaPauseMenuController.ExitClicked += OCanadaPauseMenuController_ExitClicked;
        }

        public void Dispose()
        {
            oCanadaPauseMenuController.ExitClicked -= OCanadaPauseMenuController_ExitClicked;
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
                gameObject.SetActive(true);
            }
            rootTransform.SetParent(siblingTranform.parent);
            transform.SetParent(rootTransform);
        }

        internal void StartGame(RectTransform siblingTransform, Mode selectedMode)
        {
            Parse(siblingTransform);
            rootTransform.gameObject.SetActive(true);
            this.selectedMode = selectedMode;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighScoreFormatted)));
            Score = 0;
            FlagImage flagImage = new FlagImage("OCanada.Images.Canada.png", 1);
            flagImage.SpriteLoaded += FlagImage_SpriteLoaded;
            _ = flagImage.Sprite;

            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                clickableFlag.FlagClickedEvent += ClickableFlag_FlagClickedEvent;
            }

            SpawnFlags();
        }

        public void Update()
        {
            if (Time.time * 1000 >= prevUpdate)
            {
                var currentUpdate = Mathf.FloorToInt(Time.time * 1000);
                var difference = currentUpdate - prevUpdate; //Calculate difference in time
                prevUpdate = currentUpdate;
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

        private void ExitGame()
        {
            foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
            {
                clickableFlag.FlagClickedEvent -= ClickableFlag_FlagClickedEvent;
            }
        }

        [UIAction("pause-clicked")]
        private void PauseButtonClicked()
        {
            oCanadaPauseMenuController.ShowModal(rootTransform);
            // pause(); u feel
        }

        private void OCanadaPauseMenuController_ExitClicked()
        {
            rootTransform.gameObject.SetActive(false);
            ExitGame();
            GameExit?.Invoke();
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
