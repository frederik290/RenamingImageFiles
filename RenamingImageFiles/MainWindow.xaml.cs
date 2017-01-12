using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace RenamingImageFiles
{

    public partial class MainWindow : Window
    {
        private string fileDir = "";
        private string fileExtension = "";
        private bool textChanged = false;
        public RelayCommand _renameCommand;
        public RelayCommand _renameAllCommand;
        public ObservableCollection<string> filenames;

        public MainWindow()
        {
            InitializeComponent();
            MainGrid.DataContext = this;
            openBtn.Click += OpenDialog;
            newFilenameTbx.TextChanged += fileNameChanged;
            //renameAllBtn.Click += RenameAll;
            filenamesLstBx.SelectionChanged += loadNewImage;
        }

        public void OpenDialog(object sender, RoutedEventArgs eArgs)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Supported Graphics: .jpg, .jpeg, .jpe, .bmp, .png|*.jpg;*.jpeg;*.jpe;*.bmp;*.png;";
            dialog.Title = "Select Pictures";

            if (dialog.ShowDialog() == true)
            {
                string[] filenamesArray = dialog.FileNames;
                filenames = new ObservableCollection<string>(filenamesArray);
                filenamesLstBx.ItemsSource = filenames;
                displayImgOf(new Uri(dialog.FileName));
                filenamesLstBx.SelectedIndex = 0;

                //set vars for later use 
                //we assume that alle images have the same file extension 
                fileDir = System.IO.Path.GetDirectoryName(dialog.FileName);
                fileExtension = System.IO.Path.GetExtension(dialog.FileName);
                textChanged = false;
            }
        }
        
        private void displayImgOf(Uri uri) {
            BitmapImage bitImg = new BitmapImage();
            bitImg.BeginInit();
            bitImg.CacheOption = BitmapCacheOption.OnLoad;
            bitImg.UriSource = uri;
            bitImg.EndInit();
            image.Source = bitImg;
        }

        private void fileNameChanged(object sender, RoutedEventArgs eArgs)
        {
            textChanged = true;
        }
        private void loadNewImage(object sender, RoutedEventArgs eArgs)
        {
            // a bit hacky or should we say quick and dirty
            if((fileDir != "") && newFilenameTbx.Text != "")
            {
                string fileToLoad = System.IO.Path.Combine(fileDir, newFilenameTbx.Text + fileExtension);
                displayImgOf(new Uri(fileToLoad));

            }
            textChanged = false;
        }

        /////////// Command section ///////////

        public ICommand RenameCommand
        {
            get
            {
                return _renameCommand ??
                       (_renameCommand = new RelayCommand(RenameCommandExecute, RenameCommandCanExecute));
            }
        }
        public ICommand RenameAllCommand
        {
            get
            {
                return _renameAllCommand ??
                       (_renameAllCommand = new RelayCommand(RenameAllCommandExecute, RenameCommandCanExecute));
            }
        }

        public bool RenameCommandCanExecute()
        {
            return (newFilenameTbx.Text != "") && textChanged && (filenamesLstBx.SelectedIndex > -1) ;
        }
        public void RenameCommandExecute()
        {
            string oldFilename = oldFilenameTblck.Text;
            string oldFilePath = System.IO.Path.Combine(fileDir, oldFilename);
            string newFilename = newFilenameTbx.Text;
            string newFilePath = System.IO.Path.Combine(fileDir, newFilename);
            System.IO.File.Move(oldFilePath + fileExtension , newFilePath + fileExtension);
            textChanged = false;
            filenames.Remove((string)filenamesLstBx.SelectedItem);
            NotifyPropertyChannged("filenames");
            filenamesLstBx.SelectedIndex = 0;
            MessageBox.Show("Your image was renamed to " + newFilename);
        }
        public void RenameAllCommandExecute()
        {
            int count = 0;
            string prefixName = newFilenameTbx.Text;

            // we assume that alle images have the same file extension 
            foreach (string oldFilePath in filenames)
            {
                string newFilePath = System.IO.Path.Combine(fileDir, prefixName);
                File.Move(oldFilePath, newFilePath + count.ToString() + fileExtension);
                count++;
            }

            filenames.Clear();
            NotifyPropertyChannged("filenames");
            MessageBox.Show("All the images have been renamed with the prefix: " + prefixName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChannged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
