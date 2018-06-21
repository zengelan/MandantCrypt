using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using static MandantCrypt.QpdfAdapter;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using System.Text;

namespace MandantCrypt.Tests
{
    [TestClass()]
    public class iText7PdfTests
    {
        string testRessources;
        string pdfRessources;
        string currentDir;
        string outDir;


        [TestInitialize()]
        public void Initialize()
        {
            this.currentDir = Directory.GetCurrentDirectory();
            this.testRessources = Path.GetFullPath(currentDir + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "Testresources");
            this.pdfRessources = testRessources + Path.DirectorySeparatorChar + "pdf" + Path.DirectorySeparatorChar;
            this.outDir = Path.GetFullPath(currentDir + "testout");
            Directory.CreateDirectory(outDir);
        }

        [TestMethod()]
        public void PackerTestiText7()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile= Path.Combine(outDir, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_packered.pdf"));
            string[] filesInSrcFolder = Directory.GetFiles(Path.Combine(this.testRessources,"tozip"));
            string password = "password";

            Packer myPacker = new Packer();
            myPacker.setPackerType(Packer.PackerType.ITEXT7);
            myPacker.setDstFilename(targetFile);
            myPacker.addFilesToList(filesInSrcFolder);
            myPacker.pack(password);
            Assert.IsTrue(File.Exists(targetFile),"Target file does not exist");
        }

        [TestMethod()]
        public void PackerTestSingleiText7()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(outDir, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_packered.pdf"));
            string singleFile = Path.Combine(outDir, "tozip", "2 CCNs in here.docx");
            string password = "password";

            Packer myPacker = new Packer();
            myPacker.setPackerType(Packer.PackerType.ITEXT7);
            myPacker.setDstFilename(targetFile);
            myPacker.addFilesToList(singleFile);
            myPacker.pack(password);
            Assert.IsTrue(File.Exists(targetFile), "Target file does not exist");
        }


        [TestMethod()]
        public void PackerTestNoneiText7()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(outDir, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_packered.pdf"));
            string password = "password";

            Packer myPacker = new Packer();
            myPacker.setPackerType(Packer.PackerType.ITEXT7);
            myPacker.setDstFilename(targetFile);
            myPacker.pack(password);
            Assert.IsTrue(File.Exists(targetFile), "Target file does not exist");
        }

        [TestMethod()]
        public void PackerTestSinglePlainiText7()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(outDir, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_packered.pdf"));
            string singleFile = Path.Combine(outDir, "tozip", "2 CCNs in here.docx");

            Packer myPacker = new Packer();
            myPacker.setAllowUnencryptedOutput(true);
            myPacker.setPackerType(Packer.PackerType.ITEXT7);
            myPacker.setDstFilename(targetFile);
            myPacker.addFilesToList(singleFile);
            myPacker.pack(null);
            Assert.IsTrue(File.Exists(targetFile), "Target file does not exist");
        }

        [TestMethod()]
        public void PackerTestNonePlainiText7()
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(outDir, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_packered.pdf"));

            Packer myPacker = new Packer();
            myPacker.setAllowUnencryptedOutput(true);
            myPacker.setPackerType(Packer.PackerType.ITEXT7);
            myPacker.setDstFilename(targetFile);
            myPacker.pack(null);
            Assert.IsTrue(File.Exists(targetFile), "Target file does not exist");
        }

        [TestMethod()]
        public void CompressedPdfiText()
        {
            string inPdf = "noattach.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = outDir;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFileComp = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_compressed.pdf"));
            string targetFileUnComp = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_uncompressed.pdf"));

            Assert.IsTrue(File.Exists(sourcePdfFile), "Source file doesn't exist");

            if (File.Exists(targetFileComp)) File.Delete(targetFileComp);
            Assert.IsFalse(File.Exists(targetFileComp));
            if (File.Exists(targetFileUnComp)) File.Delete(targetFileUnComp);
            Assert.IsFalse(File.Exists(targetFileUnComp));


            // read file
            WriterProperties propComp = new WriterProperties();
            propComp.SetFullCompressionMode(true);
            PdfDocument origPdf = new PdfDocument(new PdfReader(sourcePdfFile), new PdfWriter(targetFileComp, propComp));
            Assert.IsNotNull(origPdf);

            PdfWriter theCompWriter = origPdf.GetWriter();
            origPdf.Close();

            Assert.IsTrue(File.Exists(targetFileComp), "target file comp does not exist");

            // read file
            PdfDocument origPdfAgain = new PdfDocument(new PdfReader(sourcePdfFile), new PdfWriter(targetFileUnComp));
            Assert.IsNotNull(origPdfAgain);
            origPdfAgain.GetWriter().SetCompressionLevel(CompressionConstants.NO_COMPRESSION);
            PdfWriter theUncompWriter = origPdfAgain.GetWriter();
            origPdfAgain.Close();

            Assert.IsTrue(File.Exists(targetFileUnComp), "target file uncomp does not exist");

            Assert.IsTrue(new FileInfo(targetFileComp).Length < new FileInfo(targetFileUnComp).Length, "Uncompressed file is not smaller than the compressed file");

        }

        [TestMethod()]
        public void AttachFileiText()
        {
            string inPdf = "noattach.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = outDir;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + ".pdf"));

            Assert.IsTrue(File.Exists(sourcePdfFile), "Source file doesn't exist");

            if (File.Exists(targetFile)) File.Delete(targetFile);
            Assert.IsFalse(File.Exists(targetFile));

            WriterProperties propComp = new WriterProperties();
            propComp.SetFullCompressionMode(false);
            propComp.SetCompressionLevel(CompressionConstants.NO_COMPRESSION);
            PdfWriter theCompWriter = new PdfWriter(targetFile, propComp);
            PdfDocument origPdf = new PdfDocument(new PdfReader(sourcePdfFile), theCompWriter);
            Assert.IsNotNull(origPdf);

            PdfFileSpec spec = PdfFileSpec.CreateEmbeddedFileSpec(origPdf, Encoding.ASCII.GetBytes("Some text in the attached document"), "description of attachment here", "test.txt", null, null, null);
            origPdf.AddFileAttachment("attadescr", spec);
            origPdf.Close();
            Assert.IsTrue(new FileInfo(sourcePdfFile).Length < new FileInfo(targetFile).Length, "target file is not bigger than the source file");
        }

        [TestMethod()]
        public void PackerFilenameiText()
        {
            string targetFolder = outDir;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + ".dmy"));

            Packer myPacker = new Packer();
            myPacker.setPackerType(Packer.PackerType.ITEXT7);
            myPacker.setDstFilename(targetFile);
            string suggestedFilename = myPacker.getDstFilenameWithProperExtension();
            Assert.AreNotEqual(suggestedFilename, targetFile, "chosen and suggested filenames are equal but shouldn't be");
            Assert.IsTrue(suggestedFilename.EndsWith(".pdf"),"Suggested filename does not end with PDF");
            Assert.AreEqual(suggestedFilename.Substring(0, suggestedFilename.Length - 3), targetFile.Substring(0, targetFile.Length - 3), "filenames without extension are different");
        }

        [TestMethod()]
        public void EncryptiText()
        {
            string password = "password";
            string inPdf = "Skyhigh-Secure-Datasheet-0214.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = outDir;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_encrypted.pdf"));

            Assert.IsTrue(File.Exists(sourcePdfFile), "Source file doesn't exist");

            if (File.Exists(targetFile)) File.Delete(targetFile);
            Assert.IsFalse(File.Exists(targetFile));

            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);

            WriterProperties propComp = new WriterProperties();
            propComp.SetFullCompressionMode(false);
            propComp.SetCompressionLevel(CompressionConstants.NO_COMPRESSION);
            propComp.SetStandardEncryption(passwordBytes, passwordBytes, 0, EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
            PdfWriter theCompWriter = new PdfWriter(targetFile, propComp);
            PdfDocument origPdf = new PdfDocument(new PdfReader(sourcePdfFile), theCompWriter);
            Assert.IsNotNull(origPdf);
            origPdf.Close();
            Assert.IsTrue(new FileInfo(sourcePdfFile).Length < new FileInfo(targetFile).Length, "target file is not bigger than the source file");

            //check out http://itextsupport.com/apidocs/itext7/latest/com/itextpdf/kernel/pdf/PdfEncryptedPayloadDocument.html  
        }
        /*
        [TestMethod()]
        public void EncryptPayloadiText()
        {
            string password = "password";
            string inPdf = "noattach.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = testRessources;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_encryptedPayload.pdf"));

            Assert.IsTrue(File.Exists(sourcePdfFile), "Source file doesn't exist");

            if (File.Exists(targetFile)) File.Delete(targetFile);
            Assert.IsFalse(File.Exists(targetFile));

            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);

            WriterProperties propComp = new WriterProperties();
            propComp.SetFullCompressionMode(false);
            propComp.SetCompressionLevel(CompressionConstants.NO_COMPRESSION);
            //propComp.SetStandardEncryption(passwordBytes, passwordBytes, 0, EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
            PdfWriter theCompWriter = new PdfWriter(targetFile, propComp);
            PdfDocument origPdf = new PdfDocument(new PdfReader(sourcePdfFile), theCompWriter);
            Assert.IsNotNull(origPdf);

            // attach an encrypted payload
            PdfFileSpec spec = PdfFileSpec.CreateEmbeddedFileSpec(origPdf, Encoding.ASCII.GetBytes("Some text in the attached document"), "description of attachment here", "test.txt", null, null, null);
            origPdf.SetEncryptedPayload(spec);

            origPdf.Close();
            Assert.IsTrue(new FileInfo(sourcePdfFile).Length < new FileInfo(targetFile).Length, "target file is not bigger than the source file");

            //check out http://itextsupport.com/apidocs/itext7/latest/com/itextpdf/kernel/pdf/PdfEncryptedPayloadDocument.html  

        }
        */

    }
}