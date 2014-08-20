using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Starksoft.Net.Proxy;

namespace SharpTools.Proxy
{
    /// <summary>
    /// Automatically handle proxies according to the ProxyHandler class settings.
    /// Each time a TCPClient is to be created, use the provided newTcpClient() method instead.
    /// Note: Underlying proxy handling is taken from Starksoft, LLC's Biko Library.
    /// This library is open source and provided under the MIT license. More info at biko.codeplex.com.
    /// </summary>

    public static class ProxyHandler
    {
        public enum Type { HTTP, SOCKS4, SOCKS4a, SOCKS5 };

        private static ProxyClientFactory factory = new ProxyClientFactory();
        private static IProxyClient proxy;
        private static bool proxy_ok = false;
        
        /// <summary>
        /// Set to true to enable proxying
        /// </summary>
        
        public static bool ProxyEnabled = false;
        
        /// <summary>
        /// Proxy host, IP address or domain name
        /// </summary>
        
        public static string ProxyHost = "127.0.0.1";
        
        /// <summary>
        /// Proxy port
        /// </summary>
        
        public static int ProxyPort = 80;

        /// <summary>
        /// Proxy username, set to "" for no username/password
        /// </summary>

        public static string ProxyUsername = "";

        /// <summary>
        /// Proxy password, set to "" for no username/password
        /// </summary>

        public static string ProxyPassword = "";

        /// <summary>
        /// Proxy type, default is HTTP.
        /// </summary>

        public static ProxyHandler.Type proxyType = ProxyHandler.Type.HTTP;

        /// <summary>
        /// Create a regular TcpClient or a proxied TcpClient according to the handler's settings.
        /// </summary>

        public static TcpClient newTcpClient(string host, int port)
        {
            try
            {
                if (ProxyEnabled && ProxyHost != "127.0.0.1" && ProxyPort != 80)
                {
                    ProxyType innerProxytype = ProxyType.Http;

                    switch (proxyType)
                    {
                        case Type.HTTP: innerProxytype = ProxyType.Http; break;
                        case Type.SOCKS4: innerProxytype = ProxyType.Socks4; break;
                        case Type.SOCKS4a: innerProxytype = ProxyType.Socks4a; break;
                        case Type.SOCKS5: innerProxytype = ProxyType.Socks5; break;
                    }

                    if (ProxyUsername != "" && ProxyPassword != "")
                    {
                        proxy = factory.CreateProxyClient(innerProxytype, ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                    }
                    else proxy = factory.CreateProxyClient(innerProxytype, ProxyHost, ProxyPort);

                    if (!proxy_ok) { proxy_ok = true; }

                    return proxy.CreateConnection(host, port);
                }
                else return new TcpClient(host, port);
            }
            catch (ProxyException e)
            {
                proxy = null;

                if (e.Message.Contains("403"))
                    return null;

                return null;
            }
        }
    }
}
