using System.Windows;
using System;
using Ay.MvcFramework;
using Ay.MvcFramework.Wpf;
using Kang.SQLManager;
using Kang.Util;
using ExcelToDatabase.Views.Home;
using ExcelToDatabase.Views.Config;

namespace ExcelToDatabase
{
    public class Global : AYUIGlobal
    {
        public static MySQLManager my = null;
        public static ConfigUtil cu = new ConfigUtil(System.AppDomain.CurrentDomain.BaseDirectory + @"config.ini");

        public static String db_host = "", db_port = "", db_name = "", db_username = "", db_password = "";
        public static String table_name = "";

        public override void Application_Start(StartupEventArgs e, Application appliation)
        {
            LanuageManager.DisabledLanuage = true;

        }
        //此方法重写，为了导入AYUI
        public override void Application_Run(Application appliation)
        {
            appliation.AYUI();

            db_host = cu.IniReadValue("DBConfig", "db_host");
            db_port = cu.IniReadValue("DBConfig", "db_port");
            db_name = cu.IniReadValue("DBConfig", "db_name");
            db_username = cu.IniReadValue("DBConfig", "db_username");
            db_password = cu.IniReadValue("DBConfig", "db_password");
            table_name = cu.IniReadValue("TableConfig", "table_name");
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
            }else if (StringUtil.isBlank(table_name))
            {
                AyMessageBox.ShowError("数据表名称异常！");
                return;
            }
            my = new MySQLManager(db_host, db_port, db_name, db_username, db_password);
            my.showStr = false;
        }
        public override void RegisterResourceDictionary(ClientResourceDictionaryCollection resources)
        {
            resources.Add("Contents/Styles/AYUIProjectDictionary.xaml".ToApplicationCurrentResourceDictionary());
        }
        public override void RegisterLanuages(ClientLanuagesCollection languages)
        {
            languages.Add(new LanguageSelectModel { LanuageName = "简体中文", ResourceName = "zh-cn" });
            languages.Add(new LanguageSelectModel { LanuageName = "English", ResourceName = "en-us" });
        }

        public static void RefreshConfigDataToGloBal()
        {
            db_host = cu.IniReadValue("DBConfig", "db_host");
            db_port = cu.IniReadValue("DBConfig", "db_port");
            db_name = cu.IniReadValue("DBConfig", "db_name");
            db_username = cu.IniReadValue("DBConfig", "db_username");
            db_password = cu.IniReadValue("DBConfig", "db_password");
            my = new MySQLManager(db_host, db_port, db_name, db_username, db_password);
        }

        public static HomeView HomeWindow { get; set; }

        public static ConfigPage configPage { get; set; }
    }
}
