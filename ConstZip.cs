using System;

namespace LaserBendingMeasurementSystem
{
    /// <summary>
    /// 상수 선언은 여기에
    /// </summary>
    public static class ConstZip
    {
        public const string URI_RES_PATH = "pack://application:,,,/Resource/"; // 이미지 리소스 주소 (실행파일의 주소 + Resources폴더)
        
        // 컨트롤러 IP 문자열
        public const string CONTROLLER_DISCONNECTED = "Disconnected";

        // 상태 이미지 문자열
        public const string CONTROLLER_STATE_OFF = "ControllerStateOFF.png"; 
        public const string CONTROLLER_STATE_STOP = "ControllerStateSTOP.png";
        public const string CONTROLLER_STATE_RUN = "ControllerStateRUN.png";
        public const string CONTROLLER_STATE_ERROR = "ControllerStateERR.png";

        // 레이저 연결 문자열
        public const string ERR_LASER_CONNECT = "레이저 컨트롤러 Ethernet 연결에 실패했습니다.";
        public const string SCS_LASER_CONNECT = "레이저 컨트롤러 Ethernet 연결에 성공했습니다. IP : ";

        // 레이저 세팅 문자열
        public const string LASER_SAMPLING = "샘플링 주기";
        public const string LASER_DYNAMIC_RANGE = "다이나믹 레인지";
        public const string LASER_EXPOSURE_TIME = "노광 시간";
        public const string LASER_EXPOSURE_MODE = "노광 모드";
        public const string LASER_MULTI_COMBINE = "멀티 발광(합성)";
        public const string LASER_MULTI_OPTIMIZE = "멀티 발광(광량 최적화)";
        public const string LASER_DETECTION_SENSITIVITY = "검출 감도";
        public const string LASER_INVALID_DATA_PROCESSING = "무효 데이터 보간 점수";
        public const string LASER_DETECTION_MODE = "복수 피크 처리";
        public const string LASER_INVALID_BLURRED_LIGHT_FILTER = "복수 피크 폭 필터";
        public const string LASER_IRREGULAR_REFLECTION_REMOVAL = "복수 피크 미광 억제";

        // 메시지박스
        public const string MSG_RESET_TITLE = "초기화";
        public const string MSG_RESET_INFO = "변경되기 이전 값으로 돌아갑니까?";
        public const string MSG_DISCONNECTED_INFO = "디바이스가 연결되어 있지 않습니다.";
        public const string MSG_SETTINGS_SAVED_INFO = "설정이 저장되었습니다.";
        public const string MSG_GET_SETTINGS_INFO = "현재 설정을 가져왔습니다.";

    }
}
