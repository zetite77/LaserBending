using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaserBendingMeasurementSystem.Page
{
    /// <summary>
    /// MenuHome_PageMain.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 


    public partial class MenuHome : UserControl
    {
        //평면상의 세개의 점
        float x1 = 1.1f, y1 = 2.0f, z1 = 7.0f;  //첫번째 점(x1,y1,z1)
        float x2 = 1.0f, y2 = 5.0f, z2 = 16.0f; //두번째 점(x2,y2,z2)
        float x3 = 3.0f, y3 = 5.0f, z3 = 0f;  //세번째 점(x3,y3,z3)

        //세점을 가지고 평면 방정식(Ax+By+Cz+D=0)의 A,B,C,D를 구하는 식
        float A = 0, B = 0, C = 0, D = 0;

        double h = 0; //거리
        float x0, y0, z0; //한 점(x0,y0,z0)

        public MenuHome()
        {
            InitializeComponent();
        }

        //세점을 가지고 평면 방정식(Ax+By+Cz+D=0)의 A,B,C,D를 구하는 식
        public void Test(float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3)
        {
            A = y1 * (z2 - z3) + y2 * (z3 - z1) + y3 * (z1 - z2); //A 구하는 식
            B = z1 * (x2 - x3) + z2 * (x3 - x1) + z3 * (x1 - x2); //B 구하는 식
            C = x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2); //C 구하는 식
            D = -(x1 * (y2 * z3 - y3 * z2) + x2 * (y3 * z1 - y1 * z3) + x3 * (y1 * z2 - y2 * z1)); //D 구하는 식
        }

        //점과 평면 사이 거리 구하는 식
        public void Test2(float x0, float y0, float z0)
        {
            //Math.Abs() 절댓값 , Math.Sqrt()는 루트
            h = Math.Abs((A * x0) + (B * y0) + (C * z0) + D) / Math.Sqrt((A * A) + (B * B) + (C * C));
        }
    }
}
