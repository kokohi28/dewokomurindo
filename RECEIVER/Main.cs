//...Draw Captured Image Helper
#define USE_DRAW_CAPTURE_HELPER
//...Map Refresh Helper
#define USE_MAP_REFRESH_HELPER
//...Radar To Tracker
#define USE_RADAR_TO_TRACKER_HELPER
//...Debug value's
//#define SHOW_DEBUG_VALUE

using System;
using System.IO; //IO FileReader
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D; //Drawing

namespace MataDewa4
{
    public partial class Main : Form
    {
        //=============================== VARIABEL ===================================//
        private UInt16                  v_ActiveCurrentFrm;
        private Int16                   v_RunCoverShow;
        private Int16                   v_RunCoverStep;

        //...Main
        private Komurindo               m_Komurindo;
        private MyGraph                 m_Graph;
        private UI                      m_UI;

        //...Utils
        private Utils.MyPath            m_Path;
        private Utils.MyConfig          m_Config;

        //=============================== CONSTANT ===================================//
        const string                    URL_ABOUT           = @"http://www.unesa.ac.id/";

        private AdditionalButton        m_FrmAdditionalButton;

    #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
        const string        CONTEST_TYPE        = @"KOMURINDO";
        #if KOMURINDO_PAYLOAD
            const string    MYNAME              = @"UNESA Payload";
        #elif KOMURINDO_ROCKET
            const string    MYNAME              = @"UNESA Rocket";
        #endif
    #elif BALON_ATMOSFER
        const string        CONTEST_TYPE        = @"KOMBAT";
        const string        MYNAME              = @"Bala Dewa";
    #endif

        //=============================== INIT's =====================================//
        private void Init()
        {
            //...Instantiate first
            m_Path          = new Utils.MyPath();
            m_Config        = new Utils.MyConfig();

            m_Komurindo     = new Komurindo();
            m_UI            = new UI();
            m_Graph         = new MyGraph();

            //...Variable
            v_RunCoverShow  = 0;

            #if KOMURINDO_ROCKET
                m_FrmAdditionalButton = new AdditionalButton();
            #endif
        }

        private void InitForm()
        {
            Int32 x, y;

            //...frm Home, Setting Tools 
            x = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].PosX;
            y = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].PosY;

            PnlFrm_Home.Location            = new Point(x, y);
            PnlFrm_Home.Width               = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].Width;
            PnlFrm_Home.Height              = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].Height;

            PnlFrm_Tools.Location           = new Point(x, y);
            PnlFrm_Tools.Width              = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].Width;
            PnlFrm_Tools.Height             = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].Height;

            PnlFrm_Setting.Location         = new Point(x, y);
            PnlFrm_Setting.Width            = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].Width;
            PnlFrm_Setting.Height           = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeScreen].Height;

            //...frm Attitude
            x = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.MidTop].PosX;
            y = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.MidTop].PosY;

            PnlFrm_AttitudeGraph.Location   = new Point(x, y);
            PnlFrm_AttitudeGraph.Width      = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeTopLeft].Width;
            PnlFrm_AttitudeGraph.Height     = UI.s_Form.Property[(Int32)c_FORM_TYPE_LIST.WholeTopLeft].Height;

            //...frm Graphic Line
            x = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidCenter].PosX;
            y = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidCenter].PosY;

            PnlFrm_LineGraph.Location       = new Point(x, y);
            PnlFrm_LineGraph.Width          = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidCenter].Width;
            PnlFrm_LineGraph.Height         = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidCenter].Height;
            PnlFrm_LineGraph.BringToFront();

            //...frm Log Data
            x = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidBottom].PosX;
            y = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidBottom].PosY;

            PnlFrm_LogData.Location         = new Point(x, y);
            PnlFrm_LogData.Width            = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidBottom].Width;
            PnlFrm_LogData.Height           = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidBottom].Height;

            //...frm Camera, Tracking, Altimeter, Odometry
            x = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].PosX;
            y = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].PosY;

            PnlFrm_Camera.Location          = new Point(x, y);
            PnlFrm_Camera.Width             = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Width;
            PnlFrm_Camera.Height            = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Height;

            PnlFrm_Tracker.Location         = new Point(x, y);
            PnlFrm_Tracker.Width            = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Width;
            PnlFrm_Tracker.Height           = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Height;

            PnlFrm_AltimeterGraph.Location  = new Point(x, y);
            PnlFrm_AltimeterGraph.Width     = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Width;
            PnlFrm_AltimeterGraph.Height    = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Height;

            PnlFrm_Odo3D.Location = new Point(x, y);
            PnlFrm_Odo3D.Width = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Width;
            PnlFrm_Odo3D.Height = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.TopLeft].Height;

            //...frm Compass, Humidity
            x = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidRight].PosX;
            y = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidRight].PosY;

            PnlFrm_CompassGraph.Location    = new Point(x, y);
            PnlFrm_CompassGraph.Width       = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidRight].Width;
            PnlFrm_CompassGraph.Height      = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidRight].Height;

            PnlFrm_HumidityGraph.Location   = new Point(x, y);
            PnlFrm_HumidityGraph.Width      = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidRight].Width;
            PnlFrm_HumidityGraph.Height     = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.MidRight].Height;

            //...frm Maps, Trajectory
            x = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.WholeRight].PosX;
            y = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.WholeRight].PosY;

            PnlFrm_Maps.Location            = new Point(x, y);
            PnlFrm_Maps.Width               = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.WholeRight].Width;
            PnlFrm_Maps.Height              = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.WholeRight].Height;

            PnlFrm_Trajectory.Location = new Point(x, y);
            PnlFrm_Trajectory.Width = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.WholeRight].Width;
            PnlFrm_Trajectory.Height = UI.s_Form.Property[(UInt32)c_FORM_TYPE_LIST.WholeRight].Height;
        }

        private void InitUI()
        {
            //...Load ICON
            PicBtn_FrmHome.Load         (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Home].DeselectPath);
            PicBtn_FrmAttitude.Load     (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Attitude].DeselectPath);
            PicBtn_FrmAtmosfer.Load     (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Trajectory].DeselectPath);
            PicBtn_FrmPosition.Load     (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Position].DeselectPath);
            PicBtn_FrmCamera.Load       (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Camera].DeselectPath);
            PicBtn_FrmSetting.Load      (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Setting].DeselectPath);
            PicBtn_FrmTools.Load        (UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Tools].DeselectPath);

            PicBtn_RunPlay.Load         (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Play].DeselectPath);
            PicBtn_RunPause.Load        (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Suspend].DeselectPath);
            PicBtn_RunStop.Load         (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Stop].DeselectPath);
            PicBtn_RunRefresh.Load      (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Refresh].DeselectPath);
            PicBtn_RunSave.Load         (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Save].DeselectPath);
            PicBtn_RunEngine.Load       (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Engine].DeselectPath);
            PicBtn_RunClear.Load        (UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Clear].DeselectPath);

            //...Load Button Picture
            //...Title
            Title_BtnPicClose.Load                  (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Close].DeselectPath);
            Title_BtnPicMinimize.Load               (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Minimize].DeselectPath);
            //...Home
            Home_BtnPicVisit.Load                   (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Visit].DeselectPath);
            //...Setting
            Setting_Comm_BtnPicMain.Load            (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].DeselectPath);
            Setting_Comm_BtnPicSecond.Load          (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].DeselectPath);
            Setting_Map_BtnPicCalib.Load            (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].DeselectPath);
            Setting_Sensor_BtnPicCalibPressure.Load (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].DeselectPath);
            Setting_Sensor_BtnPicCalibCompass.Load  (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].DeselectPath);
            Setting_BtnPicCancel.Load               (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Cancel].DeselectPath);
            Setting_BtnPicApply.Load                (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Apply].DeselectPath);
            Setting_BtnPicBack.Load                 (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Monitor].DeselectPath);
            Setting_Cam_BtnPicMode.Load             (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Grayscale].DeselectPath);
            Setting_Sensor_BtnPicUpload.Load        (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Upload].DeselectPath);
            Setting_Sensor_BtnPicDownload.Load      (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Download].DeselectPath);
            //...Calibrate
            Tools_BtnPicConfirmed.Load              (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Confirm].DeselectPath);
            //...Maps
            Maps_BtnPicRefresh.Load                 (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.MapRefresh].DeselectPath);
            //...Checker
            Checker_PicAccelero.Load                (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerAccelero].DeselectPath);
            Checker_PicGyro.Load                    (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGyro].DeselectPath);
            Checker_PicCompass.Load                 (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCompass].DeselectPath);
            Checker_PicGPS.Load                     (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGPS].DeselectPath);
            Checker_PicPressure.Load                (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerPressure].DeselectPath);
            Checker_PicHumidity.Load                (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHumidity].DeselectPath);
            Checker_PicCamera.Load                  (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCamera].DeselectPath);
            Checker_PicHeight.Load                  (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHeight].DeselectPath);
            Checker_PicCalibHeight.Load             (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCalibHeight].DeselectPath);
            Checker_PicTemperature.Load             (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerTemperature].DeselectPath);
            Checker_PicStop.Load                    (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerStop].DeselectPath);
            Checker_PicExit.Load                    (UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerExit].DeselectPath);

            //...Load Picture
            Home_PicTeam.Load           (UI.s_Picture.Path[(UInt16)c_PICTURE_LIST.Team]);
            Home_PicUNESA.Load          (UI.s_Picture.Path[(UInt16)c_PICTURE_LIST.UNESA]);
            RunControl_PicCover.Load    (UI.s_Picture.Path[(UInt16)c_PICTURE_LIST.RunCover]);

            //...Load Text
            Home_TxtProfile.Text = Komurindo.Tools_ParserTxt(Utils.MyPath.s_Text + @"About_" + CONTEST_TYPE + @".txt");

            //...Additional
            Main_PnlMain.BorderStyle = BorderStyle.FixedSingle;
            Title_LblTitle.Text = MYNAME + @" at " + CONTEST_TYPE + DateTime.Today.Year.ToString();
            SetMeasureLine();            
        }

        //=============================== MAIN FORM ==================================//
        public Main()
        {
            //...Init's module
            Init();

            //...Init Form's here
            InitializeComponent();
            InitForm();
            InitUI();
           
            //...Module
            Komurindo.m_Instance.BindSubWindowsControl(
                Status_LblStatus, LogData_LsRec, Tools_LsLog, LogData_LsEvent
                );

            MyGraph.m_Instance.BindGeneralLabel(
                Camera_LblPercentage, Maps_LblLatitudeVal, Maps_LblLongitudeVal, Maps_LblSatelliteVal, Compass_LblHeadingVal, Tracker_LblStatusVal,
                Humidity_LblPercentageVal, Humidity_LblTemperatureVal
                );

            MyGraph.m_Instance.BindAttitudeLabel(
                LineGraph_XAccVal, LineGraph_YAccVal, LineGraph_ZAccVal, Attitude_XGyroVal,
                Attitude_YGyroVal, Attitude_ZGyroVal, Altimeter_LblHeightVal, 
                Attitude_PitchVal, Attitude_RollVal, Altimeter_LblPressureVal
                );

            MyGraph.m_Instance.BindPictureGraph(
                LineGraph_PicGraphic, Attitude_PicXGyro, Attitude_PicYGyro, Attitude_PicZGyro,
                Tracker_PicRadar, Camera_PicCapture, Odo3D_PicOdo
                );

            MyGraph.m_Instance.BindAvionicControl(
                Attitude_ControlPitchRoll, Altimeter_AltimeterGraph, Humidity_PercentageGraph, Compass_ControlHeading
                );

            MyGraph.m_Instance.BindOtherControl(Maps_GMap, Camera_PicProgress);

            MyGraph.m_Instance.InitGMap();
            
            //...Clear graph
            MyGraph.m_Instance.ClearAllGraph();

            //...Ready All :)
            Status_LblDate.Text         = DateTime.Now.ToShortDateString();
            Main_TmrDateTime.Enabled    = true;
            Status_LblStatus.Text       = @"IDLE";
            Maps_ZoomTrack.Value        = MyGraph.GMAP_DEFAULT_ZOOM;

            // Untuk mempercepat load setting
            Setting_Comm_CmbMainBaud.Text = @"57600";
            // Setting_Comm_CmbMainComm.Text = @"COM16";

            //...Temp
        #if !SHOW_DEBUG_VALUE
            Maps_TxtLatitudeVal.Text = @"-7.310856";
            Maps_TxtLongitudeVal.Text = @"112.723316";

            Maps_TxtLatitudeVal.Visible     = false;
            Maps_TxtLongitudeVal.Visible    = false;
        #else 
            Maps_TxtLatitudeVal.Text = @"-7.310856";
            Maps_TxtLongitudeVal.Text = @"112.723316";
        #endif
        }

        ~Main()
        {
            //...Destructor
        }

        //=============================== BUTTON VISUAL ==============================//
        private void OnBtnClose_Hover(object sender, EventArgs e)
        {
            Title_BtnPicClose.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Close].SelectPath);
        }

        private void OnBtnClose_Leave(object sender, EventArgs e)
        {
            Title_BtnPicClose.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Close].DeselectPath);
        }

        private void OnBtnMinimize_Hover(object sender, EventArgs e)
        {
            Title_BtnPicMinimize.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Minimize].SelectPath);
        }

        private void OnBtnMinimize_Leave(object sender, EventArgs e)
        {
            Title_BtnPicMinimize.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Minimize].DeselectPath);
        }
        
        private void OnBtnVisit_Hover(object sender, EventArgs e)
        {
            Home_BtnPicVisit.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Visit].SelectPath);
        }

        private void OnBtnVisit_Leave(object sender, EventArgs e)
        {
            Home_BtnPicVisit.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Visit].DeselectPath);
        }

        private void OnBtnOpenCommMain_Hover(object sender, EventArgs e)
        {
            if (Setting_Comm_CmbMainComm.Enabled == true)
            {
                Setting_Comm_BtnPicMain.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].SelectPath);
            }
            else
            {
                Setting_Comm_BtnPicMain.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Shut].SelectPath);
            }
        }

        private void OnBtnOpenCommMain_Leave(object sender, EventArgs e)
        {
            if (Setting_Comm_CmbMainComm.Enabled == true)
            {
                Setting_Comm_BtnPicMain.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].DeselectPath);
            }
            else
            {
                Setting_Comm_BtnPicMain.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Shut].DeselectPath);
            }
        }

        private void OnBtnOpenCommSecond_Hover(object sender, EventArgs e)
        {
            if (Setting_Comm_CmbSecondComm.Enabled == true)
            {
                Setting_Comm_BtnPicSecond.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].SelectPath);
            }
            else
            {
                Setting_Comm_BtnPicSecond.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Shut].SelectPath);
            }
        }

        private void OnBtnOpenCommSecond_Leave(object sender, EventArgs e)
        {
            if (Setting_Comm_CmbSecondComm.Enabled == true)
            {
                Setting_Comm_BtnPicSecond.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].DeselectPath);
            }
            else
            {
                Setting_Comm_BtnPicSecond.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Shut].DeselectPath);
            }
        }

        private void OnBtnCalibGroundPosition_Hover(object sender, EventArgs e)
        {
            Setting_Map_BtnPicCalib.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].SelectPath);
        }

        private void OnBtnCalibGroundPosition_Leave(object sender, EventArgs e)
        {
            Setting_Map_BtnPicCalib.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].DeselectPath);
        }

        private void OnBtnCalibGroundPressure_Hover(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicCalibPressure.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].SelectPath);
        }

        private void OnBtnCalibGroundPressure_Leave(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicCalibPressure.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].DeselectPath);
        }

        private void OnBtnCalibGroundHeading_Hover(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicCalibCompass.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].SelectPath);
        }

        private void OnBtnCalibGroundHeading_Leave(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicCalibCompass.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Calibrate].DeselectPath);
        }

        private void OnBtnCancelSetting_Hover(object sender, EventArgs e)
        {
            Setting_BtnPicCancel.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Cancel].SelectPath);
        }

        private void OnBtnCancelSetting_Leave(object sender, EventArgs e)
        {
            Setting_BtnPicCancel.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Cancel].DeselectPath);
        }

        private void OnBtnApplySetting_Hover(object sender, EventArgs e)
        {
            Setting_BtnPicApply.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Apply].SelectPath);
        }

        private void OnBtnApplySetting_Leave(object sender, EventArgs e)
        {
            Setting_BtnPicApply.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Apply].DeselectPath);
        }

        private void OnBtnBackToSetting_Hover(object sender, EventArgs e)
        {
            Setting_BtnPicBack.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Monitor].SelectPath);
        }

        private void OnBtnBackToSetting_Leave(object sender, EventArgs e)
        {
            Setting_BtnPicBack.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Monitor].DeselectPath);
        }

        private void OnBtnUpload_Hover(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicUpload.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Upload].SelectPath);
        }

        private void OnBtnUpload_Leave(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicUpload.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Upload].DeselectPath);
        }

        private void OnBtnDownload_Hover(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicDownload.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Download].SelectPath);
        }

        private void OnBtnDownload_Leave(object sender, EventArgs e)
        {
            Setting_Sensor_BtnPicDownload.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Download].DeselectPath);
        }

        private void OnBtnPicMode_Hover(object sender, EventArgs e)
        {
            if (Setting_Cam_Mode.Text == @"RGB")
            {
                Setting_Cam_BtnPicMode.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Grayscale].SelectPath);
            }
            else
            {
                Setting_Cam_BtnPicMode.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.RGB].SelectPath);
            }
        }

        private void OnBtnPicMode_Leave(object sender, EventArgs e)
        {
            if (Setting_Cam_Mode.Text == @"RGB")
            {
                Setting_Cam_BtnPicMode.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Grayscale].DeselectPath);
            }
            else
            {
                Setting_Cam_BtnPicMode.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.RGB].DeselectPath);
            }
        }

        private void OnBtnConfirmCalibrate_Hover(object sender, EventArgs e)
        {
            Tools_BtnPicConfirmed.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Confirm].SelectPath);
        }

        private void OnBtnConfirmCalibrate_Leave(object sender, EventArgs e)
        {
            Tools_BtnPicConfirmed.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Confirm].DeselectPath);
        }

        private void OnBtnMapRefresh_Hover(object sender, EventArgs e)
        {
            Maps_BtnPicRefresh.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.MapRefresh].SelectPath);
        }

        private void OnBtnMapRefresh_Leave(object sender, EventArgs e)
        {
            Maps_BtnPicRefresh.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.MapRefresh].DeselectPath);
        }
        
        //=============================== BUTTON ACTION ==============================//
        private void OnBtnClose_Click(object sender, EventArgs e)
        {
            AppClose();
        }

        private void OnBtnMinimize_Click(object sender, EventArgs e)
        {
            AppMinimize();
        }

        private void OnBtnVisit_Click(object sender, EventArgs e)
        {
            //...Web Navigate
            System.Diagnostics.Process.Start(URL_ABOUT);
        }

        private void OnBtnOpenCommMain_Click(object sender, EventArgs e)
        {
            if (Setting_Comm_CmbMainComm.Enabled == true)
            {
                //...Save sett to config
                string comm     = Setting_Comm_CmbMainComm.Text;
                UInt32 baud     = (UInt32)Komurindo.Calculation_GetInt(Setting_Comm_CmbMainBaud.Text);
            //#if SHOW_DEBUG_VALUE
                Utils.MyConfig.s_CommMain       = comm;
                Utils.MyConfig.s_CommMainBaud   = baud;
            //#endif

                //...Apply to comm
                Komurindo.m_Instance.CommSetParameter((byte)c_LEVEL.Main);
                
                //...Open
                if (Komurindo.m_Instance.CommOpen((byte)c_LEVEL.Main))
                {
                    //...Disable
                    Setting_Comm_CmbMainComm.Enabled = false;
                    Setting_Comm_CmbMainBaud.Enabled = false;
                    Setting_Comm_BtnPicMain.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Shut].SelectPath);
                }
            }
            else
            {
                if (Komurindo.m_Instance.CommClose((byte)c_LEVEL.Main))
                {
                    //...Stop Loop
                    Komurindo.m_Instance.CommRefresh();
                    //...Disable
                    Setting_Comm_CmbMainComm.Enabled = true;
                    Setting_Comm_CmbMainBaud.Enabled = true;
                    Setting_Comm_BtnPicMain.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].SelectPath);
                }
            }
        }

        private void OnBtnOpenCommSecond_Click(object sender, EventArgs e)
        {
            if (Setting_Comm_CmbSecondComm.Enabled == true)
            {
                //...Save sett to config
                string comm     = Setting_Comm_CmbSecondComm.Text;
                UInt32 baud     = (UInt32)Komurindo.Calculation_GetInt(Setting_Comm_CmbSecondBaud.Text);
            //#if !SHOW_DEBUG_VALUE
                Utils.MyConfig.s_CommSecond     = comm;
                Utils.MyConfig.s_CommSecondBaud = baud;
            //#endif

                //...Apply to comm
                Komurindo.m_Instance.CommSetParameter((byte)c_LEVEL.Second);

                //...Open
                if (Komurindo.m_Instance.CommOpen((byte)c_LEVEL.Second))
                {
                    //...Disable
                    Setting_Comm_CmbSecondComm.Enabled = false;
                    Setting_Comm_CmbSecondBaud.Enabled = false;
                    Setting_Comm_BtnPicSecond.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Shut].SelectPath);
                }
            }
            else
            {
                if (Komurindo.m_Instance.CommClose((byte)c_LEVEL.Second))
                {
                    //...Stop Loop
                    Komurindo.m_Instance.CommRefresh();
                    //...Disable
                    Setting_Comm_CmbSecondComm.Enabled = true;
                    Setting_Comm_CmbSecondBaud.Enabled = true;
                    Setting_Comm_BtnPicSecond.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Open].SelectPath);
                }
            }
        }

        private void OnMainCommCombo_Access(object sender, EventArgs e)
        {
            Komurindo.Tools_AddCommList(Setting_Comm_CmbMainComm, true);
        }

        private void OnSecondCommCombo_Access(object sender, EventArgs e)
        {
            Komurindo.Tools_AddCommList(Setting_Comm_CmbSecondComm, true);
        }

        private void OnMainBaudCombo_Access(object sender, EventArgs e)
        {
            Komurindo.Tools_AddBaudrateList(Setting_Comm_CmbMainBaud);
        }

        private void OnSecondBaudCombo_Access(object sender, EventArgs e)
        {
            Komurindo.Tools_AddBaudrateList(Setting_Comm_CmbSecondBaud);
        }

        private void OnBtnCalibGroundPosition_Click(object sender, EventArgs e)
        {
            CalibrateCommand((UInt16)c_CALIB_LIST.Position);
        }

        private void OnBtnCalibGroundPressure_Click(object sender, EventArgs e)
        {
            CalibrateCommand((UInt16)c_CALIB_LIST.Pressure);
        }

        private void OnBtnCalibGroundHeading_Click(object sender, EventArgs e)
        {
            CalibrateCommand((UInt16)c_CALIB_LIST.Heading);
        }

        private void OnBtnCancelSetting_Click(object sender, EventArgs e)
        {
            Status_LblStatus.Text = @"Cancel Setting";
        }

        private void OnBtnApplySetting_Click(object sender, EventArgs e)
        {
            Status_LblStatus.Text = @"Apply Setting";
        }

        private void OnBtnBackToSetting_Click(object sender, EventArgs e)
        {
            Status_LblStatus.Text = @"Back to Monitor";
        }

        private void OnBtnUpload_Click(object sender, EventArgs e)
        {
            Status_LblStatus.Text = @"Upload";
        }

        private void OnBtnDownload_Click(object sender, EventArgs e)
        {
            Status_LblStatus.Text = @"Download";
        }

        private void OnBtnPicMode_Click(object sender, EventArgs e)
        {
            if (Setting_Cam_Mode.Text == @"RGB")
            {
                Setting_Cam_Mode.Text = @"Grayscale";
                Setting_Cam_BtnPicMode.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.RGB].SelectPath);
            }
            else
            {
                Setting_Cam_Mode.Text = @"RGB";
                Setting_Cam_BtnPicMode.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.Grayscale].SelectPath);
            }
        }

        private void OnBtnConfirmCalibrate_Click(object sender, EventArgs e)
        {
            Status_LblStatus.Text = @"Confirmed";
        }

        //=============================== CONTROl FORM SHOW ==========================//
        private void OnBtnHome_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmHome.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Home].SelectPath);
        }

        private void OnBtnHome_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmHome.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Home].DeselectPath);
        }

        private void OnBtnHome_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Home);
        }

        private void OnBtnAttitude_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmAttitude.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Attitude].SelectPath);
        }

        private void OnBtnAttitude_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmAttitude.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Attitude].DeselectPath);
        }

        private void OnBtnAttitude_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Attitude);
        }

        private void OnBtnAtmosfer_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmAtmosfer.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Trajectory].SelectPath);
        }

        private void OnBtnAtmosfer_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmAtmosfer.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Trajectory].DeselectPath);
        }

        private void OnBtnAtmosfer_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Trajectory);
        }

        private void OnBtnPosition_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmPosition.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Position].SelectPath);
        }

        private void OnBtnPosition_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmPosition.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Position].DeselectPath);
        }

        private void OnBtnPosition_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Position);
        }

        private void OnBtnCamera_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmCamera.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Camera].SelectPath);
        }

        private void OnBtnCamera_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmCamera.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Camera].DeselectPath);
        }

        private void OnBtnCamera_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Camera);
        }

        private void OnBtnSetting_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmSetting.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Setting].SelectPath);
        }

        private void OnBtnSetting_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmSetting.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Setting].DeselectPath);
        }

        private void OnBtnSetting_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Setting);
        }

        private void OnBtnTools_Hover(object sender, EventArgs e)
        {
            PicBtn_FrmTools.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Tools].SelectPath);
        }

        private void OnBtnTools_Leave(object sender, EventArgs e)
        {
            PicBtn_FrmTools.Load(UI.s_Control.FrmControl[(UInt16)c_CONTROL_LIST.Tools].DeselectPath);
        }

        private void OnBtnTools_Click(object sender, EventArgs e)
        {
            FormCommand((UInt16)c_CONTROL_LIST.Tools);
        }

        //=============================== RUN CONTROl FORM SHOW ======================//
        private void OnBtnRunPlay_Hover(object sender, EventArgs e)
        {
            PicBtn_RunPlay.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Play].SelectPath);
        }

        private void OnBtnRunPlay_Leave(object sender, EventArgs e)
        {
            PicBtn_RunPlay.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Play].DeselectPath);
        }

        private void OnBtnRunPlay_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Play);
        }

        private void OnBtnRunPause_Hover(object sender, EventArgs e)
        {
            PicBtn_RunPause.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Suspend].SelectPath);
        }

        private void OnBtnRunPause_Leave(object sender, EventArgs e)
        {
            PicBtn_RunPause.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Suspend].DeselectPath);
        }

        private void OnBtnRunPause_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Suspend);
        }

        private void OnBtnRunStop_Hover(object sender, EventArgs e)
        {
            PicBtn_RunStop.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Stop].SelectPath);
        }

        private void OnBtnRunStop_Leave(object sender, EventArgs e)
        {
            PicBtn_RunStop.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Stop].DeselectPath);
        }

        private void OnBtnRunStop_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Stop);
        }

        private void OnBtnRunRefresh_Hover(object sender, EventArgs e)
        {
            PicBtn_RunRefresh.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Refresh].SelectPath);
        }

        private void OnBtnRunRefresh_Leave(object sender, EventArgs e)
        {
            PicBtn_RunRefresh.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Refresh].DeselectPath);
        }

        private void OnBtnRunRefresh_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Refresh);
        }

        private void OnBtnRunSave_Hover(object sender, EventArgs e)
        {
            PicBtn_RunSave.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Save].SelectPath);
        }

        private void OnBtnRunSave_Leave(object sender, EventArgs e)
        {
            PicBtn_RunSave.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Save].DeselectPath);
        }

        private void OnBtnRunSave_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Save);
        }

        private void OnBtnRunEngine_Hover(object sender, EventArgs e)
        {
            PicBtn_RunEngine.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Engine].SelectPath);
        }

        private void OnBtnRunEngine_Leave(object sender, EventArgs e)
        {
            PicBtn_RunEngine.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Engine].DeselectPath);
        }

        private void OnBtnRunEngine_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Engine);
        }

        private void OnBtnRunClear_Hover(object sender, EventArgs e)
        {
            PicBtn_RunClear.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Clear].SelectPath);
        }

        private void OnBtnRunClear_Leave(object sender, EventArgs e)
        {
            PicBtn_RunClear.Load(UI.s_Control.RunControl[(UInt16)c_RUN_CONTROL_LIST.Clear].DeselectPath);
        }

        private void OnBtnRunClear_Click(object sender, EventArgs e)
        {
            RunCommand((UInt16)c_RUN_CONTROL_LIST.Clear);
        }

        //=============================== CHECKER ==========================//
        private void OnCheckerAccelero_Hover(object sender, EventArgs e)
        {
            Checker_PicAccelero.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerAccelero].SelectPath);
        }

        private void OnCheckerAccelero_Leave(object sender, EventArgs e)
        {
            Checker_PicAccelero.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerAccelero].DeselectPath);
        }

        private void OnCheckerAccelero_Click(object sender, EventArgs e)
        {
        #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetAccelero);
        #endif
        }

        private void OnCheckerGyro_Hover(object sender, EventArgs e)
        {
            Checker_PicGyro.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGyro].SelectPath);
        }

        private void OnCheckerGyro_Leave(object sender, EventArgs e)
        {
            Checker_PicGyro.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGyro].DeselectPath);
        }

        private void OnCheckerGyro_Click(object sender, EventArgs e)
        {
        #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetGyro);
        #endif
        }

        private void OnCheckerCompass_Hover(object sender, EventArgs e)
        {
            Checker_PicCompass.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCompass].SelectPath);
        }

        private void OnCheckerCompass_Leave(object sender, EventArgs e)
        {
            Checker_PicCompass.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCompass].DeselectPath);
        }

        private void OnCheckerCompass_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetCompass);
        }

        private void OnCheckerGPS_Hover(object sender, EventArgs e)
        {
            Checker_PicGPS.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGPS].SelectPath);
        }

        private void OnCheckerGPS_Leave(object sender, EventArgs e)
        {
            Checker_PicGPS.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerGPS].DeselectPath);
        }

        private void OnCheckerGPS_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetPosition);
        }

        private void OnCheckerHumidity_Hover(object sender, EventArgs e)
        {
            Checker_PicHumidity.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHumidity].SelectPath);
        }

        private void OnCheckerHumidity_Leave(object sender, EventArgs e)
        {
            Checker_PicHumidity.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHumidity].DeselectPath);
        }

        private void OnCheckerHumidity_Click(object sender, EventArgs e)
        {
        #if BALON_ATMOSFER
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetHumidity);
        #endif
        }

        private void OnCheckerCamera_Hover(object sender, EventArgs e)
        {
            Checker_PicCamera.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCamera].SelectPath);
        }

        private void OnCheckerCamera_Leave(object sender, EventArgs e)
        {
            Checker_PicCamera.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCamera].DeselectPath);
        }

        private void OnCheckerCamera_Click(object sender, EventArgs e)
        {
        #if KOMURINDO_PAYLOAD
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetCamera);
        #endif
        }

        private void OnCheckerHeight_Hover(object sender, EventArgs e)
        {
            Checker_PicHeight.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHeight].SelectPath);
        }

        private void OnCheckerHeight_Leave(object sender, EventArgs e)
        {
            Checker_PicHeight.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerHeight].DeselectPath);
        }

        private void OnCheckerHeight_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetHeight);
        }

        private void OnCheckerCalibHeight_Hover(object sender, EventArgs e)
        {
            Checker_PicCalibHeight.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCalibHeight].SelectPath);
        }

        private void OnCheckerCalibHeight_Leave(object sender, EventArgs e)
        {
            Checker_PicCalibHeight.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerCalibHeight].DeselectPath);
        }

        private void OnCheckerCalibHeight_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.CalibHeight);
        }

        private void OnCheckerTemperature_Hover(object sender, EventArgs e)
        {
            Checker_PicTemperature.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerTemperature].SelectPath);
        }

        private void OnCheckerTemperature_Leave(object sender, EventArgs e)
        {
            Checker_PicTemperature.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerTemperature].DeselectPath);
        }

        private void OnCheckerTemperature_Click(object sender, EventArgs e)
        {
        #if BALON_ATMOSFER
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetTemperature);
        #endif
        }

        private void OnCheckerPressure_Hover(object sender, EventArgs e)
        {
            Checker_PicPressure.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerPressure].SelectPath);
        }

        private void OnCheckerPressure_Leave(object sender, EventArgs e)
        {
            Checker_PicPressure.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerPressure].DeselectPath);
        }

        private void OnCheckerPressure_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.GetPressure);
        }

        private void OnCheckerStop_Hover(object sender, EventArgs e)
        {
            Checker_PicStop.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerStop].SelectPath);
        }

        private void OnCheckerStop_Leave(object sender, EventArgs e)
        {
            Checker_PicStop.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerStop].DeselectPath);
        }

        private void OnCheckerStop_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.Contest_Stop);
        }

        private void OnCheckerExit_Hover(object sender, EventArgs e)
        {
            Checker_PicExit.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerExit].SelectPath);
        }

        private void OnCheckerExit_Leave(object sender, EventArgs e)
        {
            Checker_PicExit.Load(UI.s_Control.Button[(UInt16)c_BUTTON_PICTURE_LIST.CheckerExit].DeselectPath);
        }

        private void OnCheckerExit_Click(object sender, EventArgs e)
        {
            Komurindo.m_Instance.ActionGeneral((UInt16)c_COMMAND_LIST.Contest_Exit);
        }

        //=============================== ACCESS HELPER ==============================//
        private void OnBtnPicCapture_Click(object sender, EventArgs e)
        {
        #if USE_DRAW_CAPTURE_HELPER
            #if KOMURINDO_PAYLOAD                
                //...Percentage
                Komurindo.m_Instance.UpdateCapturedPercentage(Komurindo.DATA_CAMERA_NORMAL, false);
            #endif
        #endif
        }

        private void OnBtnPicRadar_MouseUp(object sender, MouseEventArgs e)
        {
        #if USE_RADAR_TO_TRACKER_HELPER
            MyGraph.m_Instance.Tracker(0, 0, 0, 0, (Int16)e.X, (Int16)e.Y, (byte)c_GRAPH_UPDATE_MODE.Both);
        #endif
        }

        private void OnBtnPicRadar_DoubleClick(object sender, EventArgs e)
        {
        #if USE_RADAR_TO_TRACKER_HELPER
            //Komurindo.m_Instance.PositionTracker(0, 0);
        #endif
        }

        private void OnBtnMapRefresh_Click(object sender, EventArgs e)
        {
        #if USE_MAP_REFRESH_HELPER
            // float lati = float.Parse(Maps_TxtLatitudeVal.Text);
            // float longi = float.Parse(Maps_TxtLongitudeVal.Text);

            Komurindo.m_Instance.UpdateMaps((UInt16) Maps_ZoomTrack.Value);
        #endif
        }

        private void Tools_BtnSetCoordinat_Click(object sender, EventArgs e)
        {
            //...var's
            Int32[] latitude    = new Int32[(UInt16)c_COORDINAT_TEST._Count];
            Int32[] longitude   = new Int32[(UInt16)c_COORDINAT_TEST._Count];
            string value        = @"";

            //...get
            //...latitude
            value = Tools_TxtLatitudeHome.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude[(UInt16)c_COORDINAT_TEST.Home]     = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLatitudeScale1.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude[(UInt16)c_COORDINAT_TEST.Scale1]   = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLatitudeScale2.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude[(UInt16)c_COORDINAT_TEST.Scale2]   = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLatitudeScale3.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude[(UInt16)c_COORDINAT_TEST.Scale3]   = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLatitudeScale4.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude[(UInt16)c_COORDINAT_TEST.Scale4]   = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLatitudeTrack.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude[(UInt16)c_COORDINAT_TEST.Track]    = Komurindo.Calculation_GetInt(value);
            
            //...longitude
            value = Tools_TxtLongitudeHome.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude[(UInt16)c_COORDINAT_TEST.Home]    = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLongitudeScale1.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude[(UInt16)c_COORDINAT_TEST.Scale1]  = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLongitudeScale2.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude[(UInt16)c_COORDINAT_TEST.Scale2]  = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLongitudeScale3.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude[(UInt16)c_COORDINAT_TEST.Scale3]  = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLongitudeScale4.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude[(UInt16)c_COORDINAT_TEST.Scale4]  = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLongitudeTrack.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude[(UInt16)c_COORDINAT_TEST.Track]   = Komurindo.Calculation_GetInt(value);

            //....Save
            Komurindo.m_Instance.SetTrackerCoordinat(latitude, longitude);
        }

        private void Tools_BtnTrackCoordinat_Click(object sender, EventArgs e)
        {
            //...var's
            Int32 latitude, longitude;
            string value = @"";

            //...get
            value = Tools_TxtLatitudeTrack.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            latitude    = Komurindo.Calculation_GetInt(value);
            value = Tools_TxtLongitudeTrack.Text.Replace(@"-", @"");
            value = value.Replace(@".", @"");
            longitude   = Komurindo.Calculation_GetInt(value);

            Komurindo.m_Instance.TestTracker(latitude, longitude);
        }

        //=============================== WIN FORM MODUL HERE ========================//
        private void Visibility_Form(
            bool home, bool attitude, bool line, bool altimeter, bool humidity, bool maps, 
            bool tracker, bool compass, bool camera, bool log, bool setting, bool tools,
            bool odo, bool trajectory, bool launchDeg)
        {
            if (PnlFrm_Home.Visible != home) 
                PnlFrm_Home.Visible = home;

            if (PnlFrm_AttitudeGraph.Visible != attitude)
                PnlFrm_AttitudeGraph.Visible = attitude;

            if (PnlFrm_LineGraph.Visible != line)
                PnlFrm_LineGraph.Visible = line;

            if (PnlFrm_AltimeterGraph.Visible != altimeter)
                PnlFrm_AltimeterGraph.Visible = altimeter;

            if (PnlFrm_HumidityGraph.Visible != humidity)
                PnlFrm_HumidityGraph.Visible = humidity;

            if (PnlFrm_Maps.Visible != maps)
                PnlFrm_Maps.Visible = maps;

            if (PnlFrm_Tracker.Visible != tracker)
                PnlFrm_Tracker.Visible = tracker;

            if (PnlFrm_CompassGraph.Visible != compass)
                PnlFrm_CompassGraph.Visible = compass;
            
            if (PnlFrm_Camera.Visible != camera)
                PnlFrm_Camera.Visible = camera;

            if (PnlFrm_LogData.Visible != log)
                PnlFrm_LogData.Visible = log;

            if (PnlFrm_Setting.Visible != setting)
                PnlFrm_Setting.Visible = setting;

            if (PnlFrm_Tools.Visible != tools)
                PnlFrm_Tools.Visible = tools;

            if (PnlFrm_Odo3D.Visible != odo)
                PnlFrm_Odo3D.Visible = odo;

            if (PnlFrm_Trajectory.Visible != trajectory)
                PnlFrm_Trajectory.Visible = trajectory;
        }

        private void IndicatorChange_Frm(
            Color homeColor, Color attitudeColor, Color atmosferColor, Color positionColor,
            Color cameraColor, Color settingColor, Color toolsColor
            )
        {
            if (PnlSeparator_FrmHome.BackColor != homeColor)
                PnlSeparator_FrmHome.BackColor = homeColor;

            if (PnlSeparator_FrmAttitude.BackColor != attitudeColor)
                PnlSeparator_FrmAttitude.BackColor = attitudeColor;

            if (PnlSeparator_FrmAtmosfer.BackColor != atmosferColor)
                PnlSeparator_FrmAtmosfer.BackColor = atmosferColor;

            if (PnlSeparator_FrmPosition.BackColor != positionColor)
                PnlSeparator_FrmPosition.BackColor = positionColor;

            if (PnlSeparator_FrmCamera.BackColor != cameraColor)
                PnlSeparator_FrmCamera.BackColor = cameraColor;

            if (PnlSeparator_FrmSetting.BackColor != settingColor)
                PnlSeparator_FrmSetting.BackColor = settingColor;

            if (PnlSeparator_FrmTools.BackColor != toolsColor)
                PnlSeparator_FrmTools.BackColor = toolsColor;
        }

        private void IndicatorChange_Run(Color play, Color pause, Color stop, Color refresh, Color save, Color engine, Color clear)
        {
            if (PnlSeparator_RunPlay.BackColor != play)
                PnlSeparator_RunPlay.BackColor = play;

            if (PnlSeparator_RunPause.BackColor != pause)
                PnlSeparator_RunPause.BackColor = pause;

            if (PnlSeparator_RunStop.BackColor != stop)
                PnlSeparator_RunStop.BackColor = stop;

            if (PnlSeparator_RunRefresh.BackColor != refresh)
                PnlSeparator_RunRefresh.BackColor = refresh;

            if (PnlSeparator_RunSave.BackColor != save)
                PnlSeparator_RunSave.BackColor = save;

            if (PnlSeparator_RunEngine.BackColor != engine)
                PnlSeparator_RunEngine.BackColor = engine;

            if (PnlSeparator_RunClear.BackColor != clear)
                PnlSeparator_RunClear.BackColor = clear;
        }

        private void FormCommand(UInt16 index)
        {
            //...SET This First!
            SetActiveCurrentForm(index);
            bool showAdditionalButton = false;
            switch (index)
            {
                case (UInt16)c_CONTROL_LIST.Attitude:
                    Visibility_Form(false, true, true, false, false, false, false, false, false, true, false, false, true, false, false);
                    IndicatorChange_Frm(Color.Black, Color.Aquamarine, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);
                    ShowRunControl();

                    showAdditionalButton = true;
                    break;
                case (UInt16)c_CONTROL_LIST.Trajectory:
                    Visibility_Form(false, false, false, true, true, false, false, false, false, true, false, false, false, true, false);
                    IndicatorChange_Frm(Color.Black, Color.Black, Color.Aquamarine, Color.Black, Color.Black, Color.Black, Color.Black);
                    ShowRunControl();
                    break;
                case (UInt16)c_CONTROL_LIST.Position:
                    Visibility_Form(false, false, false, true, false, true, false, true, false, true, false, false, false, false, false);
                    IndicatorChange_Frm(Color.Black, Color.Black, Color.Black, Color.Aquamarine, Color.Black, Color.Black, Color.Black);
                    ShowRunControl();
                    break;
                case (UInt16)c_CONTROL_LIST.Camera:
                    Visibility_Form(false, false, false, false, false, true, false, true, true, true, false, false, false, false, false);
                    IndicatorChange_Frm(Color.Black, Color.Black, Color.Black, Color.Black, Color.Aquamarine, Color.Black, Color.Black);
                    ShowRunControl();
                    break;
                case (UInt16)c_CONTROL_LIST.Setting:
                    Visibility_Form(false, false, false, false, false, false, false, false, false, false, true, false, false, false, false);
                    IndicatorChange_Frm(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Aquamarine, Color.Black);
                    HideRunControl();
                    break;
                case (UInt16)c_CONTROL_LIST.Tools:
                    Visibility_Form(false, false, false, false, false, false, false, false, false, false, false, true, false, false, false);
                    IndicatorChange_Frm(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Aquamarine);
                    HideRunControl();
                    break;
                case (UInt16)c_CONTROL_LIST.Home:
                    Visibility_Form(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
                    IndicatorChange_Frm(Color.Aquamarine, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);
                    HideRunControl();
                    break;
                default:
                    break;
            }

#if KOMURINDO_ROCKET
            if (showAdditionalButton)
            {
                m_FrmAdditionalButton.Show();
            }
            else
            {
                m_FrmAdditionalButton.Hide();
            }
#endif

            //...FURTHER Proceed!
            OnBtnForm_Click();
        }

        private void RunCommand(UInt16 index)
        {
            //...Visual changes
            switch (index)
            {
                case (UInt16)c_RUN_CONTROL_LIST.Play:
                    IndicatorChange_Run(Color.Magenta, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);
                    break;
                case (UInt16)c_RUN_CONTROL_LIST.Suspend:
                    IndicatorChange_Run(Color.Black, Color.Magenta, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);
                    break;
                case (UInt16)c_RUN_CONTROL_LIST.Stop:
                    IndicatorChange_Run(Color.Black, Color.Black, Color.Magenta, Color.Black, Color.Black, Color.Black, Color.Black);
                    break;
                case (UInt16)c_RUN_CONTROL_LIST.Refresh:
                    IndicatorChange_Run(Color.Black, Color.Black, Color.Black, Color.Magenta, Color.Black, Color.Black, Color.Black);
                    break;
                case (UInt16)c_RUN_CONTROL_LIST.Save:
                    IndicatorChange_Run(Color.Black, Color.Black, Color.Black, Color.Black, Color.Magenta, Color.Black, Color.Black);
                    break;
                case (UInt16)c_RUN_CONTROL_LIST.Engine:
                    IndicatorChange_Run(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Magenta, Color.Black);
                    break;
                case (UInt16)c_RUN_CONTROL_LIST.Clear:
                    IndicatorChange_Run(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Magenta);
                    break;
                default:
                    break;
            }

            //...Through Module
            Komurindo.m_Instance.Action(index);
        }

        private void CalibrateCommand(UInt16 index)
        {
            //Komurindo.m_Instance.OnReceive_Calibration(index);
            //FormCommand((UInt16)c_CONTROL_LIST.Tools);

            if (index == (UInt16)c_CALIB_LIST.Position)
            {
                //...Set Home Position
                Komurindo.m_Instance.SetHomePosition(Setting_Map_TxtLatitude.Text, Setting_Map_TxtLongitude.Text, 
                    Komurindo.Calculation_GetInt(Setting_Map_TxtRange.Text));
            }
        }

        private void SetMeasureLine()
        {
            //...New Bitmap
            Bitmap measureBottom = new Bitmap(720, 10);
            Bitmap measureLeft = new Bitmap(30, 230);

            using (Graphics gBottom = Graphics.FromImage(measureBottom))
            {
                gBottom.Clear(Color.Black);
                using (Pen pBottom = new Pen(Color.White, 1))
                {
                    //...Horizontal Line
                    gBottom.DrawLine(pBottom, 0, 0, 720, 0);
                    //...Center Line
                    gBottom.DrawLine(pBottom, 360, 0, 360, 10);
                    //...Split 1
                    gBottom.DrawLine(pBottom, 180, 0, 180, 7);
                    gBottom.DrawLine(pBottom, 540, 0, 540, 7);
                    //...Split 2
                    gBottom.DrawLine(pBottom, 90, 0, 90, 4);
                    gBottom.DrawLine(pBottom, 270, 0, 270, 4);
                    gBottom.DrawLine(pBottom, 450, 0, 450, 4);
                    gBottom.DrawLine(pBottom, 630, 0, 630, 4);
                    //...Release
                    pBottom.Dispose();
                }
                //...Release
                gBottom.Dispose();
            }

            using (Graphics gLeft = Graphics.FromImage(measureLeft))
            {
                gLeft.Clear(Color.Black);
                using (Pen pLeft = new Pen(Color.White, 1))
                {
                    //...Vertical Line
                    gLeft.DrawLine(pLeft, 29, 0, 29, 230);
                    //...Center Line
                    gLeft.DrawLine(pLeft, 15, 110, 29, 110);
                    //...Split 1
                    gLeft.DrawLine(pLeft, 20, 55, 29, 55);
                    gLeft.DrawLine(pLeft, 20, 165, 29, 165);
                    //...Split 2
                    gLeft.DrawLine(pLeft, 25, 27, 29, 27);
                    gLeft.DrawLine(pLeft, 25, 83, 29, 83);
                    gLeft.DrawLine(pLeft, 25, 137, 29, 137);
                    gLeft.DrawLine(pLeft, 25, 193, 29, 193);
                    //...Release
                    pLeft.Dispose();
                }
                //...Release
                gLeft.Dispose();
            }
            //...Apply Graph
            LineGraph_PicMeasureBottom.Image    = measureBottom;
            LineGraph_PicMeasureLeft.Image      = measureLeft;
        }

        private void Main_TmrDateTime_Tick(object sender, EventArgs e)
        {
            Status_LblTime.Text = DateTime.Now.ToLongTimeString();
        }

        private void Main_TmrRunControl_Tick(object sender, EventArgs e)
        {
            v_RunCoverShow++;
            RunControl_PicCover.Height += v_RunCoverStep;
            //...Off Timer
            if (v_RunCoverShow >= 10)
            {
                v_RunCoverShow = 0;
                Main_TmrRunControl.Enabled = false;
            }
        }

        //...RUN CONTROL HIDE - SHOW
        private void HideRunControl()
        {
            if (RunControl_PicCover.Height != 240)
            {
                Main_TmrRunControl.Enabled = true;
                RunControl_PicCover.Height = 0;
                v_RunCoverStep = 24;
            }
        }

        private void ShowRunControl()
        {
            if (RunControl_PicCover.Height != 0)
            {
                Main_TmrRunControl.Enabled = true;
                RunControl_PicCover.Height = 240;
                v_RunCoverStep = -24;
            }
        }
        
        //...WIN FORM
        private void AppClose()
        {
            Komurindo.m_Instance.CommRefresh();
            Application.Exit();
        }

        private void AppMinimize()
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //=============================== MODUL HERE =================================//
        //...FORM & RUN
        public void SetActiveCurrentForm(UInt16 current)
        {
            v_ActiveCurrentFrm = current;
            Komurindo.m_Instance.m_CurrentControl = current;
        }

        public void ExternalBtn_Click(UInt16 indexRun)
        {
            //...Through Module
            Komurindo.m_Instance.AdditionalAction(indexRun);   
        }

        public void OnBtnForm_Click()
        {
            //...W.I.P
        }

        public void OnBtnRun_Click(UInt16 indexRun)
        {
            //...W.I.P
        }

        private void Maps_ZoomTrack_Scroll(object sender, EventArgs e)
        {
            Maps_GMap.Zoom = Maps_ZoomTrack.Value;
        }

        private void Maps_BtnZoomOut_Click(object sender, EventArgs e)
        {
            if (Maps_ZoomTrack.Value > Maps_ZoomTrack.Minimum)
            {
                Maps_ZoomTrack.Value = Maps_ZoomTrack.Value - 1;
                Maps_GMap.Zoom = Maps_ZoomTrack.Value;
            }
        }

        private void Maps_BtnZoomIn_Click(object sender, EventArgs e)
        {
            if (Maps_ZoomTrack.Value < Maps_ZoomTrack.Maximum)
            {
                Maps_ZoomTrack.Value = Maps_ZoomTrack.Value + 1;
                Maps_GMap.Zoom = Maps_ZoomTrack.Value;
            }
        }

        private void Maps_GMap_Load(object sender, EventArgs e)
        {
            // DO NOTHING
        }

        private void Maps_GMap_Mouse_Wheel(object sender, EventArgs e)
        {
            // TODO GMAP ZOOM WITH MOUSE SCROLL
        }
    }
}
