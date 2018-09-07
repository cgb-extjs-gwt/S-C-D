using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestNinject.Service
{
    public class MessageService : IMessageService
    {
        public string GetMessage()
        {
            return "Hello World!";
        }
    }
}