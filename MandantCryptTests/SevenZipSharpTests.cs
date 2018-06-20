using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SevenZip;
using System.IO;
using Microsoft.Win32;


namespace MandantCrypt.Tests
{
    [TestClass()]
    public class SevenZipSharpTests
    {
        string testRessources;
        string currentDir;


        [TestInitialize()]
        public void Initialize()
        {
            this.currentDir = Directory.GetCurrentDirectory();
            this.testRessources = Path.GetFullPath(currentDir + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "testressources");
            SevenZipBase.SetLibraryPath(getLibraryPathFromDirectory(getArchitecture()));
        }

        [TestMethod()]
        public void FolderCheck()
        {
            string[] filesInCurrentDir = Directory.GetFiles(".");
            Assert.IsTrue(Directory.Exists(testRessources));
            string srcFolder = testRessources + Path.DirectorySeparatorChar + "tozip";
            Assert.IsTrue(Directory.Exists(srcFolder));
            string[] filesInSrcFolder = Directory.GetFiles(srcFolder);
            Assert.IsTrue(filesInSrcFolder.Length > 3);
        }

        [TestMethod()]
        public void x86LibTest()
        { 
            string libPath = getLibraryPathFromDirectory("x86");
            Assert.IsNotNull(libPath);
            Assert.IsTrue(File.Exists(libPath));
            FileInfo libFileInfo = new FileInfo(libPath);
            Assert.IsNotNull(libFileInfo);
            Assert.IsTrue(libFileInfo.Length > 1000000);
        }

        [TestMethod()]
        public void x64LibTest()
        {
            string libPath = getLibraryPathFromDirectory("x64");
            Assert.IsNotNull(libPath);
            Assert.IsTrue(File.Exists(libPath));
            FileInfo libFileInfo = new System.IO.FileInfo(libPath);
            Assert.IsNotNull(libFileInfo);
            Assert.IsTrue(libFileInfo.Length > 1000000);
        }

        [TestMethod()]
        public void PackerTestSevenZip7Zip()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(testRessources, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_encrypted.7z"));
            string[] filesInSrcFolder = Directory.GetFiles(Path.Combine(this.testRessources, "tozip"));
            string password = "password";

            Packer myPacker = new Packer();
            myPacker.setPackerType(Packer.PackerType.SEVEN_ZIP);
            myPacker.setDstFilename(targetFile);
            myPacker.addFilesToList(filesInSrcFolder);
            myPacker.pack(password);
            Assert.IsTrue(File.Exists(targetFile), "Target file does not exist");
        }

        [TestMethod()]
        public void PackerTestSevenZipZip()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(testRessources, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_encrypted.zip"));
            string[] filesInSrcFolder = Directory.GetFiles(Path.Combine(this.testRessources, "tozip"));
            string password = "password";

            Packer myPacker = new Packer();
            myPacker.setPackerType(Packer.PackerType.ZIP);
            myPacker.setDstFilename(targetFile);
            myPacker.addFilesToList(filesInSrcFolder);
            myPacker.pack(password);
            Assert.IsTrue(File.Exists(targetFile), "Target file does not exist");
        }

        [TestMethod()]
        public void DirCompress()
        {
            // Set source and target folders
            string targetFolder = testRessources;
            string srcFolder = testRessources + Path.DirectorySeparatorChar + "tozip" + Path.DirectorySeparatorChar;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + ".7z"));

            if (File.Exists(targetFile)){File.Delete(targetFile);}

            Assert.IsFalse(File.Exists(targetFile));

            // Specify where 7z.dll DLL is located
            SevenZipCompressor zcomp = new SevenZipCompressor();
            zcomp.CompressionLevel = SevenZip.CompressionLevel.Ultra;
            zcomp.CompressionMethod = CompressionMethod.Lzma;
            // Compress the directory and save the file in a yyyyMMdd_project-files.7z format (eg. 20141024_project-files.7z)
            zcomp.CompressDirectory(srcFolder, targetFile);
            Assert.IsTrue(File.Exists(targetFile));
            FileInfo targetFileInfo = new FileInfo(targetFile);
            Assert.IsNotNull(targetFileInfo);
            Assert.IsTrue(targetFileInfo.Length > 2000);
        }

        [TestMethod()]
        public void FilesCompress()
        {
            // Set source and target folders
            string targetFolder = testRessources;
            string srcFolder = testRessources + Path.DirectorySeparatorChar + "tozip" + Path.DirectorySeparatorChar;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_"+ method + ".7z"));

            if (File.Exists(targetFile)) { File.Delete(targetFile); }
            Assert.IsFalse(File.Exists(targetFile));
            SevenZipCompressor zcomp = new SevenZipCompressor();
            Assert.IsNotNull(zcomp);
            zcomp.CompressFiles(targetFile,Directory.GetFiles(srcFolder));
            Assert.IsTrue(File.Exists(targetFile));
            FileInfo targetFileInfo = new FileInfo(targetFile);
            Assert.IsNotNull(targetFileInfo);
            Assert.IsTrue(targetFileInfo.Length > 2000);
        }

        [TestMethod()]
        public void FilesEncryptZip()
        {
            string password = "password";
            // Set source and target folders
            string targetFolder = testRessources;
            string srcFolder = testRessources + Path.DirectorySeparatorChar + "tozip" + Path.DirectorySeparatorChar;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + ".zip"));

            if (File.Exists(targetFile)) { File.Delete(targetFile); }
            Assert.IsFalse(File.Exists(targetFile));
            SevenZipCompressor zcomp = new SevenZipCompressor();
            zcomp.ZipEncryptionMethod = ZipEncryptionMethod.Aes256;
            zcomp.ArchiveFormat = OutArchiveFormat.Zip;

            Assert.IsNotNull(zcomp);

            zcomp.CompressFilesEncrypted(targetFile, password , Directory.GetFiles(srcFolder));

            Assert.IsTrue(File.Exists(targetFile));
            FileInfo targetFileInfo = new FileInfo(targetFile);
            Assert.IsNotNull(targetFileInfo);
            Assert.IsTrue(targetFileInfo.Length > 2000);
        }


        [TestMethod()]
        public void libFeatures()
        {
            LibraryFeature featuresExtract = SevenZipBase.CurrentLibraryFeatures;
            string featStringExtract = ((uint)featuresExtract).ToString("X6");
            Console.WriteLine("Extractor Features: " + featStringExtract);
            Assert.IsTrue(featuresExtract.HasFlag(SevenZip.LibraryFeature.Extract7zAll));
        }

        [TestMethod()]
        public void RegistryTest()
        {
            const string machineRoot = "HKEY_LOCAL_MACHINE";
            string sZipRegistry = Path.Combine(machineRoot, "SOFTWARE", "7-Zip");
            string resx86 = (String) Registry.GetValue(sZipRegistry, "Path", null);
            string resx64 = (String) Registry.GetValue(sZipRegistry, "Path64", null);
            if (getArchitecture().Equals("x86"))
            {
                Assert.IsNotNull(resx86);
                Assert.IsNull(resx64);
            } else
            {
                Assert.IsNull(resx86);
                Assert.IsNotNull(resx64);
            }
        }

        public string getArchitecture()
        {
            var result = "";
            if (IntPtr.Size == 8)
                result = "x64";
            else
                result = "x86";
            return result;
        }

        public string getLibraryPathFromDirectory(string architecture)
        {
            string progPath = null;
            if (architecture.ToLower().Equals("x86"))
            {
                progPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            }
            else if (architecture.ToLower().Equals("x64"))
            {
                progPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            } else
            {
                throw new System.ArgumentException(String.Format("Architecture {0} not supported. Must be 'x86' or 'x64'",architecture),architecture);
            }
            return Path.Combine(progPath, "7-Zip", "7z.dll"); ;
        }

       
    }
}