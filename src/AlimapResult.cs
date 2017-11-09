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
	public class AlimapResult
	{
		#region 单例字段
		public static readonly AlimapResult Unknown = new AlimapResult(-1, "Unknown", null);
		#endregion

		#region 成员字段
		private int _code;
		private string _reason;
		private string _message;
		private string _value;
		#endregion

		#region 构造函数
		public AlimapResult(int code, string reason, string message, string value = null)
		{
			_code = code;
			_reason = reason;
			_message = message;
			_value = value;
		}
		#endregion

		#region 公共属性
		public int Code
		{
			get
			{
				return _code;
			}
		}

		public string Reason
		{
			get
			{
				return _reason;
			}
		}

		public string Message
		{
			get
			{
				return _message;
			}
		}

		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}
		#endregion
	}
}
