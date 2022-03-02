using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace GraphicProfiler
{
    public class DisplayDevice : Singleton<DisplayDevice>
    {
        public const float CM_TO_INCH = 0.393700787f;
        public const float INCH_TO_CM = 1 / CM_TO_INCH;

        private float dpi;
        private float dotsPerCentimeter;

        private static bool? isLaptop = null;

        private static Resolution defaultResolution;
        public DisplayDevice()
        {
            defaultResolution = new Resolution();
            defaultResolution.width = Screen.currentResolution.width;
            defaultResolution.height = Screen.currentResolution.height;
            defaultResolution.refreshRate = Screen.currentResolution.refreshRate;

            CalculateDPI();
        }

        internal static bool INTERNAL_IsLaptop
        {
            get
            {
                if (isLaptop == null)
                {
                    var gpuName = SystemInfo.graphicsDeviceName.ToLower();
                    var regex = new Regex(@"^(.*mobile.*|intel hd graphics.*|.*m\s*(series)?\s*(opengl engine)?)$", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(gpuName)) isLaptop = true;
                    else isLaptop = false;
                }
                return isLaptop == true;
            }
        }

        private void CalculateDPI()
        {
            string Name = Application.platform.ToString();
            if (INTERNAL_IsLaptop) Name += " (Laptop)";

            dpi = Screen.dpi;
            if (dpi < float.Epsilon)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.LinuxPlayer:
                        {
                            var width = Mathf.Max(Screen.currentResolution.width, Screen.currentResolution.height);
                            var height = Mathf.Min(Screen.currentResolution.width, Screen.currentResolution.height);

                            if (width >= 3840)
                            {
                                if (height <= 2160) dpi = 150; // 28-31"
                                else dpi = 200;
                            }
                            else if (width >= 2880 && height == 1800) dpi = 220; // 15" retina
                            else if (width >= 2560)
                            {
                                if (height >= 1600)
                                {
                                    if (INTERNAL_IsLaptop) dpi = 226; // 13.3" retina
                                    else dpi = 101; // 30" display
                                }
                                else if (height >= 1440) dpi = 109; // 27" iMac
                            }
                            else if (width >= 2048)
                            {
                                if (height <= 1152) dpi = 100; // 23-27"
                                else dpi = 171; // 15" laptop
                            }
                            else if (width >= 1920)
                            {
                                if (height >= 1440) dpi = 110; // 24"
                                else if (height >= 1200) dpi = 90; // 26-27"
                                else if (height >= 1080)
                                {
                                    if (INTERNAL_IsLaptop) dpi = 130; // 15" - 18" laptop
                                    else dpi = 92; // +-24" display
                                }
                            }
                            else if (width >= 1680) dpi = 129; // 15" laptop
                            else if (width >= 1600) dpi = 140; // 13" laptop
                            else if (width >= 1440)
                            {
                                if (height >= 1050) dpi = 125; // 14" laptop
                                else dpi = 110; // 13" air or 15" macbook pro
                            }
                            else if (width >= 1366) dpi = 125; // 10"-14" laptops
                            else if (width >= 1280) dpi = 110;
                            else dpi = 96;
                            break;
                        }
                    case RuntimePlatform.Android:
                        {
                            var width = Mathf.Max(Screen.currentResolution.width, Screen.currentResolution.height);
                            var height = Mathf.Min(Screen.currentResolution.width, Screen.currentResolution.height);
                            if (width >= 1280)
                            {
                                if (height >= 800) dpi = 285; //Galaxy Note
                                else dpi = 312; //Galaxy S3, Xperia S
                            }
                            else if (width >= 1024) dpi = 171; // Galaxy Tab
                            else if (width >= 960) dpi = 256; // Sensation
                            else if (width >= 800) dpi = 240; // Galaxy S2...
                            else dpi = 160;
                            break;
                        }
                    case RuntimePlatform.IPhonePlayer:
                        {
                            var width = Mathf.Max(Screen.currentResolution.width, Screen.currentResolution.height);
                            //                        var height = Mathf.Min(Screen.currentResolution.width, Screen.currentResolution.height);
                            if (width >= 2048) dpi = 290; // iPad4 or ipad2 mini
                            else if (width >= 1136) dpi = 326; // iPhone 5+
                            else if (width >= 1024) dpi = 160; // iPad mini1
                            else if (width >= 960) dpi = 326; // iPhone 4+
                            else dpi = 160;
                            break;
                        }
                    default:
                        dpi = 160;
                        break;
                }
            }

            dotsPerCentimeter = CM_TO_INCH * dpi;
        }

        public float GetDotsPerCentimeter()
        {
            return dotsPerCentimeter;
        }

        public float GetDPI()
        {
            return dpi;
        }

        public void SetScreenScale(float scale)
        {
            //Resolution current = Screen.currentResolution;
            Screen.SetResolution((int)(defaultResolution.width * scale), (int)(defaultResolution.height * scale), Screen.fullScreen, defaultResolution.refreshRate);
            
            dpi *= scale;
            dotsPerCentimeter = CM_TO_INCH * dpi;
        }

        public void ResetToDefaultScreenScale()
        {
            Screen.SetResolution((int)(defaultResolution.width), (int)(defaultResolution.height), Screen.fullScreen, defaultResolution.refreshRate);
            CalculateDPI();
        }
    }
}