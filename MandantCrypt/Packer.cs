using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using log4net;
using Newtonsoft.Json;
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
        private const string BUILTIN_TEMPLATE_RESSOURCE_NAME = "templatepdf";
        private string templatePdfFileName;
        private HashSet<string> srcFileList;
        private string dstFileName;
        private PackerType packerType = PackerType.ZIP;
        private bool overwrite = true;
        private bool allowUnencryptedOutput = false;

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
            log.Info($"Overwrite flag was set to '{this.overwrite}'");
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
            log.Info($"Packer type was set to '{this.packerType}'");
        }

        public void addFilesToList(string[] files)
        {
            srcFileList.UnionWith(files);
            log.Debug("Added files to list, current list: " + JsonConvert.SerializeObject(this.srcFileList));
        }

        public void addFilesToList(string file)
        {
            addFileToList(file);
        }

        public void addFileToList(string file)
        {
            srcFileList.Add(file);
            log.Debug("Added single file to list, current list: " + JsonConvert.SerializeObject(this.srcFileList));
        }

        public void setDstFilename (string dstFile)
        {
            this.dstFileName = dstFile;
            log.Debug($"Destination filename was set to '{this.dstFileName}'");
        }

        public string getDstFilenameWithProperExtension()
        {
            string fileNoExtension = Path.GetFileNameWithoutExtension(this.dstFileName);
            log.Debug($"Filename without extension is '{fileNoExtension}'");
            string dstDir = Path.GetDirectoryName(this.dstFileName);
            log.Debug($"Destination directory is '{dstDir}'");
            string updateFilename = Path.Combine(dstDir, fileNoExtension + "." + Extensions[this.packerType]);
            log.Debug($"Filename with proper extension is '{updateFilename}'");
            return updateFilename;
        }

        private bool deleteIfExist(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                log.Info($"Existing file was deleted: [{file}]");
                return true;
            }
            return false;
        }

        public void setAllowUnencryptedOutput(bool allow)
        {
            this.allowUnencryptedOutput = allow;
            log.Warn($"AllowUnencryptedOutput was set to '{this.allowUnencryptedOutput}'");
        }

        public bool pack(string password)
        {
            if (! allowUnencryptedOutput && String.IsNullOrEmpty(password))
            {
                log.Error("No password was specified");
                throw new ArgumentNullException("You have to specify a password as only encryted output is allowed");
            }
            log.Info($"Going to pack the following files to an archive named '{this.dstFileName}' of type '{this.packerType}': " + JsonConvert.SerializeObject(this.srcFileList));
            switch (this.packerType)
            {
                case PackerType.ZIP:
                    return createZip(password);
                case PackerType.SEVEN_ZIP:
                    return createSevenZip(password);
                case PackerType.ITEXT7:
                    return createiText7Pdf(password);
                default:
                    log.Error($"The specified packer type {this.packerType} is not (yet) implemented");
                    throw new NotImplementedException("This PackerType is not implemented");
            }
        }

        private bool createSevenZipPackage(string password, OutArchiveFormat format)
        {
            bool success = false;
            log.Info($"Using SevenZip library to create a package with format [{format}]");
            if (this.overwrite) deleteIfExist(this.dstFileName);

            SevenZipCompressor zcomp = new SevenZipCompressor();
            zcomp.ArchiveFormat = format;

            try
            {
                if (String.IsNullOrEmpty(password))
                {
                    log.Warn($"No password was supplied, so the output file will be unencrypted [{this.dstFileName}]");
                    zcomp.CompressFiles(this.dstFileName, this.srcFileList.ToArray());
                }
                else
                {
                    zcomp.ZipEncryptionMethod = ZipEncryptionMethod.Aes256;
                    log.Info("Enabled ZIP AES_256 encryption");
                    zcomp.EncryptHeaders = true;
                    zcomp.CompressFilesEncrypted(this.dstFileName, password, this.srcFileList.ToArray());
                }
            }
            catch (Exception e)
            {
                log.Error($"Exception while trying to pack with SevenZipCompressor", e);
                return false;
            }
            log.Info($"Successfully packed the files into [{this.dstFileName}]");
            return success;
        }

        private bool createZip(string password)
        {
            return createSevenZipPackage(password, OutArchiveFormat.Zip);
        }

        private bool createSevenZip(string password)
        {
            return createSevenZipPackage(password, OutArchiveFormat.SevenZip);
        }

        private Stream getTemplatePdfStreamFromResources()
        {
            log.Debug($"Getting template PDF from ressource named '{BUILTIN_TEMPLATE_RESSOURCE_NAME}'");
            byte[] templatePdf = (byte[]) Properties.Resources.ResourceManager.GetObject(BUILTIN_TEMPLATE_RESSOURCE_NAME);
            log.Debug($"Template PDF from ressource filesize is {templatePdf.Length} bytes");
            return new MemoryStream(templatePdf);
        }

        private Stream getTemplatePdfStream()
        {
            if (File.Exists(this.templatePdfFileName))
            {
                log.Debug($"Reading Template PDF from file {this.templatePdfFileName}");
                return new FileStream(this.templatePdfFileName, FileMode.Open);
            }
            else
            {
                log.Info("Using built-in Template PDF, consider using a customized version");
                return getTemplatePdfStreamFromResources();
            }
        }

        private bool createiText7Pdf(string password = null)
        {
            bool success = false;
            log.Info($"Using iText7 library to create a package with format [{this.packerType}]");
            if (this.overwrite) deleteIfExist(this.dstFileName);

            try
            {
                Stream stream = getTemplatePdfStream();
                if (stream == null)
                {
                    log.Error("Could not read the PDF Template from stream");
                    throw new FileNotFoundException("Could not read the PDF template");
                }

                WriterProperties propComp = new WriterProperties();
                propComp.SetFullCompressionMode(true);
                propComp.SetCompressionLevel(CompressionConstants.BEST_COMPRESSION);
                if (String.IsNullOrEmpty(password))
                {
                    log.Warn($"No password was supplied, so the output file will be unencrypted [{this.dstFileName}]");
                }
                else
                {
                    byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
                    propComp.SetStandardEncryption(passwordBytes, passwordBytes, 0, EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
                    log.Info("Enabled PDF AES_256 encryption");
                }
                PdfWriter theCompWriter = new PdfWriter(this.dstFileName, propComp);
                PdfDocument newPdf = new PdfDocument(new PdfReader(stream), theCompWriter);
                foreach (string file in this.srcFileList)
                {
                    string filename = Path.GetFileName(file);
                    PdfFileSpec fileSpec = PdfFileSpec.CreateEmbeddedFileSpec(
                        newPdf, File.ReadAllBytes(file), filename, filename, null, null, null);
                    newPdf.AddFileAttachment(filename, fileSpec);
                    log.Debug($"Added file '{filename}' as attachment to pdf");
                }
                newPdf.Close();
            }
            catch (Exception e)
            {
                log.Error($"Exception while trying to pack with iText7", e);
                return false;
            }
            log.Info($"Successfully packed the files into [{this.dstFileName}]");
            return success;
        }

    }
}
