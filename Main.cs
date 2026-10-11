using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SR_Manager.Properties;

namespace SR_Manager
{
    public partial class Main : Form
    {
        [DllImport("USER32.DLL")] 
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        private const string tempFile = "sr_manager.tmp";

        private readonly Config config = new Config();
        private readonly Config tmpcfg = new Config();

        private const string str1 = "U1IgTW9kdWxlIE1hbm";

        private const string common = "Global%Manager.exe," +
                                      "Machine%Manager.exe," +
                                      "Download%Server.exe," +
                                      "Gateway%Server.exe," +
                                      "Farm%Manager.exe";

        private readonly Dictionary<int, string> moduleDropDown = new Dictionary<int, string>();
        private readonly Dictionary<int, string> modulePathList = new Dictionary<int, string>();
        private readonly Dictionary<string, string> moduleFullPath = new Dictionary<string, string>();

        public Main()
        {
            if (!File.Exists(Path.GetTempPath() + tempFile)) File.Create(Path.GetTempPath() + tempFile).Dispose();

            var configFile = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)?.Replace(".exe", ".ini") ?? string.Empty);

            if (!File.Exists(configFile))
            {
                MessageBox.Show($@"Cannot find {configFile}", Title(str1), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1212);
            }

            if (!config.Load(configFile))
            {
                MessageBox.Show($@"Cannot load {configFile}", Title(str1), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1212);
            }
            
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Icon = Resources.Main;
            Text = Title(str1);
            StartPosition = FormStartPosition.CenterScreen;

            tmpcfg.Load(Path.GetTempPath() + tempFile);

            var modulecount = 0;
            moduleDropDown.Add(modulecount++, "Apply to all Modules");
            

            if (config.SectionExists("Certification"))
            {
                var name = config.Read("Name", "Certification", "ISRORCert.exe");
                var path = config.Read("Path", "Certification");

                var csfullpath = Path.Combine(path, name);

                moduleFullPath.Add("Certification", csfullpath);
                moduleDropDown.Add(modulecount++ , "Certification Server");
                modulePathList.Add(modulecount-1, "Certification");
            }
            else
            {
                UpdateControl(Controls.Find("CertificationLabel", true)[0], false);
                UpdateControl(Controls.Find("CertificationStatus", true)[0], false);
                UpdateControl(Controls.Find("CertificationButton", true)[0], false);
            }
            
            if (config.SectionExists("Billing"))
            {
                var name = config.Read("Name", "Billing", "ISRORBilling.exe");
                var path = config.Read("Path", "Billing");

                var bsfullpath = Path.Combine(path, name);

                moduleFullPath.Add("Billing", bsfullpath);
                moduleDropDown.Add(modulecount++ , "Billing Server");
                modulePathList.Add(modulecount-1, "Billing");
            }
            else
            {
                UpdateControl(Controls.Find("BillingLabel", true)[0], false);
                UpdateControl(Controls.Find("BillingStatus", true)[0], false);
                UpdateControl(Controls.Find("BillingButton", true)[0], false);
            }

            if (config.SectionExists("Common"))
            {
                foreach (var file in common.Split(','))
                {
                    var fullfilepath = Path.Combine(config.Read("Path", "Common"), file.Replace("%", ""));

                    if (!File.Exists(fullfilepath))
                    {
                        var modulname = Path.GetFileNameWithoutExtension(fullfilepath);

                        if (modulname != "MachineManager")
                        {
                            UpdateControl(Controls.Find($"{modulname}Label", true)[0], false);
                            UpdateControl(Controls.Find($"{modulname}Status", true)[0], false);
                            UpdateControl(Controls.Find($"{modulname}Button", true)[0], false);
                            continue;
                        }

                        MessageBox.Show($@"Cannot find {Path.GetFileName(fullfilepath)} in the directory", Title(str1), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1215);
                    }

                    moduleFullPath.Add($"{file.Replace(".exe", "").Replace("%", "")}", fullfilepath);
                    moduleDropDown.Add(modulecount++, $"{file.Replace(".exe", "").Replace("%", " ")}");
                    modulePathList.Add(modulecount-1, $"{file.Replace(".exe", "").Replace("%", "")}");
                }
            }
            else
            {
                MessageBox.Show(@"MachineManager cannot be found", Title(str1), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1215);
            }
            
            if (config.SectionExists("AgentServer"))
            {
                for (var i=1; i<=int.Parse(config.Read("Count", "AgentServer", "1")); i++)
                {
                    var asfullpath = Path.Combine(config.Read($"Path{i}", "AgentServer"), "AgentServer.exe");

                    if (!File.Exists(asfullpath))
                    {
                        MessageBox.Show($@"Cannot find {Path.GetFileName(asfullpath)} in the directory", Title(str1), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1216);
                    }
                    
                    moduleFullPath.Add($"AgentServer{i}", asfullpath);
                    moduleDropDown.Add(modulecount++ , $"Agent Server {(int.Parse(config.Read("Count", "AgentServer")) > 1 ? $"{i}" : null)}");
                    modulePathList.Add(modulecount-1, $"AgentServer{i}");
                }
            }
            else
            {
                UpdateControl(Controls.Find("AgentServerLabel", true)[0], false);
                UpdateControl(Controls.Find("AgentServerStatus", true)[0], false);
                UpdateControl(Controls.Find("AgentServerButton", true)[0], false);
            }

            if (config.SectionExists("ShardManager"))
            {
                for (var i=1; i<=int.Parse(config.Read("Count", "ShardManager", "1")); i++)
                {
                    var smfullpath = Path.Combine(config.Read($"Path{i}", "ShardManager"), "SR_ShardManager.exe");

                    if (!File.Exists(smfullpath))
                    {
                        MessageBox.Show($@"Cannot find {Path.GetFileName(smfullpath)} in the directory", $@"{smfullpath}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1217);
                    }
                    
                    moduleFullPath.Add($"SR_ShardManager{i}", smfullpath);
                    moduleDropDown.Add(modulecount++ , $"Shard Manager {(int.Parse(config.Read("Count", "ShardManager")) > 1 ? $"{i}" : null)}");
                    modulePathList.Add(modulecount-1, $"SR_ShardManager{i}");
                }
            }
            else
            {
                UpdateControl(Controls.Find("SR_ShardManagerLabel", true)[0], false);
                UpdateControl(Controls.Find("SR_ShardManagerStatus", true)[0], false);
                UpdateControl(Controls.Find("SR_ShardManagerButton", true)[0], false);
            }

            if (config.SectionExists("GameServer"))
            {
                for (var i=1; i<=int.Parse(config.Read("Count", "GameServer", "1")); i++)
                {
                    var gsfullpath = Path.Combine(config.Read($"Path{i}", "GameServer"), "SR_GameServer.exe");

                    if (!File.Exists(gsfullpath))
                    {
                        MessageBox.Show($@"Cannot find {Path.GetFileName(gsfullpath)} in the directory", $@"{gsfullpath}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(-1218);
                    }
                    
                    moduleFullPath.Add($"SR_GameServer{i}", gsfullpath);
                    moduleDropDown.Add(modulecount++ , $"Game Server {(int.Parse(config.Read("Count", "GameServer")) > 1 ? $"{i}" : null)}");
                    modulePathList.Add(modulecount-1, $"SR_GameServer{i}");
                }
            }
            else
            {
                UpdateControl(Controls.Find("SR_GameServerLabel", true)[0], false);
                UpdateControl(Controls.Find("SR_GameServerStatus", true)[0], false);
                UpdateControl(Controls.Find("SR_GameServerButton", true)[0], false);
            }

            DropDownModules.DataSource = new BindingSource(moduleDropDown, null);
            DropDownModules.DisplayMember = "Value";
            DropDownModules.ValueMember = "Key";

            foreach (var module in moduleFullPath)
            {
                try
                {
                    var fyl = Path.GetFileNameWithoutExtension(module.Value);
                    var pid = int.Parse(tmpcfg.Read("pid", module.Key, "0"));
                    if (Process.GetProcessById(pid).ProcessName == fyl)
                    {
                        MonitorModule(pid, fyl);
                    }
                }
                catch { /* ignore */ }
            }
        }

        private static string Title(string H1VtGJ9xqJkyVR1uozG)
        {
            var str2 = Encoding.UTF8.GetString(Convert.FromBase64String("Rm5aWElnTFNCUVdESXdNREE9"));
            return Encoding.UTF8.GetString(Convert.FromBase64String($"{H1VtGJ9xqJkyVR1uozG}{str2}"));
        }

        private async void BtnRestart_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (DropDownModules.SelectedIndex > 0)
                    {
                        var module = modulePathList[DropDownModules.SelectedIndex];

                        try
                        {
                            var pid = int.Parse(tmpcfg.Read("pid", module, "0"));
                            var prc = Process.GetProcessById(pid);

                            if (prc.ProcessName == Path.GetFileNameWithoutExtension(moduleFullPath[module]))
                            {
                                Process.GetProcessById(pid).Kill();
                            }
                        }
                        catch { /* ignore */ }

                        var dummy = StartModule(moduleFullPath[module], module);
                    }
                    else
                    {
                        var response = MessageBox.Show(@"You are about to restart all modules, click OK to continue.", Title(str1), MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        if (response == DialogResult.Cancel) return;

                        BtnRestart.Enabled = false;

                        foreach (var module in moduleFullPath)
                        {
                            try
                            {
                                var fyl = Path.GetFileNameWithoutExtension(module.Value);
                                var pid = int.Parse(tmpcfg.Read("pid", module.Key, "0"));
                                var prc = Process.GetProcessById(pid);

                                if (prc.ProcessName == fyl) Process.GetProcessById(pid).Kill();
                            }
                            catch { /* ignore */ }
                            Thread.Sleep(100);
                        }

                        var dummy = StartModule(moduleFullPath);

                        BtnRestart.Enabled = true;
                    }
                }
                catch { /* ignore */ }
            });
        }

        private async void BtnClose_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (DropDownModules.SelectedIndex > 0)
                    {
                        var module = modulePathList[DropDownModules.SelectedIndex];
                        var fyl = Path.GetFileNameWithoutExtension(moduleFullPath[module]);
                        var pid = int.Parse(tmpcfg.Read("pid", module, "0"));
                        var prc = Process.GetProcessById(pid);

                        if (prc.ProcessName == fyl) Process.GetProcessById(pid).Kill();
                    }
                    else
                    {
                        var response = MessageBox.Show(@"You are about to close all modules, click OK to continue.", Title(str1), MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        if (response == DialogResult.Cancel) return;

                        BtnClose.Enabled = false;

                        foreach (var module in moduleFullPath)
                        {
                            try
                            {
                                var fyl = Path.GetFileNameWithoutExtension(module.Value);
                                var pid = int.Parse(tmpcfg.Read("pid", module.Key, "0"));
                                var prc = Process.GetProcessById(pid);

                                if (prc.ProcessName == fyl)
                                {
                                    if (module.Key.StartsWith("SR_ShardManager"))
                                    {
                                        Thread.Sleep(3000);
                                    }

                                    Process.GetProcessById(pid).Kill();
                                }

                                Thread.Sleep(500);
                            }
                            catch { /* ignore */ }
                        }

                        BtnClose.Enabled = Enabled;
                    }
                }
                catch { /* ignore */ }
            });
        }

        private async void BtnShowHide_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (DropDownModules.SelectedIndex > 0)
                    {
                        var module = modulePathList[DropDownModules.SelectedIndex];
                        var fyl = Path.GetFileNameWithoutExtension(moduleFullPath[module]);
                        var pid = int.Parse(tmpcfg.Read("pid", module, "0"));
                        var hnd = int.Parse(tmpcfg.Read("handle", module, "0"));
                        var prc = Process.GetProcessById(pid);

                        if (prc.ProcessName == fyl) ShowWindow((IntPtr)hnd, prc.MainWindowHandle == (IntPtr)0 ? 5 : 0);
                    }
                    else
                    {
                        foreach (var module in moduleFullPath)
                        {
                            var fyl = Path.GetFileNameWithoutExtension(module.Value);
                            var pid = int.Parse(tmpcfg.Read("pid", module.Key, "0"));
                            var hnd = int.Parse(tmpcfg.Read("handle", module.Key, "0"));
                            var prc = Process.GetProcessById(pid);

                            if (prc.ProcessName == fyl) ShowWindow((IntPtr)hnd, prc.MainWindowHandle == (IntPtr)0 ? 5 : 0);
                        }
                    }
                }
                catch { /* ignore */ }
            });
        }
        private async void CertificationServerButton_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["Certification"], "Certification");
        private async void BillingServerButton_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["Billing"], "Billing");
        private async void BtnGlobalManager_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["GlobalManager"], "GlobalManager");
        private async void BtnMachineManager_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["MachineManager"], "MachineManager");
        private async void BtnDownloadServer_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["DownloadServer"], "DownloadServer");
        private async void BtnGatewayServer_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["GatewayServer"], "GatewayServer");
        private async void BtnFarmManager_Click(object sender, EventArgs e) => await StartModule(moduleFullPath["FarmManager"], "FarmManager");

        private async void BtnAgentServer_Click(object sender, EventArgs e)
        {
            for (var i = 1; i <= int.Parse(config.Read("Count", "AgentServer")); i++)
            {
                await StartModule(moduleFullPath[$"AgentServer{i}"], $"AgentServer{i}");
            }
        }

        private async void BtnShardManager_Click(object sender, EventArgs e)
        {
            for (var i = 1; i <= int.Parse(config.Read("Count", "ShardManager")); i++)
            {
                await StartModule(moduleFullPath[$"SR_ShardManager{i}"], $"SR_ShardManager{i}");
            }
        }

        private async void BtnGameServer_Click(object sender, EventArgs e)
        {
            for (var i = 1; i <= int.Parse(config.Read("Count", "GameServer")); i++)
            {
                await StartModule(moduleFullPath[$"SR_GameServer{i}"], $"SR_GameServer{i}");
            }
        }

        private async void BtnStartAll_Click(object sender, EventArgs e) => await StartModule(moduleFullPath);
        
        private async Task<bool> StartModule(Dictionary<string, string> list)
        {
            return await Task.Run(() =>
            {
                try
                {
                    foreach (var module in list)
                    {
                        var filename = Path.GetFileNameWithoutExtension(module.Value);
                        try
                        {
                            var pid = int.Parse(tmpcfg.Read("pid", module.Key, "0"));
                            var prc = Process.GetProcessById(pid);
                            if (prc.ProcessName == filename) continue;
                        }
                        catch { /* process no running, proceed */ }

                        try
                        {
                            using (var process = new Process())
                            {
                                if (module.Value == null) continue;

                                process.StartInfo = new ProcessStartInfo
                                {
                                    WorkingDirectory = Path.GetDirectoryName(module.Value) ?? string.Empty,
                                    //UseShellExecute = true,
                                    FileName = module.Value
                                };

                                process.Start();
                                Thread.Sleep(500);

                                tmpcfg.Write("pid", $"{process.Id}", module.Key);
                                tmpcfg.Write("handle", $"{process.MainWindowHandle}", module.Key);
                                
                                MonitorModule(process.Id, Path.GetFileNameWithoutExtension(module.Value));
                            }
                        }
                        catch (Exception e)
                        {
                            Global.WriteLog(e.Message);
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Global.WriteLog(e.Message);
                    return false;
                }
            });
        }

        private async Task<bool> StartModule(string filepath, string module = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var fyl = Path.GetFileNameWithoutExtension(filepath);
                    var pid = int.Parse(tmpcfg.Read("pid", module, "0"));
                    var prc = Process.GetProcessById(pid);
                    if (prc.ProcessName == fyl) return false;
                }
                catch { /* process no running, proceed */ }

                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo
                        {
                            WorkingDirectory = Path.GetDirectoryName(filepath) ?? string.Empty,
                            //UseShellExecute = true,
                            FileName = filepath
                        };

                        process.Start();
                        Thread.Sleep(500);

                        tmpcfg.Write("pid", $"{process.Id}", module);
                        tmpcfg.Write("handle", $"{process.MainWindowHandle}", module);

                        MonitorModule(process.Id, Path.GetFileNameWithoutExtension(filepath));
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Global.WriteLog(e.Message);
                    return false;
                }
            });
        }
        
        private async void UpdateControl(Control control, bool status)
        {
            await Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    if (control.Name.EndsWith("Status"))
                    {
                        control.Text = @"Not Available";
                    }
                    control.ForeColor = DefaultForeColor;
                    control.BackColor = DefaultBackColor;
                    control.Enabled = status;
                });
            });
        }

        private async void UpdateStatus(Control control, bool status)
        {
            await Task.Run(() =>
            {
                Invoke((MethodInvoker)delegate
                {
                    control.Text = status ? "Running" : "Not Running";
                    control.BackColor = status ? Color.Green : Color.Red;
                    control.ForeColor = status ? Color.Yellow : Color.White;
                });
            });
        }

        private async void MonitorModule(int pid, string modulename)
        {
            await Task.Run(() =>
            {
                if (modulename.StartsWith("ISRORCert")) modulename = "Certification";
                if (modulename.StartsWith("ISRORBill")) modulename = "Billing";

                try
                {
                    while (Process.GetProcessById(pid).Responding)
                    {
                        UpdateStatus(Controls.Find($"{modulename}Status", true)[0], true);
                        Thread.Sleep(1000);
                    }
                }
                catch { /**/ }

                UpdateStatus(Controls.Find($"{modulename}Status", true)[0], false);
            });
        }
    }
}
