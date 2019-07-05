using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using WindowsInput.Native;
using WindowsInput;
using System.Windows.Input;
using MouseSimulation;

namespace MouseSimulation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            timer1.Interval = 10;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        Random rand = new Random();

        public InputSimulator InputSimulation = new InputSimulator();

        private void button1_Click(object sender, EventArgs e)
        {
            string[] s = textBox1.Text.ToString().Split(' ');

            new Thread(() => MoveMouse(Convert.ToInt32(s[0]), Convert.ToInt32(s[1]), (int)numericUpDown1.Value)).Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = Cursor.Position.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Start")
            {
                button2.Text = "Stop";
                timer1.Start();
            }
            else if (button2.Text == "Stop")
            {
                button2.Text = "Start";
                timer1.Stop();
            }
        }

        public static void MoveMouse(int ToX, int ToY, int Pause)
        {
            Point Move = Cursor.Position;

            double MoveX = Move.X;
            double MoveY = Move.Y;
            double dX = ToX - MoveX;
            double dY = ToY - MoveY;

            double R = Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
            double RandomR = new Random().Next(20, 35) / 10;

            double a = Math.Acos(dX / R);
            double b = Math.Acos(1 / (RandomR * 2));

            double ang = a + b;
            if (dY < 0) ang = b - a;

            double oX = (Math.Cos(ang) * R * RandomR) + MoveX;
            double oY = (Math.Sin(ang) * R * RandomR) + MoveY;

            int PauseTick = 0;

            while (true)
            {
                Move = Cursor.Position;
                if (Math.Abs(ToX - Move.X) < 3 && Math.Abs(ToY - Move.Y) < 3)
                {
                    break;
                }
                if (Math.Abs(Move.X - MoveX) > 1 && Math.Abs(Move.Y - MoveY) > 1)
                {
                    MoveX = Move.X;
                    MoveY = Move.Y;
                    dX = ToX - MoveX;
                    dY = ToY - MoveY;

                    R = Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));
                    RandomR = new Random().Next(20, 35) / 10;

                    a = Math.Acos(dX / R);
                    b = Math.Acos(1 / (RandomR * 2));

                    ang = a + b;
                    if (dY < 0) ang = b - a;

                    oX = (Math.Cos(ang) * R * RandomR) + MoveX;
                    oY = (Math.Sin(ang) * R * RandomR) + MoveY;
                }

                if (Math.Abs(dX) > Math.Abs(dY))
                {
                    if (ToX > MoveX)
                    {   
                        MoveX++; //(1 is better?)
                        MoveY = oY - Math.Sqrt(Math.Pow(R * RandomR, 2) - Math.Pow(MoveX - oX, 2));
                    }
                    else
                    {
                        MoveX--;
                        MoveY = Math.Sqrt(Math.Pow(R * RandomR, 2) - Math.Pow(MoveX - oX, 2)) + oY;
                    }
                }
                else
                {
                    if (ToY > MoveY)
                    {
                        MoveY++;
                        MoveX = Math.Sqrt(Math.Pow(R * RandomR, 2) - Math.Pow(MoveY - oY, 2)) + oX;
                    }
                    else
                    {
                        MoveY--;
                        MoveX = oX - Math.Sqrt(Math.Pow(R * RandomR, 2) - Math.Pow(MoveY - oY, 2));
                    }
                }
                //Cursor.Position = new Point((int)Math.Round(MoveX), (int)Math.Round(MoveY));
                new InputSimulator().Mouse.MoveMouseTo((65535 * Math.Round(MoveX)) / Screen.PrimaryScreen.Bounds.Width, (65535 * Math.Round(MoveY)) / Screen.PrimaryScreen.Bounds.Height);

                PauseTick++;
                if (PauseTick == Pause)
                {
                    Thread.Sleep(1);
                    PauseTick = 0;
                }
            }
        }

        public static void SimulateTypingText(string Text, int typingDelay = 100, int startDelay = 0)
        {
            new InputSimulator().Keyboard.Sleep(startDelay);
            
            string[] lines = Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            foreach (string line in lines)
            {
                char[] words = line.ToCharArray();
                VirtualKeyCode VKey;
                foreach (char word in words)
                {
                    Thread.Sleep(2000);
                    bool ShiftOn = false;
                    switch (word)
                    {
                        case '@':
                            ShiftOn = true;
                            VKey = VirtualKeyCode.VK_2;
                            break;
                        case '.':
                            VKey = VirtualKeyCode.OEM_PERIOD;
                            break;
                        default:
                            if (Char.IsUpper(word))
                                ShiftOn = true;
                            VKey = (VirtualKeyCode)char.ToUpper(word);
                            break;
                    }
                    Thread.Sleep(typingDelay + new Random().Next(-10, 100));
                    if (ShiftOn)
                    {
                        new InputSimulator().Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
                        Thread.Sleep(typingDelay + new Random().Next(typingDelay * 2, typingDelay * 5));
                        new InputSimulator().Keyboard.KeyDown(VKey);
                        Thread.Sleep(typingDelay/2 + new Random().Next(0, 50));
                        new InputSimulator().Keyboard.KeyUp(VKey);
                        Thread.Sleep(typingDelay/2 + new Random().Next(0, 30));
                        new InputSimulator().Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
                    }
                    else
                    {
                        new InputSimulator().Keyboard.KeyDown(VKey);
                        Thread.Sleep(typingDelay/2 + new Random().Next(0, 50));
                        new InputSimulator().Keyboard.KeyUp(VKey);
                    }
                    
                }
            }
        }

        public static void Ctrl_C(int typingDelay = 100, int startDelay = 0)
        {
            new InputSimulator().Keyboard.Sleep(startDelay);

            new InputSimulator().Keyboard.KeyDown(VirtualKeyCode.LCONTROL);
            Thread.Sleep(typingDelay + new Random().Next(typingDelay * 2, typingDelay * 5));
            new InputSimulator().Keyboard.KeyDown(VirtualKeyCode.VK_C);
            Thread.Sleep(typingDelay / 2 + new Random().Next(0, 50));
            new InputSimulator().Keyboard.KeyUp(VirtualKeyCode.VK_C);
            Thread.Sleep(typingDelay / 2 + new Random().Next(0, 30));
            new InputSimulator().Keyboard.KeyUp(VirtualKeyCode.LCONTROL);
        }

        public static void MouseLeftClick(int typingDelay)
        {
            Thread.Sleep(typingDelay + new Random().Next(100, 700));
            new InputSimulator().Mouse.LeftButtonDown();
            Thread.Sleep(typingDelay + new Random().Next(-30, 100));
            new InputSimulator().Mouse.LeftButtonUp();
        }

        public static void MouseRightClick(int typingDelay)
        {
            Thread.Sleep(typingDelay + new Random().Next(100, 700));
            new InputSimulator().Mouse.RightButtonDown();
            Thread.Sleep(typingDelay + new Random().Next(-30, 100));
            new InputSimulator().Mouse.RightButtonUp();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SimulateTypingText(textBox2.Text, 100, 2000);
        }
        
    }

}