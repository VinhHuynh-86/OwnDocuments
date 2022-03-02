using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace GraphicProfiler
{
    public enum QualityLevel
    {
        VERY_LOW = 0,
        LOW = 1,
        MEDIUM,
        HIGH,
        ULTRA,
        COUNT
    };

    public enum GameOption
    {
        FPS_LEVEL = 0,
        UNITY_QUALITY_LEVEL = 1,
        REFERENCE_DPI_PHONE = 10,
        REFERENCE_DPI_TABLET = 11,
        COUNT
    }

    public static class GraphicsProfiler
    {
        public static bool IsInitialized { get { return isInitialized; } }
        public static System.Action ON_GRAPHIC_PROFILE_CHANGED;
        private static bool isInitialized = false;

        private static int[] gameOptions = new int[(int)GameOption.COUNT];
        private static Dictionary<GameOption, int> _defaultGameOption;
        private static GraphicsProfilerProfile _currentGraphicProfile;
        private static QualityLevel _qualityLevel = QualityLevel.MEDIUM;

        private static GraphicsProfilerConfig profilerConfig;
        public static string GameOptionLog = string.Empty;

        public static StringBuilder _hardwareInfoSB;
        static float _defaultDPI = 0;
        static float _defaultWidth = 0;
        static float _defaultHeight = 0;
        static int _defaultRefreshRate = 0;

        public static QualityLevel GetQualityLevel()
        {
            if (!isInitialized)
                Init();

            return _qualityLevel;
        }

        public static int GetGameOption(GameOption option)
        {
            if (!isInitialized)
                Init();

            return GetOptionValue(option);
        }

        public static void Init(GraphicsProfilerConfig profilerConfigObject = null)
        {
            if(isInitialized) return;

            UpdateHardwareInfo();

            isInitialized = true;
            // LogDeviceInfo();

            if (profilerConfigObject != null)
            {
                profilerConfig = profilerConfigObject;
                InitGraphicsProfile();
            }

            switch(gameoptions.GameOptions.GetQualityLevel())
            {
                case Define.QUALITIES_LEVEL.HIGH:
                case Define.QUALITIES_LEVEL.VERY_HIGH:
                case Define.QUALITIES_LEVEL.ULTRA:
                    UpdateDPIScaling(220,320);
                    break;
                default:
                    UpdateDPIScaling(192,264);
                    break;
            }
        }

        private static void InitGraphicsProfile()
        {
            SetDefaultGameOptions();

            int cpuCores = SystemInfo.processorCount;
            int cpuFrequency = SystemInfo.processorFrequency;
            int systemMemory = SystemInfo.systemMemorySize;
            string gpuName = SystemInfo.graphicsDeviceName;
            string deviceModel = SystemInfo.deviceModel;

            GraphicProfiler.GraphicsProfiler.UpdateGraphicProfileSetting(GraphicProfiler.QualityLevel.ULTRA);
        }

        private static void SetProfile(GraphicsProfilerProfile profile)
        {
            _qualityLevel = profile.qualityLevel;
            _currentGraphicProfile = profile;
        }

        private static int GetOptionValue(GameOption optionID)
        {
            GameOptionValue optionValue;
            return _currentGraphicProfile.GetOptionValue(optionID, out optionValue) ? optionValue.value : _defaultGameOption[optionID];
        }

        public static void IncreaseGraphicProfileQuality()
        {
            if (!isInitialized)
                Init();

            int newQualityLevel = (int)_qualityLevel + 1;
            newQualityLevel = Mathf.Min(newQualityLevel, (int)QualityLevel.COUNT - 1);

            UpdateGraphicProfileSetting((QualityLevel)newQualityLevel);
        }

        public static void DecreaseGraphicProfileQuality()
        {
            if (!isInitialized)
                Init();

            int newQualityLevel = (int)_qualityLevel - 1;
            newQualityLevel = Mathf.Max(newQualityLevel, 0);

            UpdateGraphicProfileSetting((QualityLevel)newQualityLevel);
        }

        public static void UpdateGraphicProfileSetting(QualityLevel level)
        {
            if (!isInitialized)
                Init();

            GraphicsProfilerProfile profile = profilerConfig.GetProfile(level);
            SetProfile(profile);

            //FPS
            Application.targetFrameRate = GetOptionValue(GameOption.FPS_LEVEL);
            QualitySettings.vSyncCount = 0;

            //DPI scaling
            int tabletDPI = GetOptionValue(GameOption.REFERENCE_DPI_TABLET);
            int phoneDPI = GetOptionValue(GameOption.REFERENCE_DPI_PHONE);
            UpdateDPIScaling(tabletDPI, phoneDPI);

            //Update Unity Quality Setting
            QualitySettings.SetQualityLevel(GetGameOption(GameOption.UNITY_QUALITY_LEVEL), true);
            //
            GameOptionLog = string.Empty;

            for (int i = 0; i < (int)GameOption.COUNT; i++)
            {
                GameOption option = (GameOption)i;
                GameOptionValue optionVal;
                int test;
                if (!_currentGraphicProfile.GetOptionValue(option, out optionVal) && !_defaultGameOption.TryGetValue(option, out test)) continue;
                string logLine = option.ToString() + " : " + GetOptionValue(option) + "\n";
                GameOptionLog += logLine;
            }

            //GameOptionLog += string.Format("{0} x {1}", Screen.currentResolution.width, Screen.currentResolution.height) + "\n";
            //ON_GRAPHIC_PROFILE_CHANGED.Raise();
        }
        private static void LogDeviceInfo()
        {
            Debug.Log("SystemInfo.deviceName = " + SystemInfo.deviceName);
            Debug.Log("SystemInfo.deviceModel = " + SystemInfo.deviceModel);
            Debug.Log("SystemInfo.processorType = " + SystemInfo.processorType);
            Debug.Log("SystemInfo.processorCount = " + SystemInfo.processorCount);
            Debug.Log("SystemInfo.processorFrequency = " + SystemInfo.processorFrequency);
            Debug.Log("SystemInfo.systemMemorySize = " + SystemInfo.systemMemorySize);
            Debug.Log("SystemInfo.graphicsDeviceName = " + SystemInfo.graphicsDeviceName);
            Debug.Log("SystemInfo.graphicsMemorySize = " + SystemInfo.graphicsMemorySize);
            Debug.Log("SystemInfo.supportsAccelerometer = " + SystemInfo.supportsAccelerometer);
            Debug.Log("SystemInfo.supportsGyroscope = " + SystemInfo.supportsGyroscope);
        }

        //
        static void UpdateDPIScaling(int tabletDPI, int phoneDPI)
        {
            DisplayDevice.Instance.ResetToDefaultScreenScale();
            float deviceDpi = DisplayDevice.Instance.GetDPI();

            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);

            bool isTablet = (diagonalInches > 6.5f);

            float referenceDPI = (float)(isTablet ? tabletDPI : phoneDPI);
            float deviceScaling = (float)(referenceDPI / deviceDpi);

            if (deviceScaling < 1.0f)
            {
                DisplayDevice.Instance.SetScreenScale(deviceScaling);
            }
        }

        static void SetDefaultGameOptions()
        {
            _defaultGameOption = new Dictionary<GameOption, int>();
            _defaultGameOption.Add(GameOption.FPS_LEVEL, 30);
            _defaultGameOption.Add(GameOption.UNITY_QUALITY_LEVEL, 3);
            _defaultGameOption.Add(GameOption.REFERENCE_DPI_PHONE, 196);
            _defaultGameOption.Add(GameOption.REFERENCE_DPI_TABLET, 132);
        }

        public static void SetDPIScale(float dpiScale)
        {
            if (!isInitialized)
                Init();

            QualitySettings.resolutionScalingFixedDPIFactor = dpiScale;
        }

        public static void SetDPI(int dpi)
        {
            if (!isInitialized)
                Init();

            float screenScale = (float)dpi / _defaultDPI;
            //SetDPIScale(screenScale);
            //DisplayDevice.Instance.SetScreenScale(screenScale);
            if(screenScale > 1.0f) return;

            Screen.SetResolution((int)(_defaultWidth * screenScale), (int)(_defaultHeight * screenScale), Screen.fullScreen, _defaultRefreshRate);
        }

        public static float GetCurrentDPI()
        {
            return Screen.dpi * (Screen.width/_defaultWidth);
        }

        static void UpdateHardwareInfo()
        {
            _hardwareInfoSB = new StringBuilder();

            _hardwareInfoSB.AppendFormat("SystemInfo.deviceName : {0}\n", SystemInfo.deviceName);
            _hardwareInfoSB.AppendFormat("SystemInfo.deviceModel : {0}\n", SystemInfo.deviceModel);
            #if UNITY_IOS
            _hardwareInfoSB.AppendFormat("Device.generation : {0}\n", UnityEngine.iOS.Device.generation);
            #endif
            _hardwareInfoSB.AppendFormat("SystemInfo.processorCount : {0}\n", SystemInfo.processorCount);
            _hardwareInfoSB.AppendFormat("SystemInfo.processorFrequency : {0}\n", SystemInfo.processorFrequency);
            _hardwareInfoSB.AppendFormat("SystemInfo.processorType : {0}\n", SystemInfo.processorType);
            _hardwareInfoSB.AppendFormat("SystemInfo.systemMemorySize : {0}\n", SystemInfo.systemMemorySize);
            _hardwareInfoSB.AppendFormat("SystemInfo.graphicsDeviceName : {0}\n", SystemInfo.graphicsDeviceName);
            _hardwareInfoSB.AppendFormat("SystemInfo.graphicsMemorySize : {0}\n", SystemInfo.graphicsMemorySize);
            _hardwareInfoSB.AppendFormat("SystemInfo.graphicsDeviceVersion : {0}\n", SystemInfo.graphicsDeviceVersion);
            _hardwareInfoSB.AppendFormat("Screen width x height : {0} x {1}\n", Screen.width, Screen.height);
            _hardwareInfoSB.AppendFormat("Screen dpi : {0}\n", Screen.dpi);
            //_hardwareInfoSB.AppendFormat(" : {0}\n", );

            _defaultDPI = Screen.dpi;
            _defaultWidth = Screen.width;
            _defaultHeight = Screen.height;
            _defaultRefreshRate = Screen.currentResolution.refreshRate;
        }

        static public string GetHardwareInfo()
        {
            if (!isInitialized)
                Init();

            return _hardwareInfoSB.ToString();
        }

        static public void ChooseGraphicProfile(int benchmarkPoint)
        {
            GraphicsProfilerProfile chooseConfig = profilerConfig.graphicsProfileList[0];
            for(int i = 0; i < profilerConfig.graphicsProfileList.Count ; i++)
            {
                var config = profilerConfig.graphicsProfileList[i];
                if(benchmarkPoint >= config.benchmarkPoint)
                {
                    chooseConfig = config;
                }
            }

            string gpuName = SystemInfo.graphicsDeviceName.ToLower();
            bool forceLow = false;
            bool forceVeryLow = false;

            QualityLevel forceGraphicProfile = QualityLevel.COUNT;
            
            if(gpuName.Contains("adreno"))
            {
                string[] adrenoLowBlacklist = new string[]{
                    "304",
                    "305",
                    "306",
                    "308",
                };

                for(int i = 0; i < adrenoLowBlacklist.Length; i++)
                {
                    if(gpuName.Contains(adrenoLowBlacklist[i]))
                    {
                        forceLow = true;
                        break;
                    }
                }

            } 
            
        #if UNITY_ANDROID
            string graphicVersion = SystemInfo.graphicsDeviceVersion.ToLower();
            bool isOpenGL20 = graphicVersion.Contains("es 2.0");
            if(isOpenGL20)
                forceLow = true;
        #endif

            if(!forceVeryLow)
                forceVeryLow = SystemInfo.graphicsMemorySize < 300;

            bool useForceGraphicProfile = forceLow || forceVeryLow;
            if(useForceGraphicProfile)
            {
                forceGraphicProfile = forceVeryLow ? QualityLevel.VERY_LOW : QualityLevel.LOW;
            } 

            QualityLevel choosenLevel = chooseConfig.qualityLevel; 

            if(useForceGraphicProfile)
            {
                if((int)forceGraphicProfile < (int)chooseConfig.qualityLevel)
                    choosenLevel = forceGraphicProfile;
            }
            UpdateGraphicProfileSetting(choosenLevel);
        }

        public static string[] iosLowDevices = {
            "A4",
            "A5",
            "A5X",
            "A6",
            "A6X",
            "A7",
        };

        public static string[] iosMediumDevices = {
            "A8",
        };

        public static string[] iosHighDevices = {
            "A9",
        };

        public static string[] iosUltraDevices = {
            "A10",
            "A10X",
            "A11",
            "A12",
            "A12X",
            "A12Z",
            "A13",
            "A14",
        };

        static public void ChooseGraphicProfile(string iosDeviceModel)
        {
            if (!isInitialized)
                Init();
                
            GraphicsProfilerProfile profile = profilerConfig.GetProfile(QualityLevel.ULTRA);
            
            for(int i = 0; i < iosLowDevices.Length; i++)
            {
                if(iosDeviceModel.Contains(iosLowDevices[i]))
                {
                    profile = profilerConfig.GetProfile(QualityLevel.LOW);

                    break;
                }
            }

            for(int i = 0; i < iosMediumDevices.Length; i++)
            {
                if(iosDeviceModel.Contains(iosMediumDevices[i]))
                {
                    profile = profilerConfig.GetProfile(QualityLevel.MEDIUM);

                    break;
                }
            }

            for(int i = 0; i < iosHighDevices.Length; i++)
            {
                if(iosDeviceModel.Contains(iosHighDevices[i]))
                {
                    profile = profilerConfig.GetProfile(QualityLevel.HIGH);

                    break;
                }
            }

            UpdateGraphicProfileSetting(profile.qualityLevel);
        }
    }
}