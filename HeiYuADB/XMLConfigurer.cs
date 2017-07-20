using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HeiYuADB
{
    class XMLConfigurer
    {
        private static XMLConfigurer _instance;
        private XMLConfigurer()
        {
        }

        public static XMLConfigurer Instance
        {
            get { if (_instance == null)
                    _instance = new XMLConfigurer();
                return _instance;
            }
        }

        public List<string> GetCommands()
        {
            List<string> commands = new List<string>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Commands.xml"); 
            XmlNode xn = xmlDoc.SelectSingleNode("Commands");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode xnf in xnl)
            {
                XmlElement xe = (XmlElement)xnf;
                //Console.WriteLine(xe.GetAttribute("genre"));//显示属性值 
                commands.Add(xe.InnerText);
            }
            return commands;
        }
        
        public void SaveCommands(List<string> commands)
        {
            commands.Remove("");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Commands.xml");
            XmlNode xn = xmlDoc.SelectSingleNode("Commands");
            xn.RemoveAll();
            foreach(string c in commands)
            {
                XmlNode node = xmlDoc.CreateElement("Command");
                node.InnerText = c;
                xn.AppendChild(node);
            }
            xmlDoc.Save("Commands.xml");
        }
    }
}
