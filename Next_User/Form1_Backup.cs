using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Management;
using System.Text;
using System.Globalization;
using System.Security.Principal;
using System.Windows.Forms;
using Microsoft.Win32;
namespace Next_User
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximumSize = new Size(273, 162);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            InitializeComponent();

            //Computer, if you're on a domain, put the name in a string called "domain".
            String domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            //Below task is capitalizing and recreating the "String domain" variable we made just above.  This is done because
            //Windows makes any domain in the login screen fully capitalized, we want to mimic that as closely as possible
            //to avoid confusion (Cause people be dumb).
            {
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                domain = ti.ToUpper(domain);
            }
            //Here we're checking if that domain variable is actually empty, if so we're simply going to be putting the local
            //machine into the first "Domain:" Combobox index since this is the only option.
            if (string.IsNullOrEmpty(domain))
            {
                this.comboBox1.Items.Insert(0, Environment.MachineName);
                this.comboBox1.Items.Insert(1, Environment.MachineName);
               // this.comboBox1.Enabled = false;
                this.comboBox1.SelectedIndex = 0;
                this.comboBox1.SelectedIndex = 1;
                this.comboBox1.BackColor = Color.White;
            }
            else
            //Machine has been found to be on a domain.  So we do something different:
            {
                //Add domain in the first combobox bsox index and trim the top-level domain (IE .Com) from the second level domain.
                // First 0 indicates the combobox index number (0 is first), domain.split indicates we want to divide this string 
                //using "." as the demarcation.  [0] indicates that we want to use in this combobox index, only the first portion
                //found in the split (0 is first).
                this.comboBox1.Items.Insert(0, domain.Split('.')[0]);
                //For the second combobox index (The 1) we indicate that we want to use the local machine name,  this is used as 
                //secondary because the assumption is this application would be used primarily in a networked domain setting.
                this.comboBox1.Items.Insert(1, Environment.MachineName);
            }
            //This line says that when the application loads, the default "Domain:" combobox selection to show to the user is 
            //the first index of that combobox (for reasons outlined above).

            this.comboBox1.SelectedIndex = 0;

            //This line makes the "User:" Combobox active for input or selection.  Again working on the assumption that the 
            //Network domain we defaulted to will be used and now the user simply wants to get along with the task of choosing
            //What user we'll be selecting.

            //ActiveControl = comboBox2;

        }
        public static void EditReg(string User, string Domain)
        {
            
            string DomainUser = (Domain.ToString() + "\\" + User.ToString());
            RegistryKey RegKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Authentication\\LogonUI", true);
            if (RegKey != null)
            {
                RegKey.SetValue("LastLoggedOnSAMUser", (string)DomainUser, RegistryValueKind.String);
                RegKey.SetValue("LastLoggedOnUser", (string)DomainUser, RegistryValueKind.String);
            }
            else
            {
                RegistryKey XPRegKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\winlogon", true);
                if (XPRegKey != null)
                {
                    
                    XPRegKey.SetValue("AltDefaultUserName", (string)User, RegistryValueKind.String);
                   XPRegKey.SetValue("AltDefaultDomainName", (string)Domain, RegistryValueKind.String);
                    XPRegKey.SetValue("DefaultUserName", (string)User, RegistryValueKind.String);
                    XPRegKey.SetValue("DefaultDomainName", (string)Domain, RegistryValueKind.String);
                }
                else
                { MessageBox.Show("Cannot find necessary registry keys, please run application with elevated privledges."); }
            }
        }
        //Below Function returns a null value if user is not found in the domain (essentially we're checking for typos in manual
        //entries in "User:" combobox).

        public bool LogDeny(string NoLogCheck, Array usersplit)
        {
            foreach (string sUser in usersplit)

                if (sUser.Contains(NoLogCheck.ToString()))
                {
                    String Check = null;
                    return Check != null;
                }
                else
                {
                    String Check = null;
                    return Check == null;
                }
            String fail = null;
            return fail == null;
        }
        public bool UserExists(string username)
        {
            try
            {
                PrincipalContext domain = new PrincipalContext(ContextType.Domain);
                UserPrincipal FoundUser = UserPrincipal.FindByIdentity(domain, IdentityType.SamAccountName, username);
                return FoundUser != null;
            }
           catch
            {
                String Fail = null;
                return Fail != null;
           }
        }
        //Below function creates self-destructing messagebox.  Not critical, but fun.
        public bool IsNullOrWhiteSpace(string theString)
        {
            if (theString != null)
            {
                foreach (char c in theString)
                {
                    if (Char.IsWhiteSpace(c) == false)
                        return false;
                }
            }
            return true;
        }
        public class AutoClosingMessageBox
        {
            System.Threading.Timer _timeoutTimer;
            string _caption;
            AutoClosingMessageBox(string text, string caption, int timeout)
            {
                _caption = caption;
                _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                    null, timeout, System.Threading.Timeout.Infinite);
                MessageBox.Show(text, caption);
            }
            public static void Show(string text, string caption, int timeout)
            {
                new AutoClosingMessageBox(text, caption, timeout);
            }
            void OnTimerElapsed(object state)
            {
                IntPtr mbWnd = FindWindow(null, _caption);
                if (mbWnd != IntPtr.Zero)
                    SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                _timeoutTimer.Dispose();
            }
            const int WM_CLOSE = 0x0010;
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
            static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
            [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        }
        //Below function handles what occurs when the "Domain:" Combobox is changed by the user.
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            string path = Environment.ExpandEnvironmentVariables("/export /cfg %temp%\\UserRights.cfg /areas User_Rights /log %temp%\\UserRights.log");
            string exe = ("secedit.exe");

            try
            {
                ProcessStartInfo secedit = new ProcessStartInfo(exe, path);
                secedit.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(secedit);
            }
            catch
            {
                MessageBox.Show("Some users may not have log on access, running Next_User in administator mode filters these users.");
            }
               Object lMac = comboBox1.SelectedItem;

            if ((lMac.ToString() == Environment.MachineName))
                try
                {

                    int counter = 0;
                    string line;
                    // Read the file and display it line by line.
                    string LineCheck = Environment.ExpandEnvironmentVariables("%temp%\\UserRights.cfg");
                    System.IO.StreamReader file = new System.IO.StreamReader(LineCheck);
                    string sline = null;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains("SeDenyInteractiveLogonRight ="))
                        {
                            sline = (line.ToString());
                            break;
                        }
                        counter++;
                    }
                    file.Close();
                    string linesplit = (sline.Split(' ')[2]);
                    Array usersplit = (linesplit.Split(','));

                    this.comboBox2.Items.Clear();
                    SelectQuery query = new SelectQuery("Win32_UserAccount", "Domain='" + Environment.MachineName + "'");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                    foreach (ManagementObject envVar in searcher.Get())
                    {
                        // MessageBox.Show(envVar["Name"].ToString());
                        String Disabled = (envVar["Disabled"].ToString());
                        if (Disabled == "False" && LogDeny((envVar["Name"].ToString()), usersplit))
                        {
                            //   ProcessStartInfo secedit = new ProcessStartInfo("secedit.exe", "/export /cfg %temp%\\UserRights.inf /areas User_Rights /log %temp%\\UserRights.log");
                            {
                                this.comboBox2.Items.Add(envVar["Name"].ToString());
                                // this.comboBox2.DropDownHeight = this.comboBox2.ItemHeight * (this.comboBox2.Items.Count + 1);
                                this.comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                            }
                        }
                        else
                        { }
                    }
                }
                catch { 
                                    this.comboBox2.Items.Clear();
                    SelectQuery query = new SelectQuery("Win32_UserAccount", "Domain='" + Environment.MachineName + "'");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                    foreach (ManagementObject envVar in searcher.Get())
                    {
                        // MessageBox.Show(envVar["Name"].ToString());
                        String Disabled = (envVar["Disabled"].ToString());
                        if (Disabled == "False")
                        {
                            //   ProcessStartInfo secedit = new ProcessStartInfo("secedit.exe", "/export /cfg %temp%\\UserRights.inf /areas User_Rights /log %temp%\\UserRights.log");
                            {
                                this.comboBox2.Items.Add(envVar["Name"].ToString());
                                // this.comboBox2.DropDownHeight = this.comboBox2.ItemHeight * (this.comboBox2.Items.Count + 1);
                                this.comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                            }
                        }
                        else
                        { }
                    }}
            else
            {
                try
                {
                    this.comboBox2.Items.Clear();
                    //  this.comboBox2.DropDownHeight = this.comboBox2.ItemHeight;
                    this.comboBox2.DropDownStyle = ComboBoxStyle.DropDown;

                    String domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                    domain = ti.ToUpper(domain);
                    String registryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\";
                    using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                    {
                        foreach (String subkeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                string SIDs = (key.OpenSubKey(subkeyName).ToString().Split('\\')[6]);
                                string Users = (new SecurityIdentifier(SIDs).Translate(typeof(NTAccount)).ToString());
                                //     MessageBox.Show((Users.Split('\\')[0]) + " " + (domain.Split('.')[0]));
                                if ((Users.Split('\\')[0]) == (domain.Split('.')[0]))
                                    this.comboBox2.Items.Add(Users.Split('\\')[1]);
                            }
                            catch {}
                           
                        }
                    }
                }
                catch (Exception ex)
                { MessageBox.Show(ex.ToString()); }
            }
           
        }
        //Below function handles what occurs when the "Okay" button is clicked, this can be triggered anywhere in the application
        //by the enter key and indicates that the user is satisfied with their selection.
        private void button1_Click(object sender, EventArgs e)
        {
            String Domain = comboBox1.Text;
            //String Machine = Environment.MachineName;
            //Is selected domain NOT the local machine name?
            string User = comboBox2.Text;
            if (IsNullOrWhiteSpace(User.ToString()) == false)
            {
                if (Domain != Environment.MachineName.ToString())
                { 
                    string Result = (Domain.ToString() + "\\" + User.ToString());
                    string uID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    if ((uID.Split('\\')[0]) == Environment.MachineName)

                    {
              //          MessageBox.Show("Current user is a local user, \ntherefore we can not run a Directory Services check to determine if the selected network account exists");
                        DialogResult dr = new DialogResult();
                        Form2 frm = new Form2();
                        dr = frm.ShowDialog(this);
                        if (dr == DialogResult.OK)
                        {
                            EditReg(User, Domain);
                            this.Hide();
                            AutoClosingMessageBox.Show("Login will default to: " + Result, "Next user:", 3000);
                            Application.Exit();
                        }
                        else
                        { }
                    }
                    else
                    {
                   //     MessageBox.Show(User.ToString());
                        if (UserExists(User.ToString()))
                        {
                            EditReg(User, Domain);
                            this.Hide();
                            AutoClosingMessageBox.Show("Login will default to: " + Result, "Next user:", 3000);
                            Application.Exit();
                        }
                        else
                        {
                            MessageBox.Show("Cannot confirm that user exists in this domain, \nVerify network connection or check spelling.");
                        }
                        }
                }
                else
                {
                  //  MessageBox.Show("Selected domain is the local computer, no manual entries accepted, no check verifications possible.");
                    string Result = (Domain.ToString() + "\\" + User.ToString());
                    EditReg(User, Domain);
                    this.Hide();
                    AutoClosingMessageBox.Show("Login will default to: " + Result, "Next user:", 3000);
                    Application.Exit();
                }
            }
            else
            {
                MessageBox.Show("Please select or type a username.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("There is no information yet, good luck");
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("There are no settings yet");
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}
