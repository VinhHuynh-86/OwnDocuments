using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

//namespace Kinder.UI
//{
    public class GodsEye : MonoBehaviour
    {
        public enum FilterType { All, Log, Warning, Error, Count };
        [HideInInspector]
        public string output = "";
        [HideInInspector]
        public string stack = "";

        [HideInInspector]
        public bool isActive = false;

        public TMP_Text logText = null;
        public Transform scrollView = null;
        public Scrollbar scrollBar = null;

        private List<string> logs = new List<string>();

        private FilterType filterType = FilterType.All;

        private bool isFilterTitle = true;
        private bool isFilterValue = true;
        private string tagFilter = "";
        private readonly string MARK_FILTER_TYPE_ERROR = "TYPE+ERROR";
        private readonly string MARK_FILTER_TYPE_LOG = "TYPE+LOG";
        private readonly string MARK_FILTER_TYPE_WARNING = "TYPE+WARNING";

        private int buffer = 300;

        private void Start()
        {
            this.SetActive(false);
        }

#if ENABLE_CHEAT
        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }
#endif

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            output = logString;
            stack = stackTrace;

            // add mark log
            string markLogType = GetMarkLogType(type);
            // store log
            logs.Add(output + markLogType);
            if (logs.Count > buffer)
            {
                List<string> tmpLogs = new List<string>();
                for (int i = 0; i < buffer; i++)
                    tmpLogs.Add(logs[(logs.Count - 1) - i]);

                logs = tmpLogs;
            }

            if (isActive)
                ShowLog();
        }

        private void ShowLog()
        {
            string resultLog = "";
            for (int i = 0; i < logs.Count; i++)
            {
                string logValue = logs[i];

                // filter empty string
                if (logValue.Length == 0)
                    continue;

                // filter log type
                FilterType curLogType = GetFilterType(logValue);
                if (!FilterByLogType(curLogType))
                    continue;

                // remove mark log
                logValue = RemoveMarkLogType(curLogType, logValue);
                // add highlight for log type
                logValue = AddHighlightLogType(curLogType, logValue);

                // filter tag
                if (tagFilter.Length > 0)
                {
                    if (logValue.Contains(tagFilter) || logValue.ToLower().Contains(tagFilter.ToLower()))
                        resultLog += HighlightTagFilter(logValue);
                    else
                        continue;
                }
                else
                {
                    resultLog += logValue;
                }

                //string logTitle = "";

                //// filter tag and value of log
                //int fstQuareMark = logValue.IndexOf('[');
                //int lastQuareMark = logValue.IndexOf(']');
                //if (fstQuareMark != -1 && lastQuareMark != -1)
                //{
                //    logTitle = logValue.Substring(0, lastQuareMark - fstQuareMark + 1);
                //    logValue = logValue.Substring(lastQuareMark + 1);
                //}

                //// add log
                //if (isFilterTitle)
                //    resultLog += logTitle;
                //if (isFilterValue)
                //    resultLog += logValue.Replace('\n', ' ');

                // breakline
                resultLog += "\n\n";
            }

            logText.text = resultLog;

            scrollView.gameObject.SetActive(false);
            scrollView.gameObject.SetActive(true);
            scrollBar.value = 0;
        }

        private FilterType GetFilterType(string val)
        {
            for (int i = 0; i < (int)FilterType.Count; i++)
            {
                FilterType checkingType = (FilterType)i;
                string markLogType = GetMarkLogType(checkingType);
                if (markLogType.Length == 0)
                    continue;

                if (val.Contains(markLogType))
                    return checkingType;
            }

            return FilterType.All;
        }

        private bool FilterByLogType(FilterType checkType)
        {
            if (filterType == FilterType.Warning && checkType != FilterType.Warning)
                return false;
            if (filterType == FilterType.Error && checkType != FilterType.Error)
                return false;
            if (filterType == FilterType.Log)
            {
                if (checkType == FilterType.Warning || checkType == FilterType.Error)
                    return false;
            }

            return true;
        }

        private string AddHighlightLogType(FilterType type, string val)
        {
            string highlightTxt = val;
            switch (type)
            {
                case FilterType.All:
                    break;
                case FilterType.Warning:
                    highlightTxt = AddColorTag(Color.yellow, "Warning: " + highlightTxt);
                    break;
                case FilterType.Error:
                    highlightTxt = AddColorTag(Color.red, "Error: " + highlightTxt);
                    break;
                case FilterType.Log:
                    break;
                case FilterType.Count:
                    break;
                default:
                    break;
            }

            return highlightTxt;
        }

        private string HighlightTagFilter(string val)
        {
            if (tagFilter.Length == 0)
                return val;

            string result = "";
            int tagId = val.ToLower().IndexOf(tagFilter.ToLower());

            string fwdPart = val.Substring(0, tagId);
            string afterPart = val.Substring(tagId + tagFilter.Length);

            result += fwdPart;
            result += AddColorTag(Color.green, val.Substring(tagId, tagFilter.Length));
            result += afterPart;

            return result;
        }

        private string GetMarkLogType(LogType type)
        {
            string markLogType = "";
            switch (type)
            {
                case LogType.Error:
                    markLogType = MARK_FILTER_TYPE_ERROR;
                    break;
                case LogType.Warning:
                    markLogType = MARK_FILTER_TYPE_WARNING;
                    break;
                case LogType.Exception:
                case LogType.Assert:
                case LogType.Log:
                    markLogType = MARK_FILTER_TYPE_LOG;
                    break;
                default:
                    break;
            }
            return markLogType;
        }
        private string GetMarkLogType(FilterType type)
        {
            string markLogType = "";
            switch (type)
            {
                case FilterType.Error:
                    markLogType = MARK_FILTER_TYPE_ERROR;
                    break;
                case FilterType.Warning:
                    markLogType = MARK_FILTER_TYPE_WARNING;
                    break;
                case FilterType.Log:
                    markLogType = MARK_FILTER_TYPE_LOG;
                    break;
                default:
                    break;
            }
            return markLogType;
        }
        private string RemoveMarkLogType(FilterType type, string val)
        {
            string markLogType = GetMarkLogType(type);
            if (markLogType.Length > 0)
                val = val.Replace(markLogType, "");
            return val;
        }

        public void SetActive(bool isActive)
        {
            this.isActive = isActive;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(isActive);
            }
        }
        // BTN Functions
        public void OnAllBtnPress()
        {
            filterType = FilterType.All;

            ShowLog();
        }
        public void OnLogBtnPress()
        {
            filterType = FilterType.Log;

            ShowLog();
        }
        public void OnWarningBtnPress()
        {
            filterType = FilterType.Warning;

            ShowLog();
        }
        public void OnErrorBtnPress()
        {
            filterType = FilterType.Error;

            ShowLog();
        }
        public void OnClearBtnPress()
        {
            logs.Clear();
            logText.text = string.Empty;
        }

        public void OnCloseBtnPress()
        {
            SetActive(false);
        }

        public void OnFilterTitle(bool isActive)
        {
            isFilterTitle = isActive;

            ShowLog();
        }
        public void OnFilterValue(bool isActive)
        {
            isFilterValue = isActive;

            ShowLog();
        }

        public void OnModifiedTagFilter(string tagStr)
        {
            tagFilter = tagStr;

            ShowLog();
        }
        public void OnModifiedBuffer(string val)
        {
            buffer = int.Parse(val);
        }

        // --- color tag
        public static string OpenColorTag(Color _color) { return "<color=#" + ColorUtility.ToHtmlStringRGBA(_color) + ">"; }
        public static string CloseColorTag() { return "</color>"; }
        public static string AddColorTag(Color _color, string _val) { return OpenColorTag(_color) + _val + CloseColorTag(); }
    }
//}