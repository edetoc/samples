using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchGroupMembers
{
    class Program
    {
        static void Main(string[] args)
        {
            var groupName = "Department 24581";
            DirectoryEntry de = new DirectoryEntry("LDAP://DC=europe,DC=corp,DC=microsoft,DC=com"); // Root Directory //
            var ds = new DirectorySearcher(de);
            ds.PropertiesToLoad.Add("SAMAccountName");
            ds.PropertiesToLoad.Add("member");
            ds.Filter = "(&(objectClass=group)(SAMAccountName=" + groupName + "))";
            SearchResultCollection AllGroupUsers;
            AllGroupUsers = ds.FindAll();

            if (AllGroupUsers.Count > 0)
            {
                ResultPropertyValueCollection values = AllGroupUsers[0].Properties["member"];
                foreach (string s in values)
                {
                    DirectoryEntry u = new DirectoryEntry("LDAP://" + s);
                    Console.WriteLine(u.Properties["displayName"].Value);
                }
            }
        }
    }
}
