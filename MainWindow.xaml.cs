using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

public enum CONTROLLER_STATE { OFF, STOP, RUN, ERR }

namespace LaserBendingMeasurementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Field
        public string appExePath; // 실행 파일 경로
        // 컨트롤러 상태 관련
        private string laserIP;
        private string motionIP;
        private CONTROLLER_STATE laserState;
        private CONTROLLER_STATE motionState;
        private BitmapImage bi;             // state 비트맵 이미지
        private ImageBrush ib;       // state 이미지 브러시(Image.Background 동적할당에 사용)

        // 자식 페이지 관련
        private Page.MenuHome m_MenuHome;
        private Page.MenuSettings m_MenuSettings;
        private Page.MenuHistoryInfo m_MenuHistoryInfo;
        private Page.MenuManual m_MenuManual;

        // Set Get
        public string LaserIP { get => laserIP; set => laserIP = value; }
        public string MotionIP { get => motionIP; set => motionIP = value; }
        public CONTROLLER_STATE LaserState { get => laserState; set => laserState = value; }
        public CONTROLLER_STATE MotionState { get => motionState; set => motionState = value; }
        public BitmapImage Bi { get => bi; set => bi = value; }
        public ImageBrush Ib { get => ib; set => ib = value; }
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            appExePath = System.IO.Directory.GetCurrentDirectory();
            LogIOStream("프로그램 실행");
            // 멤버변수 초기화
            laserIP = ConstZip.CONTROLLER_DISCONNECTED;
            motionIP = ConstZip.CONTROLLER_DISCONNECTED;
            laserState = CONTROLLER_STATE.OFF;
            motionState = CONTROLLER_STATE.OFF;
            bi = null;
            ib = null;

            // 각 페이지 생성
            m_MenuHome = new Page.MenuHome();
            m_MenuSettings = new Page.MenuSettings(this);
            m_MenuHistoryInfo = new Page.MenuHistoryInfo();
            m_MenuManual = new Page.MenuManual();

            RefreshIPState();
            Btn_menuHome_Click(null, null);
        }
        #endregion

        #region Event
        private void Btn_menuHome_Click(object sender, RoutedEventArgs e)
        {
            grdMainView.Children.Clear();
            grdMainView.Children.Add(m_MenuHome);
        }

        private void Btn_menuSettings_Click(object sender, RoutedEventArgs e)
        {
            grdMainView.Children.Clear();
            grdMainView.Children.Add(m_MenuSettings);
        }

        private void Btn_menuHistoryInfo_Click(object sender, RoutedEventArgs e)
        {
            grdMainView.Children.Clear();
            grdMainView.Children.Add(m_MenuHistoryInfo);
        }

        private void Btn_menuManual_Click(object sender, RoutedEventArgs e)
        {
            grdMainView.Children.Clear();
            grdMainView.Children.Add(m_MenuManual);
        }

        private void Btn_menuMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Btn_menuQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // 메뉴 드래그&드롭 창 위치 이동
        private void MenuMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton != System.Windows.Input.MouseButtonState.Pressed) DragMove();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_MenuSettings.LaserFinalize();
            LogIOStream("프로그램 종료\r\n");
        }
        #endregion

        #region Method
        /// <summary>
        /// 상태표시줄에 레이저, 모션 컨트롤러의 IP 및 상태(OFF, STOP, RUN, ERR)를 업데이트 
        /// </summary>
        public void RefreshIPState()
        {
            TxtBlock_lcip.Text = laserIP;
            TxtBlock_mcip.Text = motionIP;
            TxtBlock_lcimg.Background = MakeResImageBrush(laserState);
            TxtBlock_mcimg.Background = MakeResImageBrush(motionState);
        }

        /// <summary>
        /// MainWindow 상태표시줄 이미지를 컨트롤러 상태에 맞춰 동적할당
        /// </summary>
        /// <param name="state">OFF, STOP, RUN, ERR</param>
        /// <returns>전구모양 이미지(OFF = 무색, STOP = 노랑, RUN = 파랑, ERR = 빨강)</returns>
        private ImageBrush MakeResImageBrush(CONTROLLER_STATE state)
        {
            try
            {
                string fileName = null;

                switch (state)
                {
                    case CONTROLLER_STATE.OFF:
                        fileName = ConstZip.CONTROLLER_STATE_OFF;
                        break;
                    case CONTROLLER_STATE.STOP:
                        fileName = ConstZip.CONTROLLER_STATE_STOP;
                        break;
                    case CONTROLLER_STATE.RUN:
                        fileName = ConstZip.CONTROLLER_STATE_RUN;
                        break;
                    case CONTROLLER_STATE.ERR:
                        fileName = ConstZip.CONTROLLER_STATE_ERROR;
                        break;
                    default:
                        throw new Exception("");
                }
                bi = new BitmapImage(new Uri(ConstZip.URI_RES_PATH + fileName));
                ib = new ImageBrush(bi);
                return ib;
            }
            catch (Exception e)
            {
                LogIOStream(e.Message);
                MessageBox.Show(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 로그 파일 입출력
        /// </summary>
        /// <param name="logLine">로그 내용</param>
        public void LogIOStream(string logLine)
        {
            try
            {
                string pathName = appExePath + "\\Log";
                string fileName = pathName + "\\" +
                    DateTime.Now.ToString("yyMMdd") + ".txt";

                if (!System.IO.Directory.Exists(pathName))
                {
                    System.IO.Directory.CreateDirectory(pathName);
                }

                System.IO.StreamWriter fileWriter =
                    new System.IO.StreamWriter(fileName, true);

                fileWriter.Write(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] : ") 
                    + logLine + "\r\n");
                fileWriter.Flush();
                fileWriter.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #endregion

    }
}
