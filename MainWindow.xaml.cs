using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPF_GameTool.Tool;
using VeryHotKeys;
using VeryHotKeys.Wpf;
using System.Security.Policy;
using System.Windows.Controls.Primitives;

namespace WPF_GameTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,INotifyPropertyChanged
    {
        //提高性能，逻辑树查找和FindName方法是循环递归实现的、性能很拉
        private Dictionary<char, List<Button>> btnDic = new Dictionary<char, List<Button>>();
        private double ratio = 0.0;
        private DispatcherTimer timer;
        private ADB adb ;
        public ObservableCollection<String> devices;
        public event PropertyChangedEventHandler? PropertyChanged;
        

        public MainWindow()
        {
            this.ResizeMode = ResizeMode.NoResize;
            this.DataContext = this;
            InitializeComponent();
            init();
             
        }

        /// <summary>
        /// 界面初始化
        /// </summary>
        private async void init()
        {
            adb = new ADB(text_log);
            devices = await adb.GetConnectedPhone();
            devices_list.ItemsSource = devices;
            //自动刷新
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // 设置定时间隔为1秒
            timer.Tick += Auto_Refresh; // 绑定定时事件处理程序
            initBtnDic();
        }

        private void initBtnDic()
        {
           for (char i = '0'; i <= 'Z'; i++)
            {
                btnDic.Add(i, new List<Button>());
            }
        }

        /// <summary>
        /// 自动刷新事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Auto_Refresh(object? sender, EventArgs e)
        {
            if (radio_refresh.IsChecked == true)
            {
                GetScreen_Click(sender,null);
            }
        }

        private  void GetScreen_Click(object sender, RoutedEventArgs e)
        {
            GetScreen.IsEnabled = false;
            Task.Run(async () =>
            {
                string imagePath = Directory.GetCurrentDirectory() + $"/{adb.createRandomString(8)}.png";
                await adb.GetPhoneScreen(imagePath);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                        screen_img.Source = bitmap;
                        GetScreen.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        adb.debug(ex.Message);
                    }
                   
                }));
            });    
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private Button CreateButton(string c,double x,double y)
        {
            c = c.ToUpper();
            Button btn = new Button();
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.VerticalAlignment = VerticalAlignment.Center;
            btn.HorizontalContentAlignment = HorizontalAlignment;
            btn.Width = 20;
            btn.Height = 20;
            btn.FontSize = 15;
            btn.Foreground = Brushes.White;
            btn.Content = c[0] + "";
            btn.Background = Brushes.Red;


            //绑定热键事件
            var res = new HotKeyRegisterer(this, () => KeyIsClickByKeyBoard(c[0]), HotKeyMods.None, c[0]);

            btn.Click += (s,e)=> {
                var btn1 = s as Button;
                btnDic[c[0]].Remove(btn1); //从字典移出本btn
                //从界面移出btn
                if (btn1.Parent is Panel panel)
                {
                    panel.Children.Remove(btn1);
                }
                //关闭热键映射
                res.Dispose();
                
            } ;
            //添加btn到dic
            btnDic[c[0]].Add(btn);
            // 设置在 Canvas 上的位置
            Canvas.SetLeft(btn, x-10); // 设置圆圈的左侧位置
            Canvas.SetTop(btn, y+10); // 设置圆圈的顶部位置

            // 将圆圈添加到 Canvas
            canvas.Children.Add(btn);
            return btn;
        }

        private void KeyIsClickByMouse(char c)
        {

        }

        private void KeyIsClickByKeyBoard(char c)
        {
           foreach(var btn in btnDic[c])
            {
                Point buttonPosition = btn.TranslatePoint(new Point(0, 0), this);
                Task.Run(async () =>
                {
                   await adb.ClickScreen(buttonPosition.X * ratio, buttonPosition.Y * ratio);
                });
            }
        }

        /// <summary>
        /// 左键模拟点击屏幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void screen_img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 获取点击坐标 
            Point clickPoint = e.GetPosition(screen_img);
            // 获取手机上的坐标
            //adb.debug($" Image Panel Size : {image?.ActualHeight}*{image?.ActualWidth};Image Size:{image?.Source.Height}*{image?.Source.Width}");
            ratio = (screen_img.Source.Height / screen_img.ActualHeight);
            double x = (double)clickPoint.X;
            double y = (double)clickPoint.Y;
            await adb.ClickScreen(x * ratio, y * ratio);
        }

     
        /// <summary>
        /// 实时刷新屏幕显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radio_refresh_Checked(object sender, RoutedEventArgs e)
        {
           if(radio_refresh.IsChecked == true)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void checkbox_design_Checked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 右键添加映射按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void screen_img_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(screen_img);
            // 获取手机上的坐标
            //adb.debug($" Image Panel Size : {image?.ActualHeight}*{image?.ActualWidth};Image Size:{image?.Source.Height}*{image?.Source.Width}");
            double ratio = (screen_img.Source.Height / screen_img.ActualHeight);
            double x = (double)clickPoint.X;
            double y = (double)clickPoint.Y;
            if (checkbox_design.IsChecked == true)
            {
                var btn = CreateButton("f", x, y);
                adb.debug(btn.Name);
                TraverseButtonNames(this);
            }
            else
            {
               
            }
        }

        private void TraverseButtonNames(DependencyObject parent)
        {
            if (parent == null)
                return;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is Button button)
                {
                    string buttonName = button.Name;
                    // 处理按钮名称
                   adb.debug(buttonName);
                }
                else
                {
                    TraverseButtonNames(child); // 递归遍历子元素
                }
            }
        }



    }
}
