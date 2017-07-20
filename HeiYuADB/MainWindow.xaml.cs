using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = System.Windows.Application;

namespace HeiYuADB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> Commands;
        public MainWindow()
        {
            InitializeComponent();
            txtAdbDir.Text = AppDomain.CurrentDomain.BaseDirectory + @"platform-tools";
            Commands = XMLConfigurer.Instance.GetCommands();
            Commands.Insert(0, "");
            cmbCommands.ItemsSource = Commands;
            if(Commands.Count() > 1)
            {
                cmbCommands.SelectedIndex = 1;
            }
        }

        private void btnSelDir_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(txtAdbDir.Text))
            {
                m_Dialog.SelectedPath = txtAdbDir.Text;
            }
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.txtAdbDir.Text = m_Dir;
        }

        private bool CheckAdbValid()
        {
            if (string.IsNullOrEmpty(txtAdbDir.Text))
            {
                System.Windows.MessageBox.Show("请设置adb.exe所在目录", "platform-tools");
                return false;
            }
            return true;
        }

        private void btnTestDeviceClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckAdbValid()) return;
                this.Cursor = System.Windows.Input.Cursors.Wait;
                DisableButtons();
                string cmdLog = InvokeCmd("adb devices", txtAdbDir.Text);
                string otuCmdRet = "List of devices attached";
                cmdLog = cmdLog.Substring(cmdLog.IndexOf(otuCmdRet));
                cmdLog = cmdLog.Replace(otuCmdRet, "当前连接的的设备：");
                txtOutput.AppendText("执行adb devices命令\r\n" + cmdLog);
                txtOutput.ScrollToEnd();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "UnKnown Error！");
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                EnableButtons();
            }
        }

        private string InvokeCmd(string cmd, string adbFolder)
        {
            string enterFolderCmd = "cd " + adbFolder;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
            p.StartInfo.CreateNoWindow = true; //不显示程序窗口
            p.Start(); //启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(enterFolderCmd + "&" + cmd + "&exit"); //+ "&exit"

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            string outputLog;
            outputLog = p.StandardOutput.ReadToEnd();

            p.WaitForExit(); //等待程序执行完退出进程
            p.Close();

            return outputLog;
        }

        private void btnStartHY_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckAdbValid()) return;
                string command = txtInputCommand.Text.Trim();
                if (string.IsNullOrEmpty(command))
                {
                    System.Windows.MessageBox.Show("请选择或输入命令！");
                    return;
                }
                this.Cursor = System.Windows.Input.Cursors.Wait;
                DisableButtons();
                string cmd = " adb " + command;
                txtOutput.AppendText("执行命令:\r\n" + cmd + "\r\n");
                txtOutput.ScrollToEnd();
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, arg) =>
                {
                    string[] args = arg.Argument as string[];
                    string cmdLog = InvokeCmd(args[0], args[1]);
                    cmdLog = cmdLog.Substring(cmdLog.IndexOf("&exit") + 7);//+7 refers to the "\r\n"
                    arg.Result = cmdLog;
                };
                worker.RunWorkerCompleted += (o, args) =>
                {
                    string cmdLog = (string)args.Result;
                    if (string.IsNullOrEmpty(cmdLog))
                    {
                        string message;
                        if (args.Error != null)
                        {
                            message = args.Error.Message;
                        }
                        else
                        {
                            message = "请检查您的手机是否已成功连接并开启usb调试模式";
                        }
                        System.Windows.MessageBox.Show(message, "启动失败！");
                        txtOutput.AppendText("\r\n");
                    }
                    else
                    {
                        txtOutput.AppendText(cmdLog);
                        System.Windows.MessageBox.Show("启动成功，请查看手机上的黑域软件！", "Success！");
                    }
                    txtOutput.ScrollToEnd();
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                    EnableButtons();
                };
                worker.RunWorkerAsync(new string[]{cmd,txtAdbDir.Text});
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "启动失败！");
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                EnableButtons();
            }
        }

        private void DisableButtons()
        {
            btnSelDir.IsEnabled = false;
            btnTestDevice.IsEnabled = false;
            btnStartHY.IsEnabled = false;
            btnExit.IsEnabled = false;
            btnAddCommand.IsEnabled = false;
        }

        private void EnableButtons()
        {
            btnSelDir.IsEnabled = true;
            btnTestDevice.IsEnabled = true;
            btnStartHY.IsEnabled = true;
            btnExit.IsEnabled = true;
            btnAddCommand.IsEnabled = true;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void cmbCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string command = cmbCommands.SelectedValue as string;
            txtInputCommand.Text = command;
        }

        private void btnSaveCommand_Click(object sender, RoutedEventArgs e)
        {
            string command = txtInputCommand.Text.Trim();
            if(string.IsNullOrEmpty(command))
            {
                System.Windows.MessageBox.Show("请输入命令！");
                return;
            }
            if (Commands.Contains(command)) return;
            Commands.Add(command);
            cmbCommands.Items.Refresh();
            cmbCommands.SelectedIndex = Commands.Count() - 1;
            XMLConfigurer.Instance.SaveCommands(Commands);
        }
    }
}
