using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using FESOVSE.Extension;
using Xceed.Wpf.Toolkit;

namespace FESOVSE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] _saveFile; //contents of the save file
        private string path; //location of the save file
        private IEnumerable<IntegerUpDown> upDwnBoxes; //list of numeric updownboxes contained in window

        public MainWindow()
        {
            InitializeComponent();
            initControls();
        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Chapter Files|*"; //filter what shows in the open file dialog box
            ofd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory; //set starting directory
            if(ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
                if (path.Contains("Chapter") && path.Contains("dec")) loadFile(); //check if file is the right file, load if true
                else System.Windows.MessageBox.Show("Not a Chapter file OR a decrypted Chapter file");         
            }

        }

        private void saveFile_Click(object sender, RoutedEventArgs e)
        {
            if(path != null)
            {
                File.WriteAllBytes(path, _saveFile);
                System.Windows.MessageBox.Show("File Saved Successfully");
            }
            else System.Windows.MessageBox.Show("No File Found");

        }
     
   
        #region Utility Functions

        /* checks if a sequence of bytes is in the save file */
        private int hasData(int blockSize, byte[] data, int start = 0)
        {
            int maxLength = start + blockSize; //max size of search space
            for (int i = start; i + data.Length < maxLength; i++)
            {
                bool isSame = true;
                for (int j = 0; j < data.Length; j++)
                {
                    if (_saveFile[i + j] != data[j]) isSame = false;
                }
                if (isSame) return i; //if data is found return address of data

            }
            return -1; //if dota does not exist, return -1
        }

        /* converts a string of hex into byte array */
        private byte[] hexToBytes(string hexStr)
        {
            byte[] byteVal = new byte[hexStr.Length / 2];
            for (int i = 0; i < byteVal.Length; i++)
            {
                string subStr = hexStr.Substring(i * 2, 2);
                byteVal[i] = Convert.ToByte(subStr, 16);
            }

            return byteVal;
        }

        /* return a hex string of a value at a specific address */
        private string getBytesValue(int startAddress, int size)
        {
            StringBuilder hex = new StringBuilder(size*2);
            for (int i = 0; i < size; i++)
            {
                var b = _saveFile[startAddress + i];
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        /* find the location of the inventory of character */
        private int findItemAddress(int charIDStart)
        {
            byte[] itemByte = { 2, 1 }; //items are labelled with 02 01 afaik
            int charBlockSize = 180; //search space for a character block, char block changes depending on skills,supports etc
                                     //so this is just an assumable, if character's block is smaller it will get the next block's
                                     //works well if each character holds an item
            return hasData(charBlockSize, itemByte, charIDStart);
        }

        #endregion

        #region Setup Functions

        private void initControls()
        {
            cbItem.IsHitTestVisible = false; //disable controls at setup
            cbForge.IsHitTestVisible = false;
            cbClass.IsHitTestVisible = false;
            upDwnBoxes = this.FindVisualChildren<IntegerUpDown>();

        }
        /* reading file and loading database into controls*/
        private void loadFile()
        {
            _saveFile = File.ReadAllBytes(path);
            loadUnits();
            loadItems();
            loadClasses();
            bindEvents();
        }

        private void loadUnits()
        {
            //getting the pointer to character stored at 0xCC
            int charBlockAddress = 0;
            for (int i = 0; i < 4; i++)
            {
                charBlockAddress = (_saveFile[0xCC + i] << (i * 8)) | charBlockAddress;
                //its just 2 bytes but I take 4 anyway
            }

            var charDB = new Data.CharacterDatabase(); //init database of characters from xml file
            var units = charDB.getAll(); //list of all units
            var currentUnits = new List<Data.Character>(); //units that are currently available in game
            foreach (Data.Character c in units)
            {
                byte[] uID = hexToBytes(c.CharID);
                int f = hasData(_saveFile.Length - charBlockAddress, uID, charBlockAddress); //check if character is available
                if (f != -1)
                {
                    c.StartAddress = f; //set character's start address for easy access (offset of start of character id)
                    c.ItemAddress = findItemAddress(f + 30); //also set the item address as its different depending on character (no specific offset)
                    currentUnits.Add(c);
                }
            }
            //enabling data source for the control
            unitList.ItemsSource = currentUnits;
            unitList.DisplayMemberPath = "Name";
            unitList.SelectedValuePath = "ItemAddress";

        }

        private void loadItems()
        {
            var itemDB = new Data.ItemDatabase();
            var items = itemDB.getAll();
            cbItem.ItemsSource = items;
            cbItem.DisplayMemberPath = "Name";
            cbItem.SelectedValuePath = "Hex";
        }

        private void loadClasses()
        {
            var charClassDB = new Data.CharacterClassDatabase();
            var allClasses = charClassDB.getAll();
            cbClass.ItemsSource = allClasses;
            cbClass.DisplayMemberPath = "Name";
            cbClass.SelectedValuePath = "ClassID";

        }

        #endregion

        #region Updating Panel Functions

        /* updating the window screen */
        private void updateDescription(object sender, SelectionChangedEventArgs e)
        {
            unBindEvents();

            cbItem.IsHitTestVisible = true; //enable control
            var character = (Data.Character)unitList.SelectedItem; //get the currently selected character

            if (character.ItemAddress != -1) //if character is holding an item
            {
                string itemHex = getBytesValue(character.ItemAddress + 6, 8);
                cbItem.SelectedValue = itemHex; //update the item combo box with unit's item
                if (cbItem.SelectedValue == null) //disable combobox if item is not in database
                {                                 //will remove once resource is complete
                    cbItem.IsHitTestVisible = false;
                    cbForge.IsHitTestVisible = false;
                }
                else
                {
                    var currentItem = (Data.Item)cbItem.SelectedItem;
                    int currentForge = _saveFile[character.ItemAddress + 5] >> 4; // forge offset +5 after 02 01
                    updateForgeBox(currentItem.MaxForges, currentForge);
                }
            }
            else
            {
                //disable forge and item comboboxes if no held item
                cbItem.SelectedIndex = -1;
                cbForge.SelectedIndex = -1;
                cbItem.IsHitTestVisible = false;
                cbForge.IsHitTestVisible = false;
            }

            updateStatBox();
            updateClassBox();
            bindEvents();

        }
        /* update the forge combo box based on current item in item combo box*/
        private void updateForgeBox(int maxForges, int currentForge = 0)
        {
            cbForge.Items.Clear();
            if (maxForges != 0)
            {
                for (int i = 0; i <= maxForges; i++)
                {
                    cbForge.Items.Add(i);
                }
                cbForge.IsHitTestVisible = true;
                cbForge.SelectedIndex = currentForge;
            }
            else cbForge.IsHitTestVisible = false;
        }
        /* updates the numeric updowns based on character's current stat*/
        private void updateStatBox()
        {
            Data.Character character = (Data.Character)unitList.SelectedItem;
            int level = _saveFile[character.StartAddress - 2]; //level offset 2 bytes before character id
            int exp = _saveFile[character.StartAddress - 1]; //exp offset 1 byte before character id
            int statAddress = character.StartAddress + 21; //character stats offset 21 bytes after character id
            int counter = -2;
            foreach (IntegerUpDown iUD in upDwnBoxes)
            {
                if (counter == -2) iUD.Value = level;
                else if (counter == -1) iUD.Value = exp;
                else
                {
                    //calculates stat using character base and value from save file
                    //character stats = value from save file + base stats
                     iUD.Minimum = character.BaseStats[counter];
                    iUD.Maximum = character.MaxStats[counter];
                    iUD.Value = _saveFile[statAddress + counter] + character.BaseStats[counter];
                }
                counter++;
            }
        }
        /* updates class box based on selected character*/
        private void updateClassBox()
        {
            cbClass.IsHitTestVisible = true;
            Data.Character character = (Data.Character)unitList.SelectedItem;
            string classHex = getBytesValue(character.StartAddress + 8, 8);
            cbClass.SelectedValue = classHex;
        }

        #endregion

        #region Event Functions
        /* fired when item combo box is changed*/
        private void itemBoxChanged(object sender, EventArgs e)
        {
            unBindEvents();

            Data.Character character = (Data.Character)unitList.SelectedItem;
            Data.Item currentItem = (Data.Item)cbItem.SelectedItem;
            byte[] itemHex = hexToBytes(currentItem.Hex); 
            byte[] itemID = hexToBytes(currentItem.ItemID);
            byte[] itemMiddleHex;
            if (currentItem.isDLC) itemMiddleHex = hexToBytes("010008"); //I see this pattern if its a dlc
            else itemMiddleHex = hexToBytes("000000"); //default value, no forges, etc.
            IEnumerable<byte> itemVal = itemID.Concat(itemMiddleHex).Concat(itemHex); //combine the bytes to form 12 bytes of item value
            int start = character.ItemAddress + 2;
            foreach (byte b in itemVal)
            {
                _saveFile[start] = b; //insert the new value in file
                start++;
            }

            updateForgeBox(currentItem.MaxForges); //disable/enable forge box depending on current item

            bindEvents();
        }
        /* fired when forge combo box changed */
        private void forgeBoxChanged(object sender, EventArgs e)
        {
            unBindEvents();

            Data.Character character = (Data.Character)unitList.SelectedItem;
            byte forgeVal = Convert.ToByte(cbForge.SelectedItem.ToString(), 16); //get the value changed
            byte val = _saveFile[character.ItemAddress + 5]; //forge offset 5 after 02 01, the leftmost 4 bits only
            val = (byte)(val & 0x0F); //clear leftmost 4 bits
            val = (byte)(val | (forgeVal << 4)); //add the new value to leftmost 4 bits
            _saveFile[character.ItemAddress + 5] = val;

            bindEvents();
        }

        private void statChanged(object sender, EventArgs e)
        {
            unBindEvents();

            Data.Character character = (Data.Character)unitList.SelectedItem;
            IntegerUpDown statBox = (IntegerUpDown)sender; //get the control that fired the event
            Enum.Stat stat = XMLProperties.StatProperty.GetStat(statBox); //get value of attached property

            if (stat == Enum.Stat.Level || stat == Enum.Stat.Experience)
            {
                _saveFile[character.StartAddress + (int)stat] = (byte)statBox.Value; //update exp or level based on current value
            }
            else
            {
                // new value is the character base stats subtracted from current value
                // because the value in save file is just the values to be added to the base stat
                byte newVal = (byte)(statBox.Value - character.BaseStats[(int)stat]);
                _saveFile[character.StartAddress + 21 + (int)stat] = newVal;
            }
            
            bindEvents();
        }

        private void classChanged(object sender, EventArgs e)
        {
            unBindEvents();

            Data.Character character = (Data.Character)unitList.SelectedItem;
            Data.CharacterClass cc = (Data.CharacterClass)cbClass.SelectedItem;
            byte[] classVal = hexToBytes(cc.ClassID);
            int index = character.StartAddress + 8;
            foreach (byte b in classVal)
            {
                _saveFile[index] = b; //insert new value of class into save file
                index++;
            } 

            bindEvents();
        }


        private void unBindEvents()
        {
            cbItem.SelectionChanged -= itemBoxChanged;
            unitList.SelectionChanged -= updateDescription;
            cbForge.SelectionChanged -= forgeBoxChanged;
            cbClass.SelectionChanged -= classChanged;
            foreach (IntegerUpDown x in upDwnBoxes)
            {
                x.ValueChanged -= statChanged;
            }
        }

        private void bindEvents()
        {
            cbItem.SelectionChanged += itemBoxChanged;
            unitList.SelectionChanged += updateDescription;
            cbForge.SelectionChanged += forgeBoxChanged;
            cbClass.SelectionChanged += classChanged;
            foreach (IntegerUpDown x in upDwnBoxes)
            {
                x.ValueChanged += statChanged;
            }
        }

        #endregion

    }
}
