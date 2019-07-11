using System;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace RemoveACL
{
    class Program
    {
        static void Main(string[] args)
        {

            // Connection to Active Directory

            DirectoryEntry user = new DirectoryEntry();
            user.Options.SecurityMasks = SecurityMasks.Owner | SecurityMasks.Group | SecurityMasks.Dacl | SecurityMasks.Sacl;
            user.Path = "LDAP://edetoc589VM.edetoc1.lab:389/CN=user,CN=Users,DC=edetoc1,DC=lab";

            ActiveDirectorySecurity userSec = user.ObjectSecurity;

            // Elaborate the user to delegate permission

            NTAccount ntaToDelegate = new NTAccount("EDETOC1", "ITGroup");
            SecurityIdentifier sidToDelegate = (SecurityIdentifier)ntaToDelegate.Translate(typeof(SecurityIdentifier));


            // Specials Guids

            Guid UserForceChangePassword = new Guid("00299570-246d-11d0-a768-00aa006e0529");
            Guid userSchemaGuid = new Guid("BF967ABA-0DE6-11D0-A285-00AA003049E2");
            //Guid pwdLastSetSchemaGuid = new Guid("bf967a0a-0de6-11d0-a285-00aa003049e2");

            // Elaborate ACEs     

            ExtendedRightAccessRule erarResetPwd = new ExtendedRightAccessRule(ntaToDelegate, AccessControlType.Allow, UserForceChangePassword, ActiveDirectorySecurityInheritance.None, userSchemaGuid);
            //PropertyAccessRule parPwdLastSetW = new PropertyAccessRule(ntaToDelegate, AccessControlType.Allow, PropertyAccess.Write, pwdLastSetSchemaGuid, ActiveDirectorySecurityInheritance.None, userSchemaGuid);
            //PropertyAccessRule parPwdLastSetR = new PropertyAccessRule(ntaToDelegate, AccessControlType.Allow, PropertyAccess.Read, pwdLastSetSchemaGuid, ActiveDirectorySecurityInheritance.None, userSchemaGuid);

            userSec.RemoveAccessRule(erarResetPwd);
            //userSec.AddAccessRule(parPwdLastSetW);
            //userSec.AddAccessRule(parPwdLastSetR);

            user.CommitChanges();


        }
    }
}
