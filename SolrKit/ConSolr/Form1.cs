using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;				
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SolrNet;
using SolrNet.Commands;
using SolrNet.Impl;
using SolrNet.Attributes;
using SolrNet.Utils;
using SolrNet.Mapping;
using SolrNet.Commands.Parameters;
using Microsoft.Practices.ServiceLocation;
using System.Xml;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;


namespace SolrKit
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnLoadURL_Click(object sender, EventArgs e)
        {
            XmlDataDocument xmlDoc = new XmlDataDocument();
            if (!String.IsNullOrEmpty(txtURL.Text))
            {
                string url = txtURL.Text;
                if (!url.Contains("http://")) url = "http://" + url;
                if (!url.EndsWith("/")) url += "/";
                url += "admin/file/?file=schema.xml";

                // Set the reader settings.
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;

                // Create a resolver with default credentials.
                XmlUrlResolver resolver = new XmlUrlResolver();
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                // Set the reader settings object to use the resolver.
                settings.XmlResolver = resolver;

                // Create the XmlReader object.
                XmlReader reader = XmlReader.Create(url, settings);
                
                dgvSchema.Rows.Clear();
                xmlDoc.Load(reader);
                XmlNodeList nodes = xmlDoc.SelectNodes("//field");
                foreach (XmlNode node in nodes)
                {
                    XmlAttributeCollection attribs = node.Attributes;
                    List<string> fieldAttribs = new List<string>();
                    foreach (XmlAttribute attrib in attribs)
                    {
                        fieldAttribs.Add(attrib.Value);
                    }
                    dgvSchema.Rows.Add(fieldAttribs.ToArray());
                }
            }

        }


        private void btnGenerateMapping_Click(object sender, EventArgs e)
        {
            if (dgvSchema.Rows.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                //for now we treat the first row as unique key
                int r = 0;
                sb.AppendLine("public class SolrMappingClass");
                sb.AppendLine("{");
                sb.AppendLine("");
                foreach (DataGridViewRow row in dgvSchema.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        sb.AppendLine();
                        sb.Append("[SolrField(\"");
                        sb.Append(row.Cells[0].Value);
                        sb.Append("\")]");
                        sb.AppendLine();
                        if (row.Cells[4].Value != null && String.Equals(row.Cells[4].Value.ToString(), "true", StringComparison.CurrentCultureIgnoreCase))
                            sb.Append("public ICollection<string> _");
                        else
                            sb.Append("public string _");
                        sb.Append(Regex.Replace(row.Cells[0].Value.ToString().Trim(), "[^a-zA-Z0-9]", "_"));
                        sb.Append("{ get; set; } ");
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                }
                sb.AppendLine("");
                sb.AppendLine("}");
                MappingCode mappingForm = new MappingCode(sb.ToString());
                mappingForm.ShowDialog();
            }
        }

        private bool IsNumeric(char c)
        {
            return char.IsNumber(c);
        }

        private void btnQueryGo_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(txtQuery.Text) && !String.IsNullOrEmpty(txtURL.Text))
            {
                string url = txtURL.Text;
                if (!url.EndsWith("/")) url += "/";
                string query = txtQuery.Text;
                int Tries = (int) numTries.Value;
                if (!url.Contains("http://")) url = "http://" + url;
                double meanPingTime = GetMeanPingTime(new Uri(url).Host, Tries);
                label6.Text = meanPingTime.ToString() + " msecs";
                label6.Refresh();

                StartHttpListener(url);

                DateTime start = DateTime.Now;
                for (int Try = 0; Try < Tries; Try++)
                   RunQuery(query);

                TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
                double meanQueryTime = ts.TotalMilliseconds / Tries;
                label8.Text = meanQueryTime.ToString() + " msecs";
                
                //Actual response time is query time minus the network latency
                double actualResponseTime = meanQueryTime - meanPingTime;
                label10.Text = actualResponseTime.ToString() + " msecs";
            }

        }

        private static double GetMeanPingTime(string host, int echoNum)
        {
            long totalTime = 0;
            int timeout = 120;
            Ping objSender = new Ping();

            for (int i = 0; i < echoNum; i++)
            {
                PingReply reply = objSender.Send(host, timeout);
                if (reply.Status == IPStatus.Success)
                {
                    totalTime += reply.RoundtripTime;
                }
            }
            return totalTime / echoNum;
        }

        //private ISolrq
        private void RunQuery(string query)
        {
            ISolrOperations<SolrDocument> solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrDocument>>();
            var q = new SolrQueryByField("text", query);
            var results = solr.Query(q);
        }


        private void StartHttpListener(string uri)
        {
            HttpListener listener = new HttpListener();
            
            //need to grant permissions first
            //httpcfg.exe set urlacl /u http://localhost:80/StevenCheng/ ; /a D:(A;;GX;;;WD)
            //string HttpConfigPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "httpcfg.exe");
            //System.Diagnostics.Process.Start(HttpConfigPath + " set urlacl /u " + uri + "; /a D:(A;;GX;;;WD) ");

            listener.Prefixes.Add(uri);
            listener.Start();
            Console.WriteLine("Listening...");
            for (;;)
            {
                HttpListenerContext context = listener.GetContext();
                new Thread(new HttpWorker(context).ProcessRequest).Start();
            }
        }

        private void btnSaveSchema_Click(object sender, EventArgs e)
        {
            //

        }
    }
}
