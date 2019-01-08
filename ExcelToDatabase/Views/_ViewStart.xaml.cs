using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;
using Ay.Framework.WPF.Controls;
using Ay.MvcFramework;
using ExcelToDatabase.Controllers;


namespace ExcelToDatabase.Views
{
    /// <summary>
    /// _ViewStartView.xaml 
    /// 创建时间：2017/9/5 11:57:44
    /// </summary>
    public partial class _ViewStart : AyWindow
    {
        public _ViewStart()
        {
            InitializeComponent();
        }


    }


























    public partial class _ViewStart : AyWindow
    {
        #region 控制器
        private Actions<ViewStartController> _mvc;
        public Actions<ViewStartController> Mvc
        {
            get
            {
                if (_mvc == null)
                {
                    _mvc = new Actions<ViewStartController>(DataContext as ViewStartController);
                }
                return _mvc;
            }
        }
        #endregion
    }
}
