using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System.IO.Ports;
using AForge.Imaging;

namespace TestCam
{
    public partial class Form1 : Form
    {

        //Arduino
        static SerialPort _serialPort;
        public byte[] Buff = new byte[6];
        int R, G, B, yatayX, dikeyY;

        private Capture capture;
        private Image<Bgr, Byte> IMG;
        private Image<Bgr, Byte> IMG_Post;
        
        
        private Image<Gray, Byte> R_frame;
        private Image<Gray, Byte> G_frame;
        private Image<Gray, Byte> B_frame;
        private Image<Gray, Byte> GrayImg;
        private int Xpx, Ypx;

//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        
        
        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPort();
            _serialPort.PortName = "COM4";//Set your board COM
            _serialPort.BaudRate = 9600;


            if (capture == null) {
                try
                {
                    capture = new Capture(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Bilgi1");
                }

            }
        }

        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

        


        private Rectangle[] getDotsFromImage(Image<Bgr, byte> grayframe)
        {
            Bitmap xmap = grayframe.ToBitmap();

            int firstX = -1;
            int firstY = -1;

            int lastX = -1;
            int lastY = -1;
            int colorRange = 40;
            for (int i = 0; i < xmap.Width; i += 20)
            {
                for (int j = 0; j < xmap.Height; j += 20)
                {
                    if (xmap.GetPixel(i, j).B < colorRange && xmap.GetPixel(i, j).G < colorRange && xmap.GetPixel(i, j).R < colorRange)
                    {
                        if (firstX == -1)
                        {
                            firstX = i;
                            firstY = j;
                        }
                        lastX = i;
                        lastY = j;


                    }

                }


            }
            int width = lastX - firstX;
            int heigth = lastY - firstY;
            Rectangle r = new Rectangle(firstX, firstY, width, heigth);
            Rectangle[] rList = new Rectangle[1];
            rList[0] = r;
            return rList;
        }



       

        private void processFrame(object sender, EventArgs e)
        {

            IMG = capture.QueryFrame();
            IMG_Post = IMG.CopyBlank();
            
            
            R_frame = IMG[2].Copy();
            G_frame = IMG[1].Copy();
            B_frame = IMG[0].Copy();            
            //GrayImg = IMG.Convert<Gray, Byte>();

            
            //IMG.Draw(new Rectangle(50,50,200,200),new Bgr(200,200,200),3);
            
               
            

            try
            {
                
                //imageBox1.Image = GrayImg;
                imageBox2.Image = IMG;
                imageBox3.Image = R_frame;
                imageBox4.Image = G_frame;
                imageBox5.Image = B_frame;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

        private void button1_Click(object sender, EventArgs e)
        {

            _serialPort.Open();

            timer1.Start();
            Application.Idle += processFrame;
            button1.Enabled = false;
            button2.Enabled = true;
        }
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
//(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button2_Click(object sender, EventArgs e)
        {

            _serialPort.Close();
            Application.Idle -= processFrame;
            timer1.Stop();
            button1.Enabled = true;
            button2.Enabled = false;
        }
        Rectangle rectPos;
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            try
            {
               
                int dots = 0;
                var nextFrame = capture.QueryFrame();

                //imageBox1.Image = nextFrame;
                if (nextFrame != null)
                {
                    var grayframe = nextFrame;

                    Bgr lower = new Bgr(0, 0, 0);
                    Bgr upper = new Bgr(30, 30, 30);


                    Image<Gray, byte> mask = grayframe.InRange(lower, upper).Not();
                    grayframe.SetValue(new Bgr(255, 255, 255), mask);
                    //Console.WriteLine(IMG.Width);
                    grayframe.SmoothGaussian(15);

                    var points = getDotsFromImage(grayframe);

                    foreach (Rectangle rect in points)
                    {
                        grayframe.Draw(rect, new Bgr(0, 0, 250), 3);
                        dots++;
                        rectPos = rect;
                    }

                    int width = capture.Width;
                    int height = capture.Height;
                    // Getting Degrees From Pixel
                    int diffX = (width/2)-((rectPos.X )+ rectPos.Width / 2);
                    int diffY = (height/2)-((rectPos.Y )+ rectPos.Height / 2);

                    string gelenVeri = "0";
                    if (rectPos.X != -1 && rectPos.Y != -1)//if N>0 there is an object in the image
                    {

                        Xpx = ((rectPos.X) + rectPos.Width / 2);  //X center point of the foreground object
                        Ypx = ((rectPos.Y) + rectPos.Height / 2);


                            Rectangle obje = points[0];
                            yatayX = obje.X + obje.Width/2;
                            dikeyY = obje.Y - obje.Height/2;

                            int midX = width / 2;
                            int midY = height / 2;
                        if (Math.Abs(midX-yatayX)>60 ||Math.Abs(dikeyY-midY)>60) {
                            if (yatayX < midX && dikeyY < midY)
                            {
                                //g.DrawString("1.bölge" + yatayX.ToString() + "X" + dikeyY.ToString() + "Y", new Font("Italic", 20), Brushes.White, new PointF(yatayX, dikeyY));
                                gelenVeri = "1";
                                //_serialPort.Write("1");
                            }

                            else if (yatayX > midX && dikeyY < midY)
                            {
                                //g.DrawString("3.bölge" + yatayX.ToString() + "X" + dikeyY.ToString() + "Y", new Font("Italic", 20), Brushes.White, new PointF(yatayX, dikeyY));
                                //_serialPort.Write("2");
                                gelenVeri = "2";
                            }

                            else if (yatayX < midX && dikeyY > midY)
                            {
                                //g.DrawString("4.bölge" + yatayX.ToString() + "X" + dikeyY.ToString() + "Y", new Font("Italic", 20), Brushes.White, new PointF(yatayX, dikeyY));
                                //_serialPort.Write("3");
                                gelenVeri = "3";

                            }
                            else if (yatayX > midX && dikeyY > midY)
                            {
                                //g.DrawString("6.bölge" + yatayX.ToString() + "X" + dikeyY.ToString() + "Y", new Font("Italic", 20), Brushes.White, new PointF(yatayX, dikeyY));
                                //_serialPort.Write("4");
                                gelenVeri = "4";

                            }

                            
                        }
                        else
                        {
                            Console.WriteLine("Obje Merkezde");
                            gelenVeri = "7";
                        }


                    }
                    else
                    {
                        Console.WriteLine("Obje Yok");
                        gelenVeri = "0";

                    }
                    Console.WriteLine($"Giden Pos: {gelenVeri}");
                    _serialPort.Write(gelenVeri);








                    imageBox1.Image = nextFrame;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Bilgi");
                timer1.Stop();
            }
        }

        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        private void button3_Click(object sender, EventArgs e)
        {
            IMG.Save("Image" +  ".jpg");
        }

        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)
        //(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)(*)

    }
}
