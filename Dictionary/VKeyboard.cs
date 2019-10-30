/*
 * Copyright (c) 2019-2020 Yucel Guven
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
using System.Linq;
using System.Windows.Forms;

namespace Dictionary
{
    public partial class VKeyboard : Form
    {
        /* Virtual Keyboard */

        Form1 mainForm = null;
        List<int> thelist = Enumerable.Range(0, 35).ToList();

        public VKeyboard(Form callingForm)
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;

            this.mainForm = callingForm as Form1;
        }

        public static Dictionary<int, char> kbGreek = new Dictionary<int, char>()
            {
                {0,';'}, {1,'ς'}, {2,'ε'},{3,'ρ'},{4,'τ'},{5,'υ'},{6,'θ'},{7,'ι'},{8,'ο'},{9,'π'}, {10,'['}, {11,']'},
                {12,'α'},{13,'σ'},{14,'δ'},{15,'φ'},{16,'γ'},{17,'η'},{18,'ξ'},{19,'κ'}, {20,'λ'}, {21,'΄'}, {22,'\''},{23,'\\'},{24,'ζ'},
                {25,'χ'},{26,'ψ'},{27,'ω'},{28,'β'},{29,'ν'}, {30,'μ'}, {31,','}, {32,'.'},{33,'/'}, {34,'`'}
            };

        public static Dictionary<int, char> kbRuss = new Dictionary<int, char>()
            {
                {0,'й'}, {1,'ц'}, {2,'у'},{3,'к'},{4,'е'},{5,'н'},{6,'г'},{7,'ш'},{8,'щ'},{9,'з'}, {10,'х'}, {11,'ъ'},
                {12,'ф'},{13,'ы'},{14,'в'},{15,'а'},{16,'п'},{17,'р'},{18,'о'},{19,'л'}, {20,'д'}, {21,'ж'}, {22,'э'},{23,'\\'},{24,'я'},
                {25,'ч'},{26,'с'},{27,'м'},{28,'и'},{29,'т'}, {30,'ь'}, {31,'б'}, {32,'ю'},{33,'.'}, {34,'ё'}
            };
        public static Dictionary<int, char> kbTurk = new Dictionary<int, char>()
            {
                {0,'q'}, {1,'w'}, {2,'e'},{3,'r'},{4,'t'},{5,'y'},{6,'u'},{7,'ı'},{8,'o'},{9,'p'}, {10,'ğ'}, {11,'ü'},
                {12,'a'},{13,'s'},{14,'d'},{15,'f'},{16,'g'},{17,'h'},{18,'j'},{19,'k'}, {20,'l'}, {21,'ş'}, {22,'i'},{23,','},{24,'z'},
                {25,'x'},{26,'c'},{27,'v'},{28,'b'},{29,'n'}, {30,'m'}, {31,'ö'}, {32,'ç'},{33,'.'}, {34,'"'}
            };

        private void AllButtons_Clicked(Object sender, EventArgs e)
        {
            this.mainForm.richTextBox1.SelectionLength = 0;
            
            if ((Button)sender != this.button36)
                this.mainForm.richTextBox1.SelectedText = ((Button)sender).Text;
            else
                this.mainForm.richTextBox1.SelectedText = " ";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var btns = Controls.OfType<Button>().Reverse();

            if (this.checkBox1.CheckState == CheckState.Checked)
            {
                foreach (var b in btns)
                {
                    if (b == this.button37)
                        continue;
                    b.Text = b.Text.ToUpper();
                }
            }
            else
            {
                foreach (var b in btns)
                {
                    if (b == this.button37)
                        continue;
                    b.Text = b.Text.ToLower();
                }
            }
        }

        private void VKeyboards_Load(object sender, EventArgs e)
        {
            DefaultChars();
        }

        private void DefaultChars()
        {
            // Mostly used symbols: / En fazla kullanilan/bilinen semboller:
            // this.richTextBox1.Text = " √ ω φ π → ε ≠ © ® ± £ ≥ ≤ ∞ σ € ÷ ";
            // Crillic: [1040-1104]
            
            for (int i = 900; i <= 1125; i++)
                this.richTextBox1.Text += ((char)i).ToString() + " ";
            this.richTextBox1.Text += Environment.NewLine;
        }

        private void button37_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";

            if (this.textBox2.Text.Trim() == "" || this.textBox3.Text.Trim() == "")
            {
                DefaultChars();
                return;
            }

            int from, to;
            if (!int.TryParse(this.textBox2.Text, out from))
                return;
            if (!int.TryParse(this.textBox3.Text, out to))
                return;
            if (to < from)
                return;

            for (int i = from; i <= to; i++)
                this.richTextBox1.Text += ((char)i).ToString() + " ";
        }

        private void VKeyboards_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var btns = Controls.OfType<Button>().Reverse();

            int k = 0;

            if (this.comboBox1.SelectedItem.ToString() == "Greek")
            {
                foreach (var b in btns)
                {
                    if (this.checkBox1.CheckState == CheckState.Unchecked)
                        b.Text = kbGreek[thelist[k]].ToString();
                    else if (this.checkBox1.CheckState == CheckState.Checked)
                        b.Text = kbGreek[thelist[k]].ToString().ToUpper();
                    k++;
                    if (k == 35)
                        break;
                }
            }
            else if (this.comboBox1.SelectedItem.ToString() == "Russian")
            {
                foreach (var b in btns)
                {
                    if (this.checkBox1.CheckState == CheckState.Unchecked)
                        b.Text = kbRuss[thelist[k]].ToString();
                    else if (this.checkBox1.CheckState == CheckState.Checked)
                        b.Text = kbRuss[thelist[k]].ToString().ToUpper();
                    k++;
                    if (k == 35)
                        break;
                }
            }
            else if (this.comboBox1.SelectedItem.ToString() == "Turkish")
            {
                foreach (var b in btns)
                {
                    if (this.checkBox1.CheckState == CheckState.Unchecked)
                        b.Text = kbTurk[thelist[k]].ToString();
                    else if (this.checkBox1.CheckState == CheckState.Checked)
                        b.Text = kbTurk[thelist[k]].ToString().ToUpper();
                    k++;
                    if (k == 35)
                        break;
                }
            }

        }

    }
}
