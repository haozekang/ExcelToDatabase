using Ay.Framework.WPF.Controls;
using Kang.Util;
using System;
using System.Collections.Generic;
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

namespace ExcelToDatabase.Views.Config
{
    /// <summary>
    /// ConfigPage.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigPage : UserControl
    {
        public String db_host = "", db_port = "", db_name = "", db_username = "", db_password = "";
        public String table_name = "";
        public ConfigPage()
        {
            InitializeComponent();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (StringUtil.isBlank(txt_db_host.Text))
            {
                AyMessageBox.ShowError("数据库地址不能为空！");
                return;
            }
            else if (StringUtil.isBlank(txt_db_port.Text))
            {
                AyMessageBox.ShowError("数据库端口不能为空！");
                return;
            }
            else if (StringUtil.isBlank(txt_db_name.Text))
            {
                AyMessageBox.ShowError("数据库名称不能为空！");
                return;
            }
            else if (StringUtil.isBlank(txt_db_username.Text))
            {
                AyMessageBox.ShowError("数据库账号不能为空！");
                return;
            }
            else if (StringUtil.isBlank(txt_db_password.Text))
            {
                AyMessageBox.ShowError("数据库密码不能为空！");
                return;
            }

            Global.cu.IniWriteValue("DBConfig", "db_host", txt_db_host.Text);
            Global.cu.IniWriteValue("DBConfig", "db_port", txt_db_port.Text);
            Global.cu.IniWriteValue("DBConfig", "db_name", txt_db_name.Text);
            Global.cu.IniWriteValue("DBConfig", "db_username", txt_db_username.Text);
            Global.cu.IniWriteValue("DBConfig", "db_password", txt_db_password.Text);

            Global.RefreshConfigDataToGloBal();
            Global.HomeWindow.RefreshTablesNameComBoBox();
            Global.configPage = null;
            AyLayer.Close("ConfigPage".ToLower());
        }

        private void btn_exit_Click(object sender, RoutedEventArgs e)
        {
            AyLayer.Close("ConfigPage".ToLower());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txt_db_host.Text = db_host = Global.cu.IniReadValue("DBConfig", "db_host");
            txt_db_port.Text = db_port = Global.cu.IniReadValue("DBConfig", "db_port");
            txt_db_name.Text = db_name = Global.cu.IniReadValue("DBConfig", "db_name");
            txt_db_username.Text = db_username = Global.cu.IniReadValue("DBConfig", "db_username");
            txt_db_password.Text = db_password = Global.cu.IniReadValue("DBConfig", "db_password");
            if (StringUtil.isBlank(db_host))
            {
                AyMessageBox.ShowError("数据库地址不能为空或配置文件数据异常！");
                return;
            }
            else if (StringUtil.isBlank(db_port))
            {
                AyMessageBox.ShowError("数据库端口不能为空或配置文件数据异常！");
                return;
            }
            else if (StringUtil.isBlank(db_name))
            {
                AyMessageBox.ShowError("数据库名称不能为空或配置文件数据异常！");
                return;
            }
            else if (StringUtil.isBlank(db_username))
            {
                AyMessageBox.ShowError("数据库账号不能为空或配置文件数据异常！");
                return;
            }
            else if (StringUtil.isBlank(db_password))
            {
                AyMessageBox.ShowError("数据库密码不能为空或配置文件数据异常！");
                return;
            }
        }
    }
}
