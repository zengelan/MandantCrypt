using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MandantCrypt
{
    static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    public class QpdfAdapter
    {
        public const string QPDF_LIB_PATH = @"lib\qpdf\";
        public const string QPDF_DLL_NAME = "qpdf21.dll";
        private string qpdfLib;
        private bool initialized = false;
        private bool writeInitialized = false;
        private bool qdfMode = false;
        private readonly QPDF_OBJECT_STREAM_E objectStreamMode = QPDF_OBJECT_STREAM_E.qpdf_o_generate;
        IntPtr ctx = IntPtr.Zero;

        public QpdfAdapter(string dllPath = null)
        {
            // initilaize object
            if (dllPath != null)
            {
                this.qpdfLib = Path.Combine(dllPath, QPDF_DLL_NAME);
            }
            else
            {
                this.qpdfLib = Path.Combine(QPDF_LIB_PATH, QPDF_DLL_NAME); ;
            }
            // some checks:
            if (!File.Exists(this.qpdfLib))
            {
                throw new DllNotFoundException("Could not find qpdf library at " + Path.GetFullPath(this.qpdfLib));
            }
            //load the library so we have it in memory for the external calls and don't have to worry about the import path
            IntPtr pDll = NativeMethods.LoadLibrary(this.qpdfLib);
            if (pDll == IntPtr.Zero)
            {
                throw new MissingMethodException("Found, but could not load qpdf library from " + Path.GetFullPath(this.qpdfLib)); ;
            }
        }

        ~QpdfAdapter()  // destructor
        {
            cleanup();
        }

        public void setQdfMode(bool enable)
        {
            this.qdfMode = enable;
        }

        public bool getPdfMode()
        {
            return this.qdfMode;
        }

        public void cleanup()
        {
            if (ctx != IntPtr.Zero)
            {
                qpdf_cleanup(ref ctx);
            }
            initialized = false;
            writeInitialized = false;
        }

        public bool isInitialized()
        {
            if (initialized && ctx != IntPtr.Zero)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public IntPtr getContext()
        {
            return ctx;
        }

        public string getCurrentLibraryPath()
        {
            return this.qpdfLib;
        }

        public Qpdf_version_s get_qpdf_version()
        {
            string vstr = get_qpdf_version_string();
            string[] version = vstr.Split('.');
            Qpdf_version_s v = new Qpdf_version_s(Int32.Parse(version[0]), Int32.Parse(version[1]), Int32.Parse(version[2]));
            return v;
            
        }

        public string get_qpdf_version_string()
        {
            IntPtr verTextPtr = qpdf_get_qpdf_version();
            return Marshal.PtrToStringAnsi(verTextPtr);
        }

        public bool init()
        {
            if (!initialized || ctx == IntPtr.Zero)
            {
                ctx = qpdf_init();
                initialized = true;
            }
            return true;
        }

        private void initCheck()
        {
            if (!isInitialized())
            {
                throw new FileLoadException("No file loaded");
            }
        }

        private void writeInitCheck()
        {
            if (!writeInitialized)
            {
                throw new ApplicationException("initWrite must be called before setting encryption options");
            }
        }

    public bool has_error()
        {
            initCheck();
            return qpdf_has_error(ctx);
        }

        public IntPtr get_error()
        {
            initCheck();
            return qpdf_get_error(ctx);
        }

        public int get_error_code()
        {
            initCheck();
            QPDF_ERROR_CODE_E errorEnum = qpdf_get_error_code_enum(ctx, get_error());
            return (int)errorEnum;
        }

        public int get_error_code(IntPtr errIn)
        {
            initCheck();
            QPDF_ERROR_CODE_E errorEnum = qpdf_get_error_code_enum(ctx, errIn);
            return (int)errorEnum;
        }

        public QPDF_ERROR_CODE_E get_error_code_enum()
        {
            initCheck();
            return qpdf_get_error_code_enum(ctx, get_error());
        }

        public QPDF_ERROR_CODE_E get_error_code_enum(IntPtr errIn)
        {
            initCheck();
            return qpdf_get_error_code_enum(ctx, errIn);
        }

        public string get_error_full_text()
        {
            initCheck();
            IntPtr stringPtr = qpdf_get_error_full_text(ctx, get_error());
            return Marshal.PtrToStringAnsi(stringPtr);
        }

        public string get_error_full_text(IntPtr errIn)
        {
            initCheck();
            IntPtr stringPtr = qpdf_get_error_full_text(ctx, errIn);
            return Marshal.PtrToStringAnsi(stringPtr);
        }

        private QPDF_ERROR_CODE_E qpdf_get_error_code_enum(IntPtr qpdf, IntPtr qpdfError)
        {
            initCheck();
            int errorCode = qpdf_get_error_code(qpdf, qpdfError);
            return (QPDF_ERROR_CODE_E)errorCode;
        }

        public bool isEncrypted()
        {
            initCheck();
            return qpdf_is_encrypted(ctx);
        }

        public void setAes256EncryptionOptions(string userPassword, string ownerPassword, QPDF_R3_PRINT_E printOption, QPDF_R3_MODIFY_E modifyOption, bool extractAllowed, bool encryptMetadata)
        {
            initCheck();
            writeInitCheck();
            int po = (int) printOption;
            int mo = (int) modifyOption;
            int ea = extractAllowed ? 1 : 0;
            int em = encryptMetadata ? 1 : 0;
            qpdf_set_r6_encryption_parameters(ctx, userPassword, ownerPassword, 1, ea, po, mo, em);
        }

        public void setRestrictiveAes256EncryptionOptions(string userAndOwnerPassword)
        {
            QPDF_R3_PRINT_E printOption = QPDF_R3_PRINT_E.qpdf_r3p_none;
            QPDF_R3_MODIFY_E modifyOption = QPDF_R3_MODIFY_E.qpdf_r3m_none;
            bool extractAllowed = false;
            bool encryptMetadata = false;
            setAes256EncryptionOptions(userAndOwnerPassword, userAndOwnerPassword, printOption, modifyOption, extractAllowed, encryptMetadata);
        }


        // structs and enums

        public struct Qpdf_version_s
        {
            public int major,minor,subminor;

            public Qpdf_version_s(int major, int minor, int subminor)
            {
                this.major = major;
                this.minor = minor;
                this.subminor = subminor;
            }
        }

        public int initWrite(string targetFile)
        {
            initCheck();
            int result = qpdf_init_write(ctx, targetFile);
            writeInitialized = result == 0 ? true : false;
            return result;

        }

        public int writeSimple(string targetFile, bool compressed = true)
        {
            initCheck();
            //call init_write
            int result = initWrite(targetFile);
            if (result != 0) return result;            
            return write(compressed);
        }

        public int write(bool compressed = true)
        {
            initCheck();
            writeInitCheck();
            //set write options
            qpdf_set_qdf_mode(ctx, qdfMode ? 1 : 0);
            qpdf_set_object_stream_mode(ctx, (int)objectStreamMode);
            qpdf_set_compress_streams(ctx, compressed ? 1 : 0);
            qpdf_set_newline_before_endstream(ctx, 1);
            return qpdf_write(ctx);
        }

        public int read(string pdfFile, string password = null)
        {
            initCheck();
            int errorCode = 1;
            if (password != null && password.Length > 1){
                errorCode = qpdf_read(this.ctx, pdfFile, password);
            }
            else
            {
                errorCode = qpdf_read(this.ctx, pdfFile, null);
            }
            return errorCode;
        }

        public enum QPDF_ERROR_CODE_E
        {
            qpdf_e_success = 0,
            qpdf_e_internal,        /* logic/programming error -- indicates bug */
            qpdf_e_system,          /* I/O error, memory error, etc. */
            qpdf_e_unsupported,     /* PDF feature not (yet) supported by qpdf */
            qpdf_e_password,        /* incorrect password for encrypted file */
            qpdf_e_damaged_pdf,     /* syntax errors or other damage in PDF */
            qpdf_e_pages,           /* erroneous or unsupported pages structure */
        };

        /* Write Parameters. See QPDFWriter.hh for details. */
        public enum QPDF_OBJECT_STREAM_E
        {
            qpdf_o_disable = 0,     /* disable object streams */
            qpdf_o_preserve,        /* preserve object streams */
            qpdf_o_generate         /* generate object streams */
        };

        /* Stream data flags */
        /* See pipeStreamData in QPDFObjectHandle.hh for details on these flags. */
        public enum QPDF_STREAM_ENCODE_FLAGS_E
        {
            qpdf_ef_compress = 1 << 0, /* compress uncompressed streams */
            qpdf_ef_normalize = 1 << 1, /* normalize content stream */
        };
        public enum QPDF_STREAM_DECODE_LEVEL_E
        {
            /* These must be in order from less to more decoding. */
            qpdf_dl_none = 0,           /* preserve all stream filters */
            qpdf_dl_generalized,        /* decode general-purpose filters */
            qpdf_dl_specialized,        /* also decode other non-lossy filters */
            qpdf_dl_all                 /* also decode loss filters */
        };

        /* R3 Encryption Parameters */
        public enum QPDF_R3_PRINT_E
        {
            qpdf_r3p_full = 0,      /* allow all printing */
            qpdf_r3p_low,       /* allow only low-resolution printing */
            qpdf_r3p_none       /* allow no printing */
        };
        public enum QPDF_R3_MODIFY_E       /* Allowed changes: */
        {
            qpdf_r3m_all = 0,       /* General editing, comments, forms */
            qpdf_r3m_annotate,          /* Comments, form field fill-in, and signing */
            qpdf_r3m_form,      /* form field fill-in and signing */
            qpdf_r3m_assembly,      /* only document assembly */
            qpdf_r3m_none       /* no modifications */
        };

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr qpdf_get_qpdf_version();

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr qpdf_init();

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_cleanup(ref IntPtr qpdf);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool qpdf_has_error(IntPtr qpdf);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool qpdf_is_encrypted(IntPtr qpdf);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr qpdf_get_error(IntPtr qpdf);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr qpdf_get_error_full_text(IntPtr qpdf, IntPtr error);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        static extern int qpdf_get_error_code(IntPtr qpdf, IntPtr qpdfError);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int qpdf_read(IntPtr qpdfdata, [MarshalAs(UnmanagedType.LPStr)] string fileName, [MarshalAs(UnmanagedType.LPStr)] string password);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_set_object_stream_mode(IntPtr qpdf, int mode);

        /*
         * deprecated, see QPDFWriter.hh, replaced by qpdf_set_compress_streams
        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_set_stream_data_mode(IntPtr qpdf, int mode);
        */

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_set_qdf_mode(IntPtr qpdf, int trueFalse);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_set_compress_streams(IntPtr qpdf, int trueFalse);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_set_newline_before_endstream(IntPtr qpdf, int trueFalse);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int qpdf_init_write(IntPtr qpdf, [MarshalAs(UnmanagedType.LPStr)] string fileName);

        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int qpdf_write(IntPtr qpdf);

        /* R6 encryption is for AES with 256 bits, see https://github.com/qpdf/qpdf/blob/master/qpdf/qpdf.cc#L2190
         * allow_accessibility is always ignored
         */
        [DllImport(QPDF_DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern void qpdf_set_r6_encryption_parameters(IntPtr qpdf, [MarshalAs(UnmanagedType.LPStr)] string userPassword, [MarshalAs(UnmanagedType.LPStr)] string ownerPassword, int allow_accessibility, int allow_extract, int qpdf_r3_print_e, int qpdf_r3_modify_e, int encrypt_metadata);
    }

    

}
