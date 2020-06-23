﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using EnrageR.Helpers;
using EnrageR.Models;
using System.Diagnostics;
using FMUtils.KeyboardHook;

namespace EnrageR
{
    public partial class Form1 : Form
    {
        private Player Player;
        private Hook KeyboardHook;
        private GTA Gta;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Gta = new GTA();
            Player = new Player(Gta);
            foreach (var scrn in Screen.AllScreens)
                if (scrn.Bounds.Contains(this.Location)) this.Location = new Point(scrn.Bounds.Right - this.Width, scrn.Bounds.Top);
            WindowState = FormWindowState.Minimized;

            KeyboardHook = new Hook("Keyboard hook");
            KeyboardHook.KeyUpEvent += KeyPress;

            MarqueeTimer.Start();

            foreach (var location in Models.Location.GetLocations())
                TeleportBox.Items.Add(location.Name);
            TeleportBox.SelectedIndex = 0;
        }

        private void KeyPress(KeyboardHookEventArgs e)
        {
            if (e.isAltPressed)
            {
                Trace.WriteLine("Healing...");
                Player.SetHealth(200);
                return;
            }

            if (e.isRCtrlPressed)
            {
                var loc = Models.Location.GetLocations().FirstOrDefault(l => l.Name.ToString().Equals(TeleportBox.Text));
                if (loc == null) return;
                Player.SetLocation(loc);
            }

            if (e.Key == Keys.F10)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                    this.Activate();
                }
                else WindowState = FormWindowState.Minimized;
            }
        }

        private int xPos = -196;
        private void MarqueeTimer_Tick(object sender, EventArgs e)
        {
            if (xPos >= this.Width)
            {
                this.MarqueeLabel.Location = new System.Drawing.Point(-196, 460);
                xPos = -196;
                return;
            }
            this.MarqueeLabel.Location = new System.Drawing.Point(xPos, 460);
            xPos++;
        }

        private void ExitImage_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show("Are you sure?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (res.ToString() == "Yes")
            {
                Environment.Exit(0);
            }
        }


        /// <summary>
        /// Mouse drag moves form
        /// </summary>
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void TeleportButton_Click(object sender, EventArgs e)
        {
            var loccc = Player.GetLocation();
            var loc = Models.Location.GetLocations().FirstOrDefault(l => l.Name.ToString().Equals(TeleportBox.Text));
            if (loc == null) return;
            Player.SetLocation(loc);

            WindowState = FormWindowState.Minimized;
            Thread.Sleep(50);
            new Keyboard.Keyboard().Send(Keyboard.Keyboard.ScanCodeShort.LCONTROL);
        }

        private void OnHover(object sender, EventArgs e)
        {
            if (!CloseTimer.Enabled) CloseTimer.Start();
            sec = 0;
            for (double i = Opacity * 100; i <= 100; i++)
            {
                Opacity = i / 100.0;
                Thread.Sleep(1);
            }
        }

        private void OnLeave()
        {
            sec = 0;
            for (double i = Opacity * 100; i >= 50; i--)
            {
                Opacity = i / 100.0;
                Thread.Sleep(5);
            }
        }
        int sec = 0;
        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            sec++;
            if (sec >= 10) OnLeave();
        }

        private void GodModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (GodModeCheckBox.Checked) Player.EnableAutoHeal(Convert.ToInt32(AutoHealTextbox.Text));
            else Player.DisableAutoHeal();
        }
    }
}