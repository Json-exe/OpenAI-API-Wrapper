using System;
using System.Collections.Generic;

namespace OpenAI_API_Wrapper.Classes;

public class ChatClass
{
    public string Title { get; set; }
    public List<string> ChatHistory { get; set; }
    public Guid ChatIdentifier { get; set; }
}