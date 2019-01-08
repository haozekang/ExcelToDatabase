using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Ay.MvcFramework;
using Ay.MvcFramework.AyMarkupExtension;
using ExcelToDatabase.Controllers;
using System.Windows.Controls.Primitives;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.SS.UserModel;
using Kang.Util;
using Kang.ExtendMethod;
using NPOI.XSSF.UserModel;
using ExcelToDatabase.Models;
using Ay.Framework.WPF.Controls;
using System.Windows.Markup;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Threading;
using ExcelToDatabase.Contents;
using System.Threading;
using System.Data;
using ExcelToDatabase.Views.Config;
using ExcelToDatabase.Models.Domain;

namespace ExcelToDatabase.Views.Home
{
    /// <summary>
    /// HomeView.xaml 的交互逻辑
    /// </summary>
    public partial class HomeView : AyPage
    {
        IWorkbook workbook = null;
        ISheet sheet = null;
        IRow titleRow = null;

        DataGridTemplateColumn myself_col = null;
        DataGridTemplateColumn index_col = null;
        DataGridTemplateColumn input_state_col = null;

        String filePath = null;
        FileStream fileStream = null;

        Int32 rowCount = 0;
        Int32 colCount = 0;

        Boolean emptyFlag = false;
        Boolean reFlag = false;

        Boolean readTableNames = false;

        String[] fieldArray = null;
        String[] headerArray = null;

        public ObservableCollection<RowViewItem> list = new ObservableCollection<RowViewItem>();

        BackgroundWorker bgWorker = new BackgroundWorker();

        public HomeView()
        {
            InitializeComponent();
            //bgWorker.DoWork += InputDateToDatabase;

            myself_col = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(table_datagrid.Columns[1] as DataGridTemplateColumn);
            index_col = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(table_datagrid.Columns[0] as DataGridTemplateColumn);
            input_state_col = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(table_datagrid.Columns[2] as DataGridTemplateColumn);
            table_datagrid.Columns.RemoveAt(1);
            list.Clear();
            table_datagrid.ItemsSource = list;

            //cmb_fielditem.IsEnabled = false;

            //DataGridTemplateColumn col = new DataGridTemplateColumn();
            //col.Header = "";
            //col.Width = 0;
            //Setter setter = new Setter(DataGridColumnHeader.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            //Style style = new Style();
            //style.Setters.Add(setter);
            //col.HeaderStyle = style;
            //table_datagrid.Columns.Insert(0,col);

            //Sheet数量
            //workbook.NumberOfSheets;
        }

        private void AyPage_Loaded(object sender, RoutedEventArgs e)
        {
            Global.HomeWindow = this;
            //HostVisual hostVisual = new HostVisual();

            //UIElement content = new VisualHost(hostVisual);
            //this.Content = content;

            //Thread thread = new Thread(new ThreadStart(() =>
            //{
            //    VisualTarget visualTarget = new VisualTarget(hostVisual);
            //    var control = new AyPage();
            //    control.Arrange(new Rect(new Point(), content.RenderSize));
            //    visualTarget.RootVisual = control;
            //    System.Windows.Threading.Dispatcher.Run();
            //}));

            //thread.SetApartmentState(ApartmentState.STA);
            //thread.IsBackground = true;
            //thread.Start();
        }

        private async void ReadExcelData(String @path)
        {
            if (StringUtil.isBlank(@path))
            {
                AyMessageBox.ShowError("文件路径不正确！");
                btn_readFileData.IsEnabled = true;
                btn_selectFile.IsEnabled = true;
                return;
            }
            try
            {
                fileStream = new FileStream(@path, FileMode.Open, FileAccess.Read);
            }
            catch(Exception ex)
            {
                AyMessageBox.ShowError(ex.ToString());
                btn_readFileData.IsEnabled = true;
                btn_selectFile.IsEnabled = true;
                return;
            }
            if (fileStream.CanRead)
            {
                String houzhui = fileStream.Name.Substring(fileStream.Name.LastIndexOf(".") + 1, fileStream.Name.Length - fileStream.Name.LastIndexOf(".") - 1);
                if ("xls".Equals(houzhui))
                {
                    workbook = new HSSFWorkbook(fileStream);
                }else if ("xlsx".Equals(houzhui))
                {
                    workbook = new XSSFWorkbook(fileStream);
                }
            }
            else
            {
                AyMessageBox.ShowError("文件读取失败！");
                workbook = null;
                return;
            }
            sheet = workbook.GetSheetAt(0);
            if (sheet == null)
            {
                return;
            }

            table_datagrid.Columns.Clear();

            DataGridTemplateColumn index_col_val = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(index_col);
            DataGridTemplateColumn input_state_col_val = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(input_state_col);
            index_col_val.DisplayIndex = 0;
            table_datagrid.Columns.Add(index_col_val);
            //获取总行数
            rowCount = sheet.PhysicalNumberOfRows;
            int i, j;
            if (rowCount > 0)
            {
                //根据第一行，获取总列数
                titleRow = sheet.GetRow(0);
                colCount = titleRow.PhysicalNumberOfCells;

                //初始化字段名称和表格标题名称的数组大小
                fieldArray = new String[colCount];
                headerArray = new String[colCount];

                for (i = 0; i < colCount; i++)
                {
                    String titleString = titleRow.GetCell(i).StringCellValue;
                    if (StringUtil.isNotBlank(titleString))
                    {
                        String[] titleStringSplitArr = titleString.Replace(" ", "").Split(new Char[] { '(', ')', ',', ':','[',']', '（', '）', '，', '：','【','】' }, StringSplitOptions.RemoveEmptyEntries);

                        if (titleStringSplitArr != null)
                        {
                            if (titleStringSplitArr.Count() == 3)
                            {
                                headerArray[i] = titleStringSplitArr[1];
                                fieldArray[i] = titleStringSplitArr[2];
                                if ("*".Equals(titleStringSplitArr[0]))
                                {
                                    AddColumn(headerArray[i], "*", i + 1);
                                }else
                                {
                                    AddColumn(headerArray[i], titleStringSplitArr[0], i + 1);
                                }
                            }
                            else if (titleStringSplitArr.Count() == 2)
                            {
                                headerArray[i] = titleStringSplitArr[0];
                                fieldArray[i] = titleStringSplitArr[1];
                                AddColumn(headerArray[i],"200", i + 1);
                            }
                            else if (titleStringSplitArr.Count() == 1)
                            {
                                fieldArray[i] = titleStringSplitArr[0];
                                AddColumn(fieldArray[i], "200", i + 1);
                            }
                            else
                            {
                                AyMessageBox.ShowError("标题和字段信息初始化错误！");
                            }
                        }
                    }
                }
            }
            //填充数据
            AyTime.setTimeout(1,()=> {
                try
                {
                    btn_inputDataToDatabase.IsEnabled = false;
                    cb_empty.IsEnabled = false;
                    cb_re.IsEnabled = false;
                    cmb_fielditem.IsEnabled = false;
                    cmb_fielditem.SelectedIndex = -1;
                    cmb_fielditem.Text = "";
                    cmb_fielditem.Items.Clear();
                    list.Clear();
                    for (i = 1; i < rowCount; i++)
                    {
                        RowViewItem rowItem = new RowViewItem();
                        rowItem._number = i.ToString();
                        IRow rowData = sheet.GetRow(i);
                        for (j = 0; j < colCount; j++)
                        {
                            String value = rowData.GetCell(j).ToString();
                            SetValue(ref rowItem, j + 1, value);
                        }
                        rowItem._state = "尚未导入";
                        list.Add(rowItem);
                    }
                }
                catch (Exception ex)
                {
                    btn_inputDataToDatabase.IsEnabled = false;
                    cb_empty.IsEnabled = false;
                    cb_re.IsEnabled = false;
                    cmb_fielditem.IsEnabled = false;

                    cmb_fielditem.SelectedIndex = -1;
                    cmb_fielditem.Text = "";
                    cmb_fielditem.Items.Clear();
                    AyMessageBox.ShowError("数据填充时发生错误:" + ex.ToString());
                }
                finally
                {
                    if (!readTableNames)
                    {
                        if (list.Count > 0)
                        {
                            if (cmb_tables.IsEnabled)
                            {
                                btn_inputDataToDatabase.IsEnabled = true;
                            }
                        }
                    }
                    btn_selectFile.IsEnabled = true;
                    btn_readFileData.IsEnabled = true;
                }
                cb_empty.IsEnabled = true;
                cb_re.IsEnabled = true;
                cmb_fielditem.IsEnabled = true;

                int f_count = fieldArray.Count();
                int h_count = headerArray.Count();
                cmb_fielditem.Items.Add(new ComboBoxItem());
                for (i = 0;i < f_count && i < h_count; i++)
                {
                    ComboBoxItem cbi = new ComboBoxItem();
                    cbi.Content = headerArray[i];
                    cbi.Uid = fieldArray[i];
                    cmb_fielditem.Items.Add(cbi);
                }
            });
            input_state_col_val.DisplayIndex = table_datagrid.Columns.Count();
            table_datagrid.Columns.Add(input_state_col_val);

            reFlag = false;
            emptyFlag = false;
            cb_re.IsChecked = false;
            cb_empty.IsChecked = false;
            cmb_fielditem.SelectedIndex = -1;
            cmb_fielditem.Text = "";

            //添加行数据
            //table_datagrid.
            GC.Collect();
        }

        private void btn_selectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Excel 2007 (*.xlsx)|*.xlsx|Excel 2003 (*.xls)|*.xls"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                filePath = this.txt_filePath.Text = openFileDialog.FileName;
            }
        }

        private void InsertColumn(int index,String header,String width)
        {
            DataGridTemplateColumn col = null;
            if (StringUtil.isBlank(width))
            {
                return;
            }
            if ("*".Equals(width))
            {
                col = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(myself_col);
            }else
            {
                col = new DataGridTemplateColumn();
                try
                {
                    int width_value = Int32.Parse(width);
                    col.Width = width_value;
                }
                catch
                {
                    col.Width = 60;
                }
            }
            col.Header = header;
            Setter setter = new Setter(DataGridColumnHeader.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            Style style = new Style();
            style.Setters.Add(setter);
            col.HeaderStyle = style;
            table_datagrid.Columns.Insert(index, col);
        }

        private void AddColumn(String header, String width,Int32 i)
        {
            DataGridTemplateColumn col = null;
            if (StringUtil.isBlank(width))
            {
                return;
            }
            if ("*".Equals(width))
            {
                col = TransExpV2<DataGridTemplateColumn, DataGridTemplateColumn>.Trans(myself_col);
            }
            else
            {
                col = new DataGridTemplateColumn();
                try
                {
                    int width_value = Int32.Parse(width);
                    col.Width = width_value;
                }
                catch
                {
                    col.Width = 60;
                }
            }
            col.Header = header;

            StringBuilder CellTemp = new StringBuilder();
            CellTemp.Append("<DataTemplate ");
            CellTemp.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            CellTemp.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            CellTemp.Append("xmlns:local = 'clr-namespace:ExcelToDatabase.Views.Home'>");
            CellTemp.Append("<TextBlock ");
            CellTemp.Append("HorizontalAlignment='Left' ");
            CellTemp.Append("Text='{ Binding _" + i + "}'");
            CellTemp.Append("></TextBlock>");
            CellTemp.Append("</DataTemplate>");
            col.CellTemplate = (DataTemplate)XamlReader.Load(new MemoryStream(Encoding.ASCII.GetBytes(CellTemp.ToString())));

            Setter setter = new Setter(DataGridColumnHeader.HorizontalContentAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            Style style = new Style();
            style.Setters.Add(setter);
            col.HeaderStyle = style;
            col.DisplayIndex = table_datagrid.Columns.Count();
            table_datagrid.Columns.Add(col);
        }

        private void btn_readFileData_Click(object sender, RoutedEventArgs e)
        {
            AyTime.setTimeout(1,()=> {
                btn_exit.IsEnabled = false;
                btn_inputDataToDatabase.IsEnabled = false;
                btn_readFileData.IsEnabled = false;
                btn_selectFile.IsEnabled = false;
                ReadExcelData(filePath);
                btn_exit.IsEnabled = true;
            });
            GC.Collect();
        }

        private void btn_databaseConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigPage page = new ConfigPage();
            page.Width = 500;
            page.Height = 225;
            Global.configPage = page;
            AyLayer.ShowDialog(null, page, "数据库配置", new AyLayerOptions {
                LayerId = "ConfigPage".ToLower()
            });
        }

        private void txt_filePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            filePath = @txt_filePath.Text;
        }

        public void SetValue(ref RowViewItem val, int j, String value)
        {
            switch (j)
            {
                case 1:
                    val._1 = value;
                    break;
                case 2:
                    val._2 = value;
                    break;
                case 3:
                    val._3 = value;
                    break;
                case 4:
                    val._4 = value;
                    break;
                case 5:
                    val._5 = value;
                    break;
                case 6:
                    val._6 = value;
                    break;
                case 7:
                    val._7 = value;
                    break;
                case 8:
                    val._8 = value;
                    break;
                case 9:
                    val._9 = value;
                    break;
                case 10:
                    val._10 = value;
                    break;
                case 11:
                    val._11 = value;
                    break;
                case 12:
                    val._12 = value;
                    break;
                case 13:
                    val._13 = value;
                    break;
                case 14:
                    val._14 = value;
                    break;
                case 15:
                    val._15 = value;
                    break;
                case 16:
                    val._16 = value;
                    break;
                case 17:
                    val._17 = value;
                    break;
                case 18:
                    val._18 = value;
                    break;
                case 19:
                    val._19 = value;
                    break;
                case 20:
                    val._20 = value;
                    break;
                case 21:
                    val._21 = value;
                    break;
                case 22:
                    val._22 = value;
                    break;
                case 23:
                    val._23 = value;
                    break;
                case 24:
                    val._24 = value;
                    break;
                case 25:
                    val._25 = value;
                    break;
                case 26:
                    val._26 = value;
                    break;
                case 27:
                    val._27 = value;
                    break;
                case 28:
                    val._28 = value;
                    break;
                case 29:
                    val._29 = value;
                    break;
                case 30:
                    val._30 = value;
                    break;
                case 31:
                    val._31 = value;
                    break;
                case 32:
                    val._32 = value;
                    break;
                case 33:
                    val._33 = value;
                    break;
                case 34:
                    val._34 = value;
                    break;
                case 35:
                    val._35 = value;
                    break;
                case 36:
                    val._36 = value;
                    break;
                case 37:
                    val._37 = value;
                    break;
                case 38:
                    val._38 = value;
                    break;
                case 39:
                    val._39 = value;
                    break;
                case 40:
                    val._40 = value;
                    break;
                case 41:
                    val._41 = value;
                    break;
                case 42:
                    val._42 = value;
                    break;
                case 43:
                    val._43 = value;
                    break;
                case 44:
                    val._44 = value;
                    break;
                case 45:
                    val._45 = value;
                    break;
                case 46:
                    val._46 = value;
                    break;
                case 47:
                    val._47 = value;
                    break;
                case 48:
                    val._48 = value;
                    break;
                case 49:
                    val._49 = value;
                    break;
                case 50:
                    val._50 = value;
                    break;
                case 51:
                    val._51 = value;
                    break;
                case 52:
                    val._52 = value;
                    break;
                case 53:
                    val._53 = value;
                    break;
                case 54:
                    val._54 = value;
                    break;
                case 55:
                    val._55 = value;
                    break;
                case 56:
                    val._56 = value;
                    break;
                case 57:
                    val._57 = value;
                    break;
                case 58:
                    val._58 = value;
                    break;
                case 59:
                    val._59 = value;
                    break;
                case 60:
                    val._60 = value;
                    break;
                case 61:
                    val._61 = value;
                    break;
                case 62:
                    val._62 = value;
                    break;
                case 63:
                    val._63 = value;
                    break;
                case 64:
                    val._64 = value;
                    break;
                case 65:
                    val._65 = value;
                    break;
                case 66:
                    val._66 = value;
                    break;
                case 67:
                    val._67 = value;
                    break;
                case 68:
                    val._68 = value;
                    break;
                case 69:
                    val._69 = value;
                    break;
                case 70:
                    val._70 = value;
                    break;
                case 71:
                    val._71 = value;
                    break;
                case 72:
                    val._72 = value;
                    break;
                case 73:
                    val._73 = value;
                    break;
                case 74:
                    val._74 = value;
                    break;
                case 75:
                    val._75 = value;
                    break;
                case 76:
                    val._76 = value;
                    break;
                case 77:
                    val._77 = value;
                    break;
                case 78:
                    val._78 = value;
                    break;
                case 79:
                    val._79 = value;
                    break;
                case 80:
                    val._80 = value;
                    break;
                case 81:
                    val._81 = value;
                    break;
                case 82:
                    val._82 = value;
                    break;
                case 83:
                    val._83 = value;
                    break;
                case 84:
                    val._84 = value;
                    break;
                case 85:
                    val._85 = value;
                    break;
                case 86:
                    val._86 = value;
                    break;
                case 87:
                    val._87 = value;
                    break;
                case 88:
                    val._88 = value;
                    break;
                case 89:
                    val._89 = value;
                    break;
                case 90:
                    val._90 = value;
                    break;
                case 91:
                    val._91 = value;
                    break;
                case 92:
                    val._92 = value;
                    break;
                case 93:
                    val._93 = value;
                    break;
                case 94:
                    val._94 = value;
                    break;
                case 95:
                    val._95 = value;
                    break;
                case 96:
                    val._96 = value;
                    break;
                case 97:
                    val._97 = value;
                    break;
                case 98:
                    val._98 = value;
                    break;
                case 99:
                    val._99 = value;
                    break;
                case 100:
                    val._100 = value;
                    break;

            }
        }

        public String GetCellValue(int row,int col)
        {
            if (row < 0 || row >= rowCount || col < 1 || col > colCount)
            {
                return null;
            }
            switch (col)
            {
                case 1: return list[row]._1;
                case 2: return list[row]._2;
                case 3: return list[row]._3;
                case 4: return list[row]._4;
                case 5: return list[row]._5;
                case 6: return list[row]._6;
                case 7: return list[row]._7;
                case 8: return list[row]._8;
                case 9: return list[row]._9;
                case 10: return list[row]._10;
                case 11: return list[row]._11;
                case 12: return list[row]._12;
                case 13: return list[row]._13;
                case 14: return list[row]._14;
                case 15: return list[row]._15;
                case 16: return list[row]._16;
                case 17: return list[row]._17;
                case 18: return list[row]._18;
                case 19: return list[row]._19;
                case 20: return list[row]._20;
                case 21: return list[row]._21;
                case 22: return list[row]._22;
                case 23: return list[row]._23;
                case 24: return list[row]._24;
                case 25: return list[row]._25;
                case 26: return list[row]._26;
                case 27: return list[row]._27;
                case 28: return list[row]._28;
                case 29: return list[row]._29;
                case 30: return list[row]._30;
                case 31: return list[row]._31;
                case 32: return list[row]._32;
                case 33: return list[row]._33;
                case 34: return list[row]._34;
                case 35: return list[row]._35;
                case 36: return list[row]._36;
                case 37: return list[row]._37;
                case 38: return list[row]._38;
                case 39: return list[row]._39;
                case 40: return list[row]._40;
                case 41: return list[row]._41;
                case 42: return list[row]._42;
                case 43: return list[row]._43;
                case 44: return list[row]._44;
                case 45: return list[row]._45;
                case 46: return list[row]._46;
                case 47: return list[row]._47;
                case 48: return list[row]._48;
                case 49: return list[row]._49;
                case 50: return list[row]._50;
                case 51: return list[row]._51;
                case 52: return list[row]._52;
                case 53: return list[row]._53;
                case 54: return list[row]._54;
                case 55: return list[row]._55;
                case 56: return list[row]._56;
                case 57: return list[row]._57;
                case 58: return list[row]._58;
                case 59: return list[row]._59;
                case 60: return list[row]._60;
                case 61: return list[row]._61;
                case 62: return list[row]._62;
                case 63: return list[row]._63;
                case 64: return list[row]._64;
                case 65: return list[row]._65;
                case 66: return list[row]._66;
                case 67: return list[row]._67;
                case 68: return list[row]._68;
                case 69: return list[row]._69;
                case 70: return list[row]._70;
                case 71: return list[row]._71;
                case 72: return list[row]._72;
                case 73: return list[row]._73;
                case 74: return list[row]._74;
                case 75: return list[row]._75;
                case 76: return list[row]._76;
                case 77: return list[row]._77;
                case 78: return list[row]._78;
                case 79: return list[row]._79;
                case 80: return list[row]._80;
                case 81: return list[row]._81;
                case 82: return list[row]._82;
                case 83: return list[row]._83;
                case 84: return list[row]._84;
                case 85: return list[row]._85;
                case 86: return list[row]._86;
                case 87: return list[row]._87;
                case 88: return list[row]._88;
                case 89: return list[row]._89;
                case 90: return list[row]._90;
                case 91: return list[row]._91;
                case 92: return list[row]._92;
                case 93: return list[row]._93;
                case 94: return list[row]._94;
                case 95: return list[row]._95;
                case 96: return list[row]._96;
                case 97: return list[row]._97;
                case 98: return list[row]._98;
                case 99: return list[row]._99;
                case 100: return list[row]._100;
                default: return null;
            }
        }

        public String GetRowValueToString(DataGrid table, int row)
        {
            String[] valueString = new String[colCount];
            RowViewItem rowItem = list[row];
            for (int i = 1;i <= colCount; i++)
            {
                String cellString = GetCellValue(row, i);
                if (!"UUID()".Equals(cellString.Trim()))
                {
                    valueString[i - 1] = "'" + cellString + "'";
                }else
                {
                    valueString[i - 1] = cellString;
                }
            }
            return StringUtil.changeArrayToString(valueString,",");
        }

        public delegate void InputDataEventHandler();

        private void btn_inputDataToDatabase_Click(object sender, RoutedEventArgs e)
        {
            //InputDataEventHandler inputdate = new InputDataEventHandler(InputDateToDatabase);
            //inputdate();
            //bgWorker.RunWorkerAsync();

            //DispatcherTimer _timer = new DispatcherTimer();
            //_timer.Interval = TimeSpan.FromMilliseconds(100);
            //_timer.Tick += new EventHandler(delegate (object s, EventArgs a)
            //{
            //    InputDateToDatabase(_timer);
            //});
            //_timer.Start();
            InputDateToDatabase();
        }

        //判断重复未写！
        //导入功能未写！
        public async void InputDateToDatabase()
        {
            if (StringUtil.isBlank(cmb_tables.Text))
            {
                AyMessageBox.ShowError("未选择数据表！");
                return;
            }
            String table_name = cmb_tables.Text;
            String fieldString = cmb_fielditem.Text;
            int fieldIndex = cmb_fielditem.SelectedIndex;
            if (reFlag && StringUtil.isBlank(fieldString))
            {
                AyMessageBox.ShowError("未选择需要筛选的重复字段！");
                return;
            }

            btn_exit.IsEnabled = false;
            btn_databaseConfig.IsEnabled = false;
            btn_inputDataToDatabase.IsEnabled = false;
            btn_readFileData.IsEnabled = false;
            btn_selectFile.IsEnabled = false;

            if (emptyFlag)
            {
                try
                {
                    Global.my.Query("Truncate Table " + table_name);
                }
                catch
                {
                    AyMessageBox.ShowError("清空表数据失败！");
                    return;
                }
            }

            Int32 successCount = 0, failedCount = 0, reCount = 0;
            String sqlStr = "INSERT INTO " + table_name + " (" + StringUtil.changeArrayToString(fieldArray, ",") + ") VALUES ";
            String[] valueString = new String[rowCount];
            List<RowViewItem> list_no_input = list.Where(x => true).ToList();
            if (list_no_input != null)
            {
                if (list_no_input.Count > 0)
                {
                    int list_no_input_count = list_no_input.Count;
                    for (int i = 0; i < list_no_input_count; i++)
                    {
                        valueString[i] = "(" + GetRowValueToString(table_datagrid, i) + ")";
                        await Task.Run(() => {
                            Thread.Sleep(5);
                            if (reFlag && StringUtil.isNotBlank(fieldString))
                            {
                                String reSqlString = "select count(1) from " + table_name + " where " + fieldArray[fieldIndex - 1] + " = '" + GetCellValue(i, fieldIndex) + "'";
                                String baseCountString = Global.my.ExecuteSelect(reSqlString);
                                int baseCount = Int32.Parse(baseCountString);
                                if (baseCount > 0)
                                {
                                    list_no_input[i]._state = "数据重复";
                                    reCount++;
                                }else
                                {
                                    if (Global.my.Query(sqlStr + valueString[i]))
                                    {
                                        list_no_input[i]._state = "成功";
                                        successCount++;
                                    }
                                    else
                                    {
                                        list_no_input[i]._state = "数据失败";
                                        failedCount++;
                                    }
                                }
                            }else
                            {
                                if (Global.my.Query(sqlStr + valueString[i]))
                                {
                                    list_no_input[i]._state = "成功";
                                    successCount++;
                                }
                                else
                                {
                                    list_no_input[i]._state = "数据失败";
                                    failedCount++;
                                }
                            }
                        });
                    }
                    table_datagrid.ItemsSource = list;
                }
            }

            btn_exit.IsEnabled = true;
            btn_databaseConfig.IsEnabled = true;
            btn_inputDataToDatabase.IsEnabled = true;
            btn_readFileData.IsEnabled = true;
            btn_selectFile.IsEnabled = true;
            AyMessageBox.ShowInformation("导入结束，共执行：" + list_no_input.Count + " 条命令" 
                + "\n成功：" + successCount + " 条"
                + "\n重复：" + reCount + " 条"
                + "\n失败：" + failedCount + " 条");

            GC.Collect();
        }

        private void table_datagrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //DataGridRow row = e.Row as DataGridRow;
            //if (row == null)
            //{
            //    return;
            //}
            //RowViewItem rowData = e.Row.Item as RowViewItem;
            //if (rowData != null)
            //{
            //    if ("导入成功".Equals(rowData._state))
            //    {
            //        row.Background = new SolidColorBrush(Colors.GreenYellow);
            //    }
            //}
        }

        private void reCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            cmb_fielditem.IsEnabled = true;
            reFlag = true;
        }

        private void reCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            cmb_fielditem.IsEnabled = false;
            cmb_fielditem.SelectedIndex = -1;
            cmb_fielditem.Text = "";
            reFlag = false;
        }

        private void emptyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            emptyFlag = true;
        }

        private void emptyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            emptyFlag = false;
        }

        private void btn_gettables_Click(object sender, RoutedEventArgs e)
        {
            RefreshTablesNameComBoBox();
        }

        public async void RefreshTablesNameComBoBox()
        {
            readTableNames = true;
            String cmbString = "";
            if (StringUtil.isNotBlank(cmb_tables.Text))
            {
                cmbString = cmb_tables.Text;
            }
            cmb_tables.IsEnabled = false;
            btn_inputDataToDatabase.IsEnabled = false;
            cmb_tables.Items.Clear();
            if (StringUtil.isBlank(Global.db_name))
            {
                AyMessageBox.ShowError("数据库名称为空！");
                return;
            }
            try
            {
                btn_gettables.IsEnabled = false;
                btn_databaseConfig.IsEnabled = false;
                List<TableItem> list_name = null;
                await Task.Run(() => {
                    try
                    {
                        list_name = Global.my.QueryList<TableItem>("select table_name as name from information_schema.tables where table_schema='" + Global.db_name + "' and table_type='base table';");
                    }
                    catch
                    {
                        list_name = null;
                        AyMessageBox.ShowError("获取数据库表信息失败！");
                    }
                    finally
                    {
                        readTableNames = false;
                    }
                });
                if (list_name != null)
                {
                    if (list_name.Count > 0)
                    {
                        for (int i = 0; i < list_name.Count; i++)
                        {
                            ComboBoxItem cbi = new ComboBoxItem();
                            cbi.Content = list_name[i].Name;
                            cmb_tables.Items.Add(cbi);
                            if (StringUtil.isNotBlank(cmbString))
                            {
                                if (cmbString.Equals(list_name[i].Name))
                                {
                                    cmb_tables.SelectedIndex = i;
                                }
                            }
                        }
                        cmb_tables.IsEnabled = true;
                        if (list.Count > 0)
                        {
                            btn_inputDataToDatabase.IsEnabled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                cmb_tables.IsEnabled = false;
                btn_inputDataToDatabase.IsEnabled = false;
            }
            finally
            {
                btn_gettables.IsEnabled = true;
                btn_databaseConfig.IsEnabled = true;
                GC.Collect();
            }
        }
    }
}
