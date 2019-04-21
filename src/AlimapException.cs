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
using System.Runtime.Serialization;

namespace Zongsoft.Externals.Alimap
{
	[Serializable]
	public class AlimapException : ApplicationException
	{
		#region 构造函数
		public AlimapException(string message, Exception innerException = null) : base(message, innerException)
		{
		}

		public AlimapException(int code, string message, Exception innerException = null) : base(message, innerException)
		{
			this.Code = code;
		}

		protected AlimapException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.Code = info.GetInt32(nameof(this.Code));
		}
		#endregion

		#region 重写属性
		/// <summary>
		/// 获取高德API返回的错误状态码。
		/// </summary>
		public int Code
		{
			get;
		}
		#endregion

		#region 重写方法
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(this.Code), this.Code);

			//返回基类同名方法
			base.GetObjectData(info, context);
		}
		#endregion
	}
}