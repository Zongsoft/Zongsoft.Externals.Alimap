/*
 * Authors:
 *   钟峰(Popeye Zhong) <9555843@qq.com>
 * 
 * Copyright (C) 2017 Zongsoft Corporation. All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace Zongsoft.Externals.Alimap
{
	public struct DataMapping
	{
		public string Id;
		public string Name;
		public string Longitude;
		public string Latitude;
		public string Address;

		public static DataMapping? Resolve(string text)
		{
			if(string.IsNullOrEmpty(text))
				return null;

			var parts = text.Split(';', '|');
			var keys = new string[5];

			foreach(var part in parts)
			{
				var index = part.IndexOf('=');

				if(index > 0 && index < part.Length - 1)
				{
					switch(part.Substring(0, index).ToLowerInvariant())
					{
						case "id":
							keys[0] = part.Substring(index + 1);
							break;
						case "name":
							keys[1] = part.Substring(index + 1);
							break;
						case "longitude":
							keys[2] = part.Substring(index + 1);
							break;
						case "latitude":
							keys[3] = part.Substring(index + 1);
							break;
						case "address":
							keys[4] = part.Substring(index + 1);
							break;
					}
				}
			}

			return new DataMapping() {
				Id = keys[0],
				Name = keys[1],
				Longitude = keys[2],
				Latitude = keys[3],
				Address = keys[4],
			};
		}
	}
}
