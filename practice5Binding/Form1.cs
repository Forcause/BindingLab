using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using Chart = System.Web.UI.DataVisualization.Charting.Chart;

namespace practice5Binding
{
    public partial class Form1 : Form
    {
        //var t = ((int) 'u' + (int) 'k') % 8; Вариант 0 - автомобили   
        //var f = (int) 'K' % 3; Вариант 0 - отбор по всем строковым полям
        //var d = (int) 'U' % 3; - Вариант 1 - круговая диаграмма

        private List<Automobile> automobiles;
        BindingSource bs = new BindingSource();
        private BindingSource backup = new BindingSource();

        private Size areaSize;

        private static List<Automobile> JDMLegends()
        {
            List<Automobile> legends = new List<Automobile>()
            {
                new Automobile("Nissan", "Silvia s15", 1999, 790,
                    @"..\..\Images\silvia.jpg"),
                new Automobile("Toyota", "Mark 2 81", 1981, 888,
                    @"..\..\Images\mark2.jpg"),
                new Automobile("Mazda", "RX-7", 1992, 265,
                    @"..\..\Images\rx7.jpg"),
                new Automobile("Honda", "Civic ek9", 1997, 115,
                    @"..\..\Images\civic.jpg"),
                new Automobile("Toyota", "Supra A80", 1993, 330,
                    @"..\..\Images\supra.jpg"),
                new Automobile("Nissan", "Skyline r32", 1989, 225,
                    @"..\..\Images\sky.jpg")
            };
            return legends;
        }

        public Form1()
        {
            InitializeComponent();
            Text = "JDM Legends";
            Height = 800;
            Width = 1200;
            MaximumSize = new Size(1200, 800);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            areaSize = this.ClientSize;
            StartPosition = FormStartPosition.CenterScreen;
            automobiles = JDMLegends();
            bs.DataSource = automobiles;
            backup.DataSource = bs.DataSource;
            AddElements(bs);
        }

        private void AddElements(BindingSource bs)
        {
            BindingNavigator navigator = new BindingNavigator(true);
            navigator.BindingSource = bs;
            Controls.Add(navigator);

            DataGridView grid = new DataGridView()
            {
                Name = "grid",
                Height = areaSize.Height / 2,
                Width = areaSize.Width / 2,
                Top = navigator.Bottom,
                Left = this.Left,
            };
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grid.DataSource = bs;
            CreateGridColumns(grid);
            grid.RowValidating += grid_RowValidating;
            Controls.Add(grid);
            
            ToolStripButton saving = new ToolStripButton();
            saving.Image = Image.FromFile(@"..\..\Images\save_ico.png");
            saving.Enabled = true;
            saving.Click += SaveStripButton;
            navigator.Items.Add(saving);

            ToolStripButton load = new ToolStripButton();
            load.Image =
                Image.FromFile(
                    @"..\..\Images\download.png");
            load.Enabled = true;
            load.Click += LoadStripButton;
            navigator.Items.Add(load);

            navigator.Items.Add(new ToolStripSeparator());
            ToolStripTextBox txtBox = new ToolStripTextBox();
            txtBox.TextChanged += SearchText;
            navigator.Items.Add(txtBox);

            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.DataSource = from p in bs.DataSource as List<Automobile>
                group p by p.AutoCategory
                into g
                select new
                {
                    AutoCategory = g.Key.ToString(), Count = g.Count()
                };
            chart1.ChartAreas.Add(new ChartArea());
            chart1.ChartAreas[0].AxisX.Title = "Класс ТС";
            chart1.ChartAreas[0].AxisY.Title = "Кол-во";
            chart1.Series.Add(new Series());
            chart1.Series[0].ChartType = SeriesChartType.Pie;
            chart1.Series[0].XValueMember = "AutoCategory";
            chart1.Series[0].YValueMembers = "Count";
            chart1.Titles.Add("Категории машин");
            bs.CurrentChanged += (o, e) => chart1.DataBind();
            bs.DataSourceChanged += (o, e) =>
            {
                chart1.DataSource = from p in bs.DataSource as List<Automobile>
                    group p by p.AutoCategory
                    into g
                    select new
                    {
                        AutoCategory = g.Key.ToString(), Count = g.Count()
                    };
            };

            PictureBox picBox = new PictureBox()
            {
                Height = areaSize.Height / 2,
                Width = areaSize.Width / 2,
                Top = this.Top,
                Left = grid.Right,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            picBox.DataBindings.Add("ImageLocation", bs, "ImageFile");
            Controls.Add(picBox);
            picBox.DoubleClick += (o, e) =>
            {
                string fileName = AddImage((bs.Current as Automobile).ImageFile);
                (bs.Current as Automobile).ImageFile = fileName;
            };

            PropertyGrid propGrid = new PropertyGrid()
            {
                Height = areaSize.Height / 2,
                Width = areaSize.Width / 2,
                Left = chart1.Right,
                Top = picBox.Bottom,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            propGrid.DataBindings.Add("SelectedObject", bs, "");
            Controls.Add(propGrid);
        }

        private void SearchText(object sender, EventArgs e)
        {
            if ((sender as ToolStripTextBox).Text.Length != 0)
            {
                string txt = (sender as ToolStripTextBox).Text.ToUpper();
                bs.DataSource = Search(backup.DataSource, txt);
            }
            else bs.DataSource = backup.DataSource;
        }

        private object Search(object src, string txt)
        {
            var res = (List<Automobile>) src;
            res = res.Where(m =>
                m.Mark.ToUpper().Contains(txt) || m.Model.ToUpper().Contains(txt) ||
                m.AutoCategory.ToString().ToUpper().Contains(txt)).ToList();
            return res;
        }

        private void grid_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            var markCheck = (Controls["grid"] as DataGridView)["Mark", e.RowIndex].Value;
            var yearCheck = (Controls["grid"] as DataGridView)["YearOfRelease", e.RowIndex].Value;
            if (markCheck == null)
            {
                e.Cancel = true;
                (Controls["grid"] as DataGridView).CurrentCell = (Controls["grid"] as DataGridView)["Mark", e.RowIndex];
                (Controls["grid"] as DataGridView).BeginEdit(true);
            }

            int year;
            if (int.TryParse(Convert.ToString(yearCheck), out year) && year < 1886 || year > 2050)
            {
                e.Cancel = true;
                (Controls["grid"] as DataGridView).CurrentCell =
                    (Controls["grid"] as DataGridView)["YearOfRelease", e.RowIndex];
                (Controls["grid"] as DataGridView).BeginEdit(true);
            }
        }

        private void LoadStripButton(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "Файл в bin|*.bin|Файл в xml|*.xml";
            if (of.ShowDialog() == DialogResult.OK)
            {
                switch (of.FilterIndex)
                {
                    case 1:
                        BinaryDeserialize(of.FileName);
                        break;
                    case 2:
                        XmlDesrialize(of.FileName);
                        break;
                }
            }
        }

        private void BinaryDeserialize(string ofFileName)
        {
            Stream st = new FileStream(ofFileName, FileMode.Open);
            var fmt = new BinaryFormatter();
            automobiles = (List<Automobile>)fmt.Deserialize(st);
            bs.DataSource = automobiles;
            backup.DataSource = bs.DataSource;
            st.Close();
            bs.ResetBindings(false);
        }

        private void XmlDesrialize(string ofFileName)
        {
            Stream sw = new FileStream(ofFileName, FileMode.Open);
            var xmlSer = new XmlSerializer(typeof(List<Automobile>));
            automobiles = (List<Automobile>) xmlSer.Deserialize(sw);
            bs.DataSource = automobiles;
            backup.DataSource = bs.DataSource;
            sw.Close();
            bs.ResetBindings(false);
        }

        private void SaveStripButton(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = System.Environment.CurrentDirectory;
            sfd.Filter = "Файл в bin|*.bin|Файл в xml|*.xml";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (sfd.FilterIndex)
                {
                    case 1:
                        BinarySerialize(sfd.FileName);
                        break;
                    case 2:
                        XMLSerialize(sfd.FileName);
                        break;
                }
            }
        }

        private void BinarySerialize(string sfdFileName)
        {
            BinaryFormatter bin = new BinaryFormatter();
            Stream sw = new FileStream(sfdFileName, FileMode.Create);
            bin.Serialize(sw, bs.DataSource);
            sw.Close();
        }

        private void XMLSerialize(string sfdFileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Automobile>));
            using (Stream sw = new FileStream(sfdFileName, FileMode.Create))
            {
                xmlSerializer.Serialize(sw, bs.DataSource);
            }
        }

        private string AddImage(string curFileName)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            f.ShowDialog();
            string fileName = f.FileName;
            if (fileName != "") return fileName;
            else return curFileName;
        }

        private void CreateGridColumns(DataGridView grid)
        {
            var column1 = new DataGridViewTextBoxColumn();
            column1.Name = "Mark";
            column1.HeaderText = "Марка";
            column1.DataPropertyName = "Mark";
            grid.Columns.Add(column1);

            var column2 = new DataGridViewTextBoxColumn();
            column2.Name = "Model";
            column2.HeaderText = "Модель";
            column2.DataPropertyName = "Model";
            grid.Columns.Add(column2);

            var column3 = new DataGridViewTextBoxColumn();
            column3.Name = "YearOfRelease";
            column3.HeaderText = "Год выпуска";
            column3.DataPropertyName = "YearOfRelease";
            grid.Columns.Add(column3);

            var col2 = new DataGridViewComboBoxColumn();
            col2.Items.AddRange(Enum.GetNames(typeof(Automobile.AutoHPCategory)));
            grid.Columns.Add(col2);

            var column4 = new DataGridViewTextBoxColumn();
            column4.Name = "HorsePower";
            column4.HeaderText = "Кол-во ЛС";
            column4.DataPropertyName = "HorsePower";
            grid.Columns.Add(column4);

            var column5 = new DataGridViewTextBoxColumn();
            column5.Name = "AutoCategory";
            column5.HeaderText = "Категория ТС";
            column5.DataPropertyName = "AutoCategory";
            grid.Columns.Add(column5);
        }
    }
}