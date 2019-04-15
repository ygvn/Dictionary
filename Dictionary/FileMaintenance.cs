/*
 * Copyright (c) 2019 Yucel Guven
 * All rights reserved.
 * 
 * This file is part of Dictionary/Notepad Application.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted (subject to the limitations in the
 * disclaimer below) provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * NO EXPRESS OR IMPLIED LICENSES TO ANY PARTY'S PATENT RIGHTS ARE
 * GRANTED BY THIS LICENSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS 
 * AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
 * BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY,
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF 
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dictionary
{
    public partial class FileMaintenance : Form
    {
        Form1 mainForm = null;
        int Count = 0, Deleted = 0;

        long BeforeFilesize = 0, NowFileSize = 0;

        public FileMaintenance(Form callingForm)
        {
            InitializeComponent();
            //
            this.mainForm = callingForm as Form1;

            this.progressBar1.Value = 0;
            this.backgroundWorker1.RunWorkerAsync();
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            using (FileStream fs = File.OpenRead(mainForm.filename))
            using (BinaryReader reader = new BinaryReader(fs))
            using (FileStream fsn = File.Create(mainForm.filename + ".tmpnew"))
            using (BinaryWriter writer = new BinaryWriter(fsn))
            {
                byte bdel = 0;
                int Total = 0;

                this.BeforeFilesize = fs.Length;

                while (reader.BaseStream.Position != fs.Length)
                {
                    bdel = reader.ReadByte();
                    Total = reader.ReadInt32();
                    
                    if (bdel == 0)
                    {
                        writer.Write(bdel);
                        writer.Write(Total);
                        writer.Write(reader.ReadBytes(Total));
                        //
                    }
                    else // pass, deleted
                    {
                        fs.Seek(Total, SeekOrigin.Current);
                        this.Deleted++;
                    }
                    Count++;
                    this.backgroundWorker1.ReportProgress((int)(( (fs.Position*100) / fs.Length)));
                }
                this.NowFileSize = fsn.Length;

            }

            this.backgroundWorker1.ReportProgress(100);
        }

        void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.label2.Text = e.ProgressPercentage.ToString() + " %";
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (File.Exists(mainForm.filename) && File.Exists(mainForm.filename + ".tmpnew"))
            {
                File.Delete(mainForm.filename);
                File.Move(mainForm.filename + ".tmpnew", mainForm.filename);
            }

            MessageBox.Show("Number of Entries: \r\n"
                          + "-------------------- \r\n"
                          + "Before   \t: " + this.Count + "\r\n"
                          + "Deleted  \t: " + this.Deleted + "\r\n"
                          + "NewTotal \t: " + (this.Count - this.Deleted).ToString()
                          + "\r\n\r\n"
                          + "File Size: \r\n"
                          + "--------- \r\n"
                          + "Before   \t: " + this.BeforeFilesize.ToString() + " Bytes" + "\r\n"
                          + "Deleted  \t: " + (this.BeforeFilesize - this.NowFileSize).ToString() + " Bytes" + "\r\n"
                          + "New Size \t: " + this.NowFileSize.ToString() + " Bytes",
                          "File Maintenance Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            this.backgroundWorker1.CancelAsync();
            this.Close();
        }

    }
}
