using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source_MFC.Global
{   
    public enum eEQPTYPE
    {
         NONE
       , MFC       
       , MRBT
       , Mobile_X
       , Mobile_R
    }

    public enum eCUSTOMER
    {
          NONE
        , SDI
        , AMKOR
        , SAMSUNGELELC
    }

    public enum eSCENARIOMODE
    {
          PC
        , PLC
    }

    public enum ePAGE : int
    {
         DashBoard
       , System
       , Program
    }

    public enum eVIWER : int
    {
         None
       , Monitor
       , Manual
       , IO
       , TowerLamp
       , Goal
       , PIO
       , FAC
    }

    public enum eEQPSATUS : int
    {
          Init
        , Initing
        , Stop
        , Stopping
        , Idle
        , Run
        , Error
        , EMG
    }

    public enum RobotMode
    {
         None
       , Auto
       , Manual
       , Maint
       , Error
       ,
    }


    public enum eRUNMODE : int
    {
          RUN
        , DRYRUN

        , MAX_MODE
    };

    public enum eOPRGRADE : int
    {
          Operator
        , Maintenance
        , Maker
        , Master
    }

    public enum eDEV : int
    {
          MPlus
        , Vehicle        
        , IO                
    }

    public enum CHANGEMODEBY
    {
          PROCESS
        , EMG
        , SWITCH
        , UIBUTTON
        , MPLUS
        , MANUAL
    }

    public enum eSTATE : int
    {
          None
        , WorkTrg
        , Working
        , Checking
        , Checked
        , Failed
        , Stopping
        , Stopped
        , Error
        , Resume
        , Done
        , Reset
    }

    public enum eUPDATESTATUS
    {
          NONE
        , CHANGED_EQP
        , CHANGED_MODE
        , CHANGED_QGV
        , CHANGED_LDS
        , CHANGED_LIDAR
        , CHANGED_JOB
        , CHANGED_RECIPE
        , CHANGED_PROGARM
        , CHANGED_JOBSTATE
        , CHANGED_PATH
        , CHANGED_JOBTYPE
        , CHANGED_ARRIVED
        , CHANGED_STAGEALIGNED
        , MANUAL_INIT_START
        , MANUAL_WORK_DONE
        , TACTTIME_JOB
    }

    public enum eSEQLIST : int
    {
          EQP_Init
        , Conv_Init
        , Move2ParkingPos
        , Main
        , Move2Dst
        , AlignStage
        , PIO
        , LowerPick
        , UpperPick
        , LowerDropoff
        , UpperDropoff
        , ZMove4DualWrk

        , MAX_SEQ
    }

    public enum eSUBSEQLIST
    {
          SWITCH
        , MPLUSCOMM
        , VECCMD
            
        , MAX_SUB_SEQ
    }

    public enum ePIOTYPE
    {
          None
        , Normal
        , DisableClamp
    }

    


    public enum eINPUT
    {
          MFC_EMG_UP
        , MFC_EMG_LD
        , MFC_Reserved_2
        , MFC_Pre_TrayDtc
        , MFC_Cen_TrayDtc
        , MFC_End_TrayDtc
        , MFC_POS_OK
        , MFC_ConvSpdPulse

        , MFC_Reserved_8
        , MFC_Reserved_9
        , MFC_ConvMtrErr
        , MFC_Reserved_11
        , MFC_Reserved_12
        , MFC_Reserved_13
        , MFC_Reserved_14
        , MFC_PIO_GO

        , MFC_PIO_1
        , MFC_PIO_2
        , MFC_PIO_3
        , MFC_PIO_4
        , MFC_PIO_5
        , MFC_PIO_6
        , MFC_PIO_7
        , MFC_PIO_8


        // 실제 IO List에는 없는 가상 접점
        , VTA_PIO_Valid = 10000
        , VTA_PIO_Ready
        , VTA_PIO_Completed
        , VTA_PIO_MC_ERROR
    }

    public enum eOUTPUT
    {
          MFC_SIGNAL_LAMP_RED
        , MFC_SIGNAL_LAMP_YELLOW
        , MFC_SIGNAL_LAMP_GREEN
        , MFC_SIGNAL_LAMP_BUZZER
        , MFC_BUZZER
        , MFC_Conv_Run
        , MFC_Conv_Reverse
        , MFC_MoveAlarm_LD      
            
        , MFC_PIO_1
        , MFC_PIO_2
        , MFC_PIO_3
        , MFC_PIO_4
        , MFC_PIO_5
        , MFC_PIO_6
        , MFC_PIO_7
        , MFC_PIO_8        

        // 실제 IO List에는 없는 가상 접점
        , VTA_PIO_Ready = 10000
        , VTA_PIO_Completed
    }

    public enum eERROR
    {
          None
        , EMG
        , TEMP2LRAM
        , SoftWareLimtitOver // 발생축 설명필요
        , HwLimit_N // 발생축 설명필요
        , HwLimit_P // 발생축 설명필요
        , ServoAlarm // 발생축 설명필요
        , ServoEnable // 발생축 설명필요
        , HommingTimeout // 발생축 설명필요
        , MotionTimeout // 발생축 설명필요
        , SlidingCurtainClose
        , SlidingCurtainOpen
        , LDSMeasringDistOver
        , LDSCommFailed // 발생LDS 설명필요
        , PIO_T2_Timeout
        , PIO_T4_Timeout
        , PIO_T5_Timeout
        , PIO_T8_Timeout
        , PIO_ABORT
        , MegazineDetected // 매거진이 감지되어 Pickup 동작을 할 수 없음, 상하부
        , NoMegazine       // 매거진이 없어 Dropoff 동작을 할 수 없음, 상하부 
        , BASEZaxTsk_SyncFailed
        , QGVUnabvailable
        , QGVCmdTimeout
        , QGVRotateCmdFailed
        , QGVLocalize
        , BaseStageAlignSenSearchingFailed
        , PathFindingFaild
        , JobIDMissMatch

        , LowerMgzGuideOpen     // 신규 추가 에러!!!!! 2020.08.25
        , LowerMgzGuideClose
        , UpperMgzGuideOpen
        , UpperMgzGuideClose
        , EMG2
        , EMG3
        , EMGSTATUS

        , TEMP_ALARM
        , FAN_ALARM
        , FAN_LOWER_ALARM   // 신규
        , DOOR_OPEN
        , MOTION_SERVO_PWR
        , MOTION_SERVO_CTRL_PWR
        , BUMPER_SIGNAL         // 신규
        , SERVO_ENABLE          // 신규
        , AGV_LEFT_SERVO_ALRAM// 신규
        , AGV_RIGHT_SERVO_ALRAM// 신규M
        , AGV_CHARGER_ALARM
        , EMG_FRONT
        , EMG_REAR
        , LDS_DISTANCE_LIMIT_OVER
    }


    public enum eLOGTYPE
    {
          prdt
        , Comm
        , Gem
        , Err
        , Debug
        , Capture
    }

    public enum eLOGDEVTYPE
    {
          None
        , IO
        , Motion
        , LDS
        , LADAR
    }

    public enum PortName
    {
        COM1,
        COM2,
        COM3,
        COM4,
        COM5,
        COM6,
        COM7,
        COM8,
        COM9,
        COM10,
        COM11,
    }

    public enum eJOBTYPE
    {
          NONE
        , LOADING
        , UNLOADING
        , STANDBY
        , CAHRGE
        , CANCEL
        , ONLYPATH
    }

    public enum eWORKTYPE : int
    {
          NONE
        , PICK
        , DROPOFF
    }

    public enum eCOMSTATUS
    {
          DISCONNECTED
        , CONNECTED
    }

    public enum eIOTYPE
    {
         INPUT
       , OUTPUT
    }

    public enum eSENTYPE
    {
        A
      , B
    }

    public enum eLMAPTYPE : int
    {
          GREEN = 0
        , YELLOW
        , RED
        , BUZZER
    }

    public enum TWRLAMP : int
    {
        OFF
      , ON
      , BLINK
    }

    public enum eUID4VM : int
    {
          NONE = -1
        , TWR_GREEN_OFF             , TWR_GREEN_ON              , TWR_GRREN_BLINK          
        , TWR_YELLOW_OFF = 10       , TWR_YELLOW_ON             , TWR_YELLOW_BLINK          
        , TWR_RED_OFF = 20          , TWR_RED_ON                , TWR_RED_BLINK          
        , TWR_BUZZER_OFF = 30       , TWR_BUZZER_ON     

        , IO_SetList = 100          , IO_RefreshList

        , GOAL_LIST = 200           , GOAL_ITEMS        
        , GOAL_ADD                  , GOAL_DEL              
        , GOLA_HostName             , GOAL_Label                , GOAL_MCType
        , GOAL_PosX                 , GOAL_PosY                 , GOAL_PosR
        , GOAL_EscapeX              , GOAL_EscapeY              , GOAL_EscapeR

        , PIO_0 = 300               , PIO_1                     , PIO_2, PIO_3      
        , PIO_4

        , FAC_EQPType = 400         , FAC_EQPName               , FAC_Customer
        , FAC_SeqMode               , FAC_Language              , FAC_MPlusIP
        , FAC_MPlusPort             , FAC_VehicleIP     

        , DASH_MONI_ALL = 500       , DASH_MONI_EqpState        , DASH_MONI_EqpMode
        , DASH_MONI_VECSTATE        , DASH_MONI_IO
        , DASH_MONI_START           , DASH_MONI_STOP            , DASH_MONI_RESET
        , DASH_MONI_DROPJOB

        , DASH_MNL_ALL = 600        , DASH_MNL_GoalItem         , DASH_MNL_BTN_MAKEORDER    
        , DASH_MNL_RDO_GoalType_0   , DASH_MNL_RDO_GoalType_1   , DASH_MNL_RDO_GoalType_2
        , DASH_MNL_RDO_GoalType_3

        , MAINWIN_ALL = 700         , MAINWIN_EqpState          , MAINWIN_User
        , MAINWIN_CloseMenu         , MAINWIN_OpenMenu          , MAINWIN_Popup_Login
        , MAINWIN_Popup_Logout      , MAINWIN_Popup_Account     , MAINWIN_Popup_Save
        , MAINWIN_Popup_Minimize    , MAINWIN_Popup_Shutdown
    }

    public enum eDATAEXCHANGE
    {
          Load
        , Save        
        , Model2View
        , View2Model
        , StatusUpdate
    }

    public enum eDATATYPE
    {
          NONE
        , _bool
        , _int
        , _str
        , _float
        , _double
    }

    public enum CommandState
    {
        None,
        Assign,
        Enroute,
        Arrived,
        Transferring,
        TransStart,
        CarrierChanged,
        TransComplete,
        UserStopped,
    }

    public enum RobotState
    {
         None
       , NotAssigned
       , Enroute
       , Parked
       , Acquiring
       , Depositing
       , Charging
       , Standby
       ,
    }

    public enum eMPCMD : int
    {
          NONE
        , ERROR
        , JOBSTATE
        , QUERYPATH
        , QUERYWORKTYPE
    }

    public enum eLANGUAGE
    {
          kor
        , eng
        , chn
    }

    public enum eGOALTYPE
    {
          Pickup
        , Dropoff
        , Charge
        , Standby
    }

    public enum eLINE
    {
          None
        , _23
        , _24
        , _25
        , Sharing
    }
    public enum eMFC_MCTYPE
    {
          NONE
        , KCH
        , JR
        , CLEANER
    }

    public enum eSRCHGOALBY
    {
          Map
        , Host
        , Label
    }

    public class DEF_CONST
    {
        public const int SEQ_FINISH = 0;
        public const int SEQ_INIT = 1;
        public const int SEQ_STEP_FINISH = 1000;
        public const int SEQ_MAIN_FINISH = 10000;
        public const double ZERO = 0.000000000000010;
        public const double PIE = 3.141592768;
        public const double COMM_ERR = -999;
        public const double LIMIT_OVER = -9999;
        public const double MOTION_BAND = 0.015;
        public const double ORGMOTION_BAND = 0.015;
        public const int SENTIME = 200;
        public const int MOTOR_REV = 10000;
        public const int MXP_RUN_STATUS = 9;
        public const double BASE_ANG_REV = 1.6145;
        public const double MAX_SOC_VAL = 54.0;
        public const double MIN_SOC_VAL = 48.0;
    }
}
