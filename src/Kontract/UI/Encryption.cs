﻿using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections.Generic;
using System.Linq;
using Kontract.IO;
using Kontract.Interface;

namespace Kontract.UI
{
    class EncryptionLoad
    {
        [ImportMany(typeof(IEncryption))]
        public List<IEncryption> encryptions;

        public EncryptionLoad()
        {
            var catalog = new DirectoryCatalog("Komponents");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    public static class EncryptionTools
    {
        static ToolStripMenuItem AddEncryptionTab(ToolStripMenuItem encryptTab, IEncryption encryption, bool encTab = true, int count = 0)
        {
            if (encTab)
            {
                if (encryption.TabPathEncrypt == "") return encryptTab;
            }
            else
            {
                if (encryption.TabPathDecrypt == "") return encryptTab;
            }

            string[] parts = encTab ? encryption.TabPathEncrypt.Split('/') : encryption.TabPathDecrypt.Split('/');

            if (count == parts.Length - 1)
            {
                encryptTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null, Encrypt));
                encryptTab.DropDownItems[encryptTab.DropDownItems.Count - 1].Tag = encryption;
                if (encryption.TabPathEncrypt.Contains(',')) encryptTab.DropDownItems[encryptTab.DropDownItems.Count - 1].Name = encryption.TabPathEncrypt.Split(',')[1];
            }
            else
            {
                ToolStripItem duplicate = null;
                for (int i = 0; i < encryptTab.DropDownItems.Count; i++)
                {
                    if (encryptTab.DropDownItems[i].Text == parts[count])
                    {
                        duplicate = encryptTab.DropDownItems[i];
                        break;
                    }
                }
                if (duplicate != null)
                {
                    AddEncryptionTab((ToolStripMenuItem)duplicate, encryption, encTab, count + 1);
                }
                else
                {
                    encryptTab.DropDownItems.Add(new ToolStripMenuItem(parts[count], null));
                    AddEncryptionTab((ToolStripMenuItem)encryptTab.DropDownItems[encryptTab.DropDownItems.Count - 1], encryption, encTab, count + 1);
                }
            }

            return encryptTab;
        }

        public static void LoadEncryptionTools(ToolStripMenuItem tsb)
        {
            tsb.DropDownItems.Clear();

            tsb.DropDownItems.Add(new ToolStripMenuItem("Encrypt", null));
            tsb.DropDownItems.Add(new ToolStripMenuItem("Decrypt", null));

            var encryptTab = (ToolStripMenuItem)tsb.DropDownItems[0];
            var decryptTab = (ToolStripMenuItem)tsb.DropDownItems[1];

            var loadedEncs = new EncryptionLoad();
            var encryptions = loadedEncs.encryptions;

            //Adding single compressions
            for (int i = 0; i < encryptions.Count; i++)
            {
                encryptTab = AddEncryptionTab(encryptTab, encryptions[i]);
                decryptTab = AddEncryptionTab(decryptTab, encryptions[i], false);
            }
        }

        public static void Decrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (IEncryption)tsi.Tag;

            if (!Shared.PrepareFiles("Open an encrypted " + tag.Name + " file...", "Save your decrypted file...", ".dec", out FileStream openFile, out FileStream saveFile)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(tag.Decrypt(openFile));

                MessageBox.Show($"Successfully decrypted {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(saveFile.Name);
            }
        }

        public static void Encrypt(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripMenuItem;
            var tag = (IEncryption)tsi.Tag;

            if (!Shared.PrepareFiles("Open a decrypted " + tag.Name + " file...", "Save your encrypted file...", ".enc", out var openFile, out var saveFile, true)) return;

            try
            {
                using (openFile)
                using (var outFs = new BinaryWriterX(saveFile))
                    outFs.Write(tag.Encrypt(openFile));

                MessageBox.Show($"Successfully encrypted {Path.GetFileName(openFile.Name)}.", tsi.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), tsi?.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(saveFile.Name);
            }
        }
    }
}
