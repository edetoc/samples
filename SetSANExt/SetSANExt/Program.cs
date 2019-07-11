using CERTADMINLib;
using CERTENCODELib;
using System.Runtime.InteropServices;

namespace SetSANExt
{
   
    class Program
    {
        static void Main(string[] args)
        {

            CCertAdminClass admin = new CCertAdminClass();
            CCertEncodeAltNameClass altNames = new CCertEncodeAltNameClass();

            altNames.Reset(2);

            altNames.SetNameEntry(0,3,"mail2.domain.com");      // 3 for CERT_ALT_NAME_DNS_NAME

            altNames.SetNameEntry(1,3,"websso.sysfil.systest.sanpaoloimi.com");
            
            BStrWrapper wrapper = new BStrWrapper(altNames.Encode());
          
            admin.SetCertificateExtension("10.190.65.163\\EDETOCVM-CA", 23, "2.5.29.17", 3, 0,  wrapper);  //  #define PROPTYPE_BINARY		 0x00000003	// Binary data
           
        }
    }
}
