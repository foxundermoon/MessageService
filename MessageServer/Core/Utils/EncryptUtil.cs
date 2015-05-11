using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace MessageService.Core.Utils {
    public static class EncryptUtil {
        const int bufLength=1024;
        public static String EncryptBASE64ByGzip( string input ) {
            return EncryptBASE64(CompressByGzip(EncodeUTF8(input)));
        }
        public static string DecryptBASE64ByGzip( string input ) {
            return DecodeUTF8(DecompressByGzip(DecryptBASE64(input)));
        }
        public static byte[] CompressByGzip( byte[] input ) {
            using(var ims = new MemoryStream()) {
                using(var gzip = new GZipStream(ims, CompressionMode.Compress)) {
                    gzip.Write(input, 0, input.Length);
                }
                return ims.ToArray();
            }
        }
        public static byte[] DecompressByGzip( byte[] input ) {
            using(MemoryStream outms = new MemoryStream()) {
                using(MemoryStream ims = new MemoryStream(input)) {
                    //ims.Write(input, 0, input.Length);
                    using(System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(ims, System.IO.Compression.CompressionMode.Decompress)) {
                        byte[] buf = new byte[1024];
                        int len = 0;
                        //len = gzip.Read(buf, 1, buf.Length);
                        while((len = gzip.Read(buf, 0, buf.Length))>0) {
                            outms.Write(buf, 0, len);
                        }
                        return outms.ToArray();
                    }
                }
            }
        }
        public static byte[] EncodeUTF8( string input ) {
            return UTF8Encoding.UTF8.GetBytes(input);
        }
        public static string DecodeUTF8( byte[] input ) {
            return UTF8Encoding.UTF8.GetString(input);
        }
        public static string EncryptBASE64( byte[] input ) {
            return Convert.ToBase64String(input, 0, input.Length, Base64FormattingOptions.None);
        }
        public static byte[] DecryptBASE64( string input ) {
            return Convert.FromBase64String(input);
        }
    }
}
