using OnlineChat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineChatConsoleClient
{
    class UI
    {
        UserInfo me;
        OnlineChatClientSDK sdk=new OnlineChatClientSDK();
        public UI()
        {
            sdk.Connect("127.0.0.1");
        }
        public void InitPage()
        {
            Console.WriteLine("1:注册");
            Console.WriteLine("2:登录");
            string input = Console.ReadLine();
            if (input == "1")
            {
                SignUpPage();
            }
            else
            {
                SignInPage();
            }
        }
        public void SignInPage()
        {
            Console.WriteLine("输入id");
            string id = Console.ReadLine();
            Console.WriteLine("输入密码");
            string password = Console.ReadLine();
            if (sdk.SignIn(id, password))
            {
                Console.WriteLine("登录成功");
                ChatPage();
            }
            else
            {
                Console.WriteLine("登录失败");
            }
        }
        public void SignUpPage()
        {
            Console.WriteLine("输入id");
            string id = Console.ReadLine();
            Console.WriteLine("输入密码");
            string password = Console.ReadLine();
            if (sdk.SignUp(id, password))
            {
                Console.WriteLine("注册成功");
            }
            else
            {
                Console.WriteLine("注册失败");
            }
        }
        public void ChatPage()
        {
            while (true)
            {
                Console.WriteLine("1:发送消息");
                Console.WriteLine("2:添加好友");
                Console.WriteLine("3:查看好友列表");
                Console.WriteLine("0:退出");
                string input = Console.ReadLine();
                if (input == "0")
                {
                    return;
                }
                else if (input == "1")
                {
                    Console.WriteLine("输入好友id");
                    string friend_id = Console.ReadLine();
                    Console.WriteLine("输入消息");
                    string msg = Console.ReadLine();
                    sdk.SendMessage(friend_id, msg);
                }
                else if (input == "2")
                {
                    Console.WriteLine("输入好友id");
                    string friend_id=Console.ReadLine();
                    sdk.AddFriend(friend_id);
                }
                else if(input == "3")
                {
                    foreach (string friend_id in sdk.friendList)
                    {
                        Console.WriteLine(friend_id);
                    }
                }
                
            }
        }
        public void Run()
        {
            while (true)
            {
                InitPage();
            }
        }
        public void printChatPage()
        {
            Console.WriteLine("1:显示全部好友列表");
            Console.WriteLine("2:发送消息给好友");
            Console.WriteLine("3:发送消息给世界");
        }
    }
}
