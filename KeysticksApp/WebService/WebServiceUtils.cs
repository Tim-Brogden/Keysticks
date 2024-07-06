/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - Keysticks application and installer
 *
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Threading;
using Keysticks.Config;
using Keysticks.Core;

namespace Keysticks.WebService
{
    /// <summary>
    /// Performs web service requests
    /// </summary>
    internal class WebServiceUtils
    {        
        // Events
        internal event WebServiceEventHandler OnResponse;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public WebServiceUtils()
        {
        }

        /// <summary>
        /// Perform a web service call on a separate thread
        /// </summary>
        /// <param name="requestData"></param>
        public void StartWebServiceRequest(WebServiceMessageData requestData)
        {
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(PerformWebServiceTransaction);
            Thread downloaderThread = new Thread(threadStart);
            downloaderThread.Start(new WebServiceMessageData[] { requestData });
        }

        /// <summary>
        /// Perform a web service call on a separate thread
        /// </summary>
        /// <param name="requestData"></param>
        public void StartWebServiceRequests(IEnumerable<WebServiceMessageData> requestDataList)
        {
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(PerformWebServiceTransaction);
            Thread downloaderThread = new Thread(threadStart);
            downloaderThread.Start(requestDataList);
        }

        /// <summary>
        /// Perform a web service request (run on dedicated thread)
        /// </summary>
        /// <param name="data"></param>
        private void PerformWebServiceTransaction(object data)
        {
            List<WebServiceMessageData> responseDataList = new List<WebServiceMessageData>();
            KeysticksWebService wsClient = null;
            try
            {
                wsClient = new KeysticksWebService();
                wsClient.Url = AppConfig.IsLocalMode ? Constants.LocalWebServiceURL : Constants.WebServiceURL;
                IEnumerable<WebServiceMessageData> requestDataList = (IEnumerable<WebServiceMessageData>)data;

                foreach (WebServiceMessageData requestData in requestDataList)
                {
                    WebServiceMessageData responseData = PerformMessageRequest(wsClient, requestData);
                    responseDataList.Add(responseData);
                }

                // Close connection
                wsClient = null;
            }
            catch (Exception ex)
            {
                WebServiceMessageData responseData = new WebServiceMessageData(EMessageType.WebServiceError);
                responseData.Content = string.Format(Properties.Resources.String_WebServiceErrorMessage, ex.GetType().Name);
                responseDataList.Add(responseData);
            }            

            // Process response(s)
            if (OnResponse != null)
            {
                foreach (WebServiceMessageData responseData in responseDataList)
                {
                    OnResponse(responseData);
                }
            }
        }

        /// <summary>
        /// Perform general web service request
        /// </summary>
        /// <param name="wsClient"></param>
        /// <param name="requestData"></param>
        /// <returns></returns>
        private WebServiceMessageData PerformMessageRequest(KeysticksWebService wsClient, WebServiceMessageData requestData)
        {
            WebServiceMessageData responseData = null;

            // Perform web method call
            string request = requestData.ToString();                
            string response = null;
            switch (requestData.MessageType)
            {
                case EMessageType.CheckForumUser:
                    response = wsClient.CheckForumUser(request);
                    break;
                case EMessageType.GetProgramUpdates:
                    response = wsClient.GetProgramUpdates(request);
                    break;
                case EMessageType.GetProfilesList:
                    response = wsClient.GetProfilesList(request);
                    break;
                case EMessageType.GetProfileData:
                    response = wsClient.GetProfileData(request);
                    break;
                case EMessageType.GetWordPredictionLanguagePack:
                    response = wsClient.GetWordPredictionLanguagePack(request);
                    break;
                case EMessageType.SubmitProfile:
                    response = wsClient.SubmitProfile(request);
                    break;
                case EMessageType.DeleteProfile:
                    response = wsClient.DeleteProfile(request);
                    break;
            }

            if (response != null)
            {
                responseData = new WebServiceMessageData();
                responseData.FromString(response);
            }
            
            return responseData;
        }
    }
}
