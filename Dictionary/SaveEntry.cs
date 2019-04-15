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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dictionary
{
    public partial class SaveEntry : Form
    {
        private Form1 mainForm = null;
        RadioButton rb;

        public SaveEntry(Form callingForm, string key)
        {
            InitializeComponent();
            mainForm = callingForm as Form1;
            this.textBox2.Text = "'" + key + "'";

            if (mainForm.encodingWith == Encoding.UTF8)
            {
                this.rb = this.radioButton1;
                this.radioButton1.Checked = true;
            }
            else if (mainForm.encodingWith == Encoding.Unicode)
            {
                this.rb = this.radioButton2;
                this.radioButton2.Checked = true;
            }
            else if (mainForm.encodingWith == Encoding.ASCII)
            {
                this.rb = this.radioButton3;
                this.radioButton3.Checked = true;
            }

            if (mainForm.withPassword)
                this.checkBox1.Checked = true;

            if (mainForm.isCompressed)
                this.checkBox2.Checked = true;
        }

        private void AllRadioButtons_CheckedChanged(Object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                this.rb = (RadioButton)sender;

                if (this.rb.Text == "UTF8")
                    mainForm.encodingWith = Encoding.UTF8;
                else if (this.rb.Text == "Unicode")
                    mainForm.encodingWith = Encoding.Unicode;
                else if (this.rb.Text == "ASCII")
                    mainForm.encodingWith = Encoding.ASCII;
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            mainForm.SaveClicked = true;

            if (this.checkBox1.CheckState == CheckState.Checked)
            {
                mainForm.withPassword = true;
                mainForm.EncryptionValuePasswd = this.textBox1.Text;
            }
            else
            {
                mainForm.withPassword = false;
                mainForm.EncryptionValuePasswd = "";
            }

            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                mainForm.isCompressed = true;
            }
            else
            {
                mainForm.isCompressed = false;
            }

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            mainForm.SaveClicked = false;
            mainForm.EncryptionValuePasswd = "";
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                mainForm.EncryptionValuePasswd = this.textBox1.Text;
                this.Close();
            }
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            this.textBox1.UseSystemPasswordChar = false;
        }

        private void button2_MouseUp(object sender, MouseEventArgs e)
        {
            this.textBox1.UseSystemPasswordChar = true;
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                this.textBox1.UseSystemPasswordChar = false;
        }

        private void button2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                this.textBox1.UseSystemPasswordChar = true;
        }

        private void SaveEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                mainForm.SaveClicked = false;
                mainForm.EncryptionValuePasswd = "";
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                mainForm.SaveClicked = true;
                mainForm.EncryptionValuePasswd = this.textBox1.Text;
                this.Close();
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.CheckState == CheckState.Checked)
            {
                this.label1.Enabled = true;
                this.textBox1.Enabled = true;
                this.button2.Enabled = true;
                mainForm.withPassword = true;
            }
            else
            {
                this.label1.Enabled = false;
                this.textBox1.Enabled = false;
                this.button2.Enabled = false;
                mainForm.withPassword = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                mainForm.isCompressed = true;
            }
            else
            {
                mainForm.isCompressed = false;
            }
        }

    }
}
