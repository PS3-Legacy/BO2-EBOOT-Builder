using System;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace BO_II_EBOOT_Builder
{
    public partial class frmMain : Form
    {
        string tmp = Path.GetTempPath() + "BO2EB";
        const string make = "make.exe";
        const string bo2 = "BO2.elf";
        BinaryWriter bn;
        SaveFileDialog sv;
        
         struct Offset
        {
            public static uint NoRecoil = 0xE9E54,
                PrbationByPass = 0x52FC6C,
                VSAT = 0x23C60,
                WallHack = 0x734D0,
                Laser = 0xDF68F, // On 01
                SteadyAim = 0x5E0A23, //On 00 off 02
                RedBox1 = 0x683E0,
                RedBox2 = 0x68604,
                AutoStartOn = 0x4525F0, // 40 ZM // 41 MP
                UAV = 0x33CB7, // On 01 off 00
                EndProbation = 0x96012C, // 0x00
                FPS2 = 0x27FEC, // On 60 00 00 00
                FPS1 = 0x8D3590,// 0x94, 50, 0, 0, 0, 0, 0, 32, 32, 32, 32, 32, 32 
                AntiFreeze1 = 0x66B824,
                FPSText = 0x8D3290,
                AntiFreeze2 = 0x66B798;
            public static uint WallHack2 = 0x734D0;

            public struct Bytes
            {
                public static byte[] NOPE = { 0x60, 0x00, 0x00, 0x00 };
            }

        }
        void ExecCmd(string file, string arg)
        {
            Process pro = new Process();
            pro.StartInfo.FileName = file;
            pro.StartInfo.WorkingDirectory = tmp;
            pro.StartInfo.Arguments = arg;
            pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pro.StartInfo.CreateNoWindow = true;
            pro.StartInfo.UseShellExecute = false;
            pro.Start();
            pro.WaitForExit();
        }
        void PatchOffset(uint Offset, byte[] value)
        {
            bn.Seek((int)Offset, SeekOrigin.Begin);
            bn.Write(value);
            bn.Flush();
            Application.DoEvents();
        }
        void PatchOffset(uint Offset, byte value)
        {
            bn.Seek((int)Offset, SeekOrigin.Begin);
            bn.Write(value);
            bn.Flush();
            Application.DoEvents();
        }
        void LoadFiles(string path,byte[] fileBytes)
        {
            File.WriteAllBytes(path, fileBytes);
        }
        public frmMain()
        {      
            InitializeComponent();           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(tmp))
            {
                Directory.CreateDirectory(tmp);
            }
            tmp += "\\";
            if (!File.Exists(tmp + make))
            {
                LoadFiles(tmp + make, Properties.Resources.make);
            }
            if (!File.Exists(tmp + bo2))
            {
                LoadFiles(tmp + bo2, Properties.Resources.bo2);
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Directory.Delete(tmp, true);
        }

        private void fpsCh_CheckedChanged(object sender)
        {
            if (fpsCh.Checked)
                fpsBox.Visible = true;
            else
                fpsBox.Visible = false;
        }
        private void buildBtn_Click(object sender, EventArgs e)
        {
            try
            {
                bn = new BinaryWriter(File.Open(tmp + bo2, FileMode.Open, FileAccess.Write));
                if (vsatCh.Checked)
                {
                    PatchOffset(Offset.VSAT, Offset.Bytes.NOPE);
                }
                if (uavCh.Checked)
                {
                    PatchOffset(Offset.UAV, new byte[] { 0x01 });
                }
                if (saCh.Checked)
                {
                    PatchOffset(Offset.SteadyAim, new byte[] { 0x00 });
                }
                if (nrCh.Checked)
                {
                    PatchOffset(Offset.NoRecoil, Offset.Bytes.NOPE);
                }
                if (rbCh.Checked)
                {
                    PatchOffset(Offset.RedBox1, new byte[] { 0x38, 0x60, 0x00, 0x01 });
                    PatchOffset(Offset.RedBox2, Offset.Bytes.NOPE);
                }
                if (laserCh.Checked)
                {
                    PatchOffset(Offset.Laser, new byte[] { 0x01 });
                }
                if (whCh.Checked)
                {
                    PatchOffset(Offset.WallHack2, new byte[] { 0x38, 0xC0, 0xFF, 0xFF });
                }
                if (antifreezeCh.Checked)
                {
                    PatchOffset(Offset.AntiFreeze1, Offset.Bytes.NOPE);
                    PatchOffset(Offset.AntiFreeze2, Offset.Bytes.NOPE);
                }
                if (apCh.Checked)
                {
                    PatchOffset(Offset.PrbationByPass, new byte[] { 0x41, 0x80 });
                    PatchOffset(Offset.EndProbation, new byte[] { 0x00, 0x00 });
                }
                if (antiBanCh.Checked)
                {
                    byte[] anti = new byte[4];
                    anti[0] = 0x60;
                    PatchOffset(0x4b8310, new byte[] { 0x40, 0x00 });
                    PatchOffset(0x50a38f, new byte[] { 0x99 });
                    PatchOffset(0x50ba74, anti);
                    PatchOffset(0x547dd4, anti);
                    PatchOffset(0x548148, anti);
                    PatchOffset(0x50b618, new byte[] { 0x48, 0x00 });
                    PatchOffset(0x50a3bc, new byte[] { 0x48, 0x80 });
                    PatchOffset(0x5300e8, anti);
                    PatchOffset(0x5300f4, anti);
                }
                if (fpsCh.Checked)
                {
                    PatchOffset(Offset.FPS1, new byte[] { 0x94, 50, 0, 0, 0, 0, 0, 32, 32, 32, 32, 32, 32 });
                    PatchOffset(Offset.FPS2, Offset.Bytes.NOPE);
                    byte[] text = Encoding.UTF8.GetBytes(fpsBox.Text + "\0");
                    PatchOffset(Offset.FPSText, text);
                }
                byte[] startGame = new byte[] { 0x41, 0x40 };
                if (startCombo.SelectedIndex < 0)
                {
                    startCombo.SelectedIndex = 0;
                }
                PatchOffset(Offset.AutoStartOn, startGame[startCombo.SelectedIndex]);
                bn.Close();
                sv = new SaveFileDialog();
                sv.Filter = "BIN|*.BIN";
                if (sv.ShowDialog() == DialogResult.OK)
                {
                    ExecCmd(tmp + make, tmp + bo2 + " " + sv.FileName);
                }
                MessageBox.Show(String.Format("EBOOT Has Ben Successfully Created\nFile Size {0}\nFile Location {1}\nFile Extension {2}", FormatBytes(File.ReadAllBytes(sv.FileName).Length, 1, true), sv.FileName, Path.GetExtension(sv.FileName)));
                File.Delete(tmp + bo2);
                LoadFiles(tmp + bo2, Properties.Resources.bo2);
            }
            catch { }
        }
        private string FormatBytes(long bytes, int decimalPlaces, bool showByteType)
        {
            double newBytes = bytes;
            string formatString = "{0";
            string byteType = " B";

            if (newBytes > 1024 && newBytes < 1048576)
            {
                newBytes /= 1024;
                byteType = "KB";
            }
            else if (newBytes > 1048576 && newBytes < 1073741824)
            {
                newBytes /= 1048576;
                byteType = " MB";
            }
            else
            {
                newBytes /= 1073741824;
                byteType = " GB";
            }
            if (decimalPlaces > 0)
                formatString += ":0.";
            for (int i = 0; i < decimalPlaces; i++)
                formatString += "0";
            formatString += "}";
            if (showByteType)
                formatString += byteType;

            return string.Format(formatString, newBytes);
        }
    }
}
