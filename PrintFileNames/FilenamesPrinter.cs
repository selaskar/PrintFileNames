using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace PrintFileNames
{
    public class FilenamesPrinter
    {
        public void PrintFilenames()
        {
            try
            {
                FindTextFileName();

                _sbFiles.AppendLine("CURRENT DIRECTORY:" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                FindNamesRecursively(".");

                File.WriteAllText(_textFilename, _sbFiles.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                try ///dosya oluşmuşsa sil.
                {
                    if (File.Exists(_textFilename))
                        File.Delete(_textFilename);
                }
                catch { }
            }
            finally
            {
                MessageBox.Show("Process completed.");
                App.Current.Shutdown();
            }
        }

        string _textFilename;

        private void FindTextFileName()
        {
            var currentFolderName = string.Empty;
            if (Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent != null) ///dosya, kök dizinde çalışmıyorsa.
            {
                currentFolderName = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Name;
            }
            else //dosya, bir sürücünün kök dizininde çalışıyorsa.
            {
                ///dosyanın bulunduğu sürücüyü bul.
                DriveInfo drive = DriveInfo.GetDrives().Where(drv => drv.Name == Directory.GetDirectoryRoot(Assembly.GetExecutingAssembly().Location)).Single();

                ///sürücünün, volume label'ı boş ise veya
                ///volume label'ının, dosya isminde kullanılamayacak karakterler içeriyorsa,
                ///sürücü harfini, dosyanın adına koy.
                if (string.IsNullOrWhiteSpace(drive.VolumeLabel)
                    || new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]").IsMatch(drive.VolumeLabel))
                {
                    currentFolderName = Directory.GetDirectoryRoot(Assembly.GetExecutingAssembly().Location).Substring(0, 1);
                }
                else ///volume label, dosya adında kullanılmaya uygunsa.
                    currentFolderName = drive.VolumeLabel;
            }

            _textFilename = "(" + DateTime.Now.ToString("yyyy.MM.dd HH.mm") + ") " + currentFolderName + ".txt";
        }


        StringBuilder _sbFiles = new StringBuilder();

        private void FindNamesRecursively(string path)
        {
            string[] topDirectories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            string[] topFiles = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < topDirectories.Length; i++)
            {
                try
                {
                    _sbFiles.AppendLine(topDirectories[i] + "\\");
                    FindNamesRecursively(topDirectories[i]);
                }
                catch (Exception)
                {
                    _sbFiles.AppendLine("INACCESSIBLE DIRECTORY: " + topDirectories[i]);
                }
            }

            for (int i = 0; i < topFiles.Length; i++)
            {
                _sbFiles.AppendLine(topFiles[i]);
            }
        }
    }
}
