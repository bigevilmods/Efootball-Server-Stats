using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using File = System.IO.File;

namespace Efootball_Server_Stats
{
    public partial class Form1 : Form
    {
        // Classe para servidores VPN
        private class VpnServer
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public string OvpnUrl { get; set; }

            public VpnServer(string name, string ip, string ovpnUrl)
            {
                Name = name;
                IP = ip;
                OvpnUrl = ovpnUrl;
            }
        }

        // Lista de servidores do eFootball
        private readonly List<(string Name, string IP)> gameServers = new List<(string, string)>
        {
            ("Indonesia-Jakarta Raya-Jakarta", "34.128.68.219"),
            ("Japan-Tokyo-Tokyo", "35.221.126.142"),
            ("Japan-Osaka-Osaka", "34.97.138.165"),
            ("Korea (the Republic of)-Seoul-teukbyeolsi-Seoul", "34.64.235.198"),
            ("Taiwan (Province of China)-Taipei-Taipei", "35.229.165.185"),
            ("Hong Kong-Hong Kong-Hong Kong", "34.92.192.14"),
            ("Brazil-Sao Paulo-Sao Paulo", "34.95.169.15"),
            ("Chile-Region Metropolitana de Santiago-Santiago", "34.176.17.72"),
            ("India-Maharashtra-Mumbai", "34.93.13.191"),
            ("India-Delhi-Delhi", "34.131.13.17"),
            ("United Kingdom of Great Britain and Northern Ireland-England-London", "34.39.17.131"),
            ("Switzerland-Zurich-Zurich", "34.65.93.216"),
            ("Germany-Hessen-Frankfurt am Main", "34.159.137.177"),
            ("Netherlands (Kingdom of the)-Noord-Holland-Amsterdam", "34.13.163.26"),
            ("Belgium-Brussels Hoofdstedelijk Gewest-Brussels", "34.38.154.179"),
            ("United States of America-Virginia-Ashburn", "34.85.172.113"),
            ("United States of America-South Carolina-Moncks Corner", "34.138.103.216"),
            ("United States of America-Iowa-Council Bluffs", "35.224.16.17"),
            ("United States of America-California-Los Angeles", "34.102.12.90"),
            ("United States of America-Oregon-The Dalles", "34.82.40.13"),
            ("United States of America-Utah-Salt Lake City", "34.106.69.65"),
            ("United States of America-Nevada-Las Vegas", "34.125.24.248"),
            ("Canada-Quebec-Montreal", "35.203.65.250"),
            ("Canada-Ontario-Toronto", "34.130.103.243"),
            ("Poland-Mazowieckie-Warsaw", "34.118.91.25"),
            ("Australia-Victoria-Melbourne", "34.129.43.95"),
            ("Finland-Uusimaa-Helsinki", "35.228.63.230"),
            ("Spain-Madrid", "34.175.112.88"),
            ("Italy-Piemonte-Torino", "34.17.93.174"),
            ("Italy-Lombardia-Milan", "34.154.179.38"),
            ("France-Ile-de-France-Paris", "34.155.199.109"),
            ("Qatar-Ad Dawhah-Doha", "34.18.108.143"),
            ("Israel-HaMerkaz-Petah Tikva", "34.165.56.180"),
            ("South Africa-Gauteng-Johannesburg", "34.35.84.61"),
            ("Saudi Arabia-Ash Sharqiyah-Dammam", "34.166.38.226"),
            ("Mexico-Queretaro-Queretaro", "34.51.79.210"),
            ("Brazil-Sao Paulo-Sao Paulo", "34.39.165.239"),
        };

        // Lista de servidores VPN (foco em Brasil)
        private readonly List<VpnServer> vpnServers = new List<VpnServer>
        {
            //ispeed.info
            
            new VpnServer("UK 1", "80.44.179.120", "https://ipspeed.info/ovpn/80.44.179.120.ovpn"),
            new VpnServer("USA 1", "172.191.79.51", "https://ipspeed.info/ovpn/172.191.79.51.ovpn"),
        };

        // Portas a testar (comuns em jogos da Konami + fallback HTTPS)
        private readonly int[] ports = { 5730, 5731, 5732, 5733, 5734, 5735, 5736, 5737, 5738, 5739, 443 };

        // Caminho do OpenVPN
        private readonly string openVpnPath = @"C:\Program Files\OpenVPN\bin\openvpn.exe";
        private readonly string ovpnDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "eFootballRouteOptimizer", "ovpn");

        public Form1()
        {
            InitializeComponent();
            LoadGameServers();
            Directory.CreateDirectory(ovpnDir);
        }

        private void LoadGameServers()
        {
            
            foreach (var server in gameServers)
            {
                listBoxServers.Items.Add($"{server.Name} ({server.IP})");
            }
        }

        private string GetDefaultGateway()
        {
            try
            {
                var gateway = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().GatewayAddresses)
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                return gateway?.Address.ToString() ?? "192.168.1.1";
            }
            catch
            {
                return "192.168.1.1";
            }
        }

        private async Task<(bool Accessible, string Reason)> CheckPortAsync(string ip, int port)
        {
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.ReceiveTimeout = 2000;
                    var connectTask = socket.ConnectAsync(IPAddress.Parse(ip), port);
                    var timeoutTask = Task.Delay(2000);
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        return (false, "Port inaccessible: Timeout");
                    }

                    await connectTask;
                    return (true, "Accessible port");
                }
            }
            catch (SocketException ex)
            {
                return (false, $"inaccessible port: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"inaccessible port: {ex.Message}");
            }
        }

        private async Task<float> CheckLatencyAsync(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ip, 2000);
                    return reply.Status == IPStatus.Success ? reply.RoundtripTime : float.PositiveInfinity;
                }
            }
            catch
            {
                return float.PositiveInfinity;
            }
        }

        private async Task<(string Name, string IP, int Port, float Latency, List<(string Name, string IP, List<(int Port, bool Accessible, string Reason, string Latency)>) >Diagnostics)> GetBestGameServerAsync()
        {
            string bestName = null;
            string bestIP = null;
            int bestPort = 0;
            float bestLatency = float.PositiveInfinity;
            var diagnostics = new List<(string Name, string IP, List<(int Port, bool Accessible, string Reason, string Latency)>)>();
            int totalTests = gameServers.Count * ports.Length;
            int currentTest = 0;

            foreach (var server in gameServers)
            {
                var serverDiagnostic = (Name: server.Name, IP: server.IP, Ports: new List<(int Port, bool Accessible, string Reason, string Latency)>());
                foreach (var port in ports)
                {
                    currentTest++;
                    progressBar.Value = (int)((currentTest / (float)totalTests) * 100);
                    txtResult.Text = $"Testing the game server {server.Name} ({server.IP}:{port})... ({currentTest}/{totalTests})";
                    Application.DoEvents();

                    var (accessible, reason) = await CheckPortAsync(server.IP, port);
                    var latency = accessible ? await CheckLatencyAsync(server.IP) : float.PositiveInfinity;
                    serverDiagnostic.Ports.Add((port, accessible, reason, latency == float.PositiveInfinity ? "Inaccessible" : $"{latency:F2} ms"));
                    if (accessible && latency < bestLatency)
                    {
                        bestLatency = latency;
                        bestName = server.Name;
                        bestIP = server.IP;
                        bestPort = port;
                    }
                }
                diagnostics.Add(serverDiagnostic);
            }

            return (bestName, bestIP, bestPort, bestLatency, diagnostics);
        }

        private async Task<(string Name, string IP, float Latency)> GetBestVpnServerAsync()
        {
            string bestName = null;
            string bestIP = null;
            float bestLatency = float.PositiveInfinity;

            foreach (var vpn in vpnServers)
            {
                txtResult.Text = $"Testing VPN Server {vpn.Name} ({vpn.IP})...";
                Application.DoEvents();
                var latency = await CheckLatencyAsync(vpn.IP);
                if (latency < bestLatency)
                {
                    bestLatency = latency;
                    bestName = vpn.Name;
                    bestIP = vpn.IP;
                }
            }

            return (bestName, bestIP, bestLatency);
        }

        private async Task<string> DownloadOvpnFile(string ovpnUrl, string vpnName)
        {
            try
            {
                using (var client = new WebClient())
                {
                    string ovpnPath = Path.Combine(ovpnDir, $"{vpnName}.ovpn");
                    await client.DownloadFileTaskAsync(ovpnUrl, ovpnPath);
                    return ovpnPath;
                }
            }
            catch (Exception ex)
            {
                return $"Error downloading .ovpn file: {ex.Message}";
            }
        }

        private async Task<string> ConnectToVpn(string ovpnPath, string vpnName)
        {
            if (!File.Exists(openVpnPath))
            {
                return "Error: OpenVPN not found. Install on: https://openvpn.net/community-downloads/";
            }

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = openVpnPath,
                        Arguments = $"--config \"{ovpnPath}\" --auth-user-pass",
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await Task.Delay(5000); // Aguarda 5 segundos para a conexão
                return $"Connected to VPN {vpnName}. Check the connection in eFootball.";
            }
            catch (Exception ex)
            {
                return $"Error connecting to VPN: {ex.Message}. Run as administrator.";
            }
        }

        private string AddRoute(string ip)
        {
            string gateway = GetDefaultGateway();
            string cmd = $"route add {ip} mask 255.255.255.255 {gateway}";
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {cmd}",
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0 ? $"Route added to {ip} via {gateway}." : $"Error adding route. Run as administrator: '{cmd}'";
            }
            catch (Exception ex)
            {
                return $"Erro ao adicionar rota: {ex.Message}. Execute como administrador: '{cmd}'";
            }
        }

        private string OpenFirewallPorts()
        {
            var cmds = ports.Select(port => $"netsh advfirewall firewall add rule name=\"eFootball Port {port}\" dir=in action=allow protocol=UDP localport={port}");
            return "Run as administrator in Command Prompt:\n" + string.Join("\n", cmds);
        }

        private async void btnOptimize_Click(object sender, EventArgs e)
        {
            btnOptimize.Enabled = false;
            txtResult.Text = "Testing game servers, please wait...";
            
            progressBar.Value = 0;

            // Testar servidores do jogo
            var (bestGameName, bestGameIP, bestGamePort, bestGameLatency, gameDiagnostics) = await GetBestGameServerAsync();

            // Testar servidores VPN
            txtResult.Text = "Testing VPN servers, please wait...";
            var (bestVpnName, bestVpnIP, bestVpnLatency) = await GetBestVpnServerAsync();

            string vpnResult = "";
            if (!string.IsNullOrEmpty(bestVpnName))
            {
                var vpn = vpnServers.FirstOrDefault(v => v.Name == bestVpnName);
                if (vpn != null)
                {
                    txtResult.Text = $"Downloading VPN configuration for {bestVpnName}...";
                    Application.DoEvents();
                    string ovpnPath = await DownloadOvpnFile(vpn.OvpnUrl, bestVpnName);
                    if (File.Exists(ovpnPath))
                    {
                        txtResult.Text = $"Connecting to VPN {bestVpnName}...";
                        Application.DoEvents();
                        vpnResult = await ConnectToVpn(ovpnPath, bestVpnName);
                    }
                    else
                    {
                        vpnResult = ovpnPath; // Contém erro do download
                    }
                }
            }
            else
            {
                vpnResult = "No accessible VPN servers. Try a commercial VPN like ExpressVPN,Exit or CloudFlare Warp";
            }

            // Exibir resultados
            if (bestGameName != null)
            {
                string routeMessage = AddRoute(bestGameIP);
                string firewallMessage = OpenFirewallPorts();

                txtResult.Text = $"Best Server: {bestGameName} ({bestGameIP}:{bestGamePort})\r\n" +
                                 $"Latency: {(bestGameLatency == float.PositiveInfinity ? "Inaccessible" : $"{bestGameLatency:F2} ms")}\r\n" +
                                 $"Best VPN: {bestVpnName} ({bestVpnIP}, Latency: {(bestVpnLatency == float.PositiveInfinity ? "Inaccessible" : $"{bestVpnLatency:F2} ms")})\r\n" +
                                 $"VPN: {vpnResult}\r\n" +
                                 $"Route: {routeMessage}\r\n" +
                                 $"Verification: Use Wireshark (udp && ip.addr == {bestGameIP}) or look at the network icon in eFootball.";
            }
            else
            {
                txtResult.Text = "Error: No game servers reachable on tested ports.\r\n" +
                                 "Use Wireshark to identify the correct port: https://www.wireshark.org/download.html\r\n" +
                                 "Instructions: Start eFootball, capture UDP packets, filter by 'udp' and note the port used.\r\n" +
                                 $"VPN: {vpnResult}";
            }

           
            progressBar.Value = 100;
            btnOptimize.Enabled = true;
        }
    }
}
