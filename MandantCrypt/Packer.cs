using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using log4net;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MandantCrypt
{
    public class Packer
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string templatePdfFileName;
        private HashSet<string> srcFileList;
        private string dstFileName;
        private PackerType packerType = PackerType.ZIP;
        private bool overwrite = true;

        public enum PackerType
        {
            ZIP = 0,
            SEVEN_ZIP,         
            QPDF,
            ITEXT7
        };

        private static Dictionary<PackerType, String> Extensions = new Dictionary<PackerType, String>
        {
            { PackerType.ZIP, "zip" },
            { PackerType.SEVEN_ZIP, "7z" },
            { PackerType.QPDF, "pdf" },
            { PackerType.ITEXT7, "pdf" },
        };

        public void setOverwrite(bool overwrite)
        {
            this.overwrite = overwrite;
        }

        public void setTemplatePdfFileName(string templatePdfFileName)
        {
            this.templatePdfFileName = templatePdfFileName;
        }

        public FileInfo getDstFileInfo()
        {
            return new FileInfo(this.dstFileName);
        }

        public bool doesDstFileExist()
        {
            return File.Exists(this.dstFileName);
        }

        public Packer()
        {
            srcFileList = new HashSet<string>();
        }

        public void setPackerType(PackerType type)
        {
            this.packerType = type;
        }

        public void addFilesToList(string[] files)
        {
            srcFileList.UnionWith(files);
        }

        public void addFileToList(string file)
        {
            srcFileList.Add(file);
        }

        public void setDstFilename (string dstFile)
        {
            this.dstFileName = dstFile;
        }

        public string getDstFilenameWithProperExtension()
        {
            string fileNoExtension = Path.GetFileNameWithoutExtension(this.dstFileName);
            string dstDir = Path.GetDirectoryName(this.dstFileName);
            return Path.Combine(dstDir, fileNoExtension + "." + Extensions[this.packerType]);
        }

        private bool deleteIfExist(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                return true;
            }
            return false;
        }

        public bool pack(string password)
        {
            switch (this.packerType)
            {
                case PackerType.ZIP:
                    return createEncryptedZip(password);
                case PackerType.SEVEN_ZIP:
                    return createEncryptedSevenZip(password);
                case PackerType.ITEXT7:
                    return createEncryptediText7Pdf(password);
                default:
                    throw new NotImplementedException("This PackerType is not implemented");
            }
        }

        private bool createEncryptedSevenZipPackage(string password, OutArchiveFormat format)
        {
            bool success = false;
            if (this.overwrite) deleteIfExist(this.dstFileName);

            SevenZipCompressor zcomp = new SevenZipCompressor();
            zcomp.ZipEncryptionMethod = ZipEncryptionMethod.Aes256;
            zcomp.EncryptHeaders = true;
            zcomp.ArchiveFormat = format;

            try
            {
                zcomp.CompressFilesEncrypted(this.dstFileName, password, this.srcFileList.ToArray());
            }
            catch (Exception e)
            {
                log.Error($"Exception while trying to pack with SevenZipCompressor", e);
                return false;
            }
            return success;
        }

        private bool createEncryptedZip(string password)
        {
            return createEncryptedSevenZipPackage(password, OutArchiveFormat.Zip);
        }

        private bool createEncryptedSevenZip(string password)
        {
            return createEncryptedSevenZipPackage(password, OutArchiveFormat.SevenZip);
        }

        private Stream getTemplatePdfStreamFromResources()
        {
            //var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = "MandantCrypt.Properties.Resources.template.pdf";
            //string [] resources = assembly.GetManifestResourceNames();
            return new MemoryStream(MandantCrypt.Properties.Resources.templatepdf);


        }

        private Stream getTemplatePdfStream()
        {
            if (File.Exists(this.templatePdfFileName))
            {
                return new FileStream(this.templatePdfFileName, FileMode.Open);
            }
            else
            {
                return getTemplatePdfStreamFromResources();
            }
        }

        private bool createEncryptediText7Pdf(string password)
        {
            bool success = false;
            if (this.overwrite) deleteIfExist(this.dstFileName);

            
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);

            try
            {
                Stream stream = getTemplatePdfStream();
                if (stream == null)
                {
                    throw new FileNotFoundException("Could not read the PDF template");
                }

                WriterProperties propComp = new WriterProperties();
                propComp.SetFullCompressionMode(true);
                propComp.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
                propComp.SetStandardEncryption(passwordBytes, passwordBytes, 0, EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
                PdfWriter theCompWriter = new PdfWriter(this.dstFileName, propComp);
                PdfDocument newPdf = new PdfDocument(new PdfReader(stream), theCompWriter);

                foreach (string file in this.srcFileList)
                {
                    string filename = Path.GetFileName(file);
                    PdfFileSpec fileSpec = PdfFileSpec.CreateEmbeddedFileSpec(
                        newPdf, File.ReadAllBytes(file), filename, filename, null, null, null);
                    newPdf.AddFileAttachment(filename, fileSpec);
                }
                newPdf.Close();
            }
            catch (Exception e)
            {
                log.Error($"Exception while trying to pack with iText7", e);
                return false;
            }
            return success;
        }

    }
}
