using System;
using System.Collections.Generic;
using System.Text;

namespace MataDewa4
{
    class UI
    {
        //=============================== CONSTANT ===================================//
        //...FORM
        public struct FORM_PROPERTY
        {
            public UInt16 Width;
            public UInt16 Height;
            public UInt16 PosX;
            public UInt16 PosY;
        }

        public struct FORM_CONSTANT
        {
            public FORM_PROPERTY[] Property;
        }

        //...PICTURE - BUTTON PICTURE
        public struct PICTURE_CONTROL
        {
            public string[] Path;
        }

        //...CONTROL
        public struct CONTROL_FRM
        {
            public string SelectPath;
            public string DeselectPath;
        }

        public struct CONTROL
        {
            public CONTROL_FRM[] FrmControl;
            public CONTROL_FRM[] RunControl;
            public CONTROL_FRM[] Button;
        }

        //=============================== CLASS MEMBER ===============================//
        //...Private
        private string[] m_IconName;
        private string[] m_IconRunName;
        private string[] m_ButtonIconName;
        private string[] m_IconColor;
        
        //...Public
        public static CONTROL           s_Control   = new CONTROL();
        public static FORM_CONSTANT     s_Form      = new FORM_CONSTANT();
        public static PICTURE_CONTROL   s_Picture   = new PICTURE_CONTROL();
            
        //=============================== MAIN INSTANCE ==============================//
        public UI()
        {
            //...Form Init
            s_Form.Property         = new FORM_PROPERTY[(UInt16)c_FORM_TYPE_LIST._Count];
            InitFormConstant();

            //...Picture Init
            s_Picture.Path          = new string[(UInt16)c_PICTURE_LIST._Count];
            InitPicture();

            //...Control, Run Control, Button
            m_IconName              = new string[(UInt16)c_CONTROL_LIST._Count];
            m_IconColor             = new string[(UInt16)c_ICON_COLOR_LIST._Count];
            m_IconRunName           = new string[(UInt16)c_RUN_CONTROL_LIST._Count];
            m_ButtonIconName        = new string[(UInt16)c_BUTTON_PICTURE_LIST._Count];

            s_Control.FrmControl    = new CONTROL_FRM[(UInt16)c_CONTROL_LIST._Count];
            s_Control.RunControl    = new CONTROL_FRM[(UInt16)c_RUN_CONTROL_LIST._Count];
            s_Control.Button        = new CONTROL_FRM[(UInt16)c_BUTTON_PICTURE_LIST._Count];
            
            InitIconColor();
            InitControl_RunControl();
        }

        ~UI()
        {
            //...RELEASE ALL
        }

        //=============================== INIT METHOD ================================//
        private void InitFormConstant()
        {
            UInt16 index;

            //...Whole Screen
            index = (UInt16)c_FORM_TYPE_LIST.WholeScreen;
            s_Form.Property[index].Width            = 765;
            s_Form.Property[index].Height           = 690;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 40;

            //...Whole Right
            index = (UInt16)c_FORM_TYPE_LIST.WholeRight;
            s_Form.Property[index].Width            = 550;
            s_Form.Property[index].Height           = 501;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 40;

            //...Whole Center
            index = (UInt16)c_FORM_TYPE_LIST.WholeCenter;
            s_Form.Property[index].Width            = 765;
            s_Form.Property[index].Height           = 501;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 40;

            //...Whole Top Left
            index = (UInt16)c_FORM_TYPE_LIST.WholeTopLeft;
            s_Form.Property[index].Width            = 550;
            s_Form.Property[index].Height           = 240;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 40;

            //...Mid Top
            index = (UInt16)c_FORM_TYPE_LIST.MidTop;
            s_Form.Property[index].Width            = 765;
            s_Form.Property[index].Height           = 240;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 40;

            //...Mid Center
            index = (UInt16)c_FORM_TYPE_LIST.MidCenter;
            s_Form.Property[index].Width            = 765;
            s_Form.Property[index].Height           = 262;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 279;

            //...Mid Bottom
            index = (UInt16)c_FORM_TYPE_LIST.MidBottom;
            s_Form.Property[index].Width            = 765;
            s_Form.Property[index].Height           = 190;
            s_Form.Property[index].PosX             = 157;
            s_Form.Property[index].PosY             = 545;

            //...Top Left
            index = (UInt16)c_FORM_TYPE_LIST.TopLeft;
            s_Form.Property[index].Width            = 216;
            s_Form.Property[index].Height           = 253;
            s_Form.Property[index].PosX             = 706;
            s_Form.Property[index].PosY             = 40;

            //...Mid Left
            index = (UInt16)c_FORM_TYPE_LIST.MidRight;
            s_Form.Property[index].Width            = 216;
            s_Form.Property[index].Height           = 249;
            s_Form.Property[index].PosX             = 706;
            s_Form.Property[index].PosY             = 292;
        }

        private void InitPicture()
        {
            string path = Utils.MyPath.s_Picture;
            string contestType;

            #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
                contestType = @"KOMURINDO";
            #elif BALON_ATMOSFER
                contestType = @"KOMBAT";
            #endif
            
            s_Picture.Path[(UInt16)c_PICTURE_LIST.Team]         = path + @"Team_" + contestType + @".jpg";
            s_Picture.Path[(UInt16)c_PICTURE_LIST.RunCover]     = path + @"ICON_" + contestType + @".jpg";
            s_Picture.Path[(UInt16)c_PICTURE_LIST.UNESA]        = path + @"UNESA.jpg";
        }

        private void InitIconColor()
        {
            m_IconColor[(UInt16)c_ICON_COLOR_LIST.Normal_Black]          = @"_BLACK.jpg";
            m_IconColor[(UInt16)c_ICON_COLOR_LIST.Normal_White]          = @"_WHITE.jpg";
            m_IconColor[(UInt16)c_ICON_COLOR_LIST.Select_Blue]           = @"_BLUE.jpg";
            m_IconColor[(UInt16)c_ICON_COLOR_LIST.Select_Orange]         = @"_ORANGE.jpg";
            m_IconColor[(UInt16)c_ICON_COLOR_LIST.Select_Green]          = @"_GREEN.jpg";
        }

        private void InitControl_RunControl()
        {
            //...List
            m_IconName[(UInt16)c_CONTROL_LIST.Attitude]                  = @"ROCKET";
            m_IconName[(UInt16)c_CONTROL_LIST.Trajectory]                = @"TRAJECTORY";
            m_IconName[(UInt16)c_CONTROL_LIST.Camera]                    = @"CAMERA";
            m_IconName[(UInt16)c_CONTROL_LIST.Position]                  = @"POSITION";
            m_IconName[(UInt16)c_CONTROL_LIST.Setting]                   = @"GEAR";
            m_IconName[(UInt16)c_CONTROL_LIST.Tools]                     = @"TOOLS";
            m_IconName[(UInt16)c_CONTROL_LIST.Home]                      = @"HOME";

            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Play]               = @"PLAY";
            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Suspend]            = @"PAUSE";
            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Stop]               = @"STOP";
            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Refresh]            = @"REFRESH";
            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Save]               = @"SAVE";
            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Engine]             = @"ENGINE";
            m_IconRunName[(UInt16)c_RUN_CONTROL_LIST.Clear]              = @"TRASH";

            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Visit]        = @"GLOBE";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Close]        = @"CLOSE";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Minimize]     = @"MINIMIZE";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Open]         = @"OPEN";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Shut]         = @"SHUT";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate]    = @"CALIB";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.RGB]          = @"RGB";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Grayscale]    = @"GRAYSCALE";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Download]     = @"DOWNLOAD";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Upload]       = @"UPLOAD";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Monitor]      = @"MONITOR";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Apply]        = @"APPLY";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Cancel]       = @"CANCEL";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.Confirm]      = @"CONFIRM";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.MapRefresh]   = @"MAPREFRESH";

            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerAccelero]    = @"CHECKER_ACCELERO";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCalibHeight] = @"CHECKER_CALIBHEIGHT";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCamera]      = @"CHECKER_CAMERA";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCompass]     = @"CHECKER_COMPASS";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGPS]         = @"CHECKER_GPS";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGyro]        = @"CHECKER_GYRO";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHeight]      = @"CHECKER_HEIGHT";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHumidity]    = @"CHECKER_HUMIDITY";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerPressure]    = @"CHECKER_PRESSURE";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerTemperature] = @"CHECKER_TEMPERATURE";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerStop]        = @"CHECKER_STOP";
            m_ButtonIconName[(UInt16)c_BUTTON_PICTURE_LIST.CheckerExit]        = @"CHECKER_EXIT";

            string iconPath = Utils.MyPath.s_Icon;
            //...Control > Home, Attitude, Atmosfer, Position, Camera, Setting, Tools
            for (UInt16 i = 0; i < (UInt16)c_CONTROL_LIST._Count; i++)
            {
                s_Control.FrmControl[i].SelectPath      =
                    iconPath + m_IconName[i] + m_IconColor[(UInt16)c_ICON_COLOR_LIST.Select_Orange];

                s_Control.FrmControl[i].DeselectPath    =
                    iconPath + m_IconName[i] + m_IconColor[(UInt16)c_ICON_COLOR_LIST.Normal_White];
            }
            //...Run Control > Play, Pause, Stop, Refresh, Save, Clear
            for (UInt16 i = 0; i < (UInt16)c_RUN_CONTROL_LIST._Count; i++)
            {
                s_Control.RunControl[i].SelectPath      =
                    iconPath + m_IconRunName[i] + m_IconColor[(UInt16)c_ICON_COLOR_LIST.Select_Blue];

                s_Control.RunControl[i].DeselectPath    =
                    iconPath + m_IconRunName[i] + m_IconColor[(UInt16)c_ICON_COLOR_LIST.Normal_Black];
            }
            //...Button
            for (UInt16 i = 0; i < (UInt16)c_BUTTON_PICTURE_LIST._Count; i++)
            {
                s_Control.Button[i].SelectPath          =
                    iconPath + m_ButtonIconName[i] + m_IconColor[(UInt16)c_ICON_COLOR_LIST.Select_Green];

                s_Control.Button[i].DeselectPath        =
                    iconPath + m_ButtonIconName[i] + m_IconColor[(UInt16)c_ICON_COLOR_LIST.Normal_Black];
            }
        }
    }
}