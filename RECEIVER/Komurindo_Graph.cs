//...Ceiling Captured Percentage(Force 100 %)
#define USE_CEILING_CAPTURED_PERCENTAGE
//...Static or Non-Static camera data counter
//#define USE_STATIC_CAMERA_COUNTER
//...Debug value's
//#define SHOW_DEBUG_VALUE

using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Drawing.Drawing2D; //Drawing
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;

namespace MataDewa4
{
    class MyGraph
    {
        //=============================== STRUCTURE ==================================//
        //...BITMAP
        struct BITMAP
        {
            public Bitmap[]     Gyro;
            public Bitmap       LineGraph;
            //public Bitmap       Radar;
            public Bitmap       Camera;
        }

        //...GRAPH
        struct ACCELERO_LINEGRAPH
        {
            public Int32    XOld;
            public Int32    YOld;
            //public Int32    PreviousValue;
            //public Int32    Value;
            public Pen      DrawPen;
        }

        struct LINE_GRAPH
        {
            public Int32                XMax;
            public Int32                Step;
            public Int32                YCenter;
            public Graphics             Canvas;
            public ACCELERO_LINEGRAPH[] Accelero;
            //public Color                MeasureColor;
            //public Color                CanvasColor;
        }

        struct GYRO_BARGRAPH
        {
            public Int32    XCenter;
            //public Int32    PreviousValue;
            //public Int32    Value;
            public Brush    DrawBrush;
            public Graphics Canvas;
        }

        struct BAR_GRAPH
        {
            public GYRO_BARGRAPH[]  Gyro;
            public Pen              DrawPen;
        }

        struct GENERAL_GRAPH
        {
            public Graphics     Canvas;
            public Bitmap       Image;
            public Brush        DrawBrush;
            public Pen          DrawPen;
            public Brush        DrawPoint;
        }

        //=============================== CONSTANT ===================================//
        //...All
        enum GRAPH_PICTURE_LIST
        {
            LineGraph, GyroX, GyroY, GyroZ, Tracker, Camera, Odo3D, _Count
        }

        enum LABEL_LIST
        {
            CameraPercentage, Latitude, Longitude, Satellite, Heading, TrackingStatus, Humidity, Temperature, _Count
        }

        //...Specific
        enum ATTITUDE_LABEL_LIST
        {
            AccX, AccY, AccZ, GyroX, GyroY, GyroZ, Height, Pitch, Roll, Pressure, _Count
        }

        //...Constant
        const UInt16    GYRO_BAR_WIDTH              = 250;
        const UInt16    GYRO_BAR_HEIGHT             = 15;
        const UInt16    SQUARE_CANVAS_LENGHT        = 200;
        const UInt16    LINE_GRAPH_WIDTH            = 720;
        const UInt16    LINE_GRAPH_HEIGHT           = 220;
        const UInt16    LINE_GRAPH_STEP             = 2;
        const UInt16    LINE_GRAPH_LINE_BOLD        = 1;

        const Int16     HUMIDITY_0_VAL              = -6000;
        const Int16     HUMIDITY_100_VAL            = 4000;

        const double    pi                          = 3.14159265358979;
        const Int32     ZERO                        = 0;

        const UInt16    TRACKER_SERVO_MAX_DEG       = 180;
        const UInt16    TRACKER_SERVO_X_MIN         = 440;
        const UInt16    TRACKER_SERVO_X_MAX         = 1600;
        const UInt16    TRACKER_SERVO_X_OFFSET      = TRACKER_SERVO_X_MAX - TRACKER_SERVO_X_MIN;
        const UInt16    TRACKER_SERVO_Y_MIN         = 520;
        const UInt16    TRACKER_SERVO_Y_MAX         = 1550;
        const UInt16    TRACKER_SERVO_Y_OFFSET      = TRACKER_SERVO_Y_MAX - TRACKER_SERVO_Y_MIN;

        //=============================== CLASS MEMBER ===============================//
        //...Private    
        private BITMAP          m_Bitmap            = new BITMAP();
        private GENERAL_GRAPH   m_Tracker           = new GENERAL_GRAPH();  //Tracker,
        private LINE_GRAPH      m_LineGraph         = new LINE_GRAPH();     //Line Graph,
        private BAR_GRAPH       m_BarGraph          = new BAR_GRAPH();      //Gyro Graph,

    #if USE_STATIC_CAMERA_COUNTER
        private static Int32    v_DataCount;
        private static Int32    v_ColumnCount;
        private static Int32    v_RowCount;
        private static byte     v_FirstCount;
        private static byte     v_RgbCount;
    #endif

        //...Sub Windows Control
        private PictureBox[]    m_PicGraph;
        private Label[]         m_LblAttitudeValue;
        private Label[]         m_LblGeneral;

        private ControlAttitude         m_PitchRoll;
        private ControlAltimeter        m_Altimeter;
        private ControlVerticalSpeed    m_Humidity;
        private ControlHeading          m_Heading;

        public const UInt16             GMAP_DEFAULT_ZOOM = 15;
        const string                    GMAP_OVERLAY_1 = @"gmap_overlay_1";
        private UInt16                  m_GMapZoom = GMAP_DEFAULT_ZOOM;
        private GMapControl             m_GMap;
        private GMapOverlay             m_OverlayMarker = new GMapOverlay(GMAP_OVERLAY_1);
        private GMarkerGoogle           m_MapPos;

        private Math3D.Cube             m_MainCube;
        private Point                   m_DrawCube;

        private ProgressBar             m_ProgressCamera;
        //...Public
        public static MyGraph           m_Instance;
        
        //=============================== MAIN INSTANCE ==============================//
        public MyGraph()
        {
            //...Bind to it self
            m_Instance = this;

            //...Init
            InitBitmapGraph();

            //...Init Sub Windows Control
            InitSubWinControl();
        }

        ~MyGraph()
        {
            //...RELEASE ALL
            //...Sub Windows Control
            for (UInt16 i = 0; i < (UInt16)GRAPH_PICTURE_LIST._Count; i++)
            {
                m_PicGraph[i].Dispose();
            }
            for (UInt16 i = 0; i < (UInt16)ATTITUDE_LABEL_LIST._Count; i++)
            {
                m_LblAttitudeValue[i].Dispose();
            }
            for (UInt16 i = 0; i < (UInt16)LABEL_LIST._Count; i++)
            {
                m_LblGeneral[i].Dispose();
            }

            m_PitchRoll.Dispose();
            m_Altimeter.Dispose();
            m_Humidity.Dispose();
            m_Heading.Dispose();
        }

        //=============================== INIT METHOD ================================//
        private void InitSubWinControl()
        {
            m_LblAttitudeValue  = new Label[(UInt16)ATTITUDE_LABEL_LIST._Count];
            m_LblGeneral        = new Label[(UInt16)LABEL_LIST._Count];
            m_PicGraph          = new PictureBox[(UInt16)GRAPH_PICTURE_LIST._Count];
        }

        private void InitBitmapGraph()
        {
            //...Bitmap Gyro
            m_Bitmap.Gyro = new Bitmap[(UInt16)c_SENSOR_AXIS._Count];
            for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
            {
                m_Bitmap.Gyro[i]        = new Bitmap(GYRO_BAR_WIDTH, GYRO_BAR_HEIGHT); // GYRO
            }

            //...Bitmap LineGraph
            m_Bitmap.LineGraph          = new Bitmap(LINE_GRAPH_WIDTH, LINE_GRAPH_HEIGHT); // LINE GRAPH
            
            //...Camera Bitmap
            m_Bitmap.Camera             = new Bitmap(SQUARE_CANVAS_LENGHT, SQUARE_CANVAS_LENGHT); // CAMERA CAPTURE
            
            //...Tracker
            m_Tracker.Image             = new Bitmap(SQUARE_CANVAS_LENGHT, SQUARE_CANVAS_LENGHT); // TRACKER
            m_Tracker.Canvas            = Graphics.FromImage(m_Tracker.Image);
            m_Tracker.DrawBrush         = new SolidBrush(Color.Yellow);
            m_Tracker.DrawPen           = new Pen(Color.Lime, LINE_GRAPH_LINE_BOLD);
            m_Tracker.DrawPoint         = new SolidBrush(Color.Orange);

            //...GRAPH INIT's
            //...Gyro Graph
            m_BarGraph.Gyro = new GYRO_BARGRAPH[(UInt16)c_SENSOR_AXIS._Count];
            for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
            {
                m_BarGraph.Gyro[i].XCenter = (Int32)(GYRO_BAR_WIDTH / 2); // 1/2 from Canvas Gyro BarGraph(Center)
            }

            m_BarGraph.Gyro[(UInt16)c_SENSOR_AXIS.X].DrawBrush = new SolidBrush(Color.Red);
            m_BarGraph.Gyro[(UInt16)c_SENSOR_AXIS.Y].DrawBrush = new SolidBrush(Color.Orange);
            m_BarGraph.Gyro[(UInt16)c_SENSOR_AXIS.Z].DrawBrush = new SolidBrush(Color.Blue);
            m_BarGraph.DrawPen = new Pen(Color.AliceBlue, LINE_GRAPH_LINE_BOLD);

            for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
            {
                m_BarGraph.Gyro[i].Canvas                   = Graphics.FromImage(m_Bitmap.Gyro[i]);
                m_BarGraph.Gyro[i].Canvas.SmoothingMode     = SmoothingMode.AntiAlias;
                m_BarGraph.Gyro[i].Canvas.Clear(Color.Black);
            }

            //...Line Graph
            m_LineGraph.Accelero = new ACCELERO_LINEGRAPH[(UInt16)c_SENSOR_AXIS._Count];
            for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
            {
                //...Normal
                m_LineGraph.Accelero[i].XOld = 0;
                m_LineGraph.Accelero[i].YOld = (UInt16)(LINE_GRAPH_HEIGHT / 2); // 1/2 from Canvas LineGraph(Center)
            }

            m_LineGraph.XMax            = LINE_GRAPH_WIDTH;
            m_LineGraph.YCenter         = (UInt16)(LINE_GRAPH_HEIGHT / 2);
            m_LineGraph.Step            = LINE_GRAPH_STEP;
            //m_LineGraph.CanvasColor     = Color.Black;
            //m_LineGraph.MeasureColor    = Color.DarkGreen;

            m_LineGraph.Accelero[(UInt16)c_SENSOR_AXIS.X].DrawPen = new Pen(Color.Red, LINE_GRAPH_LINE_BOLD);
            m_LineGraph.Accelero[(UInt16)c_SENSOR_AXIS.Y].DrawPen = new Pen(Color.Yellow, LINE_GRAPH_LINE_BOLD);
            m_LineGraph.Accelero[(UInt16)c_SENSOR_AXIS.Z].DrawPen = new Pen(Color.Lime, LINE_GRAPH_LINE_BOLD);

            m_LineGraph.Canvas                  = Graphics.FromImage(m_Bitmap.LineGraph);
            m_LineGraph.Canvas.SmoothingMode    = SmoothingMode.AntiAlias;
            m_LineGraph.Canvas.Clear(Color.Black);

            // Cube
            m_MainCube = new Math3D.Cube(100, 200, 75);
            m_MainCube.FillFront = true; // isi depan
            m_MainCube.FillTop = true; // isi atas
        }

        public void BindGeneralLabel(
            Label cameraPercentage, Label latitude, Label longitude, Label satellite, Label heading, Label trackingStatus,
            Label humidity, Label temperature
            )
        {
            m_LblGeneral[(UInt16)LABEL_LIST.CameraPercentage]   = cameraPercentage;
            m_LblGeneral[(UInt16)LABEL_LIST.Latitude]           = latitude;
            m_LblGeneral[(UInt16)LABEL_LIST.Longitude]          = longitude;
            m_LblGeneral[(UInt16)LABEL_LIST.Satellite]          = satellite;
            m_LblGeneral[(UInt16)LABEL_LIST.Heading]            = heading;
            m_LblGeneral[(UInt16)LABEL_LIST.TrackingStatus]     = trackingStatus;
            m_LblGeneral[(UInt16)LABEL_LIST.Humidity]           = humidity;
            m_LblGeneral[(UInt16)LABEL_LIST.Temperature]        = temperature;
        }

        public void BindAttitudeLabel(
            Label accX, Label accY, Label accZ, Label gyroX, Label gyroY, Label gyroZ, 
            Label height, Label pitch, Label roll, Label pressure
            )
        {
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccX]        = accX;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccY]        = accY;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccZ]        = accZ;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroX]       = gyroX;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroY]       = gyroY;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroZ]       = gyroZ;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Height]      = height;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Pitch]       = pitch;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Roll]        = roll;
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Pressure]    = pressure;
        }

        public void BindPictureGraph(
            PictureBox lineGraph, PictureBox gyroX, PictureBox gyroY, PictureBox gyroZ,
            PictureBox tracker, PictureBox camera, PictureBox odo3D
            )
        {
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.LineGraph]           = lineGraph;
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroX]                = gyroX;
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroY]                = gyroY;
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroZ]                = gyroZ;
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Tracker]              = tracker;
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Camera]               = camera;
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Odo3D]                = odo3D;
        }

        public void BindAvionicControl(
            ControlAttitude pitchRoll, ControlAltimeter altimeter, ControlVerticalSpeed humidity, ControlHeading heading
            )
        {
            m_PitchRoll     = pitchRoll;
            m_Altimeter     = altimeter;
            m_Humidity      = humidity;
            m_Heading       = heading;
        }

        public void BindOtherControl(
            GMapControl gMap, ProgressBar progressCamera
            )
        {
            m_GMap = gMap;
            m_ProgressCamera = progressCamera;
        }

        public void InitGMap()
        {
            // Init
            m_MapPos = new GMarkerGoogle(m_GMap.Position, GMarkerGoogleType.arrow);
            m_GMap.Overlays.Add(m_OverlayMarker);
            m_OverlayMarker.Markers.Add(m_MapPos);

            m_GMap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            m_GMap.Zoom = GMAP_DEFAULT_ZOOM;
        }

        //=============================== GRAPHIC ======================================//
        public void Gyro(float gyroX, float gyroY, float gyroZ, float limitDeg, float sensor2G, byte mode)
        {
            //...Get > to Local var's
            float[] gyro = new float[(UInt16)c_SENSOR_AXIS._Count];
            gyro[(UInt16)c_SENSOR_AXIS.X] = gyroX;
            gyro[(UInt16)c_SENSOR_AXIS.Y] = gyroY;
            gyro[(UInt16)c_SENSOR_AXIS.Z] = gyroZ;

            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                //...Need to Clear
                for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                {
                    //...Clear Canvas
                    m_BarGraph.Gyro[i].Canvas.Clear(Color.Black);

                    //...Draw Center Line
                    m_BarGraph.Gyro[i].Canvas.DrawLine(m_BarGraph.DrawPen, m_BarGraph.Gyro[i].XCenter, 0, m_BarGraph.Gyro[i].XCenter, GYRO_BAR_HEIGHT);
                }

                //...Gyro 3 Axis Draw
                for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                {
                    //...Get Width
                    float posStart  = m_BarGraph.Gyro[i].XCenter; //Get from center
                    float width     = Scaling(gyro[i], GYRO_BAR_WIDTH / 2, sensor2G);

                    //...Draw
                    if (width > 0)
                    {
                        //...Clear when new Value is smaller than previous Value
                        //if (s_BarGraph.Gyro[i].Value < s_BarGraph.Gyro[i].PreviousValue)
                        //{
                        //s_BarGraph.Gyro[i].Canvas.Clear(Color.Black);
                        //}
                        m_BarGraph.Gyro[i].Canvas.FillRectangle(m_BarGraph.Gyro[i].DrawBrush, posStart + 1, 0, width, GYRO_BAR_HEIGHT);
                    }
                    else if (width < 0)
                    {
                        //...Clear when new Value is greater than previous Value
                        //if (s_BarGraph.Gyro[i].Value > s_BarGraph.Gyro[i].PreviousValue)
                        //{
                        //s_BarGraph.Gyro[i].Canvas.Clear(Color.Black);
                        //}
                        posStart = posStart + width;
                        m_BarGraph.Gyro[i].Canvas.FillRectangle(m_BarGraph.Gyro[i].DrawBrush, posStart - 1, 0, -width, GYRO_BAR_HEIGHT);
                    }
                }

                //...Display Graphic
                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroX].Image = m_Bitmap.Gyro[(UInt16)c_SENSOR_AXIS.X];
                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroY].Image = m_Bitmap.Gyro[(UInt16)c_SENSOR_AXIS.Y];
                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroZ].Image = m_Bitmap.Gyro[(UInt16)c_SENSOR_AXIS.Z];
            }

            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                //...Additional Display
                for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                {
                    gyro[i] = Scaling(gyro[i], limitDeg, sensor2G);
                }

                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroX].Text = gyro[(UInt16)c_SENSOR_AXIS.X].ToString();
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroY].Text = gyro[(UInt16)c_SENSOR_AXIS.Y].ToString();
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroZ].Text = gyro[(UInt16)c_SENSOR_AXIS.Z].ToString();
            }
        }

        // Using scale 0 - 1.0F for 1G sensor
        public void LineGraph(float accX, float accY, float accZ, float earthG, float sensor2G, byte mode)
        {
            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                //...Clear Graphic
                if (m_LineGraph.Accelero[(UInt16)c_SENSOR_AXIS.X].XOld >= m_LineGraph.XMax)
                {
                    //...Clear
                    m_LineGraph.Canvas.Clear(Color.Black);

                    //...Center horizontal line
                    Pen penGrey = new Pen(Color.Gray, LINE_GRAPH_LINE_BOLD);
                    m_LineGraph.Canvas.DrawLine(penGrey, 0, (UInt16)(LINE_GRAPH_HEIGHT / 2), LINE_GRAPH_WIDTH, (UInt16)(LINE_GRAPH_HEIGHT / 2));
                    penGrey.Dispose();

                    //...Reset Value
                    for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                    {
                        m_LineGraph.Accelero[i].XOld = 0;
                        m_LineGraph.Accelero[i].YOld = (Int32)(LINE_GRAPH_HEIGHT / 2);
                    }
                    return;
                }

                //...Get > to Local var's
                float[] acc = new float[(UInt16)c_SENSOR_AXIS._Count];
                acc[(UInt16)c_SENSOR_AXIS.X] = Scaling(accX, LINE_GRAPH_HEIGHT / 2, sensor2G);
                acc[(UInt16)c_SENSOR_AXIS.Y] = Scaling(accY, LINE_GRAPH_HEIGHT / 2, sensor2G);
                acc[(UInt16)c_SENSOR_AXIS.Z] = Scaling(accZ, LINE_GRAPH_HEIGHT / 2, sensor2G);

                //...Acceleromoter 3 Axis Draw
                for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                {
                    //...Get Previous Data
                    Int32 xOld = m_LineGraph.Accelero[i].XOld;
                    Int32 yOld = m_LineGraph.Accelero[i].YOld;

                    //...Changes
                    Int32 xNew = xOld + m_LineGraph.Step;
                    Int32 yNew = (Int32) acc[i];
                    yNew = m_LineGraph.YCenter - yNew;

                    //...Draw
                    m_LineGraph.Canvas.DrawLine(m_LineGraph.Accelero[i].DrawPen, xOld, yOld, xNew, yNew);

                    //...Store change's to Struct
                    m_LineGraph.Accelero[i].XOld = xNew;
                    m_LineGraph.Accelero[i].YOld = yNew;
                }

                //...Display Graphic
                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.LineGraph].Image = m_Bitmap.LineGraph;
            }

            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                //...Additional Display
                float gAccX = 0, gAccY = 0, gAccZ = 0;
                gAccX = Scaling(accX, earthG, sensor2G / 2); //Get at 1G
                gAccY = Scaling(accY, earthG, sensor2G / 2); //Get at 1G
                gAccZ = Scaling(accZ, earthG, sensor2G / 2); //Get at 1G

                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccX].Text = gAccX.ToString();
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccY].Text = gAccY.ToString();
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccZ].Text = gAccZ.ToString();
            }
        }

        public void LineGraphPitchYawRoll(Int32 pitch, Int32 yaw, Int32 roll, float earthG, float sensor2G, byte mode)
        {
            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                //...Clear Graphic
                if (m_LineGraph.Accelero[(UInt16)c_SENSOR_AXIS.X].XOld >= m_LineGraph.XMax)
                {
                    //...Clear
                    m_LineGraph.Canvas.Clear(Color.Black);

                    //...Center horizontal line
                    Pen penGrey = new Pen(Color.Gray, LINE_GRAPH_LINE_BOLD);
                    m_LineGraph.Canvas.DrawLine(penGrey, 0, (UInt16)(LINE_GRAPH_HEIGHT / 2), LINE_GRAPH_WIDTH, (UInt16)(LINE_GRAPH_HEIGHT / 2));
                    penGrey.Dispose();

                    //...Reset Value
                    for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                    {
                        m_LineGraph.Accelero[i].XOld = 0;
                        m_LineGraph.Accelero[i].YOld = (Int32)(LINE_GRAPH_HEIGHT / 2);
                    }
                    return;
                }

                //...Get > to Local var's
                float[] acc = new float[(UInt16)c_SENSOR_AXIS._Count];
                acc[(UInt16)c_SENSOR_AXIS.X] = Scaling(pitch, LINE_GRAPH_HEIGHT / 2, sensor2G);
                acc[(UInt16)c_SENSOR_AXIS.Y] = Scaling(yaw, LINE_GRAPH_HEIGHT / 2, sensor2G);
                acc[(UInt16)c_SENSOR_AXIS.Z] = Scaling(roll, LINE_GRAPH_HEIGHT / 2, sensor2G);

                //...Acceleromoter 3 Axis Draw
                for (UInt16 i = 0; i < (UInt16)c_SENSOR_AXIS._Count; i++)
                {
                    //...Get Previous Data
                    Int32 xOld = m_LineGraph.Accelero[i].XOld;
                    Int32 yOld = m_LineGraph.Accelero[i].YOld;

                    //...Changes
                    Int32 xNew = xOld + m_LineGraph.Step;
                    Int32 yNew = (Int32)acc[i];
                    yNew = m_LineGraph.YCenter - yNew;

                    //...Draw
                    m_LineGraph.Canvas.DrawLine(m_LineGraph.Accelero[i].DrawPen, xOld, yOld, xNew, yNew);

                    //...Store change's to Struct
                    m_LineGraph.Accelero[i].XOld = xNew;
                    m_LineGraph.Accelero[i].YOld = yNew;
                }

                //...Display Graphic
                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.LineGraph].Image = m_Bitmap.LineGraph;
            }
        }

        // Using scale 0 - 1.0F for 1G sensor
        public void PitchRoll(float pitch, float roll, float pitchDegLimit, float rollDegLimit, float sensor2G, byte mode)
        {
            //...RAW calculation
            float pitchDeg = 0, rollDeg = 0;
            //...Limit pitch, roll
            pitchDeg    = -(Scaling(pitch, pitchDegLimit, sensor2G / 2));
            rollDeg     =   Scaling(roll,  rollDegLimit,  sensor2G / 2);

            pitchDeg = (float) Math.Round((decimal)pitchDeg, 0);
            rollDeg = (float) Math.Round((decimal)rollDeg, 0);

            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Pitch].Text  = pitchDeg .ToString();
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Roll].Text   = rollDeg.ToString();
            }

            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_PitchRoll.SetAttitudeIndicatorParameters(pitchDeg, rollDeg);
            }

            // ...
            DrawCube(-pitchDeg, 0,  -rollDeg);
        }

        public void PitchYawRollDegree(Int32 pitch, Int32 yaw, Int32 roll, byte mode)
        {
            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Pitch].Text = pitch.ToString();
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Roll].Text = roll.ToString();
            }

            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_PitchRoll.SetAttitudeIndicatorParameters(pitch, roll);
            }

            // ...
            DrawCube(-pitch, yaw, -roll);
        }

        public void Heading(UInt16 heading, byte mode)
        {            
            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_Heading.SetHeadingIndicatorParameters((UInt16)heading);
            }

            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_LblGeneral[(UInt16)LABEL_LIST.Heading].Text =
                    heading.ToString() + @" " + Char.ConvertFromUtf32((UInt16)c_CHARACTER.Degree);
            }
        }

        public void DrawCube(float x, float y, float z)
        {
            m_MainCube.RotateX = x;
            m_MainCube.RotateY = y;
            m_MainCube.RotateZ = z;

            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Odo3D].Image = m_MainCube.DrawCube(m_DrawCube);
        }

        public void Altimeter(float pressure, Int32 height, bool usePressureCalculation, byte mode)
        {
            //...Get
            if (usePressureCalculation)
            {
                //...W.I.P
            }

            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_Altimeter.SetAlimeterParameters(height);
            }

            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Pressure].Text = pressure.ToString() + @" Pa";
                m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.Height].Text = height.ToString() + @" m";
            }
        }

        public void Humidity(byte humidity, Int16 temperature, byte mode)
        {
            //...Get
            Int16 humidityUp = Calculated_Humidity(humidity);
            
            //...GRAPH
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_Humidity.SetVerticalSpeedIndicatorParameters((Int16)humidityUp);
            }

            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_LblGeneral[(UInt16)LABEL_LIST.Humidity].Text = humidity.ToString() + @" %";
                m_LblGeneral[(UInt16)LABEL_LIST.Temperature].Text =
                    temperature.ToString() + @" " + Char.ConvertFromUtf32((UInt16)c_CHARACTER.Degree) + @"C";
            }
        }

        public void ParseMaps(float latitude, float longitude, UInt16 zoom, UInt16 satellite, byte mode)
        {
            //...TEXT
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Text) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                m_LblGeneral[(UInt16)LABEL_LIST.Latitude].Text = latitude.ToString();
                m_LblGeneral[(UInt16)LABEL_LIST.Longitude].Text = longitude.ToString();
                m_LblGeneral[(UInt16)LABEL_LIST.Satellite].Text = satellite.ToString();
            }

            //...MAPS
            if ((mode == (byte)c_GRAPH_UPDATE_MODE.Graph) || (mode == (byte)c_GRAPH_UPDATE_MODE.Both))
            {
                string url = Utils.MyConfig.s_MapsProvider;
                url = url.Replace(@"LATITUDE", latitude.ToString());
                url = url.Replace(@"LONGITUDE", longitude.ToString());

                m_MapPos.Position = new PointLatLng(latitude, longitude);
                m_GMap.ZoomAndCenterMarkers(GMAP_OVERLAY_1);

                if (zoom > 0)
                {
                    m_GMapZoom = zoom;
                }

                m_GMap.Zoom = m_GMapZoom;
                
            }
        }

        public void Tracker(Int32 latitude, Int32 longitude, Int32 scaleLat, Int32 scaleLong, Int16 xPic, Int16 yPic, byte mode)
        {
            //...Which DebugGUI or Tracker
            bool isDebugMode = false;
            if (xPic != 0 || yPic != 0) //Just check Radar Scale used ot NOT
            {
                isDebugMode = true;          
            }
            else
            {
                isDebugMode = false;
            }

            //...Real Coordinat
            if (!isDebugMode)
            {
                try
                {
                    yPic = (Int16)(Scaling(latitude, SQUARE_CANVAS_LENGHT, scaleLat));
                    xPic = (Int16)(Scaling(longitude, SQUARE_CANVAS_LENGHT, scaleLong));
                }
                catch
                {
                    Komurindo.m_Instance.PositionTracker(false, @"Error Scaling");
                    return;
                }

                //...Out of border
                if (yPic > SQUARE_CANVAS_LENGHT || xPic > SQUARE_CANVAS_LENGHT || yPic < 0 || xPic < 0)
                {
                    Komurindo.m_Instance.PositionTracker(false, @"Out of Border");
                    return;
                }
            }

            //...Clear
            ClearTracker();

            //...Add point
            m_Tracker.Canvas.FillRectangle(m_Tracker.DrawPoint, xPic - 2, yPic - 2, 5, 5);

            //...Set Image
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Tracker].Image = m_Tracker.Image;

            //...Change's system coordinat
            //...... BASIC CONVERT SCREEN - CARTESIUS ......//
            // screenX = (Z * cartX) + (screenWidth / 2)    //
            // screenY = (screenHeight / 2) - (Z * cartY)   // 
            //                                              //
            // cartX = (screenX - (screenWidth / 2)) / Z    //
            // cartY = -(screenY - (screenHeight / 2)) / Z  //
            //..............................................//
            double newX = 0, newY = 0, distance = 0, angle = 0;
            bool isFront = true;
            
            //...New cartesian coordinat
            newX = (double)(xPic - (SQUARE_CANVAS_LENGHT / 2));
            newY = (double)(-(yPic - (SQUARE_CANVAS_LENGHT / 2)));

            //...Distance
            distance = Math.Sqrt(Math.Pow(newY, 2) + Math.Pow(newX, 2)); //R = SQRT(X² + Y²),
            
            //...Angle
            try
            {
                //...Calculated angle
                angle = Math.Atan(newY / newX); //Tan θ = y / x,
                angle = angle / (pi / TRACKER_SERVO_MAX_DEG); //Rad to Degree
                
                //...Change's new angle based on Servo mechanism
                if (angle >= 0)
                {
                    angle -= TRACKER_SERVO_MAX_DEG;
                }
                angle = Math.Abs(angle); //Get positive angle

                //...Should face where
                if (newY < 0)
                {
                    isFront = false;
                }
            }
            catch
            {
                //...DBG Error!
            }

        #if SHOW_DEBUG_VALUE
            if (isDebugMode)
            {
                m_LblGeneral[(UInt16)LABEL_LIST.TrackingStatus].Text = @"GUI : " +
                    xPic.ToString() + @"|" + yPic.ToString() + @"|" + ((Int16)distance).ToString() +
                    @"|" + ((Int16)angle).ToString() + @"|" + isFront.ToString();
            }
            else
            {
                m_LblGeneral[(UInt16)LABEL_LIST.TrackingStatus].Text = @"Real : " +
                    xPic.ToString() + @"|" + yPic.ToString() + @"|" + ((Int16)distance).ToString() +
                    @"|" + ((Int16)angle).ToString() + @"|" + isFront.ToString();
            }
        #else
            Int16 angleUp = (Int16)angle;
            if (!isFront)
            {
                angleUp *= -1;
            }

            m_LblGeneral[(UInt16)LABEL_LIST.TrackingStatus].Text = @" Locked @: " + angleUp.ToString() + @" DEG";
        #endif

            //...Get string
            string strDegX = @"", strDegY = @"";
            UInt16 degX = 0, degY = 0;

            //...Get degree X based servo
            degX = (UInt16)((((UInt16)angle) * TRACKER_SERVO_X_OFFSET) / TRACKER_SERVO_MAX_DEG);
            degX += TRACKER_SERVO_X_MIN;

            //...Get string degree Y based servo
            if (isFront)
            {
                degY = TRACKER_SERVO_Y_MIN;
                strDegY = @"0" + degY.ToString();
            }
            else
            {
                degY = TRACKER_SERVO_Y_MAX;
                strDegY = degY.ToString();
            }

            //...Get string degree X based servo
            if (degX >= 1000) //Use 4 digit format ex : 0010, 0500, 1200
            {
                strDegX = degX.ToString();
            }
            else
            {
                strDegX = @"0" + degX.ToString();
            }

            //...Used command based format
            string command = Komurindo.s_GCSCommand[(UInt16)c_GCS_COMMAND_LIST.ServoTrack];
            command = command.Replace(@"X", strDegX);
            command = command.Replace(@"Y", strDegY);
            
            //...Tracking
            Komurindo.m_Instance.PositionTracker(true, command);
        }

    #if KOMURINDO_PAYLOAD
        //=============================== IMAGE JPEG ======================================//
        //...
        public void DrawJPEGImage(string dataImage)
        {
            Image imgObject;
            int bytesCount = (dataImage.Length) / 2;
            byte[] bytes = new byte[bytesCount];
            for (int x = 0; x < bytesCount; x++)
            {
                bytes[x] = Convert.ToByte(dataImage.Substring(x * 2, 2), 16);
            }
            using (MemoryStream mm = new MemoryStream(bytes))
            {
                imgObject = Image.FromStream(mm);
                m_Bitmap.Camera = null;
                m_Bitmap.Camera = new Bitmap(imgObject); // Should be Save for save as JPEG file purpose

                //...Display Graphic
                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Camera].Image = imgObject;
            }
        }

        public void DrawImage()
        {
            //..Get Raw Data
            string strImage = Komurindo.m_Instance.m_ImageRawData;

            //...used & initial var's
            string strData          = @"";
        #if !USE_STATIC_CAMERA_COUNTER
            Int32   v_DataCount     = 0;
            Int32   v_ColumnCount   = 0;
            Int32   v_RowCount      = 0;
            byte    v_FirstCount    = 0;
            byte    v_RgbCount      = 0;
        #endif
            byte[] data             = new byte[1];
            byte data_R             = 0;
            byte data_G             = 0;
            byte data_B             = 0;
            
            //...Loop
            while (v_DataCount < strImage.Length)
            {
                //...Get
                strData = strImage.Substring(v_DataCount, 1); //Index start from 0
                data = Komurindo.m_Instance.m_Iso_8859_1.GetBytes(strData);
                
                //...counter
                v_DataCount++;
                
                //...check LF
                if (data[0] == (UInt16)c_CHARACTER.Limit) //1st data 255 as LF
                {
                    //...Reset per line
                    v_FirstCount = 0;
                    v_RowCount++;
                    v_ColumnCount = 1;
                }
                else
                {
                    if (v_FirstCount <= 3) //2nd, 3rd, 4th data are Line ID
                    {
                        v_FirstCount++;
                    }
                    if (v_FirstCount >= 4) //4th > data
                    {
                        v_RgbCount++;
                        if (v_RgbCount == 1)
                        {
                            data_R = data[0];
                        }
                        else if (v_RgbCount == 2)
                        {
                            data_G = data[0];
                        }
                        else if (v_RgbCount == 3)
                        {
                            data_B = data[0];
                            
                            //...Draw
                            try
                            {
                                //...Bitmap set
                                m_Bitmap.Camera.SetPixel(v_ColumnCount - 1, v_RowCount - 1, Color.FromArgb(data_R, data_G, data_B));
                                
                                //...Display Graphic
                                m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Camera].Image = m_Bitmap.Camera;
                            }
                            catch
                            {
                                //...Some error! :(
                            }
                            
                            //...Change's
                            v_ColumnCount++;
                            v_RgbCount = 0;
                        }
                    }
                }
            }
            
            //...Display Graphic
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Camera].Image = m_Bitmap.Camera;
        }

        public void DrawImagePercentage(UInt32 val, UInt32 cofactor, bool undeterminate)
        {
            //...Get
            UInt16 width = (UInt16)((val * SQUARE_CANVAS_LENGHT) / cofactor);
            
            //...Draw
            if (!undeterminate)
            {
                m_ProgressCamera.Style = ProgressBarStyle.Blocks;
                m_ProgressCamera.Value = (Int32) Scaling(val, 100, cofactor);
            }
            else
            {
                m_ProgressCamera.Style = ProgressBarStyle.Marquee;
            }


            //...Set Text Label
            m_LblGeneral[(UInt16)LABEL_LIST.CameraPercentage].Text =
            #if SHOW_DEBUG_VALUE
                val.ToString() + @" _ " + 
            #endif
            #if USE_CEILING_CAPTURED_PERCENTAGE
                ((UInt16)Math.Ceiling((double)Scaling(val, 100, cofactor))).ToString() + @" %";
            #else
                (Scaling(val, 100, cofactor)).ToString() + @" %";
            #endif
        }
    #endif

        public void SaveImage(string path)
        {
            path += @".jpg";
            m_Bitmap.Camera.Save(path);
        }

        //=============================== ADDITIONAL CALCULATION =======================//
        private Int16 Calculated_Humidity(byte humidity)
        {
            Int16 max = (Int16)(Math.Abs(HUMIDITY_0_VAL) + Math.Abs(HUMIDITY_100_VAL));
            return (Int16)(((humidity * max) / 100) - Math.Abs(HUMIDITY_0_VAL));
        }

        public static Int32 Scaling(Int32 value, Int32 MaxTargetScale, Int32 maxSourceScale)
        {
            return (Int32)((value * MaxTargetScale) / maxSourceScale);
        }

        public static Int16 Scaling(Int16 value, Int16 MaxTargetScale, Int16 maxSourceScale)
        {
            return (Int16)((value * MaxTargetScale) / maxSourceScale);
        }

        public static float Scaling(float value, float MaxTargetScale, float maxSourceScale)
        {
            return (float)((value * MaxTargetScale) / maxSourceScale);
        }

        //=============================== CLEAR GRAPHIC ================================//
        public void ClearAllGraph()
        {
            ClearGyroGraph();
            ClearLineGraph();
            PitchRoll(ZERO, ZERO, ZERO, ZERO, 2, (byte)c_GRAPH_UPDATE_MODE.Both); //Give last parameter so not divide by zero
            Heading(ZERO, (byte)c_GRAPH_UPDATE_MODE.Both);
            Altimeter(ZERO, ZERO, false, (byte)c_GRAPH_UPDATE_MODE.Both);
            Humidity(ZERO, ZERO, (byte)c_GRAPH_UPDATE_MODE.Both);

            m_DrawCube = new Point(m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Odo3D].Width / 2, m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Odo3D].Height / 2);
            DrawCube(0, 0, 0);

            ClearMaps();
            ClearTracker();
        #if KOMURINDO_PAYLOAD
            ClearCaptureData();
        #endif
        }

    #if KOMURINDO_PAYLOAD
        private void ClearCaptureData()
        {
            //...Main Bitmap
            for(UInt16 i = 0; i < SQUARE_CANVAS_LENGHT; i++)
                for (UInt16 j = 0; j < SQUARE_CANVAS_LENGHT; j++)
                    m_Bitmap.Camera.SetPixel(i, j, Color.WhiteSmoke);
            
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Camera].Image = m_Bitmap.Camera;

            //...Percentage
            m_ProgressCamera.Style = ProgressBarStyle.Blocks;
            m_ProgressCamera.Value = 0;

            //...Label
            m_LblGeneral[(UInt16)LABEL_LIST.CameraPercentage].Text = ZERO.ToString() + @" %";
            
            //...Var's
            Komurindo.m_Instance.m_ImageRawData = @"";
        #if USE_STATIC_CAMERA_COUNTER
            v_DataCount     = 0;
            v_ColumnCount   = 0;
            v_RowCount      = 0;
            v_FirstCount    = 0;
            v_RgbCount      = 0;
        #endif
        }
    #endif

        private void ClearGyroGraph()
        {
            //...Clear graphic
            for (byte i = 0; i < (byte)c_SENSOR_AXIS._Count; i++)
            {
                //...Canvas Clear
                m_BarGraph.Gyro[i].Canvas.Clear(Color.Black);
                //...Draw Center Line
                m_BarGraph.Gyro[i].Canvas.DrawLine(m_BarGraph.DrawPen, m_BarGraph.Gyro[i].XCenter, 0, m_BarGraph.Gyro[i].XCenter, GYRO_BAR_HEIGHT);
            }
            
            //...Display Graphic
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroX].Image = m_Bitmap.Gyro[(UInt16)c_SENSOR_AXIS.X];
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroY].Image = m_Bitmap.Gyro[(UInt16)c_SENSOR_AXIS.Y];
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.GyroZ].Image = m_Bitmap.Gyro[(UInt16)c_SENSOR_AXIS.Z];

            //...Text
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroX].Text = ZERO.ToString();
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroY].Text = ZERO.ToString();
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.GyroZ].Text = ZERO.ToString();
        }

        private void ClearLineGraph()
        {
            //...Clear Canvas
            m_LineGraph.Canvas.Clear(Color.Black);

            //...Center horizontal line
            Pen penGrey = new Pen(Color.Gray, LINE_GRAPH_LINE_BOLD);
            m_LineGraph.Canvas.DrawLine(penGrey, 0, (UInt16)(LINE_GRAPH_HEIGHT / 2), LINE_GRAPH_WIDTH, (UInt16)(LINE_GRAPH_HEIGHT / 2));
            penGrey.Dispose();
            
            //...Reset Value
            for (byte i = 0; i < (byte)c_SENSOR_AXIS._Count; i++)
            {
                m_LineGraph.Accelero[i].XOld = 0;
                m_LineGraph.Accelero[i].YOld = (UInt16)(LINE_GRAPH_HEIGHT / 2);
            }
            
            //...Display Graphic
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.LineGraph].Image = m_Bitmap.LineGraph;

            //...Text
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccX].Text = ZERO.ToString();
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccY].Text = ZERO.ToString();
            m_LblAttitudeValue[(UInt16)ATTITUDE_LABEL_LIST.AccZ].Text = ZERO.ToString();
        }

        private void ClearMaps()
        {
            //...Text
            m_LblGeneral[(UInt16)LABEL_LIST.Latitude].Text = ZERO.ToString();
            m_LblGeneral[(UInt16)LABEL_LIST.Longitude].Text = ZERO.ToString();
        }

        private void ClearTracker()
        {
            //...Clear
            m_Tracker.Canvas.Clear(Color.FromArgb(64, 64, 64));
            
            //...Add Line
            m_Tracker.Canvas.DrawLine(m_Tracker.DrawPen, 0, 100, 200, 100);
            m_Tracker.Canvas.DrawLine(m_Tracker.DrawPen, 100, 0, 100, 200);
            
            //...Add point
            m_Tracker.Canvas.FillRectangle(m_Tracker.DrawBrush, 97, 97, 7, 7);
            
            //...Set Image
            m_PicGraph[(UInt16)GRAPH_PICTURE_LIST.Tracker].Image = m_Tracker.Image;
        }
    }
}