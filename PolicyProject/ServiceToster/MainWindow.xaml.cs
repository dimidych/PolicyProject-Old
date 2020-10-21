using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using PolicyProjectManagementService;
using PpsClientChannelProxy;

namespace ServiceToster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPolicyProjectClientChannelProxy _chnlFuck;
        private List<string> _serviceNameLst;

        public MainWindow()
        {
            InitializeComponent();
            /*var baseServiceHostApi = ConfigurationManager.AppSettings["ServiceIp"];
            var serviceAssembly = ConfigurationManager.AppSettings["ServiceAssembly"];
            serviceAssembly = Path.Combine(Environment.CurrentDirectory, serviceAssembly);
            CreateServiceNameList();
            _chnlFuck = new PolicyProjectClientChannelProxy(_serviceNameLst, serviceAssembly, baseServiceHostApi);  */
        }

        /*private void CreateServiceNameList()
        {
            _serviceNameLst = new List<string>();
            var serviceNameList = ConfigurationManager.GetSection("ServiceNameList") as NameValueCollection;

            if (serviceNameList == null || serviceNameList.Count == 0)
                return;

            foreach (var serviceName in
                    serviceNameList.AllKeys.Select(serviceKey => serviceNameList.GetValues(serviceKey).FirstOrDefault()))
                _serviceNameLst.Add(serviceName);
        } */

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*var userService = new UserServiceClient();
                var requestResult =
                    userService.GetSingleUser(new UserDataContract()
                    {
                        UserId = 1,
                        UserFirstName = "",
                        UserLastName = "",
                        UserMiddleName = ""
                    });
                requestResult =
                    userService.GetSingleUserById(1);
                dgrdUsers.DataContext = requestResult;
                userService.Close();*/

                using (var userService = /*_chnlFuck.GetPpsChannelFactory<IUserService>()*/
                    new ChannelFactory<IUserService>(new WebHttpBinding(),
                        "http://localhost:8732/PolicyProjectManagementService/UserService/"))
                {
                    userService.Endpoint.Behaviors.Add(new WebHttpBehavior());
                    var channel = userService.CreateChannel();
                    var users = channel.GetUser(new UserDataContract
                    {
                        UserId = 1,
                    });
                    dgrdUsers.DataContext = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Ошибка во время загрузки. ", ex.Message));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*var jsonSerializer = new DataContractJsonSerializer(typeof (UserDataContract));
                var stream = new MemoryStream();
                var usr = new UserDataContract() {UserId = 1};
                jsonSerializer.WriteObject(stream, usr);
                var data = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
                var proxy = new WebClient();
                proxy.Headers["Content-type"] = "application/json";
                proxy.Encoding = Encoding.UTF8;
                var res=proxy.UploadString("http://localhost:8732/PolicyProjectManagementService/UserService/GetSingleUser", "POST", data);*/

                var jsonSerializer = new DataContractJsonSerializer(typeof (UserDataContract));
                var data = string.Empty;
                using (var stream = new MemoryStream())
                {
                    var usr = new UserDataContract
                    {
                        UserId = 1
                    };
                    jsonSerializer.WriteObject(stream, usr);
                    data = Encoding.UTF8.GetString(stream.ToArray(), 0, (int) stream.Length);
                }

                var request =
                    (HttpWebRequest)
                        WebRequest.Create("http://localhost:8732/PolicyProjectManagementService/UserService/GetUser");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = data.Length;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(data);
                }

                var response = (HttpWebResponse) request.GetResponse();
                var responceStatus = response.StatusCode;

                if (responceStatus != HttpStatusCode.OK)
                    throw new Exception(response.StatusDescription);

                using (var responseStream = response.GetResponseStream())
                {
                    var resContract = jsonSerializer.ReadObject(responseStream) as UserDataContract;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Ошибка во время загрузки2. ", ex.Message));
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*var proxy=new WebClient();
                var url = string.Format("http://localhost:8732/PolicyProjectManagementService/User/GetGet/{0}&{1}", 3, 5);
                var bytes = proxy.DownloadData(url);
                var stream = new MemoryStream(bytes);
                var serializer=new DataContractJsonSerializer(typeof(string));
                txtRes.Text = (serializer.ReadObject(stream)).ToString(); */

                var usr = new UserDataContract
                {
                    UserId = 1
                };
                var serUsr = JsonConvert.SerializeObject(usr);
                var request =
                    (HttpWebRequest)
                        WebRequest.Create("http://localhost:8732/PolicyProjectManagementService/UserService/GetUser");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = serUsr.Length;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(serUsr);
                }

                var response = (HttpWebResponse) request.GetResponse();
                var responceStatus = response.StatusCode;

                if (responceStatus != HttpStatusCode.OK)
                    throw new Exception(response.StatusDescription);

                using (var responseStream = new StreamReader(response.GetResponseStream()))
                using (var jsonRdr = new JsonTextReader(responseStream))
                {
                    var serializer = new JsonSerializer();
                    var ress = serializer.Deserialize<Result<UserDataContract[]>>(jsonRdr);
                    /*var jsonStr=serializer.Deserialize<string>(jsonRdr);
                    var resContract = JsonConvert.DeserializeObject<Result<UserDataContract[]>>(jsonStr); */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Ошибка во время загрузки1. ", ex.Message));
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request =
                    (HttpWebRequest)
                        WebRequest.Create("http://localhost:8732/PolicyProjectManagementService/User/GetImage");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                var jsonSerializer = new DataContractJsonSerializer(typeof (string));
                var str = "iTunes.png";
                var data = string.Empty;

                using (var memStream = new MemoryStream())
                {
                    jsonSerializer.WriteObject(memStream, str);
                    data = Encoding.UTF8.GetString(memStream.ToArray(), 0, (int) memStream.Length);
                }

                request.ContentLength = data.Length;

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var response = (HttpWebResponse) request.GetResponse();

                using (var responseStream = response.GetResponseStream())
                {
                    jsonSerializer = new DataContractJsonSerializer(typeof (ImageDataContract));
                    var resultObj = jsonSerializer.ReadObject(responseStream) as ImageDataContract;

                    if (resultObj == null || resultObj.ImageBytes == null || resultObj.ImageBytes.Length == 0)
                        throw new Exception("Empty object");

                    using (var resultStream = new MemoryStream(resultObj.ImageBytes))
                    {
                        /*imgBox.Source = BitmapFrame.Create(resultStream,
                                      BitmapCreateOptions.None,
                                      BitmapCacheOption.OnLoad); */
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while getting image - " + ex.Message);
            }
        }

        private void BtnUuid_Click(object sender, RoutedEventArgs e)
        {
            /*using (var searcher = new ManagementObjectSearcher("select UUID FROM Win32_ComputerSystemProduct"))
            {

                MessageBox.Show();
            }*/

            var computerName = "localhost";
            var scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", computerName), null);
            scope.Connect();
            var query = new ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct");

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                foreach (ManagementObject wmiObject in searcher.Get())
                {
                    var some = wmiObject["UUID"];
                }
                //Console.WriteLine("{0,-35} {1,-40}", "UUID", WmiObject["UUID"]); // String
            }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var usr = new UserDataContract
                {
                    UserId = 1
                };
                var serUsr = JsonConvert.SerializeObject(usr);
                var request =
                    (HttpWebRequest)
                        WebRequest.Create("http://localhost:8732/PolicyProjectManagementService/UserService/GetUserRest");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = serUsr.Length;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(serUsr);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responceStatus = response.StatusCode;

                if (responceStatus != HttpStatusCode.OK)
                    throw new Exception(response.StatusDescription);

                using (var responseStream = new StreamReader(response.GetResponseStream()))
                using (var jsonRdr = new JsonTextReader(responseStream))
                {
                    var serializer = new JsonSerializer();
                    var ress = serializer.Deserialize<Result<UserDataContract[]>>(jsonRdr);
                    /*var jsonStr=serializer.Deserialize<string>(jsonRdr);
                    var resContract = JsonConvert.DeserializeObject<Result<UserDataContract[]>>(jsonStr); */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Ошибка во время загрузки1. ", ex.Message));
            }
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var usr = new UserDataContract
                {
                    UserId = 1
                };
                var serUsr = JsonConvert.SerializeObject(usr);
                var request =
                    (HttpWebRequest)
                        WebRequest.Create("http://localhost:8732/PolicyProjectManagementService/UserService/GetUserRestJson");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = serUsr.Length;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(serUsr);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responceStatus = response.StatusCode;

                if (responceStatus != HttpStatusCode.OK)
                    throw new Exception(response.StatusDescription);

                using (var responseStream = new StreamReader(response.GetResponseStream()))
                using (var jsonRdr = new JsonTextReader(responseStream))
                {
                    var serializer = new JsonSerializer();
                    var ress = serializer.Deserialize<Result<UserDataContract[]>>(jsonRdr);
                    /*var jsonStr=serializer.Deserialize<string>(jsonRdr);
                    var resContract = JsonConvert.DeserializeObject<Result<UserDataContract[]>>(jsonStr); */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Ошибка во время загрузки1. ", ex.Message));
            }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var evnt = "{\"eventLogFilter\": null}";
                var serUsr = JsonConvert.SerializeObject(evnt);
                var request =
                    (HttpWebRequest)
                        WebRequest.Create("http://localhost:8732/PolicyProjectManagementService/EventLogService/GetEventLogRestJson");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = serUsr.Length;

                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(serUsr);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responceStatus = response.StatusCode;

                if (responceStatus != HttpStatusCode.OK)
                    throw new Exception(response.StatusDescription);

                using (var responseStream = new StreamReader(response.GetResponseStream()))
                using (var jsonRdr = new JsonTextReader(responseStream))
                {
                    var serializer = new JsonSerializer();
                    var ress = serializer.Deserialize<string>(jsonRdr);
                    //var jsonStr=serializer.Deserialize<string>(jsonRdr);
                    var resContract = JsonConvert.DeserializeObject<Result<EventLogDataContract[]>>(ress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Concat("Ошибка во время geteventlog. ", ex.Message));
            }
        }
    }
}
