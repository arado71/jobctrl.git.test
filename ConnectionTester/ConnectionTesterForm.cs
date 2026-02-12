using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Core;
using Tct.ActivityRecorderClient;
using Tct.ActivityRecorderClient.ActivityRecorderServiceReference;
using Tct.ActivityRecorderClient.Communication;

namespace ConnectionTester
{
    public partial class ConnectionTesterForm : Form
    {
        private List<ActivityRecorderClientWrapper> clientWrappers;
        private new readonly SynchronizationContext context;

        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string[] endpointNames;
        private string selectedEndpoint;
        private bool run;
        private long lastPing;
        private int threadNum = 50;
        private bool useSharedClients;
        private InvokingBindingList<ConnectionData> connections = new InvokingBindingList<ConnectionData>();

	    private static Process sidekickProcess;

        ConnectionData currentConnection;

        public AuthData AuthData { get; private set; }

        public ConnectionTesterForm()
        {
            InitializeComponent();
            clientWrappers = new List<ActivityRecorderClientWrapper>();
            context = AsyncOperationManager.SynchronizationContext;

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var serviceModelSectionGroup = ServiceModelSectionGroup.GetSectionGroup(config);
            endpointNames = serviceModelSectionGroup.Client.Endpoints
                .OfType<ChannelEndpointElement>()
                .Where(
                    n =>
                        String.Equals(n.Contract, "ActivityRecorderServiceReference.IActivityRecorder",
                            StringComparison.InvariantCultureIgnoreCase))
                .Select(n => n.Name)
                .Where(n => n != null)
                .ToArray();
            selectedEndpoint = endpointNames.FirstOrDefault();
            connections =
                new InvokingBindingList<ConnectionData>(
                    endpointNames.Select(
                            e => new ConnectionData() { EndpointName = e, Ping = 0, LastActive = DateTime.MinValue })
                        .ToList(), this.dataGridView1);
            ConfigManager.EnsureLoggedIn(
                () => new ConfigManager.LoginData()
                {
                    UserId = 4296,
                    UserPassword = AuthenticationHelper.GetHashedHexString("1")
                });

            cbEndpoints.DataSource = endpointNames;
            dataGridView1.DataSource = connections;
            timer1.Interval = 5000;
            uiRefreshTimer.Start();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!timer1.Enabled)
            {
                timer1_Tick(this, null);
            }
            timer1.Start();
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            useSharedClients = checkBox1.Checked;
        }

        private void cbEndpoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedEndpoint = cbEndpoints.SelectedItem.ToString();
            if (dataGridView1.RowCount >= cbEndpoints.Items.Count)
            {
                dataGridView1.Rows[cbEndpoints.SelectedIndex].Selected = true;

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (currentConnection == null || currentConnection.EndpointName != selectedEndpoint)
            {
                currentConnection = connections.Single(el => el.EndpointName == selectedEndpoint);
            }
            for (int i = 0; i < threadNum; i++)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    TestConnection(currentConnection);
                });
                //var thr = new Thread(new ThreadStart(TestConnection));
                //thr.Start();
            }
        }

        private void TestConnection(ConnectionData conn)
        {
            var rnd = new Random();
            //Thread.Sleep(rnd.Next(0, 300));
            Task.Delay(rnd.Next(0, 300));
            var sw = new Stopwatch();
            //on background thread
            AuthData authData = null;
            int clientHash = 0;
            try
            {
                sw.Reset();
                sw.Start();
                if (useSharedClients)
                {
                    ActivityRecorderClientWrapper.PreferredEndpoint = selectedEndpoint;
                    ActivityRecorderClientWrapper.Execute(a =>
                    {
                        clientHash = a.GetHashCode();
                        authData = a.Authenticate("");
                    });
                }
                else
                {
                    ActivityRecorderClientWrapper.Execute(a =>
                    {
                        clientHash = a.GetHashCode();
                        authData = a.Authenticate("");
                    }, 0, selectedEndpoint);
                }
                sw.Stop();
                lastPing = sw.ElapsedMilliseconds;
                log.Debug(String.Format("Last successful call at {0} took {1} ms. Client ID: {2}",
                    conn.LastActive.ToString("G"),
                    lastPing, clientHash));
	            if (isSidekickRunning)
	            {
		            sidekickProcess.Kill();
					sidekickProcess = null;
		            isSidekickRunning = false;
	            }
            }
            catch (Exception exception)
            {
                conn.LastException = exception.Message;
                log.Debug(String.Format("Exception while calling service: {0}", exception.Message));

                TestConnectivity();
            }
        }

        private static volatile bool isSidekickRunning = false;
        private static object sidekickLock = new object();
        public static void TestConnectivity(bool isExternal = false)
        {
            if (!isExternal)
            {
                lock (sidekickLock)
                {

                    if (!isSidekickRunning)
                    {
						sidekickProcess = Process.Start(new ProcessStartInfo() { FileName = Application.ExecutablePath, Arguments = Process.GetCurrentProcess().Id.ToString() });
                        isSidekickRunning = true;
                    }
                }
            }

            try
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadString("http://google.com");
                    log.Debug(string.Format("HTTP Get request OK to google.com {0}", isExternal ? "(EXTERNAL TEST)" : ""));
                }
            }
            catch (Exception e)
            {
                log.Debug(string.Format("HTTP Get request FAILED to google.com {0}", isExternal ? "(EXTERNAL TEST)" : ""));
                log.Debug(e);
            }
            var binding = new BasicHttpBinding();
            var client = new Test.GeoIPServiceSoapClient(binding,
                    new EndpointAddress("http://www.webservicex.net/geoipservice.asmx"));
            try
            {
                client.GetGeoIP("64.233.169.99");

                log.Debug(string.Format("SOAP request OK to http://www.webservicex.net/geoipservice {0}",
                    isExternal ? "(EXTERNAL TEST)" : ""));
            }
            catch (Exception e)
            {
                log.Debug(string.Format("SOAP request FAILED to http://www.webservicex.net/geoipservice {0}",
                    isExternal ? "(EXTERNAL TEST)" : ""));
                log.Debug(e);
                client.Abort();
            }
            finally
            {
                client.Close();
                client = null;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = Convert.ToInt32(numericUpDown1.Value * 1000);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            threadNum = Convert.ToInt32(numericUpDown2.Value);
        }

        private void uiRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (currentConnection != null)
            {

                currentConnection.LastActive = DateTime.Now;
                currentConnection.Ping = lastPing;

            }
        }
    }
}
