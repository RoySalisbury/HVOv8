using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HVO.Power.DigitalLoggers
{
    public sealed class WebPowerSwitch
    {
        public static async Task<WebPowerSwitchController> GetControllerDetails(Uri uri, string userName, string password, string serialNumber)
        {
            var result = new WebPowerSwitchController() { SerialNumber = serialNumber };
            try
            {
                // Encode the password for use in the Basic Authentication used by the WebClient calls.
                var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));

                using (var webClient = new WebClient() { BaseAddress = uri.ToString() })
                {
                    webClient.Headers.Add("Authorization", string.Format("Basic {0}", encodedCredentials));

                    string htmlResponse = await webClient.DownloadStringTaskAsync("index.htm");
                    if (string.IsNullOrWhiteSpace(htmlResponse))
                    {
                        // Nothing downloaded
                        return null;
                    }

                    using (var stringReader = new StringReader(htmlResponse))
                    {
                        HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                        document.Load(stringReader);

                        // Get the controller name
                        result.Name = document.DocumentNode.SelectSingleNode("//title")?.InnerText;

//                        var outletRows = document.DocumentNode.SelectNodes("//body/table/tr");

                        var outlets = new List<WebPowerSwitchOutlet>();

                        //if ((outletRows != null) && (outletRows.Count > 0))
                        {
                            foreach (var outletRow in document.DocumentNode.SelectNodes("//body/table/tr"))
                            {
                                var columns = outletRow.InnerText.Split(new char[] { '\n' }, 4, StringSplitOptions.None);
                                if ((columns.Length >= 3) && (int.TryParse(columns[0], out var outletNumber)))
                                {
                                    // We know the next node is the Outlet Name
                                    var outletName = columns[1]?.Trim();

                                    // And the one ofter that is the state
                                    var ouletEnabled = string.Equals(columns[2]?.Trim(), "ON", StringComparison.OrdinalIgnoreCase);

                                    // Add the outlet to the list
                                    outlets.Add(new WebPowerSwitchOutlet(outletNumber, outletName, ouletEnabled, false));
                                }
                            }

                            if (outlets.Count > 0)
                            {
                                result.Outlets = outlets.ToArray();
                            }
                        }

                        return result;
                    }

                }
            }
            catch
            {
            }

            return null;
        }

        public static async Task<bool> SetOutletState(Uri uri, string userName, string password, int outletNumber, WebPowerSwitchOutletCommand command)
        {
            string outletCommand = "ON";
            switch (command)
            {
                case WebPowerSwitchOutletCommand.None:
                    return false;
                case WebPowerSwitchOutletCommand.On:
                    outletCommand = "ON";
                    break;
                case WebPowerSwitchOutletCommand.Off:
                    outletCommand = "OFF";
                    break;
                case WebPowerSwitchOutletCommand.Cycle:
                    outletCommand = "CCL";
                    break;
            }

            // Encode the password for use in the Basic Authentication used by the WebClient calls.
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));

            using (var webClient = new WebClient() { BaseAddress = uri.ToString() })
            {
                webClient.Headers.Add("Authorization", string.Format("Basic {0}", encodedCredentials));
                string htmlResponse = await webClient.DownloadStringTaskAsync(string.Format("outlet?{0}={1}", outletNumber, outletCommand));
                if (string.IsNullOrWhiteSpace(htmlResponse))
                {
                    // Nothing downloaded
                    return false;
                }

                return true;
            }
        }
    }

    [DataContract]
    public sealed class WebPowerSwitchController
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SerialNumber { get; set; }

        [DataMember(IsRequired = false)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<WebPowerSwitchOutlet> Outlets { get; set; }
    }

    [DataContract]
    public sealed class WebPowerSwitchOutlet 
    {
        public WebPowerSwitchOutlet(int number, string name, bool enabled, bool locked = false)
        {
            this.Number = number;
            this.Name = name;
            this.Enabled = enabled;
            this.Locked = locked;
        }

        [DataMember]
        public int Number { get; private set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public bool Enabled { get; private set; }

        [DataMember]
        public bool Locked { get; private set; }
    }

    [DataContract]
    public enum WebPowerSwitchOutletCommand
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        On = 1,

        [EnumMember]
        Off = 2,

        [EnumMember]
        Cycle = 3
    }
}
