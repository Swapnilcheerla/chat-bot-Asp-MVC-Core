using System.Diagnostics;
using System.Text;
using Amazon;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime.EventStreams.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NYPTIQ.Models;

namespace NYPTIQ.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly AppSettingConfig _appSettingConfig;


		public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor, IOptions<AppSettingConfig> appSettingConfig)
		{
			_logger = logger;
			_httpContextAccessor = httpContextAccessor;
			_appSettingConfig = appSettingConfig.Value;
		}

		public IActionResult Index()
		{
			var model = LoadOrCreateViewModel();
			return View(model);
		}

		[HttpPost]
		public IActionResult Index(string prompt)
		{
			var model = LoadOrCreateViewModel();

			if (string.IsNullOrWhiteSpace(prompt))
			{
				return View(model);
			}

			model.Prompt = prompt;
			model.GeneratedText = InvokeBedrockAgentAsync(prompt, _appSettingConfig.AgentId, _appSettingConfig.AgentAliasId).Result;

			// Add the current conversation item to the list
			model.Conversation.Add(new ConversationItem { IsHuman = true, Text = prompt });
			model.Conversation.Add(new ConversationItem { IsHuman = false, Text = model.GeneratedText });

			AppendConversationToSession(model);

			return View(model);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		private HomeViewModel LoadOrCreateViewModel()
		{
			var existingModel = _httpContextAccessor.HttpContext.Session.GetString("ViewModel");

			if (!string.IsNullOrEmpty(existingModel))
			{
				return Newtonsoft.Json.JsonConvert.DeserializeObject<HomeViewModel>(existingModel);
			}

			return new HomeViewModel();
		}

		private void AppendConversationToSession(HomeViewModel model)
		{
			var existingModel = LoadOrCreateViewModel();
			existingModel.Conversation.AddRange(model.Conversation);
			SaveViewModelToSession(existingModel);
		}

		private void SaveViewModelToSession(HomeViewModel model)
		{
			var serializedModel = Newtonsoft.Json.JsonConvert.SerializeObject(model);
			_httpContextAccessor.HttpContext.Session.SetString("ViewModel", serializedModel);
		}

		public static async Task<string> InvokeBedrockAgentAsync(string prompt, string agentId, string agentAliasId)
		{
			using (var client = new AmazonBedrockAgentRuntimeClient(RegionEndpoint.USEast1))
			{
				var response = await client.InvokeAgentAsync(new InvokeAgentRequest
				{
					AgentId = agentId,
					AgentAliasId = agentAliasId,
					SessionId = Guid.NewGuid().ToString(),
					InputText = prompt
				});

				if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
				{
					var generatedTextBuilder = new StringBuilder();

					foreach (IEventStreamEvent eventStreamEvent in response.Completion)
					{
						var payloadPart = (PayloadPart)eventStreamEvent;
						var bytes = payloadPart.Bytes;
						var generatedText = Encoding.UTF8.GetString(bytes.ToArray());

						generatedTextBuilder.Append(generatedText);
					}

					return generatedTextBuilder.ToString();
				}
				else
				{
					return ($"InvokeModelAsync failed with status code {response.HttpStatusCode}");
				}
			}


		}
	}
}
