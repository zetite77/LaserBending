using LJX8_DllSampleAll;
using System;
using System.Windows;
using System.Windows.Controls;
using LJX8_DllSampleAll.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LaserBendingMeasurementSystem.Page
{
    /// <summary>
    /// MenuSettings_PageRecipe.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuSettings : UserControl
    {

        #region Enum

        /// <summary>
        /// Send command definition
        /// </summary>
        /// <remark>Defined for separate return code distinction</remark>
        private enum SendCommand
        {
            /// <summary>None</summary>
            None,
            /// <summary>Restart</summary>
            RebootController,
            /// <summary>Trigger</summary>
            Trigger,
            /// <summary>Start measurement</summary>
            StartMeasure,
            /// <summary>Stop measurement</summary>
            StopMeasure,
            /// <summary>Get profiles</summary>
            GetProfile,
            /// <summary>Get batch profiles</summary>
            GetBatchProfile,
            /// <summary>Initialize Ethernet high-speed data communication</summary>
            InitializeHighSpeedDataCommunication,
            /// <summary>Request preparation before starting high-speed data communication</summary>
            PreStartHighSpeedDataCommunication,
            /// <summary>Start high-speed data communication</summary>
            StartHighSpeedDataCommunication,
        }

        #endregion

        #region Field
        private readonly MainWindow mainWindow;

        /// <summary>Ethernet settings structure </summary>
        private LJX8IF_ETHERNET_CONFIG _ethernetConfig;
        /// <summary>Current device ID</summary>
        private int _currentDeviceId;
        /// <summary>Send command</summary>
        private SendCommand _sendCommand;
        /// <summary>Callback function used during high-speed communication</summary>
        /// The following are maintained in arrays to support multiple controllers.
        /// <summary>Array of profile information structures</summary>
        private LJX8IF_PROFILE_INFO[] _profileInfo;
        /// <summary>Array of controller information</summary>
        private DeviceData _deviceData;
        /// <summary>Array of labels that indicate the controller status</summary>
        private static bool[] _isBufferFull = new bool[NativeMethods.DeviceCount];
        /// <summary>Array of value of stop processing has done by buffer full error</summary>
        private static bool[] _isStopCommunicationByError = new bool[NativeMethods.DeviceCount];

        private LJX8IF_TARGET_SETTING targetSetting;

        #endregion
        Image im, im2, im3, im4, im5, im6;

        #region Constructor
        public MenuSettings(MainWindow parent)
        {
            InitializeComponent();

            mainWindow = parent;
            _sendCommand = SendCommand.None;
            _deviceData = new DeviceData();
            _profileInfo = new LJX8IF_PROFILE_INFO[NativeMethods.DeviceCount];

            // 이 앱에서 프로그램(byType)은 0(0x10)에서 변하지 않음.
            // Target은 전부 0에서 변하지 않음. 
            targetSetting.byType = 0x10;
            targetSetting.reserve =
            targetSetting.byTarget1 =
            targetSetting.byTarget2 =
            targetSetting.byTarget3 =
            targetSetting.byTarget4 = 0x00;

            ComboBox1.Items.Clear(); //레시피 콤보박스  전부 없애기
            DirectoryInfo di = new DirectoryInfo("Setting"); //세팅 폴더
            //폴더가 존재하지 않으면
            if (di.Exists == false) { di.Create(); }
            foreach (FileInfo item in di.GetFiles()) //세팅 폴더 안의 모든 파일 가져오기
            {
                ComboBox1.Items.Add(item.Name.Substring(0, item.Name.Length - 4)); //레시피 콤보박스에 파일 이름들 추가

            }
            var bm = new BitmapImage(new Uri(@"/Resource/point1.png", UriKind.RelativeOrAbsolute));
            var bm2 = new BitmapImage(new Uri(@"/Resource/point2.png", UriKind.RelativeOrAbsolute));
            var bm3 = new BitmapImage(new Uri(@"/Resource/point3.png", UriKind.RelativeOrAbsolute));
            var bm4 = new BitmapImage(new Uri(@"/Resource/bar_p1.png", UriKind.RelativeOrAbsolute));
            var bm5 = new BitmapImage(new Uri(@"/Resource/bar_p2.png", UriKind.RelativeOrAbsolute));
            var bm6 = new BitmapImage(new Uri(@"/Resource/bar_p3.png", UriKind.RelativeOrAbsolute));
            im = new Image { Source = bm }; im2 = new Image { Source = bm2 }; im3 = new Image { Source = bm3 };
            im4 = new Image { Source = bm4 }; im5 = new Image { Source = bm5 }; im6 = new Image { Source = bm6 };
            im.Width = 20; im2.Width = 20; im3.Width = 20;
            im4.Width = 200; im5.Width = 200; im6.Width = 200;
            im.Height = 20; im2.Height = 20; im3.Height = 20;
            im4.Height = 5; im5.Height = 5; im6.Height = 5;
            im.Opacity = 0.5; im2.Opacity = 0.5; im3.Opacity = 0.5;
            im4.Opacity = 0.5; im5.Opacity = 0.5; im6.Opacity = 0.5;
            Canvas.SetLeft(im, 0); Canvas.SetLeft(im2, 50); Canvas.SetLeft(im3, 20);
            Canvas.SetLeft(im4, 10); Canvas.SetLeft(im5, 10); Canvas.SetLeft(im6, 10);
            Canvas.SetTop(im, 0); Canvas.SetTop(im2, 0); Canvas.SetTop(im3, 100);
            Canvas.SetTop(im4, 10); Canvas.SetTop(im5, 30); Canvas.SetTop(im6, 70);
            Point_Canvas.Children.Add(im); Point_Canvas.Children.Add(im2); Point_Canvas.Children.Add(im3);
            Point_Canvas.Children.Add(im4); Point_Canvas.Children.Add(im5); Point_Canvas.Children.Add(im6);
        }
        #endregion

        #region Event
        private void LSBtn_Connect_Click(object sender, RoutedEventArgs e)
        {
            LaserInitialize();
            ConnectEthernetIP();
        }
        

        private void LSBtn_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (_deviceData.Status == DeviceStatus.Ethernet)
            {
                if (MessageBox.Show(ConstZip.MSG_RESET_INFO, ConstZip.MSG_RESET_TITLE, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    GetSettings();
                }
            }
            else
            {
                _ = MessageBox.Show(ConstZip.MSG_DISCONNECTED_INFO);
            }
        }

        private void LSBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (_deviceData.Status == DeviceStatus.Ethernet)
            {
                Cursor = System.Windows.Input.Cursors.Wait;
                SetSettings();
                GetSettings();
                Cursor = System.Windows.Input.Cursors.Arrow;
            }
            else
            {
                _ = MessageBox.Show(ConstZip.MSG_DISCONNECTED_INFO);
            }
        }

        private void LSSlider_InvalidDataProcessing_ValueChanged
            (object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LSTBox_InvalidDataProcessing.Text =
                LSSlider_InvalidDataProcessing.Value.ToString();
        }

        private void LSCBox_ExposureMode_SelectionChanged
            (object sender, SelectionChangedEventArgs e)
        {
            UIVisibilityChange();
        }
        #endregion

        #region Method
        /// <summary>
        /// 레이저 컨트롤러 초기화
        /// </summary>
        private void LaserInitialize()
        {
            mainWindow.LogIOStream("레이저 컨트롤러 초기화 호출");

            _ = NativeMethods.LJX8IF_Initialize();
            _deviceData.Status = DeviceStatus.NoConnection;

            mainWindow.LogIOStream("레이저 컨트롤러 초기화 완료");
        }

        /// <summary>
        /// 레이저 컨트롤러 소멸자
        /// </summary>
        public void LaserFinalize()
        {
            if (_deviceData.Status == DeviceStatus.Ethernet)
            {
                mainWindow.LogIOStream("레이저 컨트롤러 소멸자 호출");

                _ = NativeMethods.LJX8IF_Finalize();
                _deviceData.Status = DeviceStatus.NoConnection;

                mainWindow.LogIOStream("레이저 컨트롤러 소멸자 완료");
            }
        }

        /// <summary>
        /// 레이저 컨트롤러 연결 처리
        /// </summary>
        private void ConnectEthernetIP()
        {
            mainWindow.LogIOStream("레이저 컨트롤러 IP연결 호출");
            try
            {
                byte[] tmpIp = new byte[4];
                int idx = 0;
                string ipAddr = LSTBox_IP.Text;
                string portAddr = LSTBox_Port.Text;

                foreach (string item in ipAddr.Split('.'))
                {
                    tmpIp[idx++] = byte.Parse(item);
                }

                _ethernetConfig.abyIpAddress = tmpIp;
                _ethernetConfig.wPortNo = ushort.Parse(portAddr);
                _ethernetConfig.reserve = null;

                int rc = NativeMethods.LJX8IF_EthernetOpen(_currentDeviceId, ref _ethernetConfig);
                if (rc == (int)Rc.Ok)
                {
                    _deviceData.Status = DeviceStatus.Ethernet;
                    _deviceData.EthernetConfig = _ethernetConfig;
                    mainWindow.LaserIP = ipAddr;
                    mainWindow.LaserState = CONTROLLER_STATE.STOP;
                    _ = MessageBox.Show(ConstZip.SCS_LASER_CONNECT + ipAddr);
                    mainWindow.LogIOStream(ConstZip.SCS_LASER_CONNECT + ipAddr);

                    GetSettings();
                }
                else
                {
                    _deviceData.Status = DeviceStatus.NoConnection;
                    mainWindow.LaserIP = ConstZip.CONTROLLER_DISCONNECTED;
                    _ = MessageBox.Show(ConstZip.ERR_LASER_CONNECT);
                    mainWindow.LogIOStream(ConstZip.ERR_LASER_CONNECT);
                }
                mainWindow.LogIOStream("레이저 컨트롤러 IP연결 완료");
                mainWindow.RefreshIPState();
            }
            catch (Exception e)
            {
                mainWindow.LogIOStream(e.Message);
                _ = MessageBox.Show(e.Message);
            }
        }


        /// <summary>
        /// 각 UI Control의 설정 내용 대로 ReflectSettings함수로 전달
        /// </summary>
        private void SetSettings()
        {
            if (_deviceData.Status == DeviceStatus.Ethernet)
            {
                // 간혹 인수 중 +1 은 ComboBox의 Index와 레이저 컨트롤러의 Index가 1씩 차이나서 넣음.
                // 샘플링 주기
                ReflectSettings(ConstZip.LASER_SAMPLING, 0x00, 0x02, new byte[1] { (byte)LSCBox_Sampling.SelectedIndex });
                // 다이나믹 레인지
                ReflectSettings(ConstZip.LASER_DYNAMIC_RANGE, 0x01, 0x05, new byte[1] { (byte)(LSCBox_DynamicRange.SelectedIndex + 1) });
                // 노광 시간
                ReflectSettings(ConstZip.LASER_EXPOSURE_TIME, 0x01, 0x06, new byte[1] { (byte)LSCBox_ExposureTime.SelectedIndex });
                // 노광 모드
                ReflectSettings(ConstZip.LASER_EXPOSURE_MODE, 0x01, 0x07, new byte[1] { (byte)LSCBox_ExposureMode.SelectedIndex });
                // 멀티 발광(합성)
                if (LSCBox_ExposureMode.SelectedIndex == 1)
                {
                    ReflectSettings(ConstZip.LASER_MULTI_COMBINE, 0x01, 0x09, new byte[1] { (byte)LSCBox_MultiCombine.SelectedIndex });
                }
                // 멀티 발광(광량최적화)
                else if (LSCBox_ExposureMode.SelectedIndex == 2)
                {
                    ReflectSettings(ConstZip.LASER_MULTI_OPTIMIZE, 0x01, 0x08, new byte[1] { (byte)LSCBox_MultiOptimize.SelectedIndex });
                }
                // 검출 감도
                ReflectSettings(ConstZip.LASER_DETECTION_SENSITIVITY, 0x01, 0x0F, new byte[1] { (byte)(LSCBox_DetectionSensitivity.SelectedIndex + 1) });
                // 무효 데이터 보간 점수
                ReflectSettings(ConstZip.LASER_INVALID_DATA_PROCESSING, 0x01, 0x10, new byte[1] { (byte)LSSlider_InvalidDataProcessing.Value });
                // 복수 피크 처리
                ReflectSettings(ConstZip.LASER_DETECTION_MODE, 0x01, 0x11, new byte[1] { (byte)LSCBox_DetectionMode.SelectedIndex });
                // 복수 피크 폭 필터
                if (LSCBox_BlurredLightFilter.SelectedIndex == 0)
                {
                    ReflectSettings(ConstZip.LASER_INVALID_BLURRED_LIGHT_FILTER, 0x01, 0x12, new byte[4] { 0x00, 0x01, 0x00, 0x00 });
                }
                else
                {
                    ReflectSettings(ConstZip.LASER_INVALID_BLURRED_LIGHT_FILTER, 0x01, 0x12, new byte[4] { 0x01, (byte)LSCBox_BlurredLightFilter.SelectedIndex, 0x00, 0x00 });
                }
                // 복수 피크 미광 억제
                if (LSCBox_IrregularReflectionRemoval.SelectedIndex == 0)
                {
                    ReflectSettings(ConstZip.LASER_IRREGULAR_REFLECTION_REMOVAL, 0x01, 0x14, new byte[4] { 0x00, 0x01, 0x00, 0x00 });
                }
                else
                {
                    ReflectSettings(ConstZip.LASER_IRREGULAR_REFLECTION_REMOVAL, 0x01, 0x14, new byte[4] { 0x01, (byte)LSCBox_IrregularReflectionRemoval.SelectedIndex, 0x00, 0x00 });
                }

                mainWindow.LogIOStream(ConstZip.MSG_SETTINGS_SAVED_INFO);
                _ = MessageBox.Show(ConstZip.MSG_SETTINGS_SAVED_INFO);
            }
            else
            {
                _ = MessageBox.Show(ConstZip.MSG_DISCONNECTED_INFO);
            }
        }

        /// <summary>
        /// 각 UI Control 설정 내용을 레이저에 적용
        /// </summary>
        /// <param name="strSettings">설정 항목 문자열</param>
        /// <param name="_byCategory">설정 대항목 byte</param>
        /// <param name="_byItem">설정 소항목 byte</param>
        /// <param name="data">실제 설정 되는 내용</param>
        /// <param name="depth">설정 저장 방법 (0x00 : 설정 쓰기 영역, 0x01 : 동작 중 설정 영역, 0x02 : 저장용 영역)</param>
        private void ReflectSettings(string strSettings, byte _byCategory, byte _byItem, byte[] data, byte depth = 0x01)
        {
            mainWindow.LogIOStream(strSettings + "세팅 호출");

            targetSetting.byCategory = _byCategory;
            targetSetting.byItem = _byItem;

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint error = 0;
                string strMsg;
                int rc = NativeMethods.LJX8IF_SetSetting(
                    _currentDeviceId,
                    depth,
                    targetSetting,
                    pin.Pointer,
                    (uint)data.Length,
                    ref error
                    );

                if (rc == 0x0000) // Setting OK
                {
                    strMsg = strSettings + " 세팅 완료 : 0x" + rc.ToString("X4"); // 4자릿수 16진수로
                    mainWindow.LogIOStream(strMsg);
                }
                else // Setting NG
                {
                    strMsg = strSettings + " 세팅 중 오류 발생 : 0x" + rc.ToString("X4");
                    if (error != 0x0000)
                    {
                        strMsg += "\r\nfatal error : 0x" + error.ToString("X4");
                    }

                    _ = MessageBox.Show(strMsg);
                    mainWindow.LogIOStream(strMsg);
                }
            }
        }

        /// <summary>
        /// 레이저의 각 설정 항목의 내용 대로 GetReflectSettings함수로 전달
        /// </summary>
        private void GetSettings()
        {
            // 샘플링 주기
            GetReflectedSettings(ConstZip.LASER_SAMPLING, 0x00, 0x02,
                LSCBox_Sampling);
            // 다이나믹 레인지
            GetReflectedSettings(ConstZip.LASER_DYNAMIC_RANGE, 0x01, 0x05,
                LSCBox_DynamicRange);
            // 노광 시간
            GetReflectedSettings(ConstZip.LASER_EXPOSURE_TIME, 0x01, 0x06,
                LSCBox_ExposureTime);
            // 노광 모드
            GetReflectedSettings(ConstZip.LASER_EXPOSURE_MODE, 0x01, 0x07,
                LSCBox_ExposureMode);
            // 멀티 발광(합성)
            if (LSCBox_ExposureMode.SelectedIndex == 1)
            {
                GetReflectedSettings(ConstZip.LASER_MULTI_COMBINE, 0x01, 0x09, LSCBox_MultiCombine);
            }
            // 멀티 발광(광량최적화)
            else if (LSCBox_ExposureMode.SelectedIndex == 2)
            {
                GetReflectedSettings(ConstZip.LASER_MULTI_OPTIMIZE, 0x01, 0x08, LSCBox_MultiOptimize);
            }
            // 검출 감도
            GetReflectedSettings(ConstZip.LASER_DETECTION_SENSITIVITY, 0x01, 0x0F,
                LSCBox_DetectionSensitivity);
            // 무효 데이터 보간 점수
            GetReflectedSettings(ConstZip.LASER_INVALID_DATA_PROCESSING, 0x01, 0x10,
                LSSlider_InvalidDataProcessing);
            // 복수 피크 처리
            GetReflectedSettings(ConstZip.LASER_DETECTION_MODE, 0x01, 0x11,
                LSCBox_DetectionMode);
            // 복수 피크 폭 필터
            if (LSCBox_BlurredLightFilter.SelectedIndex == 0)
            {
                GetReflectedSettings(ConstZip.LASER_INVALID_BLURRED_LIGHT_FILTER, 0x01, 0x12,
                    LSCBox_BlurredLightFilter);
            }
            else
            {
                GetReflectedSettings(ConstZip.LASER_INVALID_BLURRED_LIGHT_FILTER, 0x01, 0x12,
                    LSCBox_BlurredLightFilter);
            }
            // 복수 피크 미광 억제
            if (LSCBox_IrregularReflectionRemoval.SelectedIndex == 0)
            {
                GetReflectedSettings(ConstZip.LASER_IRREGULAR_REFLECTION_REMOVAL, 0x01, 0x14,
                    LSCBox_IrregularReflectionRemoval);
            }
            else
            {
                GetReflectedSettings(ConstZip.LASER_IRREGULAR_REFLECTION_REMOVAL, 0x01, 0x14,
                    LSCBox_IrregularReflectionRemoval);
            }

            mainWindow.LogIOStream(ConstZip.MSG_GET_SETTINGS_INFO);
        }

        /// <summary>
        /// 레이저의 각 설정항목마다의 내용들을 UI Control에 적용
        /// </summary>
        /// <param name="strSettings">설정 항목 문자열</param>
        /// <param name="_byCategory">설정 대항목 byte</param>
        /// <param name="_byItem">설정 소항목 byte</param>
        /// <param name="control">해당 UI Control</param>
        /// <param name="depth">설정 저장 방법 (0x00 : 설정 쓰기 영역, 0x01 : 동작 중 설정 영역, 0x02 : 저장용 영역)</param>
        private void GetReflectedSettings(string strSettings, byte _byCategory, byte _byItem, Slider control, byte depth = 0x01)
        {
            mainWindow.LogIOStream(strSettings + "설정 가져오기 호출(Slider)");

            targetSetting.byCategory = _byCategory;
            targetSetting.byItem = _byItem;
            byte[] data = new byte[1];

            using (PinnedObject pin = new PinnedObject(data))
            {
                string strMsg;
                int rc = NativeMethods.LJX8IF_GetSetting(
                    _currentDeviceId,
                    depth,
                    targetSetting,
                    pin.Pointer,
                    (uint)data.Length
                    );

                if (rc == 0x0000) // Getting OK
                {
                    control.Value = data[0];

                    strMsg = strSettings + " 설정 가져오기 완료 : 0x" + rc.ToString("X4"); // 4자릿수 16진수로
                    mainWindow.LogIOStream(strMsg);
                }
                else // Getting NG
                {
                    strMsg = strSettings + " 설정 가져오기 중 오류 발생 : 0x" + rc.ToString("X4");

                    _ = MessageBox.Show(strMsg);
                    mainWindow.LogIOStream(strMsg);
                }
            }

        }

        /// <summary>
        /// GetReflectedSettings 오버로딩 +2
        /// </summary>
        private void GetReflectedSettings(string strSettings, byte _byCategory, byte _byItem, ComboBox control, byte depth = 0x01)
        {
            mainWindow.LogIOStream(strSettings + "설정 가져오기 호출(ComboBox)");

            targetSetting.byCategory = _byCategory;
            targetSetting.byItem = _byItem;
            byte[] data = control.Name == LSCBox_BlurredLightFilter.Name ||
                control.Name == LSCBox_IrregularReflectionRemoval.Name
                ? (new byte[2])
                : (new byte[1]);

            using (PinnedObject pin = new PinnedObject(data))
            {
                string strMsg;
                int rc = NativeMethods.LJX8IF_GetSetting(
                    _currentDeviceId,
                    depth,
                    targetSetting,
                    pin.Pointer,
                    (uint)data.Length
                    );

                if (rc == 0x0000) // Getting OK
                {
                    // 난해함 주의 (서식 이렇게 수정하라고 vs가 추천함;)
                    control.SelectedIndex = control.Name == LSCBox_BlurredLightFilter.Name ||
                        control.Name == LSCBox_IrregularReflectionRemoval.Name
                        ? data[0] == 0x00 ? 0 : data[1]
                        : control.Name == LSCBox_DynamicRange.Name ||
                          control.Name == LSCBox_DetectionSensitivity.Name
                                                    ? data[0] - 1
                                                    : data[0];

                    strMsg = strSettings + " 설정 가져오기 완료 : 0x" + rc.ToString("X4"); // 4자릿수 16진수로
                    mainWindow.LogIOStream(strMsg);
                }
                else // Getting NG
                {
                    strMsg = strSettings + " 설정 가져오기 중 오류 발생 : 0x" + rc.ToString("X4");

                    _ = MessageBox.Show(strMsg);
                    mainWindow.LogIOStream(strMsg);
                }
            }
        }

        /// <summary>
        /// 노광모드 변경에 따른 각 모드별 세부 설정 UI출력 설정(Visible)
        /// </summary>
        private void UIVisibilityChange()
        {
            switch (LSCBox_ExposureMode.SelectedIndex)
            {
                case 0: // 표준 선택 시
                    if (LSCBox_MultiCombine != null &&
                        LSCBox_MultiOptimize != null)
                    {
                        LSIndex_MultiCombine.Visibility = Visibility.Hidden;
                        LSCBox_MultiCombine.Visibility = Visibility.Hidden;
                        LSIndex_MultiOptimize.Visibility = Visibility.Hidden;
                        LSCBox_MultiOptimize.Visibility = Visibility.Hidden;
                    }
                    break;
                case 1: // 멀티 발광(합성) 선택 시
                    LSIndex_MultiCombine.Visibility = Visibility.Visible;
                    LSCBox_MultiCombine.Visibility = Visibility.Visible;
                    LSIndex_MultiOptimize.Visibility = Visibility.Hidden;
                    LSCBox_MultiOptimize.Visibility = Visibility.Hidden;
                    break;
                case 2: // 멀티 발광(광량최적화) 선택 시
                    LSIndex_MultiCombine.Visibility = Visibility.Hidden;
                    LSCBox_MultiCombine.Visibility = Visibility.Hidden;
                    LSIndex_MultiOptimize.Visibility = Visibility.Visible;
                    LSCBox_MultiOptimize.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void LecipeSettingsBtn_Reset_Click(object sender, RoutedEventArgs e) //레시피 설정 초기화 버튼
        {
            //레시피 스펙 설정
            Recipe_Name.Text = "";
            Recipe_Width.Text = "100";
            Recipe_Height.Text = "100";
            Recipe_Weight.Text = "10";
            Recipe_Range.Text = "0.5";
            //기준점 설정
            auto.IsChecked = true;     manu.IsChecked = false;
            Mark_Point_1_x.Text = "0";   Mark_Point_1_y.Text = "0";
            Mark_Point_2_x.Text = "0";   Mark_Point_2_y.Text = "95";
            Mark_Point_3_x.Text = "95";   Mark_Point_3_y.Text = "47";
            //측정점 설정
            auto2.IsChecked = true;    manu2.IsChecked = false;
            Point_1_x.Text = "";        Point_1_y.Text = "";
            Point_2_x.Text = "";        Point_2_y.Text = "";
            Point_3_x.Text = "";        Point_3_y.Text = "";
            Point_4_x.Text = "";        Point_4_y.Text = "";
            Point_5_x.Text = "";        Point_5_y.Text = "";
            Point_6_x.Text = "";        Point_6_y.Text = "";
            Point_7_x.Text = "";        Point_7_y.Text = "";
            Point_8_x.Text = "";        Point_8_y.Text = "";
        }

        private void LecipeSettingsBtn_Delete_Click(object sender, RoutedEventArgs e) //레시피 설정 삭제 버튼
        {
            try{
                if (File.Exists("Setting\\" + Recipe_Name.Text + ".csv")){ //파일 존재 확인
                    File.Delete("Setting\\" + Recipe_Name.Text + ".csv"); //파일 삭제
                    MessageBox.Show("파일이 삭제되었습니다.");
                }else{ 
                    MessageBox.Show("없는 설정파일 입니다."); 
                }
                ComboBox1.Items.Clear(); //레시피 콤보박스  전부 없애기
                DirectoryInfo di = new DirectoryInfo("Setting"); //세팅 폴더
                foreach (FileInfo item in di.GetFiles()) //세팅 폴더 안의 모든 파일 가져오기
                {
                    ComboBox1.Items.Add(item.Name.Substring(0, item.Name.Length - 4)); //레시피 콤보박스에 파일 이름들 추가

                }
            }
            catch (Exception ex){
                ComboBox1.Items.Clear(); //레시피 콤보박스  전부 없애기
                DirectoryInfo di = new DirectoryInfo("Setting"); //세팅 폴더
                foreach (FileInfo item in di.GetFiles()) //세팅 폴더 안의 모든 파일 가져오기
                {
                    ComboBox1.Items.Add(item.Name.Substring(0, item.Name.Length - 4)); //레시피 콤보박스에 파일 이름들 추가

                }
                Console.WriteLine("삭제버튼 에러 메시지 : " + ex.Message);
            }
        }

        private void LecipeSettingsBtn_Save_Click(object sender, RoutedEventArgs e) //레시피 설정 저장(등록) 버튼
        {
            try
            {
                StreamWriter file = new StreamWriter("Setting\\" + Recipe_Name.Text + ".csv", false, Encoding.UTF8); //csv 파일  생성,변경  설정 등록
                file.WriteLine("#name\tx1\ty1\tz1\tx2\ty2\tz2\tx3\ty3\tz3\tWidth\tHeight\tWeight\tRange"); //key(항목이름)부분 한줄 저장
                file.WriteLine(Recipe_Name.Text + "\t" + Mark_Point_1_x.Text + "\t" + Mark_Point_1_y.Text + "\t0\t" +
                    Mark_Point_2_x.Text + "\t" + Mark_Point_2_y.Text + "\t0\t" + Mark_Point_3_x.Text + "\t" + Mark_Point_3_y.Text +
                    "\t0\t" + Recipe_Width.Text + "\t" + Recipe_Width.Text + "\t" + Recipe_Weight.Text + "\t" + Recipe_Range.Text); //value(값)부분 한줄 저장
                file.Close(); //파일 닫기
                MessageBox.Show("설정이 저장되었습니다.");

                ComboBox1.Items.Clear(); //레시피 콤보박스  전부 없애기
                DirectoryInfo di = new DirectoryInfo("Setting"); //세팅 폴더
                foreach (FileInfo item in di.GetFiles()) //세팅 폴더 안의 모든 파일 가져오기
                {
                    ComboBox1.Items.Add(item.Name.Substring(0, item.Name.Length - 4)); //레시피 콤보박스에 파일 이름들 추가

                }
            }
            catch (Exception ex)
            {
                ComboBox1.Items.Clear(); //레시피 콤보박스  전부 없애기
                DirectoryInfo di = new DirectoryInfo("Setting"); //세팅 폴더
                foreach (FileInfo item in di.GetFiles()) //세팅 폴더 안의 모든 파일 가져오기
                {
                    ComboBox1.Items.Add(item.Name.Substring(0, item.Name.Length - 4)); //레시피 콤보박스에 파일 이름들 추가

                }
                Console.WriteLine("저장버튼 에러 메시지 : " + ex.Message);
                //MessageBox.Show("에러 메시지 : " + ex.Message);

            }


        }


        
        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e) //레시피 콤보박스 선택
        {
            //ComboBox currentComboBox = sender as ComboBox;
            //ComboBoxItem currentItem = ComboBox1.SelectedItem as ComboBoxItem;
            //csv 파일 이름 
            string csv_name = "";

            //if (currentItem != null) {
                //MessageBox.Show(currentItem.Content.ToString());
                csv_name = ComboBox1.SelectedItem.ToString();//csv파일 이름(기기명)
           // }

            var x = new Dictionary<String, string>();
            string[] keys = null;
            string[] values = null;
            try
            {
                Console.WriteLine("이름 : "+ csv_name);

                StreamReader file = new StreamReader("Setting\\" + csv_name + ".csv", Encoding.Default); //csv 파일 열기
                while (!file.EndOfStream) //없을때 까지 계속
                {
                    string line = file.ReadLine(); //한줄씩
                    if (line.Substring(0, 1).Equals("#")) {         // 줄의 첫번째가 #이라면
                        keys = line.Split('\t'); // ','로 나누어서 리스트에 담기
                        keys[0] = keys[0].Replace("#", ""); //"#"->""로 변환
                        continue;
                    }else{
                        values = line.Split('\t'); // ','로 나누어서 리스트에 담기
                    }
                }
                //string 자료형이 들어가는 리스트(csv파일 안에 정보)
                Dictionary<string, string> data;
                data = keys.Zip(values, (k, v) => new { k, v }).ToDictionary(a => a.k, a => a.v); //Dictionary에 키와 값 넣기
                //Dictionary 확인
                foreach (KeyValuePair<String, string> item in data)
                {
                    Console.WriteLine("[{0}:{1}]", item.Key, item.Value);
                }
                //화면에 보이게 하기
                Recipe_Name.Text = data["name"];
                Recipe_Width.Text = data["Width"];
                Recipe_Height.Text = data["Height"];
                Recipe_Weight.Text = data["Weight"];
                Recipe_Range.Text = data["Range"];
                Mark_Point_1_x.Text = data["x1"]; Mark_Point_1_y.Text = data["y1"];
                Mark_Point_2_x.Text = data["x2"]; Mark_Point_2_y.Text = data["y2"];
                Mark_Point_3_x.Text = data["x3"]; Mark_Point_3_y.Text = data["y3"];
                file.Close(); //파일 닫기

            }
            catch (Exception ex)
            {
                MessageBox.Show("에러 메시지 : "+ex.Message+"\n 없는 설정파일입니다. 다시 선택해주세요.");
            }
            
            
        }

        //기준점 갯수 확인
        int ell_num = 0;
        private void Point_Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) //그림 클릭시
        {
            if (manu.IsChecked == true){ //수동일때
                var position = e.GetPosition(Point_Canvas); //현제위치 가져오기

                if (ell_num < 3) { // 3개 이하이면
                    //Console.WriteLine("x:" + position.X + " y:" + position.Y); //원 좌표 확인
                    ell_num++; // 기준점 갯수 증가
                    if (ell_num == 1) {
                        Mark_Point_1_x.Text = Convert.ToInt32(position.Y / 2).ToString(); //x좌표
                        Mark_Point_1_y.Text = Convert.ToInt32(position.X / 2).ToString(); //y좌표
                    }else if (ell_num == 2) {
                        Mark_Point_2_x.Text = Convert.ToInt32(position.Y / 2).ToString();
                        Mark_Point_2_y.Text = Convert.ToInt32(position.X / 2).ToString();
                    }else if (ell_num == 3) {
                        Mark_Point_3_x.Text = Convert.ToInt32(position.Y / 2).ToString();
                        Mark_Point_3_y.Text = Convert.ToInt32(position.X / 2).ToString();
                    }
                }else {
                    ell_num = 1;
                    Mark_Point_1_x.Text = Convert.ToInt32(position.Y / 2).ToString(); //x좌표
                    Mark_Point_1_y.Text = Convert.ToInt32(position.X / 2).ToString(); //y좌표
                }

                Mark_Point_TextChanged();
            }

        }

        private void Recipe_Width_TextChanged(object sender, TextChangedEventArgs e) //가로길이 변경시
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray()) //텍스트박스 문자 1개씩 검사
            {
                //숫자검사 (숫자이거나 '.'이 없을때 '.'이 나오면)
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character; //result에 추가
                    if (character == '.')//'.'이면 
                    {
                        count += 1; //count증가
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            if (Recipe_Width.Text == ""){
                Recipe_Width.Text = "0";
            }else {
                Point_Canvas.Height = Convert.ToDouble(Recipe_Width.Text) * 2;
            }
            if (auto.IsChecked == true) { //자동선택되어 있을때
                auto_Checked(auto, null);
            }
        }

        private void Recipe_Height_TextChanged(object sender, TextChangedEventArgs e) //세로길이 변경시
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.')  
                    {
                        count += 1; 
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            if (Recipe_Height.Text == ""){
                Recipe_Height.Text = "0";
            }else {
                Point_Canvas.Width = Convert.ToDouble(Recipe_Height.Text) * 2;
            }
            if (auto.IsChecked == true) { //자동선택되어 있을때
                auto_Checked(auto, null);
            }
        }

        private void manu_Checked(object sender, RoutedEventArgs e)//기준점 수동 선택되었을때
        {
            Mark_Point_1_x.IsEnabled = true;
            Mark_Point_1_y.IsEnabled = true;
            Mark_Point_2_x.IsEnabled = true;
            Mark_Point_2_y.IsEnabled = true;
            Mark_Point_3_x.IsEnabled = true;
            Mark_Point_3_y.IsEnabled = true;
        }
        private void auto_Checked(object sender, RoutedEventArgs e) //기준점 자동 선택되었을때
        {
            try{
                if (Point_Canvas == null)
                {
                    return;
                }
                Point_Canvas.Children.Clear();// 기준점 초기화
                if (Recipe_Width.Text == "")  {   Recipe_Width.Text = "0";  }
                if (Recipe_Height.Text == "") {   Recipe_Height.Text = "0"; }
                double x = Convert.ToDouble(Recipe_Height.Text) * 2;
                double y = Convert.ToDouble(Recipe_Width.Text) * 2;
                Mark_Point_1_x.Text = "0";
                Mark_Point_1_y.Text = "0";
                Mark_Point_2_x.Text = (y / 2 - 3).ToString();
                Mark_Point_2_y.Text = "0";
                Mark_Point_3_x.Text = (y / 4 - 3).ToString();
                Mark_Point_3_y.Text = (x / 2 - 3).ToString();
                Mark_Point_TextChanged();
                ell_num = 3; //갯수
                Mark_Point_1_x.IsEnabled = false;
                Mark_Point_1_y.IsEnabled = false;
                Mark_Point_2_x.IsEnabled = false;
                Mark_Point_2_y.IsEnabled = false;
                Mark_Point_3_x.IsEnabled = false;
                Mark_Point_3_y.IsEnabled = false;
            }
            catch (Exception ex) {
                Console.WriteLine("에러 메시지 : " + ex.Message);
            }
        }

        public void Mark_Point_TextChanged()
        {
            try
            {
                if (Point_Canvas == null)
                {
                    return;
                }
                if (Mark_Point_1_x.Text == "") { Mark_Point_1_x.Text = "0"; }
                if (Mark_Point_1_y.Text == "") { Mark_Point_1_y.Text = "0"; }
                if (Mark_Point_2_x.Text == "") { Mark_Point_2_x.Text = "0"; }
                if (Mark_Point_2_y.Text == "") { Mark_Point_2_y.Text = "0"; }
                if (Mark_Point_3_x.Text == "") { Mark_Point_3_x.Text = "0"; }
                if (Mark_Point_3_y.Text == "") { Mark_Point_3_y.Text = "0"; }
                Point_Canvas.Children.Clear();// 기준점 초기화
                Ellipse ellipse = new Ellipse(); //원 생성 1
                ellipse.Width = 5;
                ellipse.Height = 5;
                ellipse.Fill = new SolidColorBrush(Color.FromRgb(100, 0, 0)); //빨간색
                Canvas.SetLeft(ellipse, Convert.ToDouble(Mark_Point_1_y.Text) * 2);
                Canvas.SetTop(ellipse, Convert.ToDouble(Mark_Point_1_x.Text) * 2);
                Point_Canvas.Children.Add(ellipse); //원 보이게 하기
                Ellipse ellipse2 = new Ellipse(); //원 생성 2
                ellipse2.Width = 5;
                ellipse2.Height = 5;
                ellipse2.Fill = new SolidColorBrush(Color.FromRgb(100, 0, 0)); 
                Canvas.SetLeft(ellipse2, Convert.ToDouble(Mark_Point_2_y.Text) * 2);
                Canvas.SetTop(ellipse2, Convert.ToDouble(Mark_Point_2_x.Text) * 2);
                Point_Canvas.Children.Add(ellipse2); //원 보이게 하기
                Ellipse ellipse3 = new Ellipse(); //원 생성 3
                ellipse3.Width = 5;
                ellipse3.Height = 5;
                ellipse3.Fill = new SolidColorBrush(Color.FromRgb(100, 0, 0)); 
                Canvas.SetLeft(ellipse3, Convert.ToDouble(Mark_Point_3_y.Text) * 2);
                Canvas.SetTop(ellipse3, Convert.ToDouble(Mark_Point_3_x.Text) * 2);
                Point_Canvas.Children.Add(ellipse3); //원 보이게 하기
            }catch (Exception ex){
                Console.WriteLine("에러 메시지 : " + ex.Message);
            }
        }

        private void Mark_Point_1_x_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.')
                    {
                        count += 1;
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            Mark_Point_TextChanged();
        }

        private void Mark_Point_1_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.')
                    {
                        count += 1;
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            Mark_Point_TextChanged();
        }
        private void Mark_Point_2_x_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.')
                    {
                        count += 1;
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            Mark_Point_TextChanged();
        }

        private void Mark_Point_2_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.')
                    {
                        count += 1;
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            Mark_Point_TextChanged();
        }
        private void Mark_Point_3_x_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.')
                    {
                        count += 1;
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            Mark_Point_TextChanged();
        }

        private void Mark_Point_3_y_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            int selectionStart = textBox.SelectionStart;
            string result = string.Empty;
            int count = 0;
            foreach (char character in textBox.Text.ToCharArray())
            {
                if (char.IsDigit(character) || (character == '.' && count == 0)) {
                    result += character;
                    if (character == '.') 
                    {
                        count += 1;
                    }
                }
            }
            textBox.Text = result;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
            Mark_Point_TextChanged();
        }

        #endregion 
    }
}
