using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TV_Lab
{
    public partial class Form1 : Form
    {
        RV exper = new RV();
        public Form1()
        {
            InitializeComponent();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void START_1_Click(object sender, EventArgs e)
        {
            TABLE_1.Rows.Clear();
            TABLE_2.Rows.Clear();
            SHOW_VALUE.Enabled = true;
            CLEAR_1.Enabled = true;
            exper.finalize();
            exper = new RV();
            exper.Set(Convert.ToDouble(INTENSITY.Text), Convert.ToDouble(TIME.Text), Convert.ToInt32(SIZE_EXP.Text));
            exper.GenModel();
            exper.FilterOriginal();
            int j = 0;
            for (int i = 0; i < exper.sort_filt_count; i++)
            {
                double var = (double)(exper.sort_filt_yi[i].ni) / (double)exper.count;
               
                {
                    TABLE_1.Rows.Insert(j++, exper.sort_filt_yi[i].number + 1, exper.sort_filt_yi[i].res, exper.sort_filt_yi[i].ni, var, exper.sort_filt_yi[i].pi, exper.sort_filt_yi[i].prob_disc);
                }
            }

         
            exper.FindXMedium(); exper.SearchCh();
            //exper.CalcEn();// матожидание
            //exper.CalcRealDn();
            //exper.CalcCHOSEDn();
            //exper.CalcMe();
            //exper.CalcR();
            exper.FnReal(0.000001);
            exper.FnEx();
            exper.CalcD_outrun();
            exper.CalcProb_disc_max();
            TABLE_2.Rows.Insert(0, exper.En, exper.x_medium, Math.Abs(exper.En - exper.x_medium), exper.Dn, exper.Dn_medium, Math.Abs(exper.Dn - exper.Dn_medium), exper.Me, exper.R, exper.D_outrun);
            textBox1.Text = exper.prob_disc_max.ToString();
            if (CHART_2.Series.Count > 1)
            {
                int i = 1;
                while (CHART_2.Series.Count > 1)
                    CHART_2.Series.RemoveAt(i);
            }
           
            
            System.Windows.Forms.DataVisualization.Charting.Series serial = new System.Windows.Forms.DataVisualization.Charting.Series();
            serial.IsValueShownAsLabel = false;

            String name = CHART_2.Series[0].Name;
            serial.Name = name;
            j = 0;

            serial.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            serial.BorderWidth += 1;
            serial.Points.AddXY(0, 0);
            serial.Points.AddXY(exper.sort_filt_yi[0].res, exper.sort_filt_yi[0].pi);
            serial.Name = name + j++;
            serial.ChartArea = "ChartArea1";
            serial.IsVisibleInLegend = false;
            serial.IsValueShownAsLabel = false;
            serial.Color = Color.Blue;

           

            serial = new System.Windows.Forms.DataVisualization.Charting.Series();
            for (int i = 0; i < exper.sort_filt_count-1; i++)
            {
                 
                   
                serial.Name = name + j++;

            }
            CHART_2.ChartAreas["ChartArea1"].AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 16);
            CHART_2.ChartAreas["ChartArea1"].AxisY.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 16);
            CHART_2.ChartAreas["ChartArea1"].AxisX.Title = "y";
            CHART_2.ChartAreas["ChartArea1"].AxisY.Title = "Fn(y)";


          

            String name2 = CHART_2.Series[0].Name;
         
            serial.Name = name2;
            
            serial = new System.Windows.Forms.DataVisualization.Charting.Series();
            serial.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            serial.IsValueShownAsLabel = false;

            System.Windows.Forms.DataVisualization.Charting.Series serialFn = new System.Windows.Forms.DataVisualization.Charting.Series();
            serialFn.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            serialFn.IsValueShownAsLabel = false;
          

            
                serial.Points.AddXY(0, 0);
                serial.Points.AddXY(exper.Fn_ex[0].left, 0);

                serial.Name = name2 + j++;
                serial.ChartArea = "ChartArea1";
                serial.IsVisibleInLegend = false;
                serial.BorderWidth += 2;
                serial.Color = Color.Blue;

                this.CHART_2.Series.Add(serial);

                serial = new System.Windows.Forms.DataVisualization.Charting.Series();
                serial.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                serial.IsValueShownAsLabel = false;
            

            for (int i = 0; i < exper.sort_filt_count; i++) // Строим выборочную Fn(x)
            {
                serial.Points.AddXY(exper.Fn_ex[i].left, exper.Fn_ex[i].p);
                serial.Points.AddXY(exper.Fn_ex[i].right, exper.Fn_ex[i].p);

                serial.Name = name2 + i+'b';
                serial.ChartArea = "ChartArea1";
                serial.IsVisibleInLegend = false;
                serial.BorderWidth += 2;
                serial.Color = Color.Blue;

                this.CHART_2.Series.Add(serial);

                serial = new System.Windows.Forms.DataVisualization.Charting.Series();
                serial.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                serial.IsValueShownAsLabel = false;
                ///



            }
            for (int i = 0; i < exper.sort_filt_Fn.Count(); i++) // строим реальную Fn(x)
            {
                serialFn.Points.AddXY(exper.sort_filt_Fn[i].left, exper.sort_filt_Fn[i].p);
                serialFn.Points.AddXY(exper.sort_filt_Fn[i].right, exper.sort_filt_Fn[i].p);

              
                serialFn.ChartArea = "ChartArea1";
                serialFn.IsVisibleInLegend = false;
                serialFn.BorderWidth += 2;
                serialFn.Color = Color.Red;

                

                serialFn = new System.Windows.Forms.DataVisualization.Charting.Series();
                serialFn.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                serialFn.IsValueShownAsLabel = false;


              
            }
            if (SHOW_VALUE.Checked)
            {
             
                for (int i = 0; i < exper.sort_filt_Fn.Count(); i++) // строим реальную Fn(x)
                {
                    serialFn.Points.AddXY(exper.sort_filt_Fn[i].left, exper.sort_filt_Fn[i].p);
                    serialFn.Points.AddXY(exper.sort_filt_Fn[i].right, exper.sort_filt_Fn[i].p);

                  
                    serialFn.ChartArea = "ChartArea1";
                    serialFn.IsVisibleInLegend = false;
                    serialFn.BorderWidth += 2;
                    serialFn.Color = Color.Red;

                    this.CHART_2.Series.Add(serialFn);

                    serialFn = new System.Windows.Forms.DataVisualization.Charting.Series();
                    serialFn.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    serialFn.IsValueShownAsLabel = false;



                }

            }

            int segments = Convert.ToInt32(textBox2.Text);
            textBox8.Text = (exper.Fn_ex.Length ).ToString();
            if (segments > exper.Fn_ex.Length - 1)
            {
                segments--;
                textBox2.Text = segments.ToString();
              
                return;
            }
           
            double level = Convert.ToDouble(textBox3.Text);
            exper.a_level = level;
            exper.size_segs = segments;
            bool good_test = exper.H0();

            dataGridView2.Rows.Clear();

            double sumqi = 0;//check logic
            for (int i = 0; i < segments; i++)
            {
                dataGridView2.Rows.Insert(i, "( " + exper.segments[i].l.ToString() + ";" + exper.segments[i].r.ToString() + " ]",
                    exper.segments[i].count,
                    exper.segments[i].p,//qi
                    exper.segments[i].count * exper.segments[i].p,//n*qi
                    (Math.Pow(exper.segments[i].n - exper.segments[i].count * exper.segments[i].p, 2)) / (exper.segments[i].count * exper.segments[i].p));//Ri0
                sumqi += exper.segments[i].p;
            }
            textBox9.Text = sumqi.ToString();

            textBox4.Text = exper.R0.ToString();
            textBox5.Text = exper.F_R0.ToString();
            if (good_test) { textBox6.Text = "Принято"; }
            else { textBox6.Text = "Не принято"; }
            textBox7.Text = (segments - 1).ToString();

        }

        private void CLEAR_1_Click(object sender, EventArgs e)
        {
            TABLE_1.Rows.Clear();
            TABLE_2.Rows.Clear();
         
            if (CHART_2.Series.Count > 1)
            {
                int i = 1;
                while (CHART_2.Series.Count > 1)
                    CHART_2.Series.RemoveAt(i);
            }
           
            CLEAR_1.Enabled = false;
 
            SHOW_VALUE.Enabled = false;
            SHOW_VALUE.Checked = false;
           
            exper.finalize();
        }

   

        private void стартToolStripMenuItem_Click(object sender, EventArgs e)
        {
            START_1_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int segments = Convert.ToInt32(textBox2.Text);
            textBox8.Text = (exper.Fn_ex.Length + 1).ToString();
            if (segments > exper.Fn_ex.Length - 1)
            {
                segments--;
                textBox2.Text = segments.ToString();

                return;
            }
            double level = Convert.ToDouble(textBox3.Text);
            exper.a_level = level;
            exper.size_segs = segments;
            bool good_test = exper.H0();

            dataGridView2.Rows.Clear();

            double sumqi = 0;//check logic
            for (int i = 0; i < segments; i++)
            {
                dataGridView2.Rows.Insert(i, "( " + exper.segments[i].l.ToString() + ";" + exper.segments[i].r.ToString() + " ]",
                    exper.segments[i].count,
                    exper.segments[i].p,//qi
                    (Math.Pow(exper.segments[i].n - exper.segments[i].count * exper.segments[i].p, 2)) / (exper.segments[i].count * exper.segments[i].p));//Ri0
                sumqi += exper.segments[i].p;
            }
            textBox9.Text = sumqi.ToString();

            textBox4.Text = exper.R0.ToString();
            textBox5.Text = exper.F_R0.ToString();
            if (good_test) { textBox6.Text = "Принято"; }
            else { textBox6.Text = "Не принято"; }
            textBox7.Text = (segments - 1).ToString();
        }
    }
}
