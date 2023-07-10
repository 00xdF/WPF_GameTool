using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPF_GameTool.Tool
{  
    /// <summary>
    /// ADB控制类 用于执行adb代码
    /// </summary>
    internal class ADB
    {
        private TextBox tb;
        public ADB(TextBox tb)
        {
            this.tb = tb;
        }

        /// <summary>
        /// 获取已连接电脑的设备
        /// </summary>
        /// <returns></returns>
        public async Task<ObservableCollection<string>> GetConnectedPhone() {
            var devs = new ObservableCollection<string>();
            var res = await ExecuteADBCommand("/c adb devices");
            debug(res);
            foreach (var dev in res.Split("\n"))
            {
                devs.Add(dev);
            }
            devs.RemoveAt(0);
            return devs;
        }

        /// <summary>
        /// 获取手机屏幕截图到指定路径
        /// </summary>
        /// <param name="path"></param>
        public async Task GetPhoneScreen(string path)
        {
            var command = "/c adb shell rm /sdcard/screenshot.png ";
            var command1 = "/c adb shell screencap -p /sdcard/screenshot.png ";
            var command2 =  $"/c adb pull /sdcard/screenshot.png {path} ";
            await ExecuteADBCommand(command);  
            await ExecuteADBCommand(command1);  
            await ExecuteADBCommand(command2);  
        }


        /// <summary>
        /// 执行adb命令
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        private async Task<string> ExecuteADBCommand(string Command)
        {
            string output = "";
            await Task.Run(() =>
            {
                //debug("execute=>"+Command);
                Process process = new Process();
                // 设置要执行的命令行程序和参数
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = Command; // 替换为你想执行的命令
                                                       // 指定进程窗口不显示
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                // 重定向标准输出，以便从进程中获取输出结果
                process.StartInfo.RedirectStandardOutput = true;
                // 启动进程
                process.Start();
                // 读取输出结果
                output = process.StandardOutput.ReadToEnd();
                // 等待进程完成
                process.WaitForExit();
            });
            return output;
        }

        /// <summary>
        /// 模拟手机点击效果
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public async Task ClickScreen(double x,double y)
        {
            //debug($"{x},{y}");
            await ExecuteADBCommand($"/c adb shell input tap {x} {y}");
          
        }


        public void debug(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                tb.Text = tb.Text + $"{DateTime.Now.ToString() + " : " + message}\n";
                tb.ScrollToEnd();
            }));
            Debug.WriteLine($"{DateTime.Now.ToString()+" : "+ message}");
        }


        public string createRandomString(int len)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();

            char[] result = new char[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }
    }
}
