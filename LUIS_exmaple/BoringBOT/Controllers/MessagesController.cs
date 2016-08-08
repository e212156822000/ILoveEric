using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using BoringBot;

namespace BoringBOT
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        //luis test
        
        private async Task<string> GetStock(string StockSymbol)
        {
            double? dblStockValue = await YahooBot.GetStockRateAsync(StockSymbol);
            if (dblStockValue == null)
            {
                return string.Format("This \"{0}\" is not an valid stock symbol", StockSymbol);
            }
            else
            {
                return string.Format("Stock : {0}\n Price : {1}", StockSymbol, dblStockValue);
            }

        }
        private static async Task<StockLUIS> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            StockLUIS Data = new StockLUIS();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://api.projectoxford.ai/luis/v1/application?id=07d4c85b-b7f1-4174-86c7-cb123355dbbd&subscription-key=0a32f27c33b44259be7d0e5076bd3029&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<StockLUIS>(JsonDataResponse);
                }
            }
            return Data;
        }
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //luis

                //string StockRateString = await GetStock(activity.Text);
               
               
                string StockRateString;
                StockLUIS StLUIS = await GetEntityFromLUIS(activity.Text);
                if (StLUIS.intents.Count() > 0)
                {
                    switch (StLUIS.intents[0].intent)
                    {
                        case "StockPrice":
                            StockRateString = await GetStock(StLUIS.entities[0].entity);
                            break;
                        case "StockPrice2":
                            StockRateString = await GetStock(StLUIS.entities[0].entity);
                            break;
                        default:
                            StockRateString = "Sorry, I am not getting you...";
                            break;
                    }
                }
                else
                {
                    StockRateString = "Sorry, I am not getting you...";
                }

                Activity reply = activity.CreateReply(StockRateString);
                await connector.Conversations.ReplyToActivityAsync(reply);
                /*
                
                
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                if (activity.Text == "Beanz" )
                {
                    Activity reply = activity.CreateReply("You are my love.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if(activity.Text == "宇弘")
                {
                    Activity reply = activity.CreateReply("依玲最愛的 <3");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (activity.Text == "fuck")
                {
                    Activity reply = activity.CreateReply("fuck you too.");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else if (activity.Text == "暐翔")
                {
                    Activity reply = activity.CreateReply("是個白癡。");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                */


            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                return message.CreateReply("You're typing, aren't you.");
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}