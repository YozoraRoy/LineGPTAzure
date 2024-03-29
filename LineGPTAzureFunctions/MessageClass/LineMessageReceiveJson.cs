﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineGPTAzureFunctions.MessageClass
{
    public class LineMessageReceiveJson
    {
        public string destination { get; set; }
        public List<Event> events { get; set; }
    }

    public class Event
    {
        public string replyToken { get; set; }
        public string type { get; set; }
        public object timestamp { get; set; }
        public Source source { get; set; }
        public Message message { get; set; }

        public ImageMessage imageMessage { get; set; }
}

public class Message
{
    public string id { get; set; }
    public string type { get; set; }
    public string text { get; set; }
}


public class Source
{
    public string type { get; set; }
    public string userId { get; set; }
}

public class ImageMessage
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("originalContentUrl")]
    public string OriginalContentUrl { get; set; }

    [JsonProperty("previewImageUrl")]
    public string PreviewImageUrl { get; set; }
}
}
