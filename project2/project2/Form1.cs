using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace project2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            g = panel.CreateGraphics();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (draw)
            {
                //Draw
                g.Clear(Color.White);
                //Calculate the posotion of each vertex
                double angle = Math.PI * 2 / m;
                int[,] mPosition = new int[m, 2];
                for (int i = 0; i != m; ++i)
                {
                    mPosition[i, 0] = 150 + Convert.ToInt32(Math.Cos(i * angle) * 120);//x-coordinate
                    mPosition[i, 1] = 150 + Convert.ToInt32(Math.Sin(i * angle) * 120);//y-coordinate
                }
                for (int j = 0; j != n; ++j)
                {
                    Pen pen = null;
                    switch (Ecolor[j])
                    {
                        case 1: pen = new Pen(Color.Red, 2); break;
                        case 2: pen = new Pen(Color.Orange, 2); break;
                        case 3: pen = new Pen(Color.Yellow, 2); break;
                        case 4: pen = new Pen(Color.Green, 2); break;
                        case 5: pen = new Pen(Color.Blue, 2); break;
                        case 6: pen = new Pen(Color.Indigo, 2); break;
                        case 7: pen = new Pen(Color.Violet, 2); break;
                    }
                    int u = -1, v = 0;
                    for (int i = 0; i != m; ++i)
                        if (IM[i, j] == 1)
                            if (u == -1)
                                u = i;
                            else
                            {
                                v = i;
                                break;
                            }
                    Point p1 = new Point(mPosition[u, 0], mPosition[u, 1]);
                    Point p2 = new Point(mPosition[v, 0], mPosition[v, 1]);
                    g.DrawLine(pen, p1, p2);
                }
            }
        }

        private int[,] IM = null;
        private int m = 0;
        private int n = 0;
        private int[] Ecolor = null;//Record the color of each edge
        private int[,] Vcolor = null;//Record the color of each vertex
        private Graphics g = null;
        private bool draw = false;

        private int FindEdge(int v1, int v2)
        {
            for (int j = 0; j != n; ++j )
                if(IM[v1, j] == 1 && IM[v2, j] == 1)
                    return j;
            return -1;
        }

        private void FindFan(List<int> F, int u, int v)
        {
            int x = -1;
            for (int j = 0; j != n; ++j)
            {
                int xx = -1;
                if (IM[u, j] == 1 && IM[v, j] == 0)//find a edge connected to u but not to v
                    for (int i = 0; i != m; ++i)
                        if(IM[i, j] == 1 && i != u)//find another vertex x of that edge
                        {
                            xx = i;
                            break;
                        }
                if (xx == -1 || Ecolor[j] == 0)
                    continue;
                if (!F.Contains(xx) && Vcolor[v, Ecolor[j]] == 0)
                {
                    x = xx;
                    break;
                }
            }
            if (x == -1)//F is maximal
                return;
            else 
            {
                F.Add(x);
                FindFan(F, u, x);
            }
        }

        private void FindPath(List<int> path, int start, ref int end, int d, int c)
        {
            for (int j = 0; j != n; ++j)
                if (IM[start, j] == 1 && Ecolor[j] == d)
                {
                    path.Add(j);
                    for (int i = 0; i != m; ++i)
                        if (IM[i, j] == 1 && i != start)
                        {
                            end = i;
                            FindPath(path, i, ref end, c, d);
                        }
                }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Input
            m = Convert.ToInt32(mv.Text);
            n = Convert.ToInt32(nv.Text);
            IM = new int[m, n];//incidence matrix
            string cin = input.Text;
            int i = 0, j = 0;
            foreach (char c in cin)
            {
                if (c != '1' && c != '0')
                    continue;
                else if (c == '1')
                    IM[i, j] = 1;
                else
                    IM[i, j] = 0;
                if (++j == n)
                {
                    ++i;
                    j = 0;
                }
            }
            ///////////////////////////////////////
            //Misra & Gries edge coloring algorithm
            Ecolor = new int[n];//Record the color of each edge
            Vcolor = new int[m, m + 1];//Record the colors not free on each vertex
            /*color is an integer from 1; 
             *0 indicates the eadge hasn't been colored*/
            for (int edge = 0; edge != n; ++edge )
            {
                //find u, v relative to the edge
                int u = -1, v = 0;
                for (i = 0; i != m; ++i)
                    if (IM[i, edge] == 1)
                        if(u == -1)
                            u = i;
                        else
                        {
                            v = i;
                            break;
                        }
                //Find maximal fan of u
                List<int> F = new List<int>();
                F.Add(v);
                FindFan(F, u, v);
                int k = F.Count;
                /*now we have F[0:k-1] be a maximal fan of u*/
                int c = 0, d = 0;//two colors
                for (int color = 1; color != m + 1; ++color)
                    if (Vcolor[u, color] == 0)//means color is free on u
                    {
                        c = color;
                        break;
                    }
                for (int color = 1; color != m + 1; ++color)
                    if (Vcolor[F[k - 1], color] == 0)//means color is free on F[k-1]
                    {
                        d = color;
                        break;
                    }
                //invert the cdu path
                /*note if there is a path it must start from u with color d*/
                if (c != d)
                {
                    List<int> path = new List<int>();
                    int end = -1;
                    FindPath(path, u, ref end, d, c);
                    if (end != -1)//there is a such path, then invert it
                    {
                        int tempc = c + d;
                        foreach (int p in path)
                            Ecolor[p] = tempc - Ecolor[p];
                        Vcolor[u, d] = 0;
                        Vcolor[u, c] = 1;
                        Vcolor[end, c] = 1 - Vcolor[end, c];
                        Vcolor[end, d] = 1 - Vcolor[end, d];
                    }
                }
                //find w
                int w = 0;
                for(int index = 0; index != k; ++index)
                    if (Vcolor[F[index], d] == 0)
                    {
                        w = index;
                        break;
                    }
                //Rotate F'[0:w] and set C(u,F[w])=d
                for (int index = 0; index != w; ++index)
                {
                    int e1 = FindEdge(F[index], u);
                    int e2 = FindEdge(F[index + 1], u);
                    Vcolor[F[index], Ecolor[e1]] = 0;
                    Vcolor[F[index], Ecolor[e2]] = 1;
                    Ecolor[e1] = Ecolor[e2];
                    if (index + 1 == w)
                    {
                        int ew = FindEdge(F[w], u);
                        Vcolor[F[w], Ecolor[ew]] = 0;
                        Ecolor[ew] = 0;
                    }
                }
                Ecolor[FindEdge(u, F[w])] = d;
                Vcolor[u, d] = 1;
                Vcolor[F[w], d] = 1;
            }
            /////////////////////////////////
            //Output
            string cout = "";
            int max = 0;
            foreach (int c in Ecolor)
                if (c > max)
                    max = c;
            cout += "Number of Colors: ";
            cout += Convert.ToString(max) + "\r\n";
            cout += "Color of Each Edge:\r\n";
            for (j = 0; j != n; ++j)
                cout += Convert.ToString(Ecolor[j]) + " ";
            cout += "\r\n\r\n";
            cout += "Confirmation of Correctness:\r\n";
            for (i = 0; i != m; ++i)
            {
                cout += "Connected to Vertex " + Convert.ToString(i) + ":\r\n";
                for (j = 0; j != n; ++j)
                    if (IM[i, j] == 1)
                        cout += "Edge " + Convert.ToString(j) + ": " + Convert.ToString(Ecolor[j]) + "\r\n";
            }
            output.Text = cout;
            //////////////////////////////////
            //If the number of colors is less than 8 then draw the graph
            if (max > 7)
            {
                panel.Visible = false;
                draw = false;
            }
            else
            {
                panel.Visible = true;
                draw = true;
            }
        }

        private void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            if (openFileDialog1.FileName != "")
            {
                StreamReader sr = new StreamReader(openFileDialog1.OpenFile());
                string temp = sr.ReadLine();
                string m = "", n = "";
                bool flag = true;
                foreach (char c in temp)
                {
                    if (c >= '0' && c <= '9')
                        if (flag)
                            m += c;
                        else
                            n += c;
                    else
                        if (flag)
                            flag = false;
                }
                mv.Text = m;
                nv.Text = n;
                input.Text = sr.ReadToEnd();
                sr.Close();
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Project:\tImplementation of Edge Coloring\r\nAuthor:\t李玉翎飞");
        }

        private void readmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("1) m is the number of vertexes and n is the number of edges.\r\n2) The incidence matrix is inputed in a format of m*n.\r\n3) A test file can be imported.\r\n4) Only 7 colors are supported for drawing.");
        }
    }
}
