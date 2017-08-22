//...Saved to Log feature
#define USE_LOG_SAVED

#if USE_LOG_SAVED
    //#define USE_LOG_SAVED_HEADER
#endif

//...Timer Based Graph Update
#define USE_TIME_UPDATE_GRAPH

//...Accelero Data from MPU6050/CMPS10
// #define USE_MPU6050_ACCELERO
#define USE_GY80_ACCELERO

//...JPEG Picture data
#define USE_JPEG_PICTURE
//...Debug value's
#define SHOW_DEBUG_VALUE
//....Tracker test
#define USE_TRACKER_TEST

using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace MataDewa4
{
    class Komurindo
    {
        //=============================== STRUCTURE ==================================//
    #if USE_TIME_UPDATE_GRAPH
        struct GRAPH_UPDATE
        {
            public bool[]   IsNeedUpDate;
            public UInt16[] Counter;  
        }
    #endif

        //=============================== CONSTANT ===================================//
        //...Specific
        enum TIMER_LIST
        {
            Main, Status, _Count
        }

        enum LISTBOX_LIST
        {
            Monitor, Tools, Event, _Count
        }

        //...Data Pos
        enum PAYLOAD_LAUNCH_DATAPOS
        {
            AccX,
            AccY,
            AccZ,
            GyroX,
            GyroY,
            GyroZ,
            _Count
        }

        enum PAYLOAD_TRACKING_DATAPOS
        {
            Longitude,
            Latitude,
            Height,
            Satellite,
            Speed,
            Heading,
            Pressure,
            _Count,
        }

        enum ROCKET_LAUNCH_DATAPOS
        {
            Pitch,
            Yaw,
            Roll,
            Heading,
            Height,
            // Latitude,
            // Longitude,
            _Count,
        }

    #if BALON_ATMOSFER
        enum BALON_LAUNCH_DATAPOS
        {
            ID          = 0,
            Humidity    = 4,
            Temperature = 8,
            Pressure    = 12,
            Latitude    = 19,
            Longitude   = 30,
            Enter       = 41
        }
    #endif

        //...Graph Control list
        enum GRAPH_CONTROL
        {
            LineGraph, LineGraphText,
            PitchRoll, PitchRollText,
            Gyro, GyroText,
            Altimeter, AltimeterText,
            Maps, MapsText,
            Compass, CompassText,
            Tracker, TrackerText,
            Humidity, HumidityText,
            _Count
        }

        //...Error List
        enum ERROR_LIST
        {
            CorruptData, MissData, MissLF, _Count
        }

        //...Constant
        const string    ISO_8859                = @"iso-8859-1";
        const UInt16    MAX_LOG_LIST            = 12;
        const UInt16    MAX_LOG_TOOLS_LIST      = 37;

        const float     EARTH_G                 = 9.8F; //earth g = 9,8 m/s²
        const Int16     PITCH_LIMIT_DEG         = 90;   //limit pitch for 45°
        const Int16     ROLL_LIMIT_DEG          = 90;   //limit roll for 90°
        const Int16     FULL_CIRCLE_DEG         = 360;

        const Int16     CMPS10_2G               = 128;
        const Int16     MPU6050_2G              = 498;
        const float     GY80_1G                 = 1.1F;
        const float     GY80_2G                 = 2.1F;
        const Int16     GY80_GYRO_MAX           = 360;

        public const UInt32     DATA_CAMERA_NORMAL      = 120800;
        public const UInt32     DATA_CAMERA_PLUS        = 120800;
        public const Int32      DATA_CAMERA_MOD_BREAK   = 5;

        const double    GPS_COORDINAT_1M       = 0.000009; //By Google Maps

        const Int32     TIMER_MAIN_MAX          = 5000; //milisec
        const Int32     TIMER_INTERVAL_MAIN     = 1;
        const Int32     TIMER_INTERVAL_STATUS   = 100;

        const string    NULL_STRING             = @"";

        //=============================== CLASS MEMBER ===============================//
        //...Private    
        private string[]        m_Command       = new string[(UInt16)c_COMMAND_LIST._Count];
        private string[]        m_ErrorString   = new string[(UInt16)ERROR_LIST._Count];
        //private s_CALIB_DATA    m_CalibData     = new s_CALIB_DATA();
    
    #if USE_LOG_SAVED
        private StreamWriter    m_StreamLog;
        private string          m_StreamHeader;
        private UInt32          m_StreamBytesCount;
        private bool            m_StateStreamText;
    #endif

    #if USE_TIME_UPDATE_GRAPH
        private UInt16          m_MainTick;
        private GRAPH_UPDATE    m_GraphState    = new GRAPH_UPDATE();
    #endif

        private bool            m_FirstLaunch = true;
        private DateTime        m_StartTime;
        private UInt16          m_CounterData = 0;
        private string          m_DataLine;
        private UInt16          m_StatusShow;
        private bool            m_StateLoop;
        private bool            m_StateStop;

        private float           m_CurrentLatitude;
        private float           m_CurrentLongitude;
        private UInt16          m_SatelliteActive;

    #if USE_TRACKER_TEST
        private string          m_LatitudeHome;
        private string          m_LongitudeHome;

        private Int32           m_LatitudeMin;  //based on pixel format
        private Int32           m_LongitudeMin; //based on pixel format
        private Int32           m_LatitudeMax;  //based on pixel format
        private Int32           m_LongitudeMax; //based on pixel format
    #endif

        //...Sub Wind Control
        private SerialPort[]    m_Comm;
        private ListBox[]       m_LsLog;
        private Timer[]         m_Timer;

    #if SHOW_DEBUG_VALUE
        public Label            m_LblStatus;
    #else
        private Label           m_LblStatus;
    #endif

        //...Public
        public static Komurindo m_Instance;

        public UInt16           m_CurrentControl;   //Active control
        public Encoding         m_Iso_8859_1;       //Encoder
    
    #if KOMURINDO_PAYLOAD
        public string           m_ImageRawData;     //Image captured
    #endif

        public static string[]  s_GCSCommand = new string[(UInt16)c_GCS_COMMAND_LIST._Count];
        
        //=============================== MAIN INSTANCE ==============================//
        public Komurindo()
        {
            //...Bind to it self
            m_Instance = this;

            //...Init Sub Windows Control
            InitSubWinControl();
            
            //...Command Init
            InitCommand();

            //...Var's
            InitVars();
        }

        ~Komurindo()
        {
            //...RELEASE ALL
            //...Sub Windows Control
            for (UInt16 i = 0; i < (UInt16)c_LEVEL._Count; i++)
            {
                m_Comm[i].Dispose();
            }
            for (UInt16 i = 0; i < (UInt16)TIMER_LIST._Count; i++)
            {
                m_Timer[i].Dispose();
            }
            for (UInt16 i = 0; i < (UInt16)LISTBOX_LIST._Count; i++)
            {
                m_LsLog[i].Dispose();
            }
            m_LblStatus.Dispose();
        }

        //=============================== INIT METHOD ================================//  
        private void InitVars()
        {
            //...Streaming log
        #if USE_LOG_SAVED
            m_StateStreamText       = false;
            m_StreamBytesCount      = 0;
        #endif

            //...Error string
            m_ErrorString[(UInt16)ERROR_LIST.CorruptData]   = @"Corrupt Data";
            m_ErrorString[(UInt16)ERROR_LIST.MissData]      = @"Missed Data";
            m_ErrorString[(UInt16)ERROR_LIST.MissLF]        = @"Missed LF/CR";

            //...Var's
            m_StateStop             = false;
        }
        
        private void InitCommand()
        {
            //...All Contest Type
            m_Command[(UInt16)c_COMMAND_LIST.GetID]                     = @"I";
            m_Command[(UInt16)c_COMMAND_LIST.GetPosition]               = @"L";
            m_Command[(UInt16)c_COMMAND_LIST.GetPressure]               = @"P";
            m_Command[(UInt16)c_COMMAND_LIST.GetHeight]                 = @"H";
            m_Command[(UInt16)c_COMMAND_LIST.CalibHeight]               = @"Y";
            m_Command[(UInt16)c_COMMAND_LIST.CheckModule]               = @"O";
            //...Contest specific
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Launch]            = @"7";
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Stop]              = @"0";
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Exit]              = @"E";
            //...Launch
            m_Command[(UInt16)c_COMMAND_LIST.Arming]                    = @"1";
            m_Command[(UInt16)c_COMMAND_LIST.Disarming]                 = @"2";
            m_Command[(UInt16)c_COMMAND_LIST.TakeOff]                   = @"3";
            m_Command[(UInt16)c_COMMAND_LIST.Parachute]                 = @"4";

            //...Payload, Rocket
        #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
            m_Command[(UInt16)c_COMMAND_LIST.GetAccelero]               = @"A";
            m_Command[(UInt16)c_COMMAND_LIST.GetCompass]                = @"D";
            m_Command[(UInt16)c_COMMAND_LIST.GetGyro]                   = @"G";
            //...Contest specific
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Tracking]          = @"L";
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Trajectory]        = @"T";
        #endif

        //...Payload,
        #if KOMURINDO_PAYLOAD
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Capture]           = @"8";
            m_Command[(UInt16)c_COMMAND_LIST.GetCamera]                 = @"C";
        #endif

        //...Kombat
        #if BALON_ATMOSFER
            m_Command[(UInt16)c_COMMAND_LIST.GetHumidity]               = @"K";
            m_Command[(UInt16)c_COMMAND_LIST.GetTemperature]            = @"T";
            //...Contest specific
            m_Command[(UInt16)c_COMMAND_LIST.Contest_Launch]            = @"5";
            m_Command[(UInt16)c_COMMAND_LIST.Contest_AfterLaunch]       = @"6";
        #endif

            //...GCS
            s_GCSCommand[(UInt16)c_GCS_COMMAND_LIST.ServoTest]          = @"TEST" + Char.ConvertFromUtf32((UInt16)c_CHARACTER.Enter);
            s_GCSCommand[(UInt16)c_GCS_COMMAND_LIST.ServoTrack]         = @"X Y" + Char.ConvertFromUtf32((UInt16)c_CHARACTER.Enter);
            s_GCSCommand[(UInt16)c_GCS_COMMAND_LIST.ServoFaceFront]     = @"FACE" + Char.ConvertFromUtf32((UInt16)c_CHARACTER.Enter);
        }

        private void InitSubWinControl()
        {
            //...Set ISO Encoding
            m_Iso_8859_1 = Encoding.GetEncoding(ISO_8859);

            //...Create comm serial port
            m_Comm = new SerialPort[(UInt16)c_LEVEL._Count];
            for (UInt16 i = 0; i < (UInt16)c_LEVEL._Count; i++)
            {
                m_Comm[i]           = new SerialPort(@"COM1", 9600, Parity.None, 8, StopBits.One);
                m_Comm[i].Encoding  = m_Iso_8859_1;
            }
            
            //...Create List Box
            m_LsLog = new ListBox[(UInt16)LISTBOX_LIST._Count];
            
            //...Timer control
            InitTimerControl();
        }

        private void InitTimerControl()
        {
            //...Create
            m_Timer = new Timer[(UInt16)TIMER_LIST._Count];
            for (UInt16 i = 0; i < (UInt16)TIMER_LIST._Count; i++)
            {
                m_Timer[i]              = new Timer();
                m_Timer[i].Enabled      = false;
            }

            //...Set interval(ms)
            m_Timer[(UInt16)TIMER_LIST.Main].Interval       = TIMER_INTERVAL_MAIN;
            m_Timer[(UInt16)TIMER_LIST.Status].Interval     = TIMER_INTERVAL_STATUS;
            
            //...Add handler
            m_Timer[(UInt16)TIMER_LIST.Main].Tick       += new System.EventHandler(Tmr_Main_Tick);
            m_Timer[(UInt16)TIMER_LIST.Status].Tick     += new System.EventHandler(Tmr_Status_Tick);

            //...Update State
        #if USE_TIME_UPDATE_GRAPH
            m_GraphState.IsNeedUpDate   = new bool[(UInt16)GRAPH_CONTROL._Count];
            m_GraphState.Counter        = new UInt16[(UInt16)GRAPH_CONTROL._Count];

            //...Init
            for (UInt16 i = 0; i < (UInt16)GRAPH_CONTROL._Count; i++)
            {
                m_GraphState.IsNeedUpDate[i] = false;
            }

            //...Constant in ms, note minimum update is 1 ms
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.LineGraph]           = 1 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.LineGraphText]   = 15 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.PitchRoll]           = 5 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.PitchRollText]   = 10 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.Gyro]                = 5 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.GyroText]        = 15 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.Altimeter]           = 10 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.AltimeterText]   = 50 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.Compass]             = 10 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.CompassText]     = 50 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.Humidity]            = 20 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.HumidityText]    = 50 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.Maps]                = 5000 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.MapsText]        = 50 / TIMER_INTERVAL_MAIN;
            m_GraphState.Counter[(UInt16)GRAPH_CONTROL.Tracker]             = 100 / TIMER_INTERVAL_MAIN;
                m_GraphState.Counter[(UInt16)GRAPH_CONTROL.TrackerText]     = 50 / TIMER_INTERVAL_MAIN;
        #endif
        }

        public void BindSubWindowsControl(
            Label lblStatus, ListBox lsLog, ListBox lsTools, ListBox events
            )
        {
            m_LblStatus                             = lblStatus;
            m_LsLog[(UInt16)LISTBOX_LIST.Monitor]   = lsLog;
            m_LsLog[(UInt16)LISTBOX_LIST.Tools]     = lsTools;
            m_LsLog[(UInt16)LISTBOX_LIST.Event]     = events;
        }
        
        //=============================== EVENT HANDLER ================================//
        private void Tmr_Main_Tick(object sender, EventArgs e)
        {
        #if USE_TIME_UPDATE_GRAPH
            //...counter
            m_MainTick++;

            //...FLAG CHANGE's
            for (UInt16 i = 0; i < (UInt16)GRAPH_CONTROL._Count; i++)
            {
                if (m_MainTick % m_GraphState.Counter[i] == 0)
                {
                    m_GraphState.IsNeedUpDate[i] = true;
                }
            }
          
            //...Reset per max second
            if (m_MainTick == (TIMER_MAIN_MAX / TIMER_INTERVAL_MAIN))
            {
                m_MainTick = 0;
            }
        #endif
        }

        private void Tmr_Status_Tick(object sender, EventArgs e)
        {
            //...counter
            m_StatusShow++;

            //...counter check
            if (m_StatusShow >= 10)
            {
                //...reset counter
                m_StatusShow = 0;
                
                //...set text color
                if (m_LblStatus.ForeColor != Color.Black)
                    m_LblStatus.ForeColor = Color.Black;
                
                //...check idle or action
                if (m_StateLoop)
                {
                    m_LblStatus.Text = @"Action";
                }
                else
                {
                    m_LblStatus.Text = @"IDLE";
                }
                
                //...off timer
                m_Timer[(UInt16)TIMER_LIST.Status].Enabled = false;
            }
        }

        //=============================== RAW CALCULATION ===============================//
        //...General
        public static Int32 Calculation_GetInt(string value)
        {
            Int32 _value;
            bool isNumber;

            isNumber = Int32.TryParse(value, out _value);
            if (!isNumber)
            {
                _value = 0;
            }
            return _value;
        }

        public static float Calculation_GetFloat(string value)
        {
            float _value;
            bool isFloat;

            isFloat = float.TryParse(value, out _value);
            if (!isFloat)
            {
                _value = 0;
            }
            return _value;
        }

        //...CMPS10
        public static Int32 CMPS10_Normalize(Int32 value)
        {
            if (value >= 0 && value < 128)
            {
                return value;
            }
            else
            {
                return (value - 255);
            }
        }

        public static Int32 CMPS10_Correction(Int32 oldValue, Int32 currentValue, Int32 limit)
        {
            if (Math.Abs(currentValue - oldValue) > limit)
            {
                return oldValue;
            }
            else
            {
                return currentValue;
            }
        }

        public static UInt16 CMPS10_DegFrom8BitHeading(UInt16 data)
        {
            return (UInt16)((data * FULL_CIRCLE_DEG) / 255); //from 8bit data
        }

        public static Int32 MPU6050_Normalize(Int32 data)
        {
            //...Normalize
            return (Int32)(data - MPU6050_2G);
        }

        //...CIROCOMM GPS
        public void Cirocomm_RawDataDecoder(ref float latitude, ref float longitude, string dataLine)
        {
            //...Get
            if (dataLine[1] == '$' && dataLine[2] == 'G' && dataLine[3] == 'P' && dataLine[4] == 'G' &&
                dataLine[5] == 'G' && dataLine[6] == 'A' && dataLine[7] == ',')
            {
                //...Latitude
                string latString = NULL_STRING;
                try
                {
                    latString = dataLine.Substring(19, 10);
                }
                catch
                {
                    Tools_StatusInfo(@"Error Substring!", true);
                }

                //...Longitude
                string longString = NULL_STRING;
                try
                {
                    longString = dataLine.Substring(31, 11);
                }
                catch
                {
                    Tools_StatusInfo(@"Error Substring!", true);
                }

                //...Get
                float latitudeUp = 0, longitudeUp = 0;
                Cirocomm_LatLongDecoder(ref latitudeUp, ref longitudeUp, latString, longString);

                //...Return
                latitude = latitudeUp;
                longitude = longitudeUp;
            }
        }

        public void Cirocomm_LatLongDecoder(ref float latitude, ref float longitude, string latString, string longString)
        {
            //...Latitude
            string sLatFirst = NULL_STRING, sLatSecond = NULL_STRING;
            char cLatPos = 'S';
            try
            {
                sLatFirst = latString.Substring(0, 2); //First Number
                sLatSecond = latString.Substring(2, 7); //Second Number
                cLatPos = latString[9]; //Pole
            }
            catch
            {
                Tools_StatusInfo(@"Error Substring!", true);
            }

            float fLatFirst = Calculation_GetFloat(sLatFirst);
            float fLatSecond = Calculation_GetFloat(sLatSecond);

            latitude = fLatFirst + (fLatSecond / 60);
            if (cLatPos == 'S')
            {
                latitude *= -1;
            }

            //...Longitude
            string sLongFirst = NULL_STRING, sLongSecond = NULL_STRING;
            char cLongPos = 'W';
            try
            {
                sLongFirst = longString.Substring(0, 3); //First Number
                sLongSecond = longString.Substring(3, 7); //Second Number
                cLongPos = longString[10]; //Pole
            }
            catch
            {
                Tools_StatusInfo(@"Error Substring!", true);
            }

            float fLongFirst = Calculation_GetFloat(sLongFirst);
            float fLongSecond = Calculation_GetFloat(sLongSecond);

            longitude = fLongFirst + (fLongSecond / 60);
            if (cLongPos == 'W')
            {
                longitude *= -1;
            }
        }

        //=============================== SAVE & CLEAR ================================//
     #if KOMURINDO_PAYLOAD
        private void SaveCapturedImage()
        {
            string path = Utils.MyPath.s_Saved + @"Captured.jpg";
        #if USE_LOG_SAVED
            //...Set path
            m_StreamHeader = m_StreamHeader.Replace(@":", @"_");
            m_StreamHeader = m_StreamHeader.Replace(@"\", @"_");
            m_StreamHeader = m_StreamHeader.Replace(@"/", @"_");

            path = Utils.MyPath.s_Saved + m_StreamHeader;
            m_StreamLog = new StreamWriter(path + @".log", false, m_Iso_8859_1); //NEW with encoding

            //...Write header
            m_StreamLog.WriteLine(m_StreamHeader);
            m_StreamLog.WriteLine(NULL_STRING);

            //...Write raw image data
            m_StreamLog.WriteLine(m_ImageRawData);
            
            //...Footer
            string footer = NULL_STRING;
            footer += (@"[STOP]@" + DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());
            footer += (@", Received : " + m_ImageRawData.Length.ToString() + @" bytes data");

            //...Write footer
            m_StreamLog.WriteLine(NULL_STRING);
            m_StreamLog.WriteLine(footer);

            //...Close stream
            m_StreamLog.Close();
        #endif

            //...Save Image
            MyGraph.m_Instance.SaveImage(path);
        }
    #endif

    #if USE_LOG_SAVED
        private void LogSavedInitHeader(string title, bool append)
        {
            #if USE_LOG_SAVED_HEADER
                append = true; // Or KEEP
            #else
                append = false; //
            #endif

            //...Start stream
            if (!m_StateStreamText) //NEW
            {
                //...Reset counter
                m_StreamBytesCount = 0;

                //...Header
                m_StreamHeader = NULL_STRING;
                m_StreamHeader += title + @"@";
                m_StreamHeader += (DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

                //...Path
                //...membersihkan dari illegal character
                m_StreamHeader = m_StreamHeader.Replace(@":", @"_");
                m_StreamHeader = m_StreamHeader.Replace(@"\", @"_");
                m_StreamHeader = m_StreamHeader.Replace(@"/", @"_");

                string path = Utils.MyPath.s_Saved + m_StreamHeader + @".log";
                m_StreamLog = new StreamWriter(path, false); //NEW

                if (append)
                {
                    m_StreamLog.WriteLine(m_StreamHeader);
                }                

                //...Event log
                Tools_AddEventInfo(m_StreamHeader);
            }
            else //APPEND
            {
                //...Path
                //...membersihkan dari illegal character
                m_StreamHeader = m_StreamHeader.Replace(@":", @"_");
                m_StreamHeader = m_StreamHeader.Replace(@"\", @"_");
                m_StreamHeader = m_StreamHeader.Replace(@"/", @"_");

                string path = Utils.MyPath.s_Saved + m_StreamHeader + @".log";
                m_StreamLog = new StreamWriter(path, true); //APPEND

                //...Event log
                Tools_AddEventInfo(@"RESUME : " + m_StreamHeader);
            }

            m_StreamLog.WriteLine(NULL_STRING);
        }

        private void LogSavedAppendFooter(bool append)
        {
            #if USE_LOG_SAVED_HEADER
                append = true; // Or KEEP
            #else
                append = false; //
            #endif

            //...Footer
            string footer = NULL_STRING;
            footer += (@"[STOP]@" + DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());
            footer += (@", Received : " + m_StreamBytesCount.ToString() + @" bytes data");

            //...Event log
            Tools_AddEventInfo(footer);

            try
            {
                //...Write footer
                if (append)
                {
                    m_StreamLog.WriteLine(NULL_STRING);
                    m_StreamLog.WriteLine(footer);
                }  
                
                //...Close stream
                m_StreamLog.Close();
            }
            catch
            {
                Tools_StatusInfo(@"Error Stream Text", true);
            }
        }
    #endif

        private void ClearUI()
        {
            //...Graph Clear
            MyGraph.m_Instance.ClearAllGraph();

            //...List Clear
            for (UInt16 i = 0; i < (UInt16)LISTBOX_LIST._Count; i++)
            {
                m_LsLog[i].Items.Clear();
            }
        }

        private void ProcessedRemainBuffer(byte level)
        {
            Int32 data = 0;

            //...UNPROCESSED data > Saved, Log, but No Graph
            while (m_Comm[level].IsOpen && m_Comm[level].BytesToRead > 0)
            {
                //...Append data
                data = m_Comm[level].ReadChar();

                if (data == (Int32)c_CHARACTER.Enter) //CHECK FOR LF
                {
                    m_CounterData++; // Increase counter

                    //...Append with Date Time log
                    string log = Tools_AppendFormattedLog(m_DataLine, m_CounterData, m_StartTime);

                    //...Save log
                #if USE_LOG_SAVED
                    m_StreamLog.WriteLine(log);
                #endif

                    //...Log
                    Tools_RollListReceiver(log, MAX_LOG_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Monitor]);

                    //...Reset & Clear
                    m_DataLine = NULL_STRING;
                }
                else
                {
                    m_DataLine += Char.ConvertFromUtf32(data);
                }
            }
        }

        //=============================== TOOLS ======================================//
        //...Status
        private void setUpTime() // AKA Launch Time
        {
            if (m_FirstLaunch) m_StartTime = DateTime.Now;
            m_FirstLaunch = false;
        }

        private void Tools_StatusInfo(string text, bool isErrorStatus)
        {
            //...Check what status
            if (isErrorStatus) //error
            {
                if (m_LblStatus.ForeColor != Color.Red)
                    m_LblStatus.ForeColor = Color.Red;
            }
            else //normal
            {
                if (m_LblStatus.ForeColor != Color.Black)
                    m_LblStatus.ForeColor = Color.Black;
            }

            //...Set text
            m_LblStatus.Text = text;

            //...Set timer
            m_Timer[(UInt16)TIMER_LIST.Status].Enabled = true;
        }

        private void Tools_AddEventInfo(string text)
        {
            m_LsLog[(UInt16)LISTBOX_LIST.Event].Items.Add(text);
            //m_LsLog[(UInt16)LISTBOX_LIST.Event].SelectedIndex(m_LsLog[(UInt16)LISTBOX_LIST.Event].Items.Count - 1);
        }

        //...ROLL LOG
        private void Tools_RollListReceiver(string list, UInt16 itemCount, ListBox ls)
        {
            //...check if maximum
            if (ls.Items.Count >= itemCount)
            {
                ls.Items.RemoveAt(0);
            }

            //...add to list
            ls.Items.Add(list);
        }

        //...PARSER
        public static string Tools_ParserTxt(string pathTxtFile)
        {
            string strRead;
            try
            {
                using (StreamReader sr = File.OpenText(pathTxtFile))
                {
                    strRead = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch
            {
                strRead = @"[ERROR Parsing]";
            }

            return strRead;
        }

        public static void Tools_ParserConfig(string configFile)
        {
            //...W.I.P
        }

        public static string Tools_AppendFormattedLogRocket(string data, UInt16 counter, DateTime uptime)
        {
            string time = NULL_STRING;

            DateTime now = DateTime.Now;
            time = @"" + ((now.Hour > 9) ? @"" + now.Hour : @"0" + now.Hour) +
                   @"" + ((now.Minute > 9) ? @"" + now.Minute : @"0" + now.Minute) +
                   @"" + ((now.Second > 9) ? @"" + now.Second : @"0" + now.Second);

            TimeSpan diff = now - uptime;
            string up = @"" + (UInt16)diff.TotalSeconds + @"." + diff.TotalMilliseconds.ToString().Substring(0, 2);

            return @"$R," + counter + @","
                          + time + @","
                          + up + @","
                          + data;
        }

        public static string Tools_AppendFormattedLog(string data, UInt16 counter, DateTime uptime)
        {
            string time = NULL_STRING;
            
            DateTime now = DateTime.Now;
            time = @"" + ((now.Hour > 9) ? @"" + now.Hour : @"0" + now.Hour) +
                   @"" + ((now.Minute > 9) ? @"" + now.Minute : @"0" + now.Minute) +
                   @"" + ((now.Second > 9) ? @"" + now.Second : @"0" + now.Second);

            TimeSpan diff = now - uptime;
            string up = @"" + (UInt16) diff.TotalSeconds + @"." + diff.TotalMilliseconds.ToString().Substring(0, 2);

            String[] datas = data.Split(' ');
            String pitch    = datas[(byte)PAYLOAD_LAUNCH_DATAPOS.AccX];
            String yaw      = datas[(byte)PAYLOAD_LAUNCH_DATAPOS.AccY];
            String roll     = datas[(byte)PAYLOAD_LAUNCH_DATAPOS.AccZ];
            try
            {
                float pitchUp       = Calculation_GetFloat(pitch) * (-1); // Get negative value
                float rollUp        = Calculation_GetFloat(yaw);
                float yawUp         = Calculation_GetFloat(roll);

                float pitchDeg = 0, rollDeg = 0, yawDeg = 0;

                //...Limit pitch, roll, yaw
                pitchDeg = -(MyGraph.Scaling(pitchUp, PITCH_LIMIT_DEG, GY80_2G / 2));
                rollDeg = MyGraph.Scaling(rollUp, ROLL_LIMIT_DEG, GY80_2G / 2);
                yawDeg = MyGraph.Scaling(yawUp, ROLL_LIMIT_DEG, GY80_2G / 2); // using ROLL_LIMIT_DEG = 90.0F

                pitchDeg = (float)Math.Round((decimal)pitchDeg, 0);
                rollDeg = (float)Math.Round((decimal)rollDeg, 0);
                yawDeg = (float)Math.Round((decimal)yawDeg, 0);

                pitch = @"" + pitchDeg;
                roll = @"" + rollDeg;
                yaw = @"" + yawDeg;
            }
            catch
            {
            }
            data = data.Replace(@" ", @",");

            return @"$R," + counter + @","
                          + time + @","
                          + up + @","
                          + roll + @"," + pitch + @"," + yaw + @","
                          + data;
        }

        public static string Tools_AppendFormattedLogLocation(string data, UInt16 counter, DateTime uptime)
        {
            string time = NULL_STRING;

            DateTime now = DateTime.Now;
            time = @"" + ((now.Hour > 9) ? @"" + now.Hour : @"0" + now.Hour) +
                   @"" + ((now.Minute > 9) ? @"" + now.Minute : @"0" + now.Minute) +
                   @"" + ((now.Second > 9) ? @"" + now.Second : @"0" + now.Second);

            TimeSpan diff = now - uptime;
            string up = @"" + (UInt16)diff.TotalSeconds + @"." + diff.TotalMilliseconds.ToString().Substring(0, 2);

            String[] datas = data.Split(' ');
            String satellite = datas[(byte)PAYLOAD_TRACKING_DATAPOS.Satellite];
            try
            {
                Int32 satelliteCount = Int32.Parse(datas[(byte)PAYLOAD_TRACKING_DATAPOS.Satellite]);
                satellite = @"" + ((satelliteCount > 9) ? @"" + satelliteCount : @"0" + satelliteCount);
            }
            catch
            {
            }

            return @"$G," + time + @","
                          + datas[(byte)PAYLOAD_TRACKING_DATAPOS.Longitude] + @","
                          + datas[(byte)PAYLOAD_TRACKING_DATAPOS.Latitude] + @","
                          + datas[(byte)PAYLOAD_TRACKING_DATAPOS.Height] + @","
                          + @"1," /*GPS Quality*/ + satellite + @","
                          + datas[(byte)PAYLOAD_TRACKING_DATAPOS.Speed] + @","
                          + datas[(byte)PAYLOAD_TRACKING_DATAPOS.Heading];
        }

        public static void Tools_AddCommList(ComboBox comboComm, bool auto)
        {
            comboComm.Items.Clear();
            if (!auto)
            {
                const byte commCount = 10;
                for (byte i = 1; i <= commCount; i++)
                {
                    comboComm.Items.Add(@"COM" + i.ToString());
                }
            }
            else
            {
                foreach (string item in System.IO.Ports.SerialPort.GetPortNames())
                {
                    comboComm.Items.Add(item);
                }
            }
        }

        public static void Tools_AddBaudrateList(ComboBox comboBaud)
        {
            //...Baudrate
            const byte baudCount = 5;
            UInt32[] baud = new UInt32[baudCount];
            baud[0] = 4800;
            baud[1] = 9600;
            baud[2] = 38400;
            baud[3] = 57600;
            baud[4] = 115200;

            //...Add
            comboBaud.Items.Clear();
            for (byte i = 0; i < baudCount; i++)
            {
                comboBaud.Items.Add(baud[i].ToString());
            }
        }

        //=============================== ACTION METHOD ================================//
        //...ACTION
        public void AdditionalAction(UInt16 index)
        {
            String command = @"*"; // ERROR COMMAND
            switch (index)
            {
                case (UInt16)c_EXTERNAL_BUTTON.Arming:
                    command = m_Command[(UInt16)c_COMMAND_LIST.Arming];
                    Tools_StatusInfo(@"ARMING", false);
                    break;
                case (UInt16)c_EXTERNAL_BUTTON.Disarming:
                    command = m_Command[(UInt16)c_COMMAND_LIST.Disarming];
                    Tools_StatusInfo(@"DIS-ARMING", false);
                    break;
                case (UInt16)c_EXTERNAL_BUTTON.Parachute:
                    command = m_Command[(UInt16)c_COMMAND_LIST.Parachute];
                    Tools_StatusInfo(@"PARACHUTE", false);
                    break;
                case (UInt16)c_EXTERNAL_BUTTON.TakeOff:
                    setUpTime(); // set launch time
                    command = m_Command[(UInt16)c_COMMAND_LIST.TakeOff];
                    Tools_StatusInfo(@"TAKE-OFF", false);
                    break;
                default:
                    break;
            }

            //...Send Command
            try
            {
                m_Comm[(UInt16)c_LEVEL.Main].Write(command);
            }
            catch
            {
                Tools_StatusInfo(@"Error Launch Command", true);
            }
        }

        public void Action(UInt16 index)
        {
            //...Clear buffer
            if (m_Comm[(byte)c_LEVEL.Main].IsOpen)
            {
                m_Comm[(byte)c_LEVEL.Main].ReadExisting();
            }

            switch (index)
            {
                //======================================================//
                //...PLAY
                case (UInt16)c_RUN_CONTROL_LIST.Play:
                #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET || BALON_ATMOSFER
                    if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Attitude)
                    {
                        if (!CommRead(m_Command[(UInt16)c_COMMAND_LIST.Contest_Launch], (byte)c_LEVEL.Main))
                        {
                            return;
                        }
                    }
                #endif

                #if KOMURINDO_PAYLOAD
                    if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Camera)
                    {
                        if (!CommRead(m_Command[(UInt16)c_COMMAND_LIST.GetCamera], (byte)c_LEVEL.Main))
                        {
                            return;
                        }
                    }
                    else if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Position)
                    {
                        if (!CommRead(m_Command[(UInt16)c_COMMAND_LIST.Contest_Tracking], (byte)c_LEVEL.Main))
                        {
                            return;
                        }
                    }
                    else if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Trajectory)
                    {
                        if (!CommRead(m_Command[(UInt16)c_COMMAND_LIST.Contest_Trajectory], (byte)c_LEVEL.Main))
                        {
                            return;
                        }
                    }
                #endif

                #if BALON_ATMOSFER
                    if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Atmosfer)
                    {
                        if (!CommRead(m_Command[(UInt16)c_COMMAND_LIST.Contest_Launch], (byte)c_LEVEL.Main))
                        {
                            return;
                        }
                    }
                #endif

                    break;
                //======================================================//

                //======================================================//
                //...PAUSE
                case (UInt16)c_RUN_CONTROL_LIST.Suspend:
                #if USE_LOG_SAVED
                    //...Keep stream
                    m_StateStreamText = true;
                #endif

                    //...Stop Payload to sending data 
                    if (!CommStop((byte)c_LEVEL.Main))
                    {
                        return;
                    }
                    break;
                //======================================================//

                //======================================================//
                //...STOP
                case (UInt16)c_RUN_CONTROL_LIST.Stop:
                #if USE_LOG_SAVED
                    //...Stop stream
                    m_StateStreamText = false;
                #endif

                    //...Stop Payload to sending data
                    if (!CommStop((byte)c_LEVEL.Main))
                    {
                        return;
                    }

                    //...Set state
                    m_StateStop = true;
                    break;
                //======================================================//

                //======================================================//
                //...REFRESH
                case (UInt16)c_RUN_CONTROL_LIST.Refresh:
                    CommRefresh();
                    break;
                //======================================================//

                //======================================================//
                //...SAVE
                case (UInt16)c_RUN_CONTROL_LIST.Save:
                #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET || BALON_ATMOSFER
                    if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Attitude)
                    {
                    #if USE_LOG_SAVED
                        //...Stop stream
                        m_StateStreamText = false;
                    #endif
                    }
                #endif

                #if KOMURINDO_PAYLOAD
                    if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Camera)
                    {
                        SaveCapturedImage();
                    }
                    else if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Position)
                    {
                    #if USE_LOG_SAVED
                        //...Stop stream
                        m_StateStreamText = false;
                    #endif
                    }
                #endif

                #if BALON_ATMOSFER
                    else if (m_CurrentControl == (UInt16)c_CONTROL_LIST.Atmosfer)
                    {
                    #if USE_LOG_SAVED
                        //...Stop stream
                        m_StateStreamText = false;
                    #endif
                    }
                #endif
                    break;
                //======================================================//

                //======================================================//
                //...ENGINE
                case (UInt16)c_RUN_CONTROL_LIST.Engine:
                    // TODO
                    break;
                //======================================================//

                //======================================================//
                //...CLEAR
                case (UInt16)c_RUN_CONTROL_LIST.Clear:
                #if USE_LOG_SAVED
                    //...Stop stream
                    m_StateStreamText = false;
                #endif

                    m_CounterData = 0;
                    m_FirstLaunch = true;

                    //...ClearUI();
                    ClearUI();
                    break;
                //======================================================//

                default:
                    break;
            }
        }

        public void ActionGeneral(UInt16 index)
        {
            //...Set uart level
            byte level = (byte)c_LEVEL.Main;

            //...Clear buffer
            m_Comm[level].ReadExisting();

            //...Send Command
            try
            {
                m_Comm[level].Write(m_Command[index]);    
            }
            catch
            {
                Tools_StatusInfo(@"Error Send/Read!", true);
                return;
            }

            //...If Command Stop/Exit, don't process 
            if ((index == (UInt16)c_COMMAND_LIST.Contest_Stop) || (index == (UInt16)c_COMMAND_LIST.Contest_Exit))
            {
                return;
            }

            //...Try Process
            OnReceive_General(index, level);
        }

        public void PositionTracker(bool success, string command)
        {
            if (success)
            {
                //...Set uart level
                byte level = (byte)c_LEVEL.Second;

                //...Send Command
                try
                {
                    m_Comm[level].Write(command);
                }
                catch
                {
                    Tools_StatusInfo(@"Error Send!", true);
                }
            }
            else
            {
                Tools_StatusInfo(command, true);
            }
        }

    #if KOMURINDO_PAYLOAD
        public void UpdateCapturedPercentage(bool undeterminate)
        {
            UpdateCapturedPercentage((UInt32)m_ImageRawData.Length, undeterminate);
        }

        public void UpdateCapturedPercentage(UInt32 val, bool undeterminate)
        {
            MyGraph.m_Instance.DrawImagePercentage(val, DATA_CAMERA_NORMAL, undeterminate);
        }
    #endif

        public void UpdateMaps(float latitude, float longitude, UInt16 satellite, UInt16 zoom) // Using textbox data
        {
            m_CurrentLatitude = latitude;
            m_CurrentLongitude = longitude;
            m_SatelliteActive = satellite;

            UpdateMaps(zoom);
        }

        public void UpdateMaps(UInt16 zoom) // Using sensor
        {
            MyGraph.m_Instance.ParseMaps(m_CurrentLatitude, m_CurrentLongitude, zoom, m_SatelliteActive, (byte)c_GRAPH_UPDATE_MODE.Both);
        }

    #if USE_TRACKER_TEST
        public void SetTrackerCoordinat(Int32[] latitude, Int32[] longitude)
        {
            //...Var;s
            Int32[] sLatitude     = new Int32[(UInt16)c_COORDINAT_TEST._Count];
            Int32[] sLongitude    = new Int32[(UInt16)c_COORDINAT_TEST._Count];
            Int32   minLat, maxLat, minLong, maxLong;

            //...Store
            for (UInt16 i = 0; i < (UInt16)c_COORDINAT_TEST._Count; i++)
            {
                //...Save
                sLatitude[i]  = Math.Abs(latitude[i]);
                sLongitude[i] = Math.Abs(longitude[i]);

            #if SHOW_DEBUG_VALUE
                //...Check
                m_LsLog[(UInt16)LISTBOX_LIST.Monitor].Items.Add(sLatitude[i].ToString() + @" " + sLongitude[i].ToString());
            #endif
            }

            //...Define min-max scale
            minLat      = sLatitude[1];
            maxLat      = sLatitude[1];
            minLong     = sLongitude[1];
            maxLong     = sLongitude[1];

            for (UInt16 i = 1; i < (UInt16)c_COORDINAT_TEST._Count - 1; i++)
            {
                //...Update min-max
                if (sLatitude[i] < minLat)
                {
                    minLat = sLatitude[i];
                }
                if (sLongitude[i] < minLong)
                {
                    minLong = sLongitude[i];
                }
                if (sLatitude[i] > maxLat)
                {
                    maxLat = sLatitude[i];
                }
                if (sLongitude[i] > maxLong)
                {
                    maxLong = sLongitude[i];
                }         
            }

        #if SHOW_DEBUG_VALUE
            //...Check
            m_LsLog[(UInt16)LISTBOX_LIST.Monitor].Items.Add(minLat.ToString() + @" " + minLong.ToString() + @" " +
                                                            maxLat.ToString() + @" " + maxLong.ToString());
        #endif

            //...Save scale
            m_LatitudeMin   = minLat;
            m_LongitudeMin  = minLong;
            m_LatitudeMax   = maxLat - minLat;
            m_LongitudeMax  = maxLong - minLong;

        #if SHOW_DEBUG_VALUE
            //...Check
            m_LsLog[(UInt16)LISTBOX_LIST.Monitor].Items.Add(m_LatitudeMax.ToString() + @" " + m_LongitudeMax.ToString());

        #endif

            //...Done
            Tools_StatusInfo(@"Set Coordinat Done", false);
        }

        public void TestTracker(Int32 latitude, Int32 longitude)
        {
            //...Track
            MyGraph.m_Instance.Tracker((latitude - m_LatitudeMin), (longitude - m_LongitudeMin),
                m_LatitudeMax, m_LongitudeMax, 0, 0, (byte)c_GRAPH_UPDATE_MODE.Both);
        }

        public void SetHomePosition(string latitude, string longitude, Int32 scale)
        {
            //...Var's
            double[] borderLat      = new double[4];
            double[] borderLong     = new double[4];

            borderLat[0] = (double)(Calculation_GetFloat(latitude) - (scale * GPS_COORDINAT_1M));
            borderLat[1] = (double)(Calculation_GetFloat(latitude) - (scale * GPS_COORDINAT_1M));
            borderLat[2] = (double)(Calculation_GetFloat(latitude) + (scale * GPS_COORDINAT_1M));
            borderLat[3] = (double)(Calculation_GetFloat(latitude) + (scale * GPS_COORDINAT_1M));

            borderLong[0] = (double)(Calculation_GetFloat(longitude) - (scale * GPS_COORDINAT_1M));
            borderLong[1] = (double)(Calculation_GetFloat(longitude) + (scale * GPS_COORDINAT_1M));
            borderLong[2] = (double)(Calculation_GetFloat(longitude) - (scale * GPS_COORDINAT_1M));
            borderLong[3] = (double)(Calculation_GetFloat(longitude) + (scale * GPS_COORDINAT_1M));

            for (UInt16 i = 0; i < 4; i++)
            {
                m_LsLog[2].Items.Add(borderLat[i].ToString() + @", " + borderLong[i].ToString());
            }
        }
    #endif

        //=============================== COMM PORT ====================================//        
        //...Setting
        public void CommSetParameter(byte level)
        {
            try
            {
                //...Comm
                m_Comm[level].PortName = (level == (byte)c_LEVEL.Main ?
                    Utils.MyConfig.s_CommMain : Utils.MyConfig.s_CommSecond);

                //...Baudrate
                m_Comm[level].BaudRate = (level == (byte)c_LEVEL.Main ?
                    (Int32)Utils.MyConfig.s_CommMainBaud : (Int32)Utils.MyConfig.s_CommSecondBaud);
            }
            catch
            {
                Tools_StatusInfo(@"Wrong Parameter!", true);
            }
        }

        //...Common
        public bool CommOpen(byte level)
        {
            try
            {
                m_Comm[level].Open();
                Tools_StatusInfo(@"Open Port!", false);
                return true;
            }
            catch
            {
                Tools_StatusInfo(@"Error Open Port!", true);
                return false;
            }
        }

        public bool CommClose(byte level)
        {
            try
            {
                m_Comm[level].Close();
                Tools_StatusInfo(@"Close Port!", false);
                return true;
            }
            catch
            {
                Tools_StatusInfo(@"Error Close Port!", true);
                return false;
            }
        }

        public void CommRefresh()
        {
            //...Stop loop
            m_StateLoop = false;
        }

        public bool CommRead(string command, byte level)
        {
            //...Send Command
            try
            {
               m_Comm[level].Write(command);
            }
            catch
            {
                Tools_StatusInfo(@"Error Read/Send!", true);
                return false;
            }

            //...Proceed Attitude - Atmosfer
            if (command.CompareTo(m_Command[(UInt16)c_COMMAND_LIST.Contest_Launch]) == 0)
            {
                OnReceive_ContestLaunch();
            }
        #if KOMURINDO_PAYLOAD
            //...Proceed Camera
            else if (command.CompareTo(m_Command[(UInt16)c_COMMAND_LIST.GetCamera]) == 0)
            {
                OnReceive_PayloadCapture((byte)c_LEVEL.Main);
            }
            //...Proceed Position
            else if (command.CompareTo(m_Command[(UInt16)c_COMMAND_LIST.Contest_Tracking]) == 0)
            {
                OnReceive_PayloadTracking((byte)c_LEVEL.Main);
            }
            //...Proceed Trajectory
            else if (command.CompareTo(m_Command[(UInt16)c_COMMAND_LIST.Contest_Trajectory]) == 0)
            {
                OnReceive_PayloadTrajectory((byte)c_LEVEL.Main);
            }
        #endif

            return true;
        }

        public bool CommStop(byte level)
        {
            try
            {
                //...Stop command to payload
                m_Comm[level].Write(m_Command[(UInt16)c_COMMAND_LIST.Contest_Stop]);

                //...Stop loop
                m_StateLoop = false;

                Tools_StatusInfo(@"Stop!", false);
                return true;
            }
            catch
            {
                Tools_StatusInfo(@"Error Stop!", true);
                return false;
            }
        }  

        //=============================== RECEIVER =====================================//
        //...RECEIVE DATA
        //...General Receiever : Accelero, Gyro, Compass, Pressure, Height, Position 
        private void OnReceive_General(UInt16 checker, byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Receive Data", false);
            
            //...used & initial var's
            Int32 data          = 0;
            string dataLine     = @"";
            bool isNeedDelay    = true;
            m_StateLoop         = true;
            
            //...General Var's
            string  str1st      = @"",  str2nd      = @"",  str3rd  = @"";
            float   float1st    = 0,    float2nd    = 0;
            Int32   int1st      = 0,    int2nd      = 0,    int3rd  = 0;
            
            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    data = m_Comm[level].ReadChar();
                    if (data == (Int32)c_CHARACTER.Enter)
                    {
                        m_CounterData++; // Increase counter

                        //...Append with Date Time log
                        string log = Tools_AppendFormattedLog(dataLine, m_CounterData, m_StartTime);

                        //...Log
                        Tools_RollListReceiver(log, MAX_LOG_TOOLS_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Tools]);

                        //...Proceed as command
                        //...Compass
                        if (checker == (UInt16)c_COMMAND_LIST.GetCompass)
                        {
                            //...Get
                            try
                            {
                                str1st = dataLine.Substring(0, 3);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                            int1st = Calculation_GetInt(str1st);

                            //...Draw Graph
                            MyGraph.m_Instance.Heading((UInt16)int1st, (byte)c_GRAPH_UPDATE_MODE.Both);
                        }

                        //...Accelero
                    #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
                        else if (checker == (UInt16)c_COMMAND_LIST.GetAccelero)
                        {
                            //...Get
                            try
                            {
                                str1st = dataLine.Substring(0, 3);
                                str2nd = dataLine.Substring(4, 3);
                                str3rd = dataLine.Substring(8, 3);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                        #if USE_MPU6050_ACCELERO
                            int1st      = MPU6050_Normalize(Calculation_GetInt(str1st));
                            int2nd      = MPU6050_Normalize(Calculation_GetInt(str2nd));
                            int3rd      = MPU6050_Normalize(Calculation_GetInt(str3rd));
                        #elif USE_GY80_ACCELERO
                            int1st      = Calculation_GetInt(str1st);
                            int2nd      = Calculation_GetInt(str2nd);
                            int3rd      = Calculation_GetInt(str3rd);
                        #else
                            int1st      = CMPS10_Normalize(Calculation_GetInt(str1st));
                            int2nd      = CMPS10_Normalize(Calculation_GetInt(str2nd));
                            int3rd      = CMPS10_Normalize(Calculation_GetInt(str3rd));
                        #endif

                             //...MPU6050, CMPS10 used var's
                            Int16 gSensor;
                            Int32 pitch, roll;
                        #if !USE_MPU6050_ACCELERO
                            gSensor     = CMPS10_2G;
                            pitch       = int2nd; //based on the Sensor placement
                            roll        = int1st; //based on the Sensor placement
                        #else
                            gSensor     = MPU6050_2G;
                            pitch       = -int1st; //based on the Sensor placement
                            roll        = -int2nd; //based on the Sensor placement
                        #endif

                            //...Graph
                            MyGraph.m_Instance.LineGraph(int1st, int2nd, int3rd, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.PitchRoll(pitch, roll, PITCH_LIMIT_DEG, ROLL_LIMIT_DEG, gSensor, (byte)c_GRAPH_UPDATE_MODE.Both);
                        }

                        //...Gyro
                        else if(checker == (UInt16)c_COMMAND_LIST.GetGyro)
                        {
                            //...Get
                            try
                            {
                                str1st = dataLine.Substring(0, 3);
                                str2nd = dataLine.Substring(4, 3);
                                str3rd = dataLine.Substring(8, 3);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                            int1st = MPU6050_Normalize(Calculation_GetInt(str1st));
                            int2nd = MPU6050_Normalize(Calculation_GetInt(str2nd));
                            int3rd = MPU6050_Normalize(Calculation_GetInt(str3rd));

                            //...Draw Graph
                            MyGraph.m_Instance.Gyro(int1st, int2nd, int3rd, FULL_CIRCLE_DEG, MPU6050_2G, (byte)c_GRAPH_UPDATE_MODE.Both);
                        }
                    #endif

                        //...Pressure
                        else if (checker == (UInt16)c_COMMAND_LIST.GetPressure)
                        {
                            //...Get
                            try
                            {
                                str1st = dataLine.Substring(0, 6);
                                str2nd = dataLine.Substring(7, 3);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                            int1st = Calculation_GetInt(str1st);
                            int2nd = Calculation_GetInt(str2nd);

                            //...Draw Graph
                            MyGraph.m_Instance.Altimeter(int1st, int2nd, true, (byte)c_GRAPH_UPDATE_MODE.Both);
                        }

                        //...Position
                        else if (checker == (UInt16)c_COMMAND_LIST.GetPosition)
                        {
                            try
                            {
                                str1st = dataLine.Substring(0, 10);
                                str2nd = dataLine.Substring(11, 11);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                            Cirocomm_LatLongDecoder(ref float1st, ref float2nd, str1st, str2nd);

                            //...Draw Graph
                            MyGraph.m_Instance.ParseMaps(float1st, float2nd, 0, 0, (byte)c_GRAPH_UPDATE_MODE.Text);
                        }

                        //...Humidity
                    #if BALON_ATMOSFER || KOMURINDO_ROCKET
                        else if (checker == (UInt16)c_COMMAND_LIST.GetHumidity)
                        {
                            //...Get
                            try
                            {
                                str1st = dataLine.Substring(0, 3);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                            int1st = Calculation_GetInt(str1st);

                            //...Draw Graph
                            //...W.I.P
                        }

                        //...Temperature
                        else if (checker == (UInt16)c_COMMAND_LIST.GetTemperature)
                        {
                            //...Get
                            try
                            {
                                str1st = dataLine.Substring(0, 3);
                            }
                            catch
                            {
                                Tools_StatusInfo(@"Error Substring!", true);
                            }

                            //...Convert Values
                            int1st = Calculation_GetInt(str1st);

                            //...Draw Graph
                            //...W.I.P
                        }
                    #endif

                        //...Reset & Clear
                        dataLine = @"";
                    }
                    else
                    {
                        dataLine = dataLine + Char.ConvertFromUtf32(data);
                    }
                }
                else
                {
                    isNeedDelay = true;
                }

                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }
        }

        //...RECEIVER
        private void OnReceive_ContestLaunch()
        {
            setUpTime(); // set launch time

            //...Set uart level
            byte level = (byte)c_LEVEL.Main;

        #if KOMURINDO_PAYLOAD
            OnReceive_PayloadLaunch(level);
        #endif
        #if KOMURINDO_ROCKET
            OnReceive_RocketLaunch(level);
        #endif
        #if BALON_ATMOSFER
            OnReceive_BalonLaunch(level);
        #endif
        }

        //...Launch Payload Receiver
        private void OnReceive_PayloadLaunch(byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Payload Launch!", false);
            
            //...used & initial var's
            Int32 data          = 0;
            bool isNeedDelay    = true;
            m_StateLoop         = true;
            bool errorHappen    = false;

            //...Enabled timer
        #if USE_TIME_UPDATE_GRAPH
            m_MainTick                                  = 0;
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled    = true;
        #endif

        #if USE_LOG_SAVED
            LogSavedInitHeader(@"[PAYLOAD_LAUNCH]", true);
        #endif

            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    data = m_Comm[level].ReadChar();
                    if (data == (Int32)c_CHARACTER.Enter) //CHECK FOR LF
                    {
                        m_CounterData++; // Increase counter

                        //...Append with Date Time log
                        string log = Tools_AppendFormattedLog(m_DataLine, m_CounterData, m_StartTime);

                        //...Save log
                    #if USE_LOG_SAVED
                        m_StreamLog.WriteLine(log);
                    #endif

                        //...Log
                        Tools_RollListReceiver(log, MAX_LOG_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Monitor]);

                        //...Split
                        string xAcc = @"", yAcc = @"", zAcc = @"", xGyro = @"", yGyro = @"", zGyro = @"";

                        try
                        {
                            string[] dataSplit = m_DataLine.Split(' ');
                            if (dataSplit.Length == (UInt16)PAYLOAD_LAUNCH_DATAPOS._Count)
                            {
                                xAcc = dataSplit[(UInt16)PAYLOAD_LAUNCH_DATAPOS.AccX];
                                yAcc = dataSplit[(UInt16)PAYLOAD_LAUNCH_DATAPOS.AccY];
                                zAcc = dataSplit[(UInt16)PAYLOAD_LAUNCH_DATAPOS.AccZ];
                                xGyro = dataSplit[(UInt16)PAYLOAD_LAUNCH_DATAPOS.GyroX];
                                yGyro = dataSplit[(UInt16)PAYLOAD_LAUNCH_DATAPOS.GyroY];
                                zGyro = dataSplit[(UInt16)PAYLOAD_LAUNCH_DATAPOS.GyroZ];
                                
                                //...Error correction
                                errorHappen = false;
                            }
                            else
                            {
                                
                                Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);

                                //...Event log
                                Tools_AddEventInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData] + @" @ :" +
                                    DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

                                //...Error correction
                                errorHappen = true;
                                
                            }                            
                        }
                        catch
                        {
                            Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);

                            //...Event log
                            Tools_AddEventInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData] + @" @ :" +
                                DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());
                            
                            //...Error correction
                            errorHappen = true;
                        }

                        if (!errorHappen)
                        {
                            //...Convert Values
                            float xAccUp = 0, yAccUp = 0, zAccUp = 0, xGyroUp = 0, yGyroUp = 0, zGyroUp = 0;

                        #if USE_MPU6050_ACCELERO
                            xAccUp = MPU6050_Normalize(Calculation_GetInt(xAcc));
                            yAccUp = MPU6050_Normalize(Calculation_GetInt(yAcc));
                            zAccUp = MPU6050_Normalize(Calculation_GetInt(zAcc));
                        #elif USE_GY80_ACCELERO
                            xAccUp = Calculation_GetFloat(xAcc);
                            yAccUp = Calculation_GetFloat(yAcc);
                            zAccUp = Calculation_GetFloat(zAcc);
                        #else
                            xAccUp      = CMPS10_Normalize(Calculation_GetInt(xAcc));
                            yAccUp      = CMPS10_Normalize(Calculation_GetInt(yAcc));
                            zAccUp      = CMPS10_Normalize(Calculation_GetInt(zAcc));
                        #endif

                            xGyroUp = Calculation_GetFloat(xGyro);
                            yGyroUp = Calculation_GetFloat(yGyro);
                            zGyroUp = Calculation_GetFloat(zGyro);
                            
                            //...MPU6050, CMPS10 used var's
                            float gSensor;
                            float pitch, roll;
                        
                        #if USE_MPU6050_ACCELERO                        
                            gSensor     = MPU6050_2G;
                            pitch       = yAccUp; //based on the Sensor placement
                            roll        = xAccUp; //based on the Sensor placement                        
                        #elif USE_GY80_ACCELERO
                            gSensor     = GY80_2G;
                            pitch       = -xAccUp; //based on the Sensor placement
                            roll        = yAccUp; //based on the Sensor placement
                        #else
                            gSensor = CMPS10_2G;
                            pitch = yAccUp; //based on the Sensor placement
                            roll = xAccUp; //based on the Sensor placement
                        #endif

                            //...Draw Graph
                        #if USE_TIME_UPDATE_GRAPH
                            //...LINE GRAPH
                            MyGraph.m_Instance.LineGraph(xAccUp, yAccUp, zAccUp, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Graph);
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.LineGraphText])
                            {
                                MyGraph.m_Instance.LineGraph(xAccUp, yAccUp, zAccUp, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.LineGraphText] = false;
                            }

                            //...PITCH-ROLL
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRoll])
                            {
                                MyGraph.m_Instance.PitchRoll(pitch, roll, PITCH_LIMIT_DEG, ROLL_LIMIT_DEG, gSensor, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRoll] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRollText])
                            {
                                MyGraph.m_Instance.PitchRoll(pitch, roll, PITCH_LIMIT_DEG, ROLL_LIMIT_DEG, gSensor, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRollText] = false;
                            }

                            //...GYRO
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Gyro])
                            {
                                MyGraph.m_Instance.Gyro(xGyroUp, yGyroUp, zGyroUp, FULL_CIRCLE_DEG, GY80_GYRO_MAX, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Gyro] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.GyroText])
                            {
                                MyGraph.m_Instance.Gyro(xGyroUp, yGyroUp, zGyroUp, FULL_CIRCLE_DEG, GY80_GYRO_MAX, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.GyroText] = false;
                            }
                        #else
                            //...PLEASE CALL BY TIME, NOT EVERY EXECUTE THIS MODULE
                            MyGraph.m_Instance.LineGraph    (xAccUp, yAccUp, zAccUp, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.Gyro         (xGyroUp, yGyroUp, zGyroUp, FULL_CIRCLE_DEG, MPU6050_2G, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.PitchRoll    (pitch, roll, PITCH_LIMIT_DEG, ROLL_LIMIT_DEG, gSensor, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.Altimeter    (0, heightUp, false, (byte)c_GRAPH_UPDATE_MODE.Both);
                        #endif
                        }

                        //...Reset & Clear
                        m_DataLine = NULL_STRING;
                    }
                    else
                    {
                        //...Append data
                        m_DataLine += Char.ConvertFromUtf32(data);
                    }

                    //...Save log
                #if USE_LOG_SAVED
                    m_StreamBytesCount++;
                #endif
                }
                else
                {
                    isNeedDelay = true;
                }

                //...DELAY
                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }

            //...Processed remain buffer at Comm port
            ProcessedRemainBuffer(level);

            //...Save log
        #if USE_LOG_SAVED
            LogSavedAppendFooter(true);
        #endif

        #if USE_TIME_UPDATE_GRAPH
            //...Disabled timer
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled = false;
        #endif

            //...Clear graph when stop
            if (m_StateStop)
            {
                ClearUI();
                m_StateStop = false;
            }
        }

    #if KOMURINDO_ROCKET
        //...Launch Rocket Receiver
        private void OnReceive_RocketLaunch(byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Rocket Launch!", false);

            //...USING PIXHAWK

            //...used & initial var's
            Int32 data = 0;
            bool isNeedDelay = true;
            m_StateLoop = true;
            bool errorHappen = false;

            //...Enabled timer
        #if USE_TIME_UPDATE_GRAPH
            m_MainTick = 0;
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled = true;
        #endif

        #if USE_LOG_SAVED
            LogSavedInitHeader(@"[ROCKET_LAUNCH]", true);
        #endif

            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    data = m_Comm[level].ReadChar();
                    if (data == (Int32)c_CHARACTER.Enter) //CHECK FOR LF
                    {
                        m_CounterData++; // Increase counter

                        //...Append with Date Time log
                        string log = Tools_AppendFormattedLogRocket(m_DataLine, m_CounterData, m_StartTime);

                        //...Save log
                    #if USE_LOG_SAVED
                        m_StreamLog.WriteLine(log);
                    #endif

                        //...Log
                        Tools_RollListReceiver(log, MAX_LOG_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Monitor]);

                        //...Split
                        string pitch = @"", yaw = @"", roll = @"", heading = @"", height = @"";
                        // string latitude = @"", longitude = @"";

                        try
                        {
                            string[] dataSplit = m_DataLine.Split(' ');
                            if (dataSplit.Length == (UInt16)ROCKET_LAUNCH_DATAPOS._Count)
                            {
                                pitch = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Pitch];
                                yaw = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Yaw];
                                roll = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Roll];
                                heading = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Heading];
                                height = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Height];
                                // latitude = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Latitude];
                                // longitude = dataSplit[(UInt16)ROCKET_LAUNCH_DATAPOS.Longitude];

                                //...Error correction
                                errorHappen = false;
                            }
                            else
                            {
                                Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);

                                //...Event log
                                Tools_AddEventInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData] + @" @ :" +
                                    DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

                                //...Error correction
                                errorHappen = true;
                            }
                        }
                        catch
                        {
                            Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);

                            //...Event log
                            Tools_AddEventInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData] + @" @ :" +
                                DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

                            //...Error correction
                            errorHappen = true;
                        }

                        if (!errorHappen)
                        {
                            //...Convert Values
                            float pitchUp = 0, yawUp = 0, rollUp = 0, headingUp = 0;
                            float heightUp = 0;
                            // float latitudeUp = 0, longitudeUp = 0;

                            pitchUp = Calculation_GetFloat(pitch);
                            yawUp = Calculation_GetFloat(yaw);
                            rollUp = Calculation_GetFloat(roll);

                            headingUp = Calculation_GetInt(heading);
                            heightUp = Calculation_GetFloat(height);

                            // latitudeUp = Calculation_GetFloat(latitude);
                            // longitudeUp = Calculation_GetFloat(longitude);

                            Int32 gSensor = 180;
                            //...Draw Graph
                        #if USE_TIME_UPDATE_GRAPH
                            //...LINE GRAPH
                            MyGraph.m_Instance.LineGraphPitchYawRoll((Int32)pitchUp, (Int32)yawUp, (Int32)rollUp, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Graph);
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.LineGraphText])
                            {
                                MyGraph.m_Instance.LineGraphPitchYawRoll((Int32)pitchUp, (Int32)yawUp, (Int32)rollUp, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.LineGraphText] = false;
                            }

                            //...PITCH-ROLL
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRoll])
                            {
                                MyGraph.m_Instance.PitchYawRollDegree((Int32)pitchUp, (Int32)yawUp, (Int32)rollUp, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRoll] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRollText])
                            {
                                MyGraph.m_Instance.PitchYawRollDegree((Int32)pitchUp, (Int32)yawUp, (Int32)rollUp, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.PitchRollText] = false;
                            }

                            Tools_StatusInfo(@"" + pitchUp, false);

                            //...COMPASS
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Compass])
                            {
                                MyGraph.m_Instance.Heading((UInt16)headingUp, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Compass] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.CompassText])
                            {
                                MyGraph.m_Instance.Heading((UInt16)headingUp, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.CompassText] = false;
                            }

                            //...HEIGHT
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Altimeter])
                            {
                                MyGraph.m_Instance.Altimeter(0, (Int32)heightUp, false, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Altimeter] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.AltimeterText])
                            {
                                MyGraph.m_Instance.Altimeter(0, (Int32)heightUp, false, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.AltimeterText] = false;
                            }

                            //...GYRO
                            // if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Gyro])
                            // {
                            //     MyGraph.m_Instance.Gyro(xGyroUp, yGyroUp, zGyroUp, FULL_CIRCLE_DEG, GY80_GYRO_MAX, (byte)c_GRAPH_UPDATE_MODE.Graph);
                            //     m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Gyro] = false;
                            // }
                            // if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.GyroText])
                            // {
                            //     MyGraph.m_Instance.Gyro(xGyroUp, yGyroUp, zGyroUp, FULL_CIRCLE_DEG, GY80_GYRO_MAX, (byte)c_GRAPH_UPDATE_MODE.Text);
                            //     m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.GyroText] = false;
                            // }
                        #else
                            //...PLEASE CALL BY TIME, NOT EVERY EXECUTE THIS MODULE
                            MyGraph.m_Instance.LineGraph    (xAccUp, yAccUp, zAccUp, EARTH_G, gSensor, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.Gyro         (xGyroUp, yGyroUp, zGyroUp, FULL_CIRCLE_DEG, MPU6050_2G, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.PitchRoll    (pitch, roll, PITCH_LIMIT_DEG, ROLL_LIMIT_DEG, gSensor, (byte)c_GRAPH_UPDATE_MODE.Both);
                            MyGraph.m_Instance.Altimeter    (0, heightUp, false, (byte)c_GRAPH_UPDATE_MODE.Both);
                        #endif
                        }

                        //...Reset & Clear
                        m_DataLine = NULL_STRING;
                    }
                    else
                    {
                        //...Append data
                        m_DataLine += Char.ConvertFromUtf32(data);
                    }

                    //...Save log
#if USE_LOG_SAVED
                    m_StreamBytesCount++;
#endif
                }
                else
                {
                    isNeedDelay = true;
                }

                //...DELAY
                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }

            //...Processed remain buffer at Comm port
            ProcessedRemainBuffer(level);

            //...Save log
#if USE_LOG_SAVED
            LogSavedAppendFooter(true);
#endif

#if USE_TIME_UPDATE_GRAPH
            //...Disabled timer
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled = false;
#endif

            //...Clear graph when stop
            if (m_StateStop)
            {
                ClearUI();
                m_StateStop = false;
            }
        }
    #endif

    #if BALON_ATMOSFER
        //...Launch Balon Receiver
        private void OnReceive_BalonLaunch(byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Balon Launch!", false);

            //...used & initial var's
            Int32 data          = 0;
            bool isNeedDelay    = true;
            m_StateLoop         = true;

            //...Enabled timer
        #if USE_TIME_UPDATE_GRAPH
            m_MainTick                                  = 0;
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled    = true;
        #endif

        #if USE_LOG_SAVED
            LogSavedInitHeader(@"[BALON_LAUNCH]");
        #endif

            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    data = m_Comm[level].ReadChar();
                    if (data == (Int32)c_CHARACTER.Enter) //CHECK FOR LF
                    {
                        //...Append with Date Time log
                        string log = Tools_AppendDateTimeTag(m_DataLine);

                        //...Save log
                    #if USE_LOG_SAVED
                        m_StreamLog.WriteLine(log);
                    #endif

                        //...Log
                        Tools_RollListReceiver(log, MAX_LOG_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Monitor]);

                        //...Split
                        string ID = @"", humidity = @"", temperature = @"", pressure = @"", latitude = @"", longitude = @"";

                        try
                        {
                            ID              = m_DataLine.Substring((UInt16)BALON_LAUNCH_DATAPOS.ID, 3);
                            humidity        = m_DataLine.Substring((UInt16)BALON_LAUNCH_DATAPOS.Humidity, 3);
                            temperature     = m_DataLine.Substring((UInt16)BALON_LAUNCH_DATAPOS.Temperature, 3);
                            pressure        = m_DataLine.Substring((UInt16)BALON_LAUNCH_DATAPOS.Pressure, 6);
                            latitude        = m_DataLine.Substring((UInt16)BALON_LAUNCH_DATAPOS.Latitude, 10);
                            longitude       = m_DataLine.Substring((UInt16)BALON_LAUNCH_DATAPOS.Longitude, 11);
                        }
                        catch
                        {
                            Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);
                        }

                        //...Convert Values
                        Int32 humidityUp = 0, temperatureUp = 0, pressureUp = 0, latitudeUp = 0, longitudeUp = 0;

                        humidityUp      = Calculation_GetInt(humidity);
                        temperatureUp   = Calculation_GetInt(temperature);
                        pressureUp      = Calculation_GetInt(pressure);
                        latitudeUp      = Calculation_GetInt(latitude);
                        longitudeUp     = Calculation_GetInt(longitude);

                        //...Draw Graph
                    #if USE_TIME_UPDATE_GRAPH
                        //...HUMIDITY
                        if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Humidity])
                        {
                            MyGraph.m_Instance.Humidity((byte)humidityUp, (byte)temperatureUp, (byte)c_GRAPH_UPDATE_MODE.Graph);
                            m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Humidity] = false;
                        }
                        
                        if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.HumidityText])
                        {
                            MyGraph.m_Instance.Humidity((byte)humidityUp, (byte)temperatureUp, (byte)c_GRAPH_UPDATE_MODE.Text);
                            m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.HumidityText] = false;                            
                        }

                        //...PRESSURE
                        //MyGraph.m_Instance.Altimeter(pressureUp, 0, true, (byte)c_GRAPH_UPDATE_MODE.Graph);
                        //if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.AltimeterText])
                        //{
                        //    MyGraph.m_Instance.Altimeter(pressureUp, 0, true, (byte)c_GRAPH_UPDATE_MODE.Text);
                        //    m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.AltimeterText] = false;
                        //}

                    #else

                    #endif

                        //...Reset & Clear
                        m_DataLine = NULL_STRING;
                    }
                    else
                    {
                        //...Append data
                        m_DataLine += Char.ConvertFromUtf32(data);
                    }

                    //...Save log
                #if USE_LOG_SAVED
                    m_StreamBytesCount++;
                #endif

                    //...MISS LF Error
                    if (m_DataLine.Length > (Int32)BALON_LAUNCH_DATAPOS.Enter)
                    {
                        Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissLF], true);
                    }
                }
                else
                {
                    isNeedDelay = true;
                }

                //...DELAY
                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }

            //...Processed remain buffer at Comm port
            ProcessedRemainBuffer(level);

            //...Save log
        #if USE_LOG_SAVED
            LogSavedAppendFooter();
        #endif

        #if USE_TIME_UPDATE_GRAPH
            //...Disabled timer
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled = false;
        #endif

            //...Clear graph when stop
            if (m_StateStop)
            {
                ClearUI();
                m_StateStop = false;
            }
        }
    #endif

    #if KOMURINDO_PAYLOAD
        //...Tracking Receiver
        private void OnReceive_PayloadTracking(byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Payload Tracking!", false);
            
            //...used & initial var's
            Int32               data = 0;
            bool isNeedDelay    = true;
            m_StateLoop         = true;
            bool errorHappen    = false;
            //...Enabled timer
        #if USE_TIME_UPDATE_GRAPH
            m_MainTick = 0;
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled = true;
        #endif

        #if USE_LOG_SAVED
            LogSavedInitHeader(@"[PAYLOAD_TRACKING]", true);
        #endif

            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    data = m_Comm[level].ReadChar();
                    if (data == (UInt16)c_CHARACTER.Enter)
                    {
                        m_CounterData++; // Increase counter

                        //...Append with Date Time log
                        string log = Tools_AppendFormattedLogLocation(m_DataLine, m_CounterData, m_StartTime);

                        //...Save log
                    #if USE_LOG_SAVED
                        m_StreamLog.WriteLine(log);
                    #endif
                        
                        //...Log
                        Tools_RollListReceiver(log, MAX_LOG_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Monitor]);

                        //...Split
                        string latitude = NULL_STRING, longitude = NULL_STRING, height = NULL_STRING,
                            satellite = NULL_STRING, airspeed = NULL_STRING, compass = NULL_STRING, pressure = NULL_STRING;

                        try
                        {
                            string[] dataSplit = m_DataLine.Split(' ');
                            if (dataSplit.Length == (UInt16)PAYLOAD_TRACKING_DATAPOS._Count)
                            {
                                longitude = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Longitude];
                                latitude = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Latitude];
                                height = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Height];
                                satellite = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Satellite];
                                airspeed = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Speed];
                                compass = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Heading];
                                pressure = dataSplit[(UInt16)PAYLOAD_TRACKING_DATAPOS.Pressure];

                                //...Error correction
                                errorHappen = false;
                            }
                            else
                            {
                                Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);

                                //...Event log
                                Tools_AddEventInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData] + @" @ :" +
                                    DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

                                //...Error correction
                                errorHappen = true;
                            }
                        }
                        catch
                        {
                            Tools_StatusInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData], true);

                            //...Event log
                            Tools_AddEventInfo(m_ErrorString[(UInt16)ERROR_LIST.MissData] + @" @ :" +
                                DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

                            errorHappen = true;
                        }

                        if (!errorHappen)
                        {
                            //...Convert Values
                            float latitudeUp = 0, longitudeUp = 0;
                            longitudeUp = Calculation_GetFloat(longitude);
                            latitudeUp = Calculation_GetFloat(latitude);

                            float heightUp = Calculation_GetFloat(height);
                            UInt16 satelliteUp = (UInt16) Calculation_GetInt(satellite);
                            float airspeedUp = Calculation_GetFloat(airspeed);
                            float compassUp = Calculation_GetFloat(compass);
                            float pressureUp = Calculation_GetFloat(pressure);

                            //...Draw Graph
                        #if USE_TIME_UPDATE_GRAPH
                            //...MAPS
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Maps])
                            {
                                UpdateMaps(latitudeUp, longitudeUp, satelliteUp, MyGraph.GMAP_DEFAULT_ZOOM);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Maps] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.MapsText])
                            {
                                UpdateMaps(latitudeUp, longitudeUp, satelliteUp, MyGraph.GMAP_DEFAULT_ZOOM);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.MapsText] = false;
                            }

                            //...COMPASS
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Compass])
                            {
                                MyGraph.m_Instance.Heading((UInt16)compassUp, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Compass] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.CompassText])
                            {
                                MyGraph.m_Instance.Heading((UInt16)compassUp, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.CompassText] = false;
                            }

                            //...HEIGHT
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Altimeter])
                            {
                                MyGraph.m_Instance.Altimeter(pressureUp, (Int32)heightUp, false, (byte)c_GRAPH_UPDATE_MODE.Graph);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Altimeter] = false;
                            }
                            if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.AltimeterText])
                            {
                                MyGraph.m_Instance.Altimeter(pressureUp, (Int32)heightUp, false, (byte)c_GRAPH_UPDATE_MODE.Text);
                                m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.AltimeterText] = false;
                            }

                            //...TRACKER
                            //Int32 latitudeInt, longitudeInt;
                            //string value;

                            //...Get new value
                            //value = latitudeUp.ToString().Replace(@"-", @"");
                            //value = value.Replace(@".", @"");
                            //latitudeInt = Calculation_GetInt(value);

                            //value = longitudeUp.ToString().Replace(@"-", @"");
                            //value = value.Replace(@".", @"");
                            //longitudeInt = Calculation_GetInt(value);

                            //if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Tracker])
                            //{
                            //    MyGraph.m_Instance.Tracker((latitudeInt - m_LatitudeMin), (longitudeInt - m_LongitudeMin),
                             //       m_LatitudeMax, m_LongitudeMax, 0, 0, (byte)c_GRAPH_UPDATE_MODE.Both);
                            //    m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.Tracker] = false;
                            //}
                            //if (m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.TrackerText])
                            //{
                                //MyGraph.m_Instance.Tracker((latitudeInt - m_LatitudeMin), (longitudeInt - m_LongitudeMin),
                                //  m_LatitudeMax, m_LongitudeMax, 0, 0, (byte)c_GRAPH_UPDATE_MODE.Both);
                            //    m_GraphState.IsNeedUpDate[(UInt16)GRAPH_CONTROL.TrackerText] = false;
                            //}
                    #else
                        //...PLEASE CALL BY TIME, NOT EVERY EXECUTE THIS MODULE
                        MyGraph.m_Instance.Heading      (compassUp, (byte)c_GRAPH_UPDATE_MODE.Both);
                        MyGraph.m_Instance.Tracker      (latitudeUp, longitudeUp, 0, 0, (byte)c_GRAPH_UPDATE_MODE.Both);
                        MyGraph.m_Instance.ParseMaps    (latitudeUp, longitudeUp, (byte)c_GRAPH_UPDATE_MODE.Text);
                    #endif
                        }

                        //...Reset & Clear
                        m_DataLine = NULL_STRING;
                    }
                    else
                    {
                        m_DataLine += Char.ConvertFromUtf32(data);
                    }

                    //...Save log
                #if USE_LOG_SAVED
                    m_StreamBytesCount++;
                #endif
                }
                else
                {
                    isNeedDelay = true;
                }

                //...DELAY
                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }

            //...Processed remain buffer at Comm port
            ProcessedRemainBuffer(level);

            //...Save log
        #if USE_LOG_SAVED
            LogSavedAppendFooter(true);
        #endif

        #if USE_TIME_UPDATE_GRAPH
            //...Disabled timer
            m_Timer[(UInt16)TIMER_LIST.Main].Enabled = false;
        #endif

            //...Clear graph when stop
            if (m_StateStop)
            {
                ClearUI();
                m_StateStop = false;
            }
        }

        private void OnReceive_PayloadTrajectory(byte level)
        {
            //...W.I.P
        }

        //...Camera Receiver
        private void OnReceive_PayloadCapture(byte level)
        {
        #if USE_JPEG_PICTURE
            PayloadJPEGCapture(level);
        #else
            PayloadCapture(level);
        #endif
        }

        private void JPEGReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort) sender;
            string pj1 = sp.ReadByte().ToString("X2");
            string pj2 = sp.ReadByte().ToString("X2");
            int lenght = int.Parse((pj2 + pj1), System.Globalization.NumberStyles.HexNumber);
            string imageBuffer = string.Empty;
            for (int i = 0; i < lenght; i++)
            {
                imageBuffer += sp.ReadByte().ToString("X2");
            }
            MyGraph.m_Instance.DrawJPEGImage(imageBuffer);
        }

        private void PayloadJPEGCapture(byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Payload JPEG Capture!", false);

            //...Used & initial var's
            bool isNeedDelay = true;
            m_StateLoop = true;

        #if USE_LOG_SAVED
            //...Header
            m_StreamHeader = NULL_STRING;
            m_StreamHeader += @"[PAYLOAD_CAPTURE]@";
            m_StreamHeader += (DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

            //...Event log
            Tools_AddEventInfo(m_StreamHeader);
        #endif

            //...Set Undeterminate
            UpdateCapturedPercentage(true);
            Application.DoEvents();

            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    //...Get
                    string pj1 = m_Comm[level].ReadByte().ToString("X2");
                    string pj2 = m_Comm[level].ReadByte().ToString("X2");
                    int lenght = int.Parse((pj2 + pj1), System.Globalization.NumberStyles.HexNumber);
                    string imageBuffer = string.Empty;
                    for (int i = 0; i < lenght; i++)
                    {
                        imageBuffer += m_Comm[level].ReadByte().ToString("X2");
                    }
                    MyGraph.m_Instance.DrawJPEGImage(imageBuffer);
                }
                else
                {
                    isNeedDelay = true;
                }

                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }
        }

        private void PayloadCapture(byte level)
        {
            //...Show status
            Tools_StatusInfo(@"Payload Capture!", false);
            
            //...Used & initial var's
            UInt32 countData    = 0;
            string strData      = @"";
            bool isNeedDelay    = true;
            m_StateLoop          = true;
        
        #if USE_LOG_SAVED
            //...Header
            m_StreamHeader = NULL_STRING;
            m_StreamHeader += @"[PAYLOAD_CAPTURE]@";
            m_StreamHeader += (DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());

            //...Event log
            Tools_AddEventInfo(m_StreamHeader);
        #endif
            
            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;
                    
                    //...Get
                    strData = m_Comm[level].ReadExisting();
                    m_ImageRawData = m_ImageRawData + strData;
                    countData = (UInt32)m_ImageRawData.Length;
                    
                    //...Break to refresh Graphic
                    if (countData % DATA_CAMERA_MOD_BREAK == 0)
                    {
                        MyGraph.m_Instance.DrawImage();
                        UpdateCapturedPercentage(false);

                        //...Event log
                        Tools_AddEventInfo(@"Update " + countData.ToString() + @" bytes data to Image");
                    }
                    
                    //...If all data received, stop Loop
                    if (countData >= DATA_CAMERA_NORMAL) //Perfect receive
                    {
                        CommRefresh();
                        MyGraph.m_Instance.DrawImage();

                        //...Event log
                        Tools_AddEventInfo(@"PERFECTLY GOT ALL DATA" + @" @ :" +
                                DateTime.Now.ToShortDateString() + @"_" + DateTime.Now.ToLongTimeString());
                    }
                }
                else
                {
                    isNeedDelay = true;
                }

                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }
        }
    #endif

        public void OnReceive_Calibration(UInt16 item)
        {
            //...Set uart level
            byte level = (byte)c_LEVEL.Main;
            
            //..Set Items
            switch (item)
            {
                case (UInt16)c_CALIB_LIST.Position:
                    Tools_StatusInfo(@"Calib Position!", false);
                    break;
                case (UInt16)c_CALIB_LIST.Pressure:
                    Tools_StatusInfo(@"Calib Pressure!", false);
                    break;
                case (UInt16)c_CALIB_LIST.Heading:
                    Tools_StatusInfo(@"Calib Heading!", false);
                    break;
                default:
                    break;
            }

            //...Used & initial var's
            Int32 data          = 0;
            string dataLine     = @"";
            bool isNeedDelay    = true;
            m_StateLoop         = true;
            
            //...Loop
            while (m_StateLoop)
            {
                if (m_Comm[level].BytesToRead > 0)
                {
                    isNeedDelay = false;

                    data = m_Comm[level].ReadChar();
                    if (data == (Int32)c_CHARACTER.Enter)
                    {
                        m_CounterData++; // Increase counter

                        //...Append with Date Time log
                        string log = Tools_AppendFormattedLog(dataLine, m_CounterData, m_StartTime);

                        //...Log
                        Tools_RollListReceiver(log, MAX_LOG_LIST, m_LsLog[(UInt16)LISTBOX_LIST.Monitor]);
                        
                        //...Get
                        
                        //Reset & Clear
                        dataLine = @"";
                    }
                    else
                    {
                        dataLine = dataLine + Char.ConvertFromUtf32(data);
                    }
                }
                else
                {
                    isNeedDelay = true;
                }

                if (isNeedDelay)
                {
                    Application.DoEvents();
                }
            }
        }
    
    }  
}

namespace Utils
{
    class MyPath
    {
        //=============================== CLASS MEMBER ===============================//
        public static string s_Root;
        public static string s_Icon;
        public static string s_Picture;
        public static string s_Config;
        public static string s_Text;
        public static string s_Saved;

        //=============================== FUNCTIONS ==================================//
        public MyPath()
        {
            //...Root
            s_Root = @"C:\KOMURINDO\Res\";
            //...Sub dir
            s_Icon = s_Root + @"Icon\";
            s_Picture = s_Root + @"Pic\";
            s_Config = s_Root + @"Config\";
            s_Text = s_Root + @"Text\";
            s_Saved = s_Root + @"Save\";
        }

        ~MyPath()
        {
            //...Destructor
        }
    }

    class MyConfig
    {
        //=============================== CLASS MEMBER ===============================//
        //...Comm
        public static string s_CommMain;
        public static string s_CommSecond;
        public static UInt32 s_CommMainBaud;
        public static UInt32 s_CommSecondBaud;
        //...Setting
        public static string s_MapsProvider;
        //public static float     s_GroundLatitude;
        //public static float     s_GroundLongitude;
        //public static UInt16    s_MapsRefreshRate;
        //public static Int32     s_GroundPressure;
        //public static UInt16    s_GroundHeading;

        //=============================== FUNCTIONS ==================================//
        public MyConfig()
        {
            //...HARD SET
            s_CommMain = @"COM6";
            s_CommMainBaud = 57600;
            s_CommSecond = @"COM1";
            s_CommSecondBaud = 57600;
            s_MapsProvider = @"http://www.8051projects.info/maps.asp?lat=LATITUDE&lon=LONGITUDE";

            //...PLEASE Use Save-Load method
        }

        ~MyConfig()
        {
            //...Destructor
        }
    }
}