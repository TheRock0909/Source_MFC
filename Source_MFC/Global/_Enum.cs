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

    public enum eROBOTMODE
    {
         AUTO
       , IDLE
       , MANUAL
       , PM
       , ERROR
    }
    public enum eERRST
    {
         Clear
       , Occur
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

    public enum eBackUpType
    {
          Default
        , Bak
        , DateBak
    }

    public enum eJsonName
    {
          Cfg
        , Goal
        , Status
        , IO
    }

    public enum eSEQLIST : int
    {
          Main
        , EscapeEQP
        , Move2Dst        
        , PIO
        , Pick
        , Drop

        , MAX_SEQ
    }

    public enum eTASKLIST
    {
          SWITCH
        , MPLUSCOMM
        , VECCMD
            
        , MAX_SUB_SEQ
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
        , MFC_START
        , MFC_STOP
        , MFC_RESET
        , MFC_AUTO_MANUAL
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

        , MFC_SWLMP_START
        , MFC_SWLMP_STOP
        , MFC_SWLMP_RESET
        , MFC_SWLMP_AUTO_MANUAL

        // 실제 IO List에는 없는 가상 접점
        , VTA_PIO_Ready = 10000
        , VTA_PIO_Completed
    }

    public enum eERROR
    {
          None
        , Clear
        , EMG
        , TrayDetected
        , NoneofTrays
        , TraysJaming
        , DockingFiled
        , VEC_UNAVAILAABLE
        , VEC_COMMTIMEOUT
        , VEC_Move2Failed
        , VEC_Unnormal
        , PIO_Valid
        , PIO_Ready
        , PIO_Complete
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

        , IO_SetList = 100          , IO_RefreshList            , IO_ResetDirectIO

        , GOAL_LIST = 200           , GOAL_ITEMS        
        , GOAL_ADD                  , GOAL_DEL              
        , GOLA_HostName             , GOAL_Label                , GOAL_MCType
        , GOAL_PosX                 , GOAL_PosY                 , GOAL_PosR
        , GOAL_EscapeX              , GOAL_EscapeY              , GOAL_EscapeR

        , PIO_0 = 300               , PIO_1                     , PIO_2
        , PIO_3                     , PIO_4                     
        , SENDELAY                  , COMM_TIMEOUT      
        , CONVSPD_NORMAL            , CONVSPD_SLOW

        , FAC_EQPType = 400         , FAC_EQPName               , FAC_Customer
        , FAC_SeqMode               , FAC_Language              , FAC_MPlusIP
        , FAC_MPlusPort             , FAC_VehicleIP     

        , DASH_MONI_ALL = 500       , DASH_MONI_EqpState        
        , DASH_MONI_VECSTATE        , DASH_MONI_IO
        , DASH_MONI_START           , DASH_MONI_STOP            , DASH_MONI_RESET
        , DASH_MONI_DROPJOB         , DASH_MONI_JOB_Assigned    , DASH_MONI_JOB_Update
        , DASH_MONI_JOB_Reset       , DASH_MONI_JOB_PioStart

        , DASH_MNL_ALL = 600        , DASH_MNL_GoalItem         , DASH_MNL_BTN_MAKEORDER    
        , DASH_MNL_RDO_GoalType_0   , DASH_MNL_RDO_GoalType_1   , DASH_MNL_RDO_GoalType_2
        , DASH_MNL_RDO_GoalType_3   , DASH_MNL_SEQ_SELECTION
        , DASH_MNL_SEQ_INIT         , DASH_MNL_SEQ_START        , DASH_MNL_SEQ_STOP
        , DASH_MNL_VECTSK_INIT      , DASH_MNL_VECTSK_START     , DASH_MNL_VECTSK_STOP
        , DASH_MNL_VTSK_PARA_GOAL1  , DASH_MNL_VTSK_PARA_GOAL2
        , DASH_MNL_VTSK_PARA_POSX   , DASH_MNL_VTSK_PARA_POSY   , DASH_MNL_VTSK_PARA_POSR
        , DASH_MNL_VTSK_PARA_MOVEX  , DASH_MNL_VTSK_PARA_SPEED  , DASH_MNL_VTSK_PARA_ACC    
        , DASH_MNL_VTSK_PARA_DCC    , DASH_MNL_VTSK_PARA_MSG



        , MAINWIN_ALL = 700         , MAINWIN_EqpState          , MAINWIN_User
        , MAINWIN_CloseMenu         , MAINWIN_OpenMenu          , MAINWIN_Popup_Login
        , MAINWIN_Popup_Logout      , MAINWIN_Popup_Account     , MAINWIN_Popup_Save
        , MAINWIN_Popup_Minimize    , MAINWIN_Popup_Shutdown

        , ETC_INPUTBOX  = 100
    }

    public enum eCMD4MPLUS
    {
          NONE
        , SRC               // MPlus 2 AMR
        , DST               // MPlus 2 AMR
        , REMOTE            // MPlus 2 AMR
        , DISTANCEBTW       // MPlus 2 AMR
        , STANDBY           // MPlus 2 AMR
        , CHARGE            // MPlus 2 AMR
        , MANUAL            // MPlus 2 AMR            

        , STATUS            // AMR 2 MPlus
        , JOB               // AMR 2 MPlus            
        , ERROR             // AMR 2 MPlus            
        , MANUAL_MAG_INST   // AMR 2 MPlus
        , MANUAL_MAG_UNINST // AMR 2 MPlus
        , GOAL_LD
        , GOAL_UL
        , STOCKER            
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

    public enum eJOBST
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

    public enum eROBOTST
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
    public enum ePIOTYPE
    {
          NOUSE
        , KCH
        , JR
        , CLR
    }

    public enum eSRCHGOALBY
    {
          Map
        , Host
        , Label
    }

    public enum eREMOTE_MODE
    {
          NONE
        , RESUME
        , PAUSE
        , ABORT
        , CANCEL
    }

    public enum eREMOTE_MODE_REPLY
    {
          NONE
        , RESUMED
        , PAUSED
        , ABORTED
        , CANCELED
    }

    public enum eMNL_INST
    {
          NONE
        , INSTALL
        , UNINSTALL
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
