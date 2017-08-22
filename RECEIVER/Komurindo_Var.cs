namespace MataDewa4
{
    enum c_CONTROL_LIST
    {
        Attitude,
        Trajectory,
        Camera,
        Position,
        Setting,
        Tools,
        Home,
        _Count
    }

    enum c_RUN_CONTROL_LIST
    {
        Play,
        Suspend,
        Stop,
        Refresh,
        Save,
        Engine,
        Clear,
        _Count
    }

    enum c_ICON_COLOR_LIST
    {
        Normal_Black,
        Normal_White,
        Select_Orange,
        Select_Blue,
        Select_Green,
        _Count
    }

    enum c_FORM_TYPE_LIST
    {
        WholeScreen,
        WholeRight,
        WholeCenter,
        WholeTopLeft,
        MidTop,
        MidCenter,
        MidBottom,
        LittleLeft,
        TopLeft,
        MidRight,
        _Count
    }

    enum c_PICTURE_LIST
    {
        Team,
        UNESA,
        RunCover,
        _Count
    }

    enum c_BUTTON_PICTURE_LIST
    {
        Visit,
        Close,
        Minimize,
        Open,
        Shut,
        Calibrate,
        RGB,
        Grayscale,
        Upload,
        Download,
        Apply,
        Cancel,
        Monitor,
        Confirm,
        MapRefresh,
        CheckerAccelero,
        CheckerGyro,
        CheckerCompass,
        CheckerGPS,
        CheckerPressure,
        CheckerHumidity,
        CheckerCamera,
        CheckerHeight,
        CheckerCalibHeight,
        CheckerTemperature,
        CheckerStop,
        CheckerExit,
        _Count
    }

    enum c_COMMAND_LIST
    {
        //...All Contest Type
        GetID,
        GetPosition,
        GetPressure,
        GetCompass,
        GetHeight,
        CalibHeight,
        CheckModule,
        //...Launch
        Arming,
        Disarming,
        TakeOff,
        Parachute,
        //...Contest specific
        Contest_Launch,
        Contest_Stop,
        Contest_Exit,
    #if KOMURINDO_PAYLOAD || KOMURINDO_ROCKET
        GetAccelero,
        GetGyro,
        //...Contest specific
        Contest_Tracking,
        Contest_Trajectory,
    #endif
    #if KOMURINDO_PAYLOAD
        Contest_Capture,
        GetCamera,
    #endif
    #if BALON_ATMOSFER || KOMURINDO_ROCKET
        GetHumidity,
        GetTemperature,
        //...Contest specific
        Contest_AfterLaunch,
    #endif
        _Count
    }

    enum c_GCS_COMMAND_LIST
    {
        ServoTest,
        ServoFaceFront,
        ServoTrack,
        _Count
    }

    enum c_CALIB_LIST
    {
        Position,
        Pressure,
        Heading,
        _Count
    }

    enum c_SENSOR_AXIS
    {   
        X,
        Y,
        Z,
        _Count
    }

    enum c_DATA_MODE
    {
        CMPS8bit,
        KMR14,
        _Count
    }

    enum c_IMAGE_TYPE
    {
        RGB,
        Grayscale,
        _Count
    }

    enum c_LEVEL
    {
        Main,
        Second,
        _Count
    }

    enum c_CHARACTER
    {
        Enter       = 13,
        LineFeed    = 10,
        Space       = 32,
        Degree      = 176,
        Limit       = 255
    }

    enum c_GRAPH_UPDATE_MODE
    {
        Graph,
        Text,
        Both,
        None
    }

    enum c_COORDINAT_TEST
    {
        Home,
        Scale1,
        Scale2,
        Scale3,
        Scale4,
        Track,
        _Count
    }

    //...CALIB
    public struct s_CALIB_DATA
    {
        public float HomeLatitude;
        public float HomeLongitude;
        public int PressureZero;
        public int HeadingZero;
    }

    // External Button
    enum c_EXTERNAL_BUTTON
    {
        Arming,
        TakeOff,
        Parachute,
        Disarming,
        _Count
    }
}