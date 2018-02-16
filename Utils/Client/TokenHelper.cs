﻿using System;
using System.Text;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Utils.Client
{

    public class TokenHelper
    {
        private DateTime _Time;
        private string _Token;
        private string _RefreshToken;
        private int _ExpiryTime;
        private int _FailureTime;

        /// <summary>
        /// 请求状态
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// AccessToken字符串
        /// </summary>
        public string AccessToken
        {
            get
            {
                var now = DateTime.Now;
                if (string.IsNullOrEmpty(_Token) || now > _Time.AddSeconds(_FailureTime))
                {
                    GetTokens();
                    if (!Success) return null;
                }

                if (now > _Time.AddSeconds(_ExpiryTime)) RefresTokens();

                return _Token;

            }
        }

        /// <summary>
        /// AccessToken对象
        /// </summary>
        public AccessToken Token { get; private set; } = new AccessToken();

        /// <summary>
        /// 用户签名
        /// </summary>
        public string Sign { get; private set; }

        /// <summary>
        /// 当前连接基础应用服务器
        /// </summary>
        public string BaseServer { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="secret">用户密钥</param>
        public void Signature(string secret)
        {
            Sign = Util.Hash(Account.ToUpper() + Util.Hash(secret));
        }

        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns>bool 是否获取成功</returns>
        public void GetTokens()
        {
            var code = GetCode();
            if (code == null) return;

            var key = Util.Hash(Sign + code);
            var url = $"{BaseServer}/securityapi/v1.0/tokens?account={Account}&signature={key}&deptid={Token.deptId}";
            var request = new HttpRequest(_RefreshToken);
            Success = request.Send(url);
            if (!Success)
            {
                Messages.ShowError(request.Message);
                return;
            }

            var result = Util.Deserialize<Result<TokenPackage>>(request.Data);
            if (!result.successful)
            {
                Messages.ShowError(result.message);
                return;
            }

            _Time = DateTime.Now;
            _Token = result.data.accessToken;
            _RefreshToken = result.data.refreshToken;
            _ExpiryTime = result.data.expiryTime;
            _FailureTime = result.data.failureTime;

            var buffer = Convert.FromBase64String(_Token);
            var json = Encoding.UTF8.GetString(buffer);
            Token = Util.Deserialize<AccessToken>(json);
        }

        /// <summary>
        /// 获取Code
        /// </summary>
        /// <returns>string Code</returns>
        private string GetCode()
        {
            var url = $"{BaseServer}/securityapi/v1.0/tokens/codes?account={Account}";
            var request = new HttpRequest(_RefreshToken);
            Success = request.Send(url);
            if (!Success)
            {
                Messages.ShowError(request.Message);
                return null;
            }

            var result = Util.Deserialize<Result<string>>(request.Data);
            if (result.successful) {return result.data;}

            Messages.ShowError(result.message);
            return null;
        }

        /// <summary>
        /// 刷新AccessToken过期时间
        /// </summary>
        private void RefresTokens()
        {
            var url = $"{BaseServer}/securityapi/v1.0/tokens";
            var request = new HttpRequest(_RefreshToken);
            if (!request.Send(url))
            {
                Messages.ShowError(request.Message);
                return;
            }

            var result = Util.Deserialize<Result<TokenPackage>>(request.Data);
            if (result.code == "406")
            {
                GetTokens();
                return;
            }

            if (!result.successful)
            {
                Messages.ShowError(result.message);
                return;
            }

            _Time = DateTime.Now;
            _Token = result.data.accessToken;
            _RefreshToken = result.data.refreshToken;
            _ExpiryTime = result.data.expiryTime;
            _FailureTime = result.data.failureTime;

            var buffer = Convert.FromBase64String(_Token);
            var json = Encoding.UTF8.GetString(buffer);
            Token = Util.Deserialize<AccessToken>(json);
        }
    }
}