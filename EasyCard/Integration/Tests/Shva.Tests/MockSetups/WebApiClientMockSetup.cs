using Moq;
using Shared.Helpers;
using Shva.Models;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace Shva.Tests.MockSetups
{
    public class WebApiClientMockSetup
    {
        public Mock<IWebApiClient> MockObj { get; private set; }

        public WebApiClientMockSetup()
        {
            MockObj = new Mock<IWebApiClient>();
            Setup();
        }

        private void Setup()
        {
            MockObj.Setup(o => o.PostXml<Envelope>(
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.Is<Envelope>(obj => obj.Body != null),
                It.IsAny<Func<Task<NameValueCollection>>>(), It.IsAny<ProcessRequest>(), It.IsAny<ProcessResponse>()))
                .Returns<string, string, object, Func<Task<NameValueCollection>>, ProcessRequest , ProcessResponse> ((enpoint, actionPath, payload, getHeaders, onRequest, onResponse) => ((Envelope)payload).Body?.Content switch
                {
                    AshStartRequestBody ashStartRequest => AshStart(ashStartRequest),
                    AshAuthRequestBody ashAuthRequest => AshAuth(ashAuthRequest),
                    AshEndRequestBody ashEndRequest => AshEnd(ashEndRequest),
                    _ => throw new Exception("unknown envelope")
                });

        }

        private Task<Envelope> AshStart(AshStartRequestBody body)
        {
            var response = new AshStartResponseBody();

            response.pinpad = new clsPinPad();
            response.globalObj = new clsGlobal();
            response.AshStartResult = 777;

            return Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private Task<Envelope> AshAuth(AshAuthRequestBody body)
        {
            var response = new AshAuthResponseBody();

            response.globalObj = new clsGlobal();
            response.pinpad = new clsPinPad();
            response.AshAuthResult = 777;

            return Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private Task<Envelope> AshEnd(AshEndRequestBody body)
        {
            var response = new AshEndResponseBody();

            var random = new Random();

            response.pinpad = new clsPinPad();
            response.globalObj = new clsGlobal()
            {
                outputObj = new clsOutput()
                {
                    uid = new OField
                    {
                        valueTag = random.Next(10000000, 90000000).ToString()
                    },
                    manpik = new OField
                    {
                        valueTag = "2" // TODO
                    },
                    solek = new OField
                    {
                        valueTag = "2"
                    }
                },
                receiptObj = new clsReceipt()
                {
                    voucherNumber = new RField
                    {
                        valueTag = random.Next(1000000, 9000000).ToString()
                    }
                }
            };
            response.AshEndResult = 777;

            return Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }
    }
}
