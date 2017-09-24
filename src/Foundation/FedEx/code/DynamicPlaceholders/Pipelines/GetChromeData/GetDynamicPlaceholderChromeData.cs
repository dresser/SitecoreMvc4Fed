﻿using System;
using System.Text.RegularExpressions;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetChromeData;

namespace Sitecore.Foundation.FedEx.DynamicPlaceholders.Pipelines.GetChromeData
{
	public class GetDynamicPlaceholderChromeData : GetPlaceholderChromeData
	{
		public override void Process(GetChromeDataArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			Assert.IsNotNull(args.ChromeData, "Chrome Data");

			if (string.Equals("placeholder", args.ChromeType, StringComparison.OrdinalIgnoreCase))
			{
				var placeholderKey = args.CustomData["placeHolderKey"] as string;
				var regex = new Regex(DynamicPlaceholders.PlaceholderKeyRegex.DynamicKeyRegex);
				var match = regex.Match(placeholderKey);

				if (match.Success && match.Groups.Count > 0)
				{
					var newPlaceholderKey = match.Groups[1].Value;

					args.CustomData["placeHolderKey"] = newPlaceholderKey;

					base.Process(args);

					args.CustomData["placeHolderKey"] = placeholderKey;
				}
				else
				{
					base.Process(args);
				}
			}
		}
	}
}
