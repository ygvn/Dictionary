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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;

namespace Dictionary
{
    public partial class Form1 : Form
    {
        #region special variables  -yucel

        public string filename = "MyDictionary.bin";          /* Default file name.    Change with your favorite */
        public string keysFile = "MyDictionary.bin.keys";     /* Default keyfile name. Change with your favorite */

        int count = 0;
        bool vfound = false;
        public string[] keyitem = new string[2] { "", "" };   /* [0]:old, [1]:new */
        Dictionary<string, string> dict = new Dictionary<string, string>();
        Dictionary<string, long> keylist = new Dictionary<string, long>();
        
        public string EncryptionValuePasswd = "";             /* User-Entered Password */
        string EncryptionPasswdDefault = "default123";        /* Default Password. Change with your favorite */
        
        Font cf = null;
        Color ct, cb;
        int EntrySize = 0;
        string selectedKey = "";
        byte KeyFirstByte = 0;
        byte[] backRGB = new byte[] { 0xff, 0xff, 0xff };     /* Default white background */
        byte[] tagKey = new byte[4];
        public Encoding encodingWith = Encoding.UTF8;         /* Default */
        public bool SaveClicked = false;
        public bool withPassword = false;
        public bool isCompressed = false;
        string pw = "";
        int rtbCursor = 0;
        string CopyOfTxt = "", CopyOfRtf = "";
        DateTimePicker dateTimePicker1 = new DateTimePicker();
        int oldIndex = -1;
        #endregion

        public Form1()
        {
            InitializeComponent();

            cf = richTextBox1.Font;
            ct = richTextBox1.ForeColor;
            cb = richTextBox1.BackColor;

            this.richTextBox1.AllowDrop = true;
            this.richTextBox1.DragEnter += new DragEventHandler(this.richTextBox1_DragEnter);
            this.richTextBox1.DragDrop += new DragEventHandler(this.richTextBox1_DragDrop);

            this.toolStripStatusLabel2.Text = "File: " + this.filename;
        }

        private void OpenFile()
        {
            this.keylist.Clear();
            this.listBox1.Items.Clear();
            this.richTextBox1.Clear();
            this.richTextBox1.Rtf = "";
            this.CopyOfTxt = ""; this.CopyOfRtf = "";

            if (File.Exists(filename))
            {
                if (File.Exists(keysFile))
                {
                    this.count = ReadKeysFromFile();
                    
                    this.listBox1.Items.AddRange(keylist.Keys.Reverse().ToArray());
                    UpdateStatus();
                    return;
                }
                else
                {
                    MessageBox.Show("Could not find file: '" + keysFile + "'" + Environment.NewLine +
                        Environment.NewLine + "A new file will be created.",
                        "Keys File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    KeysAddressFromDictionary();

                    this.listBox1.Items.AddRange(keylist.Keys.Reverse().ToArray());
                    UpdateStatus();

                    return;
                }
            }
            else
            {
                MessageBox.Show("Could not find file: '" + filename + "'" + Environment.NewLine +
                    Environment.NewLine + "An empty file will be created.",
                    "Dictionary File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (FileStream fs = File.Create(filename)) { };

                using (FileStream fsk = File.Create(keysFile)) { };

                /* testEntry: */
                string testEntry = "{\\rtf1\\ansi{\\fonttbl{\\f0\\fnil\\fcharset162 Calibri;}" +
                    "{\\f1\\fnil\\fcharset0 Calibri;}}" +
                    "\\viewkind4\\uc1\\pard\\f0\\fs23  test entry. \\par }";

                this.KeyFirstByte = EncodeKeyByte();
                this.backRGB[0] = this.richTextBox1.BackColor.R;
                this.backRGB[1] = this.richTextBox1.BackColor.G;
                this.backRGB[2] = this.richTextBox1.BackColor.B;
                this.tagKey = new byte[] { this.KeyFirstByte, this.backRGB[0], this.backRGB[1], this.backRGB[2] };

                WriteToFile("test", testEntry, false, false);
                Listele();
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            this.count = this.keylist.Count;

            this.toolStripStatusLabel1.Text = "   " +
                this.listBox1.Items.Count + "/" + this.count.ToString();
            this.toolStripStatusLabel3.Text = " ";
        }

        private void Listele()
        {
            this.listBox1.Items.Clear();
            this.listBox1.Items.AddRange(keylist.Keys.Reverse().ToArray());
            UpdateStatus();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveButton_Click(null, null);
                return;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if ((this.CopyOfRtf != this.richTextBox1.Rtf) && this.richTextBox1.Text != "")
                {
                    SaveButton_Click(null, null);
                }
                this.Close();
            }
            else if (e.KeyCode == Keys.F1)
            {
                EditingKeys edk = new EditingKeys();
                edk.Show();
            }
            else if (e.KeyCode == Keys.F6)
            {
                specialCharactersToolStripMenuItem_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.B)
            {
                boldToolStripMenuItem_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.U)
            {
                underlineToolStripMenuItem_Click(null, null);
            }
            else if (e.Control && e.KeyCode == Keys.T)
            {
                italicToolStripMenuItem_Click(null, null);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.OpenFile();
        }

        void richTextBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text)
                || e.Data.GetDataPresent(DataFormats.Rtf)
                || e.Data.GetDataPresent(DataFormats.UnicodeText)
                || e.Data.GetDataPresent(DataFormats.OemText)
                || e.Data.GetDataPresent(DataFormats.CommaSeparatedValue)
                || e.Data.GetDataPresent(DataFormats.FileDrop)
                )
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        void richTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                int i = this.richTextBox1.SelectionStart;
                String s = this.richTextBox1.Text.Substring(i) + this.richTextBox1.Rtf.Substring(i);
                this.richTextBox1.Text = this.richTextBox1.Text.Substring(0, i) + this.richTextBox1.Rtf.Substring(0, i);

                this.richTextBox1.Text = this.richTextBox1.Text + e.Data.GetData(DataFormats.Text).ToString();
                this.richTextBox1.Text = this.richTextBox1.Text + s;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                int im = 0;
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).ToLower() == ".jpg"
                        || Path.GetExtension(file).ToLower() == ".jpeg"
                        || Path.GetExtension(file).ToLower() == ".png"
                        || Path.GetExtension(file).ToLower() == ".gif"
                        || Path.GetExtension(file).ToLower() == ".bmp"
                        || Path.GetExtension(file).ToLower() == ".tiff"
                        || Path.GetExtension(file).ToLower() == ".dib"
                        )
                    {
                        Image img = default(Image);
                        img = Image.FromFile(((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(im).ToString());
                        Clipboard.SetImage(img);
                        this.richTextBox1.Paste();
                        im++;
                    }
                    else if (Path.GetExtension(file).ToLower() == ".txt")
                    {
                        int i = this.richTextBox1.SelectionStart;
                        String s = this.richTextBox1.Text.Substring(i);
                        this.richTextBox1.Text = this.richTextBox1.Text.Substring(0, i);
                        this.richTextBox1.Text += File.ReadAllText(file, Encoding.Default);
                        this.richTextBox1.Text += s;
                        this.richTextBox1.SelectionStart = this.richTextBox1.TextLength;
                    }
                }
            }
        }

        private byte EncodeKeyByte()
        {
            this.KeyFirstByte = 0;

            // with user-provided password?
            if (this.withPassword)
                this.KeyFirstByte = (byte)(this.KeyFirstByte | 1);

            // is compression checked?
            if (this.isCompressed)
                this.KeyFirstByte = (byte)(this.KeyFirstByte | 128);

            //UTF8:
            if (this.encodingWith == Encoding.UTF8)
                this.KeyFirstByte = (byte)(this.KeyFirstByte | 2);

            //Unicode:
            else if (this.encodingWith == Encoding.Unicode)
                this.KeyFirstByte = (byte)(this.KeyFirstByte | 4);

            //ASCII:
            else if (this.encodingWith == Encoding.ASCII)
                this.KeyFirstByte = (byte)(this.KeyFirstByte | 8);

            // default: UTF8
            else
                this.KeyFirstByte = (byte)(this.KeyFirstByte | 2);

            return this.KeyFirstByte;
        }

        private void GetEncodingFromKeyByte(byte B)
        {
            if ((B & 1) == 1)
            {
                pw = "Password protected";
                this.withPassword = true;
                this.toolStripStatusLabel3.Text = pw;
            }
            else
            {
                pw = "No Password";
                this.withPassword = false;
                this.toolStripStatusLabel3.Text = " ";
            }

            if ((B & 128) == 128)
            {
                this.isCompressed = true;
            }
            else
            {
                this.isCompressed = false;
            }

            if ((B & 2) == 2)
            {
                this.encodingWith = Encoding.UTF8;
                //this.toolStripStatusLabel3.Text = " UTF8, " + pw;
            }
            else if ((B & 4) == 4)
            {
                this.encodingWith = Encoding.Unicode;
                //this.toolStripStatusLabel3.Text = " Unicode, " + pw;
            }
            else if ((B & 8) == 8)
            {
                this.encodingWith = Encoding.ASCII;
                //this.toolStripStatusLabel3.Text = " ASCII, " + pw;
            }
            else
            {
                //Console.WriteLine("***UnknownEncoding");
                //this.toolStripStatusLabel3.Text = " Unknown Encoding!, " + pw;
                this.encodingWith = Encoding.UTF8; // Set variable to Default
            }
        }

        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream compressed = new MemoryStream())
            using (DeflateStream deflate = new DeflateStream(compressed, CompressionMode.Compress))
            {
                deflate.Write(input, 0, input.Length);
                deflate.Close();

                return compressed.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream compressedDataStream = new MemoryStream(data))
            using (MemoryStream decompressedStream = new MemoryStream())
            using (DeflateStream deflate = new DeflateStream(compressedDataStream, CompressionMode.Decompress))
            {
                deflate.CopyTo(decompressedStream);
                deflate.Close();
                decompressedStream.Position = 0;

                return decompressedStream.ToArray();
            }
        }

        private byte[] Encrypt(string clearText, bool b)
        {
            string EncryptionPasswd = "";

            /* If user provided/specified a password then use it as 'EncryptionPasswd',
             * otherwise use the default encryption passwdord.
             */

            if (b)
                EncryptionPasswd = this.EncryptionValuePasswd;
            else
                EncryptionPasswd = this.EncryptionPasswdDefault;

            byte[] dataBytes = null;

            if (this.isCompressed)
                dataBytes = Compress(this.encodingWith.GetBytes(clearText));
            else
                dataBytes = this.encodingWith.GetBytes(clearText);


            /* AES type encryptor */
            try
            {
                using (Aes encryptor = Aes.Create())
                {
                    /* Salt: "AES Encryption" : <14Bytes>
                     * 65,69,83,  32,  69,110,99,114,121,112, 116,105,111,110,
                     * 0x41,0x45,0x53,0x20, 0x45,0x6e,0x63,0x72,0x79,0x70, 0x74,0x69,0x6f,0x6e
                     */
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionPasswd,
                        new byte[] { 0x41, 0x45, 0x53, 0x20, 0x45, 0x6e, 0x63, 0x72,
                        0x79, 0x70, 0x74, 0x69, 0x6f, 0x6e });
                    encryptor.Key = pdb.GetBytes(32); //Key
                    encryptor.IV = pdb.GetBytes(16);  //InitializationVector
                    // Default values:
                    //encryptor.BlockSize = 128;
                    //encryptor.KeySize = 256;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(),
                            CryptoStreamMode.Write))
                        {
                            cs.Write(dataBytes, 0, dataBytes.Length);
                        }
                        
                        if (b)
                        {
                            EncryptionPasswd = "";
                        }

                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Invalid Password", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //MessageBox.Show("Invalid Password" + Environment.NewLine + Environment.NewLine + ee.ToString(), "Invalid Password",
                //    MessageBoxButtons.OK, MessageBoxIcon.Error);

                EncryptionPasswd = "";
                
                return null;
            }
        }

        private string Decrypt(byte[] cipherBytes, bool b)
        {
            string EncryptionPasswd = "";

            if (b)
                EncryptionPasswd = this.EncryptionValuePasswd;
            else
                EncryptionPasswd = this.EncryptionPasswdDefault;

            try
            {
                using (Aes decryptor = Aes.Create())
                {
                    /* "AES Encryption" : <14Bytes> : Salt
                     * 65,69,83, 32, 69,110,99,114,121,112, 116,105,111,110, 32 ,89,71
                     * 0x41,0x45,0x53,0x20, 0x45,0x6e,0x63,0x72,0x79,0x70, 0x74,0x69,0x6f,0x6e
                     */
                    /* Rfc2898DeriveBytes */
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionPasswd,
                        new byte[] { 0x41, 0x45, 0x53, 0x20, 0x45, 0x6e, 0x63, 0x72,
                            0x79, 0x70, 0x74, 0x69, 0x6f, 0x6e });
                    decryptor.Key = pdb.GetBytes(32);
                    decryptor.IV = pdb.GetBytes(16);
                    
                    // Defaults:
                    //decryptor.BlockSize = 128;
                    //decryptor.KeySize = 256;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        // first Decrypt()

                        using (CryptoStream cs = new CryptoStream(ms, decryptor.CreateDecryptor(),
                            CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }

                        if (b)
                        {
                            EncryptionPasswd = "";
                        }

                        // then Decompress():

                        byte[] dataBytes = ms.ToArray();
                        
                        this.richTextBox1.ReadOnly = false;

                        if (this.isCompressed)
                        {
                            return this.encodingWith.GetString(Decompress(dataBytes));
                        }
                        else
                        {
                            return this.encodingWith.GetString(dataBytes);
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Invalid Password", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //MessageBox.Show("Invalid Password" + Environment.NewLine + Environment.NewLine + ee.ToString(), "Invalid Password",
                //    MessageBoxButtons.OK, MessageBoxIcon.Error);

                EncryptionPasswd = "";

                this.richTextBox1.ReadOnly = true;
                return null;
            }
        }

        private int ReadKeysFromFile()
        {
            keylist.Clear();

            using (FileStream fs = File.OpenRead(keysFile))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                int prekey = 0;
                byte[] fkey;
                long addr = 0;
                int keyTotal = 0, Total = 0;
                byte bdel = 0;

                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    bdel = reader.ReadByte();
                    keyTotal = reader.ReadInt32();
                    if (bdel == 0)
                    {
                        prekey = reader.ReadInt32();
                        fkey = reader.ReadBytes(prekey);
                        addr = reader.ReadInt64();
                        Total = reader.ReadInt32();

                        this.KeyFirstByte = fkey[0];
                        GetEncodingFromKeyByte(this.KeyFirstByte);

                        byte[] key = new byte[fkey.Length - 4];
                        Buffer.BlockCopy(fkey, 4, key, 0, key.Length);

                        if ((this.KeyFirstByte & 1) == 1)
                        {
                            keylist.Add(Decrypt(key, false), addr);
                        }
                        else
                        {
                            keylist.Add(this.encodingWith.GetString(key), addr);
                        }
                    }
                    else // pass, deleted
                    {
                        reader.ReadBytes(keyTotal);
                    }
                }
            }
            this.count = keylist.Count;
            UpdateStatus();

            return (keylist.Count);
        }

        private void KeysAddressFromDictionary()
        {
            long GTotal = 0;
            List<long> addrlist = new List<long>();
            addrlist.Add((long)0);
            long lastAddr = 0, nextAddr = 0;
            int i = 0, prekey = 0, predata = 0, Total = 0, ktot = 0;
            byte[] keydata;
            byte bdel = 0;

            keylist.Clear();

            using (FileStream fs = File.OpenRead(filename))
            using (BinaryReader reader = new BinaryReader(fs))
            using (FileStream fsk = File.Create(keysFile))
            using (BinaryWriter writer = new BinaryWriter(fsk))
            {
                while (GTotal < fs.Length)
                {
                    bdel = reader.ReadByte();
                    Total = reader.ReadInt32();
                    GTotal += 1 + 4 + Total;
                    prekey = reader.ReadInt32();
                    keydata = reader.ReadBytes(prekey);

                    predata = reader.ReadInt32();
                    fs.Seek(predata, SeekOrigin.Current);

                    addrlist.Add(GTotal);
                    lastAddr = nextAddr;
                    nextAddr = GTotal;

                    this.KeyFirstByte = keydata[0];
                    GetEncodingFromKeyByte(this.KeyFirstByte);

                    byte[] key = new byte[keydata.Length - 4];
                    Buffer.BlockCopy(keydata, 4, key, 0, key.Length);

                    if ((this.KeyFirstByte & 1) == 1)
                    {
                        keylist.Add(Decrypt(key, false), addrlist[i]);
                    }
                    else
                    {
                        keylist.Add(this.encodingWith.GetString(key), addrlist[i]);
                    }

                    ktot = 4 + prekey + 8 + 4;
                    writer.Write(bdel);
                    writer.Write(ktot);
                    writer.Write(prekey);
                    writer.Write(keydata);
                    writer.Write(addrlist[i]);
                    writer.Write(Total);
                    i++;
                }
            }
            this.count = keylist.Count;
            this.toolStripStatusLabel3.Text = "";

            this.ReadKeysFromFile();
        }

        private void WriteToFile(string key, string value, bool delete, bool withPass)
        {
            long addr = 0;

            if (delete)
            {
                using (FileStream fs = File.Open(this.filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    if (this.keylist.Keys.Contains(key))
                        addr = this.keylist[key];
                    
                    fs.Seek(addr, SeekOrigin.Begin);
                    writer.Write((byte)1); // X - DELETED
                }

                using (FileStream fsk = File.Open(this.keysFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                using (BinaryReader kreader = new BinaryReader(fsk))
                using (BinaryWriter kwriter = new BinaryWriter(fsk))
                {
                    byte bdel = 0;
                    int ktotal = 0, prekey = 0, Total = 0;
                    byte[] fkey = null;
                    long kaddr = 0;

                    while (kreader.BaseStream.Position != fsk.Length)
                    {
                        bdel = kreader.ReadByte();
                        ktotal = kreader.ReadInt32();

                        if (bdel == 0)
                        {
                            prekey = kreader.ReadInt32();
                            fkey = kreader.ReadBytes(prekey);
                            kaddr = kreader.ReadInt64();
                            Total = kreader.ReadInt32();

                            if (kaddr == addr)
                            {
                                fsk.Seek(-(1 + 4 + ktotal), SeekOrigin.Current);
                                kwriter.Write((byte)1);

                                break;
                            }
                            else
                                continue;
                        }
                        else // pass, deleted
                        {
                            kreader.ReadBytes(ktotal);
                        }
                    }
                }

            }
            else // if exist: delete and add, if not exist: add as a new entry
            {
                bool e;

                if (this.keylist.Keys.Contains(key))
                {
                    addr = this.keylist[key];
                    e = true;
                }
                else if (this.keylist.Keys.Contains(this.keyitem[0]))
                {
                    addr = this.keylist[this.keyitem[0]];
                    e = true;
                }
                else
                    e = false;

                if (e)
                {
                    using (FileStream fs = File.Open(this.filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    using (BinaryReader reader = new BinaryReader(fs))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        byte bdel = 0;
                        int Total = 0, prekey = 0, predat = 0;
                        byte[] k = null, v = null;
                        //
                        fs.Seek(addr, SeekOrigin.Begin);
                        writer.Write((byte)1); // X - DELETED

                        if (withPass)
                        {
                            k = Encrypt(key, false);
                            v = Encrypt(value, true);
                        }
                        else
                        {
                            k = this.encodingWith.GetBytes(key);
                            v = Encrypt(value, false);
                        }
                        prekey = (k.Length + 4);
                        predat = v.Length;
                        Total = 4 + prekey + 4 + predat;
                        long lastaddr = fs.Length;

                        fs.Seek(fs.Length, SeekOrigin.Begin);
                        writer.Write(bdel);
                        writer.Write(Total);
                        writer.Write(prekey);
                        writer.Write(this.tagKey);
                        writer.Write(k);
                        writer.Write(predat);
                        writer.Write(v);

                        using (FileStream fsk = File.Open(this.keysFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        using (BinaryReader kreader = new BinaryReader(fsk))
                        using (BinaryWriter kwriter = new BinaryWriter(fsk))
                        {
                            int ktotal = 0;
                            byte[] fkey = null;
                            long kaddr = 0;

                            while (kreader.BaseStream.Position != fsk.Length)
                            {
                                bdel = kreader.ReadByte();
                                ktotal = kreader.ReadInt32();

                                if (bdel == 0)
                                {
                                    prekey = kreader.ReadInt32();
                                    fkey = kreader.ReadBytes(prekey);
                                    kaddr = kreader.ReadInt64();
                                    Total = kreader.ReadInt32();

                                    if (kaddr == addr)
                                    {
                                        fsk.Seek(-(1 + 4 + ktotal), SeekOrigin.Current);
                                        kwriter.Write((byte)1); // X - DELETE

                                        break;
                                    }
                                    else
                                        continue;
                                }
                                else
                                {
                                    kreader.ReadBytes(ktotal);
                                }
                            }

                            fsk.Seek(fsk.Length, SeekOrigin.Begin);
                            ktotal = 4 + this.tagKey.Length + k.Length + 8 + 4;
                            kwriter.Write((byte)0);
                            kwriter.Write(ktotal);
                            kwriter.Write(this.tagKey.Length + k.Length);
                            kwriter.Write(this.tagKey);
                            kwriter.Write(k);
                            kwriter.Write(lastaddr);
                            kwriter.Write(Total);
                        }
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Append))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        byte bdel = 0;
                        int Total = 0, prekey = 0, predat = 0;
                        byte[] k = null, v = null;

                        if (withPass)
                        {
                            k = Encrypt(key, false);
                            v = Encrypt(value, true);
                        }
                        else
                        {
                            k = this.encodingWith.GetBytes(key);
                            v = Encrypt(value, false);
                        }
                        prekey = (k.Length + 4);
                        predat = v.Length;
                        Total = 4 + prekey + 4 + predat;
                        long lastaddr = fs.Length;

                        writer.Write(bdel);
                        writer.Write(Total);
                        writer.Write(prekey);
                        writer.Write(this.tagKey);
                        writer.Write(k);
                        writer.Write(predat);
                        writer.Write(v);

                        using (FileStream fsk = new FileStream(this.keysFile, FileMode.Append))
                        using (BinaryWriter kwriter = new BinaryWriter(fsk))
                        {
                            int ktotal = 4 + prekey + 8 + 4;
                            kwriter.Write((byte)0);
                            kwriter.Write(ktotal);
                            kwriter.Write(prekey);
                            kwriter.Write(this.tagKey);
                            kwriter.Write(k);
                            kwriter.Write(lastaddr);
                            kwriter.Write(Total);
                        }
                    }
                }
            }

            ReadKeysFromFile();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                return;
            }

            ListBox lb = (ListBox)sender;
            Graphics g = e.Graphics;
            SolidBrush sback = new SolidBrush(e.BackColor);
            SolidBrush sfore = new SolidBrush(e.ForeColor);

            if (e.Index % 2 == 0)
            {
                e.DrawBackground();
                DrawItemState st = DrawItemState.Selected;

                if ((e.State & st) != st)
                {
                    // Turkuaz= FF40E0D0(ARGB)
                    // Change with your favorite color here:
                    Color color = Color.FromArgb(30, 64, 224, 208);
                    g.FillRectangle(new SolidBrush(color), e.Bounds);
                    //g.FillRectangle(new SolidBrush(Color.WhiteSmoke), e.Bounds);
                    g.DrawString(lb.Items[e.Index].ToString(), e.Font,
                        sfore, new PointF(e.Bounds.X, e.Bounds.Y));
                }
                else
                {
                    g.FillRectangle(sback, e.Bounds);
                    g.DrawString(lb.Items[e.Index].ToString(), e.Font,
                        sfore, new PointF(e.Bounds.X, e.Bounds.Y));

                }
                e.DrawFocusRectangle();
            }
            else
            {
                e.DrawBackground();
                g.FillRectangle(sback, e.Bounds);
                g.DrawString(lb.Items[e.Index].ToString(), e.Font,
                    sfore, new PointF(e.Bounds.X, e.Bounds.Y));
                e.DrawFocusRectangle();
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null || listBox1.SelectedItem.ToString() == "")
                return;

            if (this.richTextBox1.Text != this.CopyOfTxt || this.richTextBox1.Rtf != this.CopyOfRtf)
            {
                SaveButton_Click(null, null);
                this.CopyOfRtf = this.richTextBox1.Rtf;
                this.CopyOfTxt = this.richTextBox1.Text;
            }

            if (listBox1.SelectedItem != null)
            {
                this.textBox1.Text = listBox1.SelectedItem.ToString();
                this.selectedKey = listBox1.SelectedItem.ToString();

                this.richTextBox1.Rtf = SearchKeyInFile(listBox1.SelectedItem.ToString());
                this.EntrySize = this.richTextBox1.Rtf.Length;

                this.CopyOfTxt = this.richTextBox1.Text;
                this.CopyOfRtf = this.richTextBox1.Rtf;

                SetBackColor(this.backRGB);
                this.richTextBox1.SelectionStart = this.rtbCursor;
                this.rtbCursor = 0;

                this.label2.Text = "  ";
            }
        }

        private void SetBackColor(byte[] rgb)
        {
            this.richTextBox1.BackColor = Color.FromArgb(255, rgb[0], rgb[1], rgb[2]);
        }

        private string SearchKeyInFile(string skey)
        {
            byte[] fkey = null, fvalue = null;
            long addr = 0;
            byte bdel = 0; int Total = 0;

            if (this.keylist.Keys.Contains(skey))
                addr = this.keylist[skey];
            else
                return null;

            using (FileStream fs = File.OpenRead(filename))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                fs.Seek(addr, SeekOrigin.Begin);

                bdel = reader.ReadByte();
                Total = reader.ReadInt32();
                int prekey = reader.ReadInt32();
                fkey = reader.ReadBytes(prekey);

                int predat = reader.ReadInt32();
                fvalue = reader.ReadBytes(predat);
                
                //this.EntrySize = predat;
                //System.Console.WriteLine("SearchKeyInFile> predatSize: {0}", predat);
            }

            if (fvalue != null)
                this.vfound = true;
            else
            {
                this.vfound = false;
                return null;
            }

            this.KeyFirstByte = fkey[0];
            GetEncodingFromKeyByte(this.KeyFirstByte);

            this.backRGB[0] = fkey[1];
            this.backRGB[1] = fkey[2];
            this.backRGB[2] = fkey[3];

            if ((this.KeyFirstByte & 1) == 1)
            {
                using (GetPasswd gep = new GetPasswd(this))
                {
                    gep.ShowDialog();
                }

                if (EncryptionValuePasswd != "")
                {
                    return (Decrypt(fvalue, true));
                }
                else
                {
                    this.richTextBox1.ReadOnly = true;
                    return null;
                }
            }
            else
            {
                return Decrypt(fvalue, false);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string key = this.textBox1.Text.Trim();

            if (key == "" || this.richTextBox1.Text == "" || this.richTextBox1.Rtf == "")
            {
                this.textBox1.Text = "";
                this.richTextBox1.Rtf = "";
                this.CopyOfTxt = this.richTextBox1.Text;
                this.label2.Text = "- Empty -";
                this.toolStripStatusLabel3.Text = "";
                UpdateStatus();
                return;
            }
            else
            {
                if (this.CopyOfRtf == this.richTextBox1.Rtf && this.CopyOfTxt == this.richTextBox1.Text)
                {
                    this.toolStripStatusLabel3.Text = "No difference, not saved.";
                    return;
                }

                using (SaveEntry saveit = new SaveEntry(this, key))
                {
                    saveit.ShowDialog();

                    if (this.SaveClicked)
                    {
                        this.CopyOfTxt = this.richTextBox1.Text;
                        this.CopyOfRtf = this.richTextBox1.Rtf;
                        
                        this.KeyFirstByte = EncodeKeyByte();
                        this.backRGB[0] = this.richTextBox1.BackColor.R;
                        this.backRGB[1] = this.richTextBox1.BackColor.G;
                        this.backRGB[2] = this.richTextBox1.BackColor.B;
                        this.tagKey = new byte[] { this.KeyFirstByte, this.backRGB[0], this.backRGB[1], this.backRGB[2] };
                        SetBackColor(this.backRGB);

                        if (this.withPassword)
                        {
                            if (this.EncryptionValuePasswd != "")
                            {
                                WriteToFile(key, this.richTextBox1.Rtf, false, true);

                                this.rtbCursor = this.richTextBox1.SelectionStart;
                                MessageBox.Show("Entry saved.", "Entry Saved",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Listele();
                                this.listBox1.SelectedItem = key;
                            }
                            else
                            {
                                this.rtbCursor = this.richTextBox1.SelectionStart;
                                MessageBox.Show("Password is Empty\r\nEntry can not be saved.", "Password Empty - Not Saved",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                                this.CopyOfTxt = "";
                                this.CopyOfRtf = "";

                                Listele();
                            }
                        }
                        else
                        {
                            WriteToFile(key, this.richTextBox1.Rtf, false, false);

                            this.rtbCursor = this.richTextBox1.SelectionStart;
                            MessageBox.Show("Entry saved!", "Entry Saved",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Listele();
                            this.listBox1.SelectedItem = key;
                        }
                    }
                    else // Cancelled
                    {
                        //
                    }
                }
            }

            UpdateStatus();
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            //
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            string key = this.textBox1.Text.Trim();

            if (this.textBox1.Text.Trim() == "")
            {
                this.label2.Text = " Empty ";
                UpdateStatus();
                this.toolStripStatusLabel3.Text = "";
                return;
            }

            if (!vfound)
                return;

            string msg = "";
            if (this.withPassword)
                msg = "is password protected.\r\n\r\nAre you sure?";
            else
                msg = "\r\nDelete from dictionary?";

            var DialogResult = MessageBox.Show(key + Environment.NewLine + msg, "Delete Entry",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (DialogResult == DialogResult.Yes)
            {
                WriteToFile(key, null, true, false);

                this.textBox1.Text = "";
                this.richTextBox1.Rtf = "";
                this.CopyOfTxt = this.richTextBox1.Text;
                resetFontsAndColorToolStripMenuItem_Click(null, null);
                MessageBox.Show("Entry Deleted!", "Entry Deleted",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Listele();
            }
            else
            {
                //
            }
            UpdateStatus();

        }

        private void resetFontsAndColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
            richTextBox1.Font = richTextBox1.SelectionFont = this.cf;
            richTextBox1.SelectionColor = this.ct;
            richTextBox1.BackColor = this.cb;
            richTextBox1.SelectionStart = 0; richTextBox1.SelectionLength = 0;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            this.toolStripStatusLabel3.Text = " ";
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                // for Enter key_UP:
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    return;
                }

                if (e.KeyCode == Keys.Escape)
                    return;
            }

            string skey = this.textBox1.Text.Trim();

            if (skey == "")
            {
                if (this.listBox1.Items.Count < keylist.Count)
                    Listele();

                this.label2.Text = "- Empty -";
                this.toolStripStatusLabel3.Text = "";
                UpdateStatus();

                return;
            }

            if (this.checkBox1.Checked) // including
            {
                var results = keylist.Where(
                    key => key.Key.ToUpper().Contains(skey.ToUpper())).Select(key => key.Key);

                this.listBox1.Items.Clear();
                this.listBox1.Items.AddRange(results.ToArray());
            }
            else
            {
                var results = keylist.Where(
                    key => key.Key.StartsWith(skey, StringComparison.OrdinalIgnoreCase)).Select(key => key.Key);

                this.listBox1.Items.Clear();
                this.listBox1.Items.AddRange(results.ToArray());
            }
            UpdateStatus();

            if (keylist.Keys.Contains(skey))
            {
                if (this.richTextBox1.ReadOnly)
                    this.richTextBox1.ReadOnly = false;

                this.richTextBox1.Rtf = SearchKeyInFile(skey);
                this.CopyOfTxt = this.richTextBox1.Text;
                this.CopyOfRtf = this.richTextBox1.Rtf;
                SetBackColor(this.backRGB);

                if (!vfound)
                {
                    this.label2.Text = "Not found";
                    this.vfound = false;

                    this.CopyOfRtf = "";
                    this.CopyOfTxt = "";
                    this.toolStripStatusLabel3.Text = "";
                    UpdateStatus();

                    return;
                }

                this.label2.Text = "  ";
            }
            else
            {
                if (this.richTextBox1.ReadOnly)
                    this.richTextBox1.ReadOnly = false;

                this.label2.Text = "Not found";
                this.vfound = false;

                this.CopyOfTxt = "";
                this.CopyOfRtf = "";
                this.toolStripStatusLabel3.Text = "";
            }

            UpdateStatus();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveButton_Click(null, null);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if ((this.CopyOfRtf != this.richTextBox1.Rtf) && this.richTextBox1.Text.Trim() != "")
            {
                SaveButton_Click(null, null);
            }
            this.Close();
        }

        private bool RenameKeyItem()
        {
            if (this.keyitem[1] == "" || (this.keyitem[0] == this.keyitem[1]))
                return false;

            if (this.withPassword)
            {
                WriteToFile(this.keyitem[1], this.richTextBox1.Rtf, false, true);
            }
            else
            {
                WriteToFile(this.keyitem[1], this.richTextBox1.Rtf, false, false);
            }

            Listele();
            
            this.textBox1.Text = this.keyitem[0] = this.keyitem[1];
            this.CopyOfRtf = this.richTextBox1.Rtf;
            this.CopyOfTxt = this.richTextBox1.Text;
            return true;
        }

        private void ChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1_MouseDoubleClick(null, null);
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.listBox1.SelectedIndex < 0)
                return;

            this.keyitem[0] = this.listBox1.SelectedItem.ToString(); // [0]:old, [1]:new


            if (this.withPassword)
            {
                string s = SearchKeyInFile(this.keyitem[0]);

                if (s == null)
                {
                    return;
                }
                else
                {
                    this.richTextBox1.Rtf = s;
                    this.CopyOfTxt = this.richTextBox1.Text;
                }
            }

            using (RenameKeyItem cki = new RenameKeyItem(this))
            {
                cki.ShowDialog();
            }

            if (this.keyitem[1] == "" || (this.keyitem[0] == this.keyitem[1]))
            {
                this.keyitem[0] = ""; this.keyitem[1] = "";
                return;
            }

            this.KeyFirstByte = EncodeKeyByte();
            this.backRGB[0] = this.richTextBox1.BackColor.R;
            this.backRGB[1] = this.richTextBox1.BackColor.G;
            this.backRGB[2] = this.richTextBox1.BackColor.B;
            this.tagKey = new byte[] { this.KeyFirstByte, this.backRGB[0], this.backRGB[1], this.backRGB[2] };

            if (RenameKeyItem())
                MessageBox.Show("Renamed!", "Renamed",
                     MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Key Not renamed!", "Not renamed",
                     MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            if (this.listBox1.SelectedItem == null || this.listBox1.SelectedItem.ToString() == "")
            {
                this.contextMenuStrip2.Items[0].Enabled = false;
                this.contextMenuStrip2.Items[3].Enabled = false;
                return;
            }
            else
            {
                this.contextMenuStrip2.Items[0].Enabled = true;
                this.contextMenuStrip2.Items[3].Enabled = true;
            }
        }

        private void FileNametoolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog changeFile = new OpenFileDialog();
            changeFile.Filter = "Bin (*.bin)|*.bin";

            if (changeFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.filename = changeFile.FileName; // SafeFileName;
                this.keysFile = filename + ".keys";
                this.toolStripStatusLabel2.Text = "File: " + changeFile.SafeFileName;
                this.toolStripStatusLabel3.Text = "File changed!";

                OpenFile();

                resetFontsAndColorToolStripMenuItem_Click(null, null);
                this.richTextBox1.Clear();
                this.textBox1.Text = "";
            }

        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = richTextBox1.SelectionStart;
            int len = richTextBox1.SelectionLength;
            richTextBox1.Select(start, len);

            richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, richTextBox1.SelectionFont.Style ^ FontStyle.Bold);
            richTextBox1.SelectionStart = richTextBox1.SelectionStart + richTextBox1.SelectionLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionFont = richTextBox1.Font;
            richTextBox1.Select(start, len);
        }

        private void underlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = richTextBox1.SelectionStart;
            int len = richTextBox1.SelectionLength;
            richTextBox1.Select(start, len);

            richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, richTextBox1.SelectionFont.Style ^ FontStyle.Underline);
            richTextBox1.SelectionStart = richTextBox1.SelectionStart + richTextBox1.SelectionLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionFont = richTextBox1.Font;
            richTextBox1.Select(start, len);
        }

        private void italicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = richTextBox1.SelectionStart;
            int len = richTextBox1.SelectionLength;
            richTextBox1.Select(start, len);

            richTextBox1.SelectionFont = new Font(richTextBox1.SelectionFont, richTextBox1.SelectionFont.Style ^ FontStyle.Italic);
            richTextBox1.SelectionStart = richTextBox1.SelectionStart + richTextBox1.SelectionLength;
            richTextBox1.SelectionLength = 0;
            richTextBox1.SelectionFont = richTextBox1.Font;
            richTextBox1.Select(start, len);

        }

        private void upperLowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = richTextBox1.SelectionStart;
            int len = richTextBox1.SelectionLength;
            richTextBox1.Select(start, len);

            if (this.richTextBox1.SelectedText == this.richTextBox1.SelectedText.ToLower())
                this.richTextBox1.SelectedText = this.richTextBox1.SelectedText.ToUpper();
            else
                this.richTextBox1.SelectedText = this.richTextBox1.SelectedText.ToLower();

            richTextBox1.Select(start, len);

        }

        private void fontsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                richTextBox1.SelectionFont = fontDialog1.Font;
        }

        private void colorstoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                richTextBox1.SelectionColor = colorDialog1.Color;
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                richTextBox1.BackColor = colorDialog1.Color;
        }

        private void specialCharactersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VKeyboard vkyb = new VKeyboard(this);
            vkyb.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                this.DeleteButton_Click(null, null);
            else if (e.Control && e.KeyCode == Keys.S)
                e.Handled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox1_KeyUp(null, null);
        }

        private void deleteEntrytoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.DeleteButton_Click(null, null);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox2.CheckState == CheckState.Checked)
            {
                this.listBox1.Sorted = true;
            }
            else
            {
                this.listBox1.Sorted = false;
            }
            this.listBox1.Items.Clear();
            this.listBox1.Items.AddRange(keylist.Keys.Reverse().ToArray());
        }

        private void NewEntryButton_Click(object sender, EventArgs e)
        {
            resetFontsAndColorToolStripMenuItem_Click(null, null);
            this.richTextBox1.Clear();

            if (this.richTextBox1.ReadOnly)
                this.richTextBox1.ReadOnly = false;

            dateTimePicker1.Value = DateTime.Now;

            string date = dateTimePicker1.Value.ToString("dd' 'MMMM' 'yyyy ");
            string time = dateTimePicker1.Value.TimeOfDay.ToString("hh':'mm':'ss");

            this.richTextBox1.Text = 
                "-------------------------------------------------------------------------"
                + Environment.NewLine + date + time + Environment.NewLine +
                "-------------------------------------------------------------------------"
                + Environment.NewLine + this.richTextBox1.Text;

            this.textBox1.Text = " " + date + time;
        }

        private void FMtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileMaintenance fm = new FileMaintenance(this);
            fm.ShowDialog();

            this.KeysAddressFromDictionary();
        }

        private void DeletetoolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DeleteButton_Click(null, null);
        }

        private void EditingKeystoolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditingKeys edk = new EditingKeys();
            edk.Show();
        }

        private void EditingKeystoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.EditingKeystoolStripMenuItem_Click(null, null);
        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            int newIndex = listBox1.IndexFromPoint(e.Location);
            if (oldIndex != newIndex)
            {
                oldIndex = newIndex;
                if (oldIndex > -1)
                {
                    this.toolTip1.Active = false;
                    this.toolTip1.SetToolTip(lb, lb.Items[oldIndex].ToString());
                    this.toolTip1.Active = true;
                }
            }
        }

        private void wordwraptoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.richTextBox1.WordWrap)
            {
                this.richTextBox1.WordWrap = false;
                this.wordwraptoolStripMenuItem1.CheckState = CheckState.Unchecked;
                this.wordwrapEditmenu.CheckState = CheckState.Unchecked;
            }
            else
            {
                this.richTextBox1.WordWrap = true;
                this.wordwraptoolStripMenuItem1.CheckState = CheckState.Checked;
                this.wordwrapEditmenu.CheckState = CheckState.Checked;
            }
        }

        private void wordwrapEditmenu_Click(object sender, EventArgs e)
        {
            wordwraptoolStripMenuItem1_Click(null, null);
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sz = CalculateSize(this.EntrySize);

            MessageBox.Show("Entry:\t\t" + this.selectedKey + Environment.NewLine
                + "Encoded with:\t" + this.encodingWith + Environment.NewLine
                + "Protection:\t" + this.pw + Environment.NewLine 
                + "Entry Size (RTF):\t" + sz, 
                "Entry Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string CalculateSize(int i) {

            string s = "0";
            double di = 0;

            if (i < 1024)
                return (Convert.ToString(i) + " Bytes");
            else if (i > 1024 && i < 1048576)
            {
                di = i / 1024.0;  // KiBytes

                s = String.Format("{0:0.##}", di) + " KBytes";
            }
            else if (i > 1048576)
            {
                di = i / 1048576.0; // MegaBytes

                s = String.Format("{0:0.##}", di) + " MBytes";
            }

            return s;
        }

    }
}
