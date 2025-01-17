﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shva.Models;
using ShvaEMV;

namespace ShvaServiceImitation.Controllers
{
    [Produces("application/xml")]
    [ApiController]
    [Route("")]
    public class MainController : ControllerBase
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
        }

        public ActionResult Default() => Ok("Ready");

        [HttpPost]
        public async Task<IActionResult> EntryPoint(Envelope request) =>
            request?.Body?.Content switch
            {
                AshStartRequestBody ashStartRequest => new ObjectResult(await AshStart(ashStartRequest)),
                AshAuthRequestBody ashAuthRequest => new ObjectResult(await AshAuth(ashAuthRequest)),
                AshEndRequestBody ashEndRequest => new ObjectResult(await AshEnd(ashEndRequest)),
                TransEMVRequestBody transEMVRequest => new ObjectResult(await TransEMV(transEMVRequest)),
                ChangePasswordRequestBody changePasswordRequest => new ObjectResult(await ChangePassword(changePasswordRequest)),
                GetTerminalDataRequestBody getTerminalDataRequest => new ObjectResult(await GetTerminal(getTerminalDataRequest)),
                _ => StatusCode(400, "unknown envelope")
            };

        private async Task<Envelope> AshStart(AshStartRequestBody body)
        {
            var response = new AshStartResponseBody();

            response.pinpad = new clsPinPad();
            response.globalObj = new clsGlobal();
            response.AshStartResult = 777;

            return await Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private async Task<Envelope> AshAuth(AshAuthRequestBody body)
        {
            var response = new AshAuthResponseBody();
            response.pinpad = new clsPinPad();
            if (body.inputObj.clientInputPan != null && (body.inputObj.clientInputPan.Contains("532610") || body.inputObj.clientInputPan.EndsWith("0443") || body.inputObj.clientInputPan == "5105105105105100"))
            {
                if (string.IsNullOrWhiteSpace(body.inputObj.authorizationNo))
                {
                    int[] numbers = new int[4] { 3, 4, 7, 9 };
                    Random rd = new Random();
                    int randomIndex = rd.Next(0, 4);
                    int rNumberForRequestAuth = numbers[randomIndex];
                    response.AshAuthResult = rNumberForRequestAuth;

                    response.globalObj = new clsGlobal()
                    {
                        outputObj = new clsOutput()
                        {
                            telAuthAbility = new OField
                            {
                                valueTag = "1"
                            },
                            telNoCom = new OField
                            {
                                valueTag = "03-5786328"
                            },
                            compRetailerNum = new OField
                            {
                                valueTag = "746520"
                            }

                        }
                    };
                }
                else
                {
                    response.globalObj = new clsGlobal();
                    response.AshAuthResult = 777;
                }
            }
            else
            {
                response.globalObj = new clsGlobal();
                response.AshAuthResult = 777;
            }
            return await Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private async Task<Envelope> AshEnd(AshEndRequestBody body)
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
                    },
                    brand = new OField
                    {
                        valueTag = "1"
                    },
                    authManpikNo = new OField
                    {
                        valueTag = random.Next(100000, 900000).ToString()
                    },
                    tranRecord = new OField
                    {
                        valueTag = Guid.NewGuid().ToString()
                    },
                    appVersion = new OField
                    {
                        valueTag = "test version"
                    },
                    compRetailerNum = new OField
                    {
                        valueTag = "test retailer"
                    }
                },
                receiptObj = new clsReceipt()
                {
                    voucherNumber = new RField
                    {
                        valueTag = random.Next(10000000, 90000000).ToString()
                    }
                }
            };
            response.AshEndResult = 777;

            return await Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private async Task<Envelope> TransEMV(TransEMVRequestBody body)
        {
            var response = new TransEMVResponseBody();

            response.RefNumber = Guid.NewGuid().ToString().Substring(0, 6);

            return await Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private async Task<Envelope> ChangePassword(ChangePasswordRequestBody body)
        {
            var response = new ChangePasswordResponseBody
            {
                ChangePasswordResult = (int)ChangePasswordResultEnum.Success
            };

            return await Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }

        private async Task<Envelope> GetTerminal(GetTerminalDataRequestBody body)
        {
            var response = new GetTerminalDataResponseBody
            {
                GetTerminalDataResult = 0
            };

            return await Task.FromResult(new Envelope
            {
                Body = new Body
                {
                    Content = response
                }
            });
        }
    }
}
