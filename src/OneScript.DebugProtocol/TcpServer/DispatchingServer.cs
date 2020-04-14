/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DebugProtocol.Abstractions;

namespace OneScript.DebugProtocol.TcpServer
{
    public class DispatchingService<TService>
    {
        private readonly ICommunicationServer _server;
        private readonly TService _requestService;
        private readonly MethodsDispatcher<TService> _requestProcessor;

        public DispatchingService(ICommunicationServer server, TService requestService)
        {
            _server = server;
            _requestProcessor = new MethodsDispatcher<TService>();
            _requestService = requestService;
        }

        public void Start()
        {
            _server.DataReceived += OnDataReceived;
            _server.Start();
        }
        
        public void Stop()
        {
            _server.DataReceived -= OnDataReceived;
            _server.Stop();
        }

        private void OnDataReceived(object sender, CommunicationEventArgs e)
        {
            if (e.Exception == null)
            {
                ProcessSuccess((TcpProtocolDto)e.Data, e.Channel);
            }
            else
            {
                // TODO
            }
        }

        private void ProcessSuccess(TcpProtocolDto message, ICommunicationChannel responseChannel)
        {
            var response = _requestProcessor.Dispatch(_requestService, message.Id, message.Parameters);
            if (response != null)
            {
                responseChannel.Write(response);
            }
        }
    }
}