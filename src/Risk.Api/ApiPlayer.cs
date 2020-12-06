﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Risk.Shared;

namespace Risk.Api
{
    public class ApiPlayer : IPlayer
    {
        public ApiPlayer() { }

        public ApiPlayer(string name, string token, HttpClient httpClient)
        {
            Name = name;
            Token = token;
            HttpClient = httpClient;
            PlayerCards = new List<Card>();
        }

        public string Name { get; private set; }
        public string Token { get; private set; }
        public HttpClient HttpClient { get; }
        public List<Card> PlayerCards { get; set; }
    }
}
