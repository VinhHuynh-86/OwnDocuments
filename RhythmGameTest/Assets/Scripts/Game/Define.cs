using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using OPS.AntiCheat;
using OPS.AntiCheat.Field;

public class StringValueAttribute : Attribute
{
    public string StringValue { get; protected set; }

    public StringValueAttribute(string value)
    {
        this.StringValue = value;
    }
}


public static class Define
{
    public static string GetStringValue(this Enum value)
    {
        Type type = value.GetType();

        FieldInfo fieldInfo = type.GetField(value.ToString());

        StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

        return attributes.Length > 0 ? attributes[0].StringValue : null;
    }

    public static object GetEnumValue(this string value, Type enumType, bool ignoreCase)
    {
        object result = null;
        string enumStringValue = null;
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("enumType should be a valid enum");
        }

        foreach (FieldInfo fieldInfo in enumType.GetFields())
        {
            var attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            if (attributes.Length > 0)
            {
                enumStringValue = attributes[0].StringValue;
            }

            if (string.Compare(enumStringValue, value, ignoreCase) == 0)
            {
                result = Enum.Parse(enumType, fieldInfo.Name);
            }
        }

        return result;
    }



    // https://randomkeygen.com/
    public static string ENCRYPT_KEY_VERSION = "zHGXsL74J7losSEk9AeSmbuxzotbbmfe"; //1.0.0
    private static int SAFE_KEY = 0b01010101010101010101010101010101; // 1431655765

    #if ENABLE_CHEAT
    public static bool ENABLE_CHEAT = true;
    #else
    public static bool ENABLE_CHEAT = false;
    #endif
    public static bool ENABLE_AUTOPLAY = false;

    public const float DISTANCE_HAND_TUTORIAL = 1.5f;
    public const float DISTANCE_COOL = 1.2f;
    public const float DISTANCE_GREAT = 0.8f;
    public const float DISTANCE_PERFECT = 0.4f;

    public static float NEAR_ACTIVATOR_CENTER = 10f;
    public const int ACTIVATOR_STATE_NONE = 0;
    public const int ACTIVATOR_STATE_ACTIVE = ACTIVATOR_STATE_NONE + 1;
    public const int ACTIVATOR_STATE_TRIGGER = ACTIVATOR_STATE_ACTIVE + 1;
    public const int ACTIVATOR_STATE_END = ACTIVATOR_STATE_TRIGGER + 1;
    public const int ACHIEVEMENT_TIER_MAX = 3;
    public const float TIME_DELAY_NO_NOTE = 2.0f;
    public const float TIME_DELAY_NO_NOTE_TUTORIAL = 0.54f;
    public const float DELAY_END_GAME = 5.0f;
    public const float FADE_OUT_TIME_SOUND_END_GAME = 5.0f;
    public const float TIME_ENABLE_MUSIC_LAYER = 10f;//after this period of time, disabled music layer will be played with half volume

    public static int VIBRATION_STRENGTH = 2;

    public static float SECOND_PER_FRAME = 0.034f; // 1/30
    public static float WAIT_FOR_SECOND = 1;
    public static float WAIT_FOR_HALFSECOND = 0.5f;

    public static float MISSING_TIME = 10f; // seconds

    public static string SCENE_GAME = "Game";    
    public static string SCENE_STUIDO = "Studio";    
    public static string SCENE_LOADING = "Loading";
    public static string STADIUM = "stadium";
    public static string STADIUM_MATERIALS = "stadium_materials";

    public static string[] VENUE_SCENE = {
        "Rio",
        "Rio",
        "Rio",
        "Rio",
        "Rio",
        "Rio",
        "Rio",
        "Rio",
        "Rio",
        "Rio"
    };
    public static string[] SONG_NAME = new string[]{
        "WWRY",
        "RGGA"
    };

    public enum SONG_INDEX:int
    {
        WWRY = 0,
        RGGA
    };

    public static string VENUE_RAINBOW           = "CV001";	
    public static string VENUE_BUDOKAN           = "CV001";	
    public static string VENUE_HAMMER            = "CV001";	
    public static string VENUE_HYDE_PARK         = "CV001";	
    public static string VENUE_SUMMIT            = "CV001";	
    public static string VENUE_FORUM             = "CV001";	
    public static string VENUE_OLYMPIAHALLE      = "CV001";	
    public static string VENUE_ESTADIO           = "CV001";	
    public static string VENUE_CITY_OF_ROCK      = "CV001";	
    public static string VENUE_WEMBLEY            = "CV001";	

    public enum VENUE_INDEX: int
    {
        RIO = 0, // CITY_OF_ROCK
        COUNT
    }


    public static string GotoAP = "GotoAP";
    public enum GAME_STATE
    {
        NONE = 0,
        INIT,
        RESET,
        SCENARIO,
        INTRO,
        INGAME,
        CANCEL,
        SPECIAL_MOVE,
        ENDGAME,
        RESULT
    }

    public enum GAME_SUB_STATE
    {
        NONE = 0,
        FRENZY,
        AUTO_COLLECT,
        END_AUTO_COLLECT,
    }

    public enum GAME_MODE
    {
        NONE = -1,
        EASY,
        MEDIUM,
        HARD
    }

    public enum INGAME_MODE:int
    {
        NONE = -1,
        FREE_VERSION,
        FULL_VERSION
    }

    public static GAME_MODE GetDifficultBy(string diff)
    {
        diff = diff.ToUpper();        
        switch(diff)
        {
            case "EASY":
            return GAME_MODE.EASY;

            case "MEDIUM":
            case "NORMAL":
            return GAME_MODE.MEDIUM;

            case "HARD":
            return GAME_MODE.HARD;
        }

        return GAME_MODE.NONE;
    }

    public enum DISC_TYPE : int
    {
        NONE = 0,
        SILVER,
        GOLD,
        PLATINUM
    }

    public static DISC_TYPE GetDiscTypeBy(string diff)
    {
        diff = diff.ToUpper();
        switch(diff)
        {
            case "SILVER":
            return DISC_TYPE.SILVER;

            case "GOLD":
            return DISC_TYPE.GOLD;

            case "PLATINUM":
            return DISC_TYPE.PLATINUM;
        }

        return DISC_TYPE.NONE;
    }

    public const int TOTAL_DISC_ONE_VENUE = 18;

    [FlagsAttribute]
    public enum INPUT_STATUS
    {
        NONE = 0,
        PRESSED = (1 << 1),
        HOLDING = (1 << 2),
        // RELEASED = (1 << 3),
        SWIPE_UP = (1 << 4),
        SWIPE_DOWN = (1 << 5),
        SWIPE_LEFT = (1 << 6),
        SWIPE_RIGHT = (1 << 7),
        SWIPE_LEFT_ON_RIGHT_SIDE = (1 << 8),
        SWIPE_LEFT_ON_LEFT_SIDE = (1 << 9),
        SWIPE_RIGHT_ON_RIGHT_SIDE = (1 << 10),
        SWIPE_RIGHT_ON_LEFT_SIDE = (1 << 11),
        SWIPE_UP_ON_RIGHT_SIDE = (1 << 12),
        SWIPE_UP_ON_LEFT_SIDE = (1 << 13),
        SWIPE_DOWN_ON_RIGHT_SIDE = (1 << 14),
        SWIPE_DOWN_ON_LEFT_SIDE = (1 << 15),

        HOLD = PRESSED | HOLDING,
        SWIPED = SWIPE_LEFT | SWIPE_RIGHT | SWIPE_UP | SWIPE_DOWN
    };

    // https://weblogs.asp.net/stefansedich/enum-with-string-values-in-c
    public enum SFX : int
    {
        [StringValue("sfx_crowd_bed")]
        CROWD_BED = 0,
        [StringValue("sfx_crowd_cheer_end")]
        CROWD_CHEER_END,
        [StringValue("sfx_crowd_boo_01")]
        CROWD_BOO_01,
        [StringValue("sfx_crowd_boo_02")]
        CROWD_BOO_02,
        [StringValue("sfx_crowd_boo_03")]
        CROWD_BOO_03,
        [StringValue("sfx_crowd_cheer_01")]
        CROWD_CHEER_01,
        [StringValue("sfx_crowd_cheer_02")]
        CROWD_CHEER_02,
        [StringValue("sfx_crowd_cheer_03")]
        CROWD_CHEER_03,
        [StringValue("sfx_ui_buy")]
        UI_BUY,
        [StringValue("sfx_ui_collect_coins")]
        UI_COLLECT_COINS,
        [StringValue("sfx_ui_confirm_level")]
        UI_CONFIRM_LEVEL,
        [StringValue("sfx_ui_earn_disc_gold")]
        UI_EARN_DISC_GOLD,
        [StringValue("sfx_ui_earn_disc_platinum")]
        UI_EARN_DISC_PLATINUM,
        [StringValue("sfx_ui_earn_disc_silver")]
        UI_EARN_DISC_SILVER,
        [StringValue("sfx_ui_gauge_full")]
        UI_GAUGE_FULL,
        [StringValue("sfx_ui_message_pop_up")]
        UI_MESSAGE_POP_UP,
        [StringValue("sfx_ui_score_tally")]
        UI_SCORE_TALLY,
        [StringValue("sfx_ui_unlock_tour")]
        UI_UNLOCK_TOUR,
        [StringValue("sfx_ui_unpause_321")]
        UI_UNPAUSE_321,
        [StringValue("sfx_ui_menu_back")]
        UI_MENU_BACK,
        [StringValue("sfx_ui_menu_select")]
        UI_MENU_SELECT,
        [StringValue("sfx_ui_claim_reward")]
        UI_CLAIM_REWARD,
        [StringValue("sfx_ui_earn_disc_in_game")]
        UI_EARN_DISC_IN_GAME,
        [StringValue("sfx_ui_new_achievement_popup")]
        UI_NEW_ACHIEVEMENT_POPUP,
        [StringValue("sfx_ui_year_change")]
        UI_YEAR_CHANGE,
        [StringValue("sfx_ui_locked")]
        UI_LOCKED,
        [StringValue("sfx_envelope_open")]
        ENVELOPE_OPEN,
        [StringValue("sfx_envelope_receive")]
        ENVELOPE_RECEIVE,
        [StringValue("sfx_fireworks")]
        FIREWORKS,
        [StringValue("sfx_ui_new_reward_popup")]
        UI_NEW_REWARD_POPUP,
        [StringValue("sfx_swipe_row_change")]
        SWIPE_ROW_CHANGE,
        [StringValue("sfx_ui_menu_scroll")]
        UI_MENU_SCROLL,
        [StringValue("sfx_mm_studio_door_open")]
        DOOR_OPEN,
        [StringValue("sfx_mm_studio_enter")]
        STUDIO_ENTER,
        [StringValue("sfx_mm_studio_amb_bed_loop")]
        STUDIO_AMB_BED_LOOP,
    }
    public static string SFX_ROCK_BAR_FULL = "sfx_ui_gauge_full";

    public static string BrianName = "brian";
    public static string RogerName = "roger";
    public static string FreddieName = "freddie";
    public static string JohnName = "john";
    public enum CHARACTERS : int
    {
        [StringValue("Unknow")]
        UNKNOW = -1,
        [StringValue("Brian May")]
        BRIAN_MAY,
        [StringValue("Roger Taylor")]
        ROGER_TAYLOR,
        [StringValue("Freddie Mercury")]
        FREDDIE_MERCURY,
        [StringValue("John Deacon")]
        JOHN_DEACON,
        [StringValue("Queen Band")]
        QUEEN_BAND,
        COUNT
    }

    public enum CHARACTERS_STATUS_COLUMNS : int
    {
        ROGER_INSTRUMENT = 0,
        JOHN_INSTRUMENT,
        BRIAN_INSTRUMENT,
        FREDDIE_INSTRUMENT,
        ROGER_SING,
        JOHN_SING,
        BRIAN_SING,
        FREDDIE_SING,
        COUNT
    }
    
    public static Define.CHARACTERS GetGamePlayCharacterName(string charName)
    {
        if(charName.ToLower().Contains(Define.FreddieName))
        {
            return Define.CHARACTERS.FREDDIE_MERCURY;
        }
        else if(charName.ToLower().Contains(Define.BrianName))
        {
            return Define.CHARACTERS.BRIAN_MAY;
        }
        else if(charName.ToLower().Contains(Define.JohnName))
        {
            return Define.CHARACTERS.JOHN_DEACON;
        }
        else if(charName.ToLower().Contains(Define.RogerName))
        {
            return Define.CHARACTERS.ROGER_TAYLOR;
        }
        return Define.CHARACTERS.UNKNOW;
    }

    public static CHARACTERS ToGamePlayCharacterName(AUDIOS_INDEX audio)
    {
        switch(audio)
        {
            case AUDIOS_INDEX.GUITAR:
                return CHARACTERS.BRIAN_MAY;

            case AUDIOS_INDEX.DRUM:
                return Define.CHARACTERS.ROGER_TAYLOR;

            case AUDIOS_INDEX.VOCAL:
                return Define.CHARACTERS.FREDDIE_MERCURY;

            case AUDIOS_INDEX.BASS:
                return Define.CHARACTERS.JOHN_DEACON;
        }
        return CHARACTERS.UNKNOW;
    }

    public enum AUDIOS_INDEX: int
    {
        [StringValue("Guitar")]
        GUITAR = Define.CHARACTERS.BRIAN_MAY,
        [StringValue("Drum")]
        DRUM = Define.CHARACTERS.ROGER_TAYLOR,
        [StringValue("Vocal")]
        VOCAL = Define.CHARACTERS.FREDDIE_MERCURY,
        [StringValue("Bass")]
        BASS = Define.CHARACTERS.JOHN_DEACON,
        [StringValue("Merge others")]
        MERGE_OTHERS = BASS + 1, // Piano, Perc, others
        [StringValue("Back")]
        BACK = MERGE_OTHERS + 1, // back(...,...)
        COUNT = BACK + 1
    }

    public enum COLORS : int
    {
        [StringValue("None")]
        NONE = -1,
        [StringValue("Disable")]
        GRAY = 0,
        [StringValue("Guitar")]
        CYAN = 1,
        [StringValue("Drum")]
        RED = 2,
        [StringValue("Vocal")]
        GREEN = 3,
        [StringValue("Bass")]
        YELLOW = 4,
        [StringValue("Count")]
        COUNT = 5,
    }

    public static Define.COLORS ParseColor(string colorName)
    {
        switch (colorName)
        {
            case "CYAN":
                return Define.COLORS.CYAN;

            case "RED":
                return Define.COLORS.RED;

            case "GREEN":
                return Define.COLORS.GREEN;

            case "YELLOW":
                return Define.COLORS.YELLOW;
        }


        return Define.COLORS.GRAY;
    }

    public enum MUSIC : int
    {
        KYAL = 0,
        SSOR,
        KIQU,
        WWRY,
        RGGA
    }

    public enum VFX : int
    {
        NONE = -1,
        TAP = 0,
        TAP_LOOP = 1,
        PERFECT = 2,
        PERFECT_LOOP = 3,
    }

    public enum UI_VFX : int
    {
        NONE = -1,
        PERFECT = 0,
        GREAT = 1,
        COOL = 2,
        FAIL_LEFT = 3,
        FAIL_RIGHT = 4,
    }

    [FlagsAttribute]
    public enum PHYSICS
    {
        NONE = (1 << 0),
        COOL = (1 << 1),
        GREAT = (1 << 2),
        PERFECT = (1 << 3),
        COLLIDED = COOL | GREAT | PERFECT
    }

	[FlagsAttribute]
    public enum DRAG_DIRECTION
    {
        NONE = 0,

        X_ZERO = (1 << 0),
        X_PLUS = (1 << 1),
        X_MINUS = (1 << 2),

        Y_ZERO = (1 << 4),
        Y_PLUS = (1 << 5),
        Y_MINUS = (1 << 6),

        DRAG_UP = X_ZERO | Y_PLUS,
        DRAG_DOWN = X_ZERO | Y_MINUS,
        DRAG_LEFT = X_MINUS | Y_ZERO,
        DRAG_RIGHT = X_PLUS | Y_ZERO,
        DRAG_END = X_ZERO | Y_ZERO,

        DRAGGED = DRAG_UP | DRAG_DOWN | DRAG_LEFT | DRAG_RIGHT | DRAG_END
    }

	
    public enum LANES_STATUS
    {
        NONE = 0,
        APPEAR = (1 << 1),
        DISAPPEAR = (1 << 2),
    }

    public enum LANE_ON_SIDE
    {
        NONE = 0,
        LEFT = -1,
        RIGHT = 1,
    }
    
    public enum LINES_NUMBER
    {
        TWO_LINES = 2,
        FOUR_LINES = 4,
        SIX_LINES = 6,
    }

    public enum LANE : int
    {
        ID_1 = 0,
        ID_2,
        ID_3,
        ID_4,
        ID_5,
        ID_6,
    }

    public const float TIME_FIRE_ANIM = 0.15f;
    public const float TIME_HOLD_SCORE_INCREASE = 0.5f;

    public enum NOTE_SPRITE_ID: int
    {
        NORMAL = 0,
        GRAY,
        SPECIAL,
    }

    public enum CONNECTION_LINE_MATERIAL_ID: int
    {
        MID = 0,
        END
    }

    public enum NOTE_TYPE
    {
        NONE = 0,
        SHORT,
        LONG,
        DRAG,
        SWIPE,
        MIGHTY_SWIPE,
    }

    public enum NOTE_DESTROY
    {
        NONE = 0,
        PREVIOUS,
        ME,
        ALL
    }

    public enum ITEM_PART
    {
        HEAD = 0,
        BODY,
        LEG,
        HAIR,
        CROWN,
        SPECIAL_MOVE
    }

    public enum ITEM_STATUS:int
    {
        DEFAULT = 0, // mean equip
        FULL_GAME, // for demo or full version
        LOCK, // two option reward or purchase
        REWARD,
        PURCHASE,
        UNLOCK, // new one
        USED // unlock and used it
    }

    //cheat
    #if ENABLE_CHEAT
    public static bool IsShowDebugInfo = true;
    #else
    public static bool IsShowDebugInfo = false;
    #endif
    public enum CHEAT_BUTTON : int
    {
        [StringValue("Debug info")]
        DEBUG_INFO = 0,
        [StringValue("Add coins")]
        ADD_COINS,
        [StringValue("subtract coins")]
        SUBTRACT_COINS,
        [StringValue("Exit menu")]
        EXIT   
    }
    //end cheat

    public enum ANIM_TYPE : int
    {
        NORMAL = 0,
        SPECIFIC_A1, // For "a1"
        SPECIFIC_A2 // For "a2"
    }
public const float LAST_DURATION_TO_SKIP_CONCERT_CANCEL = 15f;

    public enum TRIGGER
    {
        NONE = 0,
        COOL,
        GREAT,
        PERFECT
    }

    public enum QUALITIES_LEVEL
    {
        VERY_LOW = 0,
        LOW,
        MEDIUM,
        HIGH,
        VERY_HIGH,
        ULTRA    
    }
    
    public enum TUTORIAL : int
    {
        START,
        INGAME_SCENARIO_INTRO,
        INGAME_SCENARIO_FREE,
        INGAME_POPUP_INTRO,
        BASIC_LOADING,
        INGAME_TAP,
        INGAME_USER_TAP,
        INGAME_TAP_HOLD,
        INGAME_USER_TAP_HOLD,
        INGAME_MIGHTY_SWIPE,
        INGAME_USER_MIGHTY_SWIPE,
        INGAME_PRE_POPUP_NOTE_COLOR,
        INGAME_POPUP_NOTE_COLOR,
        INGAME_POPUP_NOTE_COLOR_2,
        INGAME_SCENARIO_END,
        DONE,
    }

    public enum VIEW : int
    {
        NONE = -1,
        INIT,
        MAIN_MENU,
        SETTING,
        VENUE,
        SONG,
        INGAME,
        PAUSE,
        RESULT,
        LOADING
    }
    
    public enum DLC_ERROR_TYPE
    {
        NETWORK_ERROR = 0,
        NOT_ENOUGH_SPACE,
        OTHER,
    }

    public enum FONT : int
    {
        BOLD,
        ITALIC,
    }

    public static string TutorialName = "tutorial";

    public enum TUTORIAL_TYPE : int
    {
        [StringValue("NORMAL_TUTORIAL")]
        NORMAL_TUTORIAL = 0,
        [StringValue("BASIC_TUTORIAL_REPLAY")]
        BASIC_TUTORIAL_REPLAY,
        [StringValue("ADVANCED_TUTORIAL_REPLAY")]
        ADVANCED_TUTORIAL_REPLAY,
    }

    public static string POPUP_SAVE_CONFLICT            =       "PopupSaveConflict";
    public static string POPUP_YES_NO                   =       "PopupYesNo";
    public static string POPUP_OK                       =       "PopupOK";
    public static string POPUP_IAP_SUCCESS_01           =       "PopupIAPSuccess1";
    public static string POPUP_IAP_SUCCESS_02           =       "PopupIAPSuccess2";
    public static string POPUP_REWARD                   =       "PopupReward";
    public static string POPUP_RATING                   =       "PopupRating";
    public static string POPUP_FEEDBACK                 =       "PopupFeedback";
    public static string POPUP_UPDATE                   =       "PopupUpdate";
    public static string POPUP_LOGIN_ERROR              =       "PopupLoginError";
    public static string POPUP_LOGIN                    =       "PopupLogin";
    public static string POPUP_CANNOT_BACK              =       "PopupCannotBack";



    public static string DATABASE_NAME = "Database/Queen_balance_new";
    public static string RainbowName = "rainbow";
    public static string BudokanName = "budokan";
    public static string HammersmithOdeonName = "hammersmith odeon";
    public static string HydeParkName = "hyde park";
    public static string SummitName = "summit";
    public static string ForumName = "forum";
    public static string OlympiahalleName = "olympiahalle";
    public static string EstadioName = "estadio";
    public static string CityOfRockName = "city of rock";
    public static string WembleyName = "wembley";


    public static string CONCERTVENUE_SHEET = "Concert Venues";
    public class CONCERTVENUE_ELEMENT_NAME
    {
        public static string No = "No.";
        public static string ID = "ID";
        public static string Stadium = "Stadium";
        public static string VenueCity = "VenueCity";
        public static string Tour = "Tour";
        public static string Year = "Year";
        public static string UnlockSpecialMove = "UnlockSpecialMove";
        public static string DiscRequirement = "DiscRequirement";
        public static string Unlocked = "Unlocked";
    }

    public static string SONGSINVENUE_SHEET = "Songs In Venues";
    public class SONGSINVENUE_ELEMENT_NAME
    {
        public static string No = "No.";
        public static string ID = "ID";
        public static string SongName = "SongName";
        public static string GuitarAudio = "GuitarAudio";
        public static string GuitarNoteColor = "GuitarNoteColor";
        public static string DrumsAudio = "DrumsAudio";
        public static string DrumsNoteColor = "DrumsNoteColor";
        public static string VocalAudio = "VocalAudio";
        public static string VocalNoteColor = "VocalNoteColor";
        public static string BassAudio = "BassAudio";
        public static string BassNoteColor = "BassNoteColor";
        public static string MergeOthersAudio = "MergeOthersAudio";
        public static string BackAudio = "BackAudio";
        public static string ConcertVenueID = "ConcertVenueID";
        public static string Album = "Album";
        public static string Length = "Length";
        public static string SongYear = "Song Year";
        public static string MainVocal = "MainVocal";
    }

    public static string SONGDIFFICULTY_SHEET = "Song Difficulty";
    public static string SONGDIFFICULTY_CONFIG_SHEET_1 = "SongDifficultyConfig_1";
    public static string SONGDIFFICULTY_CONFIG_SHEET_2 = "SongDifficultyConfig_2";
    public class SONGDIFFICULTY_ELEMENT_NAME
    {
        public static string No = "No.";
        public static string ID = "ID";
        public static string SongID = "SongID";
        public static string Difficult = "Difficult";
        public static string LevelDesign = "LevelDesign";
        public static string DiscRequirement = "DiscRequirement";
        public static string RequirementFrom = "RequirementFrom";
        public static string ScoreToGainSilverDisc = "SS";
        public static string ScoreToGainGoldDisc = "SG";
        public static string ScoreToGainPlatinumDisc = "SP";
        public static string CoinsRewardOnSilverDisc = "RS";
        public static string CoinsRewardOnGoldDisc = "RG";
        public static string CoinsRewardOnPlatinumDisc = "RP";
        public static string Unlocked = "Unlocked";
    }

    public static string ROCKMETER_SHEET = "RockMeter";
    public class ROCKMETER_ELEMENT_NAME
    {
        public static string No = "No.";
        public static string ID = "ID";
        public static string Type = "Type";
        public static string Value = "Value";
    }

    public static string KEY_PROFILES = "profiles";
    public static string KEY_SETTINGS = "settings";
    public static string NULL_VALUE = "NULL";

    public enum NEW_STATUS
    {
        NONE,
        NEW,
        DONE,
    }
    public static int GetCorrectValue(int value)    
    {
        return value ^ SAFE_KEY;
    }
    public static int GetEncryptValue(int value)    
    {
        return value ^ SAFE_KEY;
    }
}
