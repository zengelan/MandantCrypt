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


        [TestInitialize()]
        public void Initialize()
        {
            this.currentDir = Directory.GetCurrentDirectory();
            this.testRessources = Path.GetFullPath(currentDir + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "testressources");
            this.pdfRessources = testRessources + Path.DirectorySeparatorChar + "pdf" + Path.DirectorySeparatorChar;
        }


        [TestMethod()]
        public void CompressedPdfiText()
        {
            string inPdf = "noattach.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = testRessources;
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
            string targetFolder = testRessources;
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
        public void EncryptiText()
        {
            string password = "password";
            string inPdf = "Skyhigh-Secure-Datasheet-0214.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = testRessources;
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
        public void LibTest()
        {
            string libDir = Path.GetDirectoryName(qpdf.getCurrentLibraryPath());
            string[] qpdfLibFiles = { "qpdf21.dll" };
            foreach (string libFile in qpdfLibFiles)
            {
                string myLibFile = Path.Combine(libDir, libFile);
                Trace.WriteLine($"Checking if lib {myLibFile} exists");
                Assert.IsTrue(File.Exists(myLibFile));
                FileInfo qpdfLibInfo = new FileInfo(myLibFile);
                Assert.IsNotNull(qpdfLibInfo);
                Assert.IsTrue(qpdfLibInfo.Length > 10000);
            }
            //check version
            string qpdfVersion = qpdf.get_qpdf_version_string();
            Assert.IsNotNull(qpdfVersion);
            Trace.WriteLine($"QPDFLib version string is {qpdfVersion}");
            Qpdf_version_s v = qpdf.get_qpdf_version();
            Assert.IsNotNull(qpdfVersion);
            Trace.WriteLine($"QPDFLib version struct is: " + JsonConvert.SerializeObject(v));
            Assert.IsTrue(v.major > 7);
        }
        
        [TestMethod()]
        public void ErrorTest()
        {
            //call init
            bool success = qpdf.init();
            Assert.IsTrue(success);
            Assert.IsTrue(qpdf.isInitialized());
            Assert.AreNotEqual(qpdf.getContext(),IntPtr.Zero);
      
            //check for error, we expect one to be there here
            bool hasError = qpdf.has_error();
            Assert.IsFalse(hasError);
            IntPtr errPtr = qpdf.get_error();
            Assert.IsFalse(errPtr != IntPtr.Zero);

            int errorCodeCurr = qpdf.get_error_code(errPtr);
            Assert.AreEqual(errorCodeCurr, 0);

            int errorCode = qpdf.get_error_code();
            Assert.AreEqual(errorCode, 0);

            QPDF_ERROR_CODE_E estructCurr = qpdf.get_error_code_enum();
            Assert.AreEqual(estructCurr, QPDF_ERROR_CODE_E.qpdf_e_success);

            QPDF_ERROR_CODE_E estruct = qpdf.get_error_code_enum(errPtr);
            Assert.AreEqual(estruct, QPDF_ERROR_CODE_E.qpdf_e_success);

            string errorStrCurr = qpdf.get_error_full_text(errPtr);
            Assert.AreEqual(errorStrCurr, "");

            string errorStr = qpdf.get_error_full_text();
            Assert.AreEqual(errorStr, "");

            //call cleanup
            qpdf.cleanup();
            Assert.AreEqual(qpdf.getContext(), IntPtr.Zero);

        }

       

        [TestMethod()]
        public void ConvToQdf()
        {
            string inPdf = "with3FileAttachFoxit.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = testRessources;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFileQdf = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_qdf.qdf"));

            Assert.IsTrue(File.Exists(sourcePdfFile), "Source file doesn't exist");

            if (File.Exists(targetFileQdf)) File.Delete(targetFileQdf);
            Assert.IsFalse(File.Exists(targetFileQdf));

            //call init
            bool success = qpdf.init();
            Assert.IsTrue(success);
            Assert.IsTrue(qpdf.isInitialized());
            Assert.AreNotEqual(qpdf.getContext(), IntPtr.Zero);

            // read file
            int error = qpdf.read(sourcePdfFile);
            Assert.AreEqual(0, error, "Could not read file");
            Assert.IsFalse(qpdf.has_error(), "lib has error after reading");

            //call write uncompressed QDF
            qpdf.setQdfMode(true);
            error = qpdf.writeSimple(targetFileQdf, false);
            Assert.AreEqual(0, error,"Error writing file");
            Assert.IsFalse(qpdf.has_error(), "lib has error after writing");

            Assert.IsTrue(File.Exists(targetFileQdf), "target file uncomp does not exist after write");

            qpdf.cleanup();
            Assert.AreEqual(qpdf.getContext(), IntPtr.Zero);

        }

        [TestMethod()]
        public void PdfEncryptExisting()
        {
            string password = "password";
            string inPdf = "usdl4_match_keyword.pdf";
            string sourcePdfFile = Path.Combine(this.pdfRessources, inPdf);
            // Set source and target folders
            string targetFolder = testRessources;
            string srcFolder = testRessources + Path.DirectorySeparatorChar + "tozip" + Path.DirectorySeparatorChar;
            var method = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string targetFile = Path.Combine(targetFolder, string.Concat(DateTime.Now.ToString("yyyyMMdd"), "_" + method + "_encrypted.pdf"));

            if (File.Exists(targetFile)) File.Delete(targetFile);
            Assert.IsFalse(File.Exists(targetFile));

            //call init
            bool success = qpdf.init();
            Assert.IsTrue(success);
            Assert.IsTrue(qpdf.isInitialized());
            Assert.AreNotEqual(qpdf.getContext(), IntPtr.Zero);

            // read file
            int error = qpdf.read(sourcePdfFile);
            Assert.AreEqual(0, error);
            Assert.IsFalse(qpdf.has_error());

            error = qpdf.initWrite(targetFile);
            Assert.AreEqual(0, error);
            Assert.IsFalse(qpdf.has_error());

            qpdf.setRestrictiveAes256EncryptionOptions(password);
            Assert.IsFalse(qpdf.has_error(),"Error in lib after setting encryption options");

            //call write compressed
            error = qpdf.write(false);
            Assert.AreEqual(0, error);
            Assert.IsFalse(qpdf.has_error());

            Assert.IsTrue(File.Exists(targetFile),"Target file does not exist");
            FileInfo targetFileInfo = new FileInfo(targetFile);
            Assert.IsNotNull(targetFileInfo,"Could not read target file info");
            Assert.IsTrue(targetFileInfo.Length > 20000,"Target file is too small");

            qpdf.cleanup();
            Assert.AreEqual(qpdf.getContext(), IntPtr.Zero);

            // read the file again and check if encrypted
            this.qpdf = new QpdfAdapter();
            success = qpdf.init();
            Assert.IsTrue(success);
            Assert.IsTrue(qpdf.isInitialized());
            Assert.AreNotEqual(qpdf.getContext(), IntPtr.Zero);
            error = qpdf.read(targetFile,password);
            Assert.AreEqual(0, error,"PDF Read failed");
            Assert.IsFalse(qpdf.has_error());

            Assert.IsTrue(qpdf.isEncrypted(), "output file is not showing as encrypted after loading again");
   
            qpdf.cleanup();
            Assert.AreEqual(qpdf.getContext(), IntPtr.Zero);

        }
        */

        /*

              [TestMethod()]
              public void PdfCreate()
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

             

          */

    }
}