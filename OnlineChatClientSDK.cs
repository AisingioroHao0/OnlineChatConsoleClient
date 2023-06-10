using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Concurrent;
namespace OnlineChat
{
    class UserInfo
    {
        public string id;
        public string name;
        public string email;
        public string phone_number;
        public UserInfo(string id, string name, string email, string phone_number)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.phone_number = phone_number;
        }
        public UserInfo()
        {

        }
    }
    class Message
    {
        public string sender_id;
        public string receiver_id;
        public string message;
        public Message(string sender_id, string receiver_id, string message)
        {
            this.sender_id = sender_id;
            this.receiver_id = receiver_id;
            this.message = message;
        }
    }
    class OnlineChatClientSDK
    {
        public UserInfo userInfo = new UserInfo();
        public List<string> friendList;
        public ConcurrentQueue<Message> messages = new ConcurrentQueue<Message>();

        TcpClient server = new TcpClient();
        byte[] buffer = new byte[1024];
        public string bufferString = "";
        ConcurrentDictionary<int, JsonObject> responses=new ConcurrentDictionary<int, JsonObject>();
        int requestID = 0;
        public bool Connect(string ip = "120.53.14.170", int port = 21101)
        {
            try
            {
                server.Connect(ip, port);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            Task.Run(Listen);
            return true;
        }
        private string? SplitFirstJson()
        {
            int cnt = 0;
            for (int i = 0; i < bufferString.Length; i++)
            {
                if (bufferString[i] == '{')
                {
                    cnt++;
                }
                else if (bufferString[i] == '}')
                {
                    if (cnt > 0)
                    {
                        cnt--;
                        if (cnt == 0)
                        {
                            string res = bufferString.Substring(0, i + 1);
                            bufferString = bufferString.Substring(i + 1);
                            return res;
                        }
                    }
                    else
                    {
                        throw new Exception("algorithm error");
                    }
                }
            }
            return null;
        }   
        private string ReceiveJsonString()
        {
            string? json = null;
            while (json == null)
            {
                json = SplitFirstJson();
                if (json != null)
                {
                    return json;
                }
                NetworkStream serverStream = server.GetStream();
                int byteReceived;
                try
                {
                    byteReceived = serverStream.Read(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    throw;
                }
                bufferString += Encoding.UTF8.GetString(buffer, 0, byteReceived);
            }
            return json;
        }
        private void Listen()
        {
            try
            {
                while (true)
                {
                    string jsonString = ReceiveJsonString();
                    Console.WriteLine(jsonString);
                    JsonObject? jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString);
                    if (jsonObject != null)
                    {
                        if (jsonObject["Type"].ToString() == "response")
                        {
                            responses[jsonObject["RequestID"].GetValue<int>()] = jsonObject;
                        }
                        else if (jsonObject["Type"].ToString() == "message")
                        {
                            string senderID = JsonSerializer.Deserialize<JsonObject>(jsonString)["SenderID"].ToString();
                            string message = JsonSerializer.Deserialize<JsonObject>(jsonString)["Message"].ToString();
                            messages.Enqueue(new Message(senderID, userInfo.id, message));
                            Console.WriteLine("接收到了" + senderID + "的消息:" + message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        private JsonObject GetResponse(int request_id)
        {
            while (!responses.ContainsKey(request_id))
            {
               
            }
            JsonObject response = null;
            responses.Remove(request_id,out response);
            return response;
        }
        public bool SignUp(string id, string password, string name="", string phone_number="", string email="")
        {
            try
            {
                requestID++;
                string str = JsonSerializer.Serialize(
                    new { RequestID = requestID, MethodName = "SignUp", MethodParams = new[] { id, password, name, phone_number, email } }
                );
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                server.GetStream().Write(bytes, 0, bytes.Length);
                JsonObject response = GetResponse(requestID);
                return response["Value"].GetValue<int>() == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
        public bool SignIn(string id, string password)
        {
            try
            {
                requestID++;
                string str = JsonSerializer.Serialize(
                    new { RequestID = requestID, MethodName = "SignIn", MethodParams = new[] { id, password } }
                );
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                server.GetStream().Write(bytes, 0, bytes.Length);
                JsonObject reponse = GetResponse(requestID);
                if(reponse["Value"].GetValue<int>() == 1)
                {
                    userInfo.id = id;
                    friendList = GetFriendList();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
        public bool AddFriend(string friend_id)
        {
            try
            {
                requestID++;
                string str = JsonSerializer.Serialize(
                    new { RequestID = requestID, MethodName = "AddFriend", MethodParams = new[] { userInfo.id, friend_id } }
                );
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                server.GetStream().Write(bytes, 0, bytes.Length);
                JsonObject reponse = GetResponse(requestID);
                return reponse["Value"].GetValue<int>() == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
        public List<string> GetFriendList()
        {
            try
            {
                requestID++;
                string str = JsonSerializer.Serialize(
                    new { RequestID = requestID, MethodName = "GetFriendList", MethodParams = new[] { userInfo.id } }
                );
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                server.GetStream().Write(bytes, 0, bytes.Length);
                JsonObject response = GetResponse(requestID);
                return JsonSerializer.Deserialize<List<string>>(response["Value"].ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
        public bool SendMessage(string friend_id, string message)
        {
            try
            {
                requestID++;
                string str = JsonSerializer.Serialize(
                    new { RequestID = requestID, MethodName = "SendMessage", MethodParams = new[] { userInfo.id, friend_id, message} }
                );
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                server.GetStream().Write(bytes, 0, bytes.Length);
                JsonObject reponse = GetResponse(requestID);
                return reponse["Value"].GetValue<int>() == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
    }

}
